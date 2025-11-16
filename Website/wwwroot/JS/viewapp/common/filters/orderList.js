"use strict";

robloxFilters.filter("orderList", function() {
        return function(n, t) {
            var i=[], r; for(r in t)i.push(n[t[r]]); return i
        }
    });