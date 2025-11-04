// Login/Login.js
function addMonths() {
    $("<option>").attr("value", 1).text(Roblox.Login.Resources.january).appendTo('select[name="Month"]'), $("<option>").attr("value", 2).text(Roblox.Login.Resources.february).appendTo('select[name="Month"]'), $("<option>").attr("value", 3).text(Roblox.Login.Resources.march).appendTo('select[name="Month"]'), $("<option>").attr("value", 4).text(Roblox.Login.Resources.april).appendTo('select[name="Month"]'), $("<option>").attr("value", 5).text(Roblox.Login.Resources.may).appendTo('select[name="Month"]'), $("<option>").attr("value", 6).text(Roblox.Login.Resources.june).appendTo('select[name="Month"]'), $("<option>").attr("value", 7).text(Roblox.Login.Resources.july).appendTo('select[name="Month"]'), $("<option>").attr("value", 8).text(Roblox.Login.Resources.august).appendTo('select[name="Month"]'), $("<option>").attr("value", 9).text(Roblox.Login.Resources.september).appendTo('select[name="Month"]'), $("<option>").attr("value", 10).text(Roblox.Login.Resources.october).appendTo('select[name="Month"]'), $("<option>").attr("value", 11).text(Roblox.Login.Resources.november).appendTo('select[name="Month"]'), $("<option>").attr("value", 12).text(Roblox.Login.Resources.december).appendTo('select[name="Month"]')
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
                        window.location.href = i + "?nl=true"
                    },
                    l = function(i) {
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
                                            actionType: Roblox.TwoStepVerificationModal.ActionTypes.SignIn,
                                            onSuccess: t
                                        }, Roblox.TwoStepVerificationModal.open(o);
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
            } else $("#loginForm").submit();
            return !1
        };
    $("[roblox-js-onclick]").click(t), $("[roblox-js-onsignup]").click(function() {
        var n = $("#ReturnUrl").val(),
            t = "/account/signupredir";
        return typeof n == "string" && n.length > 0 && (t += "?returnUrl=" + encodeURIComponent(n)), window.location = t, !1
    }), $("[roblox-js-oncancel]").click(function() {
        return window.close(), !1
    }), $("#MonthSelect").change(function() {
        var n = $('select[name="Month"] option:selected').val(),
            t = $('select[name="Day"] option:selected').val(),
            i = $('select[name="Year"] option:selected').val(),
            r = String.format("/IDE/Login?isSignup=true&month={0}&day={1}&year={2}", n, t, i);
        $("[roblox-js-onsignup]").attr("href", r)
    }), $("#DaySelect").change(function() {
        var n = $('select[name="Month"] option:selected').val(),
            t = $('select[name="Day"] option:selected').val(),
            i = $('select[name="Year"] option:selected').val(),
            r = String.format("/IDE/Login?isSignup=true&month={0}&day={1}&year={2}", n, t, i);
        $("[roblox-js-onsignup]").attr("href", r)
    }), $("#YearSelect").change(function() {
        var n = $('select[name="Month"] option:selected').val(),
            t = $('select[name="Day"] option:selected').val(),
            i = $('select[name="Year"] option:selected').val(),
            r = String.format("/IDE/Login?isSignup=true&month={0}&day={1}&year={2}", n, t, i);
        $("[roblox-js-onsignup]").attr("href", r)
    }), $("#errorBanner").text().trim().length == 0 && $("#errorBanner").hide(), $("#Username").focus(), $("#loginForm").keydown(function(n) {
        if (n.which == 13) return t(), !1
    })
});