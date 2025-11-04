// ~/viewapp/common/services/realtimeService.js
"use strict";
robloxAppService.factory("realtimeService", ["$log", function(n) {
    function t() {
        return Roblox && Roblox.RealTime
    }

    function u() {
        return t() ? Roblox.RealTime.Factory.GetClient() : null
    }

    function f(i, r) {
        if (t() && angular.isDefined(r)) {
            var u = this.getRealTimeClient();
            u.Subscribe(i, function(t) {
                n.debug("--------- this is " + i + " subscription -----------" + t.Type), t && t.Type && r[t.Type] && r[t.Type](t)
            })
        }
    }
    var i = {
            friendshipNotifications: "FriendshipNotifications",
            presenceNotifications: "PresenceNotifications"
        },
        r = {
            friendshipNotifications: {
                friendshipDestroyed: "FriendshipDestroyed",
                friendshipCreated: "FriendshipCreated",
                friendshipDeclined: "FriendshipDeclined",
                friendshipRequested: "FriendshipRequested"
            },
            presenceNotifications: {
                presenceOffline: "UserOffline",
                presenceOnline: "UserOnline"
            }
        };
    return {
        realTimeTypes: i,
        notificationTypes: r,
        isRealTimeValid: t,
        getRealTimeClient: u,
        listenToNotification: f
    }
}]);