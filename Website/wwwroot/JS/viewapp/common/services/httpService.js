// ~/viewapp/common/services/httpService.js
"use strict";
robloxAppService.factory("httpService", ["$http", "$q", "$log", function(n, t, i) {
    function r(n, t) {
        return t.withCredentials && (n.withCredentials = t.withCredentials), t.retryable && (n.retryable = t.retryable), t.noCache && (n.headers = {
            "Cache-Control": "no-cache, no-store, must-revalidate",
            Pragma: "no-cache",
            Expires: 0
        }), t.headers && (n.headers = t.headers), n
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

    function u(r) {
        var u = t.defer();
        return n(r).success(function(n) {
            u.resolve(n)
        }).error(function(n) {
            i.debug("Error: unable to send " + r.url + " request."), u.reject(n)
        }), u.promise
    }
    return {
        httpGet: function(t, i, r) {
            if (!t) return !1;
            var e = f(t, i);
            return r ? n(e) : u(e)
        },
        httpPost: function(t, i, r) {
            if (!t) return !1;
            var f = e(t, i);
            return r ? n(f) : u(f)
        }
    }
}]);