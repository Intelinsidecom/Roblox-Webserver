// JSErrorTracker.js
typeof Freebloxia == "undefined" && (Freebloxia = {}), Freebloxia.JSErrorTracker = {
    showAlert: !1,
    defaultPixel: "GA",
    javascriptStackTraceEnabled: !1,
    suppressConsoleError: !1,
    data: {
        category: "JavascriptExceptions"
    },
    initialize: function(n) {
        $.extend(Freebloxia.JSErrorTracker, n), this.addOnErrorEventHandler(this.errorHandler)
    },
    errorHandler: function(n, t, i) {
        try {
            Freebloxia.JSErrorTracker.data.msg = n, Freebloxia.JSErrorTracker.data.url = t, Freebloxia.JSErrorTracker.data.line = i, Freebloxia.JSErrorTracker.data.ua = window.navigator.userAgent, Freebloxia.JSErrorTracker.logException(Freebloxia.JSErrorTracker.data)
        } catch (r) {}
        return Freebloxia.JSErrorTracker.suppressConsoleError
    },
    addOnErrorEventHandler: function(n) {
        var t = window.onerror;
        window.onerror = typeof window.onerror == "function" ? function(i, r, u) {
            t(i, r, u), n(i, r, u)
        } : n
    },
    processException: function(n, t) {
        if (typeof n != "undefined") {
            typeof n.category == "undefined" && (n.category = Freebloxia.JSErrorTracker.data.category);
            switch (t) {
                case "GA":
                    var i = {
                        category: "category",
                        url: "action",
                        msg: "opt_label",
                        line: "opt_value"
                    };
                    Freebloxia.JSErrorTracker.fireGAPixel(Freebloxia.JSErrorTracker.distillGAData(n, i));
                    break;
                default:
                    console.log("Freebloxia JSErrorTracker received an unknown pixel to fire")
            }
            return !0
        }
    },
    logException: function(n) {
        Freebloxia.JSErrorTracker.processException(n, Freebloxia.JSErrorTracker.defaultPixel), Freebloxia.JSErrorTracker.showErrorMessage(n.msg)
    },
    distillData: function(n, t) {
        var r = {},
            i;
        for (i in t) typeof n[i] != "undefined" && (r[t[i]] = encodeURIComponent(n[i]));
        return r
    },
    distillGAData: function(n, t) {
        var i = Freebloxia.JSErrorTracker.distillData(n, t),
            r = [decodeURIComponent([i.category])];
        return typeof i.action != typeof undefined ? (r = r.concat(decodeURIComponent(i.action)), typeof i.opt_label != typeof undefined && (r = r.concat(decodeURIComponent(i.opt_label)), typeof i.opt_value != typeof undefined && (r = r.concat(parseInt(decodeURIComponent(i.opt_value)))))) : Freebloxia.JSErrorTracker.showAlert && alert("Missing a required parameter for GA"), r
    },
    createURL: function(n, t, i) {
        var r = n,
            f = Freebloxia.JSErrorTracker.distillData(t, i),
            u;
        if (r += "?", f != null)
            for (u in f) typeof u != typeof undefined && t.hasOwnProperty(u) && (r += u + "=" + f[u] + "&");
        return r = r.slice(0, r.length - 1)
    },
    fireGAPixel: function(n) {
        typeof _gaq != "undefined" && _gaq.push(["c._trackEvent"].concat(n))
    },
    showErrorMessage: function(n) {
        Freebloxia.JSErrorTracker.showAlert && (n !== null ? alert(n) : alert("An error occured"))
    }
};