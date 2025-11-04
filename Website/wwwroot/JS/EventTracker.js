// EventTracker.js
EventTracker = new function() {
    var n = this;
    n.logMetrics = !1, n.transmitMetrics = !0, n.localEventLog = [];
    var t = new function() {
            var n = {};
            this.get = function(t) {
                return n[t]
            }, this.set = function(t, i) {
                n[t] = i
            }, this.remove = function(t) {
                delete n[t]
            }
        },
        r = function() {
            return (new Date).valueOf()
        },
        i = function(n, t) {
            var i = r();
            $.each(n, function(n, r) {
                u(r, t, i)
            })
        },
        u = function(i, r, u) {
            var e = t.get(i),
                f, o;
            e ? (t.remove(i), f = u - e, n.logMetrics && console.log("[event]", i, r, f), n.transmitMetrics && (o = i + "_" + r, $.ajax({
                type: "POST",
                timeout: 5e4,
                url: "/game/report-stats?name=" + o + "&value=" + f,
                crossDomain: !0,
                xhrFields: {
                    withCredentials: !0
                }
            }))) : n.logMetrics && console.log("[event]", "ERROR: event not started -", i, r)
        };
    n.start = function() {
        var n = r();
        $.each(arguments, function(i, r) {
            t.set(r, n)
        })
    }, n.endSuccess = function() {
        i(arguments, "Success")
    }, n.endCancel = function() {
        i(arguments, "Cancel")
    }, n.endFailure = function() {
        i(arguments, "Failure")
    }, n.fireEvent = function() {
        $.each(arguments, function(t, i) {
            $.ajax({
                type: "POST",
                timeout: 5e4,
                url: "/game/report-event?name=" + i,
                crossDomain: !0,
                xhrFields: {
                    withCredentials: !0
                }
            }), n.logMetrics && console.log("[event]", i), n.localEventLog.push(i)
        })
    }
};