// extensions/ThreeDeeThumbnails.js
(function() {
    function c(n) {
        // Check if script is already loaded to prevent duplicates
        if (window.loadedScripts && window.loadedScripts[n]) {
            return $.Deferred().resolve().promise();
        }
        
        window.loadedScripts = window.loadedScripts || {};
        window.loadedScripts[n] = true;
        
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
        // If scripts have already been loaded (i == true) **or** there is no
        // data-js-files value provided, just invoke the callback immediately.
        // This allows pages that preload three.js and friends globally to work
        // without needing data-js-files on the thumbnail-span.
        if (i || !r) {
            n();
        } else {
            var u = r.split(",");
            t(u, function() {
                i = !0;
                n();
            });
        }
    }

    function a(n) {
        // Add stronger ambient light for better visibility
        var ambientLight = new THREE.AmbientLight(0x404040, 0.6);
        n.add(ambientLight);
        
        // Add main directional light from front-top-right
        var mainLight = new THREE.DirectionalLight(0xffffff, 0.8);
        mainLight.position.set(1, 1, 1);
        n.add(mainLight);
        
        // Add fill light from opposite side
        var fillLight = new THREE.DirectionalLight(0x404040, 0.4);
        fillLight.position.set(-1, 0.5, -1);
        n.add(fillLight);
        
        console.log('Lighting setup complete: ambient + 2 directional lights');
    }

    function r(n) {
        // Clear any existing 3D canvas and thumbnail loader UI
        n.find("canvas").remove();
        if (window.Roblox && Roblox.ThumbnailSpinner) {
            Roblox.ThumbnailSpinner.hide(n);
        } else {
            n.find(".thumbnail-spinner").remove();
        }
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
                // Ensure we have valid dimensions
                var actualWidth = w || o.width() || 277;
                var actualHeight = p || o.height() || 277;
                console.log('Setting canvas size to:', actualWidth, 'x', actualHeight);
                
                n.setSize(actualWidth, actualHeight);
                var t = n.domElement,
                    i;
                
                // Ensure canvas has proper styling
                t.style.width = actualWidth + 'px';
                t.style.height = actualHeight + 'px';
                t.style.display = 'block';
                
                return $(window).resize(function() {
                    clearTimeout(i), i = setTimeout(d, 100)
                }), window.onbeforeunload = function() {
                    $(t).hide()
                }, t
            }

            function nt() {
                // Use standard OrbitControls constructor (camera, domElement)
                var n = new THREE.OrbitControls(c, o.get(0));
                n.rotateSpeed = 1.5;
                n.zoomSpeed = 1.5;
                n.dynamicDampingFactor = 0.3;
                n.enableDamping = true;
                n.dampingFactor = 0.3;
                n.addEventListener("change", y);
                return n;
            }

            function k(n) {
                console.log('Adding object to scene:', n);
                
                // Add lighting first
                a(l);
                
                // Add the model to scene
                l.add(n);
                console.log('Scene children count:', l.children.length);
                
                // Log model details
                if (n.geometry) {
                    console.log('Model geometry:', n.geometry);
                } else if (n.children && n.children.length > 0) {
                    console.log('Model has', n.children.length, 'children');
                    n.children.forEach((child, index) => {
                        console.log('Child', index, ':', child.type, child.geometry ? 'with geometry' : 'no geometry');
                    });
                }
                
                var t = g();
                console.log('Canvas element created:', t);
                v = nt(), y(), b(), h(t);
                console.log('Canvas appended to DOM, size:', t.width, 'x', t.height);
                
                // Force a render to see if anything appears
                console.log('Forcing initial render...');
                y();
            }
            var p = o.height() || 277,
                w = o.width() || 277,
                c = new THREE.PerspectiveCamera(u, w / p, f, e),
                l = new THREE.Scene,
                v;

            console.log('Container dimensions:', w, 'x', p);

            // Position camera to view the model - move back further and slightly up
            c.position.set(0, 1, 8);
            c.lookAt(0, 0, 0);
            console.log('Camera positioned at:', c.position, 'looking at origin');

            // Load the actual OBJ/MTL files instead of placeholder cube
            if (t && r && (typeof THREE.SimpleOBJMTLLoader !== 'undefined' || typeof THREE.OBJMTLLoader !== 'undefined')) {
                console.log('Loading 3D model:', { obj: t, mtl: r });
                var loader = typeof THREE.SimpleOBJMTLLoader !== 'undefined' ? new THREE.SimpleOBJMTLLoader() : new THREE.OBJMTLLoader();
                loader.load(t, r, function(object, materials) {
                    console.log('3D model loaded successfully:', object);
                    // Scale and position the loaded model appropriately
                    if (object) {
                        // Make the model larger and center it better
                        object.scale.set(2, 2, 2);
                        object.position.set(0, 0, 0);
                        
                        // Calculate bounding box for better positioning
                        var box = new THREE.Box3().setFromObject(object);
                        var center = box.getCenter(new THREE.Vector3());
                        var size = box.getSize(new THREE.Vector3());
                        
                        console.log('Model bounding box - center:', center, 'size:', size);
                        
                        // Center the model
                        object.position.sub(center);
                        
                        k(object);
                    }
                }, function(progress) {
                    console.log('Loading progress:', progress);
                }, function(error) {
                    console.error('Error loading 3D model:', error);
                    // Fallback to placeholder cube on error
                    var geometry = new THREE.BoxGeometry(2, 2, 2);
                    var material = new THREE.MeshLambertMaterial({ color: 0xFF0000 }); // Red to indicate error
                    var mesh = new THREE.Mesh(geometry, material);
                    mesh.position.set(0, 0, 0);
                    console.log('Created fallback red cube at origin');
                    k(mesh);
                });
            } else {
                console.log('Fallback to placeholder cube. OBJ:', t, 'MTL:', r, 'SimpleOBJMTLLoader available:', typeof THREE.SimpleOBJMTLLoader !== 'undefined', 'OBJMTLLoader available:', typeof THREE.OBJMTLLoader !== 'undefined');
                // Fallback to placeholder cube if OBJ/MTL paths not provided or loader not available
                var geometry = new THREE.BoxGeometry(2, 2, 2);
                var material = new THREE.MeshLambertMaterial({ color: 0x00FF00 }); // Green to indicate fallback
                var mesh = new THREE.Mesh(geometry, material);
                mesh.position.set(0, 0, 0);
                console.log('Created fallback green cube at origin');
                k(mesh);
            }
        }

        var c = !1;
        return this.each(function() {
            try {
                var u = $(this),
                    f = u.data("js-files"),
                    e = function() {
                        function s() {
                            f = !0;
                            if (window.Roblox && Roblox.ThumbnailSpinner) {
                                Roblox.ThumbnailSpinner.show(u);
                            } else {
                                // Fallback to legacy inline spinner div
                                i.show(), i.empty(), setTimeout(function() {
                                    f && (i.addClass("text-center"), i.html("<div class='loader' style='line-height:" + u.height().toString() + "px'>Loading</div><div></div>"))
                                }, o)
                            }
                        }

                        function h() {
                            f = !1;
                            if (window.Roblox && Roblox.ThumbnailSpinner) {
                                Roblox.ThumbnailSpinner.hide(u);
                            } else {
                                i.hide(), i.empty(), i.removeClass("text-center")
                            }
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
                        })), e = u.data("3d-url"),
                        r(u), i = $("<div class='thumbnail-spinner'></div>"), i.appendTo(u), f = !1, s(), v(e, l)
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