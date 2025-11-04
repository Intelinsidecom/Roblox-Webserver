// ~/viewapp/common/services/imagesService.js
robloxAppService.factory("robloxImagesService", ["$http", "$timeout", "$log", function(n, t, i) {
    function s(n) {
        u = n
    }

    function h(n) {
        f = n
    }

    function e(r, s, h) {
        n({
            method: "GET",
            url: r.RetryUrl,
            withCredentials: !0
        }).then(function(n) {
            return h >= f || n.data.Final ? (h = 0, s(n.data.Url)) : (h++, n.data.RetryUrl = r.RetryUrl, t(function() {
                e(n.data, s, h)
            }, u), !1)
        }, function(n, t) {
            return i.debug(o + t), !1
        })
    }
    var r = angular.element("#image-retry-data"),
        u = r.length > 0 && Number(r.data("image-retry-timer")) || 1500,
        o = "Errors from http call: ",
        f = r.length > 0 && Number(r.data("image-retry-max-times")) || 10;
    return {
        setRetryTimer: s,
        setMaxTry: h,
        getImageUrl: e
    }
}]);