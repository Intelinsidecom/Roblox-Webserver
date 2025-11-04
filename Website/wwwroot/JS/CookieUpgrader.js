// CookieUpgrader.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.CookieUpgrader == "undefined" && (Roblox.CookieUpgrader = {}), Roblox.CookieUpgrader.domain = "", Roblox.CookieUpgrader.oneMonthInMs = 2592e6, Roblox.CookieUpgrader.upgrade = function(n, t) {
    function e(n) {
        var u = null,
            i, t, r;
        if (document.cookie && document.cookie !== "")
            for (i = document.cookie.split(";"), t = 0; t < i.length; t++)
                if (r = jQuery.trim(i[t]), r.substring(0, n.length + 1) === n + "=") {
                    u = r.substring(n.length + 1);
                    break
                } return u
    }
    var i, o, r, f, s, u, h;
    if (Roblox.CookieUpgrader.domain !== "" && (i = e(n), i != null)) try {
        if (o = document.cookie.length, document.cookie = n + "=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/", document.cookie = n + "=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/; domain=" + window.location.host, document.cookie.length === o) return;
        if (r = n, typeof t.newCookieName != "undefined" && (r = t.newCookieName), f = e(n), f != null) {
            typeof GoogleAnalyticsEvents != "undefined" && GoogleAnalyticsEvents.FireEvent(["CookieUpgrader", "DeletedRedundantCookie", n]), s = {
                cookieName: n,
                cookieValue: f
            }, Roblox.EventStream && Roblox.EventStream.SendEventWithTarget("CookieUpgrader", "DeletedRedundantCookie", s, Roblox.EventStream.TargetTypes.DIAGNOSTIC);
            return
        }
        u = r + "=" + i + "; ", u += "expires=" + t.expires(i).toUTCString() + "; ", u += "path=/; domain=" + Roblox.CookieUpgrader.domain, document.cookie = u, typeof GoogleAnalyticsEvents != "undefined" && GoogleAnalyticsEvents.FireEvent(["CookieUpgrader", "ConvertedCookie", n]), h = {
            cookieName: n,
            cookieValue: i,
            newCookieName: r
        }, Roblox.EventStream && Roblox.EventStream.SendEventWithTarget("CookieUpgrader", "ConvertedCookie", h, Roblox.EventStream.TargetTypes.DIAGNOSTIC)
    } catch (c) {
        typeof GoogleAnalyticsEvents != "undefined" && GoogleAnalyticsEvents.FireEvent(["CookieUpgrader", "ExceptionDuringConvertOf" + n, c.message])
    }
}, Roblox.CookieUpgrader.getExpirationFromCookieValue = function(n, t) {
    var f = new RegExp(n + "=(\\d+)/(\\d+)/(\\d+)"),
        i = t.match(f),
        u = +new Date,
        r;
    return i != null && i.length != 0 && (r = new Date(i[3], i[1] - 1, i[2]), isNaN(r.getTime()) || (u = r.getTime())), new Date(u + Roblox.CookieUpgrader.oneMonthInMs)
}, Roblox.CookieUpgrader.thirtyDaysFromNow = function() {
    return new Date(+new Date + Roblox.CookieUpgrader.oneMonthInMs)
}, Roblox.CookieUpgrader.thirtyYearsFromNow = function() {
    return new Date(+new Date + 94608e7)
}, Roblox.CookieUpgrader.fourHoursFromNow = function() {
    return new Date(+new Date + 144e5)
};