// Login/SignupOrLogIn.js
typeof Freebloxia == "undefined" && (Freebloxia = {}), typeof Freebloxia.SignupOrLogin == "undefined" && (Freebloxia.SignupOrLogin = function() {
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
}()), Freebloxia.Animated2014SignupFormValidator = function() {
    "use strict";

    function n(n) {
        var t = "";
        return Freebloxia.SignupFormValidatorGeneric.usernameTooLong(n) && (t = Freebloxia.Resources.AnimatedSignupFormValidator.userNameRange ? Freebloxia.Resources.AnimatedSignupFormValidator.userNameRange : Freebloxia.Resources.AnimatedSignupFormValidator.tooLong), Freebloxia.SignupFormValidatorGeneric.usernameTooShort(n) && (t = Freebloxia.Resources.AnimatedSignupFormValidator.userNameRange ? Freebloxia.Resources.AnimatedSignupFormValidator.userNameRange : Freebloxia.Resources.AnimatedSignupFormValidator.tooShort), Freebloxia.SignupFormValidatorGeneric.usernameRegexInvalid(n) && (t = Freebloxia.Resources.AnimatedSignupFormValidator.invalidCharacters), Freebloxia.SignupFormValidatorGeneric.usernameStartsOrEndsWithUnderscore && Freebloxia.SignupFormValidatorGeneric.usernameStartsOrEndsWithUnderscore(n) && (t = Freebloxia.Resources.AnimatedSignupFormValidator.startsOrEndsWithUnderscore), Freebloxia.SignupFormValidatorGeneric.usernameHasMoreThanOneUnderscore && Freebloxia.SignupFormValidatorGeneric.usernameHasMoreThanOneUnderscore(n) && (t = Freebloxia.Resources.AnimatedSignupFormValidator.moreThanOneUnderscore), t
    }

    function t(n, t) {
        var i = "";
        return Freebloxia.SignupFormValidatorGeneric.passwordTooLong && Freebloxia.SignupFormValidatorGeneric.passwordTooLong(n) ? i = Freebloxia.Resources.AnimatedSignupFormValidator.tooLong : Freebloxia.SignupFormValidatorGeneric.passwordTooShort(n) ? i = Freebloxia.Resources.AnimatedSignupFormValidator.tooShort : (Freebloxia.SignupFormValidatorGeneric.passwordEnoughLetters && !Freebloxia.SignupFormValidatorGeneric.passwordEnoughLetters(n) && (i = Freebloxia.Resources.AnimatedSignupFormValidator.needsFourLetters), Freebloxia.SignupFormValidatorGeneric.passwordEnoughNumbers && !Freebloxia.SignupFormValidatorGeneric.passwordEnoughNumbers(n) && (i = Freebloxia.Resources.AnimatedSignupFormValidator.needsTwoNumbers), Freebloxia.SignupFormValidatorGeneric.passwordContainsSpaces && Freebloxia.SignupFormValidatorGeneric.passwordContainsSpaces(n) && (i = Freebloxia.Resources.AnimatedSignupFormValidator.noSpaces), Freebloxia.SignupFormValidatorGeneric.passwordIsUsername && Freebloxia.SignupFormValidatorGeneric.passwordIsUsername(n, t) && (i = Freebloxia.Resources.AnimatedSignupFormValidator.passwordIsUsername)), Freebloxia.SignupFormValidatorGeneric.weakPassword(n) && (i = Freebloxia.Resources.AnimatedSignupFormValidator.weakKey), i
    }
    return {
        verifyUsername: n,
        verifyPassword: t
    }
}();