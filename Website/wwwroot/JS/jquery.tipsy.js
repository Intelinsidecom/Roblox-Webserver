// jquery.tipsy.js
(function(n) {
    function t(n, t) {
        return typeof n == "function" ? n.call(t) : n
    }

    function r(n) {
        while (n = n.parentNode)
            if (n == document) return !0;
        return !1
    }

    function i(t, i) {
        this.$element = n(t), this.options = i, this.enabled = !0, this.fixTitle()
    }
    i.prototype = {
        show: function() {
            var s = this.getTitle(),
                r;
            if (s && this.enabled) {
                r = this.tip(), r.find(".tipsy-inner")[this.options.html ? "html" : "text"](s), r[0].className = "tipsy", r.remove().css({
                    top: 0,
                    left: 0,
                    visibility: "hidden",
                    display: "block"
                }).prependTo(document.body);
                var i = n.extend({}, this.$element.offset(), {
                        width: this.$element[0].offsetWidth,
                        height: this.$element[0].offsetHeight
                    }),
                    e = r[0].offsetWidth,
                    o = r[0].offsetHeight,
                    f = t(this.options.gravity, this.$element[0]),
                    u;
                switch (f.charAt(0)) {
                    case "n":
                        u = {
                            top: i.top + i.height + this.options.offset,
                            left: i.left + i.width / 2 - e / 2
                        };
                        break;
                    case "s":
                        u = {
                            top: i.top - o - this.options.offset,
                            left: i.left + i.width / 2 - e / 2
                        };
                        break;
                    case "e":
                        u = {
                            top: i.top + i.height / 2 - o / 2,
                            left: i.left - e - this.options.offset
                        };
                        break;
                    case "w":
                        u = {
                            top: i.top + i.height / 2 - o / 2,
                            left: i.left + i.width + this.options.offset
                        }
                }
                f.length == 2 && (u.left = f.charAt(1) == "w" ? i.left + i.width / 2 - 15 : i.left + i.width / 2 - e + 15), r.css(u).addClass("tipsy-" + f), r.find(".tipsy-arrow")[0].className = "tipsy-arrow tipsy-arrow-" + f.charAt(0), this.options.className && r.addClass(t(this.options.className, this.$element[0])), this.options.fade ? r.stop().css({
                    opacity: 0,
                    display: "block",
                    visibility: "visible"
                }).animate({
                    opacity: this.options.opacity
                }) : r.css({
                    visibility: "visible",
                    opacity: this.options.opacity
                })
            }
        },
        hide: function() {
            this.options.fade ? this.tip().stop().fadeOut(function() {
                n(this).remove()
            }) : this.tip().remove()
        },
        fixTitle: function() {
            var n = this.$element;
            (n.attr("title") || typeof n.attr("original-title") != "string") && n.attr("original-title", n.attr("title") || "").removeAttr("title")
        },
        getTitle: function() {
            var i = this.$element,
                n = this.options,
                t;
            return this.fixTitle(), n = this.options, typeof n.title == "string" ? t = i.attr(n.title == "title" ? "original-title" : n.title) : typeof n.title == "function" && (t = n.title.call(i[0])), t = ("" + t).replace(/(^\s*|\s*$)/, ""), t || n.fallback
        },
        tip: function() {
            return this.$tip || (this.$tip = n('<div class="tipsy"></div>').html('<div class="tipsy-arrow"></div><div class="tipsy-inner"></div>'), this.$tip.data("tipsy-pointee", this.$element[0])), this.$tip
        },
        validate: function() {
            this.$element[0].parentNode || (this.hide(), this.$element = null, this.options = null)
        },
        enable: function() {
            this.enabled = !0
        },
        disable: function() {
            this.enabled = !1
        },
        toggleEnabled: function() {
            this.enabled = !this.enabled
        }
    }, n.fn.tipsy = function(t) {
        function u(r) {
            var u = n.data(r, "tipsy");
            return u || (u = new i(r, n.fn.tipsy.elementOptions(r, t)), n.data(r, "tipsy", u)), u
        }

        function e() {
            var n = u(this);
            n.hoverState = "in", t.delayIn == 0 ? n.show() : (n.fixTitle(), setTimeout(function() {
                n.hoverState == "in" && n.show()
            }, t.delayIn))
        }

        function o() {
            var n = u(this);
            n.hoverState = "out", t.delayOut == 0 ? n.hide() : setTimeout(function() {
                n.hoverState == "out" && n.hide()
            }, t.delayOut)
        }
        var r;
        if (t === !0) return this.data("tipsy");
        if (typeof t == "string") return r = this.data("tipsy"), r && r[t](), this;
        if (t = n.extend({}, n.fn.tipsy.defaults, t), t.live || this.each(function() {
                u(this)
            }), t.trigger != "manual") {
            var f = t.live ? "live" : "bind",
                s = t.trigger == "hover" ? "mouseenter" : "focus",
                h = t.trigger == "hover" ? "mouseleave" : "blur";
            this[f](s, e)[f](h, o)
        }
        return this
    }, n.fn.tipsy.defaults = {
        className: null,
        delayIn: 0,
        delayOut: 0,
        fade: !1,
        fallback: "",
        gravity: "n",
        html: !1,
        live: !1,
        offset: 0,
        opacity: .8,
        title: "title",
        trigger: "hover"
    }, n.fn.tipsy.revalidate = function() {
        n(".tipsy").each(function() {
            var t = n.data(this, "tipsy-pointee");
            t && r(t) || n(this).remove()
        })
    }, n.fn.tipsy.elementOptions = function(t, i) {
        return n.metadata ? n.extend({}, i, n(t).metadata()) : i
    }, n.fn.tipsy.autoNS = function() {
        return n(this).offset().top > n(document).scrollTop() + n(window).height() / 2 ? "s" : "n"
    }, n.fn.tipsy.autoWE = function() {
        return n(this).offset().left > n(document).scrollLeft() + n(window).width() / 2 ? "e" : "w"
    }, n.fn.tipsy.autoBounds = function(t, i) {
        return function() {
            var r = {
                    ns: i[0],
                    ew: i.length > 1 ? i[1] : !1
                },
                f = n(document).scrollTop() + t,
                e = n(document).scrollLeft() + t,
                u = n(this);
            return u.offset().top < f && (r.ns = "n"), u.offset().left < e && (r.ew = "w"), n(window).width() + n(document).scrollLeft() - u.offset().left < t && (r.ew = "e"), n(window).height() + n(document).scrollTop() - u.offset().top < t && (r.ns = "s"), r.ns + (r.ew ? r.ew : "")
        }
    }
})(jQuery);