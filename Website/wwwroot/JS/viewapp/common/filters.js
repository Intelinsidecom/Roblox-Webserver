// ~/viewapp/common/filters.js
"use strict";
angular.module("robloxApp.filters", []).filter("getPercentage", function() {
    return function(n, t) {
        var i = n + t;
        return n * 100 / i + "%"
    }
}).filter("htmlToPlaintext", function() {
    return function(n) {
        return String(n).replace(/<[^>]+>/gm, "")
    }
}).filter("isEmpty", function() {
    return function(n, t, i) {
        return i === "" || i === null || typeof i == "undefined" ? t : n
    }
}).filter("positive", function() {
    return function(n) {
        return n ? Math.abs(n) : 0
    }
}).filter("startsWith", function() {
    return function(n, t, i) {
        var u = [],
            r, f;
        if (n)
            for (r = 0; r < n.length; r++) f = i ? n[r][i].toLowerCase() : n[r].toLowerCase(), f.indexOf(t.toLowerCase()) === 0 && t.length < f.length && u.push(n[r]);
        return u.length === 0 && (u = n), u
    }
}).filter("firstLetter", function() {
    return function(n) {
        return n != null ? n.substring(0, 1).toLowerCase() : ""
    }
}).filter("reverse", function() {
    return function(n) {
        if (n && n.length > 0) return n.slice().reverse()
    }
}).filter("orderList", function() {
    return function(n, t) {
        var i = [],
            r;
        for (r in t) i.push(n[t[r]]);
        return i
    }
}).filter("abbreviate", ["$filter", function(n) {
    var i = null,
        u = ["thousand", "million", "billion"],
        t = {
            thousand: 1e3,
            million: 1e6,
            billion: 1e9
        },
        f = {
            thousand: "K+",
            million: "M+",
            billion: "B+"
        },
        r = function(r, u) {
            return i && u === i ? n("number")(r) : n("number")((r / t[u]).toFixed(0), 0) + f[u]
        };
    return function(f, e) {
        return (typeof e != "undefined" && (i = u[e]), f < t.thousand * 10) ? n("number")(f) : f < t.million ? r(f, "thousand") : f < t.billion ? r(f, "million") : r(f, "billion")
    }
}]).filter("parseTimeStamp", function() {
    return function(n) {
        return n ? parseInt(typeof n == "string" && n.search("Date") > -1 ? n.slice(6, -2) : n) : null
    }
}).filter("capitalize", function() {
    return function(n) {
        var i, r, t, u;
        if (n != null) {
            for (i = n.split(" "), r = [], t = 0; t < i.length; t++) u = i[t].toLowerCase(), r.push(u.substring(0, 1).toUpperCase() + u.substring(1));
            return r.join(" ")
        }
    }
});