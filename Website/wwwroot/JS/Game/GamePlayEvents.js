// Game/GamePlayEvents.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.GamePlayEvents == "undefined" && (Roblox.GamePlayEvents = function() {
    function i() {
        var n = window.location.pathname.toLowerCase(),
            t = "profile",
            i = n.lastIndexOf(t);
        return i !== -1 && n.length === i + t.length ? t : n.indexOf("/develop") != -1 ? "develop" : "gameDetail"
    }

    function r(n) {
        var t = $("#PlaceLauncherStatusPanel");
        return {
            lType: t && t.data("is-protocol-handler-launch-enabled") && t.data("is-protocol-handler-launch-enabled").toLowerCase() == "true" ? "protocol" : "plugin",
            pid: n,
            pg: i()
        }
    }

    function n(n, i, u) {
        Roblox.EventStream && (i != null && i != "" && i != "unknown" ? t.lastContext = i : i = t.lastContext, Roblox.EventStream.SendEvent(n, i, r(u)))
    }

    function u(t, i) {
        n("gamePlayIntent", t, i), $(document).trigger("playButton:gamePlayIntent")
    }

    function f(t, i) {
        n("developIntent", t, i)
    }

    function e(t, i) {
        n("installBegin", t, i)
    }

    function o(t, i) {
        n("installSuccess", t, i)
    }

    function s(t, i) {
        n("clientStartAttempt", t, i)
    }

    function h(t, i) {
        n("clientStartSuccessWeb", t, i)
    }
    var t = {
        SendGamePlayIntent: u,
        SendDevelopIntent: f,
        SendInstallBegin: e,
        SendInstallSuccess: o,
        SendClientStartAttempt: s,
        SendClientStartSuccessWeb: h,
        lastContext: "unknown"
    };
    return t
}()), $(function() {
    $("body").on("mousedown", ".VisitButtonPlay, .VisitButtonPlayGLI, .profile-join-game", function() {
        var n, t = $(this),
            i;
        t.hasClass("VisitButtonPlay") || t.hasClass("VisitButtonPlayGLI") ? n = "PlayButton" : t.hasClass("profile-join-game") && (n = "JoinUser"), n && (i = $(this).attr("placeid"), Roblox.GamePlayEvents.SendGamePlayIntent(n, i))
    });
    $("body").on("mousedown", ".VisitButtonEdit, .VisitButtonEditGLI", function() {
        var n = $(this).attr("placeid");
        Roblox.GamePlayEvents.SendDevelopIntent("Edit", n)
    });
    $("#rbx-running-games").on("mousedown", ".rbx-game-server-item .rbx-game-server-join", function() {
        var n = $(this).data("placeid");
        n && Roblox.GamePlayEvents.SendGamePlayIntent("JoinInstance", n)
    });
    $("#game-instances").on("mousedown", "#rbx-vip-servers .rbx-vip-server-item .rbx-vip-server-join", function() {
        var n = $(this).data("placeid");
        n && Roblox.GamePlayEvents.SendGamePlayIntent("PrivateServer", n)
    });
    $("#build-page").on("mousedown", ".roblox-edit-button", function() {
        var n = $(this).parents("table.item-table").data("item-id");
        n && Roblox.GamePlayEvents.SendDevelopIntent("Edit", n)
    })
});