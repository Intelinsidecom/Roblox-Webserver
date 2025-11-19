// ~/viewapp/common/filters/formatString.js
"use strict";
robloxApp.filter("formatString", function() {
    return function(n) {
        var t, i, r, u, f;
        if (arguments && arguments.length > 1) {
            t = arguments[0], i = arguments[1];
            for (r in i) u = i[r], f = new RegExp("{" + r + "(:.*?)?\\??}"), t = t.replace(f, u);
            return t
        }
        return n
    }
});