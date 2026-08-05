// ~/viewapp/pages/accounts/controllers/accountSettingsModalEmailSetController.js
"use strict";
accounts.controller("accountSettingsModalEmailSetController", ["$scope", "$uibModalInstance", "accountsService", "regexService", "modalData", "modalConstants", function(n, t, i, r, u, f) {
    angular.extend(n, u), n.userInfo = {}, n.emailSetSuccess = !1, n.regexData = {}, r.getEmailRegex().then(function(t) {
        t && (n.regexData.email = t.Regex)
    }), n.modalConstants = f, n.title = function() {
        return n.emailRequired ? f.emailSet.emailRequiredMessage : n.emailSetSuccess ? f.emailSet.emailSetSuccessMessage : (n.changeEmail ? f.emailSet.modifyActionLabel : f.emailSet.addActionLabel) + " " + (n.over13 ? f.emailSet.over13Label : f.emailSet.under13Label) + " " + f.emailSet.emailLabel
    }, n.submitButtonText = function() {
        return n.changeEmail ? f.emailSet.modifyActionLabel + " " + f.emailSet.emailLabel : f.emailSet.addActionLabel + " " + f.emailSet.emailLabel
    }, n.emailErrorMessage = function(t) {
        return t["userInfo.emailAddress"].$error.required ? null : t["userInfo.emailAddress"].$invalid ? f.emailSet.invalidEmailAddressMessage : t.$pristine && n.error ? n.error : null
    }, n.submit = function(t) {
        t.$setPristine(), n.error = "", i.beginAccountAddEmailAddress(n.userInfo).then(function(t) {
            t.Success ? n.emailSetSuccess = !0 : n.error = t.Message ? t.Message : f.default.error.body
        }, function() {
            n.error = f.default.error.body
        })
    }, n.dismiss = function() {
        n.passwordSet ? t.close(!0) : t.dismiss()
    }
}]);