// Thumbnails/thumbnailMetrics.js
var Roblox = Roblox || {};
Roblox.ThumbnailMetrics = function () {
    function c(n) {
        i = parseFloat(n)
    }

    function l() {
        return Math.random() <= i
    }

    function a() {
        t.forEach(function (n) {
            n()
        })
    }

    function e() {
        $.ajax({
            type: "GET",
            url: h,
            crossDomain: !0,
            xhrFields: {
                withCredentials: !0
            },
            success: function (t) {
                t && (i = t.logRatio), n = !0, a()
            },
            error: function () {
                n = !0
            }
        })
    }

    function o(i) {
        if (!n) return t.push(function () {
            o(i)
        }), !1;
        if (!l()) return !1;
        $.ajax({
            type: "POST",
            timeout: 3e3,
            url: u,
            crossDomain: !0,
            xhrFields: {
                withCredentials: !0
            },
            data: {
                duration: i,
                loadState: f.complete
            }
        })
    }

    function s() {
        if (!n) return t.push(s), !1;
        $.ajax({
            type: "POST",
            timeout: 3e3,
            url: u,
            crossDomain: !0,
            xhrFields: {
                withCredentials: !0
            },
            data: {
                loadState: f.timeout
            }
        })
    }
    var r = Roblox.EnvironmentUrls && Roblox.EnvironmentUrls.metricsApi || "https://metrics.roblox.com",
        u = r + "/v1/thumbnails/load",
        h = r + "/v1/thumbnails/metadata",
        f = {
            complete: "complete",
            timeout: "timeout"
        },
        n = !1,
        t = [],
        i = 0;
    return e(), {
        logFinalThumbnailTime: o,
        setLogProbability: c,
        logThumbnailTimeout: s,
        init: e
    }
}();