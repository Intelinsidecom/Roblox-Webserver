// ~/viewapp/common/services/httpService.js
"use strict";
robloxAppService.factory("httpService", ["$http", "$q", "$log", function(n, t, i) {
    function r(n, t) {
        return t.withCredentials && (n.withCredentials = t.withCredentials), t.retryable && (n.retryable = t.retryable), t.noCache && (n.headers = {
            "Cache-Control": "no-cache, no-store, must-revalidate",
            Pragma: "no-cache",
            Expires: 0
        }), t.headers && (n.headers = angular.extend(n.headers || {}, t.headers || {})), t.withFile && (n.transformRequest = function(n) {
            var t = new FormData;
            return angular.forEach(n, function(n, i) {
                t.append(i, n)
            }), t
        }, n.headers = angular.extend(n.headers || {}, {
            "Content-Type": undefined
        })), n
    }

    function f(n, t) {
        var i = {
            method: "GET",
            url: n.url,
            params: t
        };
        return i = r(i, n)
    }

    function e(n, t) {
        var i = {
            method: "POST",
            url: n.url,
            data: t
        };
        return i = r(i, n)
    }

    function o(n) {
        var i = {
            method: "DELETE",
            url: n.url
        };
        return i = r(i, n)
    }

    function s(n, t) {
        var i = {
            method: "PATCH",
            url: n.url,
            data: t
        };
        return i = r(i, n)
    }

    function u(r) {
        var u = t.defer();
        return n(r).then(function(n) {
            var t = n.data;
            t === "null" && (t = null), u.resolve(t)
        }, function(n) {
            var t = n.data;
            i.debug("Error: unable to send " + r.url + " request."), u.reject(t)
        }), u.promise
    }
    return {
        methods: {
            get: "GET",
            post: "POST",
            "delete": "DELETE",
            patch: "PATCH"
        },
        httpGet: function(t, i, r) {
            if (!t) return !1;
            var e = f(t, i);
            return r ? n(e) : u(e)
        },
        httpPost: function(t, i, r) {
            if (!t) return !1;
            var f = e(t, i);
            return r ? n(f) : u(f)
        },
        httpDelete: function(n, t) {
            if (!n) return !1;
            var i = o(n, t);
            return u(i)
        },
        httpPatch: function(n, t) {
            if (!n) return !1;
            var i = s(n, t);
            return u(i)
        },
        buildBatchPromises: function(r, o, l, a, c) {
            if (!o || 0 === o.length) return t.when([]);
            for (var h = [], p = 0; p < o.length; p += l) h.push(o.slice(p, p + l));
            var d = this;
            return t.all(h.map(function(n) {
                var t = {};
                t[a] = n;
                return c && c.toUpperCase() === "POST" ? d.httpPost(r, t) : d.httpGet(r, t)
            }))
        }
    }
}]);