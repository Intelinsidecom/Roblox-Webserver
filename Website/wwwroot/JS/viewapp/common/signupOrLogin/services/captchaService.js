// ~/viewapp/common/signupOrLogin/services/captchaService.js
"use strict";
signupOrLogin.factory("captchaService", ["$rootScope", "$http", function(n, t) {
    var i = Roblox.SignupOrLogin.CaptchaFlowType.signup,
        u = function(n, u, f) {
            var o = angular.element(n.target),
                s = angular.element(".signup-or-log-in .captcha-container"),
                h = {
                    recaptcha_challenge_field: s.find("#recaptcha_challenge_field").val(),
                    recaptcha_response_field: s.find("#recaptcha_response_field").val()
                },
                e;
            if (f.captchaFlowType === Roblox.SignupOrLogin.CaptchaFlowType.signup) e = o.data("signup-captcha-api-url");
            else if (i === Roblox.SignupOrLogin.CaptchaFlowType.login) e = o.data("log-in-captcha-api-url"), h.username = f.username;
            else throw "Unknown Captcha flow";
            f.isSubmitting = !0, t({
                method: "POST",
                url: e,
                data: $.param(h),
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded"
                },
                crossDomain: !0
            }).success(function() {
                f.isSubmitting = !1, f.$validationMessage = "", r()
            }).error(function(n, t) {
                f.isSubmitting = !1, t === 403 ? (f.$validationMessage = "Captcha incorrect", u()) : (f.$validationMessage = "Unknown error", u())
            })
        },
        r = function() {
            i == Roblox.SignupOrLogin.CaptchaFlowType.signup ? n.$broadcast("captcha:signupSuccess") : i == Roblox.SignupOrLogin.CaptchaFlowType.login && n.$broadcast("captcha:loginSuccess")
        };
    return {
        getCaptchaFlowType: function() {
            return i
        },
        setCaptchaFlowType: function(t, r) {
            i = t;
            var u = {
                captchaFlow: t
            };
            r && (u.username = r), n.$broadcast("captcha:loaded", u)
        },
        onAfterCaptchaSuccess: r,
        submitCaptcha: u
    }
}]);