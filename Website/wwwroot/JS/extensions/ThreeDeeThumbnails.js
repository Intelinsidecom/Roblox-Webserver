// extensions/ThreeDeeThumbnails.js
(function() {
    function c(n) {
        return $.ajax({
            dataType: "script",
            cache: !0,
            url: n
        })
    }

    function t(n, i) {
        var r = n.shift();
        r != undefined ? c(r).done(function() {
            t(n, i)
        }) : i != undefined && i()
    }

    function l(n, r) {
        if (i) n();
        else {
            var u = r.split(",");
            t(u, function() {
                i = !0, n()
            })
        }
    }

    function a(n) {
        var u = new THREE.AmbientLight(8882048),
            t, i, r;
        n.add(u), t = new THREE.DirectionalLight(11316396), t.position.set(-.671597898, .671597898, .312909544).normalize(), n.add(t), i = new THREE.DirectionalLight(4473924), r = (new THREE.Vector3).copy(t.position).negate().normalize(), i.position.set(r), n.add(i)
    }

    function r(n) {
        n.find(".thumbnail-spinner").remove(), n.find("canvas").remove()
    }
    var u = 70,
        f = .1,
        e = 1e3,
        o = 500,
        s = 10,
        h = 2e3,
        n, i = !1;
    $.fn.load3DThumbnail = function(t, i) {
        function y() {
            c = !0
        }

        function v(n, t, r) {
            typeof r == "undefined" && (r = 0), $.ajax({
                url: n + "&_=" + $.now(),
                method: "GET",
                crossDomain: !0,
                xhrFields: {
                    withCredentials: !0
                },
                cache: !1
            }).success(function(u) {
                u.Final ? $.getJSON(u.Url, function(n) {
                    t(n)
                }).fail(function() {
                    GoogleAnalyticsEvents && GoogleAnalyticsEvents.FireEvent(["3D Thumbnail Errors", u.Url]), i("3D Thumbnail failed to load")
                }) : r < s && c == !1 && setTimeout(function() {
                    v(n, t, r + 1)
                }, h)
            })
        }

        function p(t, r, o, s, h) {
            function y() {
                n.render(l, c)
            }

            function b() {
                v.enabled && v.update(), TWEEN.update(), y(), requestAnimationFrame(b)
            }

            function d() {
                c.aspect = o.width() / o.height(), c.updateProjectionMatrix(), n.setSize(o.width(), o.height())
            }

            function g() {
                n.setSize(w, p);
                var t = n.domElement,
                    i;
                return $(window).resize(function() {
                    clearTimeout(i), i = setTimeout(d, 100)
                }), window.onbeforeunload = function() {
                    $(t).hide()
                }, t
            }

            function nt() {
                var n = new THREE.OrbitControls(c, o.get(0), s);
                return n.rotateSpeed = 1.5, n.zoomSpeed = 1.5, n.dynamicDampingFactor = .3, n.addEventListener("change", y), n
            }

            function k(n) {
                a(l), l.add(n);
                var t = g();
                v = nt(), y(), b(), h(t)
            }
            var tt = "/thumbnail/resolve-hash/",
                p = o.height(),
                w = o.width(),
                c = new THREE.PerspectiveCamera(u, w / p, f, e),
                l = new THREE.Scene,
                v, it = new THREE.OBJMTLLoader;
            it.load(tt, t, r, k, undefined, i)
        }
        var c = !1;
        return this.each(function() {
            try {
                var u = $(this),
                    f = u.data("js-files"),
                    e = function() {
                        function s() {
                            f = !0, i.show(), i.empty(), setTimeout(function() {
                                f && (i.addClass("text-center"), i.html("<div class='loader' style='line-height:" + u.height().toString() + "px'>Loading</div><div></div>"))
                            }, o)
                        }

                        function h() {
                            f = !1, i.hide(), i.empty(), i.removeClass("text-center")
                        }

                        function l(n) {
                            p(n.obj, n.mtl, u, n, function(n) {
                                r(u), c == !1 && (h(), u.append(n), t(n))
                            })
                        }
                        var e, i, f;
                        n || (n = new THREE.WebGLRenderer({
                            antialias: !0,
                            alpha: !0
                        })), e = u.data("3d-url"), r(u), i = $("<div class='thumbnail-spinner'></div>"), i.appendTo(u), f = !1, s(), v(e, l)
                    };
                l(e, f)
            } catch (s) {
                i(s)
            }
        }), {
            cancel: y
        }
    }
})();