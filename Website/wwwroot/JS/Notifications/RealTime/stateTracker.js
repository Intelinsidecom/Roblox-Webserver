// Notifications/RealTime/stateTracker.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.RealTime == "undefined" && (Roblox.RealTime = {}), Roblox.RealTime.StateTracker = function(n, t, i) {
    function c(n) {
        typeof i == "function" && i(n)
    }

    function l() {
        return y + Roblox.RealTime.Factory.GetUserId()
    }

    function p() {
        if (c("StateTracker Initialized"), n) {
            var t = localStorage.getItem(l());
            t && (o = a(t))
        }
    }

    function s(t) {
        o = {
            SequenceNumber: t,
            TimeStamp: Date.now()
        }, n && localStorage.setItem(l(), JSON.stringify(o))
    }

    function a(n) {
        try {
            return JSON.parse(n)
        } catch (t) {
            return c("Error parsing jsonString"), null
        }
    }

    function e(n, i, r) {
        try {
            t && Roblox.EventStream && (typeof r != "object" && (r = {}), r.ua = navigator.userAgent, Roblox.EventStream.SendEvent(n, i, r))
        } catch (u) {
            c("Error pushing to Event Stream")
        }
    }
    var r = {
            IS_REQUIRED: 1,
            NOT_REQUIRED: 2,
            UNCLEAR: 3
        },
        y = "Roblox.RealTime.StateTracker.LastSequenceNumberProcessed_U_",
        u = {
            RealTimeCheckIfDataReloadRequired: "realTimeCheckIfDataReloadRequired",
            RealTimeUpdateLatestSequenceNumber: "realTimeUpdateLatestSequenceNumber"
        },
        f = {
            OutOfOrder: "SequenceOutOfOrder",
            MissedNumber: "SequenceNumberMissed",
            UpToDate: "SequenceNumberMatched",
            TimeExpired: "TimeStampExpired",
            InvalidSequenceNumber: "InvalidSequenceNumber",
            MissingNotificationInfo: "MissingNotificationInformation"
        },
        o = null,
        h = function() {
            return o
        },
        v = function(n) {
            if (typeof n != "number") return e(u.RealTimeCheckIfDataReloadRequired, f.InvalidSequenceNumber, {
                rld: !0
            }), r.UNCLEAR;
            if (n <= 0) return r.UNCLEAR;
            var t = h();
            return typeof t == "undefined" || t == null ? (e(u.RealTimeCheckIfDataReloadRequired, f.MissingNotificationInfo, {
                rld: !0
            }), s(n), r.UNCLEAR) : n === t.SequenceNumber ? (s(n), e(u.RealTimeCheckIfDataReloadRequired, f.UpToDate, {
                rld: !1
            }), r.NOT_REQUIRED) : (e(u.RealTimeCheckIfDataReloadRequired, f.MissedNumber, {
                rld: !0
            }), n > t.SequenceNumber ? (s(n), e(u.RealTimeCheckIfDataReloadRequired, f.OutOfOrder, {
                rld: !0
            }), r.IS_REQUIRED) : r.UNCLEAR)
        },
        w = function(n) {
            if (typeof n != "number") {
                e(u.RealTimeUpdateLatestSequenceNumber, f.InvalidSequenceNumber);
                return
            }
            var t = h();
            typeof t == "object" && t != null && t.SequenceNumber > n && e(u.RealTimeUpdateLatestSequenceNumber, f.OutOfOrder), s(n)
        };
    p(), this.IsDataRefreshRequired = v, this.UpdateSequenceNumber = w, this.GetLatestState = h, this.RefreshRequiredEnum = r
};