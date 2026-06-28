// ~/viewapp/studio/toolbox/toolboxApp.js
"use strict";
var clientToolbox = angular.module("clientToolbox", ["robloxApp.services", "robloxApp.filters", "pageTemplateApp"]).config(["$httpProvider", function(n) {
    n.defaults.headers.post["Content-Type"] = "application/x-www-form-urlencoded;charset=utf-8";
    var t = function(n) {
        var r = "",
            f, i, o, h, s, u, e;
        for (f in n)
            if (i = n[f], i instanceof Array)
                for (e = 0; e < i.length; ++e) s = i[e], o = f + "[" + e + "]", u = {}, u[o] = s, r += t(u) + "&";
            else if (i instanceof Object)
            for (h in i) s = i[h], o = f + "[" + h + "]", u = {}, u[o] = s, r += t(u) + "&";
        else i !== undefined && i !== null && (r += encodeURIComponent(f) + "=" + encodeURIComponent(i) + "&");
        return r.length ? r.substr(0, r.length - 1) : r
    };
    n.defaults.transformRequest = [function(n) {
        return angular.isObject(n) && String(n) !== "[object File]" ? t(n) : n
    }], n.interceptors.push(["$q", "$injector", function() {
        return {
            request: function(n) {
                if (angular.isDefined(Roblox) && angular.isDefined(Roblox.Endpoints)) {
                    var t = Roblox.Endpoints.generateAbsoluteUrl(n.url, n.data, n.withCredentials);
                    n.url = t, Roblox.Endpoints.addCrossDomainOptionsToAllRequests && n.url.indexOf("rbxcdn.com") < 0 && n.url.indexOf("s3.amazonaws.com") < 0 && (n.withCredentials = !0)
                }
                return n
            }
        }
    }])
}]).config(["$logProvider", function(n) {
    var t = typeof Roblox.jsConsoleEnabled == "undefined" ? !1 : Roblox.jsConsoleEnabled;
    n.debugEnabled(t)
}]);