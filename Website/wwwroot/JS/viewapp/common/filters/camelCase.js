// ~/viewapp/common/filters/camelCase.js
"use strict";
robloxApp.filter("camelCase", function() {
    return function(n) {
        return n.replace(/^([A-Z])|[\s-_]+(\w)/g, function(n, t, i) {
            return i ? i.toUpperCase() : t.toLowerCase()
        })
    }
});