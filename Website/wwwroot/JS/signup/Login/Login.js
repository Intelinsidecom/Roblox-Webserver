// Login/Login.js
function addMonths() {
    if (!window.Freebloxia) {
        window.Freebloxia = {};
    }
    if (!Freebloxia.Login) {
        Freebloxia.Login = {};
    }
    if (!Freebloxia.Login.Resources) {
        Freebloxia.Login.Resources = {
            january: "January",
            february: "February",
            march: "March",
            april: "April",
            may: "May",
            june: "June",
            july: "July",
            august: "August",
            september: "September",
            october: "October",
            november: "November",
            december: "December"
        };
    }
    $("<option>").attr("value", 1).text(Freebloxia.Login.Resources.january).appendTo('select[name="Month"]'), $("<option>").attr("value", 2).text(Freebloxia.Login.Resources.february).appendTo('select[name="Month"]'), $("<option>").attr("value", 3).text(Freebloxia.Login.Resources.march).appendTo('select[name="Month"]'), $("<option>").attr("value", 4).text(Freebloxia.Login.Resources.april).appendTo('select[name="Month"]'), $("<option>").attr("value", 5).text(Freebloxia.Login.Resources.may).appendTo('select[name="Month"]'), $("<option>").attr("value", 6).text(Freebloxia.Login.Resources.june).appendTo('select[name="Month"]'), $("<option>").attr("value", 7).text(Freebloxia.Login.Resources.july).appendTo('select[name="Month"]'), $("<option>").attr("value", 8).text(Freebloxia.Login.Resources.august).appendTo('select[name="Month"]'), $("<option>").attr("value", 9).text(Freebloxia.Login.Resources.september).appendTo('select[name="Month"]'), $("<option>").attr("value", 10).text(Freebloxia.Login.Resources.october).appendTo('select[name="Month"]'), $("<option>").attr("value", 11).text(Freebloxia.Login.Resources.november).appendTo('select[name="Month"]'), $("<option>").attr("value", 12).text(Freebloxia.Login.Resources.december).appendTo('select[name="Month"]')
}

function addDays() {
    for (var n = 1; n <= 31; n++) $("<option>").attr("value", n).text(n).appendTo('select[name="Day"]')
}

