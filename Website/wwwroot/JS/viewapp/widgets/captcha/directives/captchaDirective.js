// ~/viewapp/widgets/captcha/directives/captchaDirective.js
"use strict";
captcha.directive("captcha", ["$log", "$parse", "captchaConstants", "captchaInterface", function(n, t, i, r) {
    var u = i.containerId;
    return {
        restrict: "A",
        template: i.template,
        link: function(i, f, e) {
            function s(n, t, e, o) {
                var l = f.find("#" + u),
                    s, c;
                if (i.captchaElem.isVisible = !0, s = function() {
                        h(), angular.isFunction(t) && t()
                    }, c = function() {
                        angular.isFunction(e) && e()
                    }, l[0].children.length > 0) {
                    r.reset(n, s, c, o);
                    return
                }
                r.render(u, n, s, c, o), r.execute()
            }

            function h() {
                var n = f.find("#" + u);
                n.empty(), i.captchaElem.isVisible = !1
            }
            i.captchaElem = {
                isVisible: !1
            };
            var o = t(e.captchaModel)(i);
            i.$watch(function() {
                return o.isActivated
            }, function(r) {
                if (r) {
                    var h = t(e.onCaptchaSuccess)(i) || angular.noop,
                        c = t(e.onCaptchaError)(i) || angular.noop,
                        l = t(e.onCaptchaResponse)(i) || angular.noop,
                        f = t(e.captchaType)(i);
                    if (!f) {
                        n.debug("[Captcha Error] captcha type cannot be empty");
                        return
                    }
                    s(f, h, c, l), o.isActivated = !1
                }
            }, !0)
        }
    }
}]);