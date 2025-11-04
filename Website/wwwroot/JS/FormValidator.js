// FormValidator.js
typeof Roblox == "undefined" && (Roblox = {}), Roblox.FormValidator = function() {
    function n(n) {
        var i = $(n).data("regex"),
            r = $(n).val();
        return t(r, i)
    }

    function t(n, t) {
        if (typeof n == "undefined" || typeof t == "undefined") return !1;
        var i = new RegExp(t, "i");
        return i.test(n)
    }
    return {
        validateElementRegex: n
    }
}();