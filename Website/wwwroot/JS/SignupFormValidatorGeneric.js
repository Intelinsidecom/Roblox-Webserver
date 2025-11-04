// SignupFormValidatorGeneric.js
typeof Roblox == "undefined" && (Roblox = {}), Roblox.SignupFormValidatorGeneric = function() {
    function it(n, t, i) {
        return i <= 0 || n <= 0 || t <= 0 || t > new Date(i, n, 0).getDate()
    }

    function r(n, t, i) {
        return i != 0 && n != 0 && t != 0
    }

    function tt(n, t) {
        return $(n).length != 0 || $(t).length != 0
    }

    function nt(n) {
        return n.length > 20
    }

    function g(n) {
        return n.length < 3
    }

    function d(n) {
        var t = n.length;
        if (n[0] == "_" || n[t - 1] == "_") return !0
    }

    function k(n) {
        return n.split("_").length > 2
    }

    function b(n) {
        var t = n.indexOf(" ") != -1,
            i = v2UsernameAndPasswordRulesEnabled == 1 ? /^[a-zA-Z0-9_]*$/ : /^[a-zA-Z0-9]*$/;
        return t = t || !n.match(i)
    }

    function w(t) {
        n++;
        var i = n,
            r = function(t) {
                if (i == n) {
                    if (t.data == 1) return 1;
                    if (t.data == 2) return 2;
                    if (t.data == 0) return 0
                }
            },
            u = function() {};
        $.ajax({
            type: "GET",
            url: "/UserCheck/checkifinvalidusernameforsignup?username=" + t,
            success: r,
            error: u
        })
    }

    function p(n, t) {
        return t == "" || n.length > 0 && t != "" && n == t
    }

    function rt(n) {
        return n.length < 8
    }

    function v(n) {
        return n.length > 20
    }

    function a(n) {
        return n.length < 6
    }

    function l(n) {
        return u(n) > 3
    }

    function c(n) {
        return y(n) > 1
    }

    function h(n) {
        return f(n) > 0
    }

    function s(n, t) {
        return n == t
    }

    function o(n) {
        var t = ["roblox123", "password", "password1", "password12", "password123", "trustno1", "iloveyou", "princess", "abcd1234", "qwertyui", "qwerty", "football", "baseball", "michael", "jennifer", "michelle", "babygirl", "superman", "12345678", "123456789", "1234567890", "123123123", "69696969", "11111111", "22222222", "33333333", "44444444", "55555555", "66666666", "77777777", "88888888", "99999999", "00000000"];
        for (n = n.toLowerCase(), i = 0; i < t.length; i++)
            if (n === t[i]) return !0;
        return /^[\s]*$/.test(n) ? !0 : !1
    }

    function e(n) {
        return (n = n.toLowerCase(), n.indexOf("asdf") > -1) ? !0 : n.indexOf("pass") > -1 || n.indexOf("qwer") > -1 || n.indexOf("zxcv") > -1 || n.indexOf("aaaa") > -1 || n.indexOf("zzzz") > -1 ? !0 : /^[\s]*$/.test(n) ? !0 : !1
    }

    function f(n) {
        var r = /^\s$/,
            i = 0,
            t;
        if (n == null || n == "") return 0;
        for (t = 0; t < n.length; t++) n.charAt(t).match(r) && (i += 1);
        return i
    }

    function u(n) {
        var r = /^[A-Za-z]$/,
            i = 0,
            t;
        if (n == null || n == "") return 0;
        for (t = 0; t < n.length; t++) n.charAt(t).match(r) && (i += 1);
        return i
    }

    function y(n) {
        var r = /^[0-9]$/,
            i = 0,
            t;
        if (n == null || n == "") return 0;
        for (t = 0; t < n.length; t++) n.charAt(t).match(r) && (i += 1);
        return i
    }
    var n, t;
    return v2UsernameAndPasswordRulesEnabled = parseInt($(".new-username-pwd-rule").attr("data-v2-username-password-rules-enabled")), n = 0, t = {
        invalidBirthday: it,
        selectedBirthday: r,
        genderSelected: tt,
        usernameTooLong: nt,
        usernameTooShort: g,
        usernameRegexInvalid: b,
        usernameInvalid: w,
        usernameStartsOrEndsWithUnderscore: v2UsernameAndPasswordRulesEnabled == 1 ? d : null,
        usernameHasMoreThanOneUnderscore: v2UsernameAndPasswordRulesEnabled == 1 ? k : null,
        passwordIsUsername: s,
        passwordsMatch: p,
        weakPassword: v2UsernameAndPasswordRulesEnabled == 1 ? o : e,
        passwordTooShort: v2UsernameAndPasswordRulesEnabled == 1 ? rt : a,
        passwordTooLong: v2UsernameAndPasswordRulesEnabled == 1 ? null : v,
        passwordEnoughLetters: v2UsernameAndPasswordRulesEnabled == 1 ? null : l,
        passwordEnoughNumbers: v2UsernameAndPasswordRulesEnabled == 1 ? null : c,
        passwordContainsSpaces: v2UsernameAndPasswordRulesEnabled == 1 ? null : h
    }
}(), typeof Roblox.Resources == "undefined" && (Roblox.Resources = {}), typeof Roblox.Resources.AnimatedSignupFormValidator == "undefined" && v2UsernameAndPasswordRulesEnabled == 1 ? Roblox.Resources.AnimatedSignupFormValidator = {
    doesntMatch: "Passwords don't match",
    requiredField: "Required",
    userNameRange: "Usernames can be 3 to 20 characters long",
    tooShort: "Must be at least 8 characters long",
    maxValid: "Too many accounts use this email",
    weakKey: "Please create a more complex password",
    invalidCharacters: "Spaces and special characters are not allowed.",
    invalidName: "Can't be your avatar name",
    alreadyTaken: "This username is already in use.",
    cantBeUsed: "Username not appropriate for ROBLOX",
    invalidBirthday: "Invalid birthday",
    loginFieldsRequired: "Username and Password are required",
    loginFieldsIncorrect: "Your username or password is incorrect.",
    invalidEmail: "Invalid email",
    passwordIsUsername: "Password shouldn't match username",
    startsOrEndsWithUnderscore: "Username can’t start or end with _",
    moreThanOneUnderscore: "Usernames can have at most one _"
} : typeof Roblox.Resources.AnimatedSignupFormValidator == "undefined" && (Roblox.Resources.AnimatedSignupFormValidator = {
    doesntMatch: "Passwords don't match",
    requiredField: "Required",
    tooLong: "Too long",
    tooShort: "Too short",
    maxValid: "Too many accounts use this email",
    needsFourLetters: "Needs 4 letters",
    needsTwoNumbers: "Needs 2 numbers",
    noSpaces: "No spaces allowed",
    weakKey: "Weak key combination.",
    invalidCharacters: "Spaces and special characters are not allowed.",
    invalidName: "Can't be your avatar name",
    alreadyTaken: "Already taken",
    cantBeUsed: "Can't be used",
    invalidBirthday: "Invalid birthday",
    loginFieldsRequired: "Username and Password are required.",
    loginFieldsIncorrect: "Your username or password is incorrect.",
    invalidEmail: "Invalid email",
    passwordIsUsername: "Password shouldn't match username."
});