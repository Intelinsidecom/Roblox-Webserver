// ~/viewapp/common/services/playGameService.js
"use strict";
robloxApp.factory("playGameService", ["$log", "eventStreamService", function(n, t) {
    function i(n) {
        var i = n.eventName,
            r = n.ctx,
            u = n.properties;
        t.sendEventWithTarget(i, r, u)
    }

    function r(n, i) {
        t.sendGamePlayEvent(n, i)
    }

    function u(n, t) {
        Roblox.GameLauncher.joinGameInstance(n, t, !0, !0)
    }

    function f(n) {
        Roblox.GameLauncher.followPlayerIntoGame(n)
    }

    function e(n) {
        Roblox.GameLauncher.joinMultiplayerGame(n)
    }
    return {
        launchGame: function(n, t) {
            if (Roblox && Roblox.GameLauncher) {
                var o = n.rootPlaceId,
                    s = n.placeId,
                    h = n.gameInstanceId,
                    c = n.playerId;
                s === o && h ? (t.properties.gameInstanceId = h, i(t), r(t.gamePlayIntentEventCtx, o), u(s, h)) : o && c ? (t.properties.playerId = c, i(t), r(t.gamePlayIntentEventCtx, o), f(c)) : (i(t), r(t.gamePlayIntentEventCtx, s), e(s))
            }
        },
        buildPlayGameProperties: function(n, t, i, r) {
            return {
                rootPlaceId: n,
                placeId: t,
                gameInstanceId: i,
                playerId: r
            }
        }
    }
}]);