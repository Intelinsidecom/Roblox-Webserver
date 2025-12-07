// Notifications/AuthenticationNotificationsHandler.js
$(function() {
    Roblox.RealTime.Factory.GetClient().Subscribe("AuthenticationNotifications", function(n) {
        if (n.Type === "SignOut") {
            var t = "/authentication/is-logged-in";
            Roblox.Endpoints && (t = Roblox.Endpoints.generateAbsoluteUrl(t, null, !0)), $.ajax({
                url: t,
                method: "GET",
                error: function(n) {
                    n.status === 401 && window.location.reload()
                }
            })
        }
    })
});