// XsrfToken.js
typeof Roblox == "undefined" && (Roblox = {}), Roblox.XsrfToken = function() {
    function f(t) {
        n = t
    }

    function e() {
        return n
    }
    var r = ["POST", "PUT", "DELETE"],
        n = "",
        t = "X-CSRF-TOKEN",
        u = 403,
        i;
    return $(document).ajaxSend(function(t, i, u) {
        n !== "" && r.indexOf(u.type.toUpperCase()) >= 0 && i.setRequestHeader("X-CSRF-TOKEN", n)
    }), $.ajaxPrefilter(function(i) {
        if (i.dataType != "jsonp" && i.dataType != "script" && n !== "") {
            var e = i.error;
            i.error = function(r, f, o) {
                if (r.status == u && r.getResponseHeader(t) != null) {
                    var s = r.getResponseHeader(t);
                    if (s == null) {
                        typeof e == "function" && e(r, f, o);
                        throw new Error("Null token returned by Xsrf enabled handler");
                    }
                    n = s, $.ajax(i)
                } else typeof e == "function" && e(r, f, o)
            }
        }
    }), i = {
        setToken: f,
        getToken: e
    }
}();