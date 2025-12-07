// angular/angular.audio.js
"use strict";
angular.module("ngAudio", []).constant("ngAudioDomUid", function() {
    for (var n = "", t = 0; t < 8; t++) n = n + Math.floor((1 + Math.random()) * 65536).toString(16).substring(1);
    return n
}()).directive("ngAudio", ["$compile", "$q", "ngAudio", function(n, t, i) {
    return {
        restrict: "AEC",
        scope: {
            volume: "=",
            start: "=",
            currentTime: "=",
            loop: "=",
            clickPlay: "=",
            disablePreload: "="
        },
        controller: ["$scope", "$attrs", "$element", "$timeout", function(n, t, r, u) {
            function e() {
                f = i.load(t.ngAudio, n), n.$audio = f, f.unbind()
            }
            var f;
            n.disablePreload || e();
            r.on("click", function() {
                n.clickPlay !== !1 && (n.disablePreload && e(), f.audio.play(), f.volume = n.volume || f.volume, f.loop = n.loop, f.currentTime = n.start || 0, u(function() {
                    f.play()
                }, 5))
            });
            r.on("$destroy", function() {
                f.destroy()
            })
        }]
    }
}]).directive("ngAudioHover", ["$compile", "$q", "ngAudio", function(n, t, i) {
    return {
        restrict: "AEC",
        controller: ["$scope", "$attrs", "$element", "$timeout", function(n, t, r) {
            var f = i.load(t.ngAudioHover, n);
            r.on("mouseover rollover hover", function() {
                f.audio.play(), f.volume = t.volumeHover || f.volume, f.loop = t.loop, f.currentTime = t.startHover || 0
            });
            r.on("$destroy", function() {
                f.destroy()
            })
        }]
    }
}]).service("localAudioFindingService", ["$q", function(n) {
    this.find = function(t) {
        var i = n.defer(),
            r = document.getElementById(t);
        return r ? i.resolve(r) : i.reject(t), i.promise
    }
}]).service("remoteAudioFindingService", ["$q", "ngAudioDomUid", function(n, t) {
    this.find = function(i) {
        var f = n.defer(),
            r = document.getElementById(t),
            u;
        return r ? (r.pause(), r.src = i, r.load()) : (u = document.createElement("audio"), u.style.display = "none", u.id = t, u.src = i, document.body.appendChild(u), r = document.getElementById(t), r.load()), r ? f.resolve(r) : f.reject(id), f.promise
    }
}]).service("cleverAudioFindingService", ["$q", "localAudioFindingService", "remoteAudioFindingService", function(n, t, i) {
    this.find = function(r) {
        var u = n.defer();
        return r = r.replace("|", "/"), t.find(r).then(u.resolve, function() {
            return i.find(r)
        }).then(u.resolve, u.reject), u.promise
    }
}]).value("ngAudioGlobals", {
    muting: !1,
    performance: 25,
    unlock: !0,
    volume: 1
}).factory("NgAudioObject", ["cleverAudioFindingService", "$rootScope", "$interval", "$timeout", "ngAudioGlobals", function(n, t, i, r, u) {
    return function(r, f) {
        function tt() {
            try {
                o.play(), o.pause()
            } catch (n) {}
            window.removeEventListener("click", tt)
        }

        function rt() {
            l || (h && i.cancel(h), nt && nt(), s && s(), l = !0)
        }

        function ot() {
            l || (s = p.$watch(function() {
                return {
                    volume: e.volume,
                    currentTime: e.currentTime,
                    progress: e.progress,
                    muting: e.muting,
                    loop: e.loop,
                    playbackRate: e.playbackRate,
                    globalVolume: u.volume
                }
            }, function(n, t) {
                n.currentTime !== t.currentTime && e.setCurrentTime(n.currentTime), n.progress !== t.progress && e.setProgress(n.progress), n.volume !== t.volume && e.setVolume(n.volume), n.playbackRate !== t.playbackRate && e.setPlaybackRate(n.playbackRate), n.globalVolume !== t.globalVolume && (n.globalVolume === 0 ? e.setMuting(!0) : (e.setMuting(!1), e.setVolume(n.globalVolume))), v = n.loop, n.muting !== t.muting && e.setMuting(n.muting)
            }, !0))
        }

        function ut() {
            e.error = !0
        }

        function et() {
            if (s && s(), o) {
                if (o.volume = w || u.muting ? 0 : e.volume !== undefined ? e.volume : 1, g && (o.play(), g = !1), k && (o.pause(), o.currentTime = 0, k = !1), d && (o.pause(), d = !1), b && (o.playbackRate = ft, b = !1), a && (o.volume = a, a = undefined), it) {
                    e.currentTime = o.currentTime, e.duration = o.duration, e.remaining = o.duration - o.currentTime, e.progress = 0, e.paused = o.paused, e.src = o.src;
                    var n = o.currentTime / o.duration;
                    n > 0 && (e.progress = n), e.currentTime >= e.duration && y.forEach(function(n) {
                        n(e)
                    }), c.forEach(function(n) {
                        e.duration - e.currentTime <= n.secs && (n.callback(e), c.shift())
                    }), v && e.currentTime >= e.duration && (v !== !0 && (v--, e.loop--), e.setCurrentTime(0), e.play())
                }
                w || u.muting || (e.volume = o.volume), e.audio = o
            }
            ot()
        }
        var s, nt, g = !1,
            d = !1,
            k = !1,
            b = !1,
            ft = !1,
            a, v, w = !1,
            it = !0,
            l = !1,
            p = f || t,
            o, e = this,
            y, c, h;
        this.id = r, this.safeId = r.replace("/", "|"), this.loop = 0, this.unbind = function() {
            it = !1
        }, this.play = function() {
            return g = !0, this
        }, y = [], this.complete = function(n) {
            y.push(n)
        }, c = [], this.toFinish = function(n, t) {
            c.push({
                secs: n,
                callback: t
            })
        }, this.pause = function() {
            d = !0
        }, this.restart = function() {
            k = !0
        }, this.stop = function() {
            this.restart()
        }, this.setVolume = function(n) {
            a = n
        }, this.setPlaybackRate = function(n) {
            ft = n, b = !0
        }, this.setMuting = function(n) {
            w = n
        }, this.setProgress = function(n) {
            o && o.duration && isFinite(n) && (o.currentTime = o.duration * n)
        }, this.setCurrentTime = function(n) {
            o && o.duration && (o.currentTime = n)
        }, this.destroy = rt, p.$on("$destroy", function() {
            rt()
        }), this.destroyed = function() {
            return l
        }, n.find(r).then(function(n) {
            o = n, u.unlock && (window.addEventListener("click", tt), o.addEventListener("playing", function() {
                window.removeEventListener("click", tt)
            })), o.addEventListener("error", ut), o.addEventListener("canplay", function() {
                e.canPlay = !0
            })
        }, ut), h = i(et, u.performance), nt = p.$watch(function() {
            return u.performance
        }, function() {
            i.cancel(h), h = i(et, u.performance)
        })
    }
}]).service("ngAudio", ["NgAudioObject", "ngAudioGlobals", function(n, t) {
    this.play = function(t, i) {
        var r = new n(t, i);
        return r.play(), r
    }, this.load = function(t, i) {
        return new n(t, i)
    }, this.mute = function() {
        t.muting = !0
    }, this.unmute = function() {
        t.muting = !1
    }, this.toggleMute = function() {
        t.muting = !t.muting
    }, this.setUnlock = function(n) {
        t.unlock = n
    }, this.setGlobalVolume = function(n) {
        t.volume = n
    }
}]).filter("trackTime", function() {
    return function(n) {
        var t = Math.floor(n | 0),
            f = "",
            u = 0,
            i = 0,
            r = 0;
        return t > 3599 ? (u = Math.floor(t / 3600), i = Math.floor((t - u * 3600) / 60), r = t - (i * 60 + u * 3600), u.toString().length == 1 && (u = "0" + Math.floor(t / 3600).toString()), i.toString().length == 1 && (i = "0" + Math.floor((t - u * 3600) / 60).toString()), r.toString().length == 1 && (r = "0" + (t - (i * 60 + u * 3600)).toString()), f = u + ":" + i + ":" + r) : t > 59 ? (i = Math.floor(t / 60), r = t - i * 60, i.toString().length == 1 && (i = "0" + Math.floor(t / 60).toString()), r.toString().length == 1 && (r = "0" + (t - i * 60).toString()), f = i + ":" + r) : (r = t, r.toString().length == 1 && (r = "0" + t.toString()), f = t + "s"), typeof Number.isNaN == "function" && Number.isNaN(f), f
    }
});