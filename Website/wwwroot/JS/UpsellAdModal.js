// UpsellAdModal.js
Roblox = Roblox || {}, typeof Roblox.UpsellAdModal == "undefined" && (Roblox.UpsellAdModal = function() {
    var n = function() {
        var n = {
            titleText: Roblox.UpsellAdModal.Resources.title,
            bodyContent: Roblox.UpsellAdModal.Resources.body,
            footerText: "",
            overlayClose: !0,
            escClose: !0,
            acceptText: Roblox.UpsellAdModal.Resources.accept,
            declineText: Roblox.UpsellAdModal.Resources.decline,
            acceptColor: Roblox.GenericConfirmation.green,
            onAccept: function() {
                window.location.href = "/premium/membership"
            },
            imageUrl: "/images/BuildersClub-110x110_small.png"
        };
        Roblox.GenericConfirmation.open(n)
    };
    return {
        open: n
    }
}()), $(function() {
    $("a.UpsellAdButton").click(function() {
        return Roblox.UpsellAdModal.open(), !1
    })
});