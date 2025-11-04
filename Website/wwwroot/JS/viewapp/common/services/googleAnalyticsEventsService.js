// ~/viewapp/common/services/googleAnalyticsEventsService.js
robloxAppService.factory("googleAnalyticsEventsService", function() {
    function n(n, t, i, r) {
        var u = [];
        return u.push(n), u.push(t), u.push(i), isNaN(r) || (r = Math.floor(r), u.push(r)), u
    }
    return {
        eventCategories: {
            JSErrors: "JSErrors"
        },
        eventActions: {
            Chat: "Chat",
            ChatEmbedded: "ChatEmbedded"
        },
        getUserAgent: function() {
            return navigator && navigator.userAgent ? navigator.userAgent : ""
        },
        fireEvent: function(t, i, r, u) {
            var f = n(t, i, r, u);
            GoogleAnalyticsEvents.FireEvent(f)
        }
    }
});