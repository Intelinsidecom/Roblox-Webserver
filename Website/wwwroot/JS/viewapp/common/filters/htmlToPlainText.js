"use strict";

robloxFilters.filter("htmlToPlaintext", function() {
        return function(n) {
            return String(n).replace(/<[^>]+>/gm, "")
        }
    });