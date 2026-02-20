using System;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Roblox.Wcf
{
	public abstract class ServiceHostInstaller : Installer
	{
		public abstract string ServiceName { get; }

		public abstract string DisplayName { get; }

		public abstract string Description { get; }

		protected ServiceHostInstaller()
		{
			ServiceProcessInstaller process = new ServiceProcessInstaller
			{
				Account = ServiceAccount.LocalSystem
			};
			base.Installers.Add(process);
			ServiceInstaller service = new ServiceInstaller
			{
				ServiceName = ServiceName,
				DisplayName = DisplayName,
				Description = Description,
				StartType = ServiceStartMode.Automatic
			};
			service.Committed += Service_Committed;
			base.Installers.Add(service);
		}

		private void Service_Committed(object sender, InstallEventArgs e)
		{
			using (ServiceController sc = new ServiceController(ServiceName))
			{
				sc.Start();
				sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10.0));
			}
		}
	}
}
