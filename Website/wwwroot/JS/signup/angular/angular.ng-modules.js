// angular/angular.ng-modules.js
// Modified: auto-includes chat modules when available, so chat is part of
// the same Angular app as the page (Angular 1.6.3 only allows one bootstrap).
(function() {
    function n(n) {
        function e(n) {
            n && o.push(n)
        }
        for (var o = [n], f = [], s = [], h = ["ng:module", "ng-module", "x-ng-module", "data-ng-module", "ng:modules", "ng-modules", "x-ng-modules", "data-ng-modules"], y = /\sng[:\-]module[s](:\s*([\w\d_]+);?)?\s/, u, r, t, c, a, v, i = 0; i < h.length; i++)
            if (u = h[i], e(document.getElementById(u)), u = u.replace(":", "\\:"), n.querySelectorAll) {
                for (r = n.querySelectorAll("." + u), t = 0; t < r.length; t++) e(r[t]);
                for (r = n.querySelectorAll("." + u + "\\:"), t = 0; t < r.length; t++) e(r[t]);
                for (r = n.querySelectorAll("[" + u + "]"), t = 0; t < r.length; t++) e(r[t])
            } for (i = 0; i < o.length; i++) {
            var n = o[i],
                p = " " + n.className + " ",
                l = y.exec(p);
            if (l) f.push(n), s.push((l[2] || "").replace(/\s+/g, ","));
            else if (n.attributes)
                for (t = 0; t < n.attributes.length; t++) c = n.attributes[t], h.indexOf(c.name) != -1 && (f.push(n), s.push(c.value))
        }
        for (i = 0; i < f.length; i++) {
            a = f[i]; v = s[i].replace(/ /g, "").split(",");
            var hasChat = v.indexOf("chat") !== -1;
            if (!hasChat) {
                try { angular.module("chat"); var hasUiBootstrap = v.indexOf("ui.bootstrap") !== -1; if (hasUiBootstrap) { v.push("chat"); try { angular.module("chatAppTemplates"); v.push("chatAppTemplates"); } catch(e) {} } } catch(e) {}
            }
            try { angular.element(a).injector() || angular.bootstrap(a, v) } catch (e) { console.warn("angular.ng-modules: failed to bootstrap", v, e) }
        }
    }
    angular.element(document).ready(function() {
        n(document)
    })
})();