// Accounts/EmailEntryModal.js
typeof Roblox.EmailEntryModal == "undefined" && (Roblox.EmailEntryModal = function() {
    function t(t, i) {
        var r = $("div#EmailEntryModal").filter(":first");
        r.length == 0 && (r = $("<div id='EmailEntryModal' class='modalPopup'><div class='Message'></div></div>")), t ? r.find("div.Message").html(t) : $(i).appendTo(r.find("div.Message")), r.modal(n)
    }
    var n = {
        overlayClose: !0,
        escClose: !0,
        opacity: 80,
        overlayCss: {
            backgroundColor: "#000"
        }
    };
    return {
        open: t
    }
}());