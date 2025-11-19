// ~/viewapp/common/filters/localNumberFormat.js
"use strict";
robloxApp.filter("localNumberFormat", ["languageResource", function(n) {
    return function(t, i) {
        return t ? (typeof t != "string" && (t = t.toString()), t.replace(/(\d+)/g, function(t, r, u, f) {
            var e, o;
            return r && (e = Number(r), n.intl && e) ? o = i ? n.intl.n(e, i) : n.intl.n(e) : f
        })) : t
    }
}]);