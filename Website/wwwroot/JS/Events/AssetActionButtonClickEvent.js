// Events/AssetActionButtonClickEvent.js
var Roblox = Roblox || {};
Roblox.AssetActionButtonClickEvent = function() {
    var n = "data-button-action",
        t = function(t) {
            var i = t.attr(n);
            i && Roblox.EventStream.SendEvent("assetActionButtonClick", i, {})
        },
        i = function() {
            if (Roblox.EventStream) $("a[" + n + "], button[" + n + "]").on("click", function() {
                t($(this))
            })
        };
    return {
        Init: i
    }
}(), $(function() {
    Roblox.AssetActionButtonClickEvent.Init()
});