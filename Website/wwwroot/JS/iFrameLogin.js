// iFrameLogin.js
typeof Roblox == "undefined" && (Roblox = {}), Roblox.iFrameLogin = new function() {
    function e() {
        var o = $(document.body).data("captchaon"),
            s = !1,
            l = !0,
            v = function(n) {
                var t = $(document.body).data("parent-url");
                $.postMessage("resize," + n, t, parent)
            },
            vt = function() {
                try {
                    var de = document.documentElement;
                    var calc = Math.max(
                        $("#LoginForm").outerHeight(true) || 0,
                        document.body ? document.body.scrollHeight : 0,
                        document.body ? document.body.offsetHeight : 0,
                        de ? de.clientHeight : 0,
                        de ? de.scrollHeight : 0,
                        de ? de.offsetHeight : 0
                    );
                    var h = calc;
                    if (h && h > 0) {
                        v(h + "px");
                    }
                } catch (ex) {
                    // ignore
                }
            },
            h = null;
        // initial resize to content and schedule a few follow-ups
        vt();
        setTimeout(vt, 0);
        setTimeout(vt, 200);
        setTimeout(vt, 500);
        var y = function() {
                var n = $(document.body).data("parent-url");
                n.indexOf("#") != -1 && (n = n.split("#")[0]), n += n.indexOf("?") == -1 ? "?nl=true" : "&nl=true", window.parent.location = n
            },
            c = function(n) {
                if (n) {
                    $("#LoggingInStatus").addClass("active").show();
                } else {
                    $("#LoggingInStatus").removeClass("active").hide();
                }
            },
            w = function() {
                var n = !1,
                    t = [$("#Password"), $("#UserName")];
                return o && t.push($("#recaptcha_response_field")), jQuery.each(t, function() {
                    var t = $(this);
                    t.val() == "" ? (e(t, !0), n = !0) : e(t, !1)
                }), n
            },
            e = function(n, t) {
                s = !1, c(!1), t ? n.css({
                    "background-color": "#FDD"
                }) : n.css({
                    "background-color": "white"
                })
            },
            p = function(n, t, i, r) {
                var u = Roblox.iFrameLogin.Resources.requestCodeUnauthenticatedPath,
                    f = {
                        username: n,
                        password: t,
                        actionType: Roblox.TwoStepVerificationModal.ActionTypes.SignIn
                    };
                $.ajax({
                    type: "POST",
                    url: u,
                    data: f,
                    crossDomain: !0,
                    xhrFields: {
                        withCredentials: !0
                    },
                    success: i,
                    error: r
                })
            },
            b = function() {
                var n = function() {
                        var n, t;
                        $("#TwoStepVerificationNewCodeButton").hide(), $("#TwoStepVerificationSubmitButton").show(), n = $("#TwoStepVerificationMessage"), n.text(Roblox.iFrameLogin.Resources.enterTwoStepCodeMessage), t = $("#TwoStepVerificationCodeInput"), t.css("background-color", "white")
                    },
                    t = function(n) {
                        var i = $("#TwoStepVerificationNewCodeButton"),
                            t = null,
                            r;
                        switch (n.status) {
                            case 403:
                                r = JSON.parse(n.responseText), r.message === "Flooded" ? (t = Roblox.iFrameLogin.Resources.floodedTwoStepMessage, i.addClass("disabled")) : r.message === "VerifyEmail" ? (t = Roblox.iFrameLogin.Resources.verifyEmailMessage, i.addClass("disabled")) : (t = Roblox.iFrameLogin.Resources.unknownErrorText, i.addClass("disabled"));
                                break;
                            default:
                                t = Roblox.iFrameLogin.Resources.unknownErrorText, i.addClass("disabled")
                        }
                        $("#TwoStepVerificationMessage").text(t)
                    },
                    i = $("#UserName").val();
                p(i, h, n, t)
            },
            k = function() {
                var n = function() {
                        $("#Password").val(h), a()
                    },
                    t = function(n) {
                        var t, r;
                        $("#TwoStepVerificationSubmitButton").hide(), t = $("#TwoStepVerificationNewCodeButton"), t.show();
                        var u = $("#TwoStepVerificationCodeInput"),
                            i = null,
                            f = !1;
                        switch (n.status) {
                            case 403:
                                r = JSON.parse(n.responseText), r.message === "Flooded" ? (i = Roblox.iFrameLogin.Resources.floodedTwoStepMessage, t.addClass("disabled")) : r.message === "VerifyEmail" ? (i = Roblox.iFrameLogin.Resources.verifyEmailMessage, t.addClass("disabled")) : r.message === "InvalidCode" ? (i = Roblox.iFrameLogin.Resources.invalidCodeMessage, f = !0) : (i = Roblox.iFrameLogin.Resources.unknownErrorText, t.addClass("disabled"));
                                break;
                            default:
                                i = Roblox.iFrameLogin.Resources.unknownErrorText, t.addClass("disabled")
                        }
                        u.val(""), f && u.css("background-color", "#FDD"), $("#TwoStepVerificationMessage").text(i)
                    },
                    i = Roblox.iFrameLogin.Resources.verifyCodeUnauthenticatedPath,
                    r = {
                        username: $("#UserName").val(),
                        password: h,
                        actionType: Roblox.TwoStepVerificationModal.ActionTypes.SignIn,
                        code: $("#TwoStepVerificationCodeInput").val()
                    };
                $.ajax({
                    type: "POST",
                    url: i,
                    data: r,
                    crossDomain: !0,
                    xhrFields: {
                        withCredentials: !0
                    },
                    success: n,
                    error: t
                })
            },
            a = function() {
                var v, b, d;
                if (w()) return !1;
                if (l) return e($("#UserName"), !0), !1;
                s = !0, c(!0);
                var nt = $("#UserName"),
                    tt = $("#Password"),
                    a = nt.val(),
                    k = tt.val();
                if (h = k, v = "", b = "", o && (v = $("#recaptcha_challenge_field").val(), b = $("#recaptcha_response_field").val(), v == "" || b == "")) return e($("#recaptcha_response_field"), !0), !1;
                if (o && $("#Captcha_upBadCaptcha").text(""), Roblox.iFrameLogin.Resources.useSignOnApi) {
                    var it = {
                            username: a,
                            password: k,
                            recaptcha_challenge_field: v,
                            recaptcha_response_field: b
                        },
                        d = function() {
                            y()
                        },
                        g = function(n) {
                            var t, i, r;
                            if (n.status === 403) {
                                t = JSON.parse(n.responseText);
                                switch (t.message) {
                                    case "Credentials":
                                        e($("#Password"), !0), $("#NotAMemberLink").hide(), $("#ForgotPasswordLink").show();
                                        break;
                                    case "CaptchaIncorrect":
                                        e($("#Password"), !1), $("#Captcha_upBadCaptcha").show(), $("#Captcha_upBadCaptcha").css("color", "red"), $("#Captcha_upBadCaptcha").text(Roblox.iFrameLogin.Resources.invalidCaptchaEntry);
                                        break;
                                    case "CaptchaMissing":
                                        e($("#Password"), !1), $("#Captcha_upBadCaptcha").show(), $("#Captcha_upBadCaptcha").css("color", "red");
                                        break;
                                    case "TwoStepVerification":
                                        i = function() {
                                            $("#credentials-section").hide(), $("#two-step-verification-section").show()
                                        }, r = function(n) {
                                            var t, i;
                                            $("#credentials-section").hide(), $("#two-step-verification-section").show(), t = null;
                                            switch (n.status) {
                                                case 403:
                                                    i = JSON.parse(n.responseText), t = i.message === "Flooded" ? Roblox.iFrameLogin.Resources.floodedTwoStepMessage : i.message === "VerifyEmail" ? Roblox.iFrameLogin.Resources.verifyEmailMessage : Roblox.iFrameLogin.Resources.unknownErrorText;
                                                    break;
                                                default:
                                                    t = Roblox.iFrameLogin.Resources.unknownErrorText
                                            }
                                            $("#TwoStepVerificationMessage").text(t), $("#TwoStepVerificationSubmitButton").addClass("disabled")
                                        }, p(a, k, i, r)
                                }
                            }
                            return o && Recaptcha.reload("t"), $("#Password").val(""), $("#Password").focus(), s = !1, c(!1), !1
                        };
                    $.ajax({
                        type: "POST",
                        url: Roblox.iFrameLogin.Resources.signOnApiPath,
                        data: it,
                        crossDomain: !0,
                        xhrFields: {
                            withCredentials: !0
                        },
                        success: d,
                        error: g
                    })
                } else d = g = function(h) {
                    if (h.IsValid) y();
                    else return h.ErrorCode.indexOf(f) !== -1 ? (window.parent.location = "/Login/ResetPasswordRequest.aspx?needsReset=1", !1) : h.ErrorCode.indexOf(r) != -1 ? (a != "" && window.location.href.indexOf("username") == -1 ? window.location.href = window.location.href + "&username=" + a : window.location.reload(), !1) : (h.ErrorCode.indexOf(t) != -1 && (window.parent.location = "/login/twofactorauth?username=" + encodeURIComponent(a)), h.ErrorCode.indexOf(u) != -1 ? (e($("#Password"), !0), $("#NotAMemberLink").hide(), $("#ForgotPasswordLink").show()) : h.ErrorCode.indexOf(n) != -1 ? $("#ErrorMessage").text(h.Message) : h.ErrorCode.indexOf(i) != -1 ? $("#ErrorMessage").text(h.Message) : (e($("#Password"), !1), $("#Captcha_upBadCaptcha").show(), $("#Captcha_upBadCaptcha").css("color", "red"), h.Message == "incorrect-captcha-sol" ? $("#Captcha_upBadCaptcha").text(Roblox.iFrameLogin.Resources.invalidCaptchaEntry) : $("#Captcha_upBadCaptcha").text(h.Message)), o && Recaptcha.reload("t"), $("#Password").val(""), $("#Password").focus(), s = !1, c(!1), !1)
                }, Roblox.Website.Services.Secure.LoginService.ValidateLogin(a, k, o, v, b, d, g)
            },
            d = function() {
                var n = $("#UserName").val(),
                    t = onError = function(n) {
                        e($("#UserName"), !n.success), l = !n.success, n.success || ($("#NotAMemberLink").show(), $("#ForgotPasswordLink").show())
                    };
                n != "" && $.ajax({
                    type: "GET",
                    url: "/UserCheck/doesusernameexist?username=" + n,
                    success: t,
                    error: onError
                })
            };
        $("#LoginButton").click(function() {
            a()
        }), $("#TwoStepVerificationSubmitButton").click(function() {
            k()
        }), $("#TwoStepVerificationCancelButton").click(function() {
            var n, t;
            $("#TwoStepVerificationNewCodeButton").hide(), n = $("#TwoStepVerificationSubmitButton"), n.show(), n.removeClass("disabled"), $("#TwoStepVerificationMessage").text(Roblox.iFrameLogin.Resources.enterTwoStepCodeMessage), t = $("#TwoStepVerificationCodeInput"), t.val(""), t.css("background-color", "white"), $("#two-step-verification-section").hide(), $("#credentials-section").show()
        }), $("#TwoStepVerificationNewCodeButton").click(function() {
            b()
        }), $("#UserName").blur(function() {
            d()
        }), $(document).keydown(function(n) {
            if (n.which == 13 && !s) return a(), !1
        }), $(function() {
            var n = 1;
            $("input,select").each(function() {
                if (this.type != "hidden") {
                    var t = $(this);
                    t.attr("tabindex", n), n++
                }
            });
            vt();
        }), $(function() {
            $("#UserName").val() != "" || $("#UserName").val() != undefined, l = !1
        }), $(function() {
            $("#CaptchaContainer").css({
                "margin-left": "0",
                "margin-top": "8px",
                "margin-bottom": "5px",
                width: "none"
            });
            vt();
        }), $(window).resize(function() {
            vt();
        });
        // Observe DOM changes to adjust height dynamically
        if (window.MutationObserver) {
            try {
                var observer = new MutationObserver(function() { vt(); });
                observer.observe(document.body, { attributes: true, childList: true, subtree: true });
            } catch (eobs) { /* ignore */ }
        }
    }

    var o = "1",
        s = "2",
        n = "3",
        t = "4",
        i = "5",
        r = "6",
        u = "7",
        h = "8",
        f = "10";
    return {
        init: e
    }
};