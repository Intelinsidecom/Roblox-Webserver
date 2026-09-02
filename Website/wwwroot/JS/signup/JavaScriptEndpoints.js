// JavaScriptEndpoints.js
typeof Freebloxia == typeof undefined && (Freebloxia = {}), Freebloxia.Endpoints = Freebloxia.Endpoints || {
    addCrossDomainOptionsToAllRequests: !1
}, Freebloxia.Endpoints.isAbsolute = function(n) {
    var t = new RegExp("^([a-z]+://|//)");
    return t.test(n)
}, Freebloxia.Endpoints.splitAtQueryString = function(n) {
    var i = new RegExp("\\?(?!})"),
        t = i.exec(n);
    return t === null ? {
        url: n,
        query: ""
    } : {
        url: n.substring(0, t.index),
        query: n.substring(t.index)
    }
}, Freebloxia.Endpoints.ajaxPrefilter = function(n) {
    var r = Freebloxia.Endpoints.generateAbsoluteUrl(n.url, n.data, n.crossDomain);
    n.url = r, Freebloxia.Endpoints.addCrossDomainOptionsToAllRequests && n.url.indexOf("rbxcdn.com") < 0 && n.url.indexOf("s3.amazonaws.com") < 0 && (n.crossDomain = !0, n.xhrFields = n.xhrFields || {}, n.xhrFields.withCredentials = !0)
}, Freebloxia.Endpoints.generateAbsoluteUrl = function(n, t, i) {
    var f = Freebloxia.Endpoints.splitAtQueryString(n),
        u = f.url.toLowerCase(),
        r = u;
    return typeof Freebloxia.Endpoints.Urls != typeof undefined && i && typeof Freebloxia.Endpoints.Urls[u.toLowerCase()] != typeof undefined && (r = Freebloxia.Endpoints.getAbsoluteUrl(u)), r.indexOf("{") > -1 && $.each(t, function(n, t) {
        var i = new RegExp("{" + n.toLowerCase() + "(:.*?)?\\??}");
        r = r.replace(i, t)
    }), r + f.query
}, Freebloxia.Endpoints.getAbsoluteUrl = function(n) {
    var t, r, i, u;
    return typeof Freebloxia.Endpoints.Urls == typeof undefined ? n : n.length === 0 || Freebloxia.Endpoints.isAbsolute(n) ? n : (n.indexOf("/") !== 0 && (t = window.location.pathname, r = t.slice(0, t.lastIndexOf("/") + 1), n = r + n), i = Freebloxia.Endpoints.Urls[n.toLowerCase()], i === undefined) ? (u = window.location.protocol + "//" + window.location.hostname, u + n) : i
},
// Minimal catalog URL helper used by legacy avatar JS. The original site
// supported nice SEO slugs based on name; for this recreation we only need
// a stable link that points at the right asset id.
Freebloxia.Endpoints.getCatalogItemUrl = function(id, name) {
    try {
        // Prefer whatever base URL is configured, if present
        var path = "/catalog/" + id;
        return Freebloxia.Endpoints.generateAbsoluteUrl(path);
    } catch (e) {
        // Fall back to a simple relative URL if anything goes wrong
        return "/catalog/" + id;
    }
}, $.ajaxPrefilter(Freebloxia.Endpoints.ajaxPrefilter);