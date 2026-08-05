// Notifications/NotificationSettingsAuthority.js
typeof Roblox == "undefined" && (Roblox = {}), Roblox.NotificationSettingsAuthority = function() {
    var t = !1,
        n = "",
        i = function() {
            var i = $("#notification-settings");
            t = i.data("can-toggle-mobile-push-notifications"), n = i.data("notifications-domain")
        },
        r = function() {
            i()
        },
        u = function() {
            return t
        },
        f = function(t, i) {
            $.ajax({
                method: "POST",
                url: n + "/v2/push-notifications/deregister-current-device",
                xhrFields: {
                    withCredentials: !0
                },
                crossDomain: !0,
                success: function(n) {
                    n && n.statusMessage && n.statusMessage === Roblox.Constants.http.successStatus && t && typeof t == "function" && t(!0)
                },
                error: function(n) {
                    i && typeof i == "function" && i(n)
                }
            })
        },
        e = function(t) {
            $.ajax({
                method: "GET",
                url: n + "/v2/push-notifications/get-current-device-destination",
                xhrFields: {
                    withCredentials: !0
                },
                crossDomain: !0,
                success: function(n) {
                    n && n.destination ? t(!0) : t(!1)
                }
            })
        };
    return {
        initialize: r,
        isPushEnabled: e,
        canToggleMobilePushNotifications: u,
        deregisterCurrentDevice: f
    }
}(), $(function() {
    Roblox.NotificationSettingsAuthority.initialize()
});