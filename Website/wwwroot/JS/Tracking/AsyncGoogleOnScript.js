// Tracking/AsyncGoogleOnScript.js
var Roblox = Roblox || {};
Roblox.AsyncGoogleOnScript = function() {
    function n(n, t) {
        GoogleAnalyticsEvents && GoogleAnalyticsEvents.FireEvent(["Signup", "Signup - Roblox", n, t])
    }
    return {
        googleGoalFire: n
    }
}();