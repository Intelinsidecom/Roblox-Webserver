"use strict";

robloxFilters.filter("getPercentage", function() {
        return function(n, t) {
            var i=n+t; return n*100/i+"%"
        }
    });