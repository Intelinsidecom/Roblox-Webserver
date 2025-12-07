// Notifications/RealTime/signalRConnectionWrapper.js
typeof Roblox == "undefined" && (Roblox = {}), Roblox.RealTime = Roblox.RealTime || {}, Roblox.RealTime.SignalRConnectionWrapper = function(n, t, i, r, u) {
    function v() {
        f = k(), f.start(l()).done(function() {
            e("Connected to SignalR [" + f.transport.name + "]")
        }).fail(function(n) {
            e("FAILED to connect to SignalR [" + n + "]")
        })
    }

    function p() {
        f && ($(f).unbind(), f.stop(), f = null), i(!1)
    }

    function w() {
        f === null ? v() : f.stop()
    }

    function b() {
        return s
    }

    function k() {
        var f = n.notificationsUrl,
            t = $.hubConnection(f + "/notifications", {
                useDefaultPath: !1
            }),
            i = t.createHubProxy("userNotificationHub");
        i.on("notification", r);
        i.on("subscriptionStatus", u);
        return t.stateChanged(g), t.disconnected(nt), t.reconnecting(tt), t
    }

    function d() {
        return window.WebSocket ? ["webSockets"] : ["webSockets", "longPolling"]
    }

    function l() {
        var t = {
            pingInterval: null
        };
        return n.isSignalRClientTransportRestrictionEnabled && (t.transport = d()), t
    }

    function g(n) {
        n.newState === c.connected ? (s = !0, i(!0)) : n.oldState === c.connected && s && (s = !1, i(!1)), e("Connection Status changed from [" + a[n.oldState] + "] to [" + a[n.newState] + "]")
    }

    function nt() {
        var n = h.StartNewAttempt();
        e("In disconnected handler. Will attempt Reconnect after " + n + "ms"), setTimeout(function() {
            (e("Attempting to Reconnect [" + h.GetAttemptCount() + "]..."), f != null) && f.start(l()).done(function() {
                h.Reset(), e("Connected Again!")
            }).fail(function() {
                e("Failed to Reconnect!")
            })
        }, n)
    }

    function tt() {
        e("In reconnecting handler. Attempt to force disconnect."), f.stop()
    }

    function y() {
        var n = new Roblox.Utilities.ExponentialBackoffSpecification({
                firstAttemptDelay: 2e3,
                firstAttemptRandomnessFactor: 3,
                subsequentDelayBase: 1e4,
                subsequentDelayRandomnessFactor: .5,
                maximumDelayBase: 3e5
            }),
            t = new Roblox.Utilities.ExponentialBackoffSpecification({
                firstAttemptDelay: 2e4,
                firstAttemptRandomnessFactor: .5,
                subsequentDelayBase: 4e4,
                subsequentDelayRandomnessFactor: .5,
                maximumDelayBase: 3e5
            }),
            i = 6e4,
            r = function(n) {
                var t = n.GetLastResetTime();
                return t && t + i > +new Date ? !0 : !1
            };
        return new Roblox.Utilities.ExponentialBackoff(n, r, t)
    }

    function e(n) {
        t && t(n)
    }
    var o = this;
    o.Start = v, o.Stop = p, o.Restart = w, o.IsConnected = b;
    var a = {
            0: "connecting",
            1: "connected",
            2: "reconnecting",
            4: "disconnected"
        },
        c = {
            connecting: 0,
            connected: 1,
            reconnecting: 2,
            disconnected: 4
        },
        f = null,
        s = !1,
        h = y()
};