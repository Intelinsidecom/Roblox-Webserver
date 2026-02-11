// Games/Refactor/legacy.js
function tombstone(n) {
    window.console && typeof n == "string" && (window.console.error && console.error(n, "is deprecated but was called"), window.console.trace && console.trace());
    return
}
var Roblox = Roblox || {};
typeof Roblox.GamesDisplayShared == "undefined" && (Roblox.GamesDisplayShared = {}), Roblox.GamesListBehavior = {}, Roblox.GamesListBehavior.RefreshAdsInGamesPageEnabled = !1, Roblox.GamesPageContainerBehavior = function() {
    function n() {
        return Roblox.GamesPage.settings.deviceTypeId
    }
    var t, i, r, u = 0,
        f = !1,
        e = 400,
        o;
    return $(function() {}), {
        getDeviceTypeId: n
    }
}();