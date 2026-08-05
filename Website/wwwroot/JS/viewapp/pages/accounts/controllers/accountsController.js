// ~/viewapp/pages/accounts/controllers/accountsController.js
"use strict";
accounts.controller("accountsController", ["$scope", "$state", "$log", "notificationConstants", "accountsService", function(n, t, i, r, u) {
    n.accountsTabs = [{
        name: "info",
        label: "Account Info"
    }, {
        name: "security",
        label: "Security"
    }, {
        name: "privacy",
        label: "Privacy"
    }, {
        name: "billing",
        label: "Billing"
    }], n.mappedDestinationTypes = [], n.destinationTypes = r.destinationTypes, u.beginGetAllowedNotificationDestinationTypes().then(function(t) {
        t && t.length > 0 && (n.mappedDestinationTypes = n.destinationTypes.filter(function(n) {
            return t.indexOf(n) !== -1
        }), n.mappedDestinationTypes.length > 0 && n.accountsTabs.push({
            name: "notifications",
            label: "Notifications"
        }))
    }, function() {
        i.debug("Error getting account info")
    }), n.accountData = n.accountData ? n.accountData : {
        stateProperties: {}
    }, n.currentData = n.currentData ? n.currentData : {
        activeTab: n.accountsTabs[0].name,
        stateLabel: ""
    }, n.$on("$stateChangeSuccess", function(t, i) {
        n.currentData.activeTab = i.name, n.currentData.stateLabel = i.label
    })
}]);