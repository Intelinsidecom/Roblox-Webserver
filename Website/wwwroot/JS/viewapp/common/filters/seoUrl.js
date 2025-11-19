// ~/viewapp/common/filters/seoUrl.js
"use strict";
robloxApp.filter("seoUrl", function() {
    return function(n, t, i) {
        typeof i != "string" && (i = "");
        var u = i.replace(/'/g, "").replace(/[^a-zA-Z0-9]+/g, "-").replace(/^-+|-+$/g, "") || "unnamed",
            r = "/" + n + "/" + t + "/" + u;
        return !Roblox || !Roblox.Endpoints ? r : Roblox.Endpoints.getAbsoluteUrl(r)
    }
});