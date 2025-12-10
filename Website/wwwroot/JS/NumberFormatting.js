// NumberFormatting.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.NumberFormatting == "undefined" && (Roblox.NumberFormatting = function() {
    var n = function(n) {
            if (typeof n != "number") throw "'number' is not a number";
            return n.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",")
        },
        t = function(t) {
            var i, r, u;
            if (typeof t != "number") throw "'number' is not a number";
            var f = 1e4,
                e = 1e6,
                o = 1e9;
            return t == 0 ? "0" : t < f ? n(t) : (i = "B+", r = 9, t < e ? (i = "K+", r = 3) : t < o && (i = "M+", r = 6), u = t.toString(), u.substring(0, u.length - r) + i)
        };
    return {
        abbreviatedFormat: t,
        commas: n
    }
}());