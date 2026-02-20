using System;
using System.Collections;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceModel;
using System.ServiceProcess;

namespace Roblox.Wcf
{
	public class ServiceHostApp<TServiceClass> : ServiceBasePublic where TServiceClass : class
	{
		private ServiceHost _ServiceHost;

		private readonly TServiceClass _Singleton;

		public event EventHandler HostClosed;

		public event EventHandler HostClosing;

		public event EventHandler HostFaulted;

		public event EventHandler HostOpened;

		public event EventHandler HostOpening;

		public ServiceHostApp(TServiceClass singleton)
		{
			_Singleton = singleton;
		}

		public ServiceHostApp()
		{
		}

		protected override void OnStart(string[] args)
		{
			CloseServiceHost();
			_ServiceHost = ((_Singleton != null) ? new ServiceHost(_Singleton) : new ServiceHost(typeof(TServiceClass)));
			IArgumentService argumentService = _ServiceHost.SingletonInstance as IArgumentService;
			if (argumentService != null)
			{
				argumentService.ProcessArgs(args);
			}
			_ServiceHost.Closed += ServiceHost_Closed;
			_ServiceHost.Closing += ServiceHost_Closing;
			_ServiceHost.Faulted += ServiceHost_Faulted;
			_ServiceHost.Opened += ServiceHost_Opened;
			_ServiceHost.Opening += ServiceHost_Opening;
			_ServiceHost.Open();
		}

		private void CloseServiceHost()
		{
			if (_ServiceHost != null)
			{
				if (_ServiceHost.State != CommunicationState.Closed)
				{
					_ServiceHost.Close();
				}
				_ServiceHost.Closed -= ServiceHost_Closed;
				_ServiceHost.Closing -= ServiceHost_Closing;
				_ServiceHost.Faulted -= ServiceHost_Faulted;
				_ServiceHost.Opened -= ServiceHost_Opened;
				_ServiceHost.Opening -= ServiceHost_Opening;
				_ServiceHost = null;
			}
		}

		private void ServiceHost_Closed(object sender, EventArgs e)
		{
			if (this.HostClosed != null)
			{
				this.HostClosed(sender, e);
			}
		}

		private void ServiceHost_Closing(object sender, EventArgs e)
		{
			if (this.HostClosing != null)
			{
				this.HostClosing(sender, e);
			}
		}

		private void ServiceHost_Faulted(object sender, EventArgs e)
		{
			if (this.HostFaulted != null)
			{
				this.HostFaulted(sender, e);
			}
		}

		private void ServiceHost_Opened(object sender, EventArgs e)
		{
			if (this.HostOpened != null)
			{
				this.HostOpened(sender, e);
			}
		}

		private void ServiceHost_Opening(object sender, EventArgs e)
		{
			if (this.HostOpening != null)
			{
				this.HostOpening(sender, e);
			}
		}

		protected override void OnStop()
		{
			CloseServiceHost();
		}

		public void Process(string[] args, Action statsTask = null)
		{
			if (args.Length != 0)
			{
				try
				{
					string argument = args[0];
					switch (argument.Substring(1).ToLower())
					{
					case "console":
						RunInConsoleMode(args, statsTask);
						break;
					case "install":
						Install();
						break;
					case "uninstall":
						Uninstall();
						break;
					case "reinstall":
						Reinstall();
						break;
					default:
						throw new ApplicationException("Bad argument " + argument);
					}
					return;
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
					return;
				}
			}
			ServiceBase.Run(this);
		}

		private void RunInConsoleMode(string[] args, Action statsTask)
		{
			Console.WriteLine("Starting {0}...", typeof(TServiceClass));
			OnStart(args);
			Console.WriteLine("Service started. Press any key to {0}.", (statsTask == null) ? "exit" : "get stats");
			Console.WriteLine("Press {0} to force a full Garbage Collection cycle", ConsoleKey.G);
			Console.WriteLine("Press {0} to close sockets or {1} to exit process", ConsoleKey.Q, ConsoleKey.Escape);
			while (true)
			{
				switch (Console.ReadKey(true).Key)
				{
				case ConsoleKey.G:
					Console.Write("Initiating GC cycle...");
					GC.Collect(3, GCCollectionMode.Forced);
					Console.WriteLine("done");
					continue;
				case ConsoleKey.Q:
					Console.Write("Closing sockets...");
					CloseServiceHost();
					Console.WriteLine("Done");
					Console.WriteLine("Press {0} to exit process", ConsoleKey.Escape);
					continue;
				default:
					if (statsTask != null)
					{
						statsTask();
						continue;
					}
					break;
				case ConsoleKey.Escape:
					break;
				}
				break;
			}
			Console.WriteLine("Stopping service...");
			OnStop();
		}

		private static void Reinstall()
		{
			try
			{
				Console.WriteLine("Uninstalling and reinstalling service");
				AssemblyInstaller assemblyInstaller = CreateInstaller();
				IDictionary savedState = new Hashtable();
				assemblyInstaller.Uninstall(savedState);
				Console.WriteLine("Service was uninstalled.  Will attempt to re-install.");
				AssemblyInstaller assemblyInstaller2 = CreateInstaller();
				savedState = new Hashtable();
				assemblyInstaller2.Install(savedState);
				assemblyInstaller2.Commit(savedState);
				Console.WriteLine("Service was installed.");
			}
			catch (Exception ex)
			{
				HandleException(ex);
			}
		}

		private static void Uninstall()
		{
			try
			{
				Console.WriteLine("Uninstalling...");
				AssemblyInstaller assemblyInstaller = CreateInstaller();
				IDictionary savedState = new Hashtable();
				assemblyInstaller.Uninstall(savedState);
				Console.WriteLine("Service was uninstalled.");
			}
			catch (Exception ex)
			{
				HandleException(ex);
			}
		}

		private static void Install()
		{
			try
			{
				Console.WriteLine("Installing...");
				AssemblyInstaller assemblyInstaller = CreateInstaller();
				IDictionary savedState = new Hashtable();
				assemblyInstaller.Install(savedState);
				assemblyInstaller.Commit(savedState);
				Console.WriteLine("Service was installed.");
			}
			catch (Exception ex)
			{
				HandleException(ex);
			}
		}

		private static AssemblyInstaller CreateInstaller()
		{
			return new AssemblyInstaller(Assembly.GetEntryAssembly(), new string[0])
			{
				UseNewContext = true
			};
		}

		private static void HandleException(Exception ex)
		{
			string msg = ((ex.InnerException == null) ? ex.Message : ex.InnerException.Message);
			string stackTrace = ((ex.InnerException == null) ? ex.StackTrace : ex.InnerException.StackTrace);
			Console.WriteLine("Error message: {0}", msg);
			Console.WriteLine("Stack Trace: {0}", stackTrace);
		}
	}
}
