// widgets/InlineSocial.js
var Roblox = Roblox || {};
Roblox.InlineSocial = function() {
    function n(n) {
        Roblox.Hybrid.Social.presentShareDialog(n.text, n.link, n.imageUrl)
    }

    function t(t) {
        var r, u, i, f;
        if (Roblox.Hybrid && Roblox.Hybrid.Social) {
            Roblox.Hybrid.Social.supports("presentShareDialog", function(i) {
                if (i && Roblox.Hybrid.Social.presentShareDialog) $("#rbx-share-btn").css("display", "block").on("click", function() {
                    n(t)
                })
            });
            return
        }
        for (r = new gigya.socialize.UserAction, r.setLinkBack(t.link), r.setTitle("ROBLOX: " + t.text), u = new gigya.socialize.UserAction, u.setLinkBack(t.link), u.setTitle(t.text + " via @ROBLOX"), i = 0; i < socialShareButtons.length; i++) socialShareButtons[i].provider === "Twitter" && (socialShareButtons[i].userAction = u);
        f = {
            userAction: r,
            shareButtons: socialShareButtons,
            containerID: "gigya-target",
            layout: "horizontal",
            deviceType: "auto",
            iconsOnly: "true",
            buttonWithCountTemplate: "<div class='social-button-template'><img src='$iconImg' class='social-button-icon-img' onclick='$onClick'><div class='social-button-counter'>$count</div></div>",
            buttonTemplate: "<div class='social-button-template'><img src='$iconImg' class='social-button-icon-img' onclick='$onClick'><div class='social-button-counter'>-</div></div>",
            showEmailButton: !1,
            countURL: t.countUrl
        }, gigya.socialize.showShareBarUI(f)
    }
    return {
        loadIcons: t
    }
}();