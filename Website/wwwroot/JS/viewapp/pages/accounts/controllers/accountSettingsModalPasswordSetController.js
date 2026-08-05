// ~/viewapp/pages/accounts/controllers/accountSettingsModalPasswordSetController.js
"use strict";
accounts.controller("accountSettingsModalPasswordSetController", ["$scope", "$uibModalInstance", "accountsService", "modalData", "modalConstants", function(n, t, i, r, u) {
    angular.extend(n, r), n.passwordInfo = {}, n.passwordSetSuccess = !1, n.title = n.changePassword ? u.passwordSet.changePasswordMessage : u.passwordSet.addPasswordMessage, n.passwordErrorMessage = function(t) {
        return t["passwordInfo.newPassword"].$error.required || t["passwordInfo.confirmPassword"].$error.required ? null : t["passwordInfo.confirmPassword"].$error.matchField ? u.passwordSet.matchErrorMessage : t.$pristine && n.error ? n.error : null
    }, n.submit = function(t) {
        t.$setPristine(), n.error = "", i.beginAccountChangePassword(n.passwordInfo).then(function(t) {
            t.Success ? (n.title = u.default.success.title, n.passwordSetSuccess = !0) : n.error = t.Message ? t.Message : u.default.error.body
        }, function() {
            n.error = u.default.error.body
        })
    }, n.dismiss = function() {
        n.passwordSetSuccess ? t.close(!0) : t.dismiss()
    }
}]);