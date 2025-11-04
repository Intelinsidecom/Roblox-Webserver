// ~/viewapp/common/signupOrLogin/controllers/captchaController.js
"use strict";
signupOrLogin.controller("CaptchaController", ["$scope", "$http", "$rootElement", "captchaService", "displayService", function(n, t, i, r, u) {
    var f = function() {
        Recaptcha.reload()
    };
    n.isSubmitting = !1, n.submitCaptcha = function(t) {
        r.submitCaptcha(t, f, n)
    }, n.isSectionShown = !1, n.$on("display:updated", function(t, i) {
        n.$validationMessage = "", n.isSectionShown = i === Roblox.SignupOrLogin.SectionType.captcha
    }), n.captchaFlowType = Roblox.SignupOrLogin.CaptchaFlowType.signup, n.$on("captcha:loaded", function(t, i) {
        n.captchaResponse = "", n.captchaFlowType = i.captchaFlow, n.username = i.username, f(), u.setDisplayState(Roblox.SignupOrLogin.SectionType.captcha), n.isSectionShown = !0, Roblox.SignupOrLoginIframe && Roblox.SignupOrLoginIframe.resizeFrame(Roblox.SignupOrLogin.SectionType.captcha)
    })
}]);