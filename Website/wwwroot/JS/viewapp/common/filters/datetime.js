// ~/viewapp/common/filters/datetime.js
"use strict";
robloxApp.filter("datetime", function() {
    return function(n, t) {
        var i, r;
        return i = n && n !== null ? typeof n == "string" || typeof n == "number" ? new Date(n) : n : "Invalid Date", i.toString() !== "Invalid Date" ? (r = (new Roblox.Intl).getDateTimeFormatter(), t === "full" ? r.getFullDate(i) : t === "short" ? r.getShortDate(i) : t ? r.getCustomDateTime(i, t) : r.getShortDate(i)) : n
    }
});