// ~/viewapp/common/services/numericCodeService.js
robloxAppService.factory("numericCodeService", ["$log", function(n) {
    function t(t, i) {
        return n.debug(t, i), t.length === i && /^\d+$/.test(t)
    }
    return {
        checkCode: t
    }
}]);