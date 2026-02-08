// Reference/modernizr.js
window.Modernizr = function(n, t, i) {
        function l(n) {
            c.cssText = n
        }

        function lt(n, t) {
            return l(tt.join(n + ";") + (t || ""))
        }

        function h(n, t) {
            return typeof n === t
        }

        function a(n, t) {
            return !!~("" + n).indexOf(t)
        }

        function st(n, t) {
            var u, r;
            for (u in n)
                if (r = n[u], !a(r, "-") && c[r] !== i) return t == "pfx" ? r : !0;
            return !1
        }

        function at(n, t, r) {
            var f, u;
            for (f in n)
                if (u = t[n[f]], u !== i) return r === !1 ? n[f] : h(u, "function") ? u.bind(r || t) : u;
            return !1
        }

        function e(n, t, i) {
            var r = n.charAt(0).toUpperCase() + n.slice(1),
                u = (n + " " + ft.join(r + " ") + r).split(" ");
            return h(t, "string") || h(t, "undefined") ? st(u, t) : (u = (n + " " + ht.join(r + " ") + r).split(" "), at(u, t, i))
        }

        function ct() {
            u.input = function(i) {
                for (var r = 0, u = i.length; r < u; r++) b[i[r]] = i[r] in f;
                return b.list && (b.list = !!t.createElement("datalist") && !!n.HTMLDataListElement), b
            }("autocomplete autofocus list placeholder max min multiple pattern required step".split(" ")), u.inputtypes = function(n) {
                for (var u = 0, r, e, o, h = n.length; u < h; u++) f.setAttribute("type", e = n[u]), r = f.type !== "text", r && (f.value = g, f.style.cssText = "position:absolute;visibility:hidden;", /^range$/.test(e) && f.style.WebkitAppearance !== i ? (s.appendChild(f), o = t.defaultView, r = o.getComputedStyle && o.getComputedStyle(f, null).WebkitAppearance !== "textfield" && f.offsetHeight !== 0, s.removeChild(f)) : /^(search|tel)$/.test(e) || (r = /^(url|email)$/.test(e) ? f.checkValidity && f.checkValidity() === !1 : f.value != g)), ut[n[u]] = !!r;
                return ut
            }("search tel url email datetime date month week time datetime-local number range color".split(" "))
        }
        var vt = "2.8.3",
            u = {},
            d = !0,
            s = t.documentElement,
            o = "modernizr",
            ot = t.createElement(o),
            c = ot.style,
            f = t.createElement("input"),
            g = ":)",
            yt = {}.toString,
            tt = " -webkit- -moz- -o- -ms- ".split(" "),
            et = "Webkit Moz O ms",
            ft = et.split(" "),
            ht = et.toLowerCase().split(" "),
            r = {},
            ut = {},
            b = {},
            it = [],
            nt = it.slice,
            w, y = function(n, i, r, u) {
                var l, a, c, v, f = t.createElement("div"),
                    h = t.body,
                    e = h || t.createElement("body");
                if (parseInt(r, 10))
                    while (r--) c = t.createElement("div"), c.id = u ? u[r] : o + (r + 1), f.appendChild(c);
                return l = ["&#173;", '<style id="s', o, '">', n, "</style>"].join(""), f.id = o, (h ? f : e).innerHTML += l, e.appendChild(f), h || (e.style.background = "", e.style.overflow = "hidden", v = s.style.overflow, s.style.overflow = "hidden", s.appendChild(e)), a = i(f, n), h ? f.parentNode.removeChild(f) : (e.parentNode.removeChild(e), s.style.overflow = v), !!a
            },
            rt = function() {
                function n(n, u) {
                    u = u || t.createElement(r[n] || "div"), n = "on" + n;
                    var f = n in u;
                    return f || (u.setAttribute || (u = t.createElement("div")), u.setAttribute && u.removeAttribute && (u.setAttribute(n, ""), f = h(u[n], "function"), h(u[n], "undefined") || (u[n] = i), u.removeAttribute(n))), u = null, f
                }
                var r = {
                    select: "input",
                    change: "input",
                    submit: "form",
                    reset: "form",
                    error: "img",
                    load: "img",
                    abort: "img"
                };
                return n
            }(),
            k = {}.hasOwnProperty,
            p, v;
        p = !h(k, "undefined") && !h(k.call, "undefined") ? function(n, t) {
            return k.call(n, t)
        } : function(n, t) {
            return t in n && h(n.constructor.prototype[t], "undefined")
        }, Function.prototype.bind || (Function.prototype.bind = function(n) {
            var t = this,
                i, r;
            if (typeof t != "function") throw new TypeError;
            return i = nt.call(arguments, 1), r = function() {
                var e, f, u;
                return this instanceof r ? (e = function() {}, e.prototype = t.prototype, f = new e, u = t.apply(f, i.concat(nt.call(arguments))), Object(u) === u ? u : f) : t.apply(n, i.concat(nt.call(arguments)))
            }, r
        }), r.flexbox = function() {
            return e("flexWrap")
        }, r.flexboxlegacy = function() {
            return e("boxDirection")
        }, r.canvas = function() {
            var n = t.createElement("canvas");
            return !!n.getContext && !!n.getContext("2d")
        }, r.canvastext = function() {
            return !!u.canvas && !!h(t.createElement("canvas").getContext("2d").fillText, "function")
        }, r.postmessage = function() {
            return !!n.postMessage
        }, r.websqldatabase = function() {
            return !!n.openDatabase
        }, r.indexedDB = function() {
            return !!e("indexedDB", n)
        }, r.hashchange = function() {
            return rt("hashchange", n) && (t.documentMode === i || t.documentMode > 7)
        }, r.history = function() {
            return !!n.history && !!history.pushState
        }, r.draganddrop = function() {
            var n = t.createElement("div");
            return "draggable" in n || "ondragstart" in n && "ondrop" in n
        }, r.websockets = function() {
            return "WebSocket" in n || "MozWebSocket" in n
        }, r.rgba = function() {
            return l("background-color:rgba(150,255,150,.5)"), a(c.backgroundColor, "rgba")
        }, r.hsla = function() {
            return l("background-color:hsla(120,40%,100%,.5)"), a(c.backgroundColor, "rgba") || a(c.backgroundColor, "hsla")
        }, r.multiplebgs = function() {
            return l("background:url(https://),url(https://),red url(https://)"), /(url\s*\(.*?){3}/.test(c.background)
        }, r.backgroundsize = function() {
            return e("backgroundSize")
        }, r.borderimage = function() {
            return e("borderImage")
        }, r.borderradius = function() {
            return e("borderRadius")
        }, r.boxshadow = function() {
            return e("boxShadow")
        }, r.textshadow = function() {
            return t.createElement("div").style.textShadow === ""
        }, r.opacity = function() {
            return lt("opacity:.55"), /^0.55$/.test(c.opacity)
        }, r.cssanimations = function() {
            return e("animationName")
        }, r.csscolumns = function() {
            return e("columnCount")
        }, r.cssgradients = function() {
            var n = "background-image:",
                t = "gradient(linear,left top,right bottom,from(#9f9),to(white));",
                i = "linear-gradient(left top,#9f9, white);";
            return l((n + "-webkit- ".split(" ").join(t + n) + tt.join(i + n)).slice(0, -n.length)), a(c.backgroundImage, "gradient")
        }, r.cssreflections = function() {
            return e("boxReflect")
        }, r.csstransforms = function() {
            return !!e("transform")
        }, r.csstransforms3d = function() {
            var n = !!e("perspective");
            return n && "webkitPerspective" in s.style && y("@media (transform-3d),(-webkit-transform-3d){#modernizr{left:9px;position:absolute;height:3px;}}", function(t) {
                n = t.offsetLeft === 9 && t.offsetHeight === 3
            }), n
        }, r.csstransitions = function() {
            return e("transition")
        }, r.fontface = function() {
            var n;
            return y('@font-face {font-family:"font";src:url("https://")}', function(i, r) {
                var f = t.getElementById("smodernizr"),
                    u = f.sheet || f.styleSheet,
                    e = u ? u.cssRules && u.cssRules[0] ? u.cssRules[0].cssText : u.cssText || "" : "";
                n = /src/i.test(e) && e.indexOf(r.split(" ")[0]) === 0
            }), n
        }, r.generatedcontent = function() {
            var n;
            return y(["#", o, "{font:0/0 a}#", o, ':after{content:"', g, '";visibility:hidden;font:3px/1 a}'].join(""), function(t) {
                n = t.offsetHeight >= 3
            }), n
        }, r.video = function() {
            var i = t.createElement("video"),
                n = !1;
            try {
                (n = !!i.canPlayType) && (n = new Boolean(n), n.ogg = i.canPlayType('video/ogg; codecs="theora"').replace(/^no$/, ""), n.h264 = i.canPlayType('video/mp4; codecs="avc1.42E01E"').replace(/^no$/, ""), n.webm = i.canPlayType('video/webm; codecs="vp8, vorbis"').replace(/^no$/, ""))
            } catch (r) {}
            return n
        }, r.audio = function() {
            var i = t.createElement("audio"),
                n = !1;
            try {
                (n = !!i.canPlayType) && (n = new Boolean(n), n.ogg = i.canPlayType('audio/ogg; codecs="vorbis"').replace(/^no$/, ""), n.mp3 = i.canPlayType("audio/mpeg;").replace(/^no$/, ""), n.wav = i.canPlayType('audio/wav; codecs="1"').replace(/^no$/, ""), n.m4a = (i.canPlayType("audio/x-m4a;") || i.canPlayType("audio/aac;")).replace(/^no$/, ""))
            } catch (r) {}
            return n
        }, r.localstorage = function() {
            try {
                return localStorage.setItem(o, o), localStorage.removeItem(o), !0
            } catch (n) {
                return !1
            }
        }, r.sessionstorage = function() {
            try {
                return sessionStorage.setItem(o, o), sessionStorage.removeItem(o), !0
            } catch (n) {
                return !1
            }
        }, r.webworkers = function() {
            return !!n.Worker
        }, r.applicationcache = function() {
            return !!n.applicationCache
        };
        for (v in r) p(r, v) && (w = v.toLowerCase(), u[w] = r[v](), it.push((u[w] ? "" : "no-") + w));
        return u.input || ct(), u.addTest = function(n, t) {
                if (typeof n == "object")
                    for (var r in n) p(n, r) && u.addTest(r, n[r]);
                else {
                    if (n = n.toLowerCase(), u[n] !== i) return u;
                    t = typeof t == "function" ? t() : t, typeof d != "undefined" && d && (s.className += " " + (t ? "" : "no-") + n), u[n] = t
                }
                return u
            }, l(""), ot = f = null,
            function(n, t) {
                function p(n, t) {
                    var i = n.createElement("p"),
                        r = n.getElementsByTagName("head")[0] || n.documentElement;
                    return i.innerHTML = "x<style>" + t + "</style>", r.insertBefore(i.lastChild, r.firstChild)
                }

                function a() {
                    var n = r.elements;
                    return typeof n == "string" ? n.split(" ") : n
                }

                function u(n) {
                    var t = l[n[c]];
                    return t || (t = {}, o++, n[c] = o, l[o] = t), t
                }

                function s(n, r, f) {
                    if (r || (r = t), i) return r.createElement(n);
                    f || (f = u(r));
                    var e;
                    return e = f.cache[n] ? f.cache[n].cloneNode() : b.test(n) ? (f.cache[n] = f.createElem(n)).cloneNode() : f.createElem(n), e.canHaveChildren && !w.test(n) && !e.tagUrn ? f.frag.appendChild(e) : e
                }

                function v(n, r) {
                    if (n || (n = t), i) return n.createDocumentFragment();
                    r = r || u(n);
                    for (var e = r.frag.cloneNode(), f = 0, o = a(), s = o.length; f < s; f++) e.createElement(o[f]);
                    return e
                }

                function y(n, t) {
                    t.cache || (t.cache = {}, t.createElem = n.createElement, t.createFrag = n.createDocumentFragment, t.frag = t.createFrag()), n.createElement = function(i) {
                        return r.shivMethods ? s(i, n, t) : t.createElem(i)
                    }, n.createDocumentFragment = Function("h,f", "return function(){var n=f.cloneNode(),c=n.createElement;h.shivMethods&&(" + a().join().replace(/[\w\-]+/g, function(n) {
                        return t.createElem(n), t.frag.createElement(n), 'c("' + n + '")'
                    }) + ");return n}")(r, t.frag)
                }

                function h(n) {
                    n || (n = t);
                    var f = u(n);
                    return r.shivCSS && !e && !f.hasCSS && (f.hasCSS = !!p(n, "article,aside,dialog,figcaption,figure,footer,header,hgroup,main,nav,section{display:block}mark{background:#FF0;color:#000}template{display:none}")), i || y(n, f), n
                }
                var k = "3.7.0",
                    f = n.html5 || {},
                    w = /^<|^(?:button|map|select|textarea|object|iframe|option|optgroup)$/i,
                    b = /^(?:a|b|code|div|fieldset|h1|h2|h3|h4|h5|h6|i|label|li|ol|p|q|span|strong|style|table|tbody|td|th|tr|ul)$/i,
                    e, c = "_html5shiv",
                    o = 0,
                    l = {},
                    i, r;
                (function() {
                    try {
                        var n = t.createElement("a");
                        n.innerHTML = "<xyz></xyz>", e = "hidden" in n, i = n.childNodes.length == 1 || function() {
                            t.createElement("a");
                            var n = t.createDocumentFragment();
                            return typeof n.cloneNode == "undefined" || typeof n.createDocumentFragment == "undefined" || typeof n.createElement == "undefined"
                        }()
                    } catch (r) {
                        e = !0, i = !0
                    }
                })(), r = {
                    elements: f.elements || "abbr article aside audio bdi canvas data datalist details dialog figcaption figure footer header hgroup main mark meter nav output progress section summary template time video",
                    version: k,
                    shivCSS: f.shivCSS !== !1,
                    supportsUnknownElements: i,
                    shivMethods: f.shivMethods !== !1,
                    type: "default",
                    shivDocument: h,
                    createElement: s,
                    createDocumentFragment: v
                }, n.html5 = r, h(t)
            }(this, t), u._version = vt, u._prefixes = tt, u._domPrefixes = ht, u._cssomPrefixes = ft, u.hasEvent = rt, u.testProp = function(n) {
                return st([n])
            }, u.testAllProps = e, u.testStyles = y, s.className = s.className.replace(/(^|\s)no-js(\s|$)/, "$1$2") + (d ? " js " + it.join(" ") : ""), u
    }(this, this.document),
    function(n, t, i) {
        function h(n) {
            return "[object Function]" == y.call(n)
        }

        function c(n) {
            return "string" == typeof n
        }

        function v() {}

        function tt(n) {
            return !n || "loaded" == n || "complete" == n || "uninitialized" == n
        }

        function e() {
            var n = l.shift();
            a = 1, n ? n.t ? o(function() {
                ("c" == n.t ? u.injectCss : u.injectJs)(n.s, 0, n.a, n.x, n.e, 1)
            }, 0) : (n(), e()) : a = 0
        }

        function ft(n, i, s, h, c, v, y) {
            function b(t) {
                if (!g && tt(p.readyState) && (nt.r = g = 1, !a && e(), p.onload = p.onreadystatechange = null, t)) {
                    "img" != n && o(function() {
                        d.removeChild(p)
                    }, 50);
                    for (var u in r[i]) r[i].hasOwnProperty(u) && r[i][u].onload()
                }
            }
            var y = y || u.errorTimeout,
                p = t.createElement(n),
                g = 0,
                w = 0,
                nt = {
                    t: s,
                    s: i,
                    e: c,
                    a: v,
                    x: y
                };
            1 === r[i] && (w = 1, r[i] = []), "object" == n ? p.data = i : (p.src = i, p.type = n), p.width = p.height = "0", p.onerror = p.onload = p.onreadystatechange = function() {
                b.call(this, w)
            }, l.splice(h, 0, nt), "img" != n && (w || 2 === r[i] ? (d.insertBefore(p, k ? null : f), o(b, y)) : r[i].push(p))
        }

        function ut(n, t, i, r, u) {
            return a = 0, t = t || "j", c(n) ? ft("c" == t ? et : g, n, t, this.i++, i, r, u) : (l.splice(this.i++, 0, n), 1 == l.length && e()), this
        }

        function rt() {
            var n = u;
            return n.loader = {
                load: ut,
                i: 0
            }, n
        }
        var s = t.documentElement,
            o = n.setTimeout,
            f = t.getElementsByTagName("script")[0],
            y = {}.toString,
            l = [],
            a = 0,
            b = "MozAppearance" in s.style,
            k = b && !!t.createRange().compareNode,
            d = k ? s : f.parentNode,
            s = n.opera && "[object Opera]" == y.call(n.opera),
            s = !!t.attachEvent && !s,
            g = b ? "object" : s ? "script" : "img",
            et = s ? "script" : g,
            nt = Array.isArray || function(n) {
                return "[object Array]" == y.call(n)
            },
            p = [],
            r = {},
            it = {
                timeout: function(n, t) {
                    return t.length && (n.timeout = t[0]), n
                }
            },
            w, u;
        u = function(n) {
            function l(n) {
                for (var n = n.split("!"), f = p.length, i = n.pop(), e = n.length, i = {
                        url: i,
                        origUrl: i,
                        prefixes: n
                    }, u, r, t = 0; t < e; t++) r = n[t].split("="), (u = it[r.shift()]) && (i = u(i, r));
                for (t = 0; t < f; t++) i = p[t](i);
                return i
            }

            function f(n, t, u, f, e) {
                var o = l(n),
                    s = o.autoCallback;
                o.url.split(".").pop().split("?").shift(), o.bypass || (t && (t = h(t) ? t : t[n] || t[f] || t[n.split("/").pop().split("?")[0]]), o.instead ? o.instead(n, t, u, f, e) : (r[o.url] ? o.noexec = !0 : r[o.url] = 1, u.load(o.url, o.forceCSS || !o.forceJS && "css" == o.url.split(".").pop().split("?").shift() ? "c" : i, o.noexec, o.attrs, o.timeout), (h(t) || h(s)) && u.load(function() {
                    rt(), t && t(o.origUrl, e, f), s && s(o.origUrl, e, f), r[o.url] = 2
                })))
            }

            function s(n, t) {
                function l(n, o) {
                    if (n) {
                        if (c(n)) o || (i = function() {
                            var n = [].slice.call(arguments);
                            s.apply(this, n), u()
                        }), f(n, i, t, 0, e);
                        else if (Object(n) === n)
                            for (r in a = function() {
                                    var t = 0,
                                        i;
                                    for (i in n) n.hasOwnProperty(i) && t++;
                                    return t
                                }(), n) n.hasOwnProperty(r) && (!o && !--a && (h(i) ? i = function() {
                                var n = [].slice.call(arguments);
                                s.apply(this, n), u()
                            } : i[r] = function(n) {
                                return function() {
                                    var t = [].slice.call(arguments);
                                    n && n.apply(this, t), u()
                                }
                            }(s[r])), f(n[r], i, t, r, e))
                    } else !o && u()
                }
                var e = !!n.test,
                    o = n.load || n.both,
                    i = n.callback || v,
                    s = i,
                    u = n.complete || v,
                    a, r;
                l(e ? n.yep : n.nope, !!o), o && l(o)
            }
            var e, t, o = this.yepnope.loader;
            if (c(n)) f(n, 0, o, 0);
            else if (nt(n))
                for (e = 0; e < n.length; e++) t = n[e], c(t) ? f(t, 0, o, 0) : nt(t) ? u(t) : Object(t) === t && s(t, o);
            else Object(n) === n && s(n, o)
        }, u.addPrefix = function(n, t) {
            it[n] = t
        }, u.addFilter = function(n) {
            p.push(n)
        }, u.errorTimeout = 1e4, null == t.readyState && t.addEventListener && (t.readyState = "loading", t.addEventListener("DOMContentLoaded", w = function() {
            t.removeEventListener("DOMContentLoaded", w, 0), t.readyState = "complete"
        }, 0)), n.yepnope = rt(), n.yepnope.executeStack = e, n.yepnope.injectJs = function(n, i, r, s, h, c) {
            var l = t.createElement("script"),
                a, y, s = s || u.errorTimeout;
            l.src = n;
            for (y in r) l.setAttribute(y, r[y]);
            i = c ? e : i || v, l.onreadystatechange = l.onload = function() {
                !a && tt(l.readyState) && (a = 1, i(), l.onload = l.onreadystatechange = null)
            }, o(function() {
                a || (a = 1, i(1))
            }, s), h ? l.onload() : f.parentNode.insertBefore(l, f)
        }, n.yepnope.injectCss = function(n, i, r, u, s, h) {
            var u = t.createElement("link"),
                c, i = h ? e : i || v;
            u.href = n, u.rel = "stylesheet", u.type = "text/css";
            for (c in r) u.setAttribute(c, r[c]);
            s || (f.parentNode.insertBefore(u, f), o(i, 0))
        }
    }(this, document), Modernizr.load = function() {
        yepnope.apply(window, [].slice.call(arguments, 0))
    };