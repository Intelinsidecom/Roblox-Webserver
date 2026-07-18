// Notifications/RealTime/signalRConnectionWrapper.js
// Rewritten to use ASP.NET Core SignalR client (@microsoft/signalr)
typeof Roblox == "undefined" && (Roblox = {}), Roblox.RealTime = Roblox.RealTime || {}, Roblox.RealTime.SignalRConnectionWrapper = function(n, t, i, r, u) {
    function v() {
        if (s) return;
        var url = n.notificationsUrl;
        if (!url) { e("No notificationsUrl configured"); return; }
        var hubUrl = url.replace(/\/+$/, "") + "/hubs/notifications";
        e("Connecting to SignalR hub: " + hubUrl);

        f = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, { withCredentials: true })
            .withAutomaticReconnect([0, 2000, 10000, 30000, null])
            .configureLogging(signalR.LogLevel.Warning)
            .build();

        f.on("notification", function() {
            var args = Array.prototype.slice.call(arguments);
            r(args[0], args[1], args[2]);
        });

        f.on("subscriptionStatus", function() {
            var args = Array.prototype.slice.call(arguments);
            u(args[0], args[1]);
        });

        f.onreconnecting(function() {
            e("SignalR reconnecting...");
        });

        f.onreconnected(function() {
            e("SignalR reconnected");
        });

        f.onclose(function(err) {
            e("SignalR connection closed: " + (err ? err.message : "clean"));
            s = false;
            i(false);
        });

        f.start().then(function() {
            s = true;
            e("Connected to SignalR via new client");
            i(true);
        }).catch(function(err) {
            e("FAILED to connect to SignalR: " + err);
        });
    }

    function p() {
        if (f) {
            f.stop().catch(function() {});
            f = null;
        }
        s = false;
        i(false);
    }

    function w() {
        f === null ? v() : p();
    }

    function b() {
        return s;
    }

    function e(n) {
        t && t(n);
    }

    var o = this;
    o.Start = v, o.Stop = p, o.Restart = w, o.IsConnected = b;
    var f = null,
        s = false;
};
