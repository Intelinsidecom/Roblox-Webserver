// utilities/localStorage.js
var Roblox = Roblox || {};
Roblox.LocalStorage = function() {
    function n() {
        var n = "localstorage_validation_test";
        try {
            return localStorage.setItem(n, n), localStorage.removeItem(n), !0
        } catch (t) {
            return !1
        }
    }

    function t(n) {
        localStorage.removeItem(n)
    }

    function i(n, t, i) {
        var u = +new Date,
            r = JSON.parse(localStorage.getItem(n));
        r || (r = {}), r.value = t, r.expiryTimestamp = u + i, localStorage.setItem(n, JSON.stringify(r))
    }

    function r(n) {
        var r = +new Date,
            t = JSON.parse(localStorage.getItem(n)),
            i;
        if (t && t.expiryTimestamp) {
            if (i = t.expiryTimestamp, r < i) return t.value;
            localStorage.removeItem(n)
        }
        return null
    }
    return {
        isAvailable: n,
        removeFromLocalStorage: t,
        setItemWithExpiry: i,
        getItemWithExpiry: r
    }
}();