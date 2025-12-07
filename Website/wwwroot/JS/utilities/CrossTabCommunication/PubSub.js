// utilities/CrossTabCommunication/PubSub.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.CrossTabCommunication == "undefined" && (Roblox.CrossTabCommunication = {}), Roblox.CrossTabCommunication.PubSub = function() {
    function t() {
        return f()
    }

    function i(t, i, r) {
        var u = n(t, i);
        $(window).unbind(u).bind(u, function(n) {
            n.originalEvent.key === t && r(n.originalEvent.newValue)
        })
    }

    function r(t, i) {
        var r = n(t, i);
        $(window).unbind(r)
    }

    function u(n, t) {
        localStorage.removeItem(n), localStorage.setItem(n, t)
    }

    function n(n, t) {
        return "storage." + n + "_" + t
    }

    function f() {
        var n = "roblox";
        try {
            return localStorage.setItem(n, n), localStorage.removeItem(n), !0
        } catch (t) {
            return !1
        }
    }
    return {
        IsAvailable: t,
        Subscribe: i,
        Unsubscribe: r,
        Publish: u
    }
}();