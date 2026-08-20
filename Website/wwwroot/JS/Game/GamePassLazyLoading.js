// Game/GamePassLazyLoading.js
var Roblox = Roblox || {};
Roblox.GamePassLazyLoading = function() {
    function i() {
        n.find(".list-item.real-game-pass").remove()
    }

    function r(t) {
        n.prepend(t)
    }

    function u() {
        if (Roblox.GamePassJSData.IsCreator) return;
        n.find("#store-does-not-sell").remove(), $(".tab-pane.store").append('<p id="store-does-not-sell" class="section-content-off">' + Roblox.GamePassJSData.LabelGameDoesNotSell + "</p>")
    }

    function f() {
        var f = t.clone().css("display", "inherit");
        n.append(f), $.ajax({
            type: "GET",
            url: "/Games/GetGamePassesInnerPartial?startIndex=0&maxRows=" + Roblox.GamePassJSData.GamePassesPerPlaceLimit + "&placeId=" + Roblox.GamePassJSData.PlaceID,
            contentType: "application/json; charset=utf-8",
            cache: !1,
            success: function(e) {
                n.find(".rbx-gear-passes-item-add").remove(), i(), r(e), Roblox.GamePassJSData.TotalGearItems == 0 && e.length < 10 && u()
            },
            error: function() {
                n.find(".rbx-gear-passes-item-add").remove(), Roblox.Dialog.open({
                    titleText: "Error",
                    bodyContent: "Failed to load Game Passes.  Please try again later.",
                    acceptText: "Ok",
                    acceptColor: Roblox.Dialog.none,
                    dismissable: !0
                })
            }
        })
    }
    var n = $("#rbx-passes-container"),
        t = $(".rbx-gear-passes-item-add").first();
    return {
        init: f
    }
}();
