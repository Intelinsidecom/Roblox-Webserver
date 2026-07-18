// ~/viewapp/widgets/lazyLoadImage/angularLazyImg.js
angular.module("angularLazyImg", []).factory("LazyImgMagic", ["$window", "$rootScope", "lazyImgConfig", "lazyImgHelpers", function(n, t, i, r) {
    "use strict";

    function v() {
        for (var t, n = f.length - 1; n >= 0; n--) t = f[n], t && r.isElementInView(t.$elem[0], u.offset, a) && (d(t), f.splice(n, 1));
        f.length === 0 && h()
    }

    function w(n) {
        o.forEach(function(t) {
            t[n]("scroll", l), t[n]("touchmove", l)
        }), c[n]("resize", l), c[n]("resize", p)
    }

    function y() {
        s = !0, setTimeout(function() {
            v(), w("on")
        }, 1)
    }

    function h() {
        s = !1, w("off")
    }

    function k(n) {
        var t = f.indexOf(n);
        t !== -1 && f.splice(t, 1)
    }

    function d(n) {
        var i = new Image;
        i.onerror = function() {
            u.errorClass && n.$elem.addClass(u.errorClass), t.$emit("lazyImg:error", n);
            u.onError(n)
        }, i.onload = function() {
            b(n.$elem, n.src), n.$elem.removeClass(u.loadingClass), u.successClass && n.$elem.addClass(u.successClass), t.$emit("lazyImg:success", n);
            u.onSuccess(n)
        }, i.src = n.src
    }

    function b(n, t) {
        n[0].nodeName.toLowerCase() === "img" ? n[0].src = t : n.css("background-image", 'url("' + t + '")')
    }

    function e(n) {
        n.addClass(u.loadingClass), this.$elem = n
    }
    var a, c, f, s, u, l, p, o;
    return f = [], s = !1, u = i.getOptions(), c = angular.element(n), a = r.getWinDimensions(), p = r.throttle(function() {
        a = r.getWinDimensions()
    }, 60), o = [u.container || c], l = r.throttle(v, 30), e.prototype.setSource = function(n) {
        this.src = n, f.unshift(this), s || y()
    }, e.prototype.removeImage = function() {
        k(this), f.length === 0 && h()
    }, e.prototype.checkImages = function() {
        v()
    }, e.addContainer = function(n) {
        h(), o.push(n), y()
    }, e.removeContainer = function(n) {
        h(), o.splice(o.indexOf(n), 1), y()
    }, e
}]).provider("lazyImgConfig", function() {
    "use strict";
    this.options = {
        offset: 100,
        errorClass: null,
        successClass: null,
        onError: function() {},
        onSuccess: function() {},
        loadingClass: "thumbnail-placeholder"
    }, this.$get = function() {
        var n = this.options;
        return {
            getOptions: function() {
                return n
            }
        }
    }, this.setOptions = function(n) {
        angular.extend(this.options, n)
    }
}).factory("lazyImgHelpers", ["$window", function(n) {
    "use strict";

    function t() {
        return {
            height: n.innerHeight,
            width: n.innerWidth
        }
    }

    function i(n, t, i) {
        var r = n.getBoundingClientRect(),
            u = i.height + t;
        return n.offsetParent && r.left >= 0 && r.right <= i.width + t && (r.top >= 0 && r.top <= u || r.bottom <= u && r.bottom >= 0 - t)
    }

    function r(n, t, i) {
        var r, u;
        return function() {
            var e = i || this,
                f = +new Date,
                o = arguments;
            r && f < r + t ? (clearTimeout(u), u = setTimeout(function() {
                r = f, n.apply(e, o)
            }, t)) : (r = f, n.apply(e, o))
        }
    }
    return {
        isElementInView: i,
        getWinDimensions: t,
        throttle: r
    }
}]).directive("lazyImg", ["$rootScope", "LazyImgMagic", function(n, t) {
    "use strict";

    function i(i, r, u) {
        var f = new t(r);
        u.$observe("lazyImg", function(n) {
            n && f.setSource(n)
        }), i.$on("$destroy", function() {
            f.removeImage()
        }), n.$on("lazyImg.runCheck", function() {
            f.checkImages()
        }), n.$on("lazyImg:refresh", function() {
            f.checkImages()
        })
    }
    return {
        link: i,
        restrict: "A"
    }
}]).directive("lazyImgContainer", ["LazyImgMagic", function(n) {
    "use strict";

    function t(t, i) {
        n.addContainer(i), t.$on("$destroy", function() {
            n.removeContainer(i)
        })
    }
    return {
        link: t,
        restrict: "A"
    }
}]);