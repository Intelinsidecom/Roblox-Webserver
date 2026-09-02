// Login/loginForm.js
var Freebloxia = Freebloxia || {};
Freebloxia.Login = Freebloxia.Login || {}, Freebloxia.Login.form = function() {
    function f(t, i) {
        n.toggleClass(p, !i), n.toggleClass(k, i), i ? n.removeAttr("disabled") : n.attr("disabled", "")
    }

    function v(n) {
        n ? i.parent().removeClass("hidden") : i.parent().addClass("hidden")
    }

    function l(t) {
        c.text(t), i.html(c), v(t.length > 0), f(n, t.length <= 0)
    }

    function d(n) {
        return w + "?" + $.param({
            username: n
        })
    }

    function g() {
        return !t.hasClass("hidden")
    }

    function a() {
        t.addClass("hidden"), f(n, !0), s()
    }

    function o(t) {
        var i = "";
        return r.val() ? u.val() ? h && g() && (i = Freebloxia.Login.Resources.errors.captchaRequired.message) : i = u.data(y) : i = r.data(y), t ? l(i) : f(n, i.length <= 0), i.length <= 0
    }

    function nt(n) {
        if (t.removeClass("hidden"), Freebloxia.Captcha.setEndpoint(Freebloxia.Captcha.types.login, d(n)), b) {
            Freebloxia.Captcha.reset(Freebloxia.Captcha.types.login, a);
            return
        }
        Freebloxia.Captcha.render(t.attr("id"), Freebloxia.Captcha.types.login, a), b = !0, Freebloxia.Captcha.execute()
    }

    function s() {
        if (!n.hasClass(p)) {
            if (h) {
                if (!o(!0)) return
            } else {
                $("#loginForm").submit();
                return
            }
            f(n, !1);
            var t = r.val(),
                i = u.val();
            Freebloxia.Login.service.login(t, i).done(function(n) {
                if (e.length > 0) {
                    n.isTwoStepVerificationRequired && e.attr("action", n.redirectUrl), e.submit();
                    return
                }
                window.location = n.redirectUrl
            }).fail(function(n) {
                switch (n[0].code) {
                    case Freebloxia.Login.Resources.errors.captchaRequired.code:
                        nt(t);
                        break;
                    case Freebloxia.Login.Resources.errors.usernamePasswordRequired.code:
                    case Freebloxia.Login.Resources.errors.invalidCredentials.code:
                    default:
                        l(n[0].message)
                }
            })
        }
    }

    function tt(l) {
        h = l, e = $("#samlRequest"), r = $("#Username"), u = $("#Password"), n = $("[freebloxia-js-onclick]"), i = $(".validation-summary-errors > ul"), c = $("<li>"), t = $("#captchaPanel"), Freebloxia.Captcha && (w = Freebloxia.Captcha.getEndpoint(Freebloxia.Captcha.types.login)), n.click(function() {
            return s(), !1
        }), $("[freebloxia-js-oncancel]").click(function() {
            return window.close(), !1
        });
        r.on("blur keyup change", function() {
            o(!1)
        });
        u.one("animationstart", function() {
            f(n, !0)
        }).on("blur keyup change", function(n) {
            return o(!1), n.keyCode === 13 ? (s(), !1) : void 0
        });
        l && t.addClass("hidden"), v(i.find("li").length > 0), o(!1)
    }
    var h = !1,
        e, i, c, r, u, n, t, w = "",
        p = "btn-disabled-neutral",
        k = "btn-neutral",
        y = "val-required",
        b = !1;
    return {
        init: tt,
        submit: s,
        showError: l
    }
}();