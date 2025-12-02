// utilities/performance.js
Roblox = Roblox || {}, Roblox.Performance = function() {
    function p() {
        var n = t.data("performance-relative-value");
        return Math.random() < n && "performance" in window && "timing" in window.performance && "navigation" in window.performance
    }

    function f() {
        var n = t.data("send-event-percentage");
        return Roblox.EventStream && n && n != 0 && Math.random() < n && "performance" in window && "timing" in window.performance && "navigation" in window.performance
    }

    function e() {
        i = t.data("internal-page-name")
    }

    function o() {
        n = window.performance.timing
    }

    function w(n, t, i, r) {
        var u = [];
        return u.push(n), u.push(t), u.push(i), r = Math.floor(r), u.push(r), u
    }

    function h(n, t, i, r) {
        if (typeof GoogleAnalyticsEvents != "undefined" && r > 0) {
            var u = w(n, t, i, r);
            GoogleAnalyticsEvents.FireEvent(u)
        }
    }

    function c(n, t) {
        if (window.performance && typeof window.performance.mark == "function") {
            typeof t == "undefined" && (t = "navigationStart");
            var i = t + ":" + n;
            window.performance.mark(i)
        }
    }

    function l(n) {
        var t = n.split(":"),
            r = t[0],
            i = t[1];
        t.length > 2 && (i = t[2] + ":" + i), window.performance.measure(i + "_measure", r, n)
    }

    function a() {
        var n, t, i;
        if (typeof window.performance.getEntriesByType == "function" && (n = window.performance.getEntriesByType("mark"), n && n.length > 0))
            for (t = 0; t < n.length; t++) i = n[t], l(i.name)
    }

    function r() {
        var u = [],
            i, r, f, e, t;
        if (n.domComplete && n.navigationStart && (i = Math.floor(n.domComplete - n.navigationStart), i > 0 && (t = {}, t.name = "PageLoad", t.duration = i, u.push(t))), n.responseEnd && n.navigationStart && (i = Math.floor(n.responseEnd - n.navigationStart), i > 0 && i.toString().length < 6 && (t = {}, t.name = "FirstByte", t.duration = i, u.push(t))), typeof window.performance.getEntriesByType == "function" && (r = window.performance.getEntriesByType("measure"), r && r.length > 0))
            for (f = 0; f < r.length; f++) e = r[f], t = {}, t.name = e.name, t.duration = Math.floor(e.duration), u.push(t);
        return u
    }

    function y() {
        var n = r(),
            t, u;
        if (n && n.length > 0)
            for (t = 0; t < n.length; t++) u = n[t], h(v, i, u.name, u.duration)
    }

    function s() {
        var n = r(),
            u, t;
        if (n && n.length > 0) {
            for (u = {}, t = 0; t < n.length; t++) u[n[t].name] = n[t].duration;
            Roblox.EventStream.SendEventWithTarget("pagePerformance", i, u, Roblox.EventStream.TargetTypes.WWW)
        }
    }

    function u() {
        t = $("#rbx-body"), t && (o(), e(), typeof i != "undefined" && (a(), p() && y(), f() && s()))
    }
    var n, v = "Performance",
        i, t;
    return $(window).load(function() {
        u()
    }), {
        setPerformanceMark: c,
        logPerformance: u
    }
}();