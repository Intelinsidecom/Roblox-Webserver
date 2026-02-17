// PlaceProductPromotion.js
var Roblox = Roblox || {};
Roblox.PlaceProductPromotion = function() {
    function i(n) {
        return n.__RequestVerificationToken = $("input[name=__RequestVerificationToken]").val(), n
    }

    function r(n) {
        var t = $(".system-feedback .alert-success"),
            i = "Added to your game, <span class='font-bold'>" + n + "</span>.";
        t.length > 0 && Roblox.BootstrapWidgets && Roblox.BootstrapWidgets.ToggleSystemMessage(t, 100, 2e3, i)
    }

    function t(n) {
        var t = $(".system-feedback .alert-warning");
        t.length > 0 && Roblox.BootstrapWidgets && Roblox.BootstrapWidgets.ToggleSystemMessage(t, 100, 2e3, n)
    }

    function u() {
        function i(n) {
            return n.__RequestVerificationToken = $("input[name=__RequestVerificationToken]").val(), n
        }
        var n = $("#promote-gear .product-promo-place").val();
        $.ajax({
            type: "POST",
            url: "/Games/AddProductPromotionToPlace?placeId=" + n + "&productId=" + Roblox.PlaceProductPromotionData.ProductID,
            data: i({}),
            dataType: "json",
            success: function(n) {
                n.ErrorMsg ? t(n.ErrorMsg) : r(n.PlaceName)
            },
            error: function() {
                t(response.ErrorMsg)
            }
        })
    }

    function f(n) {
        var t = $("#DeleteProductPromotionModal");
        $.ajax({
            type: "POST",
            url: "/Games/DeletePlaceProductPromotion?promotionId=" + n,
            data: i({}),
            success: function() {
                var r = $('.icon-alert[data-delete-promotion-id="' + n + '"]'),
                    u = r.attr("data-delete-promotion-name");
                t.find(".titleBar").text(Roblox.PlaceProductPromotion.Resources.success), t.find(".PurchaseModalMessageText").html(Roblox.PlaceProductPromotion.Resources.youhaveRemoved + "<a>" + u + "</a>" + Roblox.PlaceProductPromotion.Resources.fromYourGame), t.modal(Roblox.PlaceProductPromotion.modalProperties), r.parents(".list-item").hide()
            },
            error: function() {
                t.find(".titleBar").text(Roblox.PlaceProductPromotion.Resources.error), t.find(".PurchaseModalMessageText").text(Roblox.PlaceProductPromotion.Resources.sorryWeCouldnt), t.modal(Roblox.PlaceProductPromotion.modalProperties)
            }
        })
    }

    function e() {
        $("body").on("change", "#promote-gear .product-promo-group", function() {
            var n = this.value;
            $("#promote-gear .product-promo-place").load("/Games/ProductPromotionPlaceDropDown?groupId=" + n)
        });
        Roblox.require("Widgets.ItemImage", function(t) {
            t.populate(n.find(".roblox-item-image"))
        });
        $("#promote-gear-btn").on("click", function() {
            Roblox.Dialog.open({
                titleText: "Promote Item",
                bodyContent: $("#promote-gear-template").clone().attr("id", "promote-gear").removeClass("hidden"),
                onAccept: function() {
                    u()
                },
                imageUrl: n.data("asset-image-url"),
                acceptText: "Add",
                declineText: "Cancel",
                xToCancel: !0,
                dismissable: !0,
                fieldValidationRequired: !0,
                allowHtmlContentInBody: !0
            })
        })
    }
    var n = $("#promote-gear-container");
    return {
        SetUpAddPlaceProductPromotion: e,
        DeleteGear: f
    }
}();