"use strict";

robloxFilters.filter("positive", function() {
        return function(n) {
            return n?Math.abs(n):0
        }
    });