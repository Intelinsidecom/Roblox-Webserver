using System.ServiceProcess;

namespace Roblox.Wcf
{
	public class ServiceBasePublic : ServiceBase
	{
		public void OnStartPublic(string[] args)
		{
			OnStart(args);
		}

		public void OnStopPublic()
		{
			OnStop();
		}
	}
}
