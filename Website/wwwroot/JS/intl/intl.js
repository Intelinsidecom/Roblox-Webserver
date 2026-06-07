// intl/intl.js
(function(n) {
    function t(r) {
        if (i[r]) return i[r].exports;
        var u = i[r] = {
            i: r,
            l: !1,
            exports: {}
        };
        return n[r].call(u.exports, u, u.exports, t), u.l = !0, u.exports
    }
    var i = {};
    return t.m = n, t.c = i, t.d = function(n, i, r) {
        t.o(n, i) || Object.defineProperty(n, i, {
            configurable: !1,
            enumerable: !0,
            get: r
        })
    }, t.n = function(n) {
        var i = n && n.__esModule ? function() {
            return n["default"]
        } : function() {
            return n
        };
        return t.d(i, "a", i), i
    }, t.o = function(n, t) {
        return Object.prototype.hasOwnProperty.call(n, t)
    }, t.p = "", t(t.s = 1)
})([function(n, t, i) {
    "use strict";

    function u(n) {
        for (var f = Array.prototype.slice.call(arguments, 1), t, u, i = 0, e = f.length; i < e; i += 1)
            if (t = f[i], t)
                for (u in t) r.call(t, u) && (n[u] = t[u]);
        return n
    }
    i.d(t, "b", function() {
        return r
    }), i.d(t, "a", function() {
        return u
    });
    var r = Object.prototype.hasOwnProperty
}, function(n, t, i) {
    "use strict";
    Object.defineProperty(t, "__esModule", {
        value: !0
    });
    var r = i(2);
    var u = i(7);
    r.a.__addLocaleData(u.a), r.a.defaultLocale = "en", window.Roblox = window.Roblox || {},
        function(n) {
            var t = function(n, t, i) {
                var u = "RobloxLocaleCode",
                    r;
                if (!n && (r = document.querySelector('meta[name="locale-data"]'), r !== null && r.dataset && r.dataset.languageCode && (n = r.dataset.languageCode), localStorage && localStorage.getItem && !n && (n = localStorage.getItem(u)), !n)) throw new Error("Unable to initialize intl - localeCode not provided and could not load from meta tags or local storage");
                this.locale = n, this.timeZone = t || "America/Los_Angeles", this.currency = i || "USD", this.monthsList = {}, this.weekdaysList = {}, localStorage && localStorage.setItem && localStorage.setItem(u, this.locale)
            };
            t.prototype.getLocale = function() {
                return this.locale
            }, t.prototype.getTimeZone = function() {
                return this.timeZone
            }, t.prototype.getCurrency = function() {
                return this.currency
            }, t.prototype.f = function(n, t, i) {
                if (typeof n != "string") throw new TypeError("'message' must be a string");
                var u = new r.a(n, this.locale, i);
                return u.format(t)
            }, t.prototype.d = function(n, t) {
                var i, r = {
                    short: {
                        year: "numeric",
                        month: "2-digit",
                        day: "2-digit"
                    },
                    full: {
                        year: "numeric",
                        month: "2-digit",
                        day: "2-digit",
                        hour: "2-digit",
                        minute: "2-digit"
                    },
                    time: {
                        hour: "2-digit",
                        minute: "2-digit"
                    }
                };
                if (typeof t == "string" || t === undefined) i = r[t] || r.short;
                else if (typeof t == "object") i = t;
                else throw new TypeError("'options' must be either of type string or object based on Intl.DateTimeFormat");
                return Intl.DateTimeFormat.call(null, this.locale, i).format(n)
            }, t.prototype.n = function(n, t) {
                if (isNaN(n)) throw new TypeError("The argument 'number' must be of type number");
                var i, r = {
                    currency: {
                        style: "currency",
                        currency: this.currency
                    },
                    percent: {
                        style: "percent",
                        maximumFractionDigits: 2
                    },
                    decimal: {
                        style: "decimal",
                        maximumFractionDigits: 2
                    }
                };
                if (typeof t == "string" || t === undefined) i = r[t] || r.decimal;
                else if (typeof t == "object") i = t;
                else throw new TypeError("'options' must be of type string or object based on Intl.NumberFormat");
                return Intl.NumberFormat.call(null, this.locale, i).format(n)
            }, t.prototype.getMonthsList = function(n) {
                var r = ["numeric", "2-digit", "narrow", "short", "long"],
                    t = r.indexOf(n) > -1 ? n : "short",
                    u = 2017,
                    f = this,
                    i;
                return this.monthsList[t] && this.monthsList[t].length > 0 ? this.monthsList[t] : (i = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12].map(function(n) {
                    return new Date(u, n - 1)
                }), this.monthsList[t] = i.map(function(n, i) {
                    return {
                        value: i + 1,
                        name: Intl.DateTimeFormat(f.locale, {
                            month: t
                        }).format(n)
                    }
                }))
            }, t.prototype.getWeekdaysList = function(n) {
                var r = ["narrow", "short", "long"],
                    t = r.indexOf(n) > -1 ? n : "short",
                    u = 2017,
                    f = 4,
                    e = this,
                    i;
                return this.weekdaysList[t] && this.weekdaysList[t].length > 0 ? this.weekdaysList[t] : (i = [1, 2, 3, 4, 5, 6, 7].map(function(n) {
                    return new Date(u, f, n)
                }), this.weekdaysList[t] = i.map(function(n, i) {
                    return {
                        value: i + 1,
                        name: Intl.DateTimeFormat(e.locale, {
                            weekday: t
                        }).format(n)
                    }
                }))
            }, n.Intl = t
        }(window.Roblox)
}, function(n, t, i) {
    "use strict";

    function r(n, t, i) {
        var f = typeof n == "string" ? r.__parse(n) : n;
        if (!(f && f.type === "messageFormatPattern")) throw new TypeError("A message must be provided as a String or AST.");
        i = this._mergeFormats(r.formats, i), u.a(this, "_locale", {
            value: this._resolveLocale(t)
        });
        var e = this._findPluralRuleFunction(this._locale),
            o = this._compilePattern(f, t, i, e),
            s = this;
        this.format = function(n) {
            return s._format(o, n)
        }
    }
    var f = i(0),
        u = i(3),
        e = i(4),
        o = i(5),
        s = i.n(o);
    t.a = r, u.a(r, "formats", {
        enumerable: !0,
        value: {
            number: {
                currency: {
                    style: "currency"
                },
                percent: {
                    style: "percent"
                }
            },
            date: {
                short: {
                    month: "numeric",
                    day: "numeric",
                    year: "2-digit"
                },
                medium: {
                    month: "short",
                    day: "numeric",
                    year: "numeric"
                },
                long: {
                    month: "long",
                    day: "numeric",
                    year: "numeric"
                },
                full: {
                    weekday: "long",
                    month: "long",
                    day: "numeric",
                    year: "numeric"
                }
            },
            time: {
                short: {
                    hour: "numeric",
                    minute: "numeric"
                },
                medium: {
                    hour: "numeric",
                    minute: "numeric",
                    second: "numeric"
                },
                long: {
                    hour: "numeric",
                    minute: "numeric",
                    second: "numeric",
                    timeZoneName: "short"
                },
                full: {
                    hour: "numeric",
                    minute: "numeric",
                    second: "numeric",
                    timeZoneName: "short"
                }
            }
        }
    }), u.a(r, "__localeData__", {
        value: u.b(null)
    }), u.a(r, "__addLocaleData", {
        value: function(n) {
            if (!(n && n.locale)) throw new Error("Locale data provided to IntlMessageFormat is missing a `locale` property");
            r.__localeData__[n.locale.toLowerCase()] = n
        }
    }), u.a(r, "__parse", {
        value: s.a.parse
    }), u.a(r, "defaultLocale", {
        enumerable: !0,
        writable: !0,
        value: undefined
    }), r.prototype.resolvedOptions = function() {
        return {
            locale: this._locale
        }
    }, r.prototype._compilePattern = function(n, t, i, r) {
        var u = new e.a(t, i, r);
        return u.compile(n)
    }, r.prototype._findPluralRuleFunction = function(n) {
        for (var i = r.__localeData__, t = i[n.toLowerCase()]; t;) {
            if (t.pluralRuleFunction) return t.pluralRuleFunction;
            t = t.parentLocale && i[t.parentLocale.toLowerCase()]
        }
        throw new Error("Locale data added to IntlMessageFormat is missing a `pluralRuleFunction` for :" + n);
    }, r.prototype._format = function(n, t) {
        for (var r = "", i, e, o, u = 0, s = n.length; u < s; u += 1) {
            if (i = n[u], typeof i == "string") {
                r += i;
                continue
            }
            if (e = i.id, !(t && f.b.call(t, e))) throw new Error("A value must be provided for: " + e);
            o = t[e], r += i.options ? this._format(i.getOption(o), t) : i.format(o)
        }
        return r
    }, r.prototype._mergeFormats = function(n, t) {
        var r = {},
            i, e;
        for (i in n) f.b.call(n, i) && (r[i] = e = u.b(n[i]), t && f.b.call(t, i) && f.a(e, t[i]));
        return r
    }, r.prototype._resolveLocale = function(n) {
        var f, t, e, i, u, o;
        for (typeof n == "string" && (n = [n]), n = (n || []).concat(r.defaultLocale), f = r.__localeData__, t = 0, e = n.length; t < e; t += 1)
            for (i = n[t].toLowerCase().split("-"); i.length;) {
                if (u = f[i.join("-")], u) return u.locale;
                i.pop()
            }
        o = n.pop();
        throw new Error("No locale data has been added to IntlMessageFormat for: " + n.join(", ") + ", or the default locale: " + o);
    }
}, function(n, t, i) {
    "use strict";
    i.d(t, "a", function() {
        return f
    }), i.d(t, "b", function() {
        return e
    });
    var r = i(0),
        u = function() {
            try {
                return !!Object.defineProperty({}, "a", {})
            } catch (n) {
                return !1
            }
        }(),
        o = !u && !Object.prototype.__defineGetter__,
        f = u ? Object.defineProperty : function(n, t, i) {
            "get" in i && n.__defineGetter__ ? n.__defineGetter__(t, i.get) : (!r.b.call(n, t) || "value" in i) && (n[t] = i.value)
        },
        e = Object.create || function(n, t) {
            function e() {}
            var u, i;
            e.prototype = n, u = new e;
            for (i in t) r.b.call(t, i) && f(u, i, t[i]);
            return u
        }
}, function(n, t) {
    "use strict";

    function r(n, t, i) {
        this.locales = n, this.formats = t, this.pluralFn = i
    }

    function u(n) {
        this.id = n
    }

    function f(n, t, i, r, u) {
        this.id = n, this.useOrdinal = t, this.offset = i, this.options = r, this.pluralFn = u
    }

    function e(n, t, i, r) {
        this.id = n, this.offset = t, this.numberFormat = i, this.string = r
    }

    function o(n, t) {
        this.id = n, this.options = t
    }
    t.a = r, r.prototype.compile = function(n) {
        return this.pluralStack = [], this.currentPlural = null, this.pluralNumberFormat = null, this.compileMessage(n)
    }, r.prototype.compileMessage = function(n) {
        if (!(n && n.type === "messageFormatPattern")) throw new Error('Message AST is not of type: "messageFormatPattern"');
        for (var u = n.elements, r = [], i, t = 0, f = u.length; t < f; t += 1) {
            i = u[t];
            switch (i.type) {
                case "messageTextElement":
                    r.push(this.compileMessageText(i));
                    break;
                case "argumentElement":
                    r.push(this.compileArgument(i));
                    break;
                default:
                    throw new Error("Message element does not have a valid type");
            }
        }
        return r
    }, r.prototype.compileMessageText = function(n) {
        return this.currentPlural && /(^|[^\\])#/g.test(n.value) ? (this.pluralNumberFormat || (this.pluralNumberFormat = new Intl.NumberFormat(this.locales)), new e(this.currentPlural.id, this.currentPlural.format.offset, this.pluralNumberFormat, n.value)) : n.value.replace(/\\#/g, "#")
    }, r.prototype.compileArgument = function(n) {
        var i = n.format;
        if (!i) return new u(n.id);
        var r = this.formats,
            e = this.locales,
            s = this.pluralFn,
            t;
        switch (i.type) {
            case "numberFormat":
                return t = r.number[i.style], {
                    id: n.id,
                    format: new Intl.NumberFormat(e, t).format
                };
            case "dateFormat":
                return t = r.date[i.style], {
                    id: n.id,
                    format: new Intl.DateTimeFormat(e, t).format
                };
            case "timeFormat":
                return t = r.time[i.style], {
                    id: n.id,
                    format: new Intl.DateTimeFormat(e, t).format
                };
            case "pluralFormat":
                return t = this.compileOptions(n), new f(n.id, i.ordinal, i.offset, t, s);
            case "selectFormat":
                return t = this.compileOptions(n), new o(n.id, t);
            default:
                throw new Error("Message element does not have a valid format type");
        }
    }, r.prototype.compileOptions = function(n) {
        var r = n.format,
            u = r.options,
            f = {},
            t, e, i;
        for (this.pluralStack.push(this.currentPlural), this.currentPlural = r.type === "pluralFormat" ? n : null, t = 0, e = u.length; t < e; t += 1) i = u[t], f[i.selector] = this.compileMessage(i.value);
        return this.currentPlural = this.pluralStack.pop(), f
    }, u.prototype.format = function(n) {
        return !n && typeof n != "number" ? "" : typeof n == "string" ? n : String(n)
    }, f.prototype.getOption = function(n) {
        var t = this.options,
            i = t["=" + n] || t[this.pluralFn(n - this.offset, this.useOrdinal)];
        return i || t.other
    }, e.prototype.format = function(n) {
        var t = this.numberFormat.format(n - this.offset);
        return this.string.replace(/(^|[^\\])#/g, "$1" + t).replace(/\\#/g, "#")
    }, o.prototype.getOption = function(n) {
        var t = this.options;
        return t[n] || t.other
    }
}, function(n, t, i) {
    "use strict";
    t = n.exports = i(6)["default"], t["default"] = t
}, function(n, t) {
    "use strict";
    t["default"] = function() {
        function t(n, t) {
            function i() {
                this.constructor = n
            }
            i.prototype = t.prototype, n.prototype = new i
        }

        function n(t, i, r, u) {
            this.message = t, this.expected = i, this.found = r, this.location = u, this.name = "SyntaxError", typeof Error.captureStackTrace == "function" && Error.captureStackTrace(this, n)
        }

        function i(t) {
            function h() {
                return c(e, r)
            }

            function et(n) {
                var i = w[n],
                    r, u;
                if (i) return i;
                for (r = n - 1; !w[r];) r--;
                for (i = w[r], i = {
                        line: i.line,
                        column: i.column,
                        seenCR: i.seenCR
                    }; r < n;) u = t.charAt(r), u === "\n" ? (i.seenCR || i.line++, i.column = 1, i.seenCR = !1) : u === "\r" || u === "\u2028" || u === "\u2029" ? (i.line++, i.column = 1, i.seenCR = !0) : (i.column++, i.seenCR = !1), r++;
                return w[n] = i, i
            }

            function c(n, t) {
                var i = et(n),
                    r = et(t);
                return {
                    start: {
                        offset: n,
                        line: i.line,
                        column: i.column
                    },
                    end: {
                        offset: t,
                        line: r.line,
                        column: r.column
                    }
                }
            }

            function f(n) {
                r < s || (r > s && (s = r, tt = []), tt.push(n))
            }

            function g(t, i, r, u) {
                function f(n) {
                    var t = 1;
                    for (n.sort(function(n, t) {
                            return n.description < t.description ? -1 : n.description > t.description ? 1 : 0
                        }); t < n.length;) n[t - 1] === n[t] ? n.splice(t, 1) : t++
                }

                function e(n, t) {
                    function e(n) {
                        function t(n) {
                            return n.charCodeAt(0).toString(16).toUpperCase()
                        }
                        return n.replace(/\\/g, "\\\\").replace(/"/g, '\\"').replace(/\x08/g, "\\b").replace(/\t/g, "\\t").replace(/\n/g, "\\n").replace(/\f/g, "\\f").replace(/\r/g, "\\r").replace(/[\x00-\x07\x0B\x0E\x0F]/g, function(n) {
                            return "\\x0" + t(n)
                        }).replace(/[\x10-\x1F\x80-\xFF]/g, function(n) {
                            return "\\x" + t(n)
                        }).replace(/[\u0100-\u0FFF]/g, function(n) {
                            return "\\u0" + t(n)
                        }).replace(/[\u1000-\uFFFF]/g, function(n) {
                            return "\\u" + t(n)
                        })
                    }
                    for (var r = new Array(n.length), u, f, i = 0; i < n.length; i++) r[i] = n[i].description;
                    return u = n.length > 1 ? r.slice(0, -1).join(", ") + " or " + r[n.length - 1] : r[0], f = t ? '"' + e(t) + '"' : "end of input", "Expected " + u + " but " + f + " found."
                }
                return i !== null && f(i), new n(t !== null ? t : e(i, r), i, r, u)
            }

            function ut() {
                var n;
                return n = rt()
            }

            function rt() {
                var t, n, u;
                for (t = r, n = [], u = ct(); u !== i;) n.push(u), u = ct();
                return n !== i && (e = t, n = di(n)), t = n
            }

            function ct() {
                var n;
                return n = fr(), n === i && (n = ir()), n
            }

            function yi() {
                var s, u, n, f, h, c;
                if (s = r, u = [], n = r, f = o(), f !== i ? (h = b(), h !== i ? (c = o(), c !== i ? (f = [f, h, c], n = f) : (r = n, n = i)) : (r = n, n = i)) : (r = n, n = i), n !== i)
                    while (n !== i) u.push(n), n = r, f = o(), f !== i ? (h = b(), h !== i ? (c = o(), c !== i ? (f = [f, h, c], n = f) : (r = n, n = i)) : (r = n, n = i)) : (r = n, n = i);
                else u = i;
                return u !== i && (e = s, u = bi(u)), s = u, s === i && (s = r, u = nt(), s = u !== i ? t.substring(s, r) : u), s
            }

            function fr() {
                var t, n;
                return t = r, n = yi(), n !== i && (e = t, n = wi(n)), t = n
            }

            function rr() {
                var n, o, e;
                if (n = d(), n === i) {
                    if (n = r, o = [], st.test(t.charAt(r)) ? (e = t.charAt(r), r++) : (e = i, u === 0 && f(ht)), e !== i)
                        while (e !== i) o.push(e), st.test(t.charAt(r)) ? (e = t.charAt(r), r++) : (e = i, u === 0 && f(ht));
                    else o = i;
                    n = o !== i ? t.substring(n, r) : o
                }
                return n
            }

            function ir() {
                var n, v, w, y, b, s, h, c, p;
                return n = r, t.charCodeAt(r) === 123 ? (v = lt, r++) : (v = i, u === 0 && f(vt)), v !== i ? (w = o(), w !== i ? (y = rr(), y !== i ? (b = o(), b !== i ? (s = r, t.charCodeAt(r) === 44 ? (h = l, r++) : (h = i, u === 0 && f(a)), h !== i ? (c = o(), c !== i ? (p = tr(), p !== i ? (h = [h, c, p], s = h) : (r = s, s = i)) : (r = s, s = i)) : (r = s, s = i), s === i && (s = null), s !== i ? (h = o(), h !== i ? (t.charCodeAt(r) === 125 ? (c = wt, r++) : (c = i, u === 0 && f(yt)), c !== i ? (e = n, v = er(y, s), n = v) : (r = n, n = i)) : (r = n, n = i)) : (r = n, n = i)) : (r = n, n = i)) : (r = n, n = i)) : (r = n, n = i)) : (r = n, n = i), n
            }

            function tr() {
                var n;
                return n = nr(), n === i && (n = gi(), n === i && (n = ki(), n === i && (n = pi()))), n
            }

            function nr() {
                var h, n, p, s, c, v, y;
                return h = r, t.substr(r, 6) === ti ? (n = ti, r += 6) : (n = i, u === 0 && f(tu)), n === i && (t.substr(r, 4) === ri ? (n = ri, r += 4) : (n = i, u === 0 && f(nu)), n === i && (t.substr(r, 4) === ui ? (n = ui, r += 4) : (n = i, u === 0 && f(gr)))), n !== i ? (p = o(), p !== i ? (s = r, t.charCodeAt(r) === 44 ? (c = l, r++) : (c = i, u === 0 && f(a)), c !== i ? (v = o(), v !== i ? (y = b(), y !== i ? (c = [c, v, y], s = c) : (r = s, s = i)) : (r = s, s = i)) : (r = s, s = i), s === i && (s = null), s !== i ? (e = h, n = dr(n, s), h = n) : (r = h, h = i)) : (r = h, h = i)) : (r = h, h = i), h
            }

            function gi() {
                var n, s, v, h, y, c;
                return n = r, t.substr(r, 6) === ei ? (s = ei, r += 6) : (s = i, u === 0 && f(kr)), s !== i ? (v = o(), v !== i ? (t.charCodeAt(r) === 44 ? (h = l, r++) : (h = i, u === 0 && f(a)), h !== i ? (y = o(), y !== i ? (c = oi(), c !== i ? (e = n, s = br(c), n = s) : (r = n, n = i)) : (r = n, n = i)) : (r = n, n = i)) : (r = n, n = i)) : (r = n, n = i), n
            }

            function ki() {
                var n, s, v, h, y, c;
                return n = r, t.substr(r, 13) === si ? (s = si, r += 13) : (s = i, u === 0 && f(iu)), s !== i ? (v = o(), v !== i ? (t.charCodeAt(r) === 44 ? (h = l, r++) : (h = i, u === 0 && f(a)), h !== i ? (y = o(), y !== i ? (c = oi(), c !== i ? (e = n, s = wr(c), n = s) : (r = n, n = i)) : (r = n, n = i)) : (r = n, n = i)) : (r = n, n = i)) : (r = n, n = i), n
            }

            function pi() {
                var n, s, p, v, w, h, c;
                if (n = r, t.substr(r, 6) === ni ? (s = ni, r += 6) : (s = i, u === 0 && f(yr)), s !== i)
                    if (p = o(), p !== i)
                        if (t.charCodeAt(r) === 44 ? (v = l, r++) : (v = i, u === 0 && f(a)), v !== i)
                            if (w = o(), w !== i) {
                                if (h = [], c = y(), c !== i)
                                    while (c !== i) h.push(c), c = y();
                                else h = i;
                                h !== i ? (e = n, s = vr(h), n = s) : (r = n, n = i)
                            } else r = n, n = i;
                else r = n, n = i;
                else r = n, n = i;
                else r = n, n = i;
                return n
            }

            function vi() {
                var e, n, o, s;
                return e = r, n = r, t.charCodeAt(r) === 61 ? (o = ar, r++) : (o = i, u === 0 && f(lr)), o !== i ? (s = d(), s !== i ? (o = [o, s], n = o) : (r = n, n = i)) : (r = n, n = i), e = n !== i ? t.substring(e, r) : n, e === i && (e = b()), e
            }

            function y() {
                var n, s, h, v, c, y, l, p, a;
                return n = r, s = o(), s !== i ? (h = vi(), h !== i ? (v = o(), v !== i ? (t.charCodeAt(r) === 123 ? (c = lt, r++) : (c = i, u === 0 && f(vt)), c !== i ? (y = o(), y !== i ? (l = rt(), l !== i ? (p = o(), p !== i ? (t.charCodeAt(r) === 125 ? (a = wt, r++) : (a = i, u === 0 && f(yt)), a !== i ? (e = n, s = cr(h, l), n = s) : (r = n, n = i)) : (r = n, n = i)) : (r = n, n = i)) : (r = n, n = i)) : (r = n, n = i)) : (r = n, n = i)) : (r = n, n = i)) : (r = n, n = i), n
            }

            function ai() {
                var n, s, c, h;
                return n = r, t.substr(r, 7) === ii ? (s = ii, r += 7) : (s = i, u === 0 && f(hr)), s !== i ? (c = o(), c !== i ? (h = d(), h !== i ? (e = n, s = pr(h), n = s) : (r = n, n = i)) : (r = n, n = i)) : (r = n, n = i), n
            }

            function oi() {
                var n, t, s, u, f;
                if (n = r, t = ai(), t === i && (t = null), t !== i)
                    if (s = o(), s !== i) {
                        if (u = [], f = y(), f !== i)
                            while (f !== i) u.push(f), f = y();
                        else u = i;
                        u !== i ? (e = n, t = uu(t, u), n = t) : (r = n, n = i)
                    } else r = n, n = i;
                else r = n, n = i;
                return n
            }

            function nt() {
                var e, n;
                if (u++, e = [], it.test(t.charAt(r)) ? (n = t.charAt(r), r++) : (n = i, u === 0 && f(dt)), n !== i)
                    while (n !== i) e.push(n), it.test(t.charAt(r)) ? (n = t.charAt(r), r++) : (n = i, u === 0 && f(dt));
                else e = i;
                return u--, e === i && (n = i, u === 0 && f(au)), e
            }

            function o() {
                var n, e, o;
                for (u++, n = r, e = [], o = nt(); o !== i;) e.push(o), o = nt();
                return n = e !== i ? t.substring(n, r) : e, u--, n === i && (e = i, u === 0 && f(fu)), n
            }

            function fi() {
                var n;
                return gu.test(t.charAt(r)) ? (n = t.charAt(r), r++) : (n = i, u === 0 && f(du)), n
            }

            function v() {
                var n;
                return ku.test(t.charAt(r)) ? (n = t.charAt(r), r++) : (n = i, u === 0 && f(bu)), n
            }

            function d() {
                var h, n, o, s, c, l;
                if (h = r, t.charCodeAt(r) === 48 ? (n = wu, r++) : (n = i, u === 0 && f(pu)), n === i) {
                    if (n = r, o = r, yu.test(t.charAt(r)) ? (s = t.charAt(r), r++) : (s = i, u === 0 && f(nf)), s !== i) {
                        for (c = [], l = fi(); l !== i;) c.push(l), l = fi();
                        c !== i ? (s = [s, c], o = s) : (r = o, o = i)
                    } else r = o, o = i;
                    n = o !== i ? t.substring(n, r) : o
                }
                return n !== i && (e = h, n = tf(n)), h = n
            }

            function ot() {
                var n, o, h, s, c, l, a, y;
                return vu.test(t.charAt(r)) ? (n = t.charAt(r), r++) : (n = i, u === 0 && f(lu)), n === i && (n = r, t.substr(r, 2) === kt ? (o = kt, r += 2) : (o = i, u === 0 && f(cu)), o !== i && (e = n, o = hu()), n = o, n === i && (n = r, t.substr(r, 2) === at ? (o = at, r += 2) : (o = i, u === 0 && f(su)), o !== i && (e = n, o = or()), n = o, n === i && (n = r, t.substr(r, 2) === pt ? (o = pt, r += 2) : (o = i, u === 0 && f(ou)), o !== i && (e = n, o = eu()), n = o, n === i && (n = r, t.substr(r, 2) === bt ? (o = bt, r += 2) : (o = i, u === 0 && f(sr)), o !== i && (e = n, o = ru()), n = o, n === i && (n = r, t.substr(r, 2) === gt ? (o = gt, r += 2) : (o = i, u === 0 && f(ur)), o !== i ? (h = r, s = r, c = v(), c !== i ? (l = v(), l !== i ? (a = v(), a !== i ? (y = v(), y !== i ? (c = [c, l, a, y], s = c) : (r = s, s = i)) : (r = s, s = i)) : (r = s, s = i)) : (r = s, s = i), h = s !== i ? t.substring(h, r) : s, h !== i ? (e = n, o = ci(h), n = o) : (r = n, n = i)) : (r = n, n = i)))))), n
            }

            function b() {
                var u, n, t;
                if (u = r, n = [], t = ot(), t !== i)
                    while (t !== i) n.push(t), t = ot();
                else n = i;
                return n !== i && (e = u, n = li(n)), u = n
            }
            var k = arguments.length > 1 ? arguments[1] : {},
                rf = this,
                i = {},
                hi = {
                    start: ut
                },
                ft = ut,
                di = function(n) {
                    return {
                        type: "messageFormatPattern",
                        elements: n,
                        location: h()
                    }
                },
                bi = function(n) {
                    for (var u = "", i, r, e, t = 0, f = n.length; t < f; t += 1)
                        for (r = n[t], i = 0, e = r.length; i < e; i += 1) u += r[i];
                    return u
                },
                wi = function(n) {
                    return {
                        type: "messageTextElement",
                        value: n,
                        location: h()
                    }
                },
                st = /^[^ \t\n\r,.+={}#]/,
                ht = {
                    type: "class",
                    value: "[^ \\t\\n\\r,.+={}#]",
                    description: "[^ \\t\\n\\r,.+={}#]"
                },
                lt = "{",
                vt = {
                    type: "literal",
                    value: "{",
                    description: '"{"'
                },
                l = ",",
                a = {
                    type: "literal",
                    value: ",",
                    description: '","'
                },
                wt = "}",
                yt = {
                    type: "literal",
                    value: "}",
                    description: '"}"'
                },
                er = function(n, t) {
                    return {
                        type: "argumentElement",
                        id: n,
                        format: t && t[2],
                        location: h()
                    }
                },
                ti = "number",
                tu = {
                    type: "literal",
                    value: "number",
                    description: '"number"'
                },
                ri = "date",
                nu = {
                    type: "literal",
                    value: "date",
                    description: '"date"'
                },
                ui = "time",
                gr = {
                    type: "literal",
                    value: "time",
                    description: '"time"'
                },
                dr = function(n, t) {
                    return {
                        type: n + "Format",
                        style: t && t[2],
                        location: h()
                    }
                },
                ei = "plural",
                kr = {
                    type: "literal",
                    value: "plural",
                    description: '"plural"'
                },
                br = function(n) {
                    return {
                        type: n.type,
                        ordinal: !1,
                        offset: n.offset || 0,
                        options: n.options,
                        location: h()
                    }
                },
                si = "selectordinal",
                iu = {
                    type: "literal",
                    value: "selectordinal",
                    description: '"selectordinal"'
                },
                wr = function(n) {
                    return {
                        type: n.type,
                        ordinal: !0,
                        offset: n.offset || 0,
                        options: n.options,
                        location: h()
                    }
                },
                ni = "select",
                yr = {
                    type: "literal",
                    value: "select",
                    description: '"select"'
                },
                vr = function(n) {
                    return {
                        type: "selectFormat",
                        options: n,
                        location: h()
                    }
                },
                ar = "=",
                lr = {
                    type: "literal",
                    value: "=",
                    description: '"="'
                },
                cr = function(n, t) {
                    return {
                        type: "optionalFormatPattern",
                        selector: n,
                        value: t,
                        location: h()
                    }
                },
                ii = "offset:",
                hr = {
                    type: "literal",
                    value: "offset:",
                    description: '"offset:"'
                },
                pr = function(n) {
                    return n
                },
                uu = function(n, t) {
                    return {
                        type: "pluralFormat",
                        offset: n,
                        options: t,
                        location: h()
                    }
                },
                au = {
                    type: "other",
                    description: "whitespace"
                },
                it = /^[ \t\n\r]/,
                dt = {
                    type: "class",
                    value: "[ \\t\\n\\r]",
                    description: "[ \\t\\n\\r]"
                },
                fu = {
                    type: "other",
                    description: "optionalWhitespace"
                },
                gu = /^[0-9]/,
                du = {
                    type: "class",
                    value: "[0-9]",
                    description: "[0-9]"
                },
                ku = /^[0-9a-f]/i,
                bu = {
                    type: "class",
                    value: "[0-9a-f]i",
                    description: "[0-9a-f]i"
                },
                wu = "0",
                pu = {
                    type: "literal",
                    value: "0",
                    description: '"0"'
                },
                yu = /^[1-9]/,
                nf = {
                    type: "class",
                    value: "[1-9]",
                    description: "[1-9]"
                },
                tf = function(n) {
                    return parseInt(n, 10)
                },
                vu = /^[^{}\\\0-\x1F \t\n\r]/,
                lu = {
                    type: "class",
                    value: "[^{}\\\\\\0-\\x1F\\x7f \\t\\n\\r]",
                    description: "[^{}\\\\\\0-\\x1F\\x7f \\t\\n\\r]"
                },
                kt = "\\\\",
                cu = {
                    type: "literal",
                    value: "\\\\",
                    description: '"\\\\\\\\"'
                },
                hu = function() {
                    return "\\"
                },
                at = "\\#",
                su = {
                    type: "literal",
                    value: "\\#",
                    description: '"\\\\#"'
                },
                or = function() {
                    return "\\#"
                },
                pt = "\\{",
                ou = {
                    type: "literal",
                    value: "\\{",
                    description: '"\\\\{"'
                },
                eu = function() {
                    return "{"
                },
                bt = "\\}",
                sr = {
                    type: "literal",
                    value: "\\}",
                    description: '"\\\\}"'
                },
                ru = function() {
                    return "}"
                },
                gt = "\\u",
                ur = {
                    type: "literal",
                    value: "\\u",
                    description: '"\\\\u"'
                },
                ci = function(n) {
                    return String.fromCharCode(parseInt(n, 16))
                },
                li = function(n) {
                    return n.join("")
                },
                r = 0,
                e = 0,
                w = [{
                    line: 1,
                    column: 1,
                    seenCR: !1
                }],
                s = 0,
                tt = [],
                u = 0,
                p;
            if ("startRule" in k) {
                if (!(k.startRule in hi)) throw new Error("Can't start parsing from rule \"" + k.startRule + '".');
                ft = hi[k.startRule]
            }
            if (p = ft(), p !== i && r === t.length) return p;
            p !== i && r < t.length && f({
                type: "end",
                description: "end of input"
            });
            throw g(null, tt, s < t.length ? t.charAt(s) : null, s < t.length ? c(s, s + 1) : c(s, s));
        }
        return t(n, Error), {
            SyntaxError: n,
            parse: i
        }
    }()
}, function(n, t) {
    "use strict";
    t.a = {
        locale: "en",
        pluralRuleFunction: function(n, t) {
            var i = String(n).split("."),
                e = !i[1],
                f = Number(i[0]) == n,
                r = f && i[0].slice(-1),
                u = f && i[0].slice(-2);
            return t ? r == 1 && u != 11 ? "one" : r == 2 && u != 12 ? "two" : r == 3 && u != 13 ? "few" : "other" : n == 1 && e ? "one" : "other"
        }
    }
}]);