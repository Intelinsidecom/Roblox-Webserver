// ~/viewapp/studio/toolbox/toolboxController.js
clientToolbox.controller("RobloxReferralLink", ["$scope", "ClientToolboxService", function(n, t) {
    n.refer = function(n) {
        t.searchText = n, t.updateAssets(t.params.category, t.params.setId, t.searchText, t.params.sortBy, null, t.params.creatorId)
    }
}]);