// JavaScriptEndpoints.js
typeof Roblox == typeof undefined && (Roblox = {}), Roblox.Endpoints = Roblox.Endpoints || {
    addCrossDomainOptionsToAllRequests: !1
}, Roblox.Endpoints.isAbsolute = function(n) {
    var t = new RegExp("^([a-z]+://|//)");
    return t.test(n)
}, Roblox.Endpoints.splitAtQueryString = function(n) {
    var i = new RegExp("\\?(?!})"),
        t = i.exec(n);
    return t === null ? {
        url: n,
        query: ""
    } : {
        url: n.substring(0, t.index),
        query: n.substring(t.index)
    }
}, Roblox.Endpoints.ajaxPrefilter = function(n) {
    var r = Roblox.Endpoints.generateAbsoluteUrl(n.url, n.data, n.crossDomain);
    n.url = r, Roblox.Endpoints.addCrossDomainOptionsToAllRequests && n.url.indexOf("rbxcdn.com") < 0 && n.url.indexOf("s3.amazonaws.com") < 0 && (n.crossDomain = !0, n.xhrFields = n.xhrFields || {}, n.xhrFields.withCredentials = !0)
}, Roblox.Endpoints.generateAbsoluteUrl = function(n, t, i) {
    var f = Roblox.Endpoints.splitAtQueryString(n),
        u = f.url.toLowerCase(),
        r = u;
    return typeof Roblox.Endpoints.Urls != typeof undefined && i && typeof Roblox.Endpoints.Urls[u.toLowerCase()] != typeof undefined && (r = Roblox.Endpoints.getAbsoluteUrl(u)), r.indexOf("{") > -1 && $.each(t, function(n, t) {
        var i = new RegExp("{" + n.toLowerCase() + "(:.*?)?\\??}");
        r = r.replace(i, t)
    }), r + f.query
}, Roblox.Endpoints.getAbsoluteUrl = function(n) {
    var t, r, i, u;
    return typeof Roblox.Endpoints.Urls == typeof undefined ? n : n.length === 0 || Roblox.Endpoints.isAbsolute(n) ? n : (n.indexOf("/") !== 0 && (t = window.location.pathname, r = t.slice(0, t.lastIndexOf("/") + 1), n = r + n), i = Roblox.Endpoints.Urls[n.toLowerCase()], i === undefined) ? (u = window.location.protocol + "//" + window.location.hostname, u + n) : i
},
// Minimal catalog URL helper used by legacy avatar JS. The original site
// supported nice SEO slugs based on name; for this recreation we only need
// a stable link that points at the right asset id.
Roblox.Endpoints.getCatalogItemUrl = function(id, name) {
    try {
        // Prefer whatever base URL is configured, if present
        var path = "/catalog/" + id;
        return Roblox.Endpoints.generateAbsoluteUrl(path);
    } catch (e) {
        // Fall back to a simple relative URL if anything goes wrong
        return "/catalog/" + id;
    }
}, $.ajaxPrefilter(Roblox.Endpoints.ajaxPrefilter);