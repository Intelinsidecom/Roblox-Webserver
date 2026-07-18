// extensions/ThreeDeeThumbnails.js
$(function() {
    function r(n) {
        var i, r;
        for (i in t)
            if (t[i].container == n) return t[i].renderer;
        return r = {
            container: n,
            renderer: new THREE.WebGLRenderer({
                antialias: !0,
                alpha: !0
            })
        }, t.push(r), r.renderer
    }

    function v(n) {
        var u = new THREE.AmbientLight(8882048),
            t, i, r;
        n.add(u), t = new THREE.DirectionalLight(11316396), t.position.set(-.671597898, .671597898, .312909544).normalize(), n.add(t), i = new THREE.DirectionalLight(4473924), r = (new THREE.Vector3).copy(t.position).negate().normalize(), i.position.set(r), n.add(i)
    }

    function u(n) {
        n.find("canvas").remove()
    }

    function y(n) {
        return n.parent() ? typeof n.parent().data("moderation-mode") == "string" : !1
    }

    function p(n, t, i, u, f, e) {
        function a() {
            return i.parent().width()
        }

        function l() {
            return i.parent().height()
        }

        function k() {
            p.render(w, c)
        }

        function d() {
            b.enabled && b.update(), TWEEN.update(), k(), requestAnimationFrame(d)
        }

        function it() {
            c.aspect = a() / l(), c.updateProjectionMatrix(), p.setSize(a(), l())
        }

        function rt() {
            p.setSize(a(), l());
            var n = p.domElement,
                t;
            $(window).resize(function() {
                clearTimeout(t), t = setTimeout(it, 100)
            }).on("beforeunload", function() {
                $(n).hide()
            });
            return n
        }

        function ut() {
            var n = new THREE.OrbitControls(c, i.get(0), u, "static");
            return tt ? (n.autoRotate = !1, n.minDistance = .1, n.noPan = !1, n.noKeys = !1, n.noResetToInitialCameraPosition = !0) : (n.rotateSpeed = 1.5, n.zoomSpeed = 1.5, n.dynamicDampingFactor = .3), n.addEventListener("change", k), n
        }

        function nt(n) {
            v(w), w.add(n);
            var t = rt();
            b = ut(), k(), d(), f(t)
        }
        var p = r(i),
            tt = y(i),
            ft = new THREE.Vector3(u.aabb.max.x, u.aabb.max.y, u.aabb.max.z).length() * 4,
            g = Math.max(ft, h),
            c = new THREE.PerspectiveCamera(o, a() / l(), s, g),
            w = new THREE.Scene,
            b, et = new THREE.OBJMTLLoader;
        et.load(n, t, nt, undefined, e)
    }
    var i = $("#image-retry-data"),
        f = i ? i.data("ga-logging-percent") : 0,
        e = window.GoogleAnalyticsEvents && GoogleAnalyticsEvents.FireEvent || function() {},
        n = function(n) {
            Math.random() <= f / 100 && e(n)
        },
        o = 70,
        s = .1,
        h = 1e3,
        c = 500,
        l = 10,
        a = 1500,
        t = [];
    $.fn.load3DThumbnail = function(t, i) {
        function o() {
            f = !0
        }

        function e(t, r, u, f) {
            function o(i) {
                if (i.realRegeneration = !0, u-- > 0) i.retriesDone++, setTimeout(function() {
                    e(t, r, u, i)
                }, a);
                else {
                    var f = +new Date - i.start;
                    n(["ThumbnailGenTime", "3D", "Gave Up", f]), n(["ThumbnailGenRetries", "3D", "Gave Up", i.retriesDone])
                }
            }

            function s(t, u) {
                var f = +new Date - u.start;
                t.Final ? $.getJSON(t.Url, function(t) {
                    u.realRegeneration && (n(["ThumbnailGenTime", "3D", "Success", f]), n(["ThumbnailGenRetries", "3D", "Success", u.retriesDone])), r(t, u)
                }).fail(function() {
                    n(["ThumbnailGenErrors", "3D", "Failure", t.Url]), n(["ThumbnailGenTime", "3D", "Failure", f]), n(["ThumbnailGenRetries", "3D", "Failure", u.retriesDone]), i("3D Thumbnail failed to load")
                }) : o(u)
            }
            $.ajax({
                url: t + "&_=" + $.now(),
                method: "GET",
                crossDomain: !0,
                xhrFields: {
                    withCredentials: !0
                },
                cache: !1
            }).success(function(n) {
                s(n, f)
            }).fail(function() {
                o(f)
            })
        }
        var f = !1;
        return this.each(function() {
            var o = $(this),
                s;
            try {
                s = function() {
                    function v() {
                        s && (clearTimeout(s), s = undefined)
                    }

                    function y() {
                        s = setTimeout(function() {
                            f || Roblox.ThumbnailSpinner.show(o)
                        }, c)
                    }

                    function w() {
                        v(), Roblox.ThumbnailSpinner.hide(o)
                    }

                    function b(n) {
                        var t = new MutationObserver(function(i) {
                            i.forEach(function(i) {
                                var f = i.removedNodes,
                                    u, e;
                                if (i.type === "childList")
                                    for (u = 0; u < f.length; ++u)
                                        if (f[u] === n) {
                                            e = r(o), e.forceContextLoss(), t.disconnect();
                                            return
                                        }
                            })
                        });
                        t.observe(n.parentNode, {
                            childList: !0
                        })
                    }

                    function k(r, e) {
                        var s = +new Date;
                        p(r.obj, r.mtl, o, r, function(i) {
                            if (u(o), !f) {
                                w(), o.append(i), b(i);
                                var r = +new Date - s;
                                e.realRegeneration && n(["ThumbnailInBrowserRenderTime", "3D", "Success", r]), t(i)
                            }
                        }, i)
                    }
                    var a = {
                            start: +new Date,
                            realRegeneration: !1,
                            retriesDone: 0
                        },
                        h = o.data("3d-url"),
                        s;
                    h && (u(o), y(), e(h, k, l, a))
                }, s()
            } catch (h) {
                i(h)
            }
        }), {
            cancel: o
        }
    }
});
