// ~/viewapp/common/filters/translate.js
"use strict";
robloxApp.filter("translate", ["languageResource", "$log", function(n, t) {
    return function(i, r, u) {
        var f = n.get(i, r, u);
        return f ? f : (t.debug("Unable to translate key:" + i), "")
    }
}]);