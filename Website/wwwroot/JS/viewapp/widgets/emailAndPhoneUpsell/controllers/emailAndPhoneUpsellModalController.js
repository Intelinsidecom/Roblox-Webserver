// ~/viewapp/widgets/emailAndPhoneUpsell/controllers/emailAndPhoneUpsellModalController.js
"use strict";
emailAndPhoneUpsell.controller("emailAndPhoneUpsellModalController", ["$scope", "$uibModalInstance", "regexService", "eventStreamService", "emailAndPhoneUpsellService", "emailAndPhoneUpsellConstants", function(n, t, i, r, u, f) {
    n.forms = {
        emailForm: {}
    }, n.data = {}, n.layout = {}, n.layout.constants = f, n.init = function() {
        n.data.isUnder13 = Roblox.CurrentUser && Roblox.CurrentUser.isUnder13, n.initializeModalData()
    }, n.initializeModalData = function() {
        switch (n.modalData.upsellScreenType) {
            case f.screenTypes.addEmail:
                n.initializeAddEmailModal();
                break;
            case f.screenTypes.verifyEmail:
                n.initializeVerifyEmailModal()
        }
    }, n.initializeAddEmailModal = function() {
        n.layout.showAddEmailModal = !0, n.layout.modalBodyText = n.data.isUnder13 ? f.addEmailUnder13Prompt : f.addEmail13AndOverPrompt, n.layout.emailPlaceholder = n.data.isUnder13 ? f.parentsEmailPlaceholder : f.emailPlaceholder, i.getEmailRegex().then(function(t) {
            t && (n.layout.emailRegex = t.Regex)
        })
    }, n.initializeVerifyEmailModal = function() {
        n.layout.showVerifyEmailModal = !0, n.layout.modalBodyText = n.data.isUnder13 ? f.verifyEmailUnder13Prompt : f.verifyEmail13AndOverPrompt
    }, n.clearError = function() {
        n.layout.error = null
    }, n.getError = function() {
        return n.isEmailValid() ? n.layout.error : f.invalidEmail
    }, n.isEmailValid = function() {
        return n.forms.emailForm.email.$valid
    }, n.showEmailForm = function() {
        n.layout.focusOnEmailInput = !0, n.layout.showEmailForm = !0, n.modalData.upsellScreenType = f.screenTypes.emailForm, n.sendModalEvent(f.modalActions.shown)
    }, n.getSuccessMessage = function() {
        return n.data.isUnder13 ? f.verifyEmailUnder13SuccessMessage : f.verifyEmail13AndOverSuccessMessage
    }, n.submitEmail = function() {
        n.layout.isSubmitBusy = !0, n.layout.error = null, u.submitEmail(n.data.email, n.data.password).then(function(i) {
            i || (n.sendAddEmailEvent(!1), n.layout.error = f.errorMessage), i.Success ? (n.sendAddEmailEvent(!0), t.close(n.getSuccessMessage())) : (n.sendAddEmailEvent(!1, i.Message), n.layout.error = i.Message)
        }, function() {
            n.sendAddEmailEvent(!1), n.layout.error = f.errorMessage
        }).finally(function() {
            n.layout.isSubmitBusy = !1
        })
    }, n.verifyEmail = function() {
        n.layout.isSubmitBusy = !0, n.layout.error = null, u.verifyEmail().then(function(i) {
            i || (n.sendVerifyEmailEvent(!1), n.layout.error = f.errorMessage), i.Success ? (n.sendVerifyEmailEvent(!0), t.close(n.getSuccessMessage())) : (n.sendVerifyEmailEvent(!1, i.Message), n.layout.error = i.Message)
        }, function() {
            n.sendVerifyEmailEvent(!1), n.layout.error = f.errorMessage
        }).finally(function() {
            n.layout.isSubmitBusy = !1
        })
    }, n.sendAddEmailEvent = function(t, i) {
        n.sendActionEvent(f.events.addEmail, t, i)
    }, n.sendVerifyEmailEvent = function(t, i) {
        n.sendActionEvent(f.events.verifyEmail, t, i)
    }, n.sendActionEvent = function(n, t, i) {
        r.sendEventWithTarget(n, f.eventContext, {
            success: t,
            msg: i
        })
    }, n.init()
}]);