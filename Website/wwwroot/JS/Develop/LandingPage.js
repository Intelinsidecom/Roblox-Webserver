// Develop/LandingPage.js
$(function() {
    var i = $("#StudioSplashScreen").data("authentication-url"),
        n, t;
    play_placeId = 0, n = function(n) {
        prefix = "RobloxProxy/StartGame/";
        var t = Roblox.Client.CreateLauncher(!0);
        Roblox.Client.IsIE() ? t.AuthenticationTicket = n : t.Put_AuthenticationTicket(n), t.SetEditMode(), t.StartGame(i, "")
    }, t = function() {
        var t = Roblox.Endpoints.getAbsoluteUrl("/Game/GetAuthTicket");
        $.ajax({
            method: "GET",
            url: t,
            crossDomain: !0,
            xhrFields: {
                withCredentials: !0
            }
        }).done(n)
    }, $("#studio-launch").click(function() {
        $("#PlaceLauncherStatusPanel").data("is-protocol-handler-launch-enabled") == "True" ? Roblox.GameLauncher.openStudio() : Roblox.Client.WaitForRoblox(t)
    })
});