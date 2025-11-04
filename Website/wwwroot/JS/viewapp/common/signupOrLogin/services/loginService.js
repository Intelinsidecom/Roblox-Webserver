// ~/viewapp/common/signupOrLogin/services/loginService.js
"use strict";
signupOrLogin.factory("loginService", ["$http", "captchaService", "displayService", function(n, t, i) {
    var r = function(r) {
            var u = angular.element("#login-button"),
                f = u.data("login-api-url");
            r.isSubmitting = !0, n({
                method: "POST",
                url: f,
                data: $.param({
                    username: r.login.username,
                    password: r.login.password
                }),
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded"
                },
                crossDomain: !0,
                withCredentials: !0
            }).success(function(n) {
                r.$generalError = !1, r.$generalErrorText = "";
                Roblox.SignupOrLogin.onLoginSuccess(n.userId)
            }).error(function(n, u) {
                u === 403 ? n.message === "Captcha" ? (r.isSubmitting = !1, r.isSectionShown = !1, i.setDisplayState(Roblox.SignupOrLogin.SectionType.captcha), t.setCaptchaFlowType(Roblox.SignupOrLogin.CaptchaFlowType.login, r.login.username)) : n.message === "Privilege" ? (r.isSubmitting = !1, r.isSectionShown = !0, alert("Sorry, privileged accounts can't log in from here.")) : n.message === "TwoStepVerification" ? r.requestTwoStepCode() : n.message === "LoggedIn" ? window.location.reload() : n.message === "Credentials" ? (i.getDisplayState() !== Roblox.SignupOrLogin.SectionType.login && i.setDisplayState(Roblox.SignupOrLogin.SectionType.login), r.isSubmitting = !1, r.loginForm.username.$validationMessage = "", r.loginForm.username.showValidation = !0, r.loginForm.password.$validationMessage = "Your credentials were incorrect.", r.loginForm.password.showValidation = !0) : (i.getDisplayState() !== Roblox.SignupOrLogin.SectionType.login && i.setDisplayState(Roblox.SignupOrLogin.SectionType.login), r.isSubmitting = !1, r.$generalError = !0, r.$generalErrorText = "Sorry, an error occurred.") : (r.isSubmitting = !1, r.$generalError = !0, r.$generalErrorText = "Sorry, an error occurred.")
            })
        },
        u = function(t) {
            var r = angular.element("#login-button").data("two-step-code-request-url");
            n({
                method: "POST",
                url: r,
                data: $.param({
                    actionType: 0,
                    username: t.login.username,
                    password: t.login.password
                }),
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded"
                },
                crossDomain: !0,
                withCredentials: !0
            }).success(function() {
                t.isSubmitting = !1, i.setDisplayState(Roblox.SignupOrLogin.SectionType.twostep)
            }).error(function() {
                i.setDisplayState(Roblox.SignupOrLogin.SectionType.login), t.isSubmitting = !1, t.$generalError = !0, t.$generalErrorText = "Sorry, an error occurred."
            })
        },
        f = function(t, i) {
            var f = angular.element("#login-button").data("two-step-verification-api-url");
            i.isSubmitting = !0, n({
                method: "POST",
                url: f,
                data: $.param({
                    actionType: 0,
                    code: i.twoStepVerification.twoStepCode,
                    username: i.login.username,
                    password: i.login.password
                }),
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded"
                },
                crossDomain: !0,
                withCredentials: !0
            }).success(function() {
                r(i)
            }).error(function(n, t) {
                i.isSubmitting = !1, t === 403 ? n.message === "Credentials" ? (i.$generalError = !0, i.$generalErrorText = "Your credentials were incorrect.") : n.message === "Flooded" ? (i.twoStepVerificationForm.twoStepCode.$setValidity("twostep", !1), i.twoStepVerificationForm.twoStepCode.$validationMessage = "Slow down! You've made too many attempts. Try again later.") : n.message === "InvalidCode" ? (i.twoStepVerificationForm.twoStepCode.$setValidity("twostep", !1), i.twoStepVerificationForm.twoStepCode.$validationMessage = "Your code was incorrect. Try again.", u(i)) : (i.twoStepVerificationForm.twoStepCode.$setValidity("twostep", !1), i.twoStepVerificationForm.twoStepCode.$validationMessage = "Sorry, an error occurred.") : (i.twoStepVerificationForm.twoStepCode.$setValidity("twostep", !1), i.twoStepVerificationForm.twoStepCode.$validationMessage = "Sorry, an error occurred.")
            })
        };
    return {
        executeLogin: r,
        requestTwoStepCode: u,
        submitTwoStepCode: f
    }
}]);