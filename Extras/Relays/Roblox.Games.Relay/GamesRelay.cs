using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Newtonsoft.Json;

using Roblox.Grid;
using Roblox.Grid.Rcc;
using Roblox.EventLog;
using Roblox.EventLog.Windows;

namespace Roblox.Games.Relay
{
    [System.ServiceModel.ServiceContract(SessionMode = System.ServiceModel.SessionMode.NotAllowed, Namespace = "http://roblox.com/", ConfigurationName = "GamesRelay")]
    [System.ServiceModel.ServiceBehavior(ConcurrencyMode = System.ServiceModel.ConcurrencyMode.Multiple, InstanceContextMode = System.ServiceModel.InstanceContextMode.Single)]
	public class GamesRelay
	{
		public static void Main(string[] args)
		{
			var app = new Roblox.Wcf.ServiceHostApp<GamesRelay>();
			app.HostOpening += HostApplicationOpening;
			app.HostClosing += HostApplicationClosing;
			app.Process(args);
		}
		[System.ServiceModel.Web.WebGet(ResponseFormat = System.ServiceModel.Web.WebMessageFormat.Json)]
		[System.ServiceModel.OperationContract()]
		public bool IsAlive()
		{
			return true;
		}
		[System.ServiceModel.Web.WebGet]
		[System.ServiceModel.OperationContract()]
		public Stream Ping()
		{
			if (System.ServiceModel.Web.WebOperationContext.Current != null)
				System.ServiceModel.Web.WebOperationContext.Current.OutgoingResponse.ContentType = "image/gif";
			return new MemoryStream(_EmptyGif);
		}
		[System.ServiceModel.Web.WebGet(ResponseFormat = System.ServiceModel.Web.WebMessageFormat.Json)]
		[System.ServiceModel.OperationContract()]
		public ServerStatistics GetStats()
		{
			return GetServerStatistics();
		}
		[System.ServiceModel.Web.WebInvoke(BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Wrapped, ResponseFormat = System.ServiceModel.Web.WebMessageFormat.Json)]
		[System.ServiceModel.OperationContract()]
		public void StopGame(string gameId)
		{
			_Logger.LifecycleEvent("StopGame. GameId = {0}", gameId);
			_RccService.CloseJob(gameId);
		}
		[System.ServiceModel.OperationContract()]
		[System.ServiceModel.Web.WebInvoke(BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Wrapped, ResponseFormat = System.ServiceModel.Web.WebMessageFormat.Json)]
		public void EvictPlayers(string gameId, ICollection<int> playerIds, string script)
		{
			_Logger.LifecycleEvent("EvictPlayers. GameId = {0}, playerIds.Count = {1}", gameId, playerIds.Count);
			foreach (var id in playerIds)
			{
				var evictPlaterScript = Lua.NewScript($"Evict Player {id}", script, id, 0);
				_RccService.ExecuteEx(gameId, evictPlaterScript);
			}
		}
		[System.ServiceModel.OperationContract()]
		[System.ServiceModel.Web.WebInvoke(BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Wrapped, ResponseFormat = System.ServiceModel.Web.WebMessageFormat.Json)]
		public void ExecuteScript(string gameId, string scriptName, object[] arguments, string script)
		{
			_Logger.LifecycleEvent("ExecuteScript. GameId = {0}, scriptName = {1}", gameId, scriptName);
			_RccService.ExecuteEx(gameId, Lua.NewScript(scriptName, script, arguments));
		}
		[System.ServiceModel.OperationContract()]
		[System.ServiceModel.Web.WebInvoke(BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Wrapped, ResponseFormat = System.ServiceModel.Web.WebMessageFormat.Json)]
		public void RenewLease(string gameId, int expirationInSeconds)
		{
			_Logger.LifecycleEvent("RenewLease. GameId = {0}, expirationInSeconds = {1}", gameId, expirationInSeconds);
			_RccService.RenewLease(gameId, expirationInSeconds);
		}
		[System.ServiceModel.OperationContract()]
		[System.ServiceModel.Web.WebGet(UriTemplate = "Admin/Terminate?gameid={gameId}")]
		public void AdminTerminate(string gameId)
		{
			_Logger.LifecycleEvent("AdminTerminate. GameId = {0}", gameId);
			_RccService.CloseJob(gameId);
		}
		[System.ServiceModel.OperationContract()]
		[System.ServiceModel.Web.WebGet(UriTemplate = "Admin/ExecuteScript?gameid={gameId}&name={scriptName}&script={script}")]
		public void AdminExecute(string gameId, string scriptName, string script)
		{
			_Logger.LifecycleEvent("AdminExecute. GameId = {0}, scriptName = {1}, script = {2}", gameId, scriptName, script);
			_RccService.ExecuteEx(gameId, Lua.NewScript(scriptName, script));
		}
		private static void ExecuteDispatch(Dispatch dispatch)
		{
			_Logger.LifecycleEvent("ExecuteDispatch. ScriptName = {0}", dispatch.ScriptName);
			var dispatchScript = Lua.NewScript(dispatch.ScriptName, dispatch.Script, dispatch.Arguments);
			var job = new Job
			{
				id = dispatch.GameId.ToString(),
				category = 1,
				cores = 1.0,
				expirationInSeconds = dispatch.ExpirationInSeconds
			};
			_RccService.OpenJobEx(job, dispatchScript);
		}
		private static void HostApplicationClosing(object sender, EventArgs e)
		{
			_Logger.LifecycleEvent("Stopping...");
			_IsRunning = false;
			_Monitor.Dispose();
		}
		private static void HostApplicationOpening(object sender, EventArgs e)
		{
			_Logger.LifecycleEvent("Starting...");
			_IsRunning = true;
			RunInBackground(_Monitor.Start);
			RunInBackground(CheckRccVersion);
			if (string.IsNullOrEmpty(global::Roblox.Games.Relay.Properties.Settings.Default.GameServiceUrl))
				return;
			RunInBackground(RequestDispatches);
		}
		private static void CheckRccVersion()
		{
			while (_IsRunning)
			{
				Thread.Sleep(10000);
				ReadRccVersion();
			}
		}
		private static void ReadRccVersion()
		{
			try
			{
				var regkey = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\ROBLOX Corporation\Roblox", "RccServicePath", null);
				if (regkey == null) throw new FileNotFoundException("The RccService version is unknown on the current machine.");

				var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo($"{regkey}RCCService.exe");
				_RccVersion = fileVersionInfo.FileVersion;
			}
			catch (Exception ex)
			{
				_Logger.LifecycleEvent("Error in fetching Rcc Version: {0}", ex);
				_RccVersion = null;
			}
		}
		private static void RequestDispatches()
		{
			ReadRccVersion();
			_Logger.LifecycleEvent("RequestDispatches. Game Service Url = {0}, RccVersion = {1}", global::Roblox.Games.Relay.Properties.Settings.Default.GameServiceUrl, _RccVersion);
			var address = $"{(global::Roblox.Games.Relay.Properties.Settings.Default.GameServiceUrl)}/v1.0/GetDispatch?apiKey={(global::Roblox.Games.Relay.Properties.Settings.Default.GamesRelayApiKey)}";
			while (_IsRunning)
			{
				try
				{
					if (_RccVersion == null) throw new Exception("Unable to request dispatch since RCC version is not detected");
					var serverStats = JsonConvert.SerializeObject(GetServerStatistics());
					using (var client = new WebClient())
					{
						client.Headers[HttpRequestHeader.ContentType] = "application/json";
						var response = client.UploadString(address, serverStats);
						_Logger.LifecycleEvent("GetDispatch. Reponse from Games Service: {0}", response);
						var dispatch = JsonConvert.DeserializeObject<Dictionary<string, Dispatch>>(response)["data"];
						if (dispatch != null) RunInBackground(() => ExecuteDispatch(dispatch));
					}
				}
				catch (Exception ex)
				{
					_Logger.LifecycleEvent("Error in RequestDispatches: {0}", ex);
				}
				Thread.Sleep(global::Roblox.Games.Relay.Properties.Settings.Default.RequestDispatchInterval);
			}
		}
		private static ServerStatistics GetServerStatistics()
		{
			var stats = _Monitor.GetServerStatistics();
			stats.RccVersion = _RccVersion;
			return stats;
		}
		private static void RunInBackground(Action action)
		{
			Task.Factory.StartNew(() =>
			{
				try
				{
					action();
				}
				catch (Exception ex)
				{
					_Logger.Error(ex);
				}
			});
		}
		private static readonly byte[] _EmptyGif = Convert.FromBase64String("R0lGODlhAQABAHAAACH5BAUAAAAALAAAAAABAAEAAAICRAEAOw==");
		private static readonly ILogger _Logger = new Logger(global::Roblox.Games.Relay.Properties.Settings.Default.LogName, () => global::Roblox.Games.Relay.Properties.Settings.Default.LogLevel)
		{
			LogToConsole = true,
			LogThreadID = true
		};
		private static readonly RccServiceArbiter _RccService = new RccServiceArbiter(global::Roblox.Games.Relay.Properties.Settings.Default.ArbiterEndpoint);
		private static readonly PerfStatsMonitor _Monitor = new PerfStatsMonitor();
		private static string _RccVersion;
		private static bool _IsRunning;
	}
}
