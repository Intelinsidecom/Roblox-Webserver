// flot/jquery.flot.js
(function(n) {
    n.color = {}, n.color.make = function(t, i, r, u) {
        var f = {};
        return f.r = t || 0, f.g = i || 0, f.b = r || 0, f.a = u != null ? u : 1, f.add = function(n, t) {
            for (var i = 0; i < n.length; ++i) f[n.charAt(i)] += t;
            return f.normalize()
        }, f.scale = function(n, t) {
            for (var i = 0; i < n.length; ++i) f[n.charAt(i)] *= t;
            return f.normalize()
        }, f.toString = function() {
            return f.a >= 1 ? "rgb(" + [f.r, f.g, f.b].join(",") + ")" : "rgba(" + [f.r, f.g, f.b, f.a].join(",") + ")"
        }, f.normalize = function() {
            function n(n, t, i) {
                return t < n ? n : t > i ? i : t
            }
            return f.r = n(0, parseInt(f.r), 255), f.g = n(0, parseInt(f.g), 255), f.b = n(0, parseInt(f.b), 255), f.a = n(0, f.a, 1), f
        }, f.clone = function() {
            return n.color.make(f.r, f.b, f.g, f.a)
        }, f.normalize()
    }, n.color.extract = function(t, i) {
        var r;
        do {
            if (r = t.css(i).toLowerCase(), r != "" && r != "transparent") break;
            t = t.parent()
        } while (t.length && !n.nodeName(t.get(0), "body"));
        return r == "rgba(0, 0, 0, 0)" && (r = "transparent"), n.color.parse(r)
    }, n.color.parse = function(i) {
        var r, u = n.color.make,
            f;
        return (r = /rgb\(\s*([0-9]{1,3})\s*,\s*([0-9]{1,3})\s*,\s*([0-9]{1,3})\s*\)/.exec(i)) ? u(parseInt(r[1], 10), parseInt(r[2], 10), parseInt(r[3], 10)) : (r = /rgba\(\s*([0-9]{1,3})\s*,\s*([0-9]{1,3})\s*,\s*([0-9]{1,3})\s*,\s*([0-9]+(?:\.[0-9]+)?)\s*\)/.exec(i)) ? u(parseInt(r[1], 10), parseInt(r[2], 10), parseInt(r[3], 10), parseFloat(r[4])) : (r = /rgb\(\s*([0-9]+(?:\.[0-9]+)?)\%\s*,\s*([0-9]+(?:\.[0-9]+)?)\%\s*,\s*([0-9]+(?:\.[0-9]+)?)\%\s*\)/.exec(i)) ? u(parseFloat(r[1]) * 2.55, parseFloat(r[2]) * 2.55, parseFloat(r[3]) * 2.55) : (r = /rgba\(\s*([0-9]+(?:\.[0-9]+)?)\%\s*,\s*([0-9]+(?:\.[0-9]+)?)\%\s*,\s*([0-9]+(?:\.[0-9]+)?)\%\s*,\s*([0-9]+(?:\.[0-9]+)?)\s*\)/.exec(i)) ? u(parseFloat(r[1]) * 2.55, parseFloat(r[2]) * 2.55, parseFloat(r[3]) * 2.55, parseFloat(r[4])) : (r = /#([a-fA-F0-9]{2})([a-fA-F0-9]{2})([a-fA-F0-9]{2})/.exec(i)) ? u(parseInt(r[1], 16), parseInt(r[2], 16), parseInt(r[3], 16)) : (r = /#([a-fA-F0-9])([a-fA-F0-9])([a-fA-F0-9])/.exec(i)) ? u(parseInt(r[1] + r[1], 16), parseInt(r[2] + r[2], 16), parseInt(r[3] + r[3], 16)) : (f = n.trim(i).toLowerCase(), f == "transparent" ? u(255, 255, 255, 0) : (r = t[f] || [0, 0, 0], u(r[0], r[1], r[2])))
    };
    var t = {
        aqua: [0, 255, 255],
        azure: [240, 255, 255],
        beige: [245, 245, 220],
        black: [0, 0, 0],
        blue: [0, 0, 255],
        brown: [165, 42, 42],
        cyan: [0, 255, 255],
        darkblue: [0, 0, 139],
        darkcyan: [0, 139, 139],
        darkgrey: [169, 169, 169],
        darkgreen: [0, 100, 0],
        darkkhaki: [189, 183, 107],
        darkmagenta: [139, 0, 139],
        darkolivegreen: [85, 107, 47],
        darkorange: [255, 140, 0],
        darkorchid: [153, 50, 204],
        darkred: [139, 0, 0],
        darksalmon: [233, 150, 122],
        darkviolet: [148, 0, 211],
        fuchsia: [255, 0, 255],
        gold: [255, 215, 0],
        green: [0, 128, 0],
        indigo: [75, 0, 130],
        khaki: [240, 230, 140],
        lightblue: [173, 216, 230],
        lightcyan: [224, 255, 255],
        lightgreen: [144, 238, 144],
        lightgrey: [211, 211, 211],
        lightpink: [255, 182, 193],
        lightyellow: [255, 255, 224],
        lime: [0, 255, 0],
        magenta: [255, 0, 255],
        maroon: [128, 0, 0],
        navy: [0, 0, 128],
        olive: [128, 128, 0],
        orange: [255, 165, 0],
        pink: [255, 192, 203],
        purple: [128, 0, 128],
        violet: [128, 0, 128],
        red: [255, 0, 0],
        silver: [192, 192, 192],
        white: [255, 255, 255],
        yellow: [255, 255, 0]
    }
})(jQuery),
function(n) {
    function t(t, i) {
        var r = i.children("." + t)[0];
        if (r == null && (r = document.createElement("canvas"), r.className = t, n(r).css({
                direction: "ltr",
                position: "absolute",
                left: 0,
                top: 0
            }).appendTo(i), !r.getContext))
            if (window.G_vmlCanvasManager) r = window.G_vmlCanvasManager.initElement(r);
            else throw new Error("Canvas is not available. If you're using IE with a fall-back such as Excanvas, then there's either a mistake in your conditional include, or the page has no DOCTYPE and is rendering in Quirks Mode.");
        this.element = r;
        var u = this.context = r.getContext("2d"),
            f = window.devicePixelRatio || 1,
            e = u.webkitBackingStorePixelRatio || u.mozBackingStorePixelRatio || u.msBackingStorePixelRatio || u.oBackingStorePixelRatio || u.backingStorePixelRatio || 1;
        this.pixelRatio = f / e, this.resize(i.width(), i.height()), this.textContainer = null, this.text = {}, this._textCache = {}
    }

    function r(i, r, f, e) {
        function nt(n, t) {
            t = [l].concat(t);
            for (var i = 0; i < n.length; ++i) n[i].apply(this, t)
        }

        function ei() {
            for (var u = {
                    Canvas: t
                }, r, i = 0; i < e.length; ++i) r = e[i], r.init(l, u), r.options && n.extend(!0, o, r.options)
        }

        function oi(t) {
            var f;
            n.extend(!0, o, t), t && t.colors && (o.colors = t.colors), o.xaxis.color == null && (o.xaxis.color = n.color.parse(o.grid.color).scale("a", .22).toString()), o.yaxis.color == null && (o.yaxis.color = n.color.parse(o.grid.color).scale("a", .22).toString()), o.xaxis.tickColor == null && (o.xaxis.tickColor = o.grid.tickColor || o.xaxis.color), o.yaxis.tickColor == null && (o.yaxis.tickColor = o.grid.tickColor || o.yaxis.color), o.grid.borderColor == null && (o.grid.borderColor = o.grid.color), o.grid.tickColor == null && (o.grid.tickColor = n.color.parse(o.grid.color).scale("a", .22).toString());
            for (var r, s = i.css("font-size"), c = s ? +s.replace("px", "") : 13, h = {
                    style: i.css("font-style"),
                    size: Math.round(.8 * c),
                    variant: i.css("font-variant"),
                    weight: i.css("font-weight"),
                    family: i.css("font-family")
                }, e = o.xaxes.length || 1, u = 0; u < e; ++u) r = o.xaxes[u], r && !r.tickColor && (r.tickColor = r.color), r = n.extend(!0, {}, o.xaxis, r), o.xaxes[u] = r, r.font && (r.font = n.extend({}, h, r.font), r.font.color || (r.font.color = r.color), r.font.lineHeight || (r.font.lineHeight = Math.round(r.font.size * 1.15)));
            for (e = o.yaxes.length || 1, u = 0; u < e; ++u) r = o.yaxes[u], r && !r.tickColor && (r.tickColor = r.color), r = n.extend(!0, {}, o.yaxis, r), o.yaxes[u] = r, r.font && (r.font = n.extend({}, h, r.font), r.font.color || (r.font.color = r.color), r.font.lineHeight || (r.font.lineHeight = Math.round(r.font.size * 1.15)));
            for (o.xaxis.noTicks && o.xaxis.ticks == null && (o.xaxis.ticks = o.xaxis.noTicks), o.yaxis.noTicks && o.yaxis.ticks == null && (o.yaxis.ticks = o.yaxis.noTicks), o.x2axis && (o.xaxes[1] = n.extend(!0, {}, o.xaxis, o.x2axis), o.xaxes[1].position = "top"), o.y2axis && (o.yaxes[1] = n.extend(!0, {}, o.yaxis, o.y2axis), o.yaxes[1].position = "right"), o.grid.coloredAreas && (o.grid.markings = o.grid.coloredAreas), o.grid.coloredAreasColor && (o.grid.markingsColor = o.grid.coloredAreasColor), o.lines && n.extend(!0, o.series.lines, o.lines), o.points && n.extend(!0, o.series.points, o.points), o.bars && n.extend(!0, o.series.bars, o.bars), o.shadowSize != null && (o.series.shadowSize = o.shadowSize), o.highlightColor != null && (o.series.highlightColor = o.highlightColor), u = 0; u < o.xaxes.length; ++u) et(y, u + 1).options = o.xaxes[u];
            for (u = 0; u < o.yaxes.length; ++u) et(w, u + 1).options = o.yaxes[u];
            for (f in p) o.hooks[f] && o.hooks[f].length && (p[f] = p[f].concat(o.hooks[f]));
            nt(p.processOptions, [o])
        }

        function ni(n) {
            c = si(n), ci(), li()
        }

        function si(t) {
            for (var u = [], r, i = 0; i < t.length; ++i) r = n.extend(!0, {}, o.series), t[i].data != null ? (r.data = t[i].data, delete t[i].data, n.extend(!0, r, t[i]), t[i].data = r.data) : r.data = t[i], u.push(r);
            return u
        }

        function ft(n, t) {
            var i = n[t + "axis"];
            return typeof i == "object" && (i = i.n), typeof i != "number" && (i = 1), i
        }

        function tt() {
            return n.grep(y.concat(w), function(n) {
                return n
            })
        }

        function ti(n) {
            for (var i = {}, t, r = 0; r < y.length; ++r) t = y[r], t && t.used && (i["x" + t.n] = t.c2p(n.left));
            for (r = 0; r < w.length; ++r) t = w[r], t && t.used && (i["y" + t.n] = t.c2p(n.top));
            return i.x1 !== undefined && (i.x = i.x1), i.y1 !== undefined && (i.y = i.y1), i
        }

        function hi(n) {
            for (var u = {}, t, i, r = 0; r < y.length; ++r)
                if (t = y[r], t && t.used && (i = "x" + t.n, n[i] == null && t.n == 1 && (i = "x"), n[i] != null)) {
                    u.left = t.p2c(n[i]);
                    break
                } for (r = 0; r < w.length; ++r)
                if (t = w[r], t && t.used && (i = "y" + t.n, n[i] == null && t.n == 1 && (i = "y"), n[i] != null)) {
                    u.top = t.p2c(n[i]);
                    break
                } return u
        }

        function et(t, i) {
            return t[i - 1] || (t[i - 1] = {
                n: i,
                direction: t == y ? "x" : "y",
                options: n.extend(!0, {}, t == y ? o.xaxis : o.yaxis)
            }), t[i - 1]
        }

        function ci() {
            for (var f = c.length, e = -1, u, h, t, l, a, i = 0; i < c.length; ++i) u = c[i].color, u != null && (f--, typeof u == "number" && u > e && (e = u));
            f <= e && (f = e + 1);
            var v, s = [],
                p = o.colors,
                b = p.length,
                r = 0;
            for (i = 0; i < f; i++) v = n.color.parse(p[i % b] || "#666"), i % b == 0 && i && (r = r >= 0 ? r < .5 ? -r - .2 : 0 : -r), s[i] = v.scale("rgb", 1 + r);
            for (h = 0, i = 0; i < c.length; ++i) {
                if (t = c[i], t.color == null ? (t.color = s[h].toString(), ++h) : typeof t.color == "number" && (t.color = s[t.color].toString()), t.lines.show == null) {
                    a = !0;
                    for (l in t)
                        if (t[l] && t[l].show) {
                            a = !1;
                            break
                        } a && (t.lines.show = !0)
                }
                t.lines.zero == null && (t.lines.zero = !!t.lines.fill), t.xaxis = et(y, ft(t, "x")), t.yaxis = et(w, ft(t, "y"))
            }
        }

        function li() {
            function k(n, t, i) {
                t < n.datamin && t != -v && (n.datamin = t), i > n.datamax && i != v && (n.datamax = i)
            }
            var g = Number.POSITIVE_INFINITY,
                rt = Number.NEGATIVE_INFINITY,
                v = Number.MAX_VALUE,
                o, l, f, r, ct, t, u, h, ht, st, lt, i, s, ut, ft, e, et, ot, b, a;
            for (n.each(tt(), function(n, t) {
                    t.datamin = g, t.datamax = rt, t.used = !1
                }), o = 0; o < c.length; ++o) t = c[o], t.datapoints = {
                points: []
            }, nt(p.processRawData, [t, t.data, t.datapoints]);
            for (o = 0; o < c.length; ++o)
                if (t = c[o], ft = t.data, e = t.datapoints.format, e || (e = [], e.push({
                        x: !0,
                        number: !0,
                        required: !0
                    }), e.push({
                        y: !0,
                        number: !0,
                        required: !0
                    }), (t.bars.show || t.lines.show && t.lines.fill) && (et = !!(t.bars.show && t.bars.zero || t.lines.show && t.lines.zero), e.push({
                        y: !0,
                        number: !0,
                        required: !1,
                        defaultValue: 0,
                        autoscale: et
                    }), t.bars.horizontal && (delete e[e.length - 1].y, e[e.length - 1].x = !0)), t.datapoints.format = e), t.datapoints.pointsize == null)
                    for (t.datapoints.pointsize = e.length, h = t.datapoints.pointsize, u = t.datapoints.points, ot = t.lines.show && t.lines.steps, t.xaxis.used = t.yaxis.used = !0, l = f = 0; l < ft.length; ++l, f += h) {
                        if (ut = ft[l], b = ut == null, !b)
                            for (r = 0; r < h; ++r) i = ut[r], s = e[r], s && (s.number && i != null && (i = +i, isNaN(i) ? i = null : i == Infinity ? i = v : i == -Infinity && (i = -v)), i == null && (s.required && (b = !0), s.defaultValue != null && (i = s.defaultValue))), u[f + r] = i;
                        if (b)
                            for (r = 0; r < h; ++r) i = u[f + r], i != null && (s = e[r], s.autoscale !== !1 && (s.x && k(t.xaxis, i, i), s.y && k(t.yaxis, i, i))), u[f + r] = null;
                        else if (ot && f > 0 && u[f - h] != null && u[f - h] != u[f] && u[f - h + 1] != u[f + 1]) {
                            for (r = 0; r < h; ++r) u[f + h + r] = u[f + r];
                            u[f + 1] = u[f - h + 1], f += h
                        }
                    }
            for (o = 0; o < c.length; ++o) t = c[o], nt(p.processDatapoints, [t, t.datapoints]);
            for (o = 0; o < c.length; ++o) {
                t = c[o], u = t.datapoints.points, h = t.datapoints.pointsize, e = t.datapoints.format;
                var w = g,
                    y = g,
                    d = rt,
                    it = rt;
                for (l = 0; l < u.length; l += h)
                    if (u[l] != null)
                        for (r = 0; r < h; ++r)(i = u[l + r], s = e[r], s && s.autoscale !== !1 && i != v && i != -v) && (s.x && (i < w && (w = i), i > d && (d = i)), s.y && (i < y && (y = i), i > it && (it = i)));
                if (t.bars.show) {
                    switch (t.bars.align) {
                        case "left":
                            a = 0;
                            break;
                        case "right":
                            a = -t.bars.barWidth;
                            break;
                        default:
                            a = -t.bars.barWidth / 2
                    }
                    t.bars.horizontal ? (y += a, it += a + t.bars.barWidth) : (w += a, d += a + t.bars.barWidth)
                }
                k(t.xaxis, w, d), k(t.yaxis, y, it)
            }
            n.each(tt(), function(n, t) {
                t.datamin == g && (t.datamin = null), t.datamax == rt && (t.datamax = null)
            })
        }

        function ai() {
            i.css("padding", 0).children().filter(function() {
                return !n(this).hasClass("flot-overlay") && !n(this).hasClass("flot-base")
            }).remove(), i.css("position") == "static" && i.css("position", "relative"), a = new t("flot-base", i), it = new t("flot-overlay", i), s = a.context, v = it.context, g = n(it.element).unbind();
            var r = i.data("plot");
            r && (r.shutdown(), it.clear()), i.data("plot", l)
        }

        function yi() {
            o.grid.hoverable && (g.mousemove(kt), g.bind("mouseleave", vt)), o.grid.clickable && g.click(at), nt(p.bindEvents, [g])
        }

        function ii() {
            rt && clearTimeout(rt), g.unbind("mousemove", kt), g.unbind("mouseleave", vt), g.unbind("click", at), nt(p.shutdown, [g])
        }

        function sr(n) {
            function u(n) {
                return n
            }
            var i, r, t = n.options.transform || u,
                f = n.options.inverseTransform;
            n.direction == "x" ? (i = n.scale = d / Math.abs(t(n.max) - t(n.min)), r = Math.min(t(n.max), t(n.min))) : (i = n.scale = b / Math.abs(t(n.max) - t(n.min)), i = -i, r = Math.max(t(n.max), t(n.min))), n.p2c = t == u ? function(n) {
                return (n - r) * i
            } : function(n) {
                return (t(n) - r) * i
            }, n.c2p = f ? function(n) {
                return f(r + n / i)
            } : function(n) {
                return r + n / i
            }
        }

        function hr(n) {
            for (var t = n.options, u = n.ticks || [], i = t.labelWidth || 0, f = t.labelHeight || 0, s = i || (n.direction == "x" ? Math.floor(a.width / (u.length || 1)) : null), h = n.direction + "Axis " + n.direction + n.n + "Axis", c = "flot-" + n.direction + "-axis flot-" + n.direction + n.n + "-axis " + h, l = t.font || "flot-tick-label tickLabel", e, o, r = 0; r < u.length; ++r)(e = u[r], e.label) && (o = a.getTextInfo(c, e.label, l, null, s), i = Math.max(i, o.width), f = Math.max(f, o.height));
            n.labelWidth = t.labelWidth || i, n.labelHeight = t.labelHeight || f
        }

        function tr(t) {
            var r = t.labelWidth,
                u = t.labelHeight,
                e = t.options.position,
                l = t.direction === "x",
                f = t.options.tickLength,
                i = o.grid.axisMargin,
                s = o.grid.labelMargin,
                v = !0,
                p = !0,
                b = !0,
                c = !1;
            n.each(l ? y : w, function(n, i) {
                i && i.reserveSpace && (i === t ? c = !0 : i.options.position === e && (c ? p = !1 : v = !1), c || (b = !1))
            }), p && (i = 0), f == null && (f = b ? "full" : 5), isNaN(+f) || (s += +f), l ? (u += s, e == "bottom" ? (h.bottom += u + i, t.box = {
                top: a.height - h.bottom,
                height: u
            }) : (t.box = {
                top: h.top + i,
                height: u
            }, h.top += u + i)) : (r += s, e == "left" ? (t.box = {
                left: h.left + i,
                width: r
            }, h.left += r + i) : (h.right += r + i, t.box = {
                left: a.width - h.right,
                width: r
            })), t.position = e, t.tickLength = f, t.box.padding = s, t.innermost = v
        }

        function rr(n) {
            n.direction == "x" ? (n.box.left = h.left - n.labelWidth / 2, n.box.width = a.width - h.left - h.right + n.labelWidth) : (n.box.top = h.top - n.labelHeight / 2, n.box.height = a.height - h.bottom - h.top + n.labelHeight)
        }

        function ur() {
            var i = o.grid.minBorderMargin,
                u, r, t;
            if (i == null)
                for (i = 0, r = 0; r < c.length; ++r) i = Math.max(i, 2 * (c[r].points.radius + c[r].points.lineWidth / 2));
            t = {
                left: i,
                right: i,
                top: i,
                bottom: i
            }, n.each(tt(), function(n, i) {
                if (i.reserveSpace && i.ticks && i.ticks.length) {
                    var r = i.ticks[i.ticks.length - 1];
                    i.direction === "x" ? (t.left = Math.max(t.left, i.labelWidth / 2), r.v <= i.max && (t.right = Math.max(t.right, i.labelWidth / 2))) : (t.bottom = Math.max(t.bottom, i.labelHeight / 2), r.v <= i.max && (t.top = Math.max(t.top, i.labelHeight / 2)))
                }
            }), h.left = Math.ceil(Math.max(t.left, h.left)), h.right = Math.ceil(Math.max(t.right, h.right)), h.top = Math.ceil(Math.max(t.top, h.top)), h.bottom = Math.ceil(Math.max(t.bottom, h.bottom))
        }

        function ri() {
            var r, e = tt(),
                u = o.grid.show,
                f, t, i;
            for (t in h) f = o.grid.margin || 0, h[t] = typeof f == "number" ? f : f[t] || 0;
            nt(p.processOffset, [h]);
            for (t in h) h[t] += typeof o.grid.borderWidth == "object" ? u ? o.grid.borderWidth[t] : 0 : u ? o.grid.borderWidth : 0;
            if (n.each(e, function(n, t) {
                    t.show = t.options.show, t.show == null && (t.show = t.used), t.reserveSpace = t.show || t.options.reserveSpace, ui(t)
                }), u) {
                for (i = n.grep(e, function(n) {
                        return n.reserveSpace
                    }), n.each(i, function(n, t) {
                        or(t), er(t), fr(t, t.ticks), hr(t)
                    }), r = i.length - 1; r >= 0; --r) tr(i[r]);
                ur(), n.each(i, function(n, t) {
                    rr(t)
                })
            }
            d = a.width - h.left - h.right, b = a.height - h.bottom - h.top, n.each(e, function(n, t) {
                sr(t)
            }), u && nr(), pi()
        }

        function ui(n) {
            var t = n.options,
                r = +(t.min != null ? t.min : n.datamin),
                i = +(t.max != null ? t.max : n.datamax),
                f = i - r,
                e, u;
            f == 0 ? (e = i == 0 ? 1 : .01, t.min == null && (r -= e), (t.max == null || t.min != null) && (i += e)) : (u = t.autoscaleMargin, u != null && (t.min == null && (r -= f * u, r < 0 && n.datamin != null && n.datamin >= 0 && (r = 0)), t.max == null && (i += f * u, i > 0 && n.datamax != null && n.datamax <= 0 && (i = 0)))), n.min = r, n.max = i
        }

        function or(t) {
            var i = t.options,
                l, p, h, r, f, s, b, c;
            l = typeof i.ticks == "number" && i.ticks > 0 ? i.ticks : .3 * Math.sqrt(t.direction == "x" ? a.width : a.height);
            var v = (t.max - t.min) / l,
                o = -Math.floor(Math.log(v) / Math.LN10),
                e = i.tickDecimals;
            if (e != null && o > e && (o = e), p = Math.pow(10, -o), h = v / p, h < 1.5 ? r = 1 : h < 3 ? (r = 2, h > 2.25 && (e == null || o + 1 <= e) && (r = 2.5, ++o)) : r = h < 7.5 ? 5 : 10, r *= p, i.minTickSize != null && r < i.minTickSize && (r = i.minTickSize), t.delta = v, t.tickDecimals = Math.max(0, e != null ? e : o), t.tickSize = i.tickSize || r, i.mode == "time" && !t.tickGenerator) throw new Error("Time mode requires the flot.time plugin.");
            t.tickGenerator || (t.tickGenerator = function(n) {
                var i = [],
                    e = u(n.min, n.tickSize),
                    r = 0,
                    t = Number.NaN,
                    f;
                do f = t, t = e + r * n.tickSize, i.push(t), ++r; while (t < n.max && t != f);
                return i
            }, t.tickFormatter = function(n, t) {
                var u = t.tickDecimals ? Math.pow(10, t.tickDecimals) : 1,
                    i = "" + Math.round(n * u) / u,
                    f, r;
                return t.tickDecimals != null && (f = i.indexOf("."), r = f == -1 ? 0 : i.length - f - 1, r < t.tickDecimals) ? (r ? i : i + ".") + ("" + u).substr(1, t.tickDecimals - r) : i
            }), n.isFunction(i.tickFormatter) && (t.tickFormatter = function(n, t) {
                return "" + i.tickFormatter(n, t)
            }), i.alignTicksWithAxis != null && (f = (t.direction == "x" ? y : w)[i.alignTicksWithAxis - 1], f && f.used && f != t && (s = t.tickGenerator(t), s.length > 0 && (i.min == null && (t.min = Math.min(t.min, s[0])), i.max == null && s.length > 1 && (t.max = Math.max(t.max, s[s.length - 1]))), t.tickGenerator = function(n) {
                for (var r = [], t, i = 0; i < f.ticks.length; ++i) t = (f.ticks[i].v - f.min) / (f.max - f.min), t = n.min + t * (n.max - n.min), r.push(t);
                return r
            }, t.mode || i.tickDecimals != null || (b = Math.max(0, -Math.floor(Math.log(t.delta) / Math.LN10) + 1), c = t.tickGenerator(t), c.length > 1 && /\..*0$/.test((c[1] - c[0]).toFixed(b)) || (t.tickDecimals = b))))
        }

        function er(t) {
            var i = t.options.ticks,
                u = [],
                o, f, e, r;
            for (i == null || typeof i == "number" && i > 0 ? u = t.tickGenerator(t) : i && (u = n.isFunction(i) ? i(t) : i), t.ticks = [], o = 0; o < u.length; ++o) e = null, r = u[o], typeof r == "object" ? (f = +r[0], r.length > 1 && (e = r[1])) : f = +r, e == null && (e = t.tickFormatter(f, t)), isNaN(f) || t.ticks.push({
                v: f,
                label: e
            })
        }

        function fr(n, t) {
            n.options.autoscaleMargin && t.length > 0 && (n.options.min == null && (n.min = Math.min(n.min, t[0].v)), n.options.max == null && t.length > 1 && (n.max = Math.max(n.max, t[t.length - 1].v)))
        }

        function dt() {
            var n, t;
            for (a.clear(), nt(p.drawBackground, [s]), n = o.grid, n.show && n.backgroundColor && ir(), n.show && !n.aboveData && ct(), t = 0; t < c.length; ++t) nt(p.drawSeries, [s, c[t]]), gi(c[t]);
            nt(p.draw, [s]), n.show && n.aboveData && ct(), a.render(), ut()
        }

        function yt(n, t) {
            for (var f, i, r, u, o = tt(), s, e = 0; e < o.length; ++e)
                if (f = o[e], f.direction == t && (u = t + f.n + "axis", n[u] || f.n != 1 || (u = t + "axis"), n[u])) {
                    i = n[u].from, r = n[u].to;
                    break
                } return n[u] || (f = t == "x" ? y[0] : w[0], i = n[t + "1"], r = n[t + "2"]), i != null && r != null && i > r && (s = i, i = r, r = s), {
                from: i,
                to: r,
                axis: f
            }
        }

        function ir() {
            s.save(), s.translate(h.left, h.top), s.fillStyle = gt(o.grid.backgroundColor, b, 0, "rgba(255, 255, 255, 0)"), s.fillRect(0, 0, d, b), s.restore()
        }

        function ct() {
            var v, f, t, e, g, rt, p;
            if (s.save(), s.translate(h.left, h.top), g = o.grid.markings, g)
                for (n.isFunction(g) && (f = l.getAxes(), f.xmin = f.xaxis.min, f.xmax = f.xaxis.max, f.ymin = f.yaxis.min, f.ymax = f.yaxis.max, g = g(f)), v = 0; v < g.length; ++v) {
                    var nt = g[v],
                        i = yt(nt, "x"),
                        r = yt(nt, "y");
                    (i.from == null && (i.from = i.axis.min), i.to == null && (i.to = i.axis.max), r.from == null && (r.from = r.axis.min), r.to == null && (r.to = r.axis.max), i.to < i.axis.min || i.from > i.axis.max || r.to < r.axis.min || r.from > r.axis.max) || (i.from = Math.max(i.from, i.axis.min), i.to = Math.min(i.to, i.axis.max), r.from = Math.max(r.from, r.axis.min), r.to = Math.min(r.to, r.axis.max), i.from != i.to || r.from != r.to) && (i.from = i.axis.p2c(i.from), i.to = i.axis.p2c(i.to), r.from = r.axis.p2c(r.from), r.to = r.axis.p2c(r.to), i.from == i.to || r.from == r.to ? (s.beginPath(), s.strokeStyle = nt.color || o.grid.markingsColor, s.lineWidth = nt.lineWidth || o.grid.markingsLineWidth, s.moveTo(i.from, r.from), s.lineTo(i.to, r.to), s.stroke()) : (s.fillStyle = nt.color || o.grid.markingsColor, s.fillRect(i.from, r.to, i.to - i.from, r.from - r.to)))
                }
            for (f = tt(), t = o.grid.borderWidth, rt = 0; rt < f.length; ++rt) {
                var u = f[rt],
                    it = u.box,
                    k = u.tickLength,
                    a, c, y, w;
                if (u.show && u.ticks.length != 0) {
                    for (s.lineWidth = 1, u.direction == "x" ? (a = 0, c = k == "full" ? u.position == "top" ? 0 : b : it.top - h.top + (u.position == "top" ? it.height : 0)) : (c = 0, a = k == "full" ? u.position == "left" ? 0 : d : it.left - h.left + (u.position == "left" ? it.width : 0)), u.innermost || (s.strokeStyle = u.options.color, s.beginPath(), y = w = 0, u.direction == "x" ? y = d + 1 : w = b + 1, s.lineWidth == 1 && (u.direction == "x" ? c = Math.floor(c) + .5 : a = Math.floor(a) + .5), s.moveTo(a, c), s.lineTo(a + y, c + w), s.stroke()), s.strokeStyle = u.options.tickColor, s.beginPath(), v = 0; v < u.ticks.length; ++v)(p = u.ticks[v].v, y = w = 0, isNaN(p) || p < u.min || p > u.max || k == "full" && (typeof t == "object" && t[u.position] > 0 || t > 0) && (p == u.min || p == u.max)) || (u.direction == "x" ? (a = u.p2c(p), w = k == "full" ? -b : k, u.position == "top" && (w = -w)) : (c = u.p2c(p), y = k == "full" ? -d : k, u.position == "left" && (y = -y)), s.lineWidth == 1 && (u.direction == "x" ? a = Math.floor(a) + .5 : c = Math.floor(c) + .5), s.moveTo(a, c), s.lineTo(a + y, c + w));
                    s.stroke()
                }
            }
            t && (e = o.grid.borderColor, typeof t == "object" || typeof e == "object" ? (typeof t != "object" && (t = {
                top: t,
                right: t,
                bottom: t,
                left: t
            }), typeof e != "object" && (e = {
                top: e,
                right: e,
                bottom: e,
                left: e
            }), t.top > 0 && (s.strokeStyle = e.top, s.lineWidth = t.top, s.beginPath(), s.moveTo(0 - t.left, 0 - t.top / 2), s.lineTo(d, 0 - t.top / 2), s.stroke()), t.right > 0 && (s.strokeStyle = e.right, s.lineWidth = t.right, s.beginPath(), s.moveTo(d + t.right / 2, 0 - t.top), s.lineTo(d + t.right / 2, b), s.stroke()), t.bottom > 0 && (s.strokeStyle = e.bottom, s.lineWidth = t.bottom, s.beginPath(), s.moveTo(d + t.right, b + t.bottom / 2), s.lineTo(0, b + t.bottom / 2), s.stroke()), t.left > 0 && (s.strokeStyle = e.left, s.lineWidth = t.left, s.beginPath(), s.moveTo(0 - t.left / 2, b + t.bottom), s.lineTo(0 - t.left / 2, 0), s.stroke())) : (s.lineWidth = t, s.strokeStyle = o.grid.borderColor, s.strokeRect(-t / 2, -t / 2, d + t, b + t))), s.restore()
        }

        function nr() {
            n.each(tt(), function(n, t) {
                var i = t.box,
                    l = t.direction + "Axis " + t.direction + t.n + "Axis",
                    c = "flot-" + t.direction + "-axis flot-" + t.direction + t.n + "-axis " + l,
                    v = t.options.font || "flot-tick-label tickLabel",
                    r, u, f, o, s, e;
                if (a.removeText(c), t.show && t.ticks.length != 0)
                    for (e = 0; e < t.ticks.length; ++e)(r = t.ticks[e], !r.label || r.v < t.min || r.v > t.max) || (t.direction == "x" ? (o = "center", u = h.left + t.p2c(r.v), t.position == "bottom" ? f = i.top + i.padding : (f = i.top + i.height - i.padding, s = "bottom")) : (s = "middle", f = h.top + t.p2c(r.v), t.position == "left" ? (u = i.left + i.width - i.padding, o = "right") : u = i.left + i.padding), a.addText(c, u, f, r.label, v, null, null, o, s))
            })
        }

        function gi(n) {
            n.lines.show && di(n), n.bars.show && wi(n), n.points.show && ki(n)
        }

        function di(n) {
            function u(n, t, i, r, u) {
                var l = n.points,
                    a = n.pointsize,
                    v = null,
                    y = null,
                    c;
                for (s.beginPath(), c = a; c < l.length; c += a) {
                    var f = l[c - a],
                        e = l[c - a + 1],
                        o = l[c],
                        h = l[c + 1];
                    if (f != null && o != null) {
                        if (e <= h && e < u.min) {
                            if (h < u.min) continue;
                            f = (u.min - e) / (h - e) * (o - f) + f, e = u.min
                        } else if (h <= e && h < u.min) {
                            if (e < u.min) continue;
                            o = (u.min - e) / (h - e) * (o - f) + f, h = u.min
                        }
                        if (e >= h && e > u.max) {
                            if (h > u.max) continue;
                            f = (u.max - e) / (h - e) * (o - f) + f, e = u.max
                        } else if (h >= e && h > u.max) {
                            if (e > u.max) continue;
                            o = (u.max - e) / (h - e) * (o - f) + f, h = u.max
                        }
                        if (f <= o && f < r.min) {
                            if (o < r.min) continue;
                            e = (r.min - f) / (o - f) * (h - e) + e, f = r.min
                        } else if (o <= f && o < r.min) {
                            if (f < r.min) continue;
                            h = (r.min - f) / (o - f) * (h - e) + e, o = r.min
                        }
                        if (f >= o && f > r.max) {
                            if (o > r.max) continue;
                            e = (r.max - f) / (o - f) * (h - e) + e, f = r.max
                        } else if (o >= f && o > r.max) {
                            if (f > r.max) continue;
                            h = (r.max - f) / (o - f) * (h - e) + e, o = r.max
                        }(f != v || e != y) && s.moveTo(r.p2c(f) + t, u.p2c(e) + i), v = o, y = h, s.lineTo(r.p2c(o) + t, u.p2c(h) + i)
                    }
                }
                s.stroke()
            }

            function e(n, t, i) {
                for (var c = n.points, o = n.pointsize, b = Math.min(Math.max(0, i.min), i.max), h = 0, k, l = !1, a = 1, w = 0, p = 0, v, y;;) {
                    if (o > 0 && h > c.length + o) break;
                    h += o;
                    var r = c[h - o],
                        u = c[h - o + a],
                        f = c[h],
                        e = c[h + a];
                    if (l) {
                        if (o > 0 && r != null && f == null) {
                            p = h, o = -o, a = 2;
                            continue
                        }
                        if (o < 0 && h == w + o) {
                            s.fill(), l = !1, o = -o, a = 1, h = w = p + o;
                            continue
                        }
                    }
                    if (r != null && f != null) {
                        if (r <= f && r < t.min) {
                            if (f < t.min) continue;
                            u = (t.min - r) / (f - r) * (e - u) + u, r = t.min
                        } else if (f <= r && f < t.min) {
                            if (r < t.min) continue;
                            e = (t.min - r) / (f - r) * (e - u) + u, f = t.min
                        }
                        if (r >= f && r > t.max) {
                            if (f > t.max) continue;
                            u = (t.max - r) / (f - r) * (e - u) + u, r = t.max
                        } else if (f >= r && f > t.max) {
                            if (r > t.max) continue;
                            e = (t.max - r) / (f - r) * (e - u) + u, f = t.max
                        }
                        if (l || (s.beginPath(), s.moveTo(t.p2c(r), i.p2c(b)), l = !0), u >= i.max && e >= i.max) {
                            s.lineTo(t.p2c(r), i.p2c(i.max)), s.lineTo(t.p2c(f), i.p2c(i.max));
                            continue
                        } else if (u <= i.min && e <= i.min) {
                            s.lineTo(t.p2c(r), i.p2c(i.min)), s.lineTo(t.p2c(f), i.p2c(i.min));
                            continue
                        }
                        v = r, y = f, u <= e && u < i.min && e >= i.min ? (r = (i.min - u) / (e - u) * (f - r) + r, u = i.min) : e <= u && e < i.min && u >= i.min && (f = (i.min - u) / (e - u) * (f - r) + r, e = i.min), u >= e && u > i.max && e <= i.max ? (r = (i.max - u) / (e - u) * (f - r) + r, u = i.max) : e >= u && e > i.max && u <= i.max && (f = (i.max - u) / (e - u) * (f - r) + r, e = i.max), r != v && s.lineTo(t.p2c(v), i.p2c(u)), s.lineTo(t.p2c(r), i.p2c(u)), s.lineTo(t.p2c(f), i.p2c(e)), f != y && (s.lineTo(t.p2c(f), i.p2c(e)), s.lineTo(t.p2c(y), i.p2c(e)))
                    }
                }
            }
            var t, i, r, f;
            s.save(), s.translate(h.left, h.top), s.lineJoin = "round", t = n.lines.lineWidth, i = n.shadowSize, t > 0 && i > 0 && (s.lineWidth = i, s.strokeStyle = "rgba(0,0,0,0.1)", r = Math.PI / 18, u(n.datapoints, Math.sin(r) * (t / 2 + i / 2), Math.cos(r) * (t / 2 + i / 2), n.xaxis, n.yaxis), s.lineWidth = i / 2, u(n.datapoints, Math.sin(r) * (t / 2 + i / 4), Math.cos(r) * (t / 2 + i / 4), n.xaxis, n.yaxis)), s.lineWidth = t, s.strokeStyle = n.color, f = ot(n.lines, n.color, 0, b), f && (s.fillStyle = f, e(n.datapoints, n.xaxis, n.yaxis)), t > 0 && u(n.datapoints, 0, 0, n.xaxis, n.yaxis), s.restore()
        }

        function ki(n) {
            function r(n, t, i, r, u, f, e, o) {
                for (var a = n.points, v = n.pointsize, h, c, l = 0; l < a.length; l += v)(h = a[l], c = a[l + 1], h == null || h < f.min || h > f.max || c < e.min || c > e.max) || (s.beginPath(), h = f.p2c(h), c = e.p2c(c) + r, o == "circle" ? s.arc(h, c, t, 0, u ? Math.PI : Math.PI * 2, !1) : o(s, h, c, t, u), s.closePath(), i && (s.fillStyle = i, s.fill()), s.stroke())
            }
            var t;
            s.save(), s.translate(h.left, h.top);
            var i = n.points.lineWidth,
                e = n.shadowSize,
                u = n.points.radius,
                f = n.points.symbol;
            i == 0 && (i = .0001), i > 0 && e > 0 && (t = e / 2, s.lineWidth = t, s.strokeStyle = "rgba(0,0,0,0.1)", r(n.datapoints, u, null, t + t / 2, !0, n.xaxis, n.yaxis, f), s.strokeStyle = "rgba(0,0,0,0.2)", r(n.datapoints, u, null, t / 2, !0, n.xaxis, n.yaxis, f)), s.lineWidth = i, s.strokeStyle = n.color, r(n.datapoints, u, ot(n.points, n.color), 0, !1, n.xaxis, n.yaxis, f), s.restore()
        }

        function lt(n, t, i, r, u, f, e, o, s, h, c) {
            var l, y, a, v, p, w, b, k, d;
            (h ? (k = w = b = !0, p = !1, l = i, y = n, v = t + r, a = t + u, y < l && (d = y, y = l, l = d, p = !0, w = !1)) : (p = w = b = !0, k = !1, l = n + r, y = n + u, a = i, v = t, v < a && (d = v, v = a, a = d, k = !0, b = !1)), y < e.min || l > e.max || v < o.min || a > o.max) || (l < e.min && (l = e.min, p = !1), y > e.max && (y = e.max, w = !1), a < o.min && (a = o.min, k = !1), v > o.max && (v = o.max, b = !1), l = e.p2c(l), a = o.p2c(a), y = e.p2c(y), v = o.p2c(v), f && (s.fillStyle = f(a, v), s.fillRect(l, v, y - l, a - v)), c > 0 && (p || w || b || k) && (s.beginPath(), s.moveTo(l, a), p ? s.lineTo(l, v) : s.moveTo(l, v), b ? s.lineTo(y, v) : s.moveTo(y, v), w ? s.lineTo(y, a) : s.moveTo(y, a), k ? s.lineTo(l, a) : s.moveTo(l, a), s.stroke()))
        }

        function wi(n) {
            function r(t, i, r, u, f, e) {
                for (var h = t.points, c = t.pointsize, o = 0; o < h.length; o += c) h[o] != null && lt(h[o], h[o + 1], h[o + 2], i, r, u, f, e, s, n.bars.horizontal, n.bars.lineWidth)
            }
            var t, i;
            s.save(), s.translate(h.left, h.top), s.lineWidth = n.bars.lineWidth, s.strokeStyle = n.color;
            switch (n.bars.align) {
                case "left":
                    t = 0;
                    break;
                case "right":
                    t = -n.bars.barWidth;
                    break;
                default:
                    t = -n.bars.barWidth / 2
            }
            i = n.bars.fill ? function(t, i) {
                return ot(n.bars, n.color, t, i)
            } : null, r(n.datapoints, t, t + n.bars.barWidth, i, n.xaxis, n.yaxis), s.restore()
        }

        function ot(t, i, r, u) {
            var e = t.fill,
                f;
            return e ? t.fillColor ? gt(t.fillColor, r, u, i) : (f = n.color.parse(i), f.a = typeof e == "number" ? e : .4, f.normalize(), f.toString()) : null
        }

        function pi() {
            var g, r, w, b, v, t, k;
            if (o.legend.container != null ? n(o.legend.container).html("") : i.find(".legend").remove(), o.legend.show) {
                var f = [],
                    e = [],
                    y = !1,
                    d = o.legend.labelFormatter,
                    s, p;
                for (r = 0; r < c.length; ++r) s = c[r], s.label && (p = d ? d(s.label, s) : s.label, p && e.push({
                    label: p,
                    color: s.color
                }));
                for (o.legend.sorted && (n.isFunction(o.legend.sorted) ? e.sort(o.legend.sorted) : o.legend.sorted == "reverse" ? e.reverse() : (g = o.legend.sorted != "descending", e.sort(function(n, t) {
                        return n.label == t.label ? 0 : n.label < t.label != g ? 1 : -1
                    }))), r = 0; r < e.length; ++r) w = e[r], r % o.legend.noColumns == 0 && (y && f.push("</tr>"), f.push("<tr>"), y = !0), f.push('<td class="legendColorBox"><div style="border:1px solid ' + o.legend.labelBoxBorderColor + ';padding:1px"><div style="width:4px;height:0;border:5px solid ' + w.color + ';overflow:hidden"></div></div></td><td class="legendLabel">' + w.label + "</td>");
                if (y && f.push("</tr>"), f.length != 0)
                    if (b = '<table style="font-size:smaller;color:' + o.grid.color + '">' + f.join("") + "</table>", o.legend.container != null) n(o.legend.container).html(b);
                    else {
                        var l = "",
                            a = o.legend.position,
                            u = o.legend.margin;
                        u[0] == null && (u = [u, u]), a.charAt(0) == "n" ? l += "top:" + (u[1] + h.top) + "px;" : a.charAt(0) == "s" && (l += "bottom:" + (u[1] + h.bottom) + "px;"), a.charAt(1) == "e" ? l += "right:" + (u[0] + h.right) + "px;" : a.charAt(1) == "w" && (l += "left:" + (u[0] + h.left) + "px;"), v = n('<div class="legend">' + b.replace('style="', 'style="position:absolute;' + l + ";") + "</div>").appendTo(i), o.legend.backgroundOpacity != 0 && (t = o.legend.backgroundColor, t == null && (t = o.grid.backgroundColor, t = t && typeof t == "string" ? n.color.parse(t) : n.color.extract(v, "background-color"), t.a = 1, t = t.toString()), k = v.children(), n('<div style="position:absolute;width:' + k.width() + "px;height:" + k.height() + "px;" + l + "background-color:" + t + ';"> </div>').prependTo(v).css("opacity", o.legend.backgroundOpacity))
                    }
            }
        }

        function vi(n, t, i) {
            for (var k = o.grid.mouseActiveRadius, it = k * k + 1, v = null, et = !1, r, h, e, s, a, tt, u = c.length - 1; u >= 0; --u)
                if (i(c[u])) {
                    var f = c[u],
                        b = f.xaxis,
                        w = f.yaxis,
                        l = f.datapoints.points,
                        p = b.c2p(n),
                        y = w.c2p(t),
                        g = k / b.scale,
                        nt = k / w.scale;
                    if (h = f.datapoints.pointsize, b.options.inverseTransform && (g = Number.MAX_VALUE), w.options.inverseTransform && (nt = Number.MAX_VALUE), f.lines.show || f.points.show)
                        for (r = 0; r < l.length; r += h)
                            if ((e = l[r], s = l[r + 1], e != null) && !(e - p > g) && !(e - p < -g) && !(s - y > nt) && !(s - y < -nt)) {
                                var rt = Math.abs(b.p2c(e) - n),
                                    ut = Math.abs(w.p2c(s) - t),
                                    ft = rt * rt + ut * ut;
                                ft < it && (it = ft, v = [u, r / h])
                            } if (f.bars.show && !v) {
                        switch (f.bars.align) {
                            case "left":
                                a = 0;
                                break;
                            case "right":
                                a = -f.bars.barWidth;
                                break;
                            default:
                                a = -f.bars.barWidth / 2
                        }
                        for (tt = a + f.bars.barWidth, r = 0; r < l.length; r += h) {
                            var e = l[r],
                                s = l[r + 1],
                                d = l[r + 2];
                            e != null && (c[u].bars.horizontal ? p <= Math.max(d, e) && p >= Math.min(d, e) && y >= s + a && y <= s + tt : p >= e + a && p <= e + tt && y >= Math.min(d, s) && y <= Math.max(d, s)) && (v = [u, r / h])
                        }
                    }
                } return v ? (u = v[0], r = v[1], h = c[u].datapoints.pointsize, {
                datapoint: c[u].datapoints.points.slice(r * h, (r + 1) * h),
                dataIndex: r,
                series: c[u],
                seriesIndex: u
            }) : null
        }

        function kt(n) {
            o.grid.hoverable && st("plothover", n, function(n) {
                return n.hoverable != !1
            })
        }

        function vt(n) {
            o.grid.hoverable && st("plothover", n, function() {
                return !1
            })
        }

        function at(n) {
            st("plotclick", n, function(n) {
                return n.clickable != !1
            })
        }

        function st(n, t, r) {
            var e = g.offset(),
                l = t.pageX - e.left - h.left,
                a = t.pageY - e.top - h.top,
                c = ti({
                    left: l,
                    top: a
                }),
                u, s, f;
            if (c.pageX = t.pageX, c.pageY = t.pageY, u = vi(l, a, r), u && (u.pageX = parseInt(u.series.xaxis.p2c(u.datapoint[0]) + e.left + h.left, 10), u.pageY = parseInt(u.series.yaxis.p2c(u.datapoint[1]) + e.top + h.top, 10)), o.grid.autoHighlight) {
                for (s = 0; s < k.length; ++s) f = k[s], f.auto != n || u && f.series == u.series && f.point[0] == u.datapoint[0] && f.point[1] == u.datapoint[1] || bt(f.series, f.point);
                u && wt(u.series, u.datapoint, n)
            }
            i.trigger(n, [c, u])
        }

        function ut() {
            var n = o.interaction.redrawOverlayInterval;
            if (n == -1) {
                pt();
                return
            }
            rt || (rt = setTimeout(pt, n))
        }

        function pt() {
            rt = null, v.save(), it.clear(), v.translate(h.left, h.top);
            for (var n, t = 0; t < k.length; ++t) n = k[t], n.series.bars.show ? bi(n.series, n.point) : fi(n.series, n.point);
            v.restore(), nt(p.drawOverlay, [v])
        }

        function wt(n, t, i) {
            var r, u;
            typeof n == "number" && (n = c[n]), typeof t == "number" && (r = n.datapoints.pointsize, t = n.datapoints.points.slice(r * t, r * (t + 1))), u = ht(n, t), u == -1 ? (k.push({
                series: n,
                point: t,
                auto: i
            }), ut()) : i || (k[u].auto = !1)
        }

        function bt(n, t) {
            var i, r;
            if (n == null && t == null) {
                k = [], ut();
                return
            }
            typeof n == "number" && (n = c[n]), typeof t == "number" && (i = n.datapoints.pointsize, t = n.datapoints.points.slice(i * t, i * (t + 1))), r = ht(n, t), r != -1 && (k.splice(r, 1), ut())
        }

        function ht(n, t) {
            for (var r, i = 0; i < k.length; ++i)
                if (r = k[i], r.series == n && r.point[0] == t[0] && r.point[1] == t[1]) return i;
            return -1
        }

        function fi(t, i) {
            var r = i[0],
                u = i[1],
                f = t.xaxis,
                e = t.yaxis,
                h = typeof t.highlightColor == "string" ? t.highlightColor : n.color.parse(t.color).scale("a", .5).toString(),
                o, s;
            r < f.min || r > f.max || u < e.min || u > e.max || (o = t.points.radius + t.points.lineWidth / 2, v.lineWidth = o, v.strokeStyle = h, s = 1.5 * o, r = f.p2c(r), u = e.p2c(u), v.beginPath(), t.points.symbol == "circle" ? v.arc(r, u, s, 0, 2 * Math.PI, !1) : t.points.symbol(v, r, u, s, !1), v.closePath(), v.stroke())
        }

        function bi(t, i) {
            var u = typeof t.highlightColor == "string" ? t.highlightColor : n.color.parse(t.color).scale("a", .5).toString(),
                f = u,
                r;
            switch (t.bars.align) {
                case "left":
                    r = 0;
                    break;
                case "right":
                    r = -t.bars.barWidth;
                    break;
                default:
                    r = -t.bars.barWidth / 2
            }
            v.lineWidth = t.bars.lineWidth, v.strokeStyle = u, lt(i[0], i[1], i[2] || 0, r, r + t.bars.barWidth, function() {
                return f
            }, t.xaxis, t.yaxis, v, t.bars.horizontal, t.bars.lineWidth)
        }

        function gt(t, i, r, u) {
            var h, e, c, f, o;
            if (typeof t == "string") return t;
            for (h = s.createLinearGradient(0, r, 0, i), e = 0, c = t.colors.length; e < c; ++e) f = t.colors[e], typeof f != "string" && (o = n.color.parse(u), f.brightness != null && (o = o.scale("rgb", f.brightness)), f.opacity != null && (o.a *= f.opacity), f = o.toString()), h.addColorStop(e / (c - 1), f);
            return h
        }
        var c = [],
            o = {
                colors: ["#edc240", "#afd8f8", "#cb4b4b", "#4da74d", "#9440ed"],
                legend: {
                    show: !0,
                    noColumns: 1,
                    labelFormatter: null,
                    labelBoxBorderColor: "#ccc",
                    container: null,
                    position: "ne",
                    margin: 5,
                    backgroundColor: null,
                    backgroundOpacity: .85,
                    sorted: null
                },
                xaxis: {
                    show: null,
                    position: "bottom",
                    mode: null,
                    font: null,
                    color: null,
                    tickColor: null,
                    transform: null,
                    inverseTransform: null,
                    min: null,
                    max: null,
                    autoscaleMargin: null,
                    ticks: null,
                    tickFormatter: null,
                    labelWidth: null,
                    labelHeight: null,
                    reserveSpace: null,
                    tickLength: null,
                    alignTicksWithAxis: null,
                    tickDecimals: null,
                    tickSize: null,
                    minTickSize: null
                },
                yaxis: {
                    autoscaleMargin: .02,
                    position: "left"
                },
                xaxes: [],
                yaxes: [],
                series: {
                    points: {
                        show: !1,
                        radius: 3,
                        lineWidth: 2,
                        fill: !0,
                        fillColor: "#ffffff",
                        symbol: "circle"
                    },
                    lines: {
                        lineWidth: 2,
                        fill: !1,
                        fillColor: null,
                        steps: !1
                    },
                    bars: {
                        show: !1,
                        lineWidth: 2,
                        barWidth: 1,
                        fill: !0,
                        fillColor: null,
                        align: "left",
                        horizontal: !1,
                        zero: !0
                    },
                    shadowSize: 3,
                    highlightColor: null
                },
                grid: {
                    show: !0,
                    aboveData: !1,
                    color: "#545454",
                    backgroundColor: null,
                    borderColor: null,
                    tickColor: null,
                    margin: 0,
                    labelMargin: 5,
                    axisMargin: 8,
                    borderWidth: 2,
                    minBorderMargin: null,
                    markings: null,
                    markingsColor: "#f4f4f4",
                    markingsLineWidth: 2,
                    clickable: !1,
                    hoverable: !1,
                    autoHighlight: !0,
                    mouseActiveRadius: 10
                },
                interaction: {
                    redrawOverlayInterval: 1e3 / 60
                },
                hooks: {}
            },
            a = null,
            it = null,
            g = null,
            s = null,
            v = null,
            y = [],
            w = [],
            h = {
                left: 0,
                right: 0,
                top: 0,
                bottom: 0
            },
            d = 0,
            b = 0,
            p = {
                processOptions: [],
                processRawData: [],
                processDatapoints: [],
                processOffset: [],
                drawBackground: [],
                drawSeries: [],
                draw: [],
                bindEvents: [],
                drawOverlay: [],
                shutdown: []
            },
            l = this,
            k, rt;
        l.setData = ni, l.setupGrid = ri, l.draw = dt, l.getPlaceholder = function() {
            return i
        }, l.getCanvas = function() {
            return a.element
        }, l.getPlotOffset = function() {
            return h
        }, l.width = function() {
            return d
        }, l.height = function() {
            return b
        }, l.offset = function() {
            var n = g.offset();
            return n.left += h.left, n.top += h.top, n
        }, l.getData = function() {
            return c
        }, l.getAxes = function() {
            var t = {},
                i;
            return n.each(y.concat(w), function(n, i) {
                i && (t[i.direction + (i.n != 1 ? i.n : "") + "axis"] = i)
            }), t
        }, l.getXAxes = function() {
            return y
        }, l.getYAxes = function() {
            return w
        }, l.c2p = ti, l.p2c = hi, l.getOptions = function() {
            return o
        }, l.highlight = wt, l.unhighlight = bt, l.triggerRedrawOverlay = ut, l.pointOffset = function(n) {
            return {
                left: parseInt(y[ft(n, "x") - 1].p2c(+n.x) + h.left, 10),
                top: parseInt(w[ft(n, "y") - 1].p2c(+n.y) + h.top, 10)
            }
        }, l.shutdown = ii, l.destroy = function() {
            ii(), i.removeData("plot").empty(), c = [], o = null, a = null, it = null, g = null, s = null, v = null, y = [], w = [], p = null, k = [], l = null
        }, l.resize = function() {
            var n = i.width(),
                t = i.height();
            a.resize(n, t), it.resize(n, t)
        }, l.hooks = p, ei(l), oi(f), ai(), ni(r), ri(), dt(), yi(), k = [], rt = null
    }

    function u(n, t) {
        return t * Math.floor(n / t)
    }
    var i = Object.prototype.hasOwnProperty;
    t.prototype.resize = function(n, t) {
        if (n <= 0 || t <= 0) throw new Error("Invalid dimensions for plot, width = " + n + ", height = " + t);
        var i = this.element,
            u = this.context,
            r = this.pixelRatio;
        this.width != n && (i.width = n * r, i.style.width = n + "px", this.width = n), this.height != t && (i.height = t * r, i.style.height = t + "px", this.height = t), u.restore(), u.save(), u.scale(r, r)
    }, t.prototype.clear = function() {
        this.context.clearRect(0, 0, this.width, this.height)
    }, t.prototype.render = function() {
        var h = this._textCache,
            r, u, f, c, t, e, o, s, n;
        for (r in h)
            if (i.call(h, r)) {
                u = this.getTextLayer(r), f = h[r], u.hide();
                for (c in f)
                    if (i.call(f, c)) {
                        t = f[c];
                        for (e in t)
                            if (i.call(t, e)) {
                                for (o = t[e].positions, s = 0; n = o[s]; s++) n.active ? n.rendered || (u.append(n.element), n.rendered = !0) : (o.splice(s--, 1), n.rendered && n.element.detach());
                                o.length == 0 && delete t[e]
                            }
                    } u.show()
            }
    }, t.prototype.getTextLayer = function(t) {
        var i = this.text[t];
        return i == null && (this.textContainer == null && (this.textContainer = n("<div class='flot-text'></div>").css({
            position: "absolute",
            top: 0,
            left: 0,
            bottom: 0,
            right: 0,
            "font-size": "smaller",
            color: "#545454"
        }).insertAfter(this.element)), i = this.text[t] = n("<div></div>").addClass(t).css({
            position: "absolute",
            top: 0,
            left: 0,
            bottom: 0,
            right: 0
        }).appendTo(this.textContainer)), i
    }, t.prototype.getTextInfo = function(t, i, r, u, f) {
        var o, s, h, c, e;
        return i = "" + i, o = typeof r == "object" ? r.style + " " + r.variant + " " + r.weight + " " + r.size + "px/" + r.lineHeight + "px " + r.family : r, s = this._textCache[t], s == null && (s = this._textCache[t] = {}), h = s[o], h == null && (h = s[o] = {}), c = h[i], c == null && (e = n("<div></div>").html(i).css({
            position: "absolute",
            "max-width": f,
            top: -9999
        }).appendTo(this.getTextLayer(t)), typeof r == "object" ? e.css({
            font: o,
            color: r.color
        }) : typeof r == "string" && e.addClass(r), c = h[i] = {
            width: e.outerWidth(!0),
            height: e.outerHeight(!0),
            element: e,
            positions: []
        }, e.detach()), c
    }, t.prototype.addText = function(n, t, i, r, u, f, e, o, s) {
        var h = this.getTextInfo(n, r, u, f, e),
            l = h.positions,
            a, c;
        for (o == "center" ? t -= h.width / 2 : o == "right" && (t -= h.width), s == "middle" ? i -= h.height / 2 : s == "bottom" && (i -= h.height), a = 0; c = l[a]; a++)
            if (c.x == t && c.y == i) {
                c.active = !0;
                return
            } c = {
            active: !0,
            rendered: !1,
            element: l.length ? h.element.clone() : h.element,
            x: t,
            y: i
        }, l.push(c), c.element.css({
            top: Math.round(i),
            left: Math.round(t),
            "text-align": o
        })
    }, t.prototype.removeText = function(n, t, r, u, f, e) {
        var h, a, c, v, l, o, s;
        if (u == null) {
            if (h = this._textCache[n], h != null)
                for (a in h)
                    if (i.call(h, a)) {
                        c = h[a];
                        for (v in c)
                            if (i.call(c, v))
                                for (l = c[v].positions, o = 0; s = l[o]; o++) s.active = !1
                    }
        } else
            for (l = this.getTextInfo(n, u, f, e).positions, o = 0; s = l[o]; o++) s.x == t && s.y == r && (s.active = !1)
    }, n.plot = function(t, i, u) {
        return new r(n(t), i, u, n.plot.plugins)
    }, n.plot.version = "0.8.2", n.plot.plugins = [], n.fn.plot = function(t, i) {
        return this.each(function() {
            n.plot(this, t, i)
        })
    }
}(jQuery);