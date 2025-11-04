// GoogleAnalytics/GoogleAnalyticsEvents.js
function makeGoogleAnalyticsLogObject(n) {
    var t = {};
    return t.event = n, t.timestamp = +new Date, t
}

function GoogleAnalyticsTimingTracker(n, t, i, r) {
    this.maxTime = 6e4, this.category = n, this.variable = t, this.label = i ? i : undefined, this.isDebug = r
}
var GoogleAnalyticsEvents = {
    LocalEventLog: [],
    SetCustomVar: function(n, t, i, r) {
        window._gaq && (window.GoogleAnalyticsDisableRoblox2 || _gaq.push(["_setCustomVar", n, t, i, r]), _gaq.push(["b._setCustomVar", n, t, i, r]))
    },
    FireEvent: function(n) {
        var t, i;
        window._gaq && (window.GoogleAnalyticsDisableRoblox2 || (t = ["_trackEvent"], t = t.concat(n), _gaq.push(t), GoogleAnalyticsEvents.LocalEventLog.push(makeGoogleAnalyticsLogObject(t))), i = ["b._trackEvent"], i = i.concat(n), _gaq.push(i), GoogleAnalyticsEvents.LocalEventLog.push(makeGoogleAnalyticsLogObject(i)))
    },
    ViewVirtual: function(n) {
        var t, i;
        window._gaq && (window.GoogleAnalyticsDisableRoblox2 || (t = ["_trackPageview", n], window._gaq.push(t), GoogleAnalyticsEvents.LocalEventLog.push(makeGoogleAnalyticsLogObject(t))), i = ["b._trackPageview", n], window._gaq.push(i), GoogleAnalyticsEvents.LocalEventLog.push(makeGoogleAnalyticsLogObject(i)))
    },
    TrackTransaction: function(n, t) {
        if (window._gaq) {
            var i = ["_addTrans", n, "Roblox", t, "0", "0", "San Mateo", "California", "USA"];
            window.GoogleAnalyticsDisableRoblox2 || (_gaq.push(i), GoogleAnalyticsEvents.LocalEventLog.push(makeGoogleAnalyticsLogObject(i))), i[0] = "b." + i[0], _gaq.push(i), GoogleAnalyticsEvents.LocalEventLog.push(makeGoogleAnalyticsLogObject(i))
        }
    },
    TrackTransactionItem: function(n, t, i, r, u) {
        if (window._gaq) {
            var f = ["_addItem", n, t, i, r, u, 1],
                e = ["_trackTrans"];
            window.GoogleAnalyticsDisableRoblox2 || (_gaq.push(f), GoogleAnalyticsEvents.LocalEventLog.push(makeGoogleAnalyticsLogObject(f)), _gaq.push(e), GoogleAnalyticsEvents.LocalEventLog.push(makeGoogleAnalyticsLogObject(e))), f[0] = "b." + f[0], e[0] = "b." + e[0], _gaq.push(f), GoogleAnalyticsEvents.LocalEventLog.push(makeGoogleAnalyticsLogObject(f)), _gaq.push(e), GoogleAnalyticsEvents.LocalEventLog.push(makeGoogleAnalyticsLogObject(e))
        }
    },
    Log: function(n) {
        GoogleAnalyticsEvents.LocalEventLog.push(makeGoogleAnalyticsLogObject(n))
    }
};
GoogleAnalyticsTimingTracker.prototype.getTimeStamp = function() {
    return window.performance && window.performance.now ? Math.round(window.performance.now()) : +new Date
}, GoogleAnalyticsTimingTracker.prototype.start = function() {
    this.startTime = this.getTimeStamp()
}, GoogleAnalyticsTimingTracker.prototype.stop = function() {
    this.elapsedTime = this.getTimeStamp() - this.startTime
}, GoogleAnalyticsTimingTracker.prototype.send = function() {
    if (0 < this.elapsedTime && this.elapsedTime < this.maxTime) {
        var n = ["b._trackTiming", this.category, this.variable, this.elapsedTime, this.label, 100];
        window._gaq.push(n)
    }
};