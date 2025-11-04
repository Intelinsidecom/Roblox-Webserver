// IEMetroInstructions.js
(function() {
    function i() {
        return navigator.userAgent.indexOf("MSIE 10.0") != -1 || navigator.userAgent.toLowerCase().indexOf("trident") != -1 && /rv[: ]\d+/.test(navigator.userAgent)
    }

    function r() {
        try {
            return !!new ActiveXObject("htmlfile")
        } catch (n) {
            return !1
        }
    }
    var u = Roblox.Client.WaitForRoblox;
    Roblox.Client.WaitForRoblox = function(n) {
        return i() && !r() ? ($("#IEMetroInstructions").modal({
            overlayClose: !0,
            escClose: !0,
            opacity: 80,
            overlayCss: {
                backgroundColor: "#000"
            }
        }), !1) : u(n)
    }
})(window);