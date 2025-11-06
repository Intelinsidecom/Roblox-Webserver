// common/forms.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.Forms == "undefined" && (Roblox.Forms = function() {
    function n() {
        var n = document.createElement("input");
        return "autofocus" in n
    }
    n() || $(function() {
        $("input[autofocus='']").first().focus()
    })
}());