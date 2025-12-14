// ~/viewapp/common/signupOrLogin/services/signupService.js
"use strict";
signupOrLogin.factory("signupService", ["$http", "captchaService", "displayService", "$rootElement", function(n, t, i) {
    var u = function(r) {
            var s = angular.element("#signup-button"),
                h = s.data("signup-api-url"),
                u = {},
                o = angular.element(".signup-or-log-in"),
                e = o.data("params"),
                f;
            typeof e == "undefined" && (e = {}), $.each(e, function(n, t) {
                u[n] = t
            }), f = o.data("metadata-params"), typeof f == "undefined" && (f = {}), $.each(f, function(n, t) {
                u[n] = t
            }), $.extend(u, {
                username: r.signup.username,
                password: r.signup.password,
                birthday: r.signup.birthdayDay + " " + r.signup.birthdayMonth + " " + r.signup.birthdayYear,
                gender: r.signup.gender,
                context: r.signupForm.context
            }), n({
                method: "POST",
                url: h,
                data: $.param(u),
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded"
                },
                crossDomain: !0,
                withCredentials: !0
            }).success(function(n) {
                // Notify signup success but do not redirect or treat as logged in yet.
                Roblox.SignupOrLogin.onSignupSuccess(n.userId);

                var username = r.signup && r.signup.username;
                var password = r.signup && r.signup.password;

                // Only after we successfully call the login API (which sets the cookie)
                // do we run the login-success handler and redirect.
                if (username && password) {
                    $.ajax({
                        type: "POST",
                        url: "/login/v1",
                        data: { username: username, password: password },
                        crossDomain: !0,
                        xhrFields: { withCredentials: !0 },
                        success: function(t) {
                            if (t && t.userId) {
                                Roblox.SignupOrLogin.onLoginSuccess(t.userId);
                            }
                            window.location = "/home";
                        },
                        error: function(xhr) {
                            // If the login API rejects (e.g., 403), fall back to the login page
                            // so the user can try manually.
                            window.location = "/login";
                        }
                    });
                } else if (n && n.userId) {
                    // As a very last fallback (no username/password captured),
                    // just go to home and let server auth logic decide.
                    window.location = "/home";
                }
            }).error(function(n, u) {
                if (r.badSubmit = !0, r.isSubmitting = !1, u === 403) {
                    var f = 0;
                    n.reasons.indexOf("Captcha") !== -1 && (r.isSubmitting = !1, r.isSectionShown = !1, i.setDisplayState(Roblox.SignupOrLogin.SectionType.captcha), t.setCaptchaFlowType(Roblox.SignupOrLogin.CaptchaFlowType.signup), f += 1), n.reasons.indexOf("GenderInvalid") !== -1 && (r.signup.gender = 1, f += 1), n.reasons.indexOf("PasswordInvalid") !== -1 && (r.signupForm.password.$setValidity("password", !1), r.signupForm.password.$passwordMessage = "Password is invalid", f += 1), n.reasons.indexOf("UsernameInvalid") !== -1 && (r.signupForm.username.$setValidity("validusername", !1), r.signupForm.username.$usernameMessage = "Username is invalid", f += 1), n.reasons.indexOf("UsernameTaken") !== -1 && (r.signupForm.username.$setValidity("unique", !1), r.signupForm.username.$usernameMessage = "Username is taken", f += 1), f < n.reasons.length && (r.signupForm.$generalError = !0, r.signupForm.$generalErrorText = "Sorry, an error occurred.")
                } else r.signupForm.$generalError = !0, r.signupForm.$generalErrorText = "Sorry, an error occurred."
            })
        },
        f = function(t) {
            var h = angular.element("#signup-button"),
                c = h.data("signup-api-url"),
                i = {},
                e = angular.element(".signup-or-log-in"),
                u = e.data("params"),
                r;
            typeof u == "undefined" && (u = {}), $.each(u, function(n, t) {
                i[n] = t
            }), r = e.data("metadata-params"), typeof r == "undefined" && (r = {}), $.each(r, function(n, t) {
                i[n] = t
            });
            var f = angular.element(".signup-or-log-in .captcha-container"),
                o = "",
                s = "";
            f.length && (o = f.find("#recaptcha_challenge_field").val(), s = f.find("#recaptcha_response_field").val()), $.extend(i, {
                UserName: t.signup.username,
                Birthday: angular.element("#BirthDayDate").val(),
                Email: angular.element("#Email").val(),
                Name: angular.element("#Name").val(),
                FBToken: angular.element("#FbToken").val(),
                GigyaUid: angular.element("#GigyaUID").val(),
                ReturnTo: angular.element("#ReturnTo").val(),
                IsGenderInputRequired: angular.element("#IsGenderInputRequired").val(),
                Gender: angular.element("#Gender").val(),
                recaptcha_challenge_field: o,
                recaptcha_response_field: s
            }), n({
                method: "POST",
                url: c,
                data: $.param(i),
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded"
                },
                crossDomain: !0,
                withCredentials: !0
            }).success(function(n) {
                Roblox.SignupOrLogin.onSignupSuccess(n.userId)
            }).error(function(n, i) {
                if (t.badSubmit = !0, t.isSubmitting = !1, i === 403) {
                    var r = 0;
                    n.reasons.indexOf("Captcha") !== -1 && (t.isSubmitting = !1, t.isSectionShown = !1, t.isCaptchaSectionShown = !0, Recaptcha.reload(), r += 1), n.reasons.indexOf("UsernameInvalid") !== -1 && (t.fbConnectForm.username.$setValidity("validusername", !1), t.fbConnectForm.username.$usernameMessage = "Username is invalid", r += 1), n.reasons.indexOf("UsernameTaken") !== -1 && (t.fbConnectForm.username.$setValidity("unique", !1), t.fbConnectForm.username.$usernameMessage = "Username is taken", r += 1), r < n.reasons.length && (t.fbConnectForm.$generalError = !0, t.fbConnectForm.$generalErrorText = "Sorry, an error occurred.")
                } else t.fbConnectForm.$generalError = !0, t.fbConnectForm.$generalErrorText = "Sorry, an error occurred."
            })
        };
    return {
        submitSignup: u,
        socialSubmitSignup: f
    }
}]);