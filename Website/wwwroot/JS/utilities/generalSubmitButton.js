// utilities/generalSubmitButton.js
var Roblox = Roblox || {};
Roblox.SubmitButton = function () {
    function s() {
        $(e).each(function (n, r) {
            var u = $(r),
                f = u.data(i),
                s;
            if (f && typeof f == "object") {
                var l = u.data(o) === "true",
                    a = h(u, f, l),
                    e = c(u, a);
                u.on(t, e);
                s = u.data(i) !== "false", e(null, s)
            }
        })
    }

    function h(i, r, u) {
        return function (f) {
            var l = i.data(n) === "true",
                o, s, e, h, c;
            if (!l) {
                f.preventDefault();
                return
            }
            if (o = !1, r) {
                for (s = r.object.split("."), e = window[s[0]], h = 1; h < s.length; h++) e = e[s[h]];
                if (e && (c = e[r.func], typeof c == "function")) try {
                    c(), o = !0
                } catch (f) {
                    o = !1
                }
            }!u && o && i.trigger(t, !1)
        }
    }

    function c(t, i) {
        return function (e, o) {
            if (o) {
                t.data(n, "true"), t.addClass(r).removeClass(u).removeClass(f);
                t.on("click", i)
            } else t.addClass(u).addClass(f).removeClass(r), t.data(n, "false"), t.off("click")
        }
    }
    var e = ".submit-button",
        n = "clickable",
        o = "allow-multi-click",
        i = "callback",
        r = "btn-primary",
        u = "btn-disabled-primary",
        f = "disabled",
        t = "Roblox.SubmitButton.toggleButton";
    return {
        init: s,
        submitToggleEvent: t
    }
}(), $(function () {
    Roblox.SubmitButton.init()
});