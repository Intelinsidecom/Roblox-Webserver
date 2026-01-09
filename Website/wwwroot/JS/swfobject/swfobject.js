// swfobject/swfobject.js
/*!	SWFObject v2.2 <http://code.google.com/p/swfobject/> 
	is released under the MIT License <http://www.opensource.org/licenses/mit-license.php> 
*/
var swfobject = function() {
    function a() {
        var i,
            r,
            n;
        if (!l) {
            try {
                i = t.getElementsByTagName('body')[0].appendChild(o('span')),
                    i.parentNode.removeChild(i)
            } catch (u) {
                return
            }
            for (l = !0, r = k.length, n = 0; n < r; n++) k[n]()
        }
    }

    function lt(n) {
        l ? n() : k[k.length] = n
    }

    function ct(n) {
        if (typeof r.addEventListener != i) r.addEventListener('load', n, !1);
        else if (typeof t.addEventListener != i) t.addEventListener('load', n, !1);
        else if (typeof r.attachEvent != i) ii(r, 'onload', n);
        else if (typeof r.onload == 'function') {
            var u = r.onload;
            r.onload = function() {
                u(),
                    n()
            }
        } else r.onload = n
    }

    function dt() {
        wt ? ri() : ot()
    }

    function ri() {
        var s = t.getElementsByTagName('body')[0],
            u = o(e),
            r,
            f;
        u.setAttribute('type', g),
            r = s.appendChild(u),
            r ? (
                f = 0,
                function() {
                    if (typeof r.GetVariable != i) {
                        var t = r.GetVariable('$version');
                        t &&
                            (
                                t = t.split(' ')[1].split(','),
                                n.pv = [
                                    parseInt(t[0], 10),
                                    parseInt(t[1], 10),
                                    parseInt(t[2], 10)
                                ]
                            )
                    } else if (f < 10) {
                        f++,
                        setTimeout(arguments.callee, 10);
                        return
                    }
                    s.removeChild(u),
                        r = null,
                        ot()
                }()
            ) : ot()
    }

    function ot() {
        var y = h.length,
            r,
            t,
            s,
            l,
            v;
        if (y > 0)
            for (r = 0; r < y; r++) {
                var f = h[r].id,
                    e = h[r].callbackFn,
                    o = {
                        success: !1,
                        id: f
                    };
                if (n.pv[0] > 0) {
                    if (t = u(f), t)
                        if (!d(h[r].swfVersion) || n.wk && n.wk < 312)
                            if (h[r].expressInstall && ft()) {
                                s = {},
                                    s.data = h[r].expressInstall,
                                    s.width = t.getAttribute('width') ||
                                    '0',
                                    s.height = t.getAttribute('height') ||
                                    '0',
                                    t.getAttribute('class') &&
                                    (s.styleclass = t.getAttribute('class')),
                                    t.getAttribute('align') &&
                                    (s.align = t.getAttribute('align'));
                                var p = {},
                                    a = t.getElementsByTagName('param'),
                                    w = a.length;
                                for (l = 0; l < w; l++) a[l].getAttribute('name').toLowerCase() != 'movie' &&
                                    (p[a[l].getAttribute('name')] = a[l].getAttribute('value'));
                                ut(s, p, f, e)
                            } else gt(t),
                                e &&
                                e(o);
                    else c(f, !0),
                        e &&
                        (o.success = !0, o.ref = et(f), e(o))
                } else c(f, !0),
                    e &&
                    (
                        v = et(f),
                        v &&
                        typeof v.SetVariable != i &&
                        (o.success = !0, o.ref = v),
                        e(o)
                    )
            }
    }

    function et(n) {
        var r = null,
            t = u(n),
            f;
        return t &&
            t.nodeName == 'OBJECT' &&
            (
                typeof t.SetVariable != i ? r = t : (f = t.getElementsByTagName(e)[0], f && (r = f))
            ),
            r
    }

    function ft() {
        return !p &&
            d('6.0.65') &&
            (n.win || n.mac) &&
            !(n.wk && n.wk < 312)
    }

    function ut(f, e, s, h) {
        var c,
            v,
            l,
            a;
        p = !0,
            nt = h ||
            null,
            pt = {
                success: !1,
                id: s
            },
            c = u(s),
            c &&
            (
                c.nodeName == 'OBJECT' ? (y = rt(c), w = null) : (y = c, w = s),
                f.id = kt,
                (
                    typeof f.width == i ||
                    !/%$/.test(f.width) &&
                    parseInt(f.width, 10) < 310
                ) &&
                (f.width = '310'),
                (
                    typeof f.height == i ||
                    !/%$/.test(f.height) &&
                    parseInt(f.height, 10) < 137
                ) &&
                (f.height = '137'),
                t.title = t.title.slice(0, 47) + ' - Flash Player Installation',
                v = n.ie &&
                n.win ? 'ActiveX' : 'PlugIn',
                l = 'MMredirectURL=' + r.location.toString().replace(/&/g, '%26') + '&MMplayerType=' + v + '&MMdoctitle=' + t.title,
                typeof e.flashvars != i ? e.flashvars += '&' + l : e.flashvars = l,
                n.ie &&
                n.win &&
                c.readyState != 4 &&
                (
                    a = o('div'),
                    s += 'SWFObjectNew',
                    a.setAttribute('id', s),
                    c.parentNode.insertBefore(a, c),
                    c.style.display = 'none',
                    function() {
                        c.readyState == 4 ? c.parentNode.removeChild(c) : setTimeout(arguments.callee, 10)
                    }()
                ),
                it(f, e, s)
            )
    }

    function gt(t) {
        if (n.ie && n.win && t.readyState != 4) {
            var i = o('div');
            t.parentNode.insertBefore(i, t),
                i.parentNode.replaceChild(rt(t), i),
                t.style.display = 'none',
                function() {
                    t.readyState == 4 ? t.parentNode.removeChild(t) : setTimeout(arguments.callee, 10)
                }()
        } else t.parentNode.replaceChild(rt(t), t)
    }

    function rt(t) {
        var u = o('div'),
            f,
            i,
            s,
            r;
        if (n.win && n.ie) u.innerHTML = t.innerHTML;
        else if (f = t.getElementsByTagName(e)[0], f && (i = f.childNodes, i))
            for (s = i.length, r = 0; r < s; r++) i[r].nodeType == 1 &&
                i[r].nodeName == 'PARAM' ||
                i[r].nodeType == 8 ||
                u.appendChild(i[r].cloneNode(!0));
        return u
    }

    function it(t, r, f) {
        var v,
            y = u(f),
            p,
            s,
            w,
            a,
            c,
            h,
            l;
        if (n.wk && n.wk < 312) return v;
        if (y)
            if (typeof t.id == i && (t.id = f), n.ie && n.win) {
                p = '';
                for (s in t) t[s] != Object.prototype[s] &&
                    (
                        s.toLowerCase() == 'data' ? r.movie = t[s] : s.toLowerCase() == 'styleclass' ? p += ' class="' + t[s] + '"' : s.toLowerCase() != 'classid' &&
                        (p += ' ' + s + '="' + t[s] + '"')
                    );
                w = '';
                for (a in r) r[a] != Object.prototype[a] &&
                    (w += '<param name="' + a + '" value="' + r[a] + '" />');
                y.outerHTML = '<object classid="clsid:D27CDB6E-AE6D-11cf-96B8-444553540000"' + p + '>' + w + '</object>',
                    b[b.length] = t.id,
                    v = u(t.id)
            } else {
                c = o(e),
                    c.setAttribute('type', g);
                for (h in t) t[h] != Object.prototype[h] &&
                    (
                        h.toLowerCase() == 'styleclass' ? c.setAttribute('class', t[h]) : h.toLowerCase() != 'classid' &&
                        c.setAttribute(h, t[h])
                    );
                for (l in r) r[l] != Object.prototype[l] &&
                    l.toLowerCase() != 'movie' &&
                    ni(c, l, r[l]);
                y.parentNode.replaceChild(c, y),
                    v = c
            }
        return v
    }

    function ni(n, t, i) {
        var r = o('param');
        r.setAttribute('name', t),
            r.setAttribute('value', i),
            n.appendChild(r)
    }

    function st(t) {
        var i = u(t);
        i &&
            i.nodeName == 'OBJECT' &&
            (
                n.ie &&
                n.win ? (
                    i.style.display = 'none',
                    function() {
                        i.readyState == 4 ? ti(t) : setTimeout(arguments.callee, 10)
                    }()
                ) : i.parentNode.removeChild(i)
            )
    }

    function ti(n) {
        var t = u(n),
            i;
        if (t) {
            for (i in t) typeof t[i] == 'function' &&
                (t[i] = null);
            t.parentNode.removeChild(t)
        }
    }

    function u(n) {
        var i = null;
        try {
            i = t.getElementById(n)
        } catch (r) {}
        return i
    }

    function o(n) {
        return t.createElement(n)
    }

    function ii(n, t, i) {
        n.attachEvent(t, i),
            v[v.length] = [
                n,
                t,
                i
            ]
    }

    function d(t) {
        var r = n.pv,
            i = t.split('.');
        return i[0] = parseInt(i[0], 10),
            i[1] = parseInt(i[1], 10) ||
            0,
            i[2] = parseInt(i[2], 10) ||
            0,
            r[0] > i[0] ||
            r[0] == i[0] &&
            r[1] > i[1] ||
            r[0] == i[0] &&
            r[1] == i[1] &&
            r[2] >= i[2] ? !0 : !1
    }

    function vt(r, u, s, h) {
        var a,
            c,
            l;
        n.ie &&
            n.mac ||
            (a = t.getElementsByTagName('head')[0], a) &&
            (
                c = s &&
                typeof s == 'string' ? s : 'screen',
                h &&
                (f = null, tt = null),
                f &&
                tt == c ||
                (
                    l = o('style'),
                    l.setAttribute('type', 'text/css'),
                    l.setAttribute('media', c),
                    f = a.appendChild(l),
                    n.ie &&
                    n.win &&
                    typeof t.styleSheets != i &&
                    t.styleSheets.length > 0 &&
                    (f = t.styleSheets[t.styleSheets.length - 1]),
                    tt = c
                ),
                n.ie &&
                n.win ? f &&
                typeof f.addRule == e &&
                f.addRule(r, u) : f &&
                typeof t.createTextNode != i &&
                f.appendChild(t.createTextNode(r + ' {' + u + '}'))
            )
    }

    function c(n, t) {
        if (yt) {
            var i = t ? 'visible' : 'hidden';
            l &&
                u(n) ? u(n).style.visibility = i : vt('#' + n, 'visibility:' + i)
        }
    }

    function ht(n) {
        var t = /[\\\"<>\.;]/,
            r = t.exec(n) != null;
        return r &&
            typeof encodeURIComponent != i ? encodeURIComponent(n) : n
    }
    var i = 'undefined',
        e = 'object',
        at = 'Shockwave Flash',
        ui = 'ShockwaveFlash.ShockwaveFlash',
        g = 'application/x-shockwave-flash',
        kt = 'SWFObjectExprInst',
        bt = 'onreadystatechange',
        r = window,
        t = document,
        s = navigator,
        wt = !1,
        k = [
            dt
        ],
        h = [],
        b = [],
        v = [],
        y,
        w,
        nt,
        pt,
        l = !1,
        p = !1,
        f,
        tt,
        yt = !0,
        n = function() {
            var l = typeof t.getElementById != i &&
                typeof t.getElementsByTagName != i &&
                typeof t.createElement != i,
                f = s.userAgent.toLowerCase(),
                o = s.platform.toLowerCase(),
                a = o ? /win/.test(o) : /win/.test(f),
                v = o ? /mac/.test(o) : /mac/.test(f),
                y = /webkit/.test(f) ? parseFloat(f.replace(/^.*webkit\/(\d+(\.\d+)?).*$/, '$1')) : !1,
                h = !+'\v1',
                u = [
                    0,
                    0,
                    0
                ],
                n = null,
                c;
            if (typeof s.plugins != i && typeof s.plugins[at] == e) n = s.plugins[at].description,
                !n ||
                typeof s.mimeTypes != i &&
                s.mimeTypes[g] &&
                !s.mimeTypes[g].enabledPlugin ||
                (
                    wt = !0,
                    h = !1,
                    n = n.replace(/^.*\s+(\S+\s+\S+$)/, '$1'),
                    u[0] = parseInt(n.replace(/^(.*)\..*$/, '$1'), 10),
                    u[1] = parseInt(n.replace(/^.*\.(.*)\s.*$/, '$1'), 10),
                    u[2] = /[a-zA-Z]/.test(n) ? parseInt(n.replace(/^.*[a-zA-Z]+(.*)$/, '$1'), 10) : 0
                );
            else if (typeof r.ActiveXObject != i) try {
                c = new ActiveXObject(ui),
                    c &&
                    (
                        n = c.GetVariable('$version'),
                        n &&
                        (
                            h = !0,
                            n = n.split(' ')[1].split(','),
                            u = [
                                parseInt(n[0], 10),
                                parseInt(n[1], 10),
                                parseInt(n[2], 10)
                            ]
                        )
                    )
            } catch (p) {}
            return {
                w3: l,
                pv: u,
                wk: y,
                ie: h,
                win: a,
                mac: v
            }
        }(),
        fi = function() {
            n.w3 &&
                (
                    (
                        typeof t.readyState != i &&
                        t.readyState == 'complete' ||
                        typeof t.readyState == i &&
                        (t.getElementsByTagName('body')[0] || t.body)
                    ) &&
                    a(),
                    l ||
                    (
                        typeof t.addEventListener != i &&
                        t.addEventListener('DOMContentLoaded', a, !1),
                        n.ie &&
                        n.win &&
                        (
                            t.attachEvent(
                                bt,
                                function() {
                                    t.readyState == 'complete' &&
                                        (t.detachEvent(bt, arguments.callee), a())
                                }
                            ),
                            r == top &&
                            function() {
                                if (!l) {
                                    try {
                                        t.documentElement.doScroll('left')
                                    } catch (n) {
                                        setTimeout(arguments.callee, 0);
                                        return
                                    }
                                    a()
                                }
                            }()
                        ),
                        n.wk &&
                        function() {
                            if (!l) {
                                if (!/loaded|complete/.test(t.readyState)) {
                                    setTimeout(arguments.callee, 0);
                                    return
                                }
                                a()
                            }
                        }(),
                        ct(a)
                    )
                )
        }(),
        ei = function() {
            n.ie &&
                n.win &&
                window.attachEvent(
                    'onunload',
                    function() {
                        for (var e = v.length, r, i, u, f, t = 0; t < e; t++) v[t][0].detachEvent(v[t][1], v[t][2]);
                        for (r = b.length, i = 0; i < r; i++) st(b[i]);
                        for (u in n) n[u] = null;
                        n = null;
                        for (f in swfobject) swfobject[f] = null;
                        swfobject = null
                    }
                )
        }();
    return {
        registerObject: function(t, i, r, u) {
            if (n.w3 && t && i) {
                var f = {};
                f.id = t,
                    f.swfVersion = i,
                    f.expressInstall = r,
                    f.callbackFn = u,
                    h[h.length] = f,
                    c(t, !1)
            } else u &&
                u({
                    success: !1,
                    id: t
                })
        },
        getObjectById: function(t) {
            if (n.w3) return et(t)
        },
        embedSWF: function(t, r, u, f, o, s, h, l, a, v) {
            var y = {
                success: !1,
                id: r
            };
            n.w3 &&
                !(n.wk && n.wk < 312) &&
                t &&
                r &&
                u &&
                f &&
                o ? (
                    c(r, !1),
                    lt(
                        function() {
                            var n,
                                b,
                                p,
                                k,
                                w,
                                g;
                            if (u += '', f += '', n = {}, a && typeof a === e)
                                for (b in a) n[b] = a[b];
                            if (n.data = t, n.width = u, n.height = f, p = {}, l && typeof l === e)
                                for (k in l) p[k] = l[k];
                            if (h && typeof h === e)
                                for (w in h) typeof p.flashvars != i ? p.flashvars += '&' + w + '=' + h[w] : p.flashvars = w + '=' + h[w];
                            if (d(o)) g = it(n, p, r),
                                n.id == r &&
                                c(r, !0),
                                y.success = !0,
                                y.ref = g;
                            else {
                                if (s && ft()) {
                                    n.data = s,
                                        ut(n, p, r, v);
                                    return
                                }
                                c(r, !0)
                            }
                            v &&
                                v(y)
                        }
                    )
                ) : v &&
                v(y)
        },
        switchOffAutoHideShow: function() {
            yt = !1
        },
        ua: n,
        getFlashPlayerVersion: function() {
            return {
                major: n.pv[0],
                minor: n.pv[1],
                release: n.pv[2]
            }
        },
        hasFlashPlayerVersion: d,
        createSWF: function(t, i, r) {
            return n.w3 ? it(t, i, r) : undefined
        },
        showExpressInstall: function(t, i, r, u) {
            n.w3 &&
                ft() &&
                ut(t, i, r, u)
        },
        removeSWF: function(t) {
            n.w3 &&
                st(t)
        },
        createCSS: function(t, i, r, u) {
            n.w3 &&
                vt(t, i, r, u)
        },
        addDomLoadEvent: lt,
        addLoadEvent: ct,
        getQueryParamValue: function(n) {
            var r = t.location.search ||
                t.location.hash,
                u,
                i;
            if (r) {
                if (/\?/.test(r) && (r = r.split('?')[1]), n == null) return ht(r);
                for (u = r.split('&'), i = 0; i < u.length; i++)
                    if (u[i].substring(0, u[i].indexOf('=')) == n) return ht(u[i].substring(u[i].indexOf('=') + 1))
            }
            return ''
        },
        expressInstallCallback: function() {
            if (p) {
                var t = u(kt);
                t &&
                    y &&
                    (
                        t.parentNode.replaceChild(y, t),
                        w &&
                        (c(w, !0), n.ie && n.win && (y.style.display = 'block')),
                        nt &&
                        nt(pt)
                    ),
                    p = !1
            }
        }
    }
}();