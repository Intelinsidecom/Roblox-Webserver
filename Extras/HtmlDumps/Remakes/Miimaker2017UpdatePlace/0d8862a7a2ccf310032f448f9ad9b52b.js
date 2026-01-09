; // bundle: page___c05ce934217b9d4f928f57b731dfecdf_m
; // files: StyleguideConversions.js, swfobject/swfobject.js, YouTube/RobloxYouTube.js, IDE/Thumbnail.js, IDE/Icon.js, IDE/validator.js, IDE/Update.js, jquery/jquery-ui-sortable-1.10.4.min.js, places/Access.js, Places/DeviceSelection.js, utilities/dialog.js, GenericConfirmation.js, common/deviceMeta.js, AbuseReport/AbuseReportDispatcher.js
; // StyleguideConversions.js
typeof Roblox == 'undefined' &&
(Roblox = {}),
typeof Roblox.StyleguideConversions == 'undefined' &&
(
  Roblox.StyleguideConversions = function () {
    function n() {
      $('.field-validation-error').length > 0 &&
      $('.field-validation-error').each(
        function (n, t) {
          var i = $(t),
          r;
          i.addClass('tool-tip'),
          r = i.text(),
          i.html('<img src="/img/UI/img-tail-left.png" class="right">' + r)
        }
      )
    }
    return {
      convertMvcErrorToStyleGuide: n
    }
  }()
);
; // swfobject/swfobject.js
/*!	SWFObject v2.2 <http://code.google.com/p/swfobject/> 
	is released under the MIT License <http://www.opensource.org/licenses/mit-license.php> 
*/
var swfobject = function () {
  function a() {
    var i,
    r,
    n;
    if (!l) {
      try {
        i = t.getElementsByTagName('body') [0].appendChild(o('span')),
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
      r.onload = function () {
        u(),
        n()
      }
    } else r.onload = n
  }
  function dt() {
    wt ? ri() : ot()
  }
  function ri() {
    var s = t.getElementsByTagName('body') [0],
    u = o(e),
    r,
    f;
    u.setAttribute('type', g),
    r = s.appendChild(u),
    r ? (
      f = 0,
      function () {
        if (typeof r.GetVariable != i) {
          var t = r.GetVariable('$version');
          t &&
          (
            t = t.split(' ') [1].split(','),
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
    if (y > 0) for (r = 0; r < y; r++) {
      var f = h[r].id,
      e = h[r].callbackFn,
      o = {
        success: !1,
        id: f
      };
      if (n.pv[0] > 0) {
        if (t = u(f), t) if (!d(h[r].swfVersion) || n.wk && n.wk < 312) if (h[r].expressInstall && ft()) {
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
      typeof t.SetVariable != i ? r = t : (f = t.getElementsByTagName(e) [0], f && (r = f))
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
        function () {
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
      function () {
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
     else if (f = t.getElementsByTagName(e) [0], f && (i = f.childNodes, i)) for (s = i.length, r = 0; r < s; r++) i[r].nodeType == 1 &&
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
    if (y) if (typeof t.id == i && (t.id = f), n.ie && n.win) {
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
        function () {
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
    } catch (r) {
    }
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
    (a = t.getElementsByTagName('head') [0], a) &&
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
  n = function () {
    var l = typeof t.getElementById != i &&
    typeof t.getElementsByTagName != i &&
    typeof t.createElement != i,
    f = s.userAgent.toLowerCase(),
    o = s.platform.toLowerCase(),
    a = o ? /win/.test(o) : /win/.test(f),
    v = o ? /mac/.test(o) : /mac/.test(f),
    y = /webkit/.test(f) ? parseFloat(f.replace(/^.*webkit\/(\d+(\.\d+)?).*$/, '$1')) : !1,
    h = ! + '\v1',
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
          n = n.split(' ') [1].split(','),
          u = [
            parseInt(n[0], 10),
            parseInt(n[1], 10),
            parseInt(n[2], 10)
          ]
        )
      )
    } catch (p) {
    }
    return {
      w3: l,
      pv: u,
      wk: y,
      ie: h,
      win: a,
      mac: v
    }
  }(),
  fi = function () {
    n.w3 &&
    (
      (
        typeof t.readyState != i &&
        t.readyState == 'complete' ||
        typeof t.readyState == i &&
        (t.getElementsByTagName('body') [0] || t.body)
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
            function () {
              t.readyState == 'complete' &&
              (t.detachEvent(bt, arguments.callee), a())
            }
          ),
          r == top &&
          function () {
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
        function () {
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
  ei = function () {
    n.ie &&
    n.win &&
    window.attachEvent(
      'onunload',
      function () {
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
    registerObject: function (t, i, r, u) {
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
    getObjectById: function (t) {
      if (n.w3) return et(t)
    },
    embedSWF: function (t, r, u, f, o, s, h, l, a, v) {
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
          function () {
            var n,
            b,
            p,
            k,
            w,
            g;
            if (u += '', f += '', n = {}, a && typeof a === e) for (b in a) n[b] = a[b];
            if (n.data = t, n.width = u, n.height = f, p = {}, l && typeof l === e) for (k in l) p[k] = l[k];
            if (h && typeof h === e) for (w in h) typeof p.flashvars != i ? p.flashvars += '&' + w + '=' + h[w] : p.flashvars = w + '=' + h[w];
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
    switchOffAutoHideShow: function () {
      yt = !1
    },
    ua: n,
    getFlashPlayerVersion: function () {
      return {
        major: n.pv[0],
        minor: n.pv[1],
        release: n.pv[2]
      }
    },
    hasFlashPlayerVersion: d,
    createSWF: function (t, i, r) {
      return n.w3 ? it(t, i, r) : undefined
    },
    showExpressInstall: function (t, i, r, u) {
      n.w3 &&
      ft() &&
      ut(t, i, r, u)
    },
    removeSWF: function (t) {
      n.w3 &&
      st(t)
    },
    createCSS: function (t, i, r, u) {
      n.w3 &&
      vt(t, i, r, u)
    },
    addDomLoadEvent: lt,
    addLoadEvent: ct,
    getQueryParamValue: function (n) {
      var r = t.location.search ||
      t.location.hash,
      u,
      i;
      if (r) {
        if (/\?/.test(r) && (r = r.split('?') [1]), n == null) return ht(r);
        for (u = r.split('&'), i = 0; i < u.length; i++) if (u[i].substring(0, u[i].indexOf('=')) == n) return ht(u[i].substring(u[i].indexOf('=') + 1))
      }
      return ''
    },
    expressInstallCallback: function () {
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
; // YouTube/RobloxYouTube.js
function RobloxYouTubeVideo(n, t) {
  var i = this;
  this.RobloxVideoPlayerID = n,
  this.OnPlayerStateChangeCallback = function (r) {
    t(r, n, i)
  },
  this.YouTubeVideoID = null,
  this.Chromeless = !1,
  this.Autoplay = !1,
  this.Muted = !1,
  this.Player = function () {
    return document.getElementById(this.RobloxVideoPlayerID)
  },
  this.Init = function (n, t, r, u, f, e) {
    function s(n) {
      if (!o) {
        i.Muted &&
        n.target.mute();
        var t = navigator.userAgent.match(/iPad/i) != null;
        i.Autoplay &&
        !t &&
        n.target.playVideo()
      }
    }
    this.YouTubeVideoID = n,
    this.Chromeless = t,
    this.Autoplay = e,
    this.Muted = e;
    var o = u <= 120;
    RobloxYouTubeVideoManager.CallWhenReady(
      function () {
        var t = new YT.Player(
          r,
          {
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
              wmode: 'opaque'
            },
            videoId: n,
            events: {
              onReady: s
            }
          }
        );
        t.A &&
        (t.A.id = i.RobloxVideoPlayerID)
      }
    )
  },
  this.SeekToTime = function (n) {
    this.Player().seekTo(n, !0)
  },
  this.PauseVideo = function () {
    this.Player().pauseVideo()
  }
}
var RobloxYouTube = RobloxYouTube ||
{
  Events: {
    States: {
      Unstarted: - 1,
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
RobloxYouTubeVideoManager = function () {
  function f() {
    var t = document.createElement('script'),
    n;
    t.src = 'https://www.youtube.com/iframe_api',
    n = document.getElementsByTagName('script') [0],
    n.parentNode.insertBefore(t, n)
  }
  function e(t) {
    n ? t() : i.push(t)
  }
  function r() {
    n = !0,
    t !== null &&
    clearInterval(t);
    for (var r = 0; r < i.length; r++) i[r]()
  }
  function o(n) {
    return u[n.RobloxVideoPlayerID] = n,
    n
  }
  function s(n) {
    return u[n]
  }
  function h() {
    var i = 0,
    u = 10;
    t = setInterval(
      function () {
        if (i++, n || i > u) {
          clearInterval(t);
          return
        }
        n ||
        typeof YT == 'undefined' ||
        YT.loaded !== 1 ||
        r()
      },
      500
    )
  }
  function c() {
    $(
      function () {
        n ||
        typeof YT == 'undefined' ||
        (
          YT.loaded === 1 ? r() : window.onYouTubeIframeAPIReady = RobloxYouTubeVideoManager.OnYouTubeApiReady
        )
      }
    )
  }
  var u = [],
  i = [],
  n = !1,
  t = null;
  return c(),
  h(),
  {
    AddVideo: o,
    GetVideo: s,
    OnYouTubeApiReady: r,
    CallWhenReady: e,
    InsertIFrameAPI: f
  }
}();
window.onYoutubeIframeAPIReady = RobloxYouTubeVideoManager.OnYouTubeApiReady,
RobloxYouTubeVideoManager.InsertIFrameAPI();
; // IDE/Thumbnail.js
var Roblox = Roblox ||
{
};
Roblox.IDE = Roblox.IDE ||
{
},
Roblox.IDE.Resources = Roblox.IDE.Resources ||
{
},
$.extend(
  Roblox.IDE.Resources,
  {
    DeletetitleText: 'Delete Thumbnail',
    DeletebodyContent: 'Are you sure you want to permanently delete this thumbnail?',
    DeleteText: 'Delete',
    AddtitleText: 'Add Thumbnail',
    AddbodyContent: 'Are you sure you want to buy this thumbnail for ',
    AddacceptText: 'Buy Now',
    CancelText: 'Cancel',
    AddGeneratedThumbTitleText: 'Auto Generated Thumbnail',
    AddGeneratedThumbBodyText: 'This thumbnail will be moderated. Are you sure you want to submit this thumbnail?',
    AddGeneratedThumbAcceptText: 'Submit',
    AddGeneratedThumbCancelText: 'Cancel'
  }
),
Roblox.IDE.ThumbnailGallery = Roblox.IDE.ThumbnailGallery ||
function () {
  function n() {
    function s() {
      if (o) {
        var n = $('.SelectedYouTubeGalleryIcon').next();
        n.length > 0 &&
        u(n)
      }
    }
    function u(t) {
      var i = $(t).find('.smallGalleryThumbItem'),
      e = i.find('.divYouTubeVideoPlayer'),
      r,
      o,
      h,
      u,
      f;
      e.length != 0 ? (
        i = $(e.children() [0].cloneNode(!0)),
        (i.is('object') || i.is('iframe')) &&
        (
          i.height('230'),
          i.attr('Height', '230px'),
          i.width('420'),
          i.attr('Width', '420px'),
          r = RobloxYouTubeVideoManager.GetVideo(i.attr('id')),
          r.Muted = n,
          r.Autoplay = !0
        )
      ) : (
        o = i.find('img') [0],
        i = $(o).clone(),
        i.height('230'),
        i.width('420'),
        setTimeout(function () {
          s()
        }, 5000)
      ),
      $('#divTransitionLargeGalleryItem').remove(),
      $('#ItemThumbnail').append(
        '<div id=\'divTransitionLargeGalleryItem\' style=\'display: none; position: absolute;\'>'
      ),
      $('#divTransitionLargeGalleryItem').append(i),
      $('#divTransitionLargeGalleryItem').fadeIn('slow', function () {
      }),
      $('#bigGalleryThumbItem').fadeOut(
        'slow',
        function () {
          $('#divTransitionLargeGalleryItem').attr('id', 'bigGalleryThumbItem'),
          $(this).remove()
        }
      ),
      h = $('.SelectedYouTubeGalleryIcon').attr('data-place-media-item-alt-text'),
      u = $('#AltText').val(),
      h != u &&
      $('.SelectedYouTubeGalleryIcon').attr('data-place-media-item-alt-text', u),
      $('.divSmallGalleryItem').removeClass('SelectedYouTubeGalleryIcon'),
      $(t).addClass('SelectedYouTubeGalleryIcon'),
      f = $(t).attr('data-place-media-item-alt-text'),
      $('#AltText').val(f),
      $('#updateAltTextCharCount').text(f.length),
      $('#updateAltTextError').text(''),
      $('#updateAltTextFormGroup').removeClass('form-has-error'),
      $('#updateAltTextCharacters').show(),
      $('#updateAltTextSuccess').hide()
    }
    function h(t, i, r, u) {
      function f(t, i, r) {
        t == RobloxYouTube.Events.States.Ended &&
        (n = r.Player().isMuted())
      }
      var e = t,
      o = RobloxYouTubeVideoManager.AddVideo(new RobloxYouTubeVideo(e, f)),
      s = i,
      h = t,
      c = !1,
      l = !1;
      o.Init(s, c, h, r, u, l)
    }
    function c() {
      var t = [],
      n;
      $('.divSmallGalleryItem').each(function (n) {
        var i = $(this).data('place-media-item-id');
        t[n] = i
      }),
      n = '';
      for (key in t) n += t[key] + ',';
      f ? $.getJSON('/Thumbs/AssetMedia/PlaceMediaItemSortHandler.ashx', {
        sort: n
      }) : $.post('/thumbnail/set-asset-media-sort-order', {
        sort: n
      })
    }
    if (document.getElementById('ThumbnailMediaScript') === null) {
      var t = $('#ThumbnailDisplayContainer'),
      f = t.data('is-place'),
      e = t.data('media-items-count'),
      o = !1,
      n = !0,
      i = $('#AltText'),
      r = $('#saveAltTextButton');
      $('#thumbnails').attr('data-universe-id') == '0' &&
      i &&
      r &&
      (i.prop('disabled', !0), r.addClass('btn-disabled-neutral'));
      $('#AltText').on(
        'input',
        function () {
          $('#updateAltTextError').text(''),
          $('#updateAltTextCharCount').text($(this).val().length),
          $('#updateAltTextFormGroup').removeClass('form-has-error'),
          $('#updateAltTextCharacters').show(),
          $('#updateAltTextSuccess').hide()
        }
      );
      $('#saveAltTextButton').click(
        function () {
          var n = $(this),
          t = $('#thumbnails').attr('data-universe-id');
          if (t != '0') {
            var u = $('.SelectedYouTubeGalleryIcon').attr('data-place-media-item-id'),
            f = $('#AltText').val(),
            i = $('#updateAltTextError'),
            r = $('#updateAltTextSuccess');
            n.addClass('btn-disabled-neutral'),
            n.prop('disabled', !0),
            r.hide(),
            $.ajax({
              type: 'POST',
              url: Roblox.EnvironmentUrls.developApi + '/v1/universes/' + t + '/thumbnails/alt-text',
              data: JSON.stringify({
                MediaAssetId: u,
                MediaAssetAltText: f
              }),
              contentType: 'application/json; charset=utf-8',
              dataType: 'json',
              success: function (n) {
                $('.SelectedYouTubeGalleryIcon').attr('data-place-media-item-id') == n.MediaAssetId &&
                (
                  $('.SelectedYouTubeGalleryIcon').attr('data-place-media-item-alt-text', n.MediaAssetAltText),
                  $('#AltText').val(n.MediaAssetAltText)
                ),
                i.text(''),
                $('#updateAltTextFormGroup').removeClass('form-has-error'),
                $('#updateAltTextCharacters').show(),
                r.show()
              },
              error: function (n) {
                var t = n.responseJSON.errors[0].message;
                i.text(t),
                $('#updateAltTextFormGroup').addClass('form-has-error'),
                $('#updateAltTextCharacters').hide()
              },
              complete: function () {
                n.removeClass('btn-disabled-neutral'),
                n.prop('disabled', !1)
              }
            })
          }
        }
      ),
      $('.youTubeVideoOverlay').click(function () {
        n = !1,
        u($(this).parent())
      }),
      $('#divSmallGalleryScrollContainer').width(e * 130),
      $('#divSmallGalleryScrollContainer').sortable({
        update: function () {
          c()
        }
      }),
      $('.divYouTubeVideoPlayer').each(
        function () {
          var t = $(this).children('div') [0].id,
          n = $(this).data('video-size'),
          i = $(this).data('video-hash'),
          r = n === 'large' ? 420 : 120,
          u = n === 'large' ? 230 : 70;
          h(t, i, r, u)
        }
      )
    }
  }
  return {
    init: n
  }
}(),
Roblox.IDE.Thumbnails = Roblox.IDE.Thumbnails ||
function () {
  function i(n) {
    var t = $('input[name=thumbnailType]:checked'),
    i = '<span class=\'icon-robux-16x16\'></span><span>',
    r = i + t.attr('data-cost-in-robux') + '</span>';
    Roblox.GenericConfirmation.open({
      titleText: Roblox.IDE.Resources.AddtitleText,
      bodyContent: Roblox.IDE.Resources.AddbodyContent + r + '?',
      acceptText: Roblox.IDE.Resources.AddacceptText,
      declineText: Roblox.IDE.Resources.CancelText,
      allowHtmlContentInBody: !0,
      acceptColor: Roblox.GenericConfirmation.green,
      onAccept: n
    })
  }
  function u() {
    var t = $('#addGeneratedImageButton'),
    i;
    t.addClass(n),
    i = setInterval(
      function () {
        var r = $('#autogenerate span').attr('data-retry-url');
        r ||
        (t.removeClass(n), clearInterval(i))
      },
      300
    )
  }
  function f(t, i) {
    t.toggleClass('btn-neutral', i).toggleClass(n, !i),
    i ? t.removeAttr('disabled') : t.attr('disabled', '')
  }
  function t(n) {
    $('#thumbnails').html(n),
    Roblox.IDE.Thumbnails.init(),
    Roblox.IDE.ThumbnailGallery.init()
  }
  function e() {
    var h = $('#remove-thumbnail-error'),
    o = $('#addImageButton'),
    s = $('#thumbnailImageFile'),
    c = Roblox.IDE.validator({
      button: o,
      enabledClass: r,
      disabledClass: n
    }, [
      {
        input: s,
        errorSpan: $('#ImageUpload .field-validation-valid')
      }
    ], !1),
    e;
    u(),
    e = $('#ThumbnailDisplayContainer').data('place-id'),
    $('a[data-remove-url]').click(
      function () {
        var n = $(this),
        t = $('#ThumbnailDisplayContainer').data('remove-url'),
        i = {
          thumbnailid: n.data('thumbnailid'),
          id: e
        };
        return Roblox.GenericConfirmation.open({
          titleText: Roblox.IDE.Resources.DeletetitleText,
          bodyContent: Roblox.IDE.Resources.DeletebodyContent,
          acceptText: Roblox.IDE.Resources.DeleteacceptText,
          declineText: Roblox.IDE.Resources.CancelText,
          onAccept: function () {
            $.post(
              t,
              i,
              function (t) {
                t.success ? n.parent().remove() : h.addClass('field-validation-error').text(t.message)
              }
            )
          }
        }),
        !1
      }
    );
    $('#thumbnails').on(
      'click',
      'input[name=\'thumbnailType\']',
      function () {
        $(this).attr('id') === 'imageThumbnail' ? (
          $('#thumbnails #VideoUpload').hide(),
          $('#thumbnails #autogenerate').hide(),
          $('#thumbnails #ImageUpload').show()
        ) : $(this).attr('id') === 'youtubeThumbnail' ? (
          $('#thumbnails #ImageUpload').hide(),
          $('#thumbnails #autogenerate').hide(),
          $('#thumbnails #VideoUpload').show()
        ) : (
          $('#thumbnails #ImageUpload').hide(),
          $('#thumbnails #VideoUpload').hide(),
          $('#thumbnails #autogenerate').show()
        )
      }
    );
    o.click(
      function () {
        function o() {
          var n = new FormData;
          n.append('thumbnailImageFile', s[0].files[0]),
          n.append('id', e),
          n.append(
            '__RequestVerificationToken',
            $('[name=__RequestVerificationToken]').val()
          ),
          u.show(),
          r.hide(),
          $.ajax({
            url: r.data('form-post-url'),
            type: 'POST',
            success: function (n) {
              t(n),
              u.hide(),
              r.show()
            },
            error: function () {
              u.hide(),
              r.show()
            },
            data: n,
            cache: !1,
            contentType: !1,
            processData: !1
          })
        }
        var r = $(this),
        u,
        f,
        h;
        r.hasClass(n) ||
        (
          u = $('#LoadingImage'),
          f = $('input[name=thumbnailType]:checked'),
          f &&
          (
            h = f.attr('data-cost-in-robux'),
            h ? i(o) : Roblox.GenericConfirmation.open({
              titleText: Roblox.IDE.Resources.AddtitleText,
              bodyContent: Roblox.IDE.Resources.AddGeneratedThumbBodyText,
              acceptText: Roblox.IDE.Resources.AddGeneratedThumbAcceptText,
              declineText: Roblox.IDE.Resources.AddGeneratedThumbCancelText,
              onAccept: o
            })
          )
        )
      }
    ),
    $('a#addGeneratedImageButton').click(
      function () {
        function u() {
          var n = new FormData;
          n.append('id', e),
          r.show(),
          i.hide(),
          $.ajax({
            url: i.data('form-post-url'),
            type: 'POST',
            success: function (n) {
              t(n),
              r.hide(),
              i.show()
            },
            error: function () {
              r.hide(),
              i.show()
            },
            data: n,
            cache: !1,
            contentType: !1,
            processData: !1
          })
        }
        var i = $(this),
        r = $('#LoadingImage');
        if (i.hasClass(n)) {
          event.preventDefault();
          return
        }
        Roblox.GenericConfirmation.open({
          titleText: Roblox.IDE.Resources.AddGeneratedThumbTitleText,
          bodyContent: Roblox.IDE.Resources.AddGeneratedThumbBodyText,
          acceptText: Roblox.IDE.Resources.AddGeneratedThumbAcceptText,
          declineText: Roblox.IDE.Resources.AddGeneratedThumbCancelText,
          onAccept: u
        })
      }
    ),
    $('a#addVideoButton').click(
      function () {
        function o() {
          r.show(),
          n.hide(),
          $.post(
            n.data('form-post-url'),
            {
              id: e,
              thumbnailYoutubeUrl: u,
              __RequestVerificationToken: f
            },
            function (i) {
              t(i),
              r.hide(),
              n.show()
            }
          )
        }
        var n = $(this);
        if (!n.hasClass('btn-disabled-neutral')) {
          var r = $('#LoadingImage'),
          u = $('#txtYouTubeVideoUrl').val(),
          f = $('[name=__RequestVerificationToken]').val();
          i(o)
        }
      }
    );
    $('#txtYouTubeVideoUrl').on(
      'input',
      function () {
        var n = $(this).val().length > 0;
        f($('a#addVideoButton'), n)
      }
    );
    c.init()
  }
  var r = 'btn-neutral',
  n = 'btn-disabled-neutral';
  return {
    init: e
  }
}(),
$(
  function () {
    Roblox.IDE.Thumbnails.init(),
    Roblox.IDE.ThumbnailGallery.init()
  }
);
; // IDE/Icon.js
Roblox = Roblox ||
{
},
Roblox.Icons = Roblox.Icons ||
{
},
Roblox.Resources = Roblox.Resources ||
{
},
Roblox.Resources.Icons = {
  AddtitleText: 'Add Icon',
  AddbodyContent: 'Are you sure you want to add this icon? This will delete your existing icon.',
  AddacceptText: 'Upload Now',
  CancelText: 'Cancel',
  DeletetitleText: 'Delete Icon',
  DeletebodyContent: 'Are you sure you want to permanently delete this icon?',
  DeleteText: 'Delete',
  ErrorText: 'An unknown error occurred.',
  EmptyText: 'Please choose a file.',
  AddGeneratedThumbTitleText: 'Auto Generated Icon',
  AddGeneratedThumbBodyText: 'This icon will be moderated. Are you sure you want to submit this icon?',
  AddGeneratedThumbAcceptText: 'Submit',
  AddGeneratedThumbCancelText: 'Cancel'
},
Roblox.Icons.Init = function () {
  function e(n) {
    t.html(n),
    document.getElementById('IconScript') === null &&
    $(function () {
      Roblox.Icons.Init()
    })
  }
  function h() {
    var n = $('#addGeneratedIconImageButton'),
    t;
    n.addClass(f),
    t = setInterval(
      function () {
        var i = $('#autogenerate span').attr('data-retry-url');
        i ||
        (n.removeClass(f), clearInterval(t))
      },
      s
    )
  }
  h();
  var r = $('#IconDisplayContainer'),
  u = r.data('place-id'),
  o = r.data('place-icon-id'),
  z = r.data('random-icon'),
  i = $('#UploadStatus'),
  n = $('#LoadingImage'),
  f = 'btn-disabled-neutral',
  t = $('#icons'),
  s = 300;
  i.html() ||
  i.hide(),
  $('a[data-remove-icon-url]').click(
    function () {
      var i = $(this),
      n = r.data('remove-icon-url'),
      t = {
        placeId: u,
        placeIconId: o
      };
      return Roblox.GenericConfirmation.open({
        titleText: Roblox.Resources.Icons.DeletetitleText,
        bodyContent: Roblox.Resources.Icons.DeletebodyContent,
        acceptText: Roblox.Resources.Icons.DeleteacceptText,
        declineText: Roblox.Resources.Icons.CancelText,
        onAccept: function () {
          $.post(n, t, function (n) {
            e(n)
          })
        }
      }),
      !1
    }
  );
  t.on(
    'click',
    'input[name=\'iconType\']',
    function () {
      $(this).attr('id') === 'imageIcon' ? (t.find('#autogenerate').hide(), t.find('#ImageUpload').show()) : (t.find('#ImageUpload').hide(), t.find('#autogenerate').show())
    }
  );
  $('a#addIconButton').click(
    function () {
      var t = $(this);
      $('#iconImageFile').val() ? Roblox.GenericConfirmation.open({
        titleText: Roblox.Resources.Icons.AddtitleText,
        bodyContent: Roblox.Resources.Icons.AddbodyContent,
        acceptText: Roblox.Resources.Icons.AddacceptText,
        declineText: Roblox.Resources.Icons.CancelText,
        allowHtmlContentInBody: !0,
        acceptColor: Roblox.GenericConfirmation.green,
        onAccept: function () {
          var r = new FormData;
            r.append('iconImageFile', $('#iconImageFile') [0].files[0]),
            r.append('placeId', u),
            n.show(),
            t.hide(),
            $.ajax({
            url: t.data('form-post-url'),
            type: 'POST',
            success: function (i) {
              e(i),
              n.hide(),
              t.show(),
              window.location.reload()
            },
            error: function (jqXHR, textStatus, errorThrown) {
              n.hide();
              t.show();
              var errorMessage = Roblox.Resources.Icons.ErrorText;
              if (jqXHR.responseJSON && jqXHR.responseJSON.error) {
              errorMessage = jqXHR.responseJSON.error;
              }
              i.html(errorMessage).removeClass('status-confirm').addClass('error-message').show()
            },
            data: r,
            cache: !1,
            contentType: !1,
            processData: !1
            })
        }
      }) : i.html(Roblox.Resources.Icons.EmptyText).removeClass('status-confirm').addClass('error-message').show()
    }
  ),
  $('a#addGeneratedIconImageButton').click(
    function () {
      function r() {
        var r = new FormData;
        r.append('placeId', u),
		r.append('random-icon', z),
        n.show(),
        t.hide(),
        $.ajax({
          url: t.data('form-post-url'),
          type: 'POST',
          success: function (i) {
            e(i),
            n.hide(),
            t.show()
          },
          error: function () {
            n.hide(),
            t.show(),
            i.html(Roblox.Resources.Icons.ErrorText).removeClass('status-confirm').addClass('error-message').show()
          },
          data: r,
          cache: !1,
          contentType: !1,
          processData: !1
        })
      }
      var t = $(this);
      if (t.hasClass(f)) {
        event.preventDefault();
        return
      }
      Roblox.GenericConfirmation.open({
        titleText: Roblox.Resources.Icons.AddGeneratedThumbTitleText,
        bodyContent: Roblox.Resources.Icons.AddGeneratedThumbBodyText,
        acceptText: Roblox.Resources.Icons.AddGeneratedThumbAcceptText,
        declineText: Roblox.Resources.Icons.AddGeneratedThumbCancelText,
        onAccept: r
      })
    }
  )
},
$(function () {
  Roblox.Icons.Init()
});
; // IDE/validator.js
var Roblox = Roblox ||
{
};
Roblox.IDE = Roblox.IDE ||
{
},
Roblox.IDE.validator = function () {
  function r(n, t, i) {
    n.toggleClass('field-validation-valid', !t),
    n.toggleClass('field-validation-error', !!t),
    t ? (
      n.text(t),
      i &&
      Roblox.StyleguideConversions.convertMvcErrorToStyleGuide()
    ) : n.html('').removeClass('tool-tip')
  }
  function u(r) {
    var f,
    e,
    u,
    o;
    if (r.attr('type') === 'file') {
      if (r[0].files.length <= 0) return 'You must select a file.';
      if (r.attr('accept')) for (
        f = r.attr('accept').split('/') [0],
        e = new RegExp(r.attr('accept').replace('*', '.*')),
        u = 0;
        u < r[0].files.length;
        u++
      ) if (!e.test(r[0].files[u].type)) return 'File type must be ' + f
    } else {
      if (r.data(n) && !r.val().trim()) return r.data(n);
      if (r.data(t) && (o = new RegExp(r.data(t)), !o.test(r.val()))) return r.data(i)
    }
    return ''
  }
  var n = 'val-required',
  t = 'val-regex-pattern',
  i = 'val-regex';
  return function (n, t, i) {
    function f(f) {
      var e = !0;
      return t.forEach(
        function (n) {
          var t = u(n.input);
          t &&
          (e = !1),
          (f || !t) &&
          r(n.errorSpan, t, i)
        }
      ),
      e ? n.button.removeAttr('disabled') : n.button.attr('disabled', 'disabled'),
      n.button.toggleClass(n.enabledClass, e).toggleClass(n.disabledClass, !e).prop('disabled', !e),
      e
    }
    function e() {
      t.forEach(
        function (n) {
          if (n.input.attr('type') === 'file') n.input.change(function () {
            f(!0)
          });
           else n.input.on('blur keyup', function () {
            f(!0)
          })
        }
      ),
      f(!1)
    }
    return {
      init: e,
      validateInputs: f
    }
  }
}();
; // IDE/Update.js
var Roblox = Roblox ||
{
};
Roblox.IDE = Roblox.IDE ||
{
},
Roblox.IDE.Update = function () {
  function i() {
    return $('#navbar').data('is-update')
  }
  function f(n) {
    Roblox.Dialog.open({
      titleText: Roblox.IDE.Resources.AllowCopyingTitleText,
      bodyContent: Roblox.IDE.Resources.AllowCopyingTitleContent,
      acceptText: Roblox.IDE.Resources.AllowCopyingAcceptText,
      declineText: Roblox.IDE.Resources.AllowCopyingCancelText,
      imageUrl: '/img/Icons/img-alert.png',
      onAccept: n
    })
  }
  function e(n) {
    Roblox.Dialog.open({
      titleText: Roblox.IDE.Resources.DisableVIPServersWarningTitleText,
      bodyContent: Roblox.IDE.Resources.DisableVIPServersWarningBodyContent,
      acceptText: Roblox.IDE.Resources.DisableVIPServersWarningAcceptText,
      declineText: Roblox.IDE.Resources.DisableVIPServersWarningCancelText,
      imageUrl: '/img/Icons/img-alert.png',
      onAccept: n
    })
  }
  function r() {
    var n = $('#okButton');
    $('form#configureUniverseForm').submit(),
    n.addClass(t),
    n.prop('disabled', !0),
    o()
  }
  function n(n) {
    $('.creator-dashboard-redirection-banner').hide(),
    $('#playerAccess').hide(),
    $('#permissions').hide(),
    $('#basicSettings').hide(),
    $('#icons').hide(),
    $('#thumbnails').hide(),
    $('#versionHistory').hide(),
    $('#developerProducts').hide(),
    $('#universe').hide(),
    $('#navbar div.selected').removeClass('selected'),
    $('div.actionButtons').show();
    var t = n.attr('id');
    $('#' + t + '-redirection-banner').show(),
    n.addClass('selected')
  }
  function o(n) {
    var t = {
      overlayClose: !1,
      opacity: 80,
      overlayCss: {
        backgroundColor: '#000'
      },
      escClose: !1
    };
    typeof n != 'undefined' &&
    n !== '' &&
    $.modal.close('.' + n),
    $('#ProcessingView').modal(t)
  }
  function s() {
    function w() {
      var n = h.prop('checked');
      return !a &&
      n
    }
    function b() {
      var n = c.prop('checked');
      return v &&
      !n
    }
    function l() {
      return i &&
      b() ? e(function () {
        r()
      }) : r(),
      !1
    }
    function k() {
      return o.hasClass(t) ? !1 : i &&
      w() ? (f(function () {
        l()
      }), !1) : l()
    }
    function d() {
      var t = typeof localStorage != 'undefined' &&
      Roblox.LocalStorage.isAvailable(),
      n = 'Roblox.IDE.Update.versionHistoryMessageShown';
      t &&
      !localStorage.getItem(n) &&
      (
        localStorage.setItem(n, '1'),
        Roblox.Dialog.open({
          titleText: Roblox.IDE.Resources.VersionHistoryBehaviorTitleText,
          bodyContent: Roblox.IDE.Resources.VersionHistoryBehaviorBodyText,
          acceptText: Roblox.IDE.Resources.OkText,
          showAccept: !0,
          showDecline: !1
        })
      )
    }
    function s(n) {
      var t = $('#versionHistoryItems').data('asset-id');
      $('#versionHistoryItems').html(''),
      $('#versionHistoryLoading').show(),
      $.ajax({
        url: $('#versionHistoryItems').data('version-history-items-url'),
        cache: !1,
        data: {
          assetID: t,
          page: n
        },
        success: function (n) {
          $('#versionHistoryLoading').hide(),
          $('#versionHistoryItems').html(n)
        },
        error: function () {
          $('#versionHistoryLoading').hide(),
          $('#versionHistoryError').show()
        }
      })
    }
    var h = $('#copyLock input'),
    a = h.prop('checked'),
    c = $('#AllowPrivateServersCheckbox'),
    v = c.prop('checked'),
    y = $('input#Name'),
    o = $('#okButton'),
    p = Roblox.IDE.validator({
      button: o,
      enabledClass: u,
      disabledClass: t
    }, [
      {
        input: y,
        errorSpan: $('#nameRow span')
      }
    ], !0);
    o.click(k),
    $(document).ready(function () {
      $('#basicSettingsTab').click()
    }),
    $('#cancelButton').click(function () {
      document.location = $(this).attr('href')
    }),
    $('#permissionsTab').click(function () {
      n($(this)),
      $('#permissions').show()
    }),
    $('#playerAccessTab').click(
      function () {
        var t = $(this).data('cd-redirection-link');
        t &&
        (window.location.href = t),
        n($(this)),
        $('#playerAccess').show(),
        $('#GamePlaceAccess').is(':visible') &&
        (
          Roblox.PlayerAccess.initializeChosen(),
          Roblox.PlayerAccess.checkSaleOptions()
        )
      }
    ),
    $('#basicSettingsTab').click(function () {
      n($(this)),
      $('#basicSettings').show()
    }),
    $('#iconsTab').click(
      function () {
        var t = $(this).data('cd-redirection-link');
        t &&
        (window.location.href = t),
        n($(this)),
        $('#icons').show()
      }
    ),
    $('#thumbnailTab').click(
      function () {
        var t = $(this).data('cd-redirection-link');
        t &&
        (window.location.href = t),
        n($(this)),
        $('#thumbnails').show()
      }
    ),
    $('#versionHistoryTab').click(
      function () {
        n($(this)),
        $('#versionHistory').show(),
        $('#versionHistoryItems').data('show-popup') === !0 &&
        d()
      }
    ),
    $('#developerProductsTab').click(
      function () {
        n($(this));
        var t = $('#developerProducts');
        t.show(),
        t.attr('loaded') ? t.trigger('onRefreshed', []) : t.attr('loaded', !0),
        t.show(),
        $('div.actionButtons').show();
        t.off('onViewChange').on(
          'onViewChange',
          function (n, t) {
            t === 'listing' ? $('div.actionButtons').show() : $('div.actionButtons').hide()
          }
        )
      }
    ),
    $('#universeTab').click(function () {
      n($(this)),
      $('#universe').show()
    }),
    p.init(),
    $('div.validation-summary-errors').attr('data-valmsg-summary') === 'true' &&
    (
      $('#basicSettings').show(),
      $('#playerAccess').hide(),
      $('#permissions').hide(),
      $('#navbar div.selected').removeClass('selected'),
      $('#basicSettingsTab').addClass('selected')
    );
    $('#versionHistoryItems').on(
      'click',
      '.previous',
      function () {
        if (this.className.indexOf('disabled') >= 0) return !1;
        s(parseInt($('.robloxVersionHistoryPageNum').text()) - 1)
      }
    );
    $('#versionHistoryItems').on(
      'click',
      '.next',
      function () {
        if (this.className.indexOf('disabled') >= 0) return !1;
        s(parseInt($('.robloxVersionHistoryPageNum').text()) + 1)
      }
    );
    $('#versionHistoryItems').on(
      'click',
      '.revertLink',
      function () {
        var t = $(this).data('asset-version-id');
        Roblox.Dialog.open({
          titleText: Roblox.IDE.Resources.RevertTitleText,
          bodyContent: Roblox.IDE.Resources.RevertBodyContent,
          acceptText: Roblox.IDE.Resources.RevertAcceptText,
          declineText: Roblox.IDE.Resources.CancelText,
          onAccept: function () {
            $('#versionHistoryItems').html(''),
            $('#versionHistoryLoading').show(),
            $.ajax({
              url: $('#versionHistoryItems').data('revert-url'),
              type: 'POST',
              cache: !1,
              data: {
                assetVersionID: t
              },
              success: function () {
                s(1)
              },
              error: function () {
                $('#versionHistoryLoading').hide(),
                $('#versionHistoryRevertError').show()
              }
            })
          }
        })
      }
    ),
    $('#versionHistoryItems').on(
      'click',
      '.downloadLink',
      function (e) {
        e.preventDefault();
        var $ln = $(this);
        var url = $ln.data('download-url') ||
        $ln.attr('href');
        if (!url) {
          $('#versionHistoryDownloadError').show();
          return false;
        }
        fetch(url, {
          credentials: 'same-origin'
        }).then(
          function (response) {
            if (response.status === 200) {
              return response.blob().then(
                function (blob) {
                  var disposition = response.headers.get('Content-Disposition') ||
                  '';
                  var filename = 'download';
                  var m = /filename\\*=UTF-8''([^;]+)|filename="?([^";]+)"?/.exec(disposition);
                  if (m) {
                    filename = decodeURIComponent(m[1] || m[2] || filename);
                  }
                  var blobUrl = window.URL.createObjectURL(blob);
                  var a = document.createElement('a');
                  a.href = blobUrl;
                  a.download = filename;
                  document.body.appendChild(a);
                  a.click();
                  a.remove();
                  window.URL.revokeObjectURL(blobUrl);
                }
              );
            } else {
              $('#versionHistoryDownloadError').show();
            }
          }
        ).catch(function () {
          $('#versionHistoryDownloadError').show();
        });
        return false;
      }
    );
  }
  var u = 'btn-neutral',
  t = 'btn-disabled-neutral';
  return $(s),
  {
  }
}();
; // jquery/jquery-ui-sortable-1.10.4.min.js
/*! jQuery UI - v1.10.4 - 2017-09-26
* http://jqueryui.com
* Includes: jquery.ui.core.js, jquery.ui.widget.js, jquery.ui.mouse.js, jquery.ui.sortable.js
* Copyright jQuery Foundation and other contributors; Licensed MIT */
(
  function (t, e) {
    function i(e, i) {
      var n,
      o,
      a,
      r = e.nodeName.toLowerCase();
      return 'area' === r ? (
        n = e.parentNode,
        o = n.name,
        e.href &&
        o &&
        'map' === n.nodeName.toLowerCase() ? (a = t('img[usemap=#' + o + ']') [0], !!a && s(a)) : !1
      ) : (
        /input|select|textarea|button|object/.test(r) ? !e.disabled : 'a' === r ? e.href ||
        i : i
      ) &&
      s(e)
    }
    function s(e) {
      return t.expr.filters.visible(e) &&
      !t(e).parents().addBack().filter(function () {
        return 'hidden' === t.css(this, 'visibility')
      }).length
    }
    var n = 0,
    o = /^ui-id-\d+$/;
    t.ui = t.ui ||
    {
    },
    t.extend(
      t.ui,
      {
        version: '1.10.4',
        keyCode: {
          BACKSPACE: 8,
          COMMA: 188,
          DELETE: 46,
          DOWN: 40,
          END: 35,
          ENTER: 13,
          ESCAPE: 27,
          HOME: 36,
          LEFT: 37,
          NUMPAD_ADD: 107,
          NUMPAD_DECIMAL: 110,
          NUMPAD_DIVIDE: 111,
          NUMPAD_ENTER: 108,
          NUMPAD_MULTIPLY: 106,
          NUMPAD_SUBTRACT: 109,
          PAGE_DOWN: 34,
          PAGE_UP: 33,
          PERIOD: 190,
          RIGHT: 39,
          SPACE: 32,
          TAB: 9,
          UP: 38
        }
      }
    ),
    t.fn.extend({
      focus: function (e) {
        return function (i, s) {
          return 'number' == typeof i ? this.each(
            function () {
              var e = this;
              setTimeout(function () {
                t(e).focus(),
                s &&
                s.call(e)
              }, i)
            }
          ) : e.apply(this, arguments)
        }
      }(t.fn.focus),
      scrollParent: function () {
        var e;
        return e = t.ui.ie &&
        /(static|relative)/.test(this.css('position')) ||
        /absolute/.test(this.css('position')) ? this.parents().filter(
          function () {
            return /(relative|absolute|fixed)/.test(t.css(this, 'position')) &&
            /(auto|scroll)/.test(
              t.css(this, 'overflow') + t.css(this, 'overflow-y') + t.css(this, 'overflow-x')
            )
          }
        ).eq(0) : this.parents().filter(
          function () {
            return /(auto|scroll)/.test(
              t.css(this, 'overflow') + t.css(this, 'overflow-y') + t.css(this, 'overflow-x')
            )
          }
        ).eq(0),
        /fixed/.test(this.css('position')) ||
        !e.length ? t(document) : e
      },
      zIndex: function (i) {
        if (i !== e) return this.css('zIndex', i);
        if (this.length) for (var s, n, o = t(this[0]); o.length && o[0] !== document; ) {
          if (
            s = o.css('position'),
            ('absolute' === s || 'relative' === s || 'fixed' === s) &&
            (n = parseInt(o.css('zIndex'), 10), !isNaN(n) && 0 !== n)
          ) return n;
          o = o.parent()
        }
        return 0
      },
      uniqueId: function () {
        return this.each(function () {
          this.id ||
          (this.id = 'ui-id-' + ++n)
        })
      },
      removeUniqueId: function () {
        return this.each(function () {
          o.test(this.id) &&
          t(this).removeAttr('id')
        })
      }
    }),
    t.extend(
      t.expr[':'],
      {
        data: t.expr.createPseudo ? t.expr.createPseudo(function (e) {
          return function (i) {
            return !!t.data(i, e)
          }
        }) : function (e, i, s) {
          return !!t.data(e, s[3])
        },
        focusable: function (e) {
          return i(e, !isNaN(t.attr(e, 'tabindex')))
        },
        tabbable: function (e) {
          var s = t.attr(e, 'tabindex'),
          n = isNaN(s);
          return (n || s >= 0) &&
          i(e, !n)
        }
      }
    ),
    t('<a>').outerWidth(1).jquery ||
    t.each(
      ['Width',
      'Height'],
      function (i, s) {
        function n(e, i, s, n) {
          return t.each(
            o,
            function () {
              i -= parseFloat(t.css(e, 'padding' + this)) ||
              0,
              s &&
              (i -= parseFloat(t.css(e, 'border' + this + 'Width')) || 0),
              n &&
              (i -= parseFloat(t.css(e, 'margin' + this)) || 0)
            }
          ),
          i
        }
        var o = 'Width' === s ? [
          'Left',
          'Right'
        ] : [
          'Top',
          'Bottom'
        ],
        a = s.toLowerCase(),
        r = {
          innerWidth: t.fn.innerWidth,
          innerHeight: t.fn.innerHeight,
          outerWidth: t.fn.outerWidth,
          outerHeight: t.fn.outerHeight
        };
        t.fn['inner' + s] = function (i) {
          return i === e ? r['inner' + s].call(this) : this.each(function () {
            t(this).css(a, n(this, i) + 'px')
          })
        },
        t.fn['outer' + s] = function (e, i) {
          return 'number' != typeof e ? r['outer' + s].call(this, e) : this.each(function () {
            t(this).css(a, n(this, e, !0, i) + 'px')
          })
        }
      }
    ),
    t.fn.addBack ||
    (
      t.fn.addBack = function (t) {
        return this.add(null == t ? this.prevObject : this.prevObject.filter(t))
      }
    ),
    t('<a>').data('a-b', 'a').removeData('a-b').data('a-b') &&
    (
      t.fn.removeData = function (e) {
        return function (i) {
          return arguments.length ? e.call(this, t.camelCase(i)) : e.call(this)
        }
      }(t.fn.removeData)
    ),
    t.ui.ie = !!/msie [\w.]+/.exec(navigator.userAgent.toLowerCase()),
    t.support.selectstart = 'onselectstart' in document.createElement('div'),
    t.fn.extend({
      disableSelection: function () {
        return this.bind(
          (t.support.selectstart ? 'selectstart' : 'mousedown') + '.ui-disableSelection',
          function (t) {
            t.preventDefault()
          }
        )
      },
      enableSelection: function () {
        return this.unbind('.ui-disableSelection')
      }
    }),
    t.extend(
      t.ui,
      {
        plugin: {
          add: function (e, i, s) {
            var n,
            o = t.ui[e].prototype;
            for (n in s) o.plugins[n] = o.plugins[n] ||
            [],
            o.plugins[n].push([i,
            s[n]])
          },
          call: function (t, e, i) {
            var s,
            n = t.plugins[e];
            if (
              n &&
              t.element[0].parentNode &&
              11 !== t.element[0].parentNode.nodeType
            ) for (s = 0; n.length > s; s++) t.options[n[s][0]] &&
            n[s][1].apply(t.element, i)
          }
        },
        hasScroll: function (e, i) {
          if ('hidden' === t(e).css('overflow')) return !1;
          var s = i &&
          'left' === i ? 'scrollLeft' : 'scrollTop',
          n = !1;
          return e[s] > 0 ? !0 : (e[s] = 1, n = e[s] > 0, e[s] = 0, n)
        }
      }
    )
  }
) (jQuery);
(
  function (t, e) {
    var i = 0,
    s = Array.prototype.slice,
    n = t.cleanData;
    t.cleanData = function (e) {
      for (var i, s = 0; null != (i = e[s]); s++) try {
        t(i).triggerHandler('remove')
      } catch (o) {
      }
      n(e)
    },
    t.widget = function (i, s, n) {
      var o,
      a,
      r,
      h,
      l = {},
      c = i.split('.') [0];
      i = i.split('.') [1],
      o = c + '-' + i,
      n ||
      (n = s, s = t.Widget),
      t.expr[':'][o.toLowerCase()] = function (e) {
        return !!t.data(e, o)
      },
      t[c] = t[c] ||
      {
      },
      a = t[c][i],
      r = t[c][i] = function (t, i) {
        return this._createWidget ? (arguments.length && this._createWidget(t, i), e) : new r(t, i)
      },
      t.extend(
        r,
        a,
        {
          version: n.version,
          _proto: t.extend({
          }, n),
          _childConstructors: []
        }
      ),
      h = new s,
      h.options = t.widget.extend({
      }, h.options),
      t.each(
        n,
        function (i, n) {
          return t.isFunction(n) ? (
            l[i] = function () {
              var t = function () {
                return s.prototype[i].apply(this, arguments)
              },
              e = function (t) {
                return s.prototype[i].apply(this, t)
              };
              return function () {
                var i,
                s = this._super,
                o = this._superApply;
                return this._super = t,
                this._superApply = e,
                i = n.apply(this, arguments),
                this._super = s,
                this._superApply = o,
                i
              }
            }(),
            e
          ) : (l[i] = n, e)
        }
      ),
      r.prototype = t.widget.extend(
        h,
        {
          widgetEventPrefix: a ? h.widgetEventPrefix ||
          i : i
        },
        l,
        {
          constructor: r,
          namespace: c,
          widgetName: i,
          widgetFullName: o
        }
      ),
      a ? (
        t.each(
          a._childConstructors,
          function (e, i) {
            var s = i.prototype;
            t.widget(s.namespace + '.' + s.widgetName, r, i._proto)
          }
        ),
        delete a._childConstructors
      ) : s._childConstructors.push(r),
      t.widget.bridge(i, r)
    },
    t.widget.extend = function (i) {
      for (var n, o, a = s.call(arguments, 1), r = 0, h = a.length; h > r; r++) for (n in a[r]) o = a[r][n],
      a[r].hasOwnProperty(n) &&
      o !== e &&
      (
        i[n] = t.isPlainObject(o) ? t.isPlainObject(i[n]) ? t.widget.extend({
        }, i[n], o) : t.widget.extend({
        }, o) : o
      );
      return i
    },
    t.widget.bridge = function (i, n) {
      var o = n.prototype.widgetFullName ||
      i;
      t.fn[i] = function (a) {
        var r = 'string' == typeof a,
        h = s.call(arguments, 1),
        l = this;
        return a = !r &&
        h.length ? t.widget.extend.apply(null, [
          a
        ].concat(h)) : a,
        r ? this.each(
          function () {
            var s,
            n = t.data(this, o);
            return n ? t.isFunction(n[a]) &&
            '_' !== a.charAt(0) ? (
              s = n[a].apply(n, h),
              s !== n &&
              s !== e ? (l = s && s.jquery ? l.pushStack(s.get()) : s, !1) : e
            ) : t.error('no such method \'' + a + '\' for ' + i + ' widget instance') : t.error(
              'cannot call methods on ' + i + ' prior to initialization; ' + 'attempted to call method \'' + a + '\''
            )
          }
        ) : this.each(
          function () {
            var e = t.data(this, o);
            e ? e.option(a || {
            })._init() : t.data(this, o, new n(a, this))
          }
        ),
        l
      }
    },
    t.Widget = function () {
    },
    t.Widget._childConstructors = [],
    t.Widget.prototype = {
      widgetName: 'widget',
      widgetEventPrefix: '',
      defaultElement: '<div>',
      options: {
        disabled: !1,
        create: null
      },
      _createWidget: function (e, s) {
        s = t(s || this.defaultElement || this) [0],
        this.element = t(s),
        this.uuid = i++,
        this.eventNamespace = '.' + this.widgetName + this.uuid,
        this.options = t.widget.extend({
        }, this.options, this._getCreateOptions(), e),
        this.bindings = t(),
        this.hoverable = t(),
        this.focusable = t(),
        s !== this &&
        (
          t.data(s, this.widgetFullName, this),
          this._on(
            !0,
            this.element,
            {
              remove: function (t) {
                t.target === s &&
                this.destroy()
              }
            }
          ),
          this.document = t(s.style ? s.ownerDocument : s.document || s),
          this.window = t(this.document[0].defaultView || this.document[0].parentWindow)
        ),
        this._create(),
        this._trigger('create', null, this._getCreateEventData()),
        this._init()
      },
      _getCreateOptions: t.noop,
      _getCreateEventData: t.noop,
      _create: t.noop,
      _init: t.noop,
      destroy: function () {
        this._destroy(),
        this.element.unbind(this.eventNamespace).removeData(this.widgetName).removeData(this.widgetFullName).removeData(t.camelCase(this.widgetFullName)),
        this.widget().unbind(this.eventNamespace).removeAttr('aria-disabled').removeClass(this.widgetFullName + '-disabled ' + 'ui-state-disabled'),
        this.bindings.unbind(this.eventNamespace),
        this.hoverable.removeClass('ui-state-hover'),
        this.focusable.removeClass('ui-state-focus')
      },
      _destroy: t.noop,
      widget: function () {
        return this.element
      },
      option: function (i, s) {
        var n,
        o,
        a,
        r = i;
        if (0 === arguments.length) return t.widget.extend({
        }, this.options);
        if ('string' == typeof i) if (r = {}, n = i.split('.'), i = n.shift(), n.length) {
          for (
            o = r[i] = t.widget.extend({
            }, this.options[i]),
            a = 0;
            n.length - 1 > a;
            a++
          ) o[n[a]] = o[n[a]] ||
          {
          },
          o = o[n[a]];
          if (i = n.pop(), 1 === arguments.length) return o[i] === e ? null : o[i];
          o[i] = s
        } else {
          if (1 === arguments.length) return this.options[i] === e ? null : this.options[i];
          r[i] = s
        }
        return this._setOptions(r),
        this
      },
      _setOptions: function (t) {
        var e;
        for (e in t) this._setOption(e, t[e]);
        return this
      },
      _setOption: function (t, e) {
        return this.options[t] = e,
        'disabled' === t &&
        (
          this.widget().toggleClass(this.widgetFullName + '-disabled ui-state-disabled', !!e).attr('aria-disabled', e),
          this.hoverable.removeClass('ui-state-hover'),
          this.focusable.removeClass('ui-state-focus')
        ),
        this
      },
      enable: function () {
        return this._setOption('disabled', !1)
      },
      disable: function () {
        return this._setOption('disabled', !0)
      },
      _on: function (i, s, n) {
        var o,
        a = this;
        'boolean' != typeof i &&
        (n = s, s = i, i = !1),
        n ? (s = o = t(s), this.bindings = this.bindings.add(s)) : (n = s, s = this.element, o = this.widget()),
        t.each(
          n,
          function (n, r) {
            function h() {
              return i ||
              a.options.disabled !== !0 &&
              !t(this).hasClass('ui-state-disabled') ? ('string' == typeof r ? a[r] : r).apply(a, arguments) : e
            }
            'string' != typeof r &&
            (h.guid = r.guid = r.guid || h.guid || t.guid++);
            var l = n.match(/^(\w+)\s*(.*)$/),
            c = l[1] + a.eventNamespace,
            u = l[2];
            u ? o.delegate(u, c, h) : s.bind(c, h)
          }
        )
      },
      _off: function (t, e) {
        e = (e || '').split(' ').join(this.eventNamespace + ' ') + this.eventNamespace,
        t.unbind(e).undelegate(e)
      },
      _delay: function (t, e) {
        function i() {
          return ('string' == typeof t ? s[t] : t).apply(s, arguments)
        }
        var s = this;
        return setTimeout(i, e || 0)
      },
      _hoverable: function (e) {
        this.hoverable = this.hoverable.add(e),
        this._on(
          e,
          {
            mouseenter: function (e) {
              t(e.currentTarget).addClass('ui-state-hover')
            },
            mouseleave: function (e) {
              t(e.currentTarget).removeClass('ui-state-hover')
            }
          }
        )
      },
      _focusable: function (e) {
        this.focusable = this.focusable.add(e),
        this._on(
          e,
          {
            focusin: function (e) {
              t(e.currentTarget).addClass('ui-state-focus')
            },
            focusout: function (e) {
              t(e.currentTarget).removeClass('ui-state-focus')
            }
          }
        )
      },
      _trigger: function (e, i, s) {
        var n,
        o,
        a = this.options[e];
        if (
          s = s ||
          {
          },
          i = t.Event(i),
          i.type = (e === this.widgetEventPrefix ? e : this.widgetEventPrefix + e).toLowerCase(),
          i.target = this.element[0],
          o = i.originalEvent
        ) for (n in o) n in i ||
        (i[n] = o[n]);
        return this.element.trigger(i, s),
        !(
          t.isFunction(a) &&
          a.apply(this.element[0], [
            i
          ].concat(s)) === !1 ||
          i.isDefaultPrevented()
        )
      }
    },
    t.each({
      show: 'fadeIn',
      hide: 'fadeOut'
    }, function (e, i) {
      t.Widget.prototype['_' + e] = function (s, n, o) {
        'string' == typeof n &&
        (n = {
          effect: n
        });
        var a,
        r = n ? n === !0 ||
        'number' == typeof n ? i : n.effect ||
        i : e;
        n = n ||
        {
        },
        'number' == typeof n &&
        (n = {
          duration: n
        }),
        a = !t.isEmptyObject(n),
        n.complete = o,
        n.delay &&
        s.delay(n.delay),
        a &&
        t.effects &&
        t.effects.effect[r] ? s[e](n) : r !== e &&
        s[r] ? s[r](n.duration, n.easing, o) : s.queue(function (i) {
          t(this) [e](),
          o &&
          o.call(s[0]),
          i()
        })
      }
    })
  }
) (jQuery);
(
  function (t) {
    var e = !1;
    t(document).mouseup(function () {
      e = !1
    }),
    t.widget(
      'ui.mouse',
      {
        version: '1.10.4',
        options: {
          cancel: 'input,textarea,button,select,option',
          distance: 1,
          delay: 0
        },
        _mouseInit: function () {
          var e = this;
          this.element.bind(
            'mousedown.' + this.widgetName,
            function (t) {
              return e._mouseDown(t)
            }
          ).bind(
            'click.' + this.widgetName,
            function (i) {
              return !0 === t.data(i.target, e.widgetName + '.preventClickEvent') ? (
                t.removeData(i.target, e.widgetName + '.preventClickEvent'),
                i.stopImmediatePropagation(),
                !1
              ) : undefined
            }
          ),
          this.started = !1
        },
        _mouseDestroy: function () {
          this.element.unbind('.' + this.widgetName),
          this._mouseMoveDelegate &&
          t(document).unbind('mousemove.' + this.widgetName, this._mouseMoveDelegate).unbind('mouseup.' + this.widgetName, this._mouseUpDelegate)
        },
        _mouseDown: function (i) {
          if (!e) {
            this._mouseStarted &&
            this._mouseUp(i),
            this._mouseDownEvent = i;
            var s = this,
            n = 1 === i.which,
            o = 'string' == typeof this.options.cancel &&
            i.target.nodeName ? t(i.target).closest(this.options.cancel).length : !1;
            return n &&
            !o &&
            this._mouseCapture(i) ? (
              this.mouseDelayMet = !this.options.delay,
              this.mouseDelayMet ||
              (
                this._mouseDelayTimer = setTimeout(function () {
                  s.mouseDelayMet = !0
                }, this.options.delay)
              ),
              this._mouseDistanceMet(i) &&
              this._mouseDelayMet(i) &&
              (
                this._mouseStarted = this._mouseStart(i) !== !1,
                !this._mouseStarted
              ) ? (i.preventDefault(), !0) : (
                !0 === t.data(i.target, this.widgetName + '.preventClickEvent') &&
                t.removeData(i.target, this.widgetName + '.preventClickEvent'),
                this._mouseMoveDelegate = function (t) {
                  return s._mouseMove(t)
                },
                this._mouseUpDelegate = function (t) {
                  return s._mouseUp(t)
                },
                t(document).bind('mousemove.' + this.widgetName, this._mouseMoveDelegate).bind('mouseup.' + this.widgetName, this._mouseUpDelegate),
                i.preventDefault(),
                e = !0,
                !0
              )
            ) : !0
          }
        },
        _mouseMove: function (e) {
          return t.ui.ie &&
          (!document.documentMode || 9 > document.documentMode) &&
          !e.button ? this._mouseUp(e) : this._mouseStarted ? (this._mouseDrag(e), e.preventDefault()) : (
            this._mouseDistanceMet(e) &&
            this._mouseDelayMet(e) &&
            (
              this._mouseStarted = this._mouseStart(this._mouseDownEvent, e) !== !1,
              this._mouseStarted ? this._mouseDrag(e) : this._mouseUp(e)
            ),
            !this._mouseStarted
          )
        },
        _mouseUp: function (e) {
          return t(document).unbind('mousemove.' + this.widgetName, this._mouseMoveDelegate).unbind('mouseup.' + this.widgetName, this._mouseUpDelegate),
          this._mouseStarted &&
          (
            this._mouseStarted = !1,
            e.target === this._mouseDownEvent.target &&
            t.data(e.target, this.widgetName + '.preventClickEvent', !0),
            this._mouseStop(e)
          ),
          !1
        },
        _mouseDistanceMet: function (t) {
          return Math.max(
            Math.abs(this._mouseDownEvent.pageX - t.pageX),
            Math.abs(this._mouseDownEvent.pageY - t.pageY)
          ) >= this.options.distance
        },
        _mouseDelayMet: function () {
          return this.mouseDelayMet
        },
        _mouseStart: function () {
        },
        _mouseDrag: function () {
        },
        _mouseStop: function () {
        },
        _mouseCapture: function () {
          return !0
        }
      }
    )
  }
) (jQuery);
(
  function (t) {
    function e(t, e, i) {
      return t > e &&
      e + i > t
    }
    function i(t) {
      return /left|right/.test(t.css('float')) ||
      /inline|table-cell/.test(t.css('display'))
    }
    t.widget(
      'ui.sortable',
      t.ui.mouse,
      {
        version: '1.10.4',
        widgetEventPrefix: 'sort',
        ready: !1,
        options: {
          appendTo: 'parent',
          axis: !1,
          connectWith: !1,
          containment: !1,
          cursor: 'auto',
          cursorAt: !1,
          dropOnEmpty: !0,
          forcePlaceholderSize: !1,
          forceHelperSize: !1,
          grid: !1,
          handle: !1,
          helper: 'original',
          items: '> *',
          opacity: !1,
          placeholder: !1,
          revert: !1,
          scroll: !0,
          scrollSensitivity: 20,
          scrollSpeed: 20,
          scope: 'default',
          tolerance: 'intersect',
          zIndex: 1000,
          activate: null,
          beforeStop: null,
          change: null,
          deactivate: null,
          out: null,
          over: null,
          receive: null,
          remove: null,
          sort: null,
          start: null,
          stop: null,
          update: null
        },
        _create: function () {
          var t = this.options;
          this.containerCache = {},
          this.element.addClass('ui-sortable'),
          this.refresh(),
          this.floating = this.items.length ? 'x' === t.axis ||
          i(this.items[0].item) : !1,
          this.offset = this.element.offset(),
          this._mouseInit(),
          this.ready = !0
        },
        _destroy: function () {
          this.element.removeClass('ui-sortable ui-sortable-disabled'),
          this._mouseDestroy();
          for (var t = this.items.length - 1; t >= 0; t--) this.items[t].item.removeData(this.widgetName + '-item');
          return this
        },
        _setOption: function (e, i) {
          'disabled' === e ? (
            this.options[e] = i,
            this.widget().toggleClass('ui-sortable-disabled', !!i)
          ) : t.Widget.prototype._setOption.apply(this, arguments)
        },
        _mouseCapture: function (e, i) {
          var s = null,
          n = !1,
          o = this;
          return this.reverting ? !1 : this.options.disabled ||
          'static' === this.options.type ? !1 : (
            this._refreshItems(e),
            t(e.target).parents().each(
              function () {
                return t.data(this, o.widgetName + '-item') === o ? (s = t(this), !1) : undefined
              }
            ),
            t.data(e.target, o.widgetName + '-item') === o &&
            (s = t(e.target)),
            s ? !this.options.handle ||
            i ||
            (
              t(this.options.handle, s).find('*').addBack().each(function () {
                this === e.target &&
                (n = !0)
              }),
              n
            ) ? (this.currentItem = s, this._removeCurrentsFromItems(), !0) : !1 : !1
          )
        },
        _mouseStart: function (e, i, s) {
          var n,
          o,
          a = this.options;
          if (
            this.currentContainer = this,
            this.refreshPositions(),
            this.helper = this._createHelper(e),
            this._cacheHelperProportions(),
            this._cacheMargins(),
            this.scrollParent = this.helper.scrollParent(),
            this.offset = this.currentItem.offset(),
            this.offset = {
              top: this.offset.top - this.margins.top,
              left: this.offset.left - this.margins.left
            },
            t.extend(
              this.offset,
              {
                click: {
                  left: e.pageX - this.offset.left,
                  top: e.pageY - this.offset.top
                },
                parent: this._getParentOffset(),
                relative: this._getRelativeOffset()
              }
            ),
            this.helper.css('position', 'absolute'),
            this.cssPosition = this.helper.css('position'),
            this.originalPosition = this._generatePosition(e),
            this.originalPageX = e.pageX,
            this.originalPageY = e.pageY,
            a.cursorAt &&
            this._adjustOffsetFromHelper(a.cursorAt),
            this.domPosition = {
              prev: this.currentItem.prev() [0],
              parent: this.currentItem.parent() [0]
            },
            this.helper[0] !== this.currentItem[0] &&
            this.currentItem.hide(),
            this._createPlaceholder(),
            a.containment &&
            this._setContainment(),
            a.cursor &&
            'auto' !== a.cursor &&
            (
              o = this.document.find('body'),
              this.storedCursor = o.css('cursor'),
              o.css('cursor', a.cursor),
              this.storedStylesheet = t('<style>*{ cursor: ' + a.cursor + ' !important; }</style>').appendTo(o)
            ),
            a.opacity &&
            (
              this.helper.css('opacity') &&
              (this._storedOpacity = this.helper.css('opacity')),
              this.helper.css('opacity', a.opacity)
            ),
            a.zIndex &&
            (
              this.helper.css('zIndex') &&
              (this._storedZIndex = this.helper.css('zIndex')),
              this.helper.css('zIndex', a.zIndex)
            ),
            this.scrollParent[0] !== document &&
            'HTML' !== this.scrollParent[0].tagName &&
            (this.overflowOffset = this.scrollParent.offset()),
            this._trigger('start', e, this._uiHash()),
            this._preserveHelperProportions ||
            this._cacheHelperProportions(),
            !s
          ) for (n = this.containers.length - 1; n >= 0; n--) this.containers[n]._trigger('activate', e, this._uiHash(this));
          return t.ui.ddmanager &&
          (t.ui.ddmanager.current = this),
          t.ui.ddmanager &&
          !a.dropBehaviour &&
          t.ui.ddmanager.prepareOffsets(this, e),
          this.dragging = !0,
          this.helper.addClass('ui-sortable-helper'),
          this._mouseDrag(e),
          !0
        },
        _mouseDrag: function (e) {
          var i,
          s,
          n,
          o,
          a = this.options,
          r = !1;
          for (
            this.position = this._generatePosition(e),
            this.positionAbs = this._convertPositionTo('absolute'),
            this.lastPositionAbs ||
            (this.lastPositionAbs = this.positionAbs),
            this.options.scroll &&
            (
              this.scrollParent[0] !== document &&
              'HTML' !== this.scrollParent[0].tagName ? (
                this.overflowOffset.top + this.scrollParent[0].offsetHeight - e.pageY < a.scrollSensitivity ? this.scrollParent[0].scrollTop = r = this.scrollParent[0].scrollTop + a.scrollSpeed : e.pageY - this.overflowOffset.top < a.scrollSensitivity &&
                (
                  this.scrollParent[0].scrollTop = r = this.scrollParent[0].scrollTop - a.scrollSpeed
                ),
                this.overflowOffset.left + this.scrollParent[0].offsetWidth - e.pageX < a.scrollSensitivity ? this.scrollParent[0].scrollLeft = r = this.scrollParent[0].scrollLeft + a.scrollSpeed : e.pageX - this.overflowOffset.left < a.scrollSensitivity &&
                (
                  this.scrollParent[0].scrollLeft = r = this.scrollParent[0].scrollLeft - a.scrollSpeed
                )
              ) : (
                e.pageY - t(document).scrollTop() < a.scrollSensitivity ? r = t(document).scrollTop(t(document).scrollTop() - a.scrollSpeed) : t(window).height() - (e.pageY - t(document).scrollTop()) < a.scrollSensitivity &&
                (
                  r = t(document).scrollTop(t(document).scrollTop() + a.scrollSpeed)
                ),
                e.pageX - t(document).scrollLeft() < a.scrollSensitivity ? r = t(document).scrollLeft(t(document).scrollLeft() - a.scrollSpeed) : t(window).width() - (e.pageX - t(document).scrollLeft()) < a.scrollSensitivity &&
                (
                  r = t(document).scrollLeft(t(document).scrollLeft() + a.scrollSpeed)
                )
              ),
              r !== !1 &&
              t.ui.ddmanager &&
              !a.dropBehaviour &&
              t.ui.ddmanager.prepareOffsets(this, e)
            ),
            this.positionAbs = this._convertPositionTo('absolute'),
            this.options.axis &&
            'y' === this.options.axis ||
            (this.helper[0].style.left = this.position.left + 'px'),
            this.options.axis &&
            'x' === this.options.axis ||
            (this.helper[0].style.top = this.position.top + 'px'),
            i = this.items.length - 1;
            i >= 0;
            i--
          ) if (
            s = this.items[i],
            n = s.item[0],
            o = this._intersectsWithPointer(s),
            o &&
            s.instance === this.currentContainer &&
            n !== this.currentItem[0] &&
            this.placeholder[1 === o ? 'next' : 'prev']() [0] !== n &&
            !t.contains(this.placeholder[0], n) &&
            (
              'semi-dynamic' === this.options.type ? !t.contains(this.element[0], n) : !0
            )
          ) {
            if (
              this.direction = 1 === o ? 'down' : 'up',
              'pointer' !== this.options.tolerance &&
              !this._intersectsWithSides(s)
            ) break;
            this._rearrange(e, s),
            this._trigger('change', e, this._uiHash());
            break
          }
          return this._contactContainers(e),
          t.ui.ddmanager &&
          t.ui.ddmanager.drag(this, e),
          this._trigger('sort', e, this._uiHash()),
          this.lastPositionAbs = this.positionAbs,
          !1
        },
        _mouseStop: function (e, i) {
          if (e) {
            if (
              t.ui.ddmanager &&
              !this.options.dropBehaviour &&
              t.ui.ddmanager.drop(this, e),
              this.options.revert
            ) {
              var s = this,
              n = this.placeholder.offset(),
              o = this.options.axis,
              a = {};
              o &&
              'x' !== o ||
              (
                a.left = n.left - this.offset.parent.left - this.margins.left + (
                  this.offsetParent[0] === document.body ? 0 : this.offsetParent[0].scrollLeft
                )
              ),
              o &&
              'y' !== o ||
              (
                a.top = n.top - this.offset.parent.top - this.margins.top + (
                  this.offsetParent[0] === document.body ? 0 : this.offsetParent[0].scrollTop
                )
              ),
              this.reverting = !0,
              t(this.helper).animate(
                a,
                parseInt(this.options.revert, 10) ||
                500,
                function () {
                  s._clear(e)
                }
              )
            } else this._clear(e, i);
            return !1
          }
        },
        cancel: function () {
          if (this.dragging) {
            this._mouseUp({
              target: null
            }),
            'original' === this.options.helper ? this.currentItem.css(this._storedCSS).removeClass('ui-sortable-helper') : this.currentItem.show();
            for (var e = this.containers.length - 1; e >= 0; e--) this.containers[e]._trigger('deactivate', null, this._uiHash(this)),
            this.containers[e].containerCache.over &&
            (
              this.containers[e]._trigger('out', null, this._uiHash(this)),
              this.containers[e].containerCache.over = 0
            )
          }
          return this.placeholder &&
          (
            this.placeholder[0].parentNode &&
            this.placeholder[0].parentNode.removeChild(this.placeholder[0]),
            'original' !== this.options.helper &&
            this.helper &&
            this.helper[0].parentNode &&
            this.helper.remove(),
            t.extend(
              this,
              {
                helper: null,
                dragging: !1,
                reverting: !1,
                _noFinalSort: null
              }
            ),
            this.domPosition.prev ? t(this.domPosition.prev).after(this.currentItem) : t(this.domPosition.parent).prepend(this.currentItem)
          ),
          this
        },
        serialize: function (e) {
          var i = this._getItemsAsjQuery(e && e.connected),
          s = [];
          return e = e ||
          {
          },
          t(i).each(
            function () {
              var i = (t(e.item || this).attr(e.attribute || 'id') || '').match(e.expression || /(.+)[\-=_](.+)/);
              i &&
              s.push((e.key || i[1] + '[]') + '=' + (e.key && e.expression ? i[1] : i[2]))
            }
          ),
          !s.length &&
          e.key &&
          s.push(e.key + '='),
          s.join('&')
        },
        toArray: function (e) {
          var i = this._getItemsAsjQuery(e && e.connected),
          s = [];
          return e = e ||
          {
          },
          i.each(
            function () {
              s.push(t(e.item || this).attr(e.attribute || 'id') || '')
            }
          ),
          s
        },
        _intersectsWith: function (t) {
          var e = this.positionAbs.left,
          i = e + this.helperProportions.width,
          s = this.positionAbs.top,
          n = s + this.helperProportions.height,
          o = t.left,
          a = o + t.width,
          r = t.top,
          h = r + t.height,
          l = this.offset.click.top,
          c = this.offset.click.left,
          u = 'x' === this.options.axis ||
          s + l > r &&
          h > s + l,
          d = 'y' === this.options.axis ||
          e + c > o &&
          a > e + c,
          p = u &&
          d;
          return 'pointer' === this.options.tolerance ||
          this.options.forcePointerForContainers ||
          'pointer' !== this.options.tolerance &&
          this.helperProportions[this.floating ? 'width' : 'height'] > t[this.floating ? 'width' : 'height'] ? p : e + this.helperProportions.width / 2 > o &&
          a > i - this.helperProportions.width / 2 &&
          s + this.helperProportions.height / 2 > r &&
          h > n - this.helperProportions.height / 2
        },
        _intersectsWithPointer: function (t) {
          var i = 'x' === this.options.axis ||
          e(this.positionAbs.top + this.offset.click.top, t.top, t.height),
          s = 'y' === this.options.axis ||
          e(this.positionAbs.left + this.offset.click.left, t.left, t.width),
          n = i &&
          s,
          o = this._getDragVerticalDirection(),
          a = this._getDragHorizontalDirection();
          return n ? this.floating ? a &&
          'right' === a ||
          'down' === o ? 2 : 1 : o &&
          ('down' === o ? 2 : 1) : !1
        },
        _intersectsWithSides: function (t) {
          var i = e(
            this.positionAbs.top + this.offset.click.top,
            t.top + t.height / 2,
            t.height
          ),
          s = e(
            this.positionAbs.left + this.offset.click.left,
            t.left + t.width / 2,
            t.width
          ),
          n = this._getDragVerticalDirection(),
          o = this._getDragHorizontalDirection();
          return this.floating &&
          o ? 'right' === o &&
          s ||
          'left' === o &&
          !s : n &&
          ('down' === n && i || 'up' === n && !i)
        },
        _getDragVerticalDirection: function () {
          var t = this.positionAbs.top - this.lastPositionAbs.top;
          return 0 !== t &&
          (t > 0 ? 'down' : 'up')
        },
        _getDragHorizontalDirection: function () {
          var t = this.positionAbs.left - this.lastPositionAbs.left;
          return 0 !== t &&
          (t > 0 ? 'right' : 'left')
        },
        refresh: function (t) {
          return this._refreshItems(t),
          this.refreshPositions(),
          this
        },
        _connectWith: function () {
          var t = this.options;
          return t.connectWith.constructor === String ? [
            t.connectWith
          ] : t.connectWith
        },
        _getItemsAsjQuery: function (e) {
          function i() {
            r.push(this)
          }
          var s,
          n,
          o,
          a,
          r = [],
          h = [],
          l = this._connectWith();
          if (l && e) for (s = l.length - 1; s >= 0; s--) for (o = t(l[s]), n = o.length - 1; n >= 0; n--) a = t.data(o[n], this.widgetFullName),
          a &&
          a !== this &&
          !a.options.disabled &&
          h.push(
            [t.isFunction(a.options.items) ? a.options.items.call(a.element) : t(a.options.items, a.element).not('.ui-sortable-helper').not('.ui-sortable-placeholder'),
            a]
          );
          for (
            h.push(
              [t.isFunction(this.options.items) ? this.options.items.call(
                this.element,
                null,
                {
                  options: this.options,
                  item: this.currentItem
                }
              ) : t(this.options.items, this.element).not('.ui-sortable-helper').not('.ui-sortable-placeholder'),
              this]
            ),
            s = h.length - 1;
            s >= 0;
            s--
          ) h[s][0].each(i);
          return t(r)
        },
        _removeCurrentsFromItems: function () {
          var e = this.currentItem.find(':data(' + this.widgetName + '-item)');
          this.items = t.grep(
            this.items,
            function (t) {
              for (var i = 0; e.length > i; i++) if (e[i] === t.item[0]) return !1;
              return !0
            }
          )
        },
        _refreshItems: function (e) {
          this.items = [],
          this.containers = [
            this
          ];
          var i,
          s,
          n,
          o,
          a,
          r,
          h,
          l,
          c = this.items,
          u = [
            [t.isFunction(this.options.items) ? this.options.items.call(this.element[0], e, {
              item: this.currentItem
            }) : t(this.options.items, this.element),
            this]
          ],
          d = this._connectWith();
          if (d && this.ready) for (i = d.length - 1; i >= 0; i--) for (n = t(d[i]), s = n.length - 1; s >= 0; s--) o = t.data(n[s], this.widgetFullName),
          o &&
          o !== this &&
          !o.options.disabled &&
          (
            u.push(
              [t.isFunction(o.options.items) ? o.options.items.call(o.element[0], e, {
                item: this.currentItem
              }) : t(o.options.items, o.element),
              o]
            ),
            this.containers.push(o)
          );
          for (i = u.length - 1; i >= 0; i--) for (a = u[i][1], r = u[i][0], s = 0, l = r.length; l > s; s++) h = t(r[s]),
          h.data(this.widgetName + '-item', a),
          c.push({
            item: h,
            instance: a,
            width: 0,
            height: 0,
            left: 0,
            top: 0
          })
        },
        refreshPositions: function (e) {
          this.offsetParent &&
          this.helper &&
          (this.offset.parent = this._getParentOffset());
          var i,
          s,
          n,
          o;
          for (i = this.items.length - 1; i >= 0; i--) s = this.items[i],
          s.instance !== this.currentContainer &&
          this.currentContainer &&
          s.item[0] !== this.currentItem[0] ||
          (
            n = this.options.toleranceElement ? t(this.options.toleranceElement, s.item) : s.item,
            e ||
            (s.width = n.outerWidth(), s.height = n.outerHeight()),
            o = n.offset(),
            s.left = o.left,
            s.top = o.top
          );
          if (this.options.custom && this.options.custom.refreshContainers) this.options.custom.refreshContainers.call(this);
           else for (i = this.containers.length - 1; i >= 0; i--) o = this.containers[i].element.offset(),
          this.containers[i].containerCache.left = o.left,
          this.containers[i].containerCache.top = o.top,
          this.containers[i].containerCache.width = this.containers[i].element.outerWidth(),
          this.containers[i].containerCache.height = this.containers[i].element.outerHeight();
          return this
        },
        _createPlaceholder: function (e) {
          e = e ||
          this;
          var i,
          s = e.options;
          s.placeholder &&
          s.placeholder.constructor !== String ||
          (
            i = s.placeholder,
            s.placeholder = {
              element: function () {
                var s = e.currentItem[0].nodeName.toLowerCase(),
                n = t('<' + s + '>', e.document[0]).addClass(i || e.currentItem[0].className + ' ui-sortable-placeholder').removeClass('ui-sortable-helper');
                return 'tr' === s ? e.currentItem.children().each(
                  function () {
                    t('<td>&#160;</td>', e.document[0]).attr('colspan', t(this).attr('colspan') || 1).appendTo(n)
                  }
                ) : 'img' === s &&
                n.attr('src', e.currentItem.attr('src')),
                i ||
                n.css('visibility', 'hidden'),
                n
              },
              update: function (t, n) {
                (!i || s.forcePlaceholderSize) &&
                (
                  n.height() ||
                  n.height(
                    e.currentItem.innerHeight() - parseInt(e.currentItem.css('paddingTop') || 0, 10) - parseInt(e.currentItem.css('paddingBottom') || 0, 10)
                  ),
                  n.width() ||
                  n.width(
                    e.currentItem.innerWidth() - parseInt(e.currentItem.css('paddingLeft') || 0, 10) - parseInt(e.currentItem.css('paddingRight') || 0, 10)
                  )
                )
              }
            }
          ),
          e.placeholder = t(s.placeholder.element.call(e.element, e.currentItem)),
          e.currentItem.after(e.placeholder),
          s.placeholder.update(e, e.placeholder)
        },
        _contactContainers: function (s) {
          var n,
          o,
          a,
          r,
          h,
          l,
          c,
          u,
          d,
          p,
          f = null,
          g = null;
          for (n = this.containers.length - 1; n >= 0; n--) if (
            !t.contains(this.currentItem[0], this.containers[n].element[0])
          ) if (this._intersectsWith(this.containers[n].containerCache)) {
            if (f && t.contains(this.containers[n].element[0], f.element[0])) continue;
            f = this.containers[n],
            g = n
          } else this.containers[n].containerCache.over &&
          (
            this.containers[n]._trigger('out', s, this._uiHash(this)),
            this.containers[n].containerCache.over = 0
          );
          if (f) if (1 === this.containers.length) this.containers[g].containerCache.over ||
          (
            this.containers[g]._trigger('over', s, this._uiHash(this)),
            this.containers[g].containerCache.over = 1
          );
           else {
            for (
              a = 10000,
              r = null,
              p = f.floating ||
              i(this.currentItem),
              h = p ? 'left' : 'top',
              l = p ? 'width' : 'height',
              c = this.positionAbs[h] + this.offset.click[h],
              o = this.items.length - 1;
              o >= 0;
              o--
            ) t.contains(this.containers[g].element[0], this.items[o].item[0]) &&
            this.items[o].item[0] !== this.currentItem[0] &&
            (
              !p ||
              e(
                this.positionAbs.top + this.offset.click.top,
                this.items[o].top,
                this.items[o].height
              )
            ) &&
            (
              u = this.items[o].item.offset() [h],
              d = !1,
              Math.abs(u - c) > Math.abs(u + this.items[o][l] - c) &&
              (d = !0, u += this.items[o][l]),
              a > Math.abs(u - c) &&
              (a = Math.abs(u - c), r = this.items[o], this.direction = d ? 'up' : 'down')
            );
            if (!r && !this.options.dropOnEmpty) return;
            if (this.currentContainer === this.containers[g]) return;
            r ? this._rearrange(s, r, null, !0) : this._rearrange(s, null, this.containers[g].element, !0),
            this._trigger('change', s, this._uiHash()),
            this.containers[g]._trigger('change', s, this._uiHash(this)),
            this.currentContainer = this.containers[g],
            this.options.placeholder.update(this.currentContainer, this.placeholder),
            this.containers[g]._trigger('over', s, this._uiHash(this)),
            this.containers[g].containerCache.over = 1
          }
        },
        _createHelper: function (e) {
          var i = this.options,
          s = t.isFunction(i.helper) ? t(i.helper.apply(this.element[0], [
            e,
            this.currentItem
          ])) : 'clone' === i.helper ? this.currentItem.clone() : this.currentItem;
          return s.parents('body').length ||
          t(
            'parent' !== i.appendTo ? i.appendTo : this.currentItem[0].parentNode
          ) [0].appendChild(s[0]),
          s[0] === this.currentItem[0] &&
          (
            this._storedCSS = {
              width: this.currentItem[0].style.width,
              height: this.currentItem[0].style.height,
              position: this.currentItem.css('position'),
              top: this.currentItem.css('top'),
              left: this.currentItem.css('left')
            }
          ),
          (!s[0].style.width || i.forceHelperSize) &&
          s.width(this.currentItem.width()),
          (!s[0].style.height || i.forceHelperSize) &&
          s.height(this.currentItem.height()),
          s
        },
        _adjustOffsetFromHelper: function (e) {
          'string' == typeof e &&
          (e = e.split(' ')),
          t.isArray(e) &&
          (e = {
            left: + e[0],
            top: + e[1] ||
            0
          }),
          'left' in e &&
          (this.offset.click.left = e.left + this.margins.left),
          'right' in e &&
          (
            this.offset.click.left = this.helperProportions.width - e.right + this.margins.left
          ),
          'top' in e &&
          (this.offset.click.top = e.top + this.margins.top),
          'bottom' in e &&
          (
            this.offset.click.top = this.helperProportions.height - e.bottom + this.margins.top
          )
        },
        _getParentOffset: function () {
          this.offsetParent = this.helper.offsetParent();
          var e = this.offsetParent.offset();
          return 'absolute' === this.cssPosition &&
          this.scrollParent[0] !== document &&
          t.contains(this.scrollParent[0], this.offsetParent[0]) &&
          (
            e.left += this.scrollParent.scrollLeft(),
            e.top += this.scrollParent.scrollTop()
          ),
          (
            this.offsetParent[0] === document.body ||
            this.offsetParent[0].tagName &&
            'html' === this.offsetParent[0].tagName.toLowerCase() &&
            t.ui.ie
          ) &&
          (e = {
            top: 0,
            left: 0
          }),
          {
            top: e.top + (parseInt(this.offsetParent.css('borderTopWidth'), 10) || 0),
            left: e.left + (parseInt(this.offsetParent.css('borderLeftWidth'), 10) || 0)
          }
        },
        _getRelativeOffset: function () {
          if ('relative' === this.cssPosition) {
            var t = this.currentItem.position();
            return {
              top: t.top - (parseInt(this.helper.css('top'), 10) || 0) + this.scrollParent.scrollTop(),
              left: t.left - (parseInt(this.helper.css('left'), 10) || 0) + this.scrollParent.scrollLeft()
            }
          }
          return {
            top: 0,
            left: 0
          }
        },
        _cacheMargins: function () {
          this.margins = {
            left: parseInt(this.currentItem.css('marginLeft'), 10) ||
            0,
            top: parseInt(this.currentItem.css('marginTop'), 10) ||
            0
          }
        },
        _cacheHelperProportions: function () {
          this.helperProportions = {
            width: this.helper.outerWidth(),
            height: this.helper.outerHeight()
          }
        },
        _setContainment: function () {
          var e,
          i,
          s,
          n = this.options;
          'parent' === n.containment &&
          (n.containment = this.helper[0].parentNode),
          ('document' === n.containment || 'window' === n.containment) &&
          (
            this.containment = [
              0 - this.offset.relative.left - this.offset.parent.left,
              0 - this.offset.relative.top - this.offset.parent.top,
              t('document' === n.containment ? document : window).width() - this.helperProportions.width - this.margins.left,
              (
                t('document' === n.containment ? document : window).height() ||
                document.body.parentNode.scrollHeight
              ) - this.helperProportions.height - this.margins.top
            ]
          ),
          /^(document|window|parent)$/.test(n.containment) ||
          (
            e = t(n.containment) [0],
            i = t(n.containment).offset(),
            s = 'hidden' !== t(e).css('overflow'),
            this.containment = [
              i.left + (parseInt(t(e).css('borderLeftWidth'), 10) || 0) + (parseInt(t(e).css('paddingLeft'), 10) || 0) - this.margins.left,
              i.top + (parseInt(t(e).css('borderTopWidth'), 10) || 0) + (parseInt(t(e).css('paddingTop'), 10) || 0) - this.margins.top,
              i.left + (s ? Math.max(e.scrollWidth, e.offsetWidth) : e.offsetWidth) - (parseInt(t(e).css('borderLeftWidth'), 10) || 0) - (parseInt(t(e).css('paddingRight'), 10) || 0) - this.helperProportions.width - this.margins.left,
              i.top + (s ? Math.max(e.scrollHeight, e.offsetHeight) : e.offsetHeight) - (parseInt(t(e).css('borderTopWidth'), 10) || 0) - (parseInt(t(e).css('paddingBottom'), 10) || 0) - this.helperProportions.height - this.margins.top
            ]
          )
        },
        _convertPositionTo: function (e, i) {
          i ||
          (i = this.position);
          var s = 'absolute' === e ? 1 : - 1,
          n = 'absolute' !== this.cssPosition ||
          this.scrollParent[0] !== document &&
          t.contains(this.scrollParent[0], this.offsetParent[0]) ? this.scrollParent : this.offsetParent,
          o = /(html|body)/i.test(n[0].tagName);
          return {
            top: i.top + this.offset.relative.top * s + this.offset.parent.top * s - (
              'fixed' === this.cssPosition ? - this.scrollParent.scrollTop() : o ? 0 : n.scrollTop()
            ) * s,
            left: i.left + this.offset.relative.left * s + this.offset.parent.left * s - (
              'fixed' === this.cssPosition ? - this.scrollParent.scrollLeft() : o ? 0 : n.scrollLeft()
            ) * s
          }
        },
        _generatePosition: function (e) {
          var i,
          s,
          n = this.options,
          o = e.pageX,
          a = e.pageY,
          r = 'absolute' !== this.cssPosition ||
          this.scrollParent[0] !== document &&
          t.contains(this.scrollParent[0], this.offsetParent[0]) ? this.scrollParent : this.offsetParent,
          h = /(html|body)/i.test(r[0].tagName);
          return 'relative' !== this.cssPosition ||
          this.scrollParent[0] !== document &&
          this.scrollParent[0] !== this.offsetParent[0] ||
          (this.offset.relative = this._getRelativeOffset()),
          this.originalPosition &&
          (
            this.containment &&
            (
              e.pageX - this.offset.click.left < this.containment[0] &&
              (o = this.containment[0] + this.offset.click.left),
              e.pageY - this.offset.click.top < this.containment[1] &&
              (a = this.containment[1] + this.offset.click.top),
              e.pageX - this.offset.click.left > this.containment[2] &&
              (o = this.containment[2] + this.offset.click.left),
              e.pageY - this.offset.click.top > this.containment[3] &&
              (a = this.containment[3] + this.offset.click.top)
            ),
            n.grid &&
            (
              i = this.originalPageY + Math.round((a - this.originalPageY) / n.grid[1]) * n.grid[1],
              a = this.containment ? i - this.offset.click.top >= this.containment[1] &&
              i - this.offset.click.top <= this.containment[3] ? i : i - this.offset.click.top >= this.containment[1] ? i - n.grid[1] : i + n.grid[1] : i,
              s = this.originalPageX + Math.round((o - this.originalPageX) / n.grid[0]) * n.grid[0],
              o = this.containment ? s - this.offset.click.left >= this.containment[0] &&
              s - this.offset.click.left <= this.containment[2] ? s : s - this.offset.click.left >= this.containment[0] ? s - n.grid[0] : s + n.grid[0] : s
            )
          ),
          {
            top: a - this.offset.click.top - this.offset.relative.top - this.offset.parent.top + (
              'fixed' === this.cssPosition ? - this.scrollParent.scrollTop() : h ? 0 : r.scrollTop()
            ),
            left: o - this.offset.click.left - this.offset.relative.left - this.offset.parent.left + (
              'fixed' === this.cssPosition ? - this.scrollParent.scrollLeft() : h ? 0 : r.scrollLeft()
            )
          }
        },
        _rearrange: function (t, e, i, s) {
          i ? i[0].appendChild(this.placeholder[0]) : e.item[0].parentNode.insertBefore(
            this.placeholder[0],
            'down' === this.direction ? e.item[0] : e.item[0].nextSibling
          ),
          this.counter = this.counter ? ++this.counter : 1;
          var n = this.counter;
          this._delay(function () {
            n === this.counter &&
            this.refreshPositions(!s)
          })
        },
        _clear: function (t, e) {
          function i(t, e, i) {
            return function (s) {
              i._trigger(t, s, e._uiHash(e))
            }
          }
          this.reverting = !1;
          var s,
          n = [];
          if (
            !this._noFinalSort &&
            this.currentItem.parent().length &&
            this.placeholder.before(this.currentItem),
            this._noFinalSort = null,
            this.helper[0] === this.currentItem[0]
          ) {
            for (s in this._storedCSS) ('auto' === this._storedCSS[s] || 'static' === this._storedCSS[s]) &&
            (this._storedCSS[s] = '');
            this.currentItem.css(this._storedCSS).removeClass('ui-sortable-helper')
          } else this.currentItem.show();
          for (
            this.fromOutside &&
            !e &&
            n.push(
              function (t) {
                this._trigger('receive', t, this._uiHash(this.fromOutside))
              }
            ),
            !this.fromOutside &&
            this.domPosition.prev === this.currentItem.prev().not('.ui-sortable-helper') [0] &&
            this.domPosition.parent === this.currentItem.parent() [0] ||
            e ||
            n.push(function (t) {
              this._trigger('update', t, this._uiHash())
            }),
            this !== this.currentContainer &&
            (
              e ||
              (
                n.push(function (t) {
                  this._trigger('remove', t, this._uiHash())
                }),
                n.push(
                  function (t) {
                    return function (e) {
                      t._trigger('receive', e, this._uiHash(this))
                    }
                  }.call(this, this.currentContainer)
                ),
                n.push(
                  function (t) {
                    return function (e) {
                      t._trigger('update', e, this._uiHash(this))
                    }
                  }.call(this, this.currentContainer)
                )
              )
            ),
            s = this.containers.length - 1;
            s >= 0;
            s--
          ) e ||
          n.push(i('deactivate', this, this.containers[s])),
          this.containers[s].containerCache.over &&
          (
            n.push(i('out', this, this.containers[s])),
            this.containers[s].containerCache.over = 0
          );
          if (
            this.storedCursor &&
            (
              this.document.find('body').css('cursor', this.storedCursor),
              this.storedStylesheet.remove()
            ),
            this._storedOpacity &&
            this.helper.css('opacity', this._storedOpacity),
            this._storedZIndex &&
            this.helper.css('zIndex', 'auto' === this._storedZIndex ? '' : this._storedZIndex),
            this.dragging = !1,
            this.cancelHelperRemoval
          ) {
            if (!e) {
              for (
                this._trigger('beforeStop', t, this._uiHash()),
                s = 0;
                n.length > s;
                s++
              ) n[s].call(this, t);
              this._trigger('stop', t, this._uiHash())
            }
            return this.fromOutside = !1,
            !1
          }
          if (
            e ||
            this._trigger('beforeStop', t, this._uiHash()),
            this.placeholder[0].parentNode.removeChild(this.placeholder[0]),
            this.helper[0] !== this.currentItem[0] &&
            this.helper.remove(),
            this.helper = null,
            !e
          ) {
            for (s = 0; n.length > s; s++) n[s].call(this, t);
            this._trigger('stop', t, this._uiHash())
          }
          return this.fromOutside = !1,
          !0
        },
        _trigger: function () {
          t.Widget.prototype._trigger.apply(this, arguments) === !1 &&
          this.cancel()
        },
        _uiHash: function (e) {
          var i = e ||
          this;
          return {
            helper: i.helper,
            placeholder: i.placeholder ||
            t([]),
            position: i.position,
            originalPosition: i.originalPosition,
            offset: i.positionAbs,
            item: i.currentItem,
            sender: e ? e.element : null
          }
        }
      }
    )
  }
) (jQuery);
; // places/Access.js
typeof Roblox == 'undefined' &&
(Roblox = {}),
Roblox.PlayerAccess = function () {
  function u() {
    var n = $('#GamePlaceAccess').data('invite-list-user-limit');
    $('#GamePlaceAccess').data('permissions-service-enabled') === 'True' &&
    $('.chzn-container').length <= 0 &&
    (
      $('.chosen').chosen({
        no_results_text: Roblox.PlayerAccess.strings.SearchingFor,
        max_selected_options: n,
        disable_search: !0,
        disable_search_threshold: 1
      }),
      $('.chosen').bind(
        'liszt:maxselected',
        function () {
          $('#maxUserNameLimit').show()
        }
      ),
      $('.chzn-choices').before(
        '<label class="form-label">' + Roblox.PlayerAccess.strings.InviteList + ':</label>'
      ),
      $('#Access_Whitelist').is(':checked') ||
      $('input[id*=visit]').filter(function () {
        return /\d+ visit/.test(this.id)
      }).is(':checked') ||
      $('#customlist_chzn').hide(),
      $('.chzn-container input').keyup(function (n) {
        n.which !== 13 &&
        $('#customlist-error').remove()
      }),
      $('#customlist').chosen().change(
        function () {
          var n = $('#customlist').val();
          $('#customlist-usernames').val(n),
          $.browser.msie &&
          parseInt($.browser.version, 10) === 7 &&
          (
            n == null ? $('.chzn-container-multi .chzn-choices').css('padding-bottom', '0') : $('.chzn-container-multi .chzn-choices').css('padding-bottom', '3px')
          )
        }
      ),
      $('.search-choice-close').click(function (n) {
        n.stopPropagation()
      })
    )
  }
  function t() {
    $('#PlayerLimit').show(),
    $('#NumPlayers').show(),
    $('#GamePlaceAccess').show(),
    $('#PrivateServersAccess').show();
    var n = $('#playerAccess');
    n.is(':visible') &&
    $('#GamePlaceAccess').is(':visible') &&
    (
      Roblox.PlayerAccess.initializeChosen(),
      $('#navbar').height(n.height())
    )
  }
  function f() {
    $('#SellGameAccessCheckbox').attr('checked') ? (
      $('#PricingPanel').show(),
      i('Robux'),
      $('#PlayerLimit').show(),
      $('#NumPlayers').show(),
      $('#PlaceTypePanel').hide(),
      $('#copyLock').hide(),
      $('#comments').hide()
    ) : (
      $('#PricingPanel').hide(),
      t(),
      $('#PlaceTypePanel').show(),
      $('#copyLock').show(),
      $('#comments').show()
    )
  }
  function e() {
    var i = $('#SellGameAccess'),
    r = i.data('minprice'),
    t = i.data('maxprice'),
    n = $('#PriceInput').val();
    $('#PricingError').hide(),
    $('#PricingErrorMax').hide(),
    isNaN(n) ? (
      $('#PriceInput').val(n.replace(/\D/g, '')),
      $('#PricingError').show()
    ) : n < r ? ($('#PriceInput').val(r), $('#PricingError').show()) : t !== null &&
    n > t &&
    ($('#PriceInput').val(t), $('#PricingErrorMax').show())
  }
  function o(n, t) {
    var i = $('#PrivateServerPricingError').hide();
    return !isNaN(t) &&
    !isNaN(n) &&
    t < n ? ($('#PrivateServerPriceInput').val(n), i.show(), n) : t
  }
  function i() {
    var n = $('#SellGameAccess'),
    t = n.data('minprice'),
    i = n.data('maxprice');
    t !== i &&
    e(),
    $('#MarketPlaceFee').html(h()),
    $('#Profit').html(l())
  }
  function s() {
    var i = $('#PrivateServerPriceContainer'),
    r = i.data('taxrate'),
    t = i.data('minprice'),
    n = $('#PrivateServerPriceInput').val();
    n = o(t, n),
    $('#PrivateServerMarketplaceFeeText').html(c(r, t, n)),
    $('#PrivateServerUserProfitText').html(a(r, t, n))
  }
  function h() {
    var t = $('#SellGameAccess'),
    i = t.data('minprice'),
    r = t.data('maxprice'),
    u = t.data('commisionrate'),
    f = $('#PriceInput').val();
    return n(u, i, r, f)
  }
  function c(t, i, r) {
    return n(t, i, - 1, r)
  }
  function n(n, t, i, r) {
    var s = 1,
    u,
    f,
    o = s,
    e;
    return f = t !== i ? r : t,
    e = Math.round(f * n),
    u = e > o ? e : o
  }
  function l() {
    var n = $('#SellGameAccess'),
    t = n.data('minprice'),
    i = n.data('maxprice'),
    u = n.data('commisionrate'),
    f = $('#PriceInput').val();
    return r(u, t, i, f)
  }
  function a(n, t, i) {
    return r(n, t, - 1, i)
  }
  function r(t, i, r, u) {
    var f;
    return f = i !== r ? u : i,
    f - n(t, i, r, u)
  }
  return {
    initializeChosen: u,
    displayGamePlace: t,
    checkSaleOptions: f,
    calculateFeeAndProfit: i,
    calculatePrivateServerFeeAndProfit: s
  }
}(),
$(
  function () {
    'use strict';
    function at(n) {
      a.attr('disabled', n ? !0 : null),
      ht.toggleClass('disabled', n),
      n ? e.show() : e.hide()
    }
    function vt(n) {
      l.attr('disabled', n ? !0 : null),
      l.toggleClass('disabled', n),
      n ? s.show() : s.hide()
    }
    function yt(n) {
      c.attr('disabled', n ? !0 : null),
      pt.toggleClass('disabled', n),
      n ? st.show() : st.hide(),
      c.prop('checked') ? (v.hide(), ut.show(), it.is(':checked') ? r.hide() : r.show()) : (
        Roblox.AccessData.privateServersAllowed ? v.show() : v.hide(),
        ut.hide(),
        r.hide()
      )
    }
    function o() {
      var n = a.prop('checked'),
      t = l.val() === 'Everyone',
      i = c.prop('checked'),
      f = !t ||
      i,
      r,
      u;
      at(f),
      r = n ||
      i,
      vt(r),
      u = n ||
      !t,
      yt(u)
    }
    var b = $('#MaxPlayersInput'),
    t = $('#PreferredPlayersInput'),
    i = $('#FriendSlotsInput'),
    tt = $('#FriendSlotRadioButtons'),
    u = $('#CustomFriendSlotDropdown'),
    f = $('#EmptyFriendSlotError'),
    k = $('label[data-device=\'Console\'] input[type=\'checkbox\']'),
    lt = $('div[data-console-agreement-enabled]').attr('data-console-agreement-enabled') === 'True',
    ut = $('#PrivateServerDetails'),
    g = $('#PrivateServerPriceInput'),
    et = $('#PrivateServerPricingError'),
    d = $('#PrivateServerPriceChangeWarning'),
    ht = $('#SellGameAccessLabel'),
    pt = $('#PrivateServerAccessLabel'),
    e = $('.sell-access-tooltip'),
    s = $('.place-access-tooltip'),
    nt = $('.private-server-tooltip'),
    rt = $('.number-of-players-preferred-tooltip'),
    ft = $('.number-of-players-max-tooltip'),
    ot = $('.regional-availability-tooltip'),
    a = $('#SellGameAccessCheckbox'),
    l = $('#Access'),
    c = $('#AllowPrivateServersCheckbox'),
    p = $('#EmptyFriendSlots'),
    h = $('#CustomFriendSlots'),
    st = $('#PrivateServersError'),
    v = $('#PrivateServerDisableWarning'),
    wt = $('#PrivateServerRadioButtons'),
    it = $('#FreePrivateServers'),
    ct = $('#PaidPrivateServers'),
    r = $('#PrivateServerPriceContainer'),
    y,
    w,
    n;
    k.change(
      function () {
        lt &&
        this.checked &&
        (
          k.prop('checked', !1),
          Roblox.GenericConfirmation.open({
            titleText: Roblox.PlayerAccess.strings.ConsoleAccessContentAgreementTitleText,
            bodyContent: Roblox.PlayerAccess.strings.ConsoleAccessContentAgreementBodyContent,
            acceptText: Roblox.PlayerAccess.strings.ConsoleAccessContentAgreementAcceptText,
            declineText: Roblox.PlayerAccess.strings.ConsoleAccessContentAgreementDeclineText,
            acceptColor: Roblox.GenericConfirmation.gray,
            declineColor: Roblox.GenericConfirmation.gray,
            onAccept: function () {
              k.prop('checked', !0)
            },
            allowHtmlContentInBody: !0
          })
        )
      }
    ),
    $('#Genre').change(
      function () {
        $('#advancedsettings_genre').text($(this).find(':selected').text())
      }
    ),
    h.is(':checked') ? ($(u).attr('hidden', null), $(f).prop('hidden', 'hidden')) : p.is(':checked') &&
    ($(f).prop('hidden', null), $(u).prop('hidden', 'hidden'));
    $('#NumPlayers').on(
      'change',
      '#MaxPlayersInput',
      function () {
        var e = 1,
        r = Number(b.val()),
        u = Number($(i).val()),
        f = Number($(t).val());
        for ($(i).empty(), $(t).empty(), n = e; n < r; n++) $(i).append($('<option></option>').attr('value', n).text(n));
        for (n = e; n <= r; n++) $(t).append($('<option></option>').attr('value', n).text(n));
        u < r &&
        u > 0 ? i.val(u) : i.val(1),
        f < r &&
        f > 0 ? t.val(f) : t.val(1)
      }
    );
    if (
      $(tt).change(
        function () {
          h.is(':checked') ? ($(u).attr('hidden', null), $(f).prop('hidden', 'hidden')) : p.is(':checked') ? ($(f).prop('hidden', null), $(u).prop('hidden', 'hidden')) : ($(f).prop('hidden', 'hidden'), $(u).prop('hidden', 'hidden'))
        }
      ),
      $(tt).length > 0
    ) $('#NumPlayers').on(
      'change',
      '#MaxPlayersInput, #FriendSlotsInput',
      function () {
        var n = Number(b.val()),
        t;
        n == 1 ? ($(p).prop('checked', !0).change(), $(h).prop('disabled', !0)) : ($(h).prop('disabled', !1), t = Number(i.val()), t > n - 1 && i.val(n - 1))
      }
    );
     else $('#NumPlayers').on(
      'change',
      '#MaxPlayersInput, #PreferredPlayersInput',
      function () {
        var n = Number(b.val()),
        i = Number(t.val());
        i > n &&
        t.val(n)
      }
    );
    if (
      nt.length > 0 &&
      nt.tipsy(),
      rt.length > 0 &&
      rt.tipsy(),
      ft.length > 0 &&
      ft.tipsy(),
      e.length > 0 &&
      e.tipsy(),
      s.length > 0 &&
      s.tipsy(),
      ot.length > 0 &&
      ot.tipsy(),
      et.hide(),
      d.hide(),
      ct.is(':checked') &&
      Roblox.PlayerAccess.calculatePrivateServerFeeAndProfit(),
      Roblox.PlayerAccess.checkSaleOptions(),
      a.click(function () {
        o(),
        Roblox.PlayerAccess.checkSaleOptions()
      }),
      $(wt).change(
        function () {
          if (it.is(':checked')) $(r).hide(),
          $(g).val(0),
          $(et).hide();
           else {
            $(r).show();
            var n = r.data('defaultprice');
            $(g).val(n),
            Roblox.PlayerAccess.calculatePrivateServerFeeAndProfit()
          }
          d.show()
        }
      ),
      $('#PriceInput').change(function () {
        Roblox.PlayerAccess.calculateFeeAndProfit()
      }),
      g.change(
        function () {
          d.show(),
          Roblox.PlayerAccess.calculatePrivateServerFeeAndProfit()
        }
      ),
      Roblox.PlayerAccess.displayGamePlace(),
      $('#GamePlaceAccess').data('permissions-service-enabled') === 'True' &&
      (
        $('#GamePlaceAccess input').change(
          function () {
            $(this).is(':checked') &&
            (
              $(this).is('#Access_Whitelist') ||
              $(this).attr('id').indexOf('visit') > - 1
            ) ? $('#customlist_chzn').show() : $('#customlist_chzn').hide()
          }
        ),
        y = $('#customlist-usernames').val(),
        y.length > 0
      )
    ) for (w = y.split(','), n = 0; n < w.length; n++) $('#customlist').append('<option selected="selected">' + w[n] + '</option>');
    $('#playerAccessTab').one(
      'click',
      function () {
        if (!Roblox.AccessData.isDevelopSiteForVipServersEnabled) return !1;
        var t = $('#ActivePrivateServersSubscriptions'),
        r = $('#ActivePrivateServersCount'),
        n = $('#PrivateServerCountsLoading'),
        i = $('#PrivateServerDetailsError');
        return i.hide(),
        $.ajax({
          cache: !1,
          type: 'GET',
          url: Roblox.AccessData.vipServerConfigurationLink
        }).done(
          function (i) {
            var e = i.activeSubscriptionsCount < 0 ? 'over ' + - i.activeSubscriptionsCount : i.activeSubscriptionsCount,
            o = 'There are currently ' + e + ' private server subscriptions active for this place.',
            u,
            f;
            t.text(o),
            u = i.activeServersCount < 0 ? 'over ' + - i.activeServersCount : i.activeServersCount,
            f = 'There are currently ' + u + ' private servers active for this place.',
            r.text(f),
            n &&
            n.hide()
          }
        ).fail(function () {
          t.hide(),
          i.show(),
          n &&
          n.hide()
        }),
        !1
      }
    );
    $('#configureUniverseForm div.tab[data-id = \'accessTab\']').one(
      'click',
      function () {
        if (!Roblox.AccessData.isDevelopSiteForVipServersEnabled) return !1;
        $('#ActivePrivateServersCount').hide(),
        $('#ActivePrivateServersSubscriptions').hide(),
        $('#PrivateServerDetailsError').hide()
      }
    );
    l.change(o),
    c.click(o),
    o()
  }
);
; // Places/DeviceSelection.js
$(
  function () {
    'use strict';
    var n = $('#device-type-error').hide(),
    t = $('div.deviceTypeSection input:checkbox');
    t.on(
      'change',
      function () {
        var r,
        i;
        if (this.checked) n.hide();
         else {
          for (r = 0, i = 0; i < t.length; i++) t[i].checked &&
          (r += 1);
          r < 1 ? (n.show(), this.checked = !0) : n.hide()
        }
      }
    )
  }
);
; // utilities/dialog.js
typeof Roblox == 'undefined' &&
(Roblox = {}),
typeof Roblox.Dialog == 'undefined' &&
(
  Roblox.Dialog = function () {
    function g(f) {
      var k,
      c,
      v,
      w,
      o,
      b;
      n.isOpen = !0,
      k = {
        titleText: '',
        bodyContent: '',
        footerText: '',
        footerMiddleAligned: !1,
        acceptText: a.Yes,
        declineText: a.No,
        acceptColor: h,
        declineColor: s,
        xToCancel: !1,
        onAccept: function () {
          return !1
        },
        onDecline: function () {
          return !1
        },
        onCancel: function () {
          return !1
        },
        imageUrl: null,
        showAccept: !0,
        showDecline: !0,
        allowHtmlContentInBody: !1,
        allowHtmlContentInFooter: !1,
        dismissable: !0,
        fieldValidationRequired: !1,
        onOpenCallback: function () {
        },
        onCloseCallback: t,
        cssClass: null,
        checkboxAgreementText: a.Agree,
        checkboxAgreementRequired: !1
      },
      f = $.extend({
      }, k, f),
      i.overlayClose = f.dismissable,
      i.escClose = f.dismissable,
      f.onCloseCallback &&
      (i.onClose = function () {
        f.onCloseCallback(),
        t()
      }),
      c = $(u),
      c.html(f.acceptText),
      c.attr('class', f.acceptColor),
      c.unbind(),
      c.bind(
        'click',
        function () {
          return p(c) ? !1 : (f.fieldValidationRequired ? nt(f.onAccept) : l(f.onAccept), !1)
        }
      ),
      v = $(e),
      v.html(f.declineText),
      v.attr('class', f.declineColor),
      v.unbind(),
      v.bind('click', function () {
        return p(v) ? !1 : (l(f.onDecline), !1)
      }),
      w = $(tt),
      w.unbind(),
      w.bind('change', function () {
        w.is(':checked') ? y(c) : r(c)
      }),
      o = $('[data-modal-type="confirmation"]'),
      o.find('.modal-title').text(f.titleText),
      f.imageUrl == null ? o.addClass('noImage') : (
        o.find('img.modal-thumb').attr('src', f.imageUrl),
        o.removeClass('noImage')
      ),
      n.extraClass &&
      (o.removeClass(n.extraClass), n.extraClass = !1),
      f.cssClass != null &&
      (o.addClass(f.cssClass), n.extraClass = f.cssClass),
      f.allowHtmlContentInBody ? o.find('.modal-message').html(f.bodyContent) : o.find('.modal-message').text(f.bodyContent),
      f.checkboxAgreementRequired ? (
        r(c),
        o.find('.modal-checkbox.checkbox > input').prop('checked', !1),
        o.find('.modal-checkbox.checkbox > label').text(f.checkboxAgreementText),
        o.find('.modal-checkbox.checkbox').show()
      ) : (
        o.find('.modal-checkbox.checkbox > input').prop('checked', !0),
        o.find('.modal-checkbox.checkbox').hide()
      ),
      $.trim(f.footerText) == '' ? o.find('.modal-footer').hide() : o.find('.modal-footer').show(),
      f.allowHtmlContentInFooter ? o.find('.modal-footer').html(f.footerText) : o.find('.modal-footer').text(f.footerText),
      f.footerMiddleAligned &&
      o.find('.modal-footer').addClass('modal-footer-center'),
      o.modal(i),
      b = $(d + ' .modal-header .close'),
      b.unbind(),
      b.bind('click', function () {
        return l(f.onCancel),
        !1
      }),
      f.xToCancel ||
      b.hide(),
      f.showAccept ||
      c.hide(),
      f.showDecline ||
      v.hide(),
      $('#rbx-body').addClass('modal-mask'),
      f.onOpenCallback()
    }
    function r(n) {
      n.hasClass(s) ? n.addClass(f) : n.hasClass(v) ? n.addClass(o) : n.hasClass(h) &&
      n.addClass(c)
    }
    function p(n) {
      return n.hasClass(c) ||
      n.hasClass(f) ||
      n.hasClass(o) ? !0 : !1
    }
    function rt() {
      var n = $(u),
      t = $(e);
      r(n),
      r(t)
    }
    function y(n) {
      n.hasClass(f) ? (n.removeClass(f), n.addClass(s)) : n.hasClass(o) ? (n.removeClass(o), n.addClass(v)) : n.hasClass(c) &&
      (n.removeClass(c), n.addClass(h))
    }
    function w() {
      var n = $(u),
      t = $(e);
      y(n),
      y(t)
    }
    function k() {
      if (n.isOpen) {
        var t = $(u);
        t.click()
      }
    }
    function b() {
      var n = $(e);
      n.click()
    }
    function t(t) {
      n.isOpen = !1,
      typeof t != 'undefined' ? $.modal.close(t) : $.modal.close(),
      $('#rbx-body').removeClass('modal-mask')
    }
    function l(n) {
      t(),
      typeof n == 'function' &&
      n()
    }
    function nt(n) {
      if (typeof n == 'function') {
        var i = n();
        if (i !== 'undefined' && i == !1) return !1
      }
      t()
    }
    function ut(n, t) {
      var i = $('.modal-body');
      n ? (
        i.find('.modal-btns').hide(),
        i.find('.modal-processing').show()
      ) : (
        i.find('.modal-btns').show(),
        i.find('.modal-processing').hide()
      ),
      typeof t != 'undefined' &&
      t !== '' &&
      $.modal.close('.' + t)
    }
    var d = '.simplemodal-container',
    v = 'btn-primary-md',
    h = 'btn-secondary-md',
    s = 'btn-control-md',
    o = 'btn-primary-md disabled',
    c = 'btn-secondary-md disabled',
    f = 'btn-control-md disabled',
    it = 'btn-none',
    u = '.modal-btns #confirm-btn',
    e = '.modal-btns #decline-btn',
    tt = '#modal-checkbox-input',
    n = {
      isOpen: !1
    },
    i = {
      overlayClose: !0,
      escClose: !0,
      opacity: 80,
      zIndex: 1040,
      overlayCss: {
        backgroundColor: '#000'
      },
      onClose: t,
      focus: !1
    },
    a = {
      Yes: 'Yes',
      No: 'No',
      Agree: 'Agree'
    };
    return {
      open: g,
      close: t,
      disableButtons: rt,
      enableButtons: w,
      clickYes: k,
      clickNo: b,
      status: n,
      toggleProcessing: ut,
      green: v,
      blue: h,
      white: s,
      none: it
    }
  }()
),
$(document).keypress(
  function (n) {
    Roblox.Dialog.isOpen &&
    n.which === 13 &&
    Roblox.Dialog.clickYes()
  }
);
; // GenericConfirmation.js
typeof Roblox == 'undefined' &&
(Roblox = {}),
typeof Roblox.GenericConfirmation == 'undefined' &&
(
  Roblox.GenericConfirmation = function () {
    function w() {
      n.isOpen = !1,
      t()
    }
    function k(t) {
      var a,
      e,
      o,
      s,
      l;
      n.isOpen = !0,
      a = {
        titleText: '',
        bodyContent: '',
        footerText: '',
        acceptText: Roblox.Resources.GenericConfirmation.yes,
        declineText: Roblox.Resources.GenericConfirmation.No,
        acceptColor: u,
        declineColor: f,
        xToCancel: !1,
        onAccept: function () {
          return !1
        },
        onDecline: function () {
          return !1
        },
        onCancel: function () {
          return !1
        },
        imageUrl: null,
        allowHtmlContentInBody: !1,
        allowHtmlContentInFooter: !1,
        dismissable: !0,
        fieldValidationRequired: !1,
        onOpenCallback: function () {
        }
      },
      t = $.extend({
      }, a, t),
      c.overlayClose = t.dismissable,
      c.escClose = t.dismissable,
      e = $(i),
      e.html(t.acceptText),
      e.attr('class', 'btn-large ' + t.acceptColor),
      e.unbind(),
      e.bind(
        'click',
        function () {
          return v(e) ? !1 : (t.fieldValidationRequired ? nt(t.onAccept) : h(t.onAccept), !1)
        }
      ),
      o = $(r),
      o.html(t.declineText),
      o.attr('class', 'btn-large ' + t.declineColor),
      o.unbind(),
      o.bind('click', function () {
        return v(o) ? !1 : (h(t.onDecline), !1)
      }),
      $('[data-modal-handle="confirmation"] div.Title').text(t.titleText),
      s = $('[data-modal-handle=\'confirmation\']'),
      t.imageUrl == null ? s.addClass('noImage') : (
        s.find('img.GenericModalImage').attr('src', t.imageUrl),
        s.removeClass('noImage')
      ),
      t.allowHtmlContentInBody ? $('[data-modal-handle=\'confirmation\'] div.Message').html(t.bodyContent) : $('[data-modal-handle=\'confirmation\'] div.Message').text(t.bodyContent),
      $.trim(t.footerText) == '' ? $(
        '[data-modal-handle="confirmation"] div.ConfirmationModalFooter'
      ).hide() : $(
        '[data-modal-handle="confirmation"] div.ConfirmationModalFooter'
      ).show(),
      t.allowHtmlContentInFooter ? $(
        '[data-modal-handle="confirmation"] div.ConfirmationModalFooter'
      ).html(t.footerText) : $(
        '[data-modal-handle="confirmation"] div.ConfirmationModalFooter'
      ).text(t.footerText),
      $('[data-modal-handle=\'confirmation\']').modal(c),
      l = $('a.genericmodal-close'),
      l.unbind(),
      l.bind('click', function () {
        return h(t.onCancel),
        !1
      }),
      t.xToCancel ||
      l.hide(),
      t.onOpenCallback()
    }
    function a(n) {
      n.hasClass(f) ? n.addClass(s) : n.hasClass(l) ? n.addClass(e) : n.hasClass(u) &&
      n.addClass(o)
    }
    function v(n) {
      return n.hasClass(o) ||
      n.hasClass(s) ||
      n.hasClass(e) ? !0 : !1
    }
    function b() {
      var n = $(i),
      t = $(r);
      a(n),
      a(t)
    }
    function g() {
      var u = $(i),
      t = $(r),
      n = o + ' ' + s + ' ' + e;
      u.removeClass(n),
      t.removeClass(n)
    }
    function p() {
      if (n.isOpen) {
        var t = $(i);
        t.click()
      }
    }
    function y() {
      var n = $(r);
      n.click()
    }
    function t(t) {
      n.isOpen = !1,
      typeof t != 'undefined' ? $.modal.close(t) : $.modal.close()
    }
    function h(n) {
      t(),
      typeof n == 'function' &&
      n()
    }
    function nt(n) {
      if (typeof n == 'function') {
        var i = n();
        if (i !== 'undefined' && i == !1) return !1
      }
      t()
    }
    var l = 'btn-primary',
    u = 'btn-neutral',
    f = 'btn-negative',
    e = 'btn-disabled-primary',
    o = 'btn-disabled-neutral',
    s = 'btn-disabled-negative',
    d = 'btn-none',
    i = '#roblox-confirm-btn',
    r = '#roblox-decline-btn',
    n = {
      isOpen: !1
    },
    c = {
      overlayClose: !0,
      escClose: !0,
      opacity: 80,
      overlayCss: {
        backgroundColor: '#000'
      },
      onClose: w
    };
    return {
      open: k,
      close: t,
      disableButtons: b,
      enableButtons: g,
      clickYes: p,
      clickNo: y,
      status: n,
      green: l,
      blue: u,
      gray: f,
      none: d
    }
  }()
),
$(document).keypress(
  function (n) {
    Roblox.GenericConfirmation.status.isOpen &&
    n.which === 13 &&
    Roblox.GenericConfirmation.clickYes()
  }
);
; // common/deviceMeta.js
var Roblox = Roblox ||
{
};
Roblox.DeviceMeta = function () {
  var t = document.querySelector('meta[name="device-meta"]');
  if (t === null) {
    console.debug(
      'Error loading device information from meta tag - please check if meta tag is present'
    );
    return
  }
  var n = t.dataset ||
  {
  },
  i = {
    android: 'android',
    ios: 'ios',
    xbox: 'xbox',
    uwp: 'uwp',
    amazon: 'amazon',
    win32: 'win32',
    universalapp: 'universalApp',
    unknown: 'unknown'
  },
  r = {
    computer: 'computer',
    tablet: 'tablet',
    phone: 'phone',
    console: 'console'
  };
  return function () {
    return {
      deviceType: r[n.deviceType] ||
      '',
      appType: i[n.appType] ||
      '',
      isInApp: n.isInApp === 'true',
      isDesktop: n.isDesktop === 'true',
      isPhone: n.isPhone === 'true',
      isTablet: n.isTablet === 'true',
      isConsole: n.isConsole === 'true',
      isAndroidApp: n.isAndroidApp === 'true',
      isIosApp: n.isIosApp === 'true',
      isUWPApp: n.isUwpApp === 'true',
      isXboxApp: n.isXboxApp === 'true',
      isAmazonApp: n.isAmazonApp === 'true',
      isWin32App: n.isWin32App === 'true',
      isStudio: n.isStudio === 'true',
      isIosDevice: n.isIosDevice === 'true',
      isAndroidDevice: n.isAndroidDevice === 'true',
      isUniversalApp: n.isUniversalApp === 'true'
    }
  }
}();
; // AbuseReport/AbuseReportDispatcher.js
var Roblox = Roblox ||
{
};
Roblox.AbuseReportDispatcher = function () {
  var i = !1,
  n = function (n, t) {
    n = n.replace(/[\[\]]/g, '\\$&');
    var r = new RegExp('[?&]' + n + '(=([^&#]*)|&|#|$)'),
    i = r.exec(t);
    return i ? i[2] ? decodeURIComponent(i[2].replace(/\+/g, ' ')) : '' : null
  },
  r = function (t) {
    var i,
    o;
    if (typeof t != 'string') return !1;
    t = t.toLowerCase();
    var s = t.split('?'),
    f = s[0],
    r,
    u = Roblox.AbuseReportPVMeta,
    h = Roblox.DeviceMeta();
    if (
      u.inApp = h.isInApp,
      f &&
      typeof f == 'string' &&
      (r = f.split('abusereport/') [1]),
      r &&
      (
        i = {
          actionName: r,
          id: n('id', t),
          redirectUrl: n('redirecturl', t)
        },
        n('conversationid', t) &&
        (i.conversationId = n('conversationid', t)),
        n('partyguid', t) &&
        (i.partyGuid = n('partyguid', t)),
        i.id &&
        i.redirectUrl
      )
    ) {
      var c = $.param(i),
      e = 'abusereport/embedded/' + r + '?' + c,
      l = Roblox.Endpoints.getAbsoluteUrl('/' + e);
      u.inApp ? u.inAppEnabled ? (
        o = {
          urlPath: e,
          feature: 'Abuse Report'
        },
        console.debug('Calling navigateToFeature for Hybrid Overlay'),
        Roblox.Hybrid.Navigation.navigateToFeature(
          o,
          function (n) {
            console.debug('navigateToFeature ---- status:' + n)
          }
        )
      ) : window.location.href = t : window.location.href = u.phoneEnabled ? l : t
    }
  },
  t = function (n) {
    n.preventDefault();
    var t = $(this).attr('href');
    t &&
    r(t)
  },
  u = function () {
    if (!i) {
      $('.abuse-report-modal').click(t);
      $('.messages-container').on('click', '.abuse-report-modal', t);
      $('#AjaxCommentsContainer').on('click', '.abuse-report-modal', t);
      $('#item-context-menu').on('click', '.abuse-report-modal', t);
      $('.GroupWallPane').on('click', '.abuse-report-modal', t);
      $('.group-details-container').on('click', '.abuse-report-modal', t);
      i = !0
    }
  };
  return {
    triggerUrlAction: r,
    initialize: u
  }
}(),
$(document).ready(function () {
  Roblox.AbuseReportDispatcher.initialize()
});
; //Bundle detector
Roblox &&
Roblox.BundleDetector &&
Roblox.BundleDetector.bundleDetected('page');
