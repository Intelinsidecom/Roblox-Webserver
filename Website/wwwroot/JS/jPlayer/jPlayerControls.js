// jPlayer/jPlayerControls.js
(function() {
    var t = null;
    var i = false;
    var n = null;

    $(document).on("click", ".MediaPlayerIcon", function(event) {
        var r = $(event.target);
        r.mediaUrl = r.attr("data-mediathumb-url");
        r.hasSameMediaAs = function(other) {
            return r.mediaUrl === other.mediaUrl;
        };
        r.play = function() {
            if (n === null || !n.hasSameMediaAs(r)) {
                if (n != null) n.stop();
                t.jPlayer("setMedia", { mp3: r.mediaUrl });
                n = r;
                t.on($.jPlayer.event.ended, r.onJPlayerEnded);
                t.on($.jPlayer.event.error, r.onJPlayerError);
            }
            t.jPlayer("play");
            r.removeClass("icon-play").addClass("icon-pause");
        };
        r.stop = function() {
            if (n && n.hasSameMediaAs(r)) {
                n = null;
                t.jPlayer("clearMedia");
                t.off($.jPlayer.event.ended);
                t.off($.jPlayer.event.error);
                r.removeClass("icon-pause").addClass("icon-play");
            }
        };
        r.pause = function() {
            if (n && n.hasSameMediaAs(r)) {
                t.jPlayer("pause");
                r.removeClass("icon-pause").addClass("icon-play");
            }
        };
        r.onJPlayerError = function() { r.stop(); };
        r.onJPlayerEnded = function() { r.stop(); };

        if (t == null) {
            var mediaPlayer = $("<div id='MediaPlayerSingleton'></div>").appendTo("body");
            t = mediaPlayer.jPlayer({
                swfPath: "jPlayer/2.9.2/jquery.jplayer.swf",
                solution: "html, flash",
                supplied: "mp3",
                wmode: "transparent",
                errorAlerts: false,
                warningAlerts: false,
                ready: function() {
                    r.play();
                }
            });
        } else if (r.hasClass("icon-pause")) {
            r.pause();
        } else {
            r.play();
        }
    }).on("DOMNodeRemoved", function(event) {
        if (n && $(event.target).find(n).length > 0) n.stop();
    });
})();