function addYears() {
    for (var n = (new Date).getFullYear(); n >= (new Date).getFullYear() - 100; n--) $("<option>").attr("value", n).text(n).appendTo('select[name="Year"]')
}
$(function() {
    addDays(), addMonths(), addYears();
    var n = "/newlogin",
        i = "/home",
        r = {
            credentials: 3
        },
        t = function() {
            var s = $("#signInButtonPanel").attr("data-use-apiproxy-signin") === "True";
            if (s) {
                var u = $("#Username").val(),
                    f = $("#Password").val(),
                    e = "",
                    o = "",
                    h = $("#loginarea").attr("data-is-captcha-on");
                h && (e = $("#recaptcha_challenge_field").val(), o = $("#recaptcha_response_field").val());
                var c = function() {
                        console.log("Login.js: main login success, redirecting to", i + "?nl=true");
                        window.location.href = i + "?nl=true"
                    },
                    l = function(i) {
                        console.error("Login.js: main login error", i && i.status, i && i.responseText);

                        var e, o;
                        switch (i.status) {
                            case 403:
                                e = JSON.parse(i.responseText);
                                switch (e.message) {
                                    case "Credentials":
                                        window.location.href = n + "?failureReason=" + r.credentials;
                                        break;
                                    case "TwoStepVerification":
                                        o = {
                                            username: u,
                                            password: f,
                                            actionType: Freebloxia.TwoStepVerificationModal.ActionTypes.SignIn,
                                            onSuccess: t
                                        }, Freebloxia.TwoStepVerificationModal.open(o);
                                        break;
                                    default:
                                        window.location.href = n
                                }
                                break;
                            default:
                                window.location.href = n
                        }
                    },
                    a = $("#signInButtonPanel").attr("data-sign-on-api-path"),
                    v = {
                        username: u,
                        password: f,
                        recaptcha_challenge_field: e,
                        recaptcha_response_field: o
                    };
                $.ajax({
                    type: "POST",
                    url: a,
                    data: v,
                    crossDomain: !0,
                    xhrFields: {
                        withCredentials: !0
                    },
                    success: c,
                    error: l
                })
            } else {
                console.log("Login.js: falling back to #loginForm submit");
                $("#loginForm").submit();
            }

            return !1
        };
    // IE11 compatible event binding
    var signInButtons = document.querySelectorAll('[freebloxia-js-onclick]');
    for (var i = 0; i < signInButtons.length; i++) {
        if (signInButtons[i].addEventListener) {
            signInButtons[i].addEventListener('click', t);
        } else if (signInButtons[i].attachEvent) {
            signInButtons[i].attachEvent('onclick', t);
        }
    }
    var signUpButtons = document.querySelectorAll('[freebloxia-js-onsignup]');
    for (var i = 0; i < signUpButtons.length; i++) {
        (function(button) {
            var handler = function() {
                var n = $("#ReturnUrl").val(),
                    t = "/account/signupredir";
                return typeof n == "string" && n.length > 0 && (t += "?returnUrl=" + encodeURIComponent(n)), window.location = t, !1
            };
            if (button.addEventListener) {
                button.addEventListener('click', handler);
            } else if (button.attachEvent) {
                button.attachEvent('onclick', handler);
            }
        })(signUpButtons[i]);
    }
    var cancelButtons = document.querySelectorAll('[freebloxia-js-oncancel]');
    for (var i = 0; i < cancelButtons.length; i++) {
        (function(button) {
            var handler = function() {
                return window.close(), !1
            };
            if (button.addEventListener) {
                button.addEventListener('click', handler);
            } else if (button.attachEvent) {
                button.attachEvent('onclick', handler);
            }
        })(cancelButtons[i]);
    }
    $("#MonthSelect").change(function() {
        var n = $('select[name="Month"] option:selected').val(),
            t = $('select[name="Day"] option:selected').val(),
            i = $('select[name="Year"] option:selected').val(),
            r = String.format("/IDE/Login?isSignup=true&month={0}&day={1}&year={2}", n, t, i);
        $("[freebloxia-js-onsignup]").attr("href", r)
    }), $("#DaySelect").change(function() {
        var n = $('select[name="Month"] option:selected').val(),
            t = $('select[name="Day"] option:selected').val(),
            i = $('select[name="Year"] option:selected').val(),
            r = String.format("/IDE/Login?isSignup=true&month={0}&day={1}&year={2}", n, t, i);
        $("[freebloxia-js-onsignup]").attr("href", r)
    }), $("#YearSelect").change(function() {
        var n = $('select[name="Month"] option:selected').val(),
            t = $('select[name="Day"] option:selected').val(),
            i = $('select[name="Year"] option:selected').val(),
            r = String.format("/IDE/Login?isSignup=true&month={0}&day={1}&year={2}", n, t, i);
        $("[freebloxia-js-onsignup]").attr("href", r)
    }), $("#errorBanner").text().trim().length == 0 && $("#errorBanner").hide(), $("#Username").focus(), $("#loginForm").keydown(function(n) {
        if (n.which == 13) return t(), !1
    });

    var $headerForm = $("#LogInForm");
    if ($headerForm.length) {
        $headerForm.on("submit", function(ev){
            ev.preventDefault();
            var u = $("#LoginUsername").val() || "";
            var f = $("#LoginPassword").val() || "";
            var a = $("#signInButtonPanel").attr("data-sign-on-api-path") || "/login/v1";
            console.log("Login.js: header login submit", { apiPath: a, usernameLength: u.length });
            var c = function(resp){
                console.log("Login.js: header login success", resp);
                window.location.href = i + "?nl=true";
            };
            var l = function(xhr){
                console.error("Login.js: header login error", xhr && xhr.status, xhr && xhr.responseText);
                try {
                    if (xhr && xhr.status === 403) {
                        var e = JSON.parse(xhr.responseText || "{}");
                        if (e && e.message === "Credentials") { window.location.href = n + "?failureReason=" + r.credentials; return; }
                    }
                } catch(ex){}
                window.location.href = n;
            };
            $.ajax({
                type: "POST",
                url: a,
                data: { username: u, password: f },
                crossDomain: !0,
                xhrFields: { withCredentials: !0 },
                success: c,
                error: l
            });
        });

    }

})
;