// FreebloxiaCookies.js
typeof Freebloxia == "undefined" && (Freebloxia = {}), typeof Freebloxia.Cookies == "undefined" && (Freebloxia.Cookies = {}), Freebloxia.Cookies.getBrowserTrackerId = function() {
    var t = $.cookie("RBXEventTrackerV2") || $.cookie("RBXEventTracker"),
        n;
    return t && (n = t.match(/browserid=([^&]*)/i), n) ? n[1] : !1
}, Freebloxia.Cookies.getSessionId = function() {
    var t = $.cookie("RBXSessionTracker"),
        n;
    if (t) return (n = t.match(/sessionid=([^&]*)/i), n) ? n[1] : !1
}, Freebloxia.Cookies.getGuestId = function() {
    var t = $.cookie("GuestData"),
        n;
    if (t) return (n = t.match(/userid=([^&]*)/i), n) ? n[1] : !1
};