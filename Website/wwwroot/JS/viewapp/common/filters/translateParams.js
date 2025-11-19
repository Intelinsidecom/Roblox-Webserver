// ~/viewapp/common/filters/translateParams.js
"use strict";
robloxApp.filter("translateParams", ["languageResource", "$log", function(n, t) {
    return function(i, r, u) {
        var s = {},
            f, e, o;
        for (f in r) {
            if (e = n.get(r[f]), !e) return t.debug("Unable to translate key:" + r[f]), "";
            s[f] = e
        }
        return (o = n.get(i, s, u), !o) ? (t.debug("Unable to translate key:" + i), "") : o
    }
}]);