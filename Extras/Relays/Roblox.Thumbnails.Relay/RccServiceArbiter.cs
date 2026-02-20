namespace Roblox.Thumbnails.Relay
{
	public class RccServiceArbiter
	{
        private readonly string _ArbiterEndPoint;

        public RccServiceArbiter(string arbiterEndPoint)
		{
			_ArbiterEndPoint = arbiterEndPoint;
		}

		public Roblox.Grid.Rcc.LuaValue[] BatchJobEx(Roblox.Grid.Rcc.Job job, Roblox.Grid.Rcc.ScriptExecution script)
		{
			using (var rccService = GetRccService())
			{
				return rccService.BatchJobEx(job, script);
			}
		}

		private Roblox.Grid.Rcc.RCCServiceSoap GetRccService()
		{
			return new Roblox.Grid.Rcc.RCCServiceSoap
			{
				Url = _ArbiterEndPoint
			};
		}
	}
}
