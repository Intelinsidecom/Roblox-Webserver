namespace Roblox.Games.Relay
{
	public class RccServiceArbiter
	{
		public RccServiceArbiter(string arbiterEndPoint)
		{
			_ArbiterEndPoint = arbiterEndPoint;
		}
		public void OpenJobEx(Roblox.Grid.Rcc.Job job, Roblox.Grid.Rcc.ScriptExecution script)
		{
			using (var rccService = GetRccService())
			{
				rccService.OpenJobEx(job, script);
			}
		}
		public void ExecuteEx(string jobId, Roblox.Grid.Rcc.ScriptExecution script)
		{
			using (var rccService = GetRccService())
			{
				rccService.ExecuteEx(jobId, script);
			}
		}
		public void CloseJob(string jobId)
		{
			using (var rccService = GetRccService())
			{
				rccService.CloseJob(jobId);
			}
		}
		public void RenewLease(string jobId, int expirationInSeconds)
		{
			using (var rccService = GetRccService())
			{
				rccService.RenewLease(jobId, expirationInSeconds);
			}
		}
		public string GetVersion()
		{
			string version;
			using (var rccService = GetRccService())
			{
				version = rccService.GetVersion();
			}
			return version;
		}
		private Roblox.Grid.Rcc.RCCServiceSoap GetRccService()
		{
			return new Roblox.Grid.Rcc.RCCServiceSoap
			{
				Url = _ArbiterEndPoint
			};
		}
		private readonly string _ArbiterEndPoint;
	}
}
