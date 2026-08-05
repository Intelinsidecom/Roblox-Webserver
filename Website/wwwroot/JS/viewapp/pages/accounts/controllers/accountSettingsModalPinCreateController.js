// ~/viewapp/pages/accounts/controllers/accountSettingsModalPinCreateController.js
"use strict";
accounts.controller("accountSettingsModalPinCreateController", ["$scope", "$uibModalInstance", "accountsService", "modalData", "modalConstants", function(n, t, i, r, u) {
    angular.extend(n, r), n.pinInfo = {}, n.pinCreateSuccess = !1, n.pinErrorMessage = function(t) {
        return t.newPin.$error.required || t.newPin.$error.minlength || t.newPinConfirm.$error.required || t.newPinConfirm.$error.minlength ? null : t.$dirty && t.newPinConfirm.$error.matchField ? u.pinCreate.matchErrorMessage : t.$pristine && n.error ? n.error : null
    }, n.submit = function(t) {
        t.$valid && (t.$setPristine(), n.error = "", i.beginCreateAccountPinSetting(n.pinInfo.newPin).then(function() {
            n.pinCreateSuccess = !0
        }, function(t) {
            n.error = t ? t : u.default.error.body
        }))
    }, n.dismiss = function() {
        n.pinCreated ? t.close(!0) : t.dismiss()
    }
}]);