// SignalR/SignalRAbortAsyncPatch-2.2.0.js
(function() {
    "use strict";
    var n, t;
    $.signalR && $.signalR.version === "2.2.0" && (n = $.signalR.transports.webSockets.abort, $.signalR.transports.webSockets.abort = function(t) {
        n(t, !0)
    }, t = $.signalR.transports.longPolling.abort, $.signalR.transports.longPolling.abort = function(n) {
        t(n, !0)
    })
})();