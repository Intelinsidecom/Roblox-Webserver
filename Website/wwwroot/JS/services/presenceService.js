// services/presenceService.js
"use strict";
var Roblox = Roblox || {};
Roblox.PresenceService = function () {
    function n(n) {
        var t = Roblox.EnvironmentUrls.presenceApi + "/v1/presence/users";
        $.ajax({
            method: "POST",
            url: t,
            data: n,
            success: function (n) {
                $(document).trigger("Roblox.Presence.Update", [n.userPresences])
            }
        })
    }

    function t() {
        if (Roblox && Roblox.RealTime) {
            var n = Roblox.RealTime.Factory.GetClient(),
                t = Roblox.Constants && Roblox.Constants.realTimeNotifications && (Roblox.Constants.realTimeNotifications.presenceBulkNotifications || Roblox.Constants.realTimeNotifications.presenceNotifications);
            if (!t) return;
            var i = t.types,
                r = [i.presenceChanged, i.presenceOnline, i.presenceOffline].filter(function (n) { return !!n; });
            t = t.name;
            n.Subscribe(t, function (n) {
                n.forEach(function (n) {
                    if (r.indexOf(n.Type) !== -1) {
                        var t = {
                            userIds: []
                        };
                        t.userIds.push(n.UserId), Roblox.PresenceService.getPresences(t)
                    }
                })
            })
        }
    }

    function i() {
        Roblox.PresenceService.initializeRealTimeSubscriptions()
    }
    return $(function () {
        Roblox.CurrentUser && Roblox.CurrentUser.isAuthenticated && Roblox.PresenceService.init()
    }), {
        init: i,
        initializeRealTimeSubscriptions: t,
        getPresences: n
    }
}();
