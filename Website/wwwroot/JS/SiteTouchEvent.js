// SiteTouchEvent.js
typeof Roblox == "undefined" && (Roblox = {}), Roblox.SiteTouchEvent = function() {
    function i() {
        var t, i;
        return localStorage == null ? new Date(0) : (typeof localStorage != "undefined" && (t = localStorage.getItem(n)), (typeof t == "undefined" || t === null) && (t = $.cookie(n)), i = Date.parse(t), t && !isNaN(i) ? new Date(i) : new Date(0))
    }

    function r(i) {
        localStorage != null && (typeof i == "undefined" && (i = new Date), typeof localStorage != "undefined" && (t.useLocalStorage ? $.cookie(n, null) : localStorage.removeItem(n)), t.useLocalStorage && typeof localStorage != "undefined" ? localStorage.setItem(n, i) : $.cookie(n, i, {
            expires: 100
        }))
    }

    function u() {
        var n = i();
        Math.floor((new Date - n) / 36e5) >= t.dateDiffThresholdInHours && RobloxEventManager.triggerEvent("rbx_evt_sitetouch"), r()
    }
    var n = "LastActivity",
        t = {
            updateLastActivityAndFireEvent: u,
            getLastActivity: i,
            setLastActivity: r,
            dateDiffThresholdInHours: 3,
            useLocalStorage: !1
        };
    return t
}();