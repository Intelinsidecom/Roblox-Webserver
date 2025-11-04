// TwoStepVerificationModal.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.TwoStepVerificationModal == "undefined" && (Roblox.TwoStepVerificationModal = function() {
    function t(n) {
        switch (n) {
            case Roblox.TwoStepVerificationModal.ActionTypes.SignIn:
                return !0
        }
        return !1
    }

    function i(n) {
        var c = {
                actionType: 0,
                onSuccess: null,
                onError: null
            },
            i;
        n = $.extend({}, c, n);
        var s = {
                titleText: "Enter Verification Code",
                bodyContent: "Enter the verification code sent to your email.<br/><div id='codeInputValidation' class='validationTooltip'></div><br/><input id='verificationCodeInput' type='text'/>",
                acceptText: "Submit",
                onAccept: function() {
                    e(n)
                },
                declineText: "Cancel",
                allowHtmlContentInBody: !0,
                fieldValidationRequired: !0,
                onOpenCallback: o
            },
            f = null,
            h = t(n.actionType);
        if (f = h ? $("#TwoStepVerificationApiPaths").data("request-code-unauthenticated") : $("#TwoStepVerificationApiPaths").data("request-code"), i = {
                actionType: n.actionType
            }, h) {
            if (n.username === undefined) throw "'username' required in properties for unauthenticated requests";
            if (n.password === undefined) throw "'password' required in properties for unauthenticated requests";
            i.username = n.username, i.password = n.password
        }
        $.ajax({
            type: "POST",
            url: f,
            data: i,
            crossDomain: !0,
            xhrFields: {
                withCredentials: !0
            },
            success: function(n) {
                var t = r(n.mediaType);
                s.bodyContent = "Enter the verification code sent via " + t + ".<br/><div id='codeInputValidation' class='validationTooltip'></div><br/><input id='verificationCodeInput' type='text'/>", Roblox.GenericConfirmation.open(s)
            },
            error: u
        })
    }

    function u(t) {
        if (t.getResponseHeader(n) !== null)
            if (t.status === 403) {
                var i = JSON.parse(t.responseText);
                i.message === "Flooded" ? Roblox.GenericModal.open("Slow Down", "/images/Icons/img-alert.png", "You have tried to do this too many times. Try again later.") : i.message === "VerifyEmail" ? Roblox.GenericModal.open("Verify Email", "/images/Icons/img-alert.png", "You must have a verified email address.", function() {
                    window.location.href = "/my/account"
                }) : i.message === "EnableTwoStep" ? Roblox.GenericModal.open("Disabled", "/images/Icons/img-alert.png", "You must enable two step verification before requesting a code.") : i.message === "Credentials" && Roblox.GenericModal.open("Invalid Username/Password", "/images/Icons/img-alert.png", "The username/password combination was incorrect.")
            } else t.status === 404 ? Roblox.GenericModal.open("Disabled", "/images/Icons/img-alert.png", "This feature has been disabled.") : t.status === 500 && Roblox.GenericModal.open("Error", "/images/Icons/img-alert.png", "Sorry, an unknown error has occured.")
    }

    function f(t, i) {
        if (i.getResponseHeader(n) !== null)
            if (i.status === 403) {
                var r = JSON.parse(i.responseText);
                r.message === "Flooded" ? Roblox.GenericModal.open("Slow Down", "/images/Icons/img-alert.png", "You have tried to do this too many times. Try again later.") : r.message === "VerifyEmail" ? Roblox.GenericModal.open("Verify Email", "/images/Icons/img-alert.png", "You must have a verified email address.", function() {
                    window.location.href = "/my/account"
                }) : r.message === "EnableTwoStep" ? Roblox.GenericModal.open("Disabled", "/images/Icons/img-alert.png", "You must enable two step verification before requesting a code.") : r.message === "Credentials" ? Roblox.GenericModal.open("Invalid Username/Password", "/images/Icons/img-alert.png", "The username/password combination was incorrect.") : r.message === "InvalidCode" && Roblox.GenericModal.open("Code Invalid", "/images/Icons/img-alert.png", "Sorry, but the code you entered is invalid or has expired.")
            } else i.status === 404 ? Roblox.GenericModal.open("Disabled", "/images/Icons/img-alert.png", "This feature has been disabled.") : i.status === 500 && Roblox.GenericModal.open("Error", "/images/Icons/img-alert.png", "Sorry, an unknown error has occured.");
        else if (t.onError !== null && t.onError !== undefined) t.onError(i)
    }

    function e(n) {
        var e = $("#verificationCodeInput").val(),
            r = null,
            u = t(n.actionType),
            i;
        if (r = u ? $("#TwoStepVerificationApiPaths").data("verify-code-unauthenticated") : $("#TwoStepVerificationApiPaths").data("verify-code"), i = {
                code: e,
                actionType: n.actionType
            }, u) {
            if (n.username === undefined) throw "'username' required in properties for unauthenticated verifications";
            if (n.password === undefined) throw "'password' required in properties for unauthenticated verifications";
            i.username = n.username, i.password = n.password
        }
        return $.ajax({
            type: "POST",
            url: r,
            data: i,
            crossDomain: !0,
            xhrFields: {
                withCredentials: !0
            },
            success: n.onSuccess,
            error: function(t) {
                f(n, t)
            }
        }), !0
    }

    function o() {
        var n = "0123456789",
            t = 6;
        $("#verificationCodeInput").on("input propertychange paste", function() {
            for (var r = $("#verificationCodeInput").val(), o = !1, e, f, i, u = 0; u < r.length; u++) {
                for (e = !1, f = 0; f < n.length; f++)
                    if (r.charAt(u) === n.charAt(f)) {
                        e = !0;
                        break
                    } if (!e) {
                    o = !0;
                    break
                }
            }
            if (i = $("#codeInputValidation"), o) {
                i.text("Contains invalid characters"), i.show();
                return
            }
            if (r.length > 0 && r.length < t) {
                i.text("Too short"), i.show();
                return
            }
            if (r.length > t) {
                i.text("Too long"), i.show();
                return
            }
            i.hide()
        })
    }
    var n = "Two-Step-Verification",
        r = function(n) {
            return n === "Sms" ? "SMS" : n === "Email" ? "email" : n
        },
        s = {
            SignIn: 0,
            PasswordChange: 1,
            EnableTwoStep: 2,
            ChangeEmail: 3
        };
    $(document).on("keypress", "#verificationCodeInput", function(n) {
        if (n.which == 13) return Roblox.GenericConfirmation.clickYes(), !1
    });
    return {
        open: i,
        ActionTypes: s
    }
}());