"use strict";

robloxFilters.filter("firstLetter", function() {
        return function(n) {
            return n !=null?n.substring(0, 1).toLowerCase():""
        }
    });