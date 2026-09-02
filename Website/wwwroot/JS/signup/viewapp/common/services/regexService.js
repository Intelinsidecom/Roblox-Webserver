// ~/viewapp/common/services/regexService.js
freebloxiaApp.factory("regexService", ["httpService", "urlService", function(n, t) {
    function r() {
        var r = i,
            u = {
                url: t.getAbsoluteUrl(r)
            };
        return n.httpGet(u, null)
    }
    var i = "/regex/email";
    return {
        getEmailRegex: r
    }
}]);