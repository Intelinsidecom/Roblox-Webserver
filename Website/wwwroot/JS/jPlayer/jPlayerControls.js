// jPlayer/jPlayerControls.js
$(function() {
    var t = null,
        i = !1,
        n = null,
        r = function() {
            typeof jQuery == "undefined" || i || (i = !0, $(function() {
                $(document).on("click", ".MediaPlayerIcon", function(i) {
                    var r = $(i.target);
                    r.mediaUrl = r.attr("data-mediathumb-url"), r.hasSameMediaAs = function(n) {
                        return r.mediaUrl === n.mediaUrl
                    }, r.play = function() {
                        if (n === null || !n.hasSameMediaAs(r)) {
                            n != null && n.stop(), t.jPlayer("setMedia", {
                                mp3: r.mediaUrl
                            }), n = r;
                            t.on($.jPlayer.event.ended, r.onJPlayerEnded);
                            t.on($.jPlayer.event.error, r.onJPlayerError)
                        }
                        t.jPlayer("play"), r.removeClass("icon-play").addClass("icon-pause")
                    }, r.stop = function() {
                        n.hasSameMediaAs(r) && (n = null, t.jPlayer("clearMedia"), t.off($.jPlayer.event.ended), t.off($.jPlayer.event.error), r.removeClass("icon-pause").addClass("icon-play"))
                    }, r.pause = function() {
                        n.hasSameMediaAs(r) && (t.jPlayer("pause"), r.removeClass("icon-pause").addClass("icon-play"))
                    }, r.onJPlayerError = function() {
                        r.stop()
                    }, r.onJPlayerEnded = function() {
                        r.stop()
                    }, t == null ? t = $("<div id='MediaPlayerSingleton'></div>").appendTo("body").jPlayer({
                        swfPath: "jPlayer/2.9.2/jquery.jplayer.swf",
                        solution: "html, flash",
                        supplied: "mp3",
                        wmode: "transparent",
                        errorAlerts: !1,
                        warningAlerts: !1,
                        ready: function() {
                            r.play()
                        }
                    }) : r.hasClass("icon-pause") ? r.pause() : r.play()
                }).on("DOMNodeRemoved", function(t) {
                    n && $(t.target).find(n).length > 0 && n.stop()
                })
            }))
        };
    $(function() {
        r()
    })
});