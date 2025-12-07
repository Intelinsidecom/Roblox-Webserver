// Notifications/RealTime/Sources/signalRSource.js
var Roblox = Roblox || {};
Roblox.RealTime = Roblox.RealTime || {}, Roblox.RealTime.Sources = Roblox.RealTime.Sources || {}, Roblox.RealTime.Sources.SignalRSource = function(n, t) {
    var it = function() {
            return !0
        },
        c = {
            connectionLost: "ConnectionLost",
            reconnected: "Reconnected",
            subscribed: "Subscribed"
        },
        v, a, l, w = !1,
        u = !1,
        s = null,
        h = !1,
        f = null,
        g = 2e3,
        o = -1,
        r = null,
        i = function(n, i) {
            t && t("SignalRSource: " + n, i)
        },
        nt = function() {
            if (!Roblox.CrossTabCommunication || !Roblox.CrossTabCommunication.Kingmaker || !Roblox.CrossTabCommunication.PubSub) {
                i("CrossTabCommunication dependencies required for replication are not present - will not replicate notifications"), u = !1;
                return
            }
            Roblox.CrossTabCommunication.Kingmaker.SubscribeToMasterChange(function(n) {
                u = n, n || v()
            }), u = Roblox.CrossTabCommunication.Kingmaker.IsMasterTab(), Roblox.CrossTabCommunication.PubSub.Subscribe(Roblox.RealTime.Events.RequestForConnectionStatus, "Roblox.RealTime.Sources.SignalRSource", function() {
                if (u) {
                    var n = {
                        isConnected: w,
                        sequenceNumber: o
                    };
                    i("Responding to request for connection status: " + JSON.stringify(n)), Roblox.CrossTabCommunication.PubSub.Publish(Roblox.RealTime.Events.ConnectionEvent, JSON.stringify(n))
                }
            })
        },
        rt = function(n, t, r) {
            var f = {
                namespace: n,
                detail: JSON.parse(t),
                sequenceNumber: r
            };
            i("Notification received: " + JSON.stringify(f), !0), o = r || -1, a(f), u && (i("Replicating Notification"), Roblox.CrossTabCommunication.PubSub.Publish(Roblox.RealTime.Events.Notification, JSON.stringify(f)))
        },
        e = function(n, t) {
            w = n;
            var r = {
                isConnected: n
            };
            t ? (r.sequenceNumber = t, o = t) : o = -1, i("Sending Connection Event: " + JSON.stringify(r)), l(r), u && (i("Replicating Connection Event."), Roblox.CrossTabCommunication.PubSub.Publish(Roblox.RealTime.Events.ConnectionEvent, JSON.stringify(r)))
        },
        b = function() {
            $(window).unbind("focus.enforceMaxTimeout"), s !== null && (clearTimeout(s), s = null)
        },
        p = function() {
            b(), s = setTimeout(function() {
                e(!1), r.Stop(), $(window).unbind("focus.enforceMaxTimeout").bind("focus.enforceMaxTimeout", function() {
                    r.Start(), p()
                })
            }, n.maxConnectionTimeInMs)
        },
        y = function(n) {
            f !== null && (clearTimeout(f), f = null), n.MillisecondsBeforeHandlingReconnect > 0 ? (i("Waiting " + n.MillisecondsBeforeHandlingReconnect + "ms to send Reconnected signal"), setTimeout(function() {
                r.IsConnected() && e(!0, n.SequenceNumber)
            }, n.MillisecondsBeforeHandlingReconnect)) : r.IsConnected() && e(!0, n.SequenceNumber)
        },
        k = function(n, t) {
            try {
                i("Status Update Received: [" + n + "]" + t)
            } catch (f) {}
            if (n === c.connectionLost) i("Server Backend Connection Lost!"), r.Restart();
            else if (n === c.reconnected) i("Server reconnected"), y(JSON.parse(t));
            else if (n === c.subscribed) {
                var u = JSON.parse(t);
                i("Server connected"), h || (h = !0, u.MillisecondsBeforeHandlingReconnect = 0), y(u)
            }
        },
        d = function(n) {
            n ? f = setTimeout(function() {
                f = null, r.IsConnected() && (h = !0, e(!0))
            }, g) : e(!1)
        },
        tt = function(u, f, e) {
            return v = u, a = f, l = e, nt(), r = new Roblox.RealTime.SignalRConnectionWrapper(n, t, d, rt, k), r.Start(), p(), i("Started"), !0
        },
        ut = function() {
            b(), r && r.Stop()
        };
    this.IsAvailable = it, this.Start = tt, this.Stop = ut, this.Name = "SignalRSource"
};