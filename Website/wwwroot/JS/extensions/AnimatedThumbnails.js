$(function() {
        function r(n) {
            n.find("canvas").remove()
        }

        function u(n) {
            var i, r; for(i in t)if(t[i].container===n)return t[i].renderer; return r= {
                container:n, renderer:new THREE.WebGLRenderer({
                    antialias: !0, alpha: !0
                })
        }

        , t.push(r), r.renderer
    }

    function v(n) {
        var u=new THREE.AmbientLight(8882048), t, i, r; n.add(u), t=new THREE.DirectionalLight(11316396), t.position.set(-.671597898, .671597898, .312909544).normalize(), n.add(t), i=new THREE.DirectionalLight(4473924), r=(new THREE.Vector3).copy(t.position).negate().normalize(), i.position.set(r), n.add(i)
    }

    function y(n, t, i) {
        function y() {
            return n.parent().width()
        }

        function w() {
            return n.parent().height()
        }

        function st(n, t, i) {
            var f, u, r; i.preload(), f=new THREE.MTLLoader.MaterialCreator, f.setMaterials(i), t.children[1].geometry.computeBoundingBox(), u=t.children[1].geometry.boundingBox, r=new THREE.Vector3, r.subVectors(u.max, u.min), r.multiplyScalar(.5), r.add(u.min), r.applyMatrix4(t.children[0].matrixWorld), t.children[1].geometry.applyMatrix((new THREE.Matrix4).makeTranslation(-r.x, -r.y, -r.z)), t.children[1].geometry.verticesNeedUpdate= !0, t.children[1].position.x=r.x, t.children[1].position.y=r.y, t.children[1].position.z=r.z, rt[n]=t, t.matrixAutoUpdate= !1, e.add(t), it++, ht()
        }

        function ht() {
            it===wt&&ct()
        }

        function ct() {
            v(e); var n=ut(); h=bt(e, s), d(), tt(), i(n)
        }

        function d() {
            p.render(e, s)
        }

        function tt() {
            yt(), h.enabled&&h.update(), TWEEN.update(), d(), requestAnimationFrame(tt)
        }

        function yt() {
            var r, t, n, i; for(h.update(), o===nt.length&&(o=0), r=nt[o], t=0; t<f.length; ++t)n=r[f[t]], i=rt[f[t]], typeof n !="undefined" &&typeof i !="undefined" &&(i.children[1].position.set(n.Position.x, n.Position.y, n.Position.z), i.children[1].quaternion.set(n.Rotation.x, n.Rotation.y, n.Rotation.z, n.Rotation.w), i.updateMatrix()); o++, e.updateMatrixWorld
        }

        function pt() {
            s.aspect=y()/w(), s.updateProjectionMatrix(), p.setSize(y(), w())
        }

        function ut() {
            p.setSize(y(), w()); var n=p.domElement, t; $(window).resize(function() {
                    clearTimeout(t), t=setTimeout(pt, 100)

                }).on("beforeunload", function() {
                    $(n).hide()
                }); return n
        }

        function bt(i, r) {
            var u=new THREE.OrbitControls(r, n.get(0), t, "animated"); return u.addEventListener("change", d), u
        }

        var p=u(n), g=t.aabb.max, vt=new THREE.Vector3(g.x, g.y, g.z).length()*4, at=Math.max(vt, a), s=new THREE.PerspectiveCamera(c, y()/w(), l, at), e=new THREE.Scene, b, nt, o; e.add(s); var h, rt= {}

        , lt=new THREE.OBJMTLLoader, f=Object.keys(t.partobjs), it=0, wt=f.length; for(b=0; b<f.length; ++b) {
            var k=f[b], ft=t.partobjs[k].files["scene.obj"].content, et=t.partobjs[k].files["scene.mtl"].content; function ot(n) {
                return function(t, i) {
                    return st(n, t, i)
                }
            }

            lt.load(ft, et, ot(k))
        }

        nt=t.frames, o=0
    }

    var i=$("#image-retry-data"), f=10, e=i?i.data("ga-logging-percent"):0, o=window.GoogleAnalyticsEvents&&GoogleAnalyticsEvents.FireEvent||function() {}

    , n=function(n) {
        Math.random()<=e/100&&o(n)
    }

    , s=500, h=1500, c=70, l=.1, a=1e3, t=[]; $.fn.loadAnimatedThumbnail=function(t, i) {
        function c() {
            e= !0
        }

        function o(t, r, u, f) {
            function e(i) {
                if(i.realRegeneration= !0, u-->0)i.retriesDone++, setTimeout(function() {
                        o(t, r, u, i)
                    }

                    , h); else {
                    var f=+new Date-i.start; n(["ThumbnailGenTime", "Animated", "Gave Up", f]), n(["ThumbnailGenRetries", "Animated", "Gave Up", i.retriesDone])
                }
            }

            function s(t, u) {
                var f=+new Date-u.start; t.Final?$.getJSON(t.Url, function(t) {
                        u.realRegeneration&&(n(["ThumbnailGenTime", "Animated", "Success", f]), n(["ThumbnailGenRetries", "Animated", "Success", u.retriesDone])), r(t, u)

                    }).fail(function() {
                        n(["ThumbnailGenErrors", "Animated", "Failure", t.Url]), n(["ThumbnailGenTime", "Animated", "Failure", f]), n(["ThumbnailGenRetries", "Animated", "Failure", u.retriesDone]), i("Animated Thumbnail failed to load")
                    }):e(u)
            }

            $.ajax({
                url:t+"&_=" +$.now(), method:"GET", crossDomain: !0, xhrFields: {
                    withCredentials: !0
                }

                , cache: !1

            }).success(function(n) {
                s(n, f)

            }).fail(function() {
                e(f)
            })
    }

    var e= !1; return this.each(function() {
            var h=$(this), c; try {
                c=function() {
                    function v() {
                        c&&(clearTimeout(c), c=undefined)
                    }

                    function p() {
                        c=setTimeout(function() {
                                e||Roblox.ThumbnailSpinner.show(h)
                            }

                            , s)
                    }

                    function w() {
                        v(), Roblox.ThumbnailSpinner.hide(h)
                    }

                    function b(n) {
                        var t=new MutationObserver(function(i) {
                                i.forEach(function(i) {
                                        var f=i.removedNodes, r, e; if(i.type==="childList")for(r=0; r<f.length; ++r)if(f[r]===n) {
                                            e=u(h), e.forceContextLoss(), t.disconnect(); return
                                        }
                                    })

                            }); t.observe(n.parentNode, {
                            childList: !0
                        })
                }

                function k(u, f) {
                    var o=+new Date; y(h, u, function(i) {
                            if(r(h), !e) {
                                w(), h.append(i), b(i); var u=+new Date-o; f.realRegeneration&&n(["ThumbnailInBrowserRenderTime", "Animated", "Success", u]), t(i)
                            }
                        }

                        , i)
                }

                var a= {
                    start:+new Date, realRegeneration: !1, retriesDone:0
                }

                , l=h.data("animated-asset-url"), c; l&&(r(h), p(), o(l, k, f, a))
            }

            , c()
        }

        catch(l) {
            i(l)
        }

    }), {
cancel:c
}
}
});