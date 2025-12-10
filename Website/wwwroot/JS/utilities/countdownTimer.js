// utilities/countdownTimer.js
"use strict";
var Roblox = Roblox || {};
Roblox.CountdownTimer = function() {
    function e(e) {
        return e < n ? (t = 0, i = 0, r = 0, u = 0, f = !0) : (t = Math.floor(e / n % 60), i = Math.floor(e / n / 60 % 60), r = Math.floor(e / (n * 3600) % 24), u = Math.floor(e / (n * 86400)), f = !1), {
            days: u,
            hours: r,
            minutes: i,
            seconds: t,
            isDone: f
        }
    }

    function o(t, i, r) {
        if (t && i) var u = Date.parse(i) - Date.parse(t),
            f = setInterval(function() {
                var t = e(u);
                t.isDone && clearInterval(f), r(t), u -= n
            }, n)
    }
    var n = 1e3,
        t, i, r, u, f;
    return {
        InitializeClock: o
    }
}();