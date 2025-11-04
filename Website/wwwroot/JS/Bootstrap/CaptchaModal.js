// Bootstrap/CaptchaModal.js
Roblox = Roblox || {}, Roblox.Bootstrap = Roblox.Bootstrap || {}, Roblox.Resources = Roblox.Resources || {}, Roblox.Resources.CaptchaModal = {
    title: "Are you human?",
    message: "To finish, please verify that you are human.",
    captchaEmptyMessage: "The CAPTCHA field should not be empty, please fill it.",
    captchaErrorMessage: "The CAPTCHA you entered is invalid. Please try again.",
    finish: "Finish"
}, Roblox.Bootstrap.CaptchaModal = function() {
    "use strict";

    function t(t) {
        function r() {
            i.hide(), i.appendTo(t)
        }

        function u() {
            Roblox.Bootstrap.GenericConfirmation.disableButtons(), i.hide(), i.appendTo(t), i.removeClass("roblox-captcha-modal"), t.submit()
        }

        function f() {
            var n = $("#BootstrapConfirmationModal"),
                t;
            n.find("#roblox-decline-btn").hide(), t = n.find(".modal-body"), i.appendTo(t), i.show()
        }
        var i = t.find(n),
            e = t.find(n).data("captcha-valid");
        if (typeof Roblox.Bootstrap.GenericConfirmation == "undefined") {
            i.remove(), t.submit();
            return
        }
        Roblox.Bootstrap.GenericConfirmation.open({
            titleText: Roblox.Resources.CaptchaModal.title,
            bodyContent: Roblox.Resources.CaptchaModal.message,
            acceptText: Roblox.Resources.CaptchaModal.finish,
            acceptColor: Roblox.Bootstrap.GenericConfirmation.green,
            allowHtmlContentInBody: !0,
            dismissable: !0,
            xToCancel: !0,
            isCaptchaValid: e == "False" ? !1 : !0,
            captchaErrorMessage: Roblox.Resources.CaptchaModal.captchaErrorMessage,
            onAccept: u,
            onCancel: r,
            onOpenCallback: f
        })
    }

    function i(t) {
        return t.find(n).length != 0
    }

    function r(n) {
        return !n.hasClass("ng-invalid")
    }
    var n = ".roblox-captcha-modal";
    return {
        Create: t,
        FormNeedsCaptcha: i,
        FormIsValid: r
    }
}(), $(function() {
    "use strict";
    $("form").submit(function(n) {
        var t = $(this);
        Roblox.Bootstrap.CaptchaModal.FormNeedsCaptcha(t) && Roblox.Bootstrap.CaptchaModal.FormIsValid(t) && (n.preventDefault(), Roblox.Bootstrap.CaptchaModal.Create(t))
    })
});