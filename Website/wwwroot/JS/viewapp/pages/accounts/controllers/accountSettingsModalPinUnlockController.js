// ~/viewapp/pages/accounts/controllers/accountSettingsModalPinUnlockController.js
"use strict";
accounts.controller("accountSettingsModalPinUnlockController", ["$scope", "$log", "$uibModalInstance", "accountsService", "modalData", function(n, t, i, r, u) {
    angular.extend(n, u), n.pinInfo = {}, n.pinErrorMessage = function(t) {
        return t.$pristine && n.error ? n.error : null
    }, n.submit = function(t) {
        t.$valid && (t.$setPristine(), n.error = "", r.beginUnlockAccountPinSetting(n.pinInfo.pin).then(function(n) {
            i.close(n)
        }, function(t) {
            n.error = r.beginProcessErrorMessage(t)
        }))
    }
}]);