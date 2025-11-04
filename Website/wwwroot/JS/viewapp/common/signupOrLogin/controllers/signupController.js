// ~/viewapp/common/signupOrLogin/controllers/signupController.js
"use strict";
signupOrLogin.controller("SignupController", ["$scope", "$http", "displayService", "captchaService", "signupService", function(n, t, i, r, u) {
    n.signup = {}, n.signup.gender = Roblox.SignupOrLogin.GenderType.unknown, n.setGender = function(t, i) {
        n.signup.gender = i, t.preventDefault()
    }, n.showBirthdayValidation = function() {
        return (n.badSubmit || n.signupForm.birthdayMonth.$dirty && n.signupForm.birthdayDay.$dirty && n.signupForm.birthdayYear.$dirty) && (!n.isValidBirthday(n.signup.birthdayDay) || n.signupForm.birthdayYear.$invalid)
    }, n.isValidBirthday = function(t) {
        var u = n.signup.birthdayYear ? n.signup.birthdayYear : "2014",
            f = n.signup.birthdayMonth ? n.signup.birthdayMonth : 1,
            i = new Date(f + " " + t + " " + u),
            r;
        return Object.prototype.toString.call(i) !== "[object Date]" || isNaN(i.getTime()) ? !1 : i.getDate().toString() != t ? !1 : (r = new Date, i.getTime() < r.getTime() && i.getFullYear() > r.getFullYear() - 100)
    }, n.badSubmit = !1, n.isSubmitting = !1, n.submitSignup = function() {
        if (n.signupForm.$valid && n.signup.gender !== Roblox.SignupOrLogin.GenderType.unknown && n.isValidBirthday(n.signup.birthdayDay)) n.badSubmit = !1;
        else {
            n.badSubmit = !0;
            return
        }
        n.isSubmitting = !0, u.submitSignup(n)
    };
    var f = angular.element(".signup-or-log-in").data("is-login-default-section");
    n.isSectionShown = f === !1, n.$on("display:updated", function(t, i) {
        n.isSectionShown = i === Roblox.SignupOrLogin.SectionType.signup, n.isSectionShown && (Roblox.SignupOrLoginIframe && Roblox.SignupOrLoginIframe.resizeFrame(Roblox.SignupOrLogin.SectionType.signup), Roblox.SignupOrLoginModal && Roblox.SignupOrLoginModal.resizeModal(Roblox.SignupOrLogin.SectionType.signup), n.signup = {}, n.badSubmit = !1, n.signupForm.$setPristine(), n.signupForm.$generalError = !1)
    }), n.$on("captcha:signupSuccess", function() {
        u.submitSignup(n)
    })
}]);