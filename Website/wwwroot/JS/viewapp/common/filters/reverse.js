"use strict";

robloxFilters.filter("reverse", function() {
        return function(n) {
            if(n&&n.length>0)return n.slice().reverse()
        }
    });