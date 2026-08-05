// ~/viewapp/pages/accounts/controllers/accountSettingsModalUsernameUpdateController.js
"use strict";
accounts.controller("accountSettingsModalUsernameUpdateController", ["$scope", "$uibModalInstance", "accountsService", "modalData", "modalConstants", function(n, t, i, r, u) {
    angular.extend(n, r), n.user = {}, n.usernameUpdateSuccess = !1, n.usernameErrorMessage = function(t) {
        return t.$pristine && n.error ? n.error : null
    }, n.submit = function(t) {
        t.$setPristine(), n.error = "", i.beginAccountChangeUsername(n.user).then(function(t) {
            t.success ? n.usernameUpdateSuccess = !0 : n.error = t.message ? t.message : u.default.error.body
        }, function() {
            n.error = u.default.error.body
        })
    }, n.dismiss = function() {
        n.usernameUpdateSuccess ? t.close(!0) : t.dismiss()
    }
}]);