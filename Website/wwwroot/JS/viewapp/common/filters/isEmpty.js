"use strict";

robloxFilters.filter("isEmpty", function() {
        return function(n, t, i) {
            return i==="" ||i===null||typeof i=="undefined" ?t:n
        }
    });