// ~/viewapp/common/services/phoneService.js
freebloxiaAppService.factory("phoneService", ["$q", "httpService", "phoneConstants", function(n, t, i) {
    function f(n) {
        var i = n + r,
            f = {
                url: i
            };
        return t.httpGet(f, null).then(function(n) {
            var t;
            return _.reject(n, function(n) {
                return n.code === u ? (t = n, !0) : !1
            }), t && n.unshift(t), n
        })
    }
    var r = i.phonePrefixesUrl,
        u = i.defaultCountryCode;
    return {
        getPhonePrefixes: f
    }
}]);