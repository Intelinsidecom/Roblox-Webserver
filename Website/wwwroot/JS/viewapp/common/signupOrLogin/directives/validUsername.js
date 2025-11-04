// ~/viewapp/common/signupOrLogin/directives/validUsername.js
"use strict";
signupOrLogin.directive("rbxValidUsername", ["$http", function(n) {
    return {
        require: "ngModel",
        link: function(t, i, r, u) {
            t.signup.username = i.val(), t.usernameValidationRequestNum = 0, t.onChange = function() {
                var e = t.signup.username === "" || angular.isUndefined(t.signup.username),
                    i = "",
                    r = !1,
                    f = ++t.usernameValidationRequestNum;
                f == 1 && t.signupForm.username && t.signup.username && (t.signupForm.username.$dirty = !0), e ? (r = !0, i = "Please enter a username.") : (i = Roblox.Animated2014SignupFormValidator.verifyUsername(t.signup.username), i !== "" && (r = !0)), u.$setValidity("validusername", !r), r ? u.$validationMessage = i : (u.$validationMessage = "", n({
                    method: "GET",
                    url: "/UserCheck/checkifinvalidusernameforsignup",
                    params: {
                        username: t.signup.username
                    }
                }).success(function(n) {
                    if (f == t.usernameValidationRequestNum) {
                        var r = !0,
                            e = !0,
                            i = "";
                        n.data == 1 && (r = !1, i = Roblox.Resources.AnimatedSignupFormValidator.alreadyTaken), n.data == 2 && (e = !1, i = Roblox.Resources.AnimatedSignupFormValidator.cantBeUsed), u.$setValidity("unique", r), u.$setValidity("moderated", e), u.$invalid ? i != "" && (u.$validationMessage = i) : u.$validationMessage = ""
                    }
                }))
            }, t.$evalAsync(function() {
                t.onChange()
            })
        }
    }
}]);