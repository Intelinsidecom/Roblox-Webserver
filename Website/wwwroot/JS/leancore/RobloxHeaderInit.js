// utilities/performance.js
Roblox = Roblox || {}, Roblox.Performance = function() {
    function r() {
        return "performance" in window && "timing" in window.performance && "navigation" in window.performance && "measure" in window.performance
    }

    function y() {
        var n = t.data("performance-relative-value");
        return Math.random() < n && r()
    }

    function v() {
        var n = t.data("send-event-percentage");
        return Roblox.EventStream && n && n != 0 && Math.random() < n && r()
    }

    function a() {
        typeof i == "undefined" && (i = t.data("internal-page-name"))
    }

    function l() {
        typeof n == "undefined" && (n = window.performance.timing)
    }

    function nt(n, t, i, r) {
        var u = [];
        return u.push(n), u.push(t), u.push(i), r = Math.floor(r), u.push(r), u
    }

    function tt(n, t, i, r) {
        if (typeof GoogleAnalyticsEvents != "undefined" && r > 0) {
            var u = nt(n, t, i, r);
            GoogleAnalyticsEvents.FireEvent(u)
        }
    }

    function h(n, t) {
        return typeof t == "undefined" && (t = d), t + ":" + n
    }

    function s(n) {
        return n + "_measure"
    }

    function o(n, t) {
        if (window.performance && typeof window.performance.mark == "function") {
            var i = h(n, t);
            window.performance.mark(i)
        }
    }

    function e(n) {
        var t = n.split(":"),
            r = t[0],
            i = t[1];
        t.length > 2 && (i = t[2] + ":" + i), window.performance.measure(s(i), r, n)
    }

    function w() {
        var n, t, i;
        if (typeof window.performance.getEntriesByType == "function" && (n = window.performance.getEntriesByType("mark"), n && n.length > 0))
            for (t = 0; t < n.length; t++) i = n[t], e(i.name)
    }

    function b(n) {
        var f = [],
            t, i, u, r;
        if (typeof window.performance.getEntriesByName == "function" && (t = window.performance.getEntriesByName(n), t && t.length > 0))
            for (i = 0; i < t.length; i++) u = t[i], r = {}, r.name = u.name, r.duration = Math.floor(u.duration), f.push(r);
        return f
    }

    function k() {
        var u = [],
            i, r, f, e, t;
        if (n.domComplete && n.navigationStart && (i = Math.floor(n.domComplete - n.navigationStart), i > 0 && (t = {}, t.name = "PageLoad", t.duration = i, u.push(t))), n.responseEnd && n.navigationStart && (i = Math.floor(n.responseEnd - n.navigationStart), i > 0 && i.toString().length < 6 && (t = {}, t.name = "FirstByte", t.duration = i, u.push(t))), typeof window.performance.getEntriesByType == "function" && (r = window.performance.getEntriesByType("measure"), r && r.length > 0))
            for (f = 0; f < r.length; f++) e = r[f], t = {}, t.name = e.name, t.duration = Math.floor(e.duration), u.push(t);
        return u
    }

    function f(n) {
        var t, r;
        if (n && n.length > 0)
            for (t = 0; t < n.length; t++) r = n[t], tt(g, i, r.name, r.duration)
    }

    function u(n) {
        var r, t;
        if (n && n.length > 0) {
            for (r = {}, t = 0; t < n.length; t++) r[n[t].name] = n[t].duration;
            Roblox.EventStream.SendEventWithTarget("pagePerformance", i, r, Roblox.EventStream.TargetTypes.WWW)
        }
    }

    function p(n) {
        var p, w, c;
        t = $("#rbx-body"), t && r() && (l(), a(), typeof i != "undefined" && (o(n), p = h(n), e(p), w = s(n), c = b(w), y() && f(c), v() && u(c)))
    }

    function c() {
        if (t = $("#rbx-body"), t && r() && (l(), a(), typeof i != "undefined")) {
            w();
            var n = k();
            y() && f(n), v() && u(n)
        }
    }
    var n, g = "Performance",
        i, t, d = "navigationStart";
    return $(window).load(function() {
        c()
    }), {
        setPerformanceMark: o,
        logSinglePerformanceMark: p,
        logPerformance: c
    }
}();