// polyfill/ie7localStorage.js
(typeof window.localStorage == "undefined" || typeof window.sessionStorage == "undefined") && function() {
    var n = function(n) {
        function f() {
            for (var i = u + "=", r = document.cookie.split(";"), n, t = 0; t < r.length; t++) {
                for (n = r[t]; n.charAt(0) === " ";) n = n.substring(1, n.length);
                if (n.indexOf(i) === 0) return n.substring(i.length, n.length)
            }
            return null
        }

        function i(t) {
            if (t = t.length ? JSON.stringify(t) : "", n == "session") window.name = t, r || (window.addEventListener ? window.addEventListener("unload", i, !1) : window.attachEvent && window.attachEvent("onunload", i), r = !0);
            else {
                var f = new Date;
                f.setTime(f.getTime() + 31536e6), document.cookie = u + "=" + t + "; expires=" + f.toGMTString() + "; path=/"
            }
        }
        var r = !1,
            u = "localStorage",
            t = n == "session" ? window.name : f();
        return t = t ? JSON.parse(t) : {}, {
            clear: function() {
                t = {}, i("")
            },
            getItem: function(n) {
                return t[n] === undefined ? null : t[n]
            },
            key: function(n) {
                var i = 0,
                    r;
                for (r in t) {
                    if (i == n) return r;
                    i++
                }
                return null
            },
            removeItem: function(n) {
                delete t[n], i(t)
            },
            setItem: function(n, r) {
                t[n] = r + "", i(t)
            }
        }
    };
    typeof window.localStorage == "undefined" && (window.localStorage = new n("local")), typeof window.sessionStorage == "undefined" && (window.sessionStorage = new n("session"))
}();