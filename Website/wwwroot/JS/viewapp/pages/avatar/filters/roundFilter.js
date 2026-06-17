// ~/viewapp/pages/avatar/filters/roundFilter.js
"use strict";
avatar.filter("round", [function() {
    return function(n, t) {
        return t * Math.round(n / t)
    }
}]);