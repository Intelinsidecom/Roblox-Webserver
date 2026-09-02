// Events/PageHeartbeatEvent.js
typeof Freebloxia == "undefined" && (Freebloxia = {}), typeof Freebloxia.PageHeartbeatEvent == "undefined" && (Freebloxia.PageHeartbeatEvent = function() {
    var n = function(n) {
            Freebloxia.EventStream && Freebloxia.EventStream.SendEvent("pageHeartbeat", "heartbeat" + n, {})
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