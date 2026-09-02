// ~/viewapp/common/services/eventStreamService.js
"use strict";
freebloxiaAppService.factory("eventStreamService", ["$log", function() {
    function t() {
        return Freebloxia && Freebloxia.EventStream
    }
    return {
        targetTypes: t() ? {
            DEFAULT: Freebloxia.EventStream.TargetTypes.DEFAULT,
            WWW: Freebloxia.EventStream.TargetTypes.WWW,
            STUDIO: Freebloxia.EventStream.TargetTypes.STUDIO,
            DIAGNOSTIC: Freebloxia.EventStream.TargetTypes.DIAGNOSTIC
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
            t() && Freebloxia.EventStream.SendEventWithTarget && (u = u ? u : this.targetTypes.WWW, Freebloxia.EventStream.SendEventWithTarget(n, i, r, u))
        }
    }
}]);