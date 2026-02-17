// GamePass.js
var Roblox = Roblox || {};
Roblox.GamePass = function() {
    function n(n) {
        var t, i, r;
        if (n.length == 0) $(".GamePassesDisplayContainer").hasClass("OwnerIsViewing") ? ($("#GamePassesItemContainer").text(Roblox.GamePass.Resources.noneforSale), $("#GamePassesPagerContainer").hide()) : $(".GamePassesDisplayContainer").hide();
        else return t = $(".GamePasses.PlacePagePagersItem.template").clone().removeClass("template"), t.find(".ItemURL").attr("href", n.PassItemURL).text(fitStringToWidthSafeText(n.PassName, 100)), t.find(".TotalSales").text(n.TotalSales), t.find("[data-item-purchase-enabled=False]").css("display", "block"), i = t.find(".PurchaseButton"), n.PriceInRobux > 0 && (i.attr("data-expected-currency", 1).attr("data-expected-price", n.PriceInRobux), t.find(".Price.robux").text(n.PriceInRobux).show()), n.PriceInRobux == 0 && (i.attr("data-expected-currency", 1).attr("data-expected-price", 0), t.find(".Price.robux-text").text("FREE").show()), n.PriceInRobux > 0 && t.find("input").attr("name", "currency_" + n.PassID).show(), i.attr("data-expected-price", r).attr("data-item-name", n.PassName), n.UserOwns === !0 && (t.find(".PurchaseButton").hide(), t.find("input").attr("name", "currency_" + n.PassID).hide(), t.find(".UserOwns").show()), t.find("[data-item-id]").attr("data-item-id", n.PassID), t.find("[data-product-id]").attr("data-product-id", n.ProductID), t
    }

    function t(n, t) {
        placeId = Roblox.GamePassJSData.PlaceID;
        var i = Math.floor(Math.random() * 9001);
        $.ajax({
            type: "GET",
            url: "/Games/GetGamePassesPaged?placeId=" + placeId + "&startIndex=" + n + "&maxRows=" + t + "&cachebuster=" + i,
            contentType: "application/json; charset=utf-8",
            success: function(n) {
                Roblox.GamePassesPager.update(n)
            },
            error: function() {
                $("#GamePassesItemContainer").addClass("empty").text(anErrorOccurred)
            }
        })
    }

    function i(n) {
        var i = $(".GamePasses.PlacePagePagersItem .roblox-item-image[data-item-id=" + n.PassID + "]"),
            t;
        Roblox.require("Widgets.ItemImage", function(n) {
            n.load(i)
        }), t = $(".GamePasses.PlacePagePagersItem .PurchaseButton[data-item-id=" + n.PassID + "]"), t.click(function() {
            return Roblox.GamePassItemPurchase.openPurchaseVerificationView(t[0]), !1
        }), $(".GamePasses.PlacePagePagersItem input:radio").change(function(n) {
            var t = $(n.target),
                i = t.parents(".InfoContainer");
            t.hasClass("robux") && i.find(".PurchaseButton").attr("data-expected-currency", 1).attr("data-expected-price", i.find(".Price.robux").text())
        })
    }
    return {
        getGamePasses: t,
        formatGamePassHTML: n,
        FormatGamePassCallback: i
    }
}(), Roblox.GamePass.Resources = {
    noneforSale: "None of your game passes are currently for sale.",
    anErrorOccurred: "An error occurred, please try again."
};