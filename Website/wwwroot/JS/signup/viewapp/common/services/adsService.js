// ~/viewapp/common/services/adsService.js
freebloxiaAppService.factory("adsService", [function() {
    function t(n) {
        Freebloxia.AdsHelper && Freebloxia.AdsHelper.AdRefresher && Freebloxia.AdsHelper.AdRefresher.registerAd(n)
    }

    function i() {
        Freebloxia.AdsHelper && Freebloxia.AdsHelper.AdRefresher && Freebloxia.AdsHelper.AdRefresher.refreshAds()
    }
    var n = {
        leaderboardAbp: "Leaderboard-Abp",
        skyscraperAdpRight: "Skyscraper-Adp-Right"
    };
    return {
        registerAd: t,
        refreshAllAds: i,
        adIds: n
    }
}]);