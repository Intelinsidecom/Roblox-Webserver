// RobloxEventStream.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.EventStream == "undefined" && (Roblox.EventStream = function() {
    function s() {
        t = !0
    }

    function h(n, i) {
        var u, r, f, e;
        return u = t ? this.location.href : window.location.href, r = {
            evt: n,
            ctx: i,
            url: u,
            lt: (new Date).toISOString()
        }, t || (f = Roblox.Cookies.getGuestId(), f && (r.gid = f), e = Roblox.Cookies.getSessionId(), e && (r.sid = e)), r
    }

    function e(e, s, c, l) {
        var y = "",
            w, p, b;
        switch (l) {
            case n.DEFAULT:
                y = i;
                break;
            case n.WWW:
                y = r;
                break;
            case n.STUDIO:
                y = u;
                break;
            case n.DIAGNOSTIC:
                y = f
        }
        if (t && l !== n.DEFAULT) throw "TargetType '" + l + "' is not supported in Service Worker mode";
        e && s && y !== "" && (w = h(e, s), p = y, p += y.indexOf("?") === -1 ? "?" : "&", t ? (a(c, w), p += v(c), Roblox.Fetch.getWithNoCredentials(p)) : ($.extend(c, w), p += $.param(c), b = new Image, b.src = p), o.LocalEventLog.push({
            eventName: e,
            context: s,
            additionalProperties: c
        }))
    }

    function c(t, i, r) {
        return e(t, i, r, n.DEFAULT)
    }

    function l(n, t, e, o) {
        i = n, r = t, u = e, f = o
    }

    function a(n, t) {
        for (var i in t) t.hasOwnProperty(i) && (n[i] = t[i]);
        return n
    }

    function v(n) {
        var i = [],
            t;
        for (t in n) n.hasOwnProperty(t) && i.push(encodeURIComponent(t) + "=" + encodeURIComponent(n[t]));
        return i.join("&")
    }
    var n = {
            DEFAULT: 0,
            WWW: 1,
            STUDIO: 2,
            DIAGNOSTIC: 3
        },
        i, r, u, f, t = !1,
        o = {
            Init: l,
            SendEvent: c,
            SendEventWithTarget: e,
            LocalEventLog: [],
            TargetTypes: n,
            SetServiceWorkerMode: s
        };
    return o
}());