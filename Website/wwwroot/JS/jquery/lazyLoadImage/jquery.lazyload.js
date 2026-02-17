// jquery/lazyLoadImage/jquery.lazyload.js
/*!
 * Lazy Load - jQuery plugin for lazy loading images
 *
 * Copyright (c) 2007-2015 Mika Tuupola
 *
 * Licensed under the MIT license:
 *   http://www.opensource.org/licenses/mit-license.php
 *
 * Project home:
 *   http://www.appelsiini.net/projects/lazyload
 *
 * Version:  1.9.7
 *
 */
(function(n, t, i, r) {
    var u = n(t);
    n.fn.lazyload = function(f) {
        function s() {
            var t = 0;
            o.each(function() {
                var i = n(this);
                if ((!e.skip_invisible || i.is(":visible")) && !n.abovethetop(this, e) && !n.leftofbegin(this, e))
                    if (n.belowthefold(this, e) || n.rightoffold(this, e)) {
                        if (++t > e.failure_limit) return !1
                    } else i.trigger("appear"), t = 0
            })
        }
        var o = this,
            h, e = {
                threshold: 0,
                failure_limit: 0,
                event: "scroll",
                effect: "show",
                container: t,
                data_attribute: "original",
                skip_invisible: !1,
                appear: null,
                load: null,
                placeholder: "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsQAAA7EAZUrDhsAAAANSURBVBhXYzh8+PB/AAffA0nNPuCLAAAAAElFTkSuQmCC",
                loading_class: ""
            };
        return f && (r !== f.failurelimit && (f.failure_limit = f.failurelimit, delete f.failurelimit), r !== f.effectspeed && (f.effect_speed = f.effectspeed, delete f.effectspeed), n.extend(e, f)), h = e.container === r || e.container === t ? u : n(e.container), 0 === e.event.indexOf("scroll") && h.bind(e.event, function() {
            return s()
        }), this.each(function() {
            var i = this,
                t = n(i);
            i.loaded = !1, (t.attr("src") === r || t.attr("src") === !1) && (t.is("img") && e.loading_class.length < 1 && t.attr("src", e.placeholder), e.loading_class.length >= 1 && t.addClass(e.loading_class));
            t.one("appear", function() {
                if (!this.loaded) {
                    if (e.appear) {
                        var r = o.length;
                        e.appear.call(i, r, e)
                    }
                    n("<img />").bind("load", function() {
                        var r = t.attr("data-" + e.data_attribute),
                            u, f;
                        t.hide(), t.is("img") ? t.attr("src", r) : t.css("background-image", "url('" + r + "')"), t.removeClass(e.loading_class), t[e.effect](e.effect_speed), i.loaded = !0, u = n.grep(o, function(n) {
                            return !n.loaded
                        }), o = n(u), e.load && (f = o.length, e.load.call(i, f, e))
                    }).attr("src", t.attr("data-" + e.data_attribute))
                }
            });
            0 !== e.event.indexOf("scroll") && t.bind(e.event, function() {
                i.loaded || t.trigger("appear")
            })
        }), u.bind("resize", function() {
            s()
        }), /(?:iphone|ipod|ipad).*os 5/gi.test(navigator.appVersion) && u.bind("pageshow", function(t) {
            t.originalEvent && t.originalEvent.persisted && o.each(function() {
                n(this).trigger("appear")
            })
        }), n(i).ready(function() {
            s()
        }), this
    }, n.belowthefold = function(i, f) {
        var e;
        return e = f.container === r || f.container === t ? (t.innerHeight ? t.innerHeight : u.height()) + u.scrollTop() : n(f.container).offset().top + n(f.container).height(), e <= n(i).offset().top - f.threshold
    }, n.rightoffold = function(i, f) {
        var e;
        return e = f.container === r || f.container === t ? u.width() + u.scrollLeft() : n(f.container).offset().left + n(f.container).width(), e <= n(i).offset().left - f.threshold
    }, n.abovethetop = function(i, f) {
        var e;
        return e = f.container === r || f.container === t ? u.scrollTop() : n(f.container).offset().top, e >= n(i).offset().top + f.threshold + n(i).height()
    }, n.leftofbegin = function(i, f) {
        var e;
        return e = f.container === r || f.container === t ? u.scrollLeft() : n(f.container).offset().left, e >= n(i).offset().left + f.threshold + n(i).width()
    }, n.inviewport = function(t, i) {
        return !n.rightoffold(t, i) && !n.leftofbegin(t, i) && !n.belowthefold(t, i) && !n.abovethetop(t, i)
    }, n.extend(n.expr[":"], {
        "below-the-fold": function(t) {
            return n.belowthefold(t, {
                threshold: 0
            })
        },
        "above-the-top": function(t) {
            return !n.belowthefold(t, {
                threshold: 0
            })
        },
        "right-of-screen": function(t) {
            return n.rightoffold(t, {
                threshold: 0
            })
        },
        "left-of-screen": function(t) {
            return !n.rightoffold(t, {
                threshold: 0
            })
        },
        "in-viewport": function(t) {
            return n.inviewport(t, {
                threshold: 0
            })
        },
        "above-the-fold": function(t) {
            return !n.belowthefold(t, {
                threshold: 0
            })
        },
        "right-of-fold": function(t) {
            return n.rightoffold(t, {
                threshold: 0
            })
        },
        "left-of-fold": function(t) {
            return !n.rightoffold(t, {
                threshold: 0
            })
        }
    })
})(jQuery, window, document);