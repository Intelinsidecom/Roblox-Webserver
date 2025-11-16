"use strict";

robloxFilters.filter("parseTimeStamp", function() {
        return function(n) {
            return n?typeof n=="string" &&n.search("Date")>-1?parseInt(n.slice(6, -2)):new Date(n):null
        }
    });