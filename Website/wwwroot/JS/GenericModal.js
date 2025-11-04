// GenericModal.js
typeof Roblox.GenericModal == "undefined" && (Roblox.GenericModal = function() {
    function s(u, f, e, s, h, c) {
        var l, a;
        t.isOpen = !0, n = $.extend({}, n, c), i = s, l = $("div.GenericModal").filter(":first"), l.find("div.Title").text(u), f === null ? l.addClass("noImage") : (l.find("img.GenericModalImage").attr("src", f), l.removeClass("noImage")), l.find("div.Message").html(e), h && (l.removeClass("smallModal"), l.addClass("largeModal")), a = l.find(o), a.attr("class", "btn-large " + n.acceptColor), a.unbind(), a.bind("click", function() {
            r()
        }), l.modal(n)
    }

    function r() {
        t.isOpen = !1, $.modal.close(), typeof i == "function" && i()
    }
    var f = "btn-primary",
        u = "btn-neutral",
        e = "btn-negative",
        o = ".ImageButton.btn-neutral.btn-large.roblox-ok",
        t = {
            isOpen: !1
        },
        n = {
            overlayClose: !0,
            escClose: !0,
            opacity: 80,
            overlayCss: {
                backgroundColor: "#000"
            },
            acceptColor: u
        },
        i;
    return $(function() {
        $(document).on("click", ".GenericModal .roblox-ok", function() {
            r()
        })
    }), {
        close: r,
        open: s,
        status: t,
        green: f,
        blue: u,
        gray: e
    }
}()), Roblox.GenericModal.Resources = {
    ErrorText: "Error",
    ErrorMessage: "Sorry, an error occurred."
}, $(document).keypress(function(n) {
    n.which === 13 && Roblox.GenericModal.status.isOpen && Roblox.GenericModal.close()
});