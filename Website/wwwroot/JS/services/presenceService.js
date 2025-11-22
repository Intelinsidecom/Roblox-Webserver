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
                t = Roblox.Constants.realTimeNotifications.presenceBulkNotifications.name,
                i = Roblox.Constants.realTimeNotifications.presenceBulkNotifications.types;
            n.Subscribe(t, function (n) {
                n.forEach(function (n) {
                    switch (n.Type) {
                    case i.presenceChanged:
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