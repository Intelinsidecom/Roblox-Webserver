// flot/jquery.flot.categories.js
(function(n) {
    function r(n, t, i, r) {
        var o = t.xaxis.options.mode == "categories",
            s = t.yaxis.options.mode == "categories",
            u, f, h, e;
        if (o || s)
            for (u = r.format, u || (f = t, u = [], u.push({
                    x: !0,
                    number: !0,
                    required: !0
                }), u.push({
                    y: !0,
                    number: !0,
                    required: !0
                }), (f.bars.show || f.lines.show && f.lines.fill) && (h = !!(f.bars.show && f.bars.zero || f.lines.show && f.lines.zero), u.push({
                    y: !0,
                    number: !0,
                    required: !1,
                    defaultValue: 0,
                    autoscale: h
                }), f.bars.horizontal && (delete u[u.length - 1].y, u[u.length - 1].x = !0)), r.format = u), e = 0; e < u.length; ++e) u[e].x && o && (u[e].number = !1), u[e].y && s && (u[e].number = !1)
    }

    function u(n) {
        var t = -1,
            i;
        for (i in n) n[i] > t && (t = n[i]);
        return t + 1
    }

    function f(n) {
        var i = [],
            r, t;
        for (r in n.categories) t = n.categories[r], t >= n.min && t <= n.max && i.push([t, r]);
        return i.sort(function(n, t) {
            return n[0] - t[0]
        }), i
    }

    function t(t, i, r) {
        var s, u, o, h;
        if (t[i].options.mode == "categories") {
            if (!t[i].categories) {
                if (s = {}, u = t[i].options.categories || {}, n.isArray(u))
                    for (o = 0; o < u.length; ++o) s[u[o]] = o;
                else
                    for (h in u) s[h] = u[h];
                t[i].categories = s
            }
            t[i].options.ticks || (t[i].options.ticks = f), e(r, i, t[i].categories)
        }
    }

    function e(n, t, i) {
        for (var o = n.points, s = n.pointsize, c = n.format, l = t.charAt(0), h = u(i), f, e, r = 0; r < o.length; r += s)
            if (o[r] != null)
                for (f = 0; f < s; ++f)(e = o[r + f], e != null && c[f][l]) && (e in i || (i[e] = h, ++h), o[r + f] = i[e])
    }

    function o(n, i, r) {
        t(i, "xaxis", r), t(i, "yaxis", r)
    }

    function s(n) {
        n.hooks.processRawData.push(r), n.hooks.processDatapoints.push(o)
    }
    var i = {
        xaxis: {
            categories: null
        },
        yaxis: {
            categories: null
        }
    };
    n.plot.plugins.push({
        init: s,
        options: i,
        name: "categories",
        version: "1.0"
    })
})(jQuery);