// ~/viewapp/common/services/urlService.js
freebloxiaAppService.factory("urlService", [function() {
    function n(n) {
        return Freebloxia && Freebloxia.Endpoints ? Freebloxia.Endpoints.getAbsoluteUrl(n) : n
    }
    return {
        getAbsoluteUrl: n
    }
}]);