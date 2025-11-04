// ~/viewapp/common/services/eventStreamService.js
"use strict";
robloxAppService.factory("eventStreamService", ["$log", function() {
    function t() {
        return Roblox && Roblox.EventStream
    }
    return {
        targetTypes: t() ? {
            DEFAULT: Roblox.EventStream.TargetTypes.DEFAULT,
            WWW: Roblox.EventStream.TargetTypes.WWW,
            STUDIO: Roblox.EventStream.TargetTypes.STUDIO,
            DIAGNOSTIC: Roblox.EventStream.TargetTypes.DIAGNOSTIC
        } : {
            DEFAULT: 0,
            WWW: 1,
            STUDIO: 2,
            DIAGNOSTIC: 3
        },
        eventNames: {
            notificationStream: {
                openFromNewIntro: "nsOpenFromNewIntro",
                openContent: "nsOpenContent",
                acceptFriendRequest: "nsAcceptFriendRequest",
                ignoreFriendRequest: "nsIgnoreFriendRequest",
                viewAllFriendRequests: "nsViewAllFriendRequests",
                chat: "nsChat",
                goToProfilePage: "nsGoToProfilePage",
                goToSettingPage: "nsGoToSettingPage"
            }
        },
        sendEventWithTarget: function(n, i, r, u) {
            t() && Roblox.EventStream.SendEventWithTarget && (u = u ? u : this.targetTypes.WWW, Roblox.EventStream.SendEventWithTarget(n, i, r, u))
        }
    }
}]);