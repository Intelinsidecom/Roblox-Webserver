// InstallationInstructions.js
typeof Roblox == "undefined" && (Roblox = {}), Roblox.InstallationInstructions = function() {
    function n(n) {
        var t, r, u;
        typeof n == "undefined" && (n = "installation"), i(n), t = 0, r = $(".InstallInstructionsImage"), r && typeof $(r).data("modalwidth") != "undefined" && (t = $(".InstallInstructionsImage").data("modalwidth")), t > 0 ? (u = ($(window).width() - (t - 10)) / 2, $("#InstallationInstructions").modal({
            escClose: !0,
            opacity: 80,
            minWidth: t,
            maxWidth: t,
            overlayCss: {
                backgroundColor: "#000"
            },
            position: [50, u],
            zIndex: 1031
        })) : $("#InstallationInstructions").modal({
            escClose: !0,
            opacity: 80,
            maxWidth: $(window).width() / 2,
            minWidth: $(window).width() / 2,
            overlayCss: {
                backgroundColor: "#000"
            },
            position: [50, "25%"],
            zIndex: 1031
        })
    }

    function t() {
        $.modal.close()
    }

    function i(n) {
        var t = $(".InstallInstructionsImage");
        navigator.userAgent.match(/Mac OS X 10[_|\.]5/) ? t && typeof t.data("oldmacdelaysrc") != "undefined" && t.attr("src", t.data("oldmacdelaysrc")) : n == "activation" && t.data("activationsrc") !== undefined ? t.attr("src", t.data("activationsrc")) : t.data("delaysrc") !== undefined && t.attr("src", t.data("delaysrc"))
    }
    return {
        show: n,
        hide: t
    }
}();