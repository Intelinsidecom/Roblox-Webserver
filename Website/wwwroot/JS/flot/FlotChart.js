// flot/FlotChart.js
var Roblox = Roblox || {};
Roblox.FlotChart = function(n, t) {
    function g(n) {
        for (var r = n.length, i = 0, t = 0; t < r; t++) i += n[t][1];
        return i
    }

    function b() {
        for (var r = [], f = i.data, l, c, t, n = 0; n < f.length; n++) e[n] && (o() || s()) ? r[n] = 0 : o() ? (l = g(f[n]), r[n] = l) : r[n] = f[n];
        if (c = [], u.length > 0)
            for (t = 0; t < u.length; t++) c.push({
                data: r[t],
                label: u[t],
                yaxis: 1,
                index: t,
                lines: {
                    show: h() && !e[t]
                }
            });
        return c
    }
    this.plot = null, this.url = null;
    var i = this,
        f = t.chartType || "line",
        w = t.chartTypes || [],
        u = t.seriesNames || [],
        r = t.seriesUnits || [],
        it = t.seriesUnitFormat || "int",
        e = {},
        l = 0,
        tt = f,
        p = n + "-legend",
        y = n + "-chart-types",
        d = "flot-tooltip",
        ut = !0,
        k = function(n) {
            var r = l++;
            l >= u.length && (l = 0);
            var f = i.data.length > 1,
                o = e[r] ? "" : ' checked="checked" ',
                s = f ? '<input data-index="' + r + '"type="checkbox" ' + o + "></input>" : "";
            return '<td class="legendLabel">' + s + n + "</td>"
        },
        o = function(n) {
            return (n || f) === "pie"
        },
        h = function(n) {
            return (n || f) === "line"
        },
        s = function(n) {
            return (n || f) === "stacked"
        },
        nt = function() {
            var r = $(y),
                u, i, t, f;
            if (r.length)
                for (r.insertAfter($(n)), r.html(""), u = 0; u < w.length; u++) i = w[u], t = undefined, o(i) ? t = $('<span class="icon-pie-chart"></span>') : h(i) ? t = $('<span class="icon-line-chart"></span>') : s(i) && (t = $('<span class="icon-bar-chart"></span>')), typeof t != "undefined" && (t.data("chart-type", i), f = $("<a href=''></a>"), f.append(t), r.append(f))
        },
        v = function(t) {
            var r = b(),
                u;
            t ? (u = a(), i.plot.shutdown(), i.plot = $.plot(n, r, u), c()) : (i.plot.setData(r), i.plot.setupGrid(), i.plot.draw(), c())
        },
        c = function() {
            nt(), $(y).find("span").click(function(n) {
                n.preventDefault();
                var t = $(this).data("chart-type");
                typeof t != "undefined" && (tt = f, f = t, v(!0))
            }), $(p).find("input").click(function() {
                var n = $(this).data("index");
                e[n] = !e[n], v(!1)
            })
        },
        rt = function() {
            var n = i.data;
            if (n && n.length) {
                var t = n[0],
                    r = t[0][0],
                    u = t[1][0];
                return u - r
            }
            return 0
        },
        a = function() {
            var n = {
                    legend: {
                        position: "sw",
                        container: p,
                        labelFormatter: k
                    },
                    series: {}
                },
                t, i, u;
            return ut && (t = r[0], typeof t == "undefined" && (t = ""), i = it === "int", u = "%s | " + (o() ? "%n" : i ? "%y.0" : "%y.2") + " " + t, n.tooltip = {
                show: !0,
                content: u.escapeHTML(),
                defaultTheme: !1,
                cssClass: d
            }, n.grid = {
                hoverable: !0,
                clickable: !0
            }), o() && (n.series.pie = {
                innerRadius: .5,
                show: !0
            }), h() && (n.lines = {
                show: !0
            }), s() && (n.series = {
                stack: !0,
                bars: {
                    show: !0,
                    barWidth: rt()
                }
            }), (h() || s()) && (n.xaxis = {
                mode: "time",
                tickLength: 0,
                timezone: "browser"
            }, r.length > 1 ? n.yaxes = [{
                min: 0,
                tickFormatter: function(n, t) {
                    return n < t.max ? n.toFixed(1) : r[0]
                }
            }, {
                min: 0,
                alignTicksWithAxis: 1,
                position: "right",
                tickFormatter: function(n, t) {
                    return n < t.max ? n.toFixed(0) : r[1]
                }
            }] : n.yaxis = {
                min: 0,
                tickFormatter: function(n, t) {
                    return n < t.max ? n.toFixed(1) : r[0]
                }
            }), n
        };
    this.getDataFromUrl = function(n, t) {
        $.ajax({
            url: i.url,
            success: n,
            error: t
        })
    }, this.setUrl = function(n) {
        i.url = n
    }, this.drawChartFromEndpoint = function(t, r) {
        i.getDataFromUrl(function(r) {
            typeof t == "function" && t(r.Data, n), i.data = r.Data, i.drawChartFromData(r)
        }, function(t) {
            typeof r == "function" && r(t, n)
        })
    }, this.drawChartFromData = function(t) {
        var s = t.Data,
            f, e, o;
        if (i.data = s, t.SeriesNames !== null && (u = t.SeriesNames), !r && u)
            for (r = [], f = 0; f < u.length; f++) r.push("");
        e = a(), o = b(), this.plot = $.plot(n, o, e), c()
    }
};