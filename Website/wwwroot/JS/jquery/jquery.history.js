// jquery/jquery.history.js
typeof window.console == "undefined" && (window.console = {}), typeof window.console.emulated == "undefined" && (typeof window.console.log == "function" ? window.console.hasLog = !0 : (typeof window.console.log == "undefined" && (window.console.log = function() {}), window.console.hasLog = !1), typeof window.console.debug == "function" ? window.console.hasDebug = !0 : (typeof window.console.debug == "undefined" && (window.console.debug = window.console.hasLog ? function() {
        for (var t = ["console.debug:"], n = 0; n < arguments.length; n++) t.push(arguments[n]);
        window.console.log.apply(window.console, t)
    } : function() {}), window.console.hasDebug = !1), typeof window.console.warn == "function" ? window.console.hasWarn = !0 : (typeof window.console.warn == "undefined" && (window.console.warn = window.console.hasLog ? function() {
        for (var t = ["console.warn:"], n = 0; n < arguments.length; n++) t.push(arguments[n]);
        window.console.log.apply(window.console, t)
    } : function() {}), window.console.hasWarn = !1), typeof window.console.error == "function" ? window.console.hasError = !0 : (typeof window.console.error == "undefined" && (window.console.error = function() {
        var t = "An error has occured.",
            i, n;
        if (window.console.hasLog) {
            for (i = ["console.error:"], n = 0; n < arguments.length; n++) i.push(arguments[n]);
            window.console.log.apply(window.console, i), t = "An error has occured. More information is available in your browser's javascript console."
        }
        for (n = 0; n < arguments.length; ++n) {
            if (typeof arguments[n] != "string") break;
            t += "\n" + arguments[n]
        }
        if (typeof Error != "undefined") throw new Error(t);
        else throw t;
    }), window.console.hasError = !1), typeof window.console.trace == "function" ? window.console.hasTrace = !0 : (typeof window.console.trace == "undefined" && (window.console.trace = function() {
        window.console.error("console.trace does not exist")
    }), window.console.hasTrace = !1), window.console.emulated = !0),
    function(n) {
        n.History || !1 ? window.console.warn("$.History has already been defined...") : (n.History = {
            options: {
                debug: !1
            },
            state: "",
            $window: null,
            $iframe: null,
            handlers: {
                generic: [],
                specific: {}
            },
            extractHash: function(n) {
                return n.replace(/^[^#]*#/, "").replace(/^#+|#+$/, "")
            },
            getState: function() {
                var t = n.History;
                return t.state
            },
            setState: function(t) {
                var i = n.History;
                return t = i.extractHash(t), i.state = t
            },
            getHash: function() {
                var t = n.History;
                return t.extractHash(window.location.hash || location.hash)
            },
            setHash: function(t) {
                var i = n.History;
                return t = i.extractHash(t), typeof window.location.hash != "undefined" ? window.location.hash !== t && (window.location.hash = t) : location.hash !== t && (location.hash = t), t
            },
            go: function(t) {
                var i = n.History,
                    r, u;
                return t = i.extractHash(t), r = i.getHash(), u = i.getState(), t !== r ? i.setHash(t) : (t !== u && i.setState(t), i.trigger()), !0
            },
            hashchange: function() {
                var i = n.History,
                    r = i.getHash();
                return i.go(r), !0
            },
            bind: function(t, i) {
                var r = n.History;
                return i ? (typeof r.handlers.specific[t] == "undefined" && (r.handlers.specific[t] = []), r.handlers.specific[t].push(i)) : (i = t, r.handlers.generic.push(i)), !0
            },
            trigger: function(t) {
                var u = n.History,
                    i, f, e, r;
                if (typeof t == "undefined" && (t = u.getState()), typeof u.handlers.specific[t] != "undefined")
                    for (r = u.handlers.specific[t], i = 0, f = r.length; i < f; ++i) e = r[i], e(t);
                for (r = u.handlers.generic, i = 0, f = r.length; i < f; ++i) e = r[i], e(t);
                return !0
            },
            construct: function() {
                var t = n.History;
                return n(document).ready(function() {
                    t.domReady()
                }), !0
            },
            configure: function(t) {
                var i = n.History;
                return i.options = n.extend(i.options, t), !0
            },
            domReadied: !1,
            domReady: function() {
                var t = n.History;
                if (!t.domRedied) return t.domRedied = !0, t.$window = n(window), t.$window.bind("hashchange", this.hashchange), setTimeout(t.hashchangeLoader, 200), !0
            },
            nativeSupport: function(t) {
                t = t || n.browser;
                var e = t.version,
                    o = parseInt(e, 10),
                    u = e.split(/[^0-9]/g),
                    r = parseInt(u[0], 10),
                    f = parseInt(u[1], 10),
                    s = parseInt(u[2], 10),
                    i = !1;
                return (t.msie || !1) && o >= 8 ? i = !0 : (t.webkit || !1) && o >= 528 ? i = !0 : t.mozilla || !1 ? r > 1 ? i = !0 : r === 1 && (f > 9 ? i = !0 : f === 9 && s >= 2 && (i = !0)) : (t.opera || !1) && (r > 10 ? i = !0 : r === 10 && f >= 60 && (i = !0)), i
            },
            hashchangeLoader: function() {
                var t = n.History,
                    f = t.nativeSupport(),
                    r, i, u;
                return f ? (u = t.getHash(), u && t.$window.trigger("hashchange")) : (n.browser.msie ? (t.$iframe = n('<iframe id="jquery-history-iframe" style="display: none;"></$iframe>').prependTo(document.body)[0], t.$iframe.contentWindow.document.open(), t.$iframe.contentWindow.document.close(), i = !1, r = function() {
                    var n = t.getHash(),
                        r = t.getState(),
                        u = t.extractHash(t.$iframe.contentWindow.document.location.hash);
                    r !== n ? (i || (t.$iframe.contentWindow.document.open(), t.$iframe.contentWindow.document.close(), t.$iframe.contentWindow.document.location.hash = n), i = !1, t.$window.trigger("hashchange")) : r !== u && (i = !0, t.setHash(u))
                }) : r = function() {
                    var n = t.getHash(),
                        i = t.getState();
                    i !== n && t.$window.trigger("hashchange")
                }, setInterval(r, 200)), !0
            }
        }, n.History.construct())
    }(jQuery);