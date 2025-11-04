// ~/viewapp/common/services/urlService.js
robloxAppService.factory("urlService", [function() {
    function n(n) {
        return Roblox && Roblox.Endpoints ? Roblox.Endpoints.getAbsoluteUrl(n) : n
    }
    return {
        getAbsoluteUrl: n
    }
}]);