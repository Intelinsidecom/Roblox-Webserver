// ~/viewapp/common/services/httpService.js
"use strict";
robloxAppService.factory("httpService", ["$http", "$q", "$log", function (n, t, i) {
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
        n(r).then(function (response) {
            // Preserve original behavior: resolve with the response payload
            u.resolve(response.data !== undefined ? response.data : response);
        }, function (error) {
            i.debug("Error: unable to send " + r.url + " request.");
            // Preserve original behavior: reject with the error payload
            u.reject(error && error.data !== undefined ? error.data : error);
        });
        return u.promise;
    }
    return {
        httpGet: function (t, i, r) {
            if (!t) return !1;
            var e = f(t, i);
            return r ? n(e) : u(e)
        },
        httpPost: function (t, i, r) {
            if (!t) return !1;
            var f = e(t, i);
            return r ? n(f) : u(f)
        }
    }
}]);