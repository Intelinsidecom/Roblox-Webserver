using Roblox.Common;
using Roblox.ServiceProcess;

namespace Roblox.Games.Relay
{
	[System.ComponentModel.RunInstaller(true)]
	public class GamesRelayInstaller : ServiceHostInstaller
	{
		public override string Description => "Manages games on an RCC node.";
		public override string DisplayName => "Roblox Games Relay";
		public override string ServiceName => "Roblox.Games.Relay";
	}
}
