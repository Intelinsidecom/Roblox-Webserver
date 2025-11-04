// roblox.js
(function(n, t) {
    function b(n, i) {
        var r = i.split(".");
        for (i = r.shift(); r.length > 0; n = n[i], i = r.shift())
            if (n[i] === t) return t;
        return n[i]
    }

    function y(n, i, r) {
        var u = i.split(".");
        for (i = u.shift(); u.length > 0; n = n[i], i = u.shift()) n[i] === t && (n[i] = {});
        n[i] = r
    }

    function w(n, t) {
        var i = f.createElement("link");
        i.href = n, i.rel = "stylesheet", i.type = "text/css", u.parentNode.insertBefore(i, u), t()
    }

    function p(n, t) {
        var i = f.createElement("script");
        i.type = "text/javascript", i.src = n, i.onload = i.onreadystatechange = function() {
            i.readyState && i.readyState != "loaded" && i.readyState != "complete" || (t(), i.onload = i.onreadystatechange = null)
        }, u.parentNode.insertBefore(i, u)
    }

    function g(n) {
        return n.split(".").pop().split("?").shift()
    }

    function l(n) {
        if (n.indexOf(".js") < 0) return n;
        if (n.indexOf(r.modulePath) >= 0) return n.split(r.modulePath + "/").pop().split(".").shift().replace("/", ".");
        for (var t in r.paths)
            if (r.paths[t] == n) return t;
        return n
    }

    function v(n) {
        return n.indexOf(".js") >= 0 || n.indexOf(".css") >= 0 ? n : r.paths[n] || r.baseUrl + r.modulePath + "/" + n.replace(".", "/") + ".js"
    }

    function c(n) {
        for (var r, u = [], i = 0; i < n.length; i++) r = b(Roblox, l(n[i])), r !== t && u.push(r);
        return u
    }

    function e(n) {
        var t = i[n];
        if (t.loaded && t.depsLoaded)
            while (t.listeners.length > 0) t.listeners.shift()()
    }

    function a(n, u) {
        var f, o, s;
        if (!d(n) || r.externalResources.toString().indexOf(n) >= 0) return u();
        f = l(n), i[f] === t ? (i[f] = {
            loaded: !1,
            depsLoaded: !0,
            listeners: []
        }, i[f].listeners.push(u), o = v(f), s = g(o) == "css" ? w : p, s(o, function() {
            i[f].loaded = !0, e(f)
        })) : (i[f].listeners.push(u), e(f))
    }

    function o(n, t) {
        var i = n.shift(),
            r = n.length == 0 ? t : function() {
                o(n, t)
            };
        a(i, r)
    }

    function h(n, t) {
        s(n) || (n = [n]);
        var i = function() {
            t.apply(null, c(n))
        };
        o(n.slice(0), i)
    }

    function nt(n, t, r) {
        k(t) ? (r = t, t = []) : s(t) || (t = [t]), i[n] = i[n] || {
            loaded: !0,
            listeners: []
        }, i[n].depsLoaded = !1, i[n].listeners.unshift(function() {
            y(Roblox, n, r.apply(null, c(t)))
        }), h(t, function() {
            i[n].depsLoaded = !0, e(n)
        })
    }
    var f = n.document,
        u = f.getElementsByTagName("script")[0],
        d = function(n) {
            return typeof n == "string"
        },
        s = function(n) {
            return Object.prototype.toString.call(n) == "[object Array]"
        },
        k = function(n) {
            return Object.prototype.toString.call(n) == "[object Function]"
        },
        i = {},
        r = {
            baseUrl: "",
            modulePath: "/js/modules",
            paths: {},
            externalResources: []
        };
    typeof Roblox == "undefined" && (Roblox = {}), Roblox.config = r, Roblox.require = h, Roblox.define = nt
})(window);