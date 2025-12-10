// Tracking/ModalEvents.js
Roblox = Roblox || {}, Roblox.ModalEvents = function() {
    function n(n, t, i) {
        Roblox.EventStream && Roblox.EventStream.SendEvent(n, t, i)
    }

    function t(t, i) {
        var r = {
            referralButton: i
        };
        n("assetPurchaseConfirmationShown", t, r)
    }
    return {
        SendAssetPurchaseConfirmationShown: t
    }
}();