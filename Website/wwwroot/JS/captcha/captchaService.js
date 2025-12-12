// captcha/captchaService.js
"use strict";
var Roblox = Roblox || {},
    grecaptcha = window.grecaptcha || null,
    EventTracker = window.EventTracker || null;
Roblox.Captcha = function() {
    function r(n) {
        return n.charAt(0).toUpperCase() + n.slice(1)
    }

    function a(n) {
        return n.charAt(0).toLowerCase() + n.slice(1)
    }

    function h() {
        return (new Date).valueOf()
    }

    function y(t) {
        for (var e = 1e3, u = n.captchaSolvedPrefix, f = n.captchaSolveTimeIntervals, r, i = 0; i < f.length; i++)
            if (r = f[i], t <= r.seconds * e) return u + r.suffix;
        return u + n.captchaSolveTimeLarge
    }

    function p(n) {
        var t = y(n);
        EventTracker.fireEvent(r(i + t))
    }

    function c(i, u) {
        var e = {
            "g-Recaptcha-Response": u,
            isInvisible: t.invisible
        };
        $.ajax({
            method: "POST",
            data: e,
            success: function() {
                EventTracker.fireEvent(r(i + n.successSuffix)), o && (o(), $("#" + l).empty())
            },
            error: function() {
                EventTracker.fireEvent(r(i + n.failSuffix)), f && f(), Roblox.BootstrapWidgets && Roblox.BootstrapWidgets.ToggleSystemMessage($(".alert-warning"), 100, 2e3, Roblox.CaptchaConstants.messages.error)
            },
            url: s[i]
        })
    }
    var s = Roblox.CaptchaConstants.endpoints,
        l, t = {
            invisible: !1
        },
        n = Roblox.CaptchaConstants.serviceData,
        e, i, o, f, u, v = function(n) {
            if (typeof u == "function" && u(), e) {
                var t = h() - e;
                p(t), e = null
            }
            c(i, n)
        };
    return {
        types: Roblox.CaptchaConstants.types,
        setEndpoint: function(n, t) {
            s[n] = t
        },
        getEndpoint: function(n) {
            return s[n]
        },
        setInvisibleMode: function(n) {
            t.invisible = n
        },
        getInvisibleMode: function() {
            return t.invisible
        },
        setSiteKey: function(t) {
            n.sitekey = t
        },
        verify: c,
        reset: function(n, r, e, s) {
            i = n, o = r, f = e, u = s, grecaptcha && (grecaptcha.reset(), t.invisible && grecaptcha.execute())
        },
        render: function(s, c, a, y, p) {
            if (i = c, o = a, f = y, u = p, l = s, grecaptcha) {
                var w = {
                    sitekey: n.sitekey,
                    callback: v
                };
                t.invisible && (w.size = "invisible"), grecaptcha.render(s, w), EventTracker.fireEvent(r(i + n.displayedSuffix)), e = h()
            }
        },
        execute: function() {
            grecaptcha && t.invisible && grecaptcha.execute()
        },
        setMultipleEndpoints: function(n, t) {
            var i, r;
            if (n && t)
                for (i = 0; i < n.length; i++) r = a(n[i]), s[r] = t
        }
    }
}();