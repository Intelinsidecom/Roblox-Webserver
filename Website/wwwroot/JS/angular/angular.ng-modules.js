// angular/angular.ng-modules.js
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
        for (i = 0; i < f.length; i++) a = f[i], v = s[i].replace(/ /g, "").split(","), angular.bootstrap(a, v)
    }
    angular.element(document).ready(function() {
        n(document)
    })
})();