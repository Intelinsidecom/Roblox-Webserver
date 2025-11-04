// Events/UserInteractionsEvent.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.UserInteractionsEvent == "undefined" && (Roblox.UserInteractionsEvent = function() {
    var n = "mousemove touchstart",
        t = function(n) {
            Roblox.EventStream && Roblox.EventStream.SendEvent("userInteractions", n, {})
        },
        i = function(r) {
            r.type === "mousemove" ? t("mouse") : t("touch"), $.each(n.split(" "), function(n, t) {
                $(document).off(t, null, i)
            })
        },
        r = function() {
            $(document).on(n, i)
        };
    return {
        Init: r
    }
}(), Roblox.UserInteractionsEvent.Init());