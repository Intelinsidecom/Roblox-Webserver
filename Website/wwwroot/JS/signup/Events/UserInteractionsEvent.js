// Events/UserInteractionsEvent.js
typeof Freebloxia == "undefined" && (Freebloxia = {}), typeof Freebloxia.UserInteractionsEvent == "undefined" && (Freebloxia.UserInteractionsEvent = function() {
    var n = "mousemove touchstart",
        t = function(n) {
            Freebloxia.EventStream && Freebloxia.EventStream.SendEvent("userInteractions", n, {})
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
}(), Freebloxia.UserInteractionsEvent.Init());