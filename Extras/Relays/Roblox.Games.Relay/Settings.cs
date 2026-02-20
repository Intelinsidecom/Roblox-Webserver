namespace Roblox.Games.Relay.Properties
{
    [System.Configuration.SettingsProvider(typeof(Roblox.Configuration.Provider))]
    internal sealed partial class Settings
    {
        protected override void OnSettingsLoaded(System.Object sender, System.Configuration.SettingsLoadedEventArgs e)
        {
            base.OnSettingsLoaded(sender, e);

            global::Roblox.Configuration.Provider.RegisterSettings(e, this);
        }
    }
}
