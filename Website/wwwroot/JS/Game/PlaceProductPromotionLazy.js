// Game/PlaceProductPromotionLazy.js
var Roblox = Roblox || {};
Roblox.PlaceProductPromotionLazy = Roblox.PlaceProductPromotionLazy || function() {
    function i() {
        n.find(".list-item").not(".rbx-gear-passes-item-add").remove()
    }

    function r(t) {
        n.prepend(t)
    }

    function u() {
        var u = t.clone().css("display", "inherit"),
            n;
        $("#rbx-gear-container").append(u), n = "/Games/GetPromotedProductsInnerPartial?placeId=" + Roblox.PromotedProductJSData.PlaceID, $.ajax({
            type: "GET",
            url: n,
            contentType: "application/json; charset=utf-8",
            cache: !1,
            success: function(n) {
                $(".rbx-Gear-item-container#spinner").remove(), i(), r(n)
            },
            error: function() {
                Roblox.Dialog.open({
                    titleText: Roblox.PlaceProductPromotion.Resources.anErrorOccurred,
                    bodyContent: Roblox.PlaceProductPromotion.Resources.error,
                    acceptText: Roblox.PlaceProductPromotion.Resources.success,
                    acceptColor: Roblox.Dialog.none,
                    dismissable: !0
                })
            }
        })
    }
    var n = $("#rbx-gear-container"),
        t = $(".rbx-gear-passes-item-add#spinner");
    return {
        init: u
    }
}();