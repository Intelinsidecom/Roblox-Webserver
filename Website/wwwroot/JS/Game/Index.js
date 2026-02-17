// Game/Index.js
$(function() {
    function t() {
        if (Roblox && Roblox.RealTime) {
            var n = $("#game-detail-page").data("place-id"),
                t = $("#game-detail-page").data("post-game-loop-variation"),
                r = Roblox.RealTime.Factory.GetClient();
            r.Subscribe("ClientPresenceNotifications", function(r) {
                if (r.Type === "Disconnect" && r.PlaceId === n) switch (t) {
                    case 2:
                        i()
                }
            })
        }
    }

    function i() {
        window.location.href = "/games"
    }
    var n = $("#badges-see-more");
    n.click(function() {
        n.text() == "See More" ? n.text("See Less") : n.text("See More"), $(".badge-see-more-row").toggle()
    }), $(".game-play-buttons").data("autoplay") == !0 && $("#MultiplayerVisitButton").trigger("click");
    $(".VisitButtonPlay a").on("click", function() {
        var t = $(this),
            n = t.attr("backup-url");
        n && n.length > 0 && setTimeout(function() {
            document.hasFocus && document.hasFocus() && (window.location = n)
        }, 250)
    });
    t()
});