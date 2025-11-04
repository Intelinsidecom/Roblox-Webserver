// ~/viewapp/common/signupOrLogin/controllers/loginController.js
"use strict";
signupOrLogin.controller("LoginController", ["$scope", "displayService", "captchaService", "loginService", function(n, t, i, r) {
    var u, f;
    n.login = {}, n.badSubmit = !1, n.submitLogin = function(t) {
        if (n.loginForm.$invalid) {
            t.preventDefault(), n.badSubmit = !0, n.loginForm.username.$validationMessage = "Username is required.", n.loginForm.password.$validationMessage = "Password is required.";
            return
        }
        n.loginForm.username.showValidation = !1, n.loginForm.password.showValidation = !1, n.badSubmit = !1, r.executeLogin(n)
    }, u = 13, n.enterLogin = function(t) {
        t.which == u && n.submitLogin(t)
    }, n.requestTwoStepCode = function() {
        r.requestTwoStepCode(n)
    }, n.enterTwoStepCode = function(t) {
        t.which == u && n.submitTwoStepCode(t)
    }, n.submitTwoStepCode = function(t) {
        if (n.twoStepVerificationForm.twoStepCode.$setValidity("twostep", !0), n.twoStepVerificationForm.$invalid || n.loginForm.$invalid) {
            t.preventDefault(), n.badSubmit = !0;
            return
        }
        r.submitTwoStepCode(t, n)
    }, f = angular.element(".signup-or-log-in").data("is-login-default-section"), n.isSectionShown = f, n.isTwoStepSectionShown = !1, n.$on("display:updated", function(t, i) {
        n.isSectionShown = i === Roblox.SignupOrLogin.SectionType.login, n.isTwoStepSectionShown = i === Roblox.SignupOrLogin.SectionType.twostep, n.isSectionShown && (Roblox.SignupOrLoginIframe && Roblox.SignupOrLoginIframe.resizeFrame(Roblox.SignupOrLogin.SectionType.login), Roblox.SignupOrLoginModal && Roblox.SignupOrLoginModal.resizeModal(Roblox.SignupOrLogin.SectionType.login), n.badSubmit = !1, n.login = {}, n.twoStepVerification = {}, n.isSubmitting = !1, n.loginForm.$setPristine(), n.$generalError = !1, n.twoStepVerificationForm && (n.twoStepVerificationForm.twoStepCode.$validationMessage = "", n.twoStepVerificationForm.$setPristine()))
    }), n.$on("captcha:loginSuccess", function() {
        r.executeLogin(n)
    })
}]);