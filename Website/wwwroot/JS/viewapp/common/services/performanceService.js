// ~/viewapp/common/services/performanceService.js
robloxAppService.factory("performanceService", ["$log", function() {
    function t() {
        return Roblox && Roblox.Performance
    }

    function i(n) {
        t() && Roblox.Performance.logSinglePerformanceMark(n)
    }
    return {
        logSinglePerformanceMark: i
    }
}]);