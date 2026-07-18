// flot/jquery.flot.time.js
(function(n) {
    function i(n, t) {
        return t * Math.floor(n / t)
    }

    function r(n, t, i, r) {
        var o, s, u;
        if (typeof n.strftime == "function") return n.strftime(t);
        var f = function(n, t) {
                return n = "" + n, t = "" + (t == null ? "0" : t), n.length == 1 ? t + n : n
            },
            h = [],
            c = !1,
            e = n.getHours(),
            l = e < 12;
        for (i == null && (i = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"]), r == null && (r = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"]), o = e > 12 ? e - 12 : e == 0 ? 12 : e, s = 0; s < t.length; ++s)
            if (u = t.charAt(s), c) {
                switch (u) {
                    case "a":
                        u = "" + r[n.getDay()];
                        break;
                    case "b":
                        u = "" + i[n.getMonth()];
                        break;
                    case "d":
                        u = f(n.getDate());
                        break;
                    case "e":
                        u = f(n.getDate(), " ");
                        break;
                    case "h":
                    case "H":
                        u = f(e);
                        break;
                    case "I":
                        u = f(o);
                        break;
                    case "l":
                        u = f(o, " ");
                        break;
                    case "m":
                        u = f(n.getMonth() + 1);
                        break;
                    case "M":
                        u = f(n.getMinutes());
                        break;
                    case "q":
                        u = "" + (Math.floor(n.getMonth() / 3) + 1);
                        break;
                    case "S":
                        u = f(n.getSeconds());
                        break;
                    case "y":
                        u = f(n.getFullYear() % 100);
                        break;
                    case "Y":
                        u = "" + n.getFullYear();
                        break;
                    case "p":
                        u = l ? "am" : "pm";
                        break;
                    case "P":
                        u = l ? "AM" : "PM";
                        break;
                    case "w":
                        u = "" + n.getDay()
                }
                h.push(u), c = !1
            } else u == "%" ? c = !0 : h.push(u);
        return h.join("")
    }

    function u(n) {
        function u(n, t, i, r) {
            n[t] = function() {
                return i[r].apply(i, arguments)
            }
        }
        var i = {
                date: n
            },
            r, t;
        for (n.strftime != undefined && u(i, "strftime", n, "strftime"), u(i, "getTime", n, "getTime"), u(i, "setTime", n, "setTime"), r = ["Date", "Day", "FullYear", "Hours", "Milliseconds", "Minutes", "Month", "Seconds"], t = 0; t < r.length; t++) u(i, "get" + r[t], n, "getUTC" + r[t]), u(i, "set" + r[t], n, "setUTC" + r[t]);
        return i
    }

    function f(n, t) {
        if (t.timezone == "browser") return new Date(n);
        if (t.timezone && t.timezone != "utc") {
            if (typeof timezoneJS != "undefined" && typeof timezoneJS.Date != "undefined") {
                var i = new timezoneJS.Date;
                return i.setTimezone(t.timezone), i.setTime(n), i
            }
            return u(new Date(n))
        }
        return u(new Date(n))
    }

    function c(u) {
        u.hooks.processOptions.push(function(u) {
            n.each(u.getAxes(), function(n, u) {
                var e = u.options;
                e.mode == "time" && (u.tickGenerator = function(n) {
                    var d = [],
                        r = f(n.min, e),
                        k = 0,
                        a = e.tickSize && e.tickSize[1] === "quarter" || e.minTickSize && e.minTickSize[1] === "quarter" ? h : s,
                        c, l, u, b, p, o, v, w, y, g, nt, tt;
                    for (e.minTickSize != null && (k = typeof e.tickSize == "number" ? e.tickSize : e.minTickSize[0] * t[e.minTickSize[1]]), c = 0; c < a.length - 1; ++c)
                        if (n.delta < (a[c][0] * t[a[c][1]] + a[c + 1][0] * t[a[c + 1][1]]) / 2 && a[c][0] * t[a[c][1]] >= k) break;
                    l = a[c][0], u = a[c][1], u == "year" && (e.minTickSize != null && e.minTickSize[1] == "year" ? l = Math.floor(e.minTickSize[0]) : (b = Math.pow(10, Math.floor(Math.log(n.delta / t.year) / Math.LN10)), p = n.delta / t.year / b, l = p < 1.5 ? 1 : p < 3 ? 2 : p < 7.5 ? 5 : 10, l *= b), l < 1 && (l = 1)), n.tickSize = e.tickSize || [l, u], o = n.tickSize[0], u = n.tickSize[1], v = o * t[u], u == "second" ? r.setSeconds(i(r.getSeconds(), o)) : u == "minute" ? r.setMinutes(i(r.getMinutes(), o)) : u == "hour" ? r.setHours(i(r.getHours(), o)) : u == "month" ? r.setMonth(i(r.getMonth(), o)) : u == "quarter" ? r.setMonth(3 * i(r.getMonth() / 3, o)) : u == "year" && r.setFullYear(i(r.getFullYear(), o)), r.setMilliseconds(0), v >= t.minute && r.setSeconds(0), v >= t.hour && r.setMinutes(0), v >= t.day && r.setHours(0), v >= t.day * 4 && r.setDate(1), v >= t.month * 2 && r.setMonth(i(r.getMonth(), 3)), v >= t.quarter * 2 && r.setMonth(i(r.getMonth(), 6)), v >= t.year && r.setMonth(0), w = 0, y = Number.NaN;
                    do g = y, y = r.getTime(), d.push(y), u == "month" || u == "quarter" ? o < 1 ? (r.setDate(1), nt = r.getTime(), r.setMonth(r.getMonth() + (u == "quarter" ? 3 : 1)), tt = r.getTime(), r.setTime(y + w * t.hour + (tt - nt) * o), w = r.getHours(), r.setHours(0)) : r.setMonth(r.getMonth() + o * (u == "quarter" ? 3 : 1)) : u == "year" ? r.setFullYear(r.getFullYear() + o) : r.setTime(y + v); while (y < n.max && y != g);
                    return d
                }, u.tickFormatter = function(n, i) {
                    var a = f(n, i.options),
                        v;
                    if (e.timeformat != null) return r(a, e.timeformat, e.monthNames, e.dayNames);
                    var s = i.options.tickSize && i.options.tickSize[1] == "quarter" || i.options.minTickSize && i.options.minTickSize[1] == "quarter",
                        o = i.tickSize[0] * t[i.tickSize[1]],
                        h = i.max - i.min,
                        c = e.twelveHourClock ? " %p" : "",
                        l = e.twelveHourClock ? "%I" : "%H",
                        u;
                    return u = o < t.minute ? l + ":%M:%S" + c : o < t.day ? h < 2 * t.day ? l + ":%M" + c : "%b %d " + l + ":%M" + c : o < t.month ? "%b %d" : s && o < t.quarter || !s && o < t.year ? h < t.year ? "%b" : "%b %Y" : s && o < t.year ? h < t.year ? "Q%q" : "Q%q %Y" : "%Y", v = r(a, u, e.monthNames, e.dayNames)
                })
            })
        })
    }
    var o = {
            xaxis: {
                timezone: null,
                timeformat: null,
                twelveHourClock: !1,
                monthNames: null
            }
        },
        t = {
            second: 1e3,
            minute: 6e4,
            hour: 36e5,
            day: 864e5,
            month: 2592e6,
            quarter: 7776e6,
            year: 365.2425 * 864e5
        },
        e = [
            [1, "second"],
            [2, "second"],
            [5, "second"],
            [10, "second"],
            [30, "second"],
            [1, "minute"],
            [2, "minute"],
            [5, "minute"],
            [10, "minute"],
            [30, "minute"],
            [1, "hour"],
            [2, "hour"],
            [4, "hour"],
            [8, "hour"],
            [12, "hour"],
            [1, "day"],
            [2, "day"],
            [3, "day"],
            [.25, "month"],
            [.5, "month"],
            [1, "month"],
            [2, "month"]
        ],
        s = e.concat([
            [3, "month"],
            [6, "month"],
            [1, "year"]
        ]),
        h = e.concat([
            [1, "quarter"],
            [2, "quarter"],
            [1, "year"]
        ]);
    n.plot.plugins.push({
        init: c,
        options: o,
        name: "time",
        version: "1.0"
    }), n.plot.formatDate = r
})(jQuery);