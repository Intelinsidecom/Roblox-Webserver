// ~/viewapp/pages/accounts/controllers/accountSettingsModalEmailVerifyController.js
"use strict";
accounts.controller("accountSettingsModalEmailVerifyController", ["$scope", "$uibModalInstance", "accountsService", "modalData", function(n, t, i, r) {
    function u() {
        n.verifiedEmailRequired || n.beginAccountVerifyEmailAddress()
    }
    angular.extend(n, r), n.verificationEmailSent = !1, n.verificationEmailError = !1, n.beginAccountVerifyEmailAddress = function() {
        i.beginAccountVerifyEmailAddress().then(function() {
            n.verificationEmailSent = !0
        }, function(t) {
            n.verificationEmailError = !0, n.error = i.beginProcessErrorMessage(t)
        })
    }, n.sendVerificationEmail = function() {
        n.verifiedEmailRequired = !1, n.beginAccountVerifyEmailAddress()
    }, u()
}]);