// Notifications/RealTime/Sources/hybridSource.js
typeof Roblox == "undefined" && (Roblox = {}), Roblox.RealTime = Roblox.RealTime || {}, Roblox.RealTime.Sources = Roblox.RealTime.Sources || {}, Roblox.RealTime.Sources.HybridSource = function(n, t) {
    var r, f, u, v, a = 5e3,
        p = 3e3,
        l = !0,
        i = function(n, i) {
            t && t("HybridSource: " + n, i)
        },
        h = function() {
            return Roblox && Roblox.Hybrid && Roblox.Hybrid.RealTime && Roblox.Hybrid.RealTime.supports ? Roblox.Hybrid.RealTime.isConnected && Roblox.Hybrid.RealTime.onNotification && Roblox.Hybrid.RealTime.onConnectionEvent ? Roblox.RealTime.Sources.HybridSourceDisabled ? (i("Roblox.Hybrid has previously told us it is not supported. Will not try again"), !1) : !0 : (i("Roblox.Hybrid.RealTime module does not provide all required methods. Cannot use Hybrid Source"), !1) : (i("Roblox.Hybrid or its RealTime module not present. Cannot use Hybrid Source"), !1)
        },
        c = function() {
            v = +new Date, setTimeout(function() {
                if (l) {
                    var n = +new Date;
                    n - v > a + p && (i("possible resume from suspension detected: polling for status"), e()), c()
                }
            }, a)
        },
        y = function() {
            l = !1
        },
        s = function(n) {
            if (!n || !n.params) {
                i("onNotification event without sufficient data");
                return
            }
            var t = {
                namespace: n.params.namespace || "",
                detail: JSON.parse(n.params.detail) || {},
                sequenceNumber: n.params.sequenceNumber || -1
            };
            i("Relaying parsed notification: " + JSON.stringify(t), !0), f(t)
        },
        o = function(n) {
            if (!n || !n.params) {
                i("onConnectionEvent event without sufficient data");
                return
            }
            i("ConnectionEvent received: " + JSON.stringify(n), !0), u({
                isConnected: n.params.isConnected || !1,
                sequenceNumber: n.params.sequenceNumber || -1
            })
        },
        w = function() {
            Roblox.Hybrid.RealTime.supports("isConnected", function(n) {
                n ? (i("Roblox.Hybrid.RealTime isConnected is supported. Subscribing to events"), Roblox.Hybrid.RealTime.onNotification.subscribe(s), Roblox.Hybrid.RealTime.onConnectionEvent.subscribe(o), e()) : (i("Roblox.Hybrid.RealTime isConnected not supported. Aborting attempt to use HybridSource"), Roblox.RealTime.Sources.HybridSourceDisabled = !0, r && r())
            })
        },
        b = function() {
            Roblox.Hybrid.RealTime.onNotification.unsubscribe(s), Roblox.Hybrid.RealTime.onConnectionEvent.unsubscribe(o)
        },
        e = function() {
            Roblox.Hybrid.RealTime.isConnected(function(n, t) {
                n && t ? (i("ConnectionStatus response received: " + JSON.stringify(t)), u({
                    isConnected: t.isConnected,
                    sequenceNumber: t.sequenceNumber || 0
                })) : (i("ConnectionStatus request failed! Aborting attempt to use HybridSource"), r && r())
            })
        },
        k = function() {
            i("Stopping. Detaching from native events"), b(), y()
        },
        d = function(n, t, e) {
            return (i("Starting"), !h()) ? !1 : (r = n, f = t, u = e, w(), c(), !0)
        };
    this.IsAvailable = h, this.Start = d, this.Stop = k, this.Name = "HybridSource"
};