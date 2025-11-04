// Events/PageHeartbeatEvent.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.PageHeartbeatEvent == "undefined" && (Roblox.PageHeartbeatEvent = function() {
    var n = function(n) {
            Roblox.EventStream && Roblox.EventStream.SendEvent("pageHeartbeat", "heartbeat" + n, {})
        },
        t = function(t) {
            if (t) {
                var i = 0;

                function r() {
                    if (t.length && i < t.length) {
                        var u = t[i++];
                        setTimeout(function() {
                            n(i), r()
                        }, u * 1e3)
                    }
                }
                r()
            }
        },
        i = function(n) {
            t(n)
        };
    return {
        Init: i
    }
}());