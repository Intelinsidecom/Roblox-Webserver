// Login/SignupOrLogIn.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.SignupOrLogin == "undefined" && (Roblox.SignupOrLogin = function() {
    var r = {
            unknown: 1,
            male: 2,
            female: 3
        },
        u = {
            signup: 0,
            login: 1
        },
        f = {
            signup: 0,
            login: 1,
            captcha: 2,
            twostep: 3
        },
        n, t, e = function(t) {
            typeof n == "function" && n(t)
        },
        o = function(n) {
            typeof t == "function" && t(n)
        },
        i = function(n) {
            n.data("params", {})
        },
        s = function(r) {
            typeof r.onSignupSuccess == "function" && (n = r.onSignupSuccess), typeof r.onLoginSuccess == "function" && (t = r.onLoginSuccess), i($(".signup-or-log-in"))
        },
        h = function(n, t, i) {
            var r = n.data("params");
            typeof r == "undefined" && (r = {}), r[t] = {
                name: t,
                value: i
            }, n.data("params", r)
        };
    return {
        GenderType: r,
        SectionType: f,
        CaptchaFlowType: u,
        addSignupParam: h,
        onLoginSuccess: o,
        onSignupSuccess: e,
        resetParams: i,
        init: s
    }
}()), Roblox.Animated2014SignupFormValidator = function() {
    "use strict";

    function n(n) {
        var t = "";
        return Roblox.SignupFormValidatorGeneric.usernameTooLong(n) && (t = Roblox.Resources.AnimatedSignupFormValidator.userNameRange ? Roblox.Resources.AnimatedSignupFormValidator.userNameRange : Roblox.Resources.AnimatedSignupFormValidator.tooLong), Roblox.SignupFormValidatorGeneric.usernameTooShort(n) && (t = Roblox.Resources.AnimatedSignupFormValidator.userNameRange ? Roblox.Resources.AnimatedSignupFormValidator.userNameRange : Roblox.Resources.AnimatedSignupFormValidator.tooShort), Roblox.SignupFormValidatorGeneric.usernameRegexInvalid(n) && (t = Roblox.Resources.AnimatedSignupFormValidator.invalidCharacters), Roblox.SignupFormValidatorGeneric.usernameStartsOrEndsWithUnderscore && Roblox.SignupFormValidatorGeneric.usernameStartsOrEndsWithUnderscore(n) && (t = Roblox.Resources.AnimatedSignupFormValidator.startsOrEndsWithUnderscore), Roblox.SignupFormValidatorGeneric.usernameHasMoreThanOneUnderscore && Roblox.SignupFormValidatorGeneric.usernameHasMoreThanOneUnderscore(n) && (t = Roblox.Resources.AnimatedSignupFormValidator.moreThanOneUnderscore), t
    }

    function t(n, t) {
        var i = "";
        return Roblox.SignupFormValidatorGeneric.passwordTooLong && Roblox.SignupFormValidatorGeneric.passwordTooLong(n) ? i = Roblox.Resources.AnimatedSignupFormValidator.tooLong : Roblox.SignupFormValidatorGeneric.passwordTooShort(n) ? i = Roblox.Resources.AnimatedSignupFormValidator.tooShort : (Roblox.SignupFormValidatorGeneric.passwordEnoughLetters && !Roblox.SignupFormValidatorGeneric.passwordEnoughLetters(n) && (i = Roblox.Resources.AnimatedSignupFormValidator.needsFourLetters), Roblox.SignupFormValidatorGeneric.passwordEnoughNumbers && !Roblox.SignupFormValidatorGeneric.passwordEnoughNumbers(n) && (i = Roblox.Resources.AnimatedSignupFormValidator.needsTwoNumbers), Roblox.SignupFormValidatorGeneric.passwordContainsSpaces && Roblox.SignupFormValidatorGeneric.passwordContainsSpaces(n) && (i = Roblox.Resources.AnimatedSignupFormValidator.noSpaces), Roblox.SignupFormValidatorGeneric.passwordIsUsername && Roblox.SignupFormValidatorGeneric.passwordIsUsername(n, t) && (i = Roblox.Resources.AnimatedSignupFormValidator.passwordIsUsername)), Roblox.SignupFormValidatorGeneric.weakPassword(n) && (i = Roblox.Resources.AnimatedSignupFormValidator.weakKey), i
    }
    return {
        verifyUsername: n,
        verifyPassword: t
    }
}();