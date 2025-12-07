// Notifications/RealTime/factory.js
typeof Roblox == "undefined" && (Roblox = {}), Roblox.RealTime = Roblox.RealTime || {}, Roblox.RealTime.Factory = function() {
    "use strict";
    var i = null,
        r = function() {
            return i === null && (i = u()), i
        },
        u = function() {
            var n = [];
            return Roblox.RealTime.Sources && (Roblox.RealTime.Sources.HybridSource && n.push(Roblox.RealTime.Sources.HybridSource), Roblox.RealTime.Sources.CrossTabReplicatedSource && n.push(Roblox.RealTime.Sources.CrossTabReplicatedSource), Roblox.RealTime.Sources.SignalRSource && n.push(Roblox.RealTime.Sources.SignalRSource)), new Roblox.RealTime.Client(n)
        },
        c = function(n) {
            var t = parseInt(n);
            return isNaN(t) ? null : t
        },
        n = null,
        t = function() {
            return n === null && (n = {}, Roblox && Roblox.RealTimeSettings ? (n.notificationsUrl = Roblox.RealTimeSettings.NotificationsEndpoint, n.maxConnectionTimeInMs = parseInt(Roblox.RealTimeSettings.MaxConnectionTime), n.isEventPublishingEnabled = Roblox.RealTimeSettings.IsEventPublishingEnabled, n.isDisconnectOnSlowConnectionDisabled = Roblox.RealTimeSettings.IsDisconnectOnSlowConnectionDisabled, n.userId = parseInt(Roblox.RealTimeSettings.UserId) || -1, n.isSignalRClientTransportRestrictionEnabled = Roblox.RealTimeSettings.IsSignalRClientTransportRestrictionEnabled, n.isLocalStorageEnabled = Roblox.RealTimeSettings.IsLocalStorageInRealTimeEnabled) : (n.notificationsUrl = null, n.maxConnectionTimeInMs = 216e5, n.isEventPublishingEnabled = !1, n.isDisconnectOnSlowConnectionDisabled = !1, n.userId = -1, n.isSignalRClientTransportRestrictionEnabled = !1, n.isLocalStorageEnabled = !1)), n
        },
        f = function() {
            return t().notificationsUrl
        },
        e = function() {
            return t().maxConnectionTimeInMs
        },
        o = function() {
            return t().isEventPublishingEnabled
        },
        s = function() {
            return localStorage && t().isLocalStorageEnabled
        },
        h = function() {
            return t().userId
        };
    return {
        GetClient: r,
        GetNotificationsUrl: f,
        GetMaximumConnectionTime: e,
        IsEventPublishingEnabled: o,
        IsLocalStorageEnabled: s,
        GetUserId: h,
        GetSettings: t
    }
}(), Roblox.RealTime.Events = {
    Notification: "Roblox.RealTime.Events.Notification",
    ConnectionEvent: "Roblox.RealTime.Events.ConnectionEvent",
    RequestForConnectionStatus: "Roblox.RealTime.Events.RequestForConnectionStatus"
};