// YouTube/RobloxYouTube.js
function RobloxYouTubeVideo(n, t) {
    var i = this;
    this.RobloxVideoPlayerID = n, this.OnPlayerStateChangeCallback = function(r) {
        t(r, n, i)
    }, this.YouTubeVideoID = null, this.Chromeless = !1, this.Autoplay = !1, this.Muted = !1, this.Player = function() {
        return document.getElementById(this.RobloxVideoPlayerID)
    }, this.Init = function(n, t, r, u, f, e) {
        function s(n) {
            if (!o) {
                i.Muted && n.target.mute();
                var t = navigator.userAgent.match(/iPad/i) != null;
                i.Autoplay && !t && n.target.playVideo()
            }
        }
        this.YouTubeVideoID = n, this.Chromeless = t, this.Autoplay = e, this.Muted = e;
        var o = u <= 120;
        RobloxYouTubeVideoManager.CallWhenReady(function() {
            var t = new YT.Player(r, {
                width: u,
                height: f,
                playerVars: {
                    showinfo: 0,
                    showsearch: 0,
                    rel: 0,
                    fs: 0,
                    version: 3,
                    autohide: 1,
                    enablejsapi: 1,
                    iv_load_policy: 3,
                    playerapiid: n,
                    controls: 1,
                    wmode: "opaque"
                },
                videoId: n,
                events: {
                    onReady: s
                }
            });
            t.A && (t.A.id = i.RobloxVideoPlayerID)
        })
    }, this.SeekToTime = function(n) {
        this.Player().seekTo(n, !0)
    }, this.PauseVideo = function() {
        this.Player().pauseVideo()
    }
}

function onYouTubeIframeAPIReady() {
    RobloxYouTubeVideoManager.OnYouTubeApiReady()
}
var RobloxYouTube = RobloxYouTube || {
        Events: {
            States: {
                Unstarted: -1,
                Ended: 0,
                Playing: 1,
                Paused: 2,
                Buffering: 3,
                VideoCued: 5
            },
            Errors: {
                InvalidParameters: 2,
                VideoNotFound: 100,
                NotEmbeddable: 101,
                NotEmbeddable2: 150
            }
        }
    },
    RobloxYouTubeVideoManager = function() {
        function e(i) {
            t ? i() : n.push(i)
        }

        function f() {
            t = !0;
            for (var i = 0; i < n.length; i++) n[i]()
        }

        function o(n) {
            return r[n.RobloxVideoPlayerID] = n, n
        }

        function s(n) {
            return r[n]
        }
        var r = [],
            n = [],
            t = !1,
            u = document.createElement("script"),
            i;
        return u.src = "https://www.youtube.com/iframe_api", i = document.getElementsByTagName("script")[0], i.parentNode.insertBefore(u, i), $(function() {
            t || typeof YT == "undefined" || f()
        }), {
            AddVideo: o,
            GetVideo: s,
            OnYouTubeApiReady: f,
            CallWhenReady: e
        }
    }();