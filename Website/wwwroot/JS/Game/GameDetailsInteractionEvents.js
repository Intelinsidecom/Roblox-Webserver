// Game/GameDetailsInteractionEvents.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.GameDetailsInteractionEvents == "undefined" && (Roblox.GameDetailsInteractionEvents = function() {
    var i = !1,
        t, r = function() {
            return {
                pid: t
            }
        },
        n = function(n) {
            Roblox.EventStream && Roblox.EventStream.SendEvent("gameDetailsTabInteraction", n, r(t))
        },
        u = function() {
            n("about")
        },
        f = function() {
            n("store")
        },
        e = function() {
            n("leaderboards")
        },
        o = function() {
            n("gameInstances")
        },
        s = function() {
            i || (i = !0, t = $(".rbx-tabs-horizontal").data("place-id"), $("#tab-about").mousedown(u), $("#tab-store").mousedown(f), $("#tab-leaderboards").mousedown(e), $("#tab-game-instances").mousedown(o))
        };
    return {
        Init: s
    }
}()), $(function() {
    Roblox.GameDetailsInteractionEvents.Init()
});