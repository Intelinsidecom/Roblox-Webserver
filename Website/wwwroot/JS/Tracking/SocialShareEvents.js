// Tracking/SocialShareEvents.js
Roblox = Roblox || {}, Roblox.SocialShareEvents = function() {
    function n(n, t, i) {
        Roblox.EventStream && Roblox.EventStream.SendEvent(n, t, i)
    }

    function t(t, i, r, u, f) {
        f = f || "gigya";
        var e = {
            shareType: i,
            shareItemId: r,
            shareTarget: u,
            shareWidget: f
        };
        n("socialShareIntent", t, e)
    }
    return {
        SendSocialShareIntentEvent: t
    }
}();