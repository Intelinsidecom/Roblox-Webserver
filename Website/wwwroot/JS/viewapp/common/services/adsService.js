// ~/viewapp/common/services/adsService.js
robloxAppService.factory("adsService", [function() {
    function t(n) {
        Roblox.AdsHelper && Roblox.AdsHelper.AdRefresher && Roblox.AdsHelper.AdRefresher.registerAd(n)
    }

    function i() {
        Roblox.AdsHelper && Roblox.AdsHelper.AdRefresher && Roblox.AdsHelper.AdRefresher.refreshAds()
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