// Item/ItemPage.js
var Roblox = Roblox || {};
Roblox.ItemPage = function() {
    function n() {
        var n = "content-overflow-toggle",
            t = $("." + n),
            i = "content-height",
            r = "content-overflow-page-loading";
        $(".toggle-content").removeClass("hidden"), t.each(function() {
            var t = $(this),
                u = $(this).clone().hide().height("auto");
            u.width(t.width()), u.css("font-weight", t.css("font-weight")), $("body").append(u);
            var f = t.attr("id"),
                e = $(".toggle-content[data-container-id='" + f + "']"),
                o = $(".show-more-end[data-container-id='" + f + "']");
            o.removeClass("hide"), (u.height() <= t.height() || !e.is(":visible")) && (t.removeClass(n).removeClass(i), e.hide(), o.addClass("hide")), t.removeClass(r), u.remove()
        })
    }

    function t() {
        var n = "content-overflow-toggle-off",
            t = "content-height",
            i = "Read More",
            r = "Show Less";
        $(".toggle-content").bind("click", function() {
            var f = $(this).data("container-id"),
                u = $("#" + f);
            $(this).text() === i ? (u.removeClass(t).addClass(n), $(this).text(r), u.find(".show-more-end").addClass("hide")) : (u.removeClass(n).addClass(t), $(this).text(i), u.find(".show-more-end").removeClass("hide"))
        })
    }

    function i() {
        var n = $("#item-container"),
            t = n.data("asset-granted"),
            i = n.data("forward-url");
        t === "True" && Roblox.Dialog.open({
            titleText: Roblox.Item.Resources.assetGrantedModalTitle,
            bodyContent: Roblox.Item.Resources.assetGrantedModalMessage,
            acceptText: "OK",
            acceptColor: Roblox.Dialog.green,
            onAccept: function() {
                var n = window.open(i, "_blank");
                n.focus()
            },
            declineColor: Roblox.Dialog.none,
            dismissable: !0,
            xToCancel: !0
        })
    }
    return {
        TruncateContent: n,
        ToggleContent: t,
        ShowGrantedItemPopUp: i
    }
}(), $(function() {
    function n() {
        var n = $(".sale-clock .text"),
            t, i;
        n.length && (t = $("#item-container").data("current-time"), i = $("#sale-clock").data("off-sale-deadline"), Roblox.CountdownTimer.InitializeClock(t, i, function(t) {
            var i;
            i = t.days < 1 ? "Offsale in <span>" + t.hours + "</span> h <span>" + t.minutes + "</span> m <span>" + t.seconds + "</span> s " : "Offsale in <span>" + t.days + "</span> d <span>" + t.hours + "</span> h <span>" + t.minutes + "</span> m ", n.html(i)
        }))
    }
    Roblox.ItemReskinPurchase = new Roblox.ItemPurchase(null, null, !0);
    $("#item-container").on("click", ".PurchaseButton", function() {
        Roblox.ItemReskinPurchase.openPurchaseVerificationView($(this))
    });
    $(".container-main").hasClass("in-app") || $(".description-content").linkify(), Roblox.ItemPage.TruncateContent(), Roblox.ItemPage.ToggleContent(), Roblox.ItemPage.ShowGrantedItemPopUp(), n(), Roblox && Roblox.Performance && Roblox.Performance.setPerformanceMark("itemReskin_end")
});