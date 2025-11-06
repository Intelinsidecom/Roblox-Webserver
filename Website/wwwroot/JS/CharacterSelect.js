// CharacterSelect.js
typeof Roblox == "undefined" && (Roblox = {}), Roblox.CharacterSelect = function() {
    function r() {
        return n.genderID = t.male, i(1, "Male", t.male), !1
    }

    function u() {
        return n.genderID = t.female, i(0, "Female", t.female), !1
    }

    function i(t, i, r) {
        $.modal.close(".GuestModePromptModal"), n.robloxLaunchFunction(r)
    }

    function f() {
        $("#GuestModePrompt_BoyGirl").modal({
            overlayClose: !0,
            escClose: !0,
            opacity: n.modalOpacity,
            overlayCss: {
                backgroundColor: "#000"
            },
            onShow: o,
            onClose: e
        })
    }

    function e() {
        $(this).trigger(n.closeEvent), $.modal.close()
    }

    function o(n) {
        if (n.container.innerHeight() == 15) {
            var t = -Math.floor($("#GuestModePrompt_BoyGirl").innerHeight() / 2);
            $("#GuestModePrompt_BoyGirl").css({
                position: "relative",
                top: t + "px"
            })
        }
    }

    function s() {
        $.modal.close(".GuestModePromptModal")
    }
    var t = {
            male: 2,
            female: 3
        },
        n;
    return $(function() {
        $(".VisitButtonGirlGuest").click(u), $(".VisitButtonBoyGuest").click(r)
    }), n = {
        robloxLaunchFunction: function(n) {
            $(document).trigger("CharacterSelectLaunch", [n])
        },
        genderID: null,
        show: f,
        hide: s,
        placeid: undefined,
        closeEvent: "close",
        modalOpacity: 80
    }
}();