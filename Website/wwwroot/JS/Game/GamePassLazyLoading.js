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
        $("rbx-passes-container").find("#rbx-game-passes").remove(), $("rbx-passes-container").find("#store-does-not-sell").remove(), $(".tab-pane.store").append('<p id="store-does-not-sell" class="section-content-off">' + Roblox.GamePassJSData.LabelGameDoesNotSell + "</p>")
    }

    function f() {
        var f = t.clone().css("display", "inherit"),
            n;
        $("#rbx-passes-container").append(f), n = "/Games/GetGamePassesInnerPartial?startIndex=0&maxRows=" + Roblox.GamePassJSData.GamePassesPerPlaceLimit + "&placeId=" + Roblox.GamePassJSData.PlaceID, $.ajax({
            type: "GET",
            url: n,
            contentType: "application/json; charset=utf-8",
            cache: !1,
            success: function(n) {
                $(".rbx-passes-item-container#spinner").remove(), i(), r(n), Roblox.GamePassJSData.TotalGearItems == 0 && n.length < 10 && u()
            },
            error: function() {
                Roblox.Dialog.open({
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
        t = $(".rbx-gear-passes-item-add#spinner");
    return {
        init: f
    }
}();