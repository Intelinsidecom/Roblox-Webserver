// Tracking/SignupTrackingScript.js
var Roblox = Roblox || {};
Roblox.SignupTrackingScript = function() {
    function n(n, t) {
        RobloxEventManager.triggerEvent("rbx_evt_signup", {
            age: n,
            gender: t
        })
    }
    return {
        trackingScript: n
    }
}();