namespace Roblox.Thumbnails.Relay
{
	[System.ComponentModel.RunInstaller(true)]
	public class ThumbnailsRelayInstaller : Roblox.Wcf.ServiceHostInstaller
	{
		public override string Description => "Manages rendering of thumbnails on an RCC node.";
		public override string DisplayName => "Roblox Thumbnails Relay";
		public override string ServiceName => "Roblox.Thumbnails.Relay";
	}
}
