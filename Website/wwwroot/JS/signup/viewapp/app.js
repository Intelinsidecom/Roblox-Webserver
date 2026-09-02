// ~/viewapp/app.js
"use strict";
var freebloxiaApp = (function() {
    try { return angular.module("freebloxiaApp"); } catch(e) { return null; }
})() || angular.module("freebloxiaApp", ["ngSanitize", "ui.router", "freebloxiaApp.services", "freebloxiaApp.filters", "templateApp"]).config(["$httpProvider", function(n) {
    var r = "X-CSRF-TOKEN",
        u = 403,
        t = angular.element("#http-retry-data"),
        f = t && t.data("http-retry-base-timeout") ? t.data("http-retry-base-timeout") : 1e3,
        e = t && t.data("http-retry-max-timeout") ? t.data("http-retry-max-timeout") : 8e3,
        i;
    n.interceptors.push(["$q", "$injector", function(n, t) {
        return {
            request: function(n) {
                return n.method.toLowerCase() === "post" && Freebloxia.XsrfToken.getToken() && (i || (i = Freebloxia.XsrfToken.getToken()), n.headers[r] = i), n
            },
            responseError: function(f) {
                var o = f.status,
                    s = t.get("$http"),
                    e;
                return o === u && f.headers(r) && (e = f.headers(r), e) ? (i = e, s(f.config)) : n.reject(f)
            }
        }
    }]), n.interceptors.push(["$q", "$injector", "$log", function(n, t, i) {
        function r(n) {
            var i = t.get("$timeout");
            return i(function() {
                n.incrementalTimeout = n.incrementalTimeout * 2;
                var i = t.get("$http");
                return i(n)
            }, n.incrementalTimeout)
        }
        return {
            responseError: function(t) {
                var o = t.status;
                return o !== u && angular.isDefined(t.config.retryable) && t.config.retryable ? (t.config.incrementalTimeout || (t.config.incrementalTimeout = f), i.debug("---- rejection.config.url ------" + t.config.url), i.debug("---- incrementalTimeout ------" + t.config.incrementalTimeout), t.config.incrementalTimeout <= e ? (i.debug("---- retry ------"), r(t.config)) : (t.config.incrementalTimeout = f, i.debug("---- failure promise ------"), n.reject(t))) : n.reject(t)
            }
        }
    }]), n.interceptors.push(["$q", "$injector", function() {
        return {
            request: function(n) {
                if (angular.isDefined(Freebloxia) && angular.isDefined(Freebloxia.Endpoints)) {
                    var t = Freebloxia.Endpoints.generateAbsoluteUrl(n.url, n.data, n.withCredentials);
                    n.url = t, Freebloxia.Endpoints.addCrossDomainOptionsToAllRequests && n.url.indexOf("rbxcdn.com") < 0 && n.url.indexOf("s3.amazonaws.com") < 0 && (n.withCredentials = !0)
                }
                return n
            }
        }
    }])
}]).config(["$logProvider", function(n) {
    var t = angular.isDefined(Freebloxia) && angular.isDefined(Freebloxia.jsConsoleEnabled) ? Freebloxia.jsConsoleEnabled : !1;
    n.debugEnabled(t)
}]).constant("_", window._ || {});