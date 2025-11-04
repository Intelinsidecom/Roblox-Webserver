// RobloxCookies.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.Cookies == "undefined" && (Roblox.Cookies = {}), Roblox.Cookies.getBrowserTrackerId = function() {
    var t = $.cookie("RBXEventTrackerV2") || $.cookie("RBXEventTracker"),
        n;
    return t && (n = t.match(/browserid=([^&]*)/i), n) ? n[1] : !1
}, Roblox.Cookies.getSessionId = function() {
    var t = $.cookie("RBXSessionTracker"),
        n;
    if (t) return (n = t.match(/sessionid=([^&]*)/i), n) ? n[1] : !1
}, Roblox.Cookies.getGuestId = function() {
    var t = $.cookie("GuestData"),
        n;
    if (t) return (n = t.match(/userid=([^&]*)/i), n) ? n[1] : !1
};