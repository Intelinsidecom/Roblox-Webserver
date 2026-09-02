// angular/angular-elastic.js
typeof module != "undefined" && typeof exports != "undefined" && module.exports === exports && (module.exports = "monospaced.elastic"), angular.module("monospaced.elastic", []).constant("msdElasticConfig", {
    append: ""
}).directive("msdElastic", ["$timeout", "$window", "msdElasticConfig", function(n, t, i) {
    "use strict";
    return {
        require: "ngModel",
        restrict: "A, C",
        link: function(r, u, f, e) {
            function nt() {
                var n = b;
                g = o, s = getComputedStyle(o), angular.forEach(ot, function(t) {
                    n += t + ":" + s.getPropertyValue(t) + ";"
                }), h.setAttribute("style", n)
            }

            function y() {
                var u, i, t, f, e;
                g !== o && nt(), a || (a = !0, h.value = o.value + et, h.style.overflowY = o.style.overflowY, u = o.style.height === "" ? "auto" : parseInt(o.style.height, 10), i = getComputedStyle(o).getPropertyValue("width"), i.substr(i.length - 2, 2) === "px" && (f = parseInt(i, 10) - p.width, h.style.width = f + "px"), t = h.scrollHeight, t > c ? (t = c, e = "scroll") : t < k && (t = k), t += p.height, o.style.overflowY = e || "hidden", u !== t && (r.$emit("elastic:resize", v, u, t), o.style.height = t + "px"), n(function() {
                    a = !1
                }, 1))
            }

            function l() {
                a = !1, y()
            }
            var o = u[0],
                v = u,
                it;
            if (o.nodeName === "TEXTAREA" && t.getComputedStyle) {
                v.css({
                    overflow: "hidden",
                    "overflow-y": "hidden",
                    "word-wrap": "break-word"
                }), it = o.value, o.value = "", o.value = it;
                var et = f.msdElastic ? f.msdElastic.replace(/\\n/g, "\n") : i.append,
                    d = angular.element(t),
                    b = "position: absolute; top: -999px; right: auto; bottom: auto;left: 0; overflow: hidden; -webkit-box-sizing: content-box;-moz-box-sizing: content-box; box-sizing: content-box;min-height: 0 !important; height: 0 !important; padding: 0;word-wrap: break-word; border: 0;",
                    tt = angular.element('<textarea aria-hidden="true" tabindex="-1" style="' + b + '"/>').data("elastic", !0),
                    h = tt[0],
                    s = getComputedStyle(o),
                    w = s.getPropertyValue("resize"),
                    rt = s.getPropertyValue("box-sizing") === "border-box" || s.getPropertyValue("-moz-box-sizing") === "border-box" || s.getPropertyValue("-webkit-box-sizing") === "border-box",
                    p = rt ? {
                        width: parseInt(s.getPropertyValue("border-right-width"), 10) + parseInt(s.getPropertyValue("padding-right"), 10) + parseInt(s.getPropertyValue("padding-left"), 10) + parseInt(s.getPropertyValue("border-left-width"), 10),
                        height: parseInt(s.getPropertyValue("border-top-width"), 10) + parseInt(s.getPropertyValue("padding-top"), 10) + parseInt(s.getPropertyValue("padding-bottom"), 10) + parseInt(s.getPropertyValue("border-bottom-width"), 10)
                    } : {
                        width: 0,
                        height: 0
                    },
                    ut = parseInt(s.getPropertyValue("min-height"), 10),
                    ft = parseInt(s.getPropertyValue("height"), 10),
                    k = Math.max(ut, ft) - p.height,
                    c = parseInt(s.getPropertyValue("max-height"), 10),
                    g, a, ot = ["font-family", "font-size", "font-weight", "font-style", "letter-spacing", "line-height", "text-transform", "word-spacing", "text-indent"];
                v.data("elastic") || (c = c && c > 0 ? c : 9e4, h.parentNode !== document.body && angular.element(document.body).append(h), v.css({
                    resize: w === "none" || w === "vertical" ? "none" : "horizontal"
                }).data("elastic", !0), o.oninput = "onpropertychange" in o && "oninput" in o ? o.onkeyup = y : y, d.bind("resize", l), r.$watch(function() {
                    return e.$modelValue
                }, function() {
                    l()
                }), r.$on("elastic:adjust", function() {
                    nt(), l()
                }), n(y), r.$on("$destroy", function() {
                    tt.remove(), d.unbind("resize", l)
                }))
            }
        }
    }
}]);