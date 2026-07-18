// HighCharts/modules/data.js
(function(n) {
    typeof module == "object" && module.exports ? module.exports = n : n(Highcharts)
})(function(n) {
    var f = n.win.document,
        t = n.each,
        e = n.pick,
        u = n.inArray,
        o = n.splat,
        i, r = function(n, t) {
            this.init(n, t)
        };
    n.extend(r.prototype, {
        init: function(n, t) {
            this.options = n, this.chartOptions = t, this.columns = n.columns || this.rowsToColumns(n.rows) || [], this.firstRowAsNames = e(n.firstRowAsNames, !0), this.decimalRegex = n.decimalPoint && RegExp("^(-?[0-9]+)" + n.decimalPoint + "([0-9]+)$"), this.rawColumns = [], this.columns.length ? this.dataFound() : (this.parseCSV(), this.parseTable(), this.parseGoogleSpreadsheet())
        },
        getColumnDistribution: function() {
            var u = this.chartOptions,
                r = this.options,
                o = [],
                s = function(t) {
                    return (n.seriesTypes[t || "line"].prototype.pointArrayMap || [0]).length
                },
                f = u && u.chart && u.chart.type,
                h = [],
                l = [],
                c = 0,
                e;
            t(u && u.series || [], function(n) {
                h.push(s(n.type || f))
            }), t(r && r.seriesMapping || [], function(n) {
                o.push(n.x || 0)
            }), o.length === 0 && o.push(0), t(r && r.seriesMapping || [], function(t) {
                var r = new i,
                    o, v = h[c] || s(f),
                    a = n.seriesTypes[((u && u.series || [])[c] || {}).type || f || "line"].prototype.pointArrayMap || ["y"];
                r.addColumnReader(t.x, "x");
                for (o in t) t.hasOwnProperty(o) && o !== "x" && r.addColumnReader(t[o], o);
                for (e = 0; e < v; e++) r.hasReader(a[e]) || r.addColumnReader(void 0, a[e]);
                l.push(r), c++
            }), r = n.seriesTypes[f || "line"].prototype.pointArrayMap, r === void 0 && (r = ["y"]), this.valueCount = {
                global: s(f),
                xColumns: o,
                individual: h,
                seriesBuilders: l,
                globalPointArrayMap: r
            }
        },
        dataFound: function() {
            this.options.switchRowsAndColumns && (this.columns = this.rowsToColumns(this.columns)), this.getColumnDistribution(), this.parseTypes(), this.parsed() !== !1 && this.complete()
        },
        parseCSV: function() {
            var s = this,
                n = this.options,
                r = n.csv,
                u = this.columns,
                h = n.startRow || 0,
                c = n.endRow || Number.MAX_VALUE,
                i = n.startColumn || 0,
                l = n.endColumn || Number.MAX_VALUE,
                f, e, o = 0;
            r && (e = r.replace(/\r\n/g, "\n").replace(/\r/g, "\n").split(n.lineDelimiter || "\n"), f = n.itemDelimiter || (r.indexOf("\t") !== -1 ? "\t" : ","), t(e, function(n, r) {
                var e = s.trim(n),
                    a = e.indexOf("#") === 0;
                r >= h && r <= c && !a && e !== "" && (e = n.split(f), t(e, function(n, t) {
                    t >= i && t <= l && (u[t - i] || (u[t - i] = []), u[t - i][o] = n)
                }), o += 1)
            }), this.dataFound())
        },
        parseTable: function() {
            var n = this.options,
                i = n.table,
                u = this.columns,
                e = n.startRow || 0,
                o = n.endRow || Number.MAX_VALUE,
                r = n.startColumn || 0,
                s = n.endColumn || Number.MAX_VALUE;
            i && (typeof i == "string" && (i = f.getElementById(i)), t(i.getElementsByTagName("tr"), function(n, i) {
                i >= e && i <= o && t(n.children, function(n, t) {
                    (n.tagName === "TD" || n.tagName === "TH") && t >= r && t <= s && (u[t - r] || (u[t - r] = []), u[t - r][i - e] = n.innerHTML)
                })
            }), this.dataFound())
        },
        parseGoogleSpreadsheet: function() {
            var h = this,
                n = this.options,
                e = n.googleSpreadsheetKey,
                u = this.columns,
                f = n.startRow || 0,
                o = n.endRow || Number.MAX_VALUE,
                t = n.startColumn || 0,
                s = n.endColumn || Number.MAX_VALUE,
                i, r;
            e && jQuery.ajax({
                dataType: "json",
                url: "https://web.archive.org/web/20170830160504/https://spreadsheets.google.com/feeds/cells/" + e + "/" + (n.googleSpreadsheetWorksheet || "od6") + "/public/values?alt=json-in-script&callback=?",
                error: n.error,
                success: function(n) {
                    for (var n = n.feed.entry, c, v = n.length, l = 0, a = 0, e = 0; e < v; e++) c = n[e], l = Math.max(l, c.gs$cell.col), a = Math.max(a, c.gs$cell.row);
                    for (e = 0; e < l; e++) e >= t && e <= s && (u[e - t] = [], u[e - t].length = Math.min(a, o - f));
                    for (e = 0; e < v; e++)(c = n[e], i = c.gs$cell.row - 1, r = c.gs$cell.col - 1, r >= t && r <= s && i >= f && i <= o) && (u[r - t][i - f] = c.content.$t);
                    h.dataFound()
                }
            })
        },
        trim: function(n, t) {
            return typeof n == "string" && (n = n.replace(/^\s+|\s+$/g, ""), t && /^[0-9\s]+$/.test(n) && (n = n.replace(/\s/g, "")), this.decimalRegex && (n = n.replace(this.decimalRegex, "$1.$2"))), n
        },
        parseTypes: function() {
            for (var t = this.columns, n = t.length; n--;) this.parseColumn(t[n], n)
        },
        parseColumn: function(n, t) {
            var e = this.rawColumns,
                s = this.columns,
                i = n.length,
                f, r, h, v, p = this.firstRowAsNames,
                l = u(t, this.valueCount.xColumns) !== -1,
                w = [],
                a = this.chartOptions,
                c, y = (this.options.columnTypes || [])[t],
                a = l && (a && a.xAxis && o(a.xAxis)[0].type === "category" || y === "string");
            for (e[t] || (e[t] = []); i--;)(f = w[i] || n[i], h = this.trim(f), v = this.trim(f, !0), r = parseFloat(v), e[t][i] === void 0 && (e[t][i] = h), a || i === 0 && p) ? n[i] = h : +v === r ? (n[i] = r, r > 31536e6 && y !== "float" ? n.isDatetime = !0 : n.isNumeric = !0, n[i + 1] !== void 0 && (c = r > n[i + 1])) : (r = this.parseDate(f), l && typeof r == "number" && !isNaN(r) && y !== "float") ? (w[i] = f, n[i] = r, n.isDatetime = !0, n[i + 1] !== void 0) && (f = r > n[i + 1], f !== c && c !== void 0 && (this.alternativeFormat ? (this.dateFormat = this.alternativeFormat, i = n.length, this.alternativeFormat = this.dateFormats[this.dateFormat].alternative) : n.unsorted = !0), c = f) : (n[i] = h === "" ? null : h, i !== 0 && (n.isDatetime || n.isNumeric)) && (n.mixed = !0);
            if (l && n.mixed && (s[t] = e[t]), l && c && this.options.sort)
                for (t = 0; t < s.length; t++) s[t].reverse(), p && s[t].unshift(s[t].pop())
        },
        dateFormats: {
            "YYYY-mm-dd": {
                regex: /^([0-9]{4})[\-\/\.]([0-9]{2})[\-\/\.]([0-9]{2})$/,
                parser: function(n) {
                    return Date.UTC(+n[1], n[2] - 1, +n[3])
                }
            },
            "dd/mm/YYYY": {
                regex: /^([0-9]{1,2})[\-\/\.]([0-9]{1,2})[\-\/\.]([0-9]{4})$/,
                parser: function(n) {
                    return Date.UTC(+n[3], n[2] - 1, +n[1])
                },
                alternative: "mm/dd/YYYY"
            },
            "mm/dd/YYYY": {
                regex: /^([0-9]{1,2})[\-\/\.]([0-9]{1,2})[\-\/\.]([0-9]{4})$/,
                parser: function(n) {
                    return Date.UTC(+n[3], n[1] - 1, +n[2])
                }
            },
            "dd/mm/YY": {
                regex: /^([0-9]{1,2})[\-\/\.]([0-9]{1,2})[\-\/\.]([0-9]{2})$/,
                parser: function(n) {
                    return Date.UTC(+n[3] + 2e3, n[2] - 1, +n[1])
                },
                alternative: "mm/dd/YY"
            },
            "mm/dd/YY": {
                regex: /^([0-9]{1,2})[\-\/\.]([0-9]{1,2})[\-\/\.]([0-9]{2})$/,
                parser: function(n) {
                    return Date.UTC(+n[3] + 2e3, n[1] - 1, +n[2])
                }
            }
        },
        parseDate: function(n) {
            var i = this.options.parseDate,
                r, u, f = this.options.dateFormat || this.dateFormat,
                t;
            if (i) r = i(n);
            else if (typeof n == "string") {
                if (f) i = this.dateFormats[f], (t = n.match(i.regex)) && (r = i.parser(t));
                else
                    for (u in this.dateFormats)
                        if (i = this.dateFormats[u], t = n.match(i.regex)) {
                            this.dateFormat = u, this.alternativeFormat = i.alternative, r = i.parser(t);
                            break
                        } t || (t = Date.parse(n), typeof t == "object" && t !== null && t.getTime ? r = t.getTime() - t.getTimezoneOffset() * 6e4 : typeof t == "number" && !isNaN(t) && (r = t - new Date(t).getTimezoneOffset() * 6e4))
            }
            return r
        },
        rowsToColumns: function(n) {
            var i, u, t, f, r;
            if (n)
                for (r = [], u = n.length, i = 0; i < u; i++)
                    for (f = n[i].length, t = 0; t < f; t++) r[t] || (r[t] = []), r[t][i] = n[i][t];
            return r
        },
        parsed: function() {
            if (this.options.parsed) return this.options.parsed.call(this, this.columns)
        },
        getFreeIndexes: function(n, t) {
            for (var r, u = [], e = [], f, i = 0; i < n; i += 1) u.push(!0);
            for (r = 0; r < t.length; r += 1)
                for (f = t[r].getReferencedColumnIndexes(), i = 0; i < f.length; i += 1) u[f[i]] = !1;
            for (i = 0; i < u.length; i += 1) u[i] && e.push(i);
            return e
        },
        complete: function() {
            var f = this.columns,
                o, s = this.options,
                h, r, n, c, e = [],
                t;
            if (s.complete || s.afterComplete) {
                for (n = 0; n < f.length; n++) this.firstRowAsNames && (f[n].name = f[n].shift());
                for (h = [], r = this.getFreeIndexes(f.length, this.valueCount.seriesBuilders), n = 0; n < this.valueCount.seriesBuilders.length; n++) t = this.valueCount.seriesBuilders[n], t.populateColumns(r) && e.push(t);
                for (; r.length > 0;) {
                    for (t = new i, t.addColumnReader(0, "x"), n = u(0, r), n !== -1 && r.splice(n, 1), n = 0; n < this.valueCount.global; n++) t.addColumnReader(void 0, this.valueCount.globalPointArrayMap[n]);
                    t.populateColumns(r) && e.push(t)
                }
                if (e.length > 0 && e[0].readers.length > 0 && (t = f[e[0].readers[0].columnIndex], t !== void 0 && (t.isDatetime ? o = "datetime" : t.isNumeric || (o = "category"))), o === "category")
                    for (n = 0; n < e.length; n++)
                        for (t = e[n], r = 0; r < t.readers.length; r++) t.readers[r].configName === "x" && (t.readers[r].configName = "name");
                for (n = 0; n < e.length; n++) {
                    for (t = e[n], r = [], c = 0; c < f[0].length; c++) r[c] = t.read(f, c);
                    h[n] = {
                        data: r
                    }, t.name && (h[n].name = t.name), o === "category" && (h[n].turboThreshold = 0)
                }
                f = {
                    series: h
                }, o && (f.xAxis = {
                    type: o
                }), s.complete && s.complete(f), s.afterComplete && s.afterComplete(f)
            }
        }
    }), n.Data = r, n.data = function(n, t) {
        return new r(n, t)
    }, n.wrap(n.Chart.prototype, "init", function(t, i, r) {
        var u = this;
        i && i.data ? n.data(n.extend(i.data, {
            afterComplete: function(f) {
                var e, o;
                if (i.hasOwnProperty("series"))
                    if (typeof i.series == "object")
                        for (e = Math.max(i.series.length, f.series.length); e--;) o = i.series[e] || {}, i.series[e] = n.merge(o, f.series[e]);
                    else delete i.series;
                i = n.merge(f, i), t.call(u, i, r)
            }
        }), i) : t.call(u, i, r)
    }), i = function() {
        this.readers = [], this.pointIsArray = !0
    }, i.prototype.populateColumns = function(n) {
        var i = !0;
        return t(this.readers, function(t) {
            t.columnIndex === void 0 && (t.columnIndex = n.shift())
        }), t(this.readers, function(n) {
            n.columnIndex === void 0 && (i = !1)
        }), i
    }, i.prototype.read = function(n, i) {
        var f = this.pointIsArray,
            u = f ? [] : {},
            r;
        return t(this.readers, function(t) {
            var r = n[t.columnIndex][i];
            f ? u.push(r) : u[t.configName] = r
        }), this.name === void 0 && this.readers.length >= 2 && (r = this.getReferencedColumnIndexes(), r.length >= 2) && (r.shift(), r.sort(), this.name = n[r.shift()].name), u
    }, i.prototype.addColumnReader = function(n, t) {
        this.readers.push({
            columnIndex: n,
            configName: t
        }), t === "x" || t === "y" || t === void 0 || (this.pointIsArray = !1)
    }, i.prototype.getReferencedColumnIndexes = function() {
        for (var i = [], t, n = 0; n < this.readers.length; n += 1) t = this.readers[n], t.columnIndex !== void 0 && i.push(t.columnIndex);
        return i
    }, i.prototype.hasReader = function(n) {
        for (var i, t = 0; t < this.readers.length; t += 1)
            if (i = this.readers[t], i.configName === n) return !0
    }
});