// Interstitial.js
$(function() {
    $("a.roblox-interstitial").on("click", function(n) {
        n.preventDefault();
        var i = window.location.host,
            t = $(this).attr("href"),
            r = t.split("/")[2];
        Roblox.GenericConfirmation.open({
            titleText: "Leaving Roblox",
            bodyContent: "You are now leaving " + i + " to go to " + r + ".<br /><br />Remember, other websites have their own Terms of Service and Privacy Policy.",
            acceptText: "Continue",
            declineText: "Return",
            acceptColor: Roblox.GenericConfirmation.blue,
            declineColor: Roblox.GenericConfirmation.gray,
            onAccept: function() {
                document.location.href = t
            },
            allowHtmlContentInBody: !0,
            dismissable: !0,
            xToCancel: !0
        })
    })
});