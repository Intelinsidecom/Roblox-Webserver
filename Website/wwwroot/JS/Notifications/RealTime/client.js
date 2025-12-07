// Notifications/RealTime/client.js
typeof Roblox == "undefined" && (Roblox = {}), Roblox.RealTime = Roblox.RealTime || {}, Roblox.RealTime.Client = function(n) {
    var i = null,
        r = !1,
        v = !1,
        f = {},
        e = [],
        l = [],
        h = [],
        s = null,
        c = !1,
        t = function(n, t) {
            n = "RealTime Client: " + n, (!t || c) && s && s(n)
        },
        u = null,
        k = function(n) {
            s = n
        },
        nt = function(n) {
            c = n
        },
        a = function() {
            var e, f, u, s;
            for (i && (t("Stopping current source: " + i.Name), i.Stop(), i = null, r && (r = !1, o())), e = Roblox.RealTime.Factory.GetSettings(), f = 0; f < n.length; f++)
                if (u = new n[f](e, t), t("Attempting to start a new source: " + u.Name), s = u.Start(a, function(n) {
                        d(u, n)
                    }, function(n) {
                        it(u, n)
                    }), s) {
                    t("New source started: " + u.Name), i = u;
                    break
                } i === null && t("No source can be started!")
        },
        g = function() {
            Roblox.RealTime.StateTracker && (u = new Roblox.RealTime.StateTracker(Roblox.RealTime.Factory.IsLocalStorageEnabled(), Roblox.RealTime.Factory.IsEventPublishingEnabled())), a(), Roblox && Roblox.Performance && Roblox.Performance.setPerformanceMark("signalR_initialized")
        },
        d = function(n, r) {
            var e, o;
            if (n === i) {
                if (e = f[r.namespace], e)
                    for (o = 0; o < e.length; o++) try {
                        e[o](r.detail)
                    } catch (s) {
                        t("Error running subscribed event handler for notification [" + r.namespace + "]:" + s)
                    }
                u && u.UpdateSequenceNumber(r.sequenceNumber)
            }
        },
        it = function(n, f) {
            var e, s, h;
            n === i && (f.isConnected ? (e = u ? u.IsDataRefreshRequired(f.sequenceNumber) : null, r && e === u.RefreshRequiredEnum.IS_REQUIRED && (t("Have detected messages were missed. Triggering reconnect logic. Data Reload Required: " + e), r = !1, o()), r || (r = !0, v ? w(e === null || e === u.RefreshRequiredEnum.IS_REQUIRED || e === u.RefreshRequiredEnum.UNCLEAR) : (s = e !== u.RefreshRequiredEnum.NOT_REQUIRED, v = !0, Roblox && Roblox.Performance && (h = "signalR_" + i.Name + "_connected", Roblox.Performance.logSinglePerformanceMark(h)), b(s)))) : r && (r = !1, o()))
        },
        b = function(n) {
            t("Client Connected!");
            for (var i = 0; i < e.length; i++) try {
                e[i](n)
            } catch (r) {
                t("Error running subscribed event handler for connected:" + r)
            }
        },
        w = function(n) {
            t("Client Reconnected! Data Reload Required: " + n);
            for (var i = 0; i < h.length; i++) try {
                h[i](n)
            } catch (r) {
                t("Error running subscribed event handler for reconnected:" + r)
            }
        },
        o = function() {
            t("Client Disconnected!");
            for (var n = 0; n < e.length; n++) try {
                l[n]()
            } catch (i) {
                t("Error running subscribed event handler for disconnected:" + i)
            }
        },
        p = function(n, t) {
            f[n] || (f[n] = []);
            var i = f[n];
            i.push(t)
        },
        y = function(n, t) {
            if (f[n]) {
                var i = f[n],
                    r = i.indexOf(t);
                r >= 0 && i.splice(r, 1)
            }
        },
        tt = function() {
            return r
        },
        rt = function(n, t, i) {
            n && e.push(n), t && h.push(t), i && l.push(i)
        };
    g(), this.Subscribe = p, this.Unsubscribe = y, this.SubscribeToConnectionEvents = rt, this.IsConnected = tt, this.SetLogger = k, this.SetVerboseLogging = nt
};