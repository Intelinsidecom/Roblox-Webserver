// ShopInterstitialModal.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.ShopInterstitialModal == "undefined" && (Roblox.ShopInterstitialModal = function() {
    function f() {
        n.isOpen = !1, t()
    }

    function e(f) {
        var h, e, o, s;
        n.isOpen = !0, h = {}, f = $.extend({}, h, f), i.overlayClose = f.dismissable, i.escClose = f.dismissable, e = $(r), e.unbind(), e.bind("click", function() {
            return t(), window.open("https://web.archive.org/web/20240128234921/http://www.shoproblox.com/", "_blank"), !1
        }), o = $(u), o.unbind(), o.bind("click", function() {
            return t(), !1
        }), s = $("a.genericmodal-close"), s.unbind(), s.bind("click", function() {
            return t(), !1
        }), $("[data-modal-handle='shop-confirmation']").modal(i)
    }

    function o() {
        if (n.isOpen) {
            var t = $(r);
            t.click()
        }
    }

    function s() {
        var n = $(u);
        n.click()
    }

    function t(t) {
        n.isOpen = !1, typeof t != "undefined" ? $.modal.close(t) : $.modal.close()
    }
    var r = "#rbx-continue-shopping-btn",
        u = "#rbx-shopping-close-btn",
        n = {
            isOpen: !1
        },
        i = {
            overlayClose: !0,
            escClose: !0,
            opacity: 80,
            overlayCss: {
                backgroundColor: "#000"
            },
            onClose: f
        };
    return {
        open: e,
        close: t,
        clickYes: o,
        clickNo: s,
        status: n
    }
}()), $(document).keypress(function(n) {
    Roblox.ShopInterstitialModal.status.isOpen && n.which === 13 && Roblox.ShopInterstitialModal.clickYes()
}), $(function() {
    $("a.roblox-shop-interstitial").on("click", function(n) {
        n.preventDefault(), Roblox.ShopInterstitialModal.open()
    })
});