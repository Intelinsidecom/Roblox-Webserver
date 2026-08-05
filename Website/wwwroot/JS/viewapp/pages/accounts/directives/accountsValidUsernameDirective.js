// ~/viewapp/pages/accounts/directives/accountsValidUsernameDirective.js
"use strict";
accounts.directive("accountValidUsername", ["httpService", function(n) {
    return {
        restrict: "A",
        require: "ngModel",
        link: function(t, i, r, u) {
            t.usernameValidationRequestNum = 0, t.onChange = function() {
                var o = ++t.usernameValidationRequestNum,
                    s = t.user.username === "" || angular.isUndefined(t.user.username),
                    i = "",
                    r = !1,
                    f, e;
                s ? (r = !0, i = "Please enter a username.") : (i = Roblox.SignupFormValidatorGeneric.getInvalidUsernameMessage(t.user.username), i !== "" && (r = !0)), u.$setValidity("validusername", !r), r ? u.$usernameMessage = i : (u.$usernameMessage = "", f = {
                    url: "/UserCheck/checkifinvalidusernameforsignup"
                }, e = {
                    username: t.user.username
                }, n.httpGet(f, e).then(function(n) {
                    if (o === t.usernameValidationRequestNum) {
                        var r = !0,
                            f = !0,
                            i = "";
                        n.data === 1 && (r = !1, i = Roblox.Resources.AnimatedSignupFormValidator.alreadyTaken), n.data === 2 && (f = !1, i = Roblox.Resources.AnimatedSignupFormValidator.cantBeUsed), u.$setValidity("unique", r), u.$setValidity("moderated", f), u.$invalid ? i !== "" && (u.$usernameMessage = i) : u.$usernameMessage = ""
                    }
                }))
            }
        }
    }
}]);