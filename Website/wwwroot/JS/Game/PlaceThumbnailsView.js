// Game/PlaceThumbnailsView.js
function onYouTubeIframeAPIReady() {
    var n = "#carousel-game-details";
    $(n).find(".flex-video").each(function(n, t) {
        youTubeId = $(t).find("iframe").attr("id"), rbxplayer[rbxplayer.length] = new YT.Player(youTubeId, {})
    });
    $(window).on("message", function(n) {
        var i = n.originalEvent.data,
            t;
        i.charAt(0) == "{" && (t = $.parseJSON(i), t.event == "onReady" && Roblox.Carousel.onPlayerReady(), t.event == "infoDelivery" && t.info.playerState && t.info.playerState == 1 && Roblox.Carousel.onPlayerPlaying())
    })
}
var Roblox = Roblox || {},
    rbxplayer;
Roblox.Carousel = function() {
    var n = "#carousel-game-details",
        i = !1,
        t = !1,
        r = function() {
            function i() {
                var n = $("#playHiddenYouTubeVideo").data("link");
                window.open(n, "_blank")
            }
            t = $(n).data("is-mobile"), t ? $(n).carousel({
                interval: !1,
                pause: "hover"
            }) : $(n).carousel({
                interval: 6e3,
                pause: "hover"
            }), $(n).on("slide.bs.carousel", function() {
                Roblox.Carousel.pauseAllVideos(), $(n).carousel("cycle")
            }).hover(function() {
                $(this).addClass("hover")
            }, function() {
                $(this).removeClass("hover")
            }), $(n + " .item").length < 2 && $(n).find(".carousel-control").css("display", "none");
            $(document).on("playButton:gamePlayIntent", function() {
                Roblox.Carousel.pauseAllVideos()
            });
            Roblox.Carousel.setUpYouTubeAPI(), $(function() {
                $(n + " .item span").loadRobloxThumbnails()
            }), $("#playHiddenYouTubeVideo").click(function() {
                Roblox.Dialog.open({
                    titleText: "You are leaving ROBLOX",
                    bodyContent: "<p>You are about to leave Roblox to view a video on Youtube.</p><p>Youtube is not part of ROBLOX.com and is governed by a separate privacy policy.</p>",
                    allowHtmlContentInBody: !0,
                    acceptText: "Continue to Video",
                    declineText: "Cancel",
                    xToCancel: !0,
                    acceptColor: Roblox.Dialog.green,
                    declineColor: Roblox.Dialog.white,
                    onAccept: i
                })
            })
        },
        u = function() {
            var t = document.createElement("script"),
                n;
            t.src = "https://www.youtube.com/iframe_api", n = document.getElementsByTagName("script")[0], n.parentNode.insertBefore(t, n)
        },
        f = function(n) {
            var t = $(".flex-video"),
                i, r;
            t.length > 0 && (i = t.find("iframe")[0].contentWindow, r = n == "hide" ? "pauseVideo" : "playVideo", i.postMessage('{"event":"command","func":"' + r + '","args":""}', "*"))
        },
        e = function(n) {
            if (rbxplayer && rbxplayer.length > 0 && !t) try {
                rbxplayer[n].pauseVideo()
            } catch (i) {} else return !1
        },
        o = function(n) {
            return rbxplayer && rbxplayer.length > 0 && rbxplayer[n] && !t ? (rbxplayer[n].playVideo(), !0) : !1
        },
        s = function() {
            var t, n;
            if (rbxplayer && rbxplayer.length > 0)
                for (t = rbxplayer.length, n = 0; n < t; n++) Roblox.Carousel.pauseVideoAtIndex(n)
        },
        h = function() {
            if (i) return !1;
            var t = $(n);
            t.find(".item").each(function(n, r) {
                if ($(r).find(".flex-video").length > 0) {
                    t.carousel(n), t.carousel("pause");
                    var u = Roblox.Carousel.playVideoAtIndex(0);
                    return i = u, !1
                }
                return !0
            })
        },
        c = function() {
            var i = $(n).data("is-video-autoplayed-on-ready");
            i && !t && Roblox.Carousel.checkForVideo()
        },
        l = function() {
            var t = $(n);
            t.carousel("pause")
        };
    return {
        initialize: r,
        toggleVideo: f,
        checkForVideo: h,
        setUpYouTubeAPI: u,
        onPlayerReady: c,
        onPlayerPlaying: l,
        pauseVideoAtIndex: e,
        playVideoAtIndex: o,
        pauseAllVideos: s
    }
}(), rbxplayer = [], $(document).ready(function() {
    Roblox.Carousel.initialize()
});