// Bootstrap/GenericConfirmation.js
typeof Roblox == "undefined" && (Roblox = {}), Roblox.Bootstrap = Roblox.Bootstrap || {}, typeof Roblox.Bootstrap.GenericConfirmation == "undefined" && (Roblox.Bootstrap.GenericConfirmation = function() {
    function ut(f) {
        var k = {
                titleText: "",
                bodyContent: "",
                footerText: "",
                acceptText: Roblox.Resources.GenericConfirmation.yes,
                declineText: Roblox.Resources.GenericConfirmation.No,
                acceptColor: l,
                declineColor: c,
                xToCancel: !1,
                isCaptchaValid: !0,
                captchaEmptyMessage: Roblox.Resources.CaptchaModal.captchaEmptyMessage,
                captchaErrorMessage: "",
                onAccept: function() {
                    return !1
                },
                onDecline: function() {
                    return !1
                },
                onCancel: function() {
                    return !1
                },
                imageUrl: null,
                allowHtmlContentInBody: !1,
                allowHtmlContentInFooter: !1,
                dismissable: !0,
                fieldValidationRequired: !1,
                onOpenCallback: function() {}
            },
            y, p, v, w, b;
        f = $.extend({}, k, f), u.backdrop = f.dismissable ? !0 : "static", u.keyboard = f.xToCancel, y = $(r), y.text(f.acceptText), y.unbind(), y.bind("click", function() {
            return o(y) ? !1 : $(d).val() == "" ? (v.find(h).text(f.captchaEmptyMessage), !1) : (f.fieldValidationRequired ? ft(f.onAccept) : n(f.onAccept), !1)
        }), p = $(t), p.html(f.declineText), p.unbind(), p.bind("click", function() {
            return o(p) ? !1 : (n(f.onDecline), !1)
        }), v = $(a), v.find(g).text(f.titleText), f.imageUrl == null || f.imageUrl == "" ? v.find(e).hide() : (v.find(e).attr("src", f.imageUrl), v.show()), f.allowHtmlContentInBody ? v.find(s).html(f.bodyContent) : v.find(s).text(f.bodyContent), f.isCaptchaValid || v.find(h).text(f.captchaErrorMessage), $.trim(f.footerText) == "" ? v.find(i).hide() : v.find(i).show(), f.allowHtmlContentInFooter ? v.find(i).html(f.footerText) : v.find(i).text(f.footerText), typeof v.modal == "function" ? v.modal(u) : v.bootstrapModal(u), w = $("#roblox-close-btn"), w.unbind(), w.bind("click", function() {
            return n(f.onCancel), !1
        }), f.xToCancel || w.hide(), b = $("#BootstrapConfirmationModal"), b.off("click"), b.click(function(t) {
            if ($(t.target).closest(".modal-content").length == 0) return n(f.onCancel()), !1
        }), $(document).keyup(function(t) {
            if (t.keyCode == 27) return n(f.onCancel), !1
        }), typeof v.modal == "function" ? v.modal("show") : v.bootstrapModal("show"), f.onOpenCallback()
    }

    function v(n) {
        n.attr("disabled", !0)
    }

    function o(n) {
        return n.attr("disabled") == !0
    }

    function y() {
        var n = $(r),
            i = $(t);
        v(n), v(i)
    }

    function p() {
        var u = $(r),
            i = $(t),
            n = it + " " + tt + " " + rt;
        u.removeClass(n), i.removeClass(n)
    }

    function w() {
        var n = $(r);
        n.click()
    }

    function b() {
        var n = $(t);
        n.click()
    }

    function f() {
        var n = $(a);
        typeof n.modal == "function" ? n.modal("hide") : n.bootstrapModal("hide")
    }

    function n(n) {
        f(), typeof n == "function" && n()
    }

    function ft(n) {
        if (typeof n == "function") {
            var t = n();
            if (t !== "undefined" && t == !1) return !1
        }
        f()
    }
    var k = "btn-primary",
        l = "btn-neutral",
        c = "btn-negative",
        rt = "btn-disabled-primary",
        it = "btn-disabled-neutral",
        tt = "btn-disabled-negative",
        nt = "btn-none",
        r = "#roblox-confirm-btn",
        t = "#roblox-decline-btn",
        a = "[data-modal-handle='bootstrap-confirmation']",
        g = ".modal-title",
        s = ".modal-body-text",
        d = "#recaptcha_response_field",
        h = "#roblox-captcha-error",
        i = "div.ConfirmationModalFooter",
        e = "img.GenericModalImage",
        u = {
            backdrop: !0,
            keyboard: !0
        };
    return {
        open: ut,
        close: f,
        disableButtons: y,
        enableButtons: p,
        clickYes: w,
        clickNo: b,
        green: k,
        blue: l,
        gray: c,
        none: nt
    }
}());