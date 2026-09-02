// ~/viewapp/common/services/performanceService.js
freebloxiaAppService.factory("performanceService", ["$log", function() {
    function t() {
        return Freebloxia && Freebloxia.Performance
    }

    function i(n) {
        t() && Freebloxia.Performance.logSinglePerformanceMark(n)
    }
    return {
        logSinglePerformanceMark: i
    }
}]);