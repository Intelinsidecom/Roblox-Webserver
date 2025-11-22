// utilities/touchMovementSensitivity.js
"use strict";
var Roblox = Roblox || {};
Roblox.TouchMovementSensitivity = function () {
    function n(n, t) {
        return /touch/.test(n.type) ? (n.originalEvent || n).changedTouches[0][t] : n[t]
    }

    function r(r) {
        var o, s, u = 0,
            f = 0,
            e = 20;
        $(".container-main").on("touchstart", r, function (r) {
            o = n(r, t), s = n(r, i)
        }).on("touchmove", function (r) {
            u = Math.max(u, Math.abs(n(r, t) - o)), f = Math.max(f, Math.abs(n(r, i) - s))
        }).on("touchend", function (r) {
            var h = n(r, t),
                c = n(r, i);
            Math.abs(h - o) < e && Math.abs(c - s) < e && u < e && f < e && (r.preventDefault(), r.target.click()), u = 0, f = 0
        })
    }
    var t = "clientX",
        i = "clientY";
    return {
        improveTouchHandling: r
    }
}(), $(function () {
    Roblox.TouchMovementSensitivity.improveTouchHandling(".game-card-container"), Roblox.TouchMovementSensitivity.improveTouchHandling(".VisitButton a")
});