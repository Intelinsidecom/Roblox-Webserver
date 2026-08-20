;// bundle: LayoutShared
;// files: JS/leancore/libs/bootstrap.min.js, JS/widgets/jquery.mCustomScrollbar.concat.min.js, JS/jquery.cookie.js, JS/leancore/libs/underscore-min.js, JS/angular/angular.min.js, JS/angular/angular-sanitize.min.js, JS/angular/angular-ui-router.min.js, JS/angular/ui-bootstrap-0.11.2.js, JS/angular/ui-bootstrap-custom-tpls-2.5.0.min.js, JS/templateApp.js, JS/viewapp/common/services/robloxService.js, JS/viewapp/common/services/httpService.js, JS/viewapp/common/services/urlService.js, JS/viewapp/common/services/userService.js, JS/viewapp/common/services/eventStreamService.js, JS/viewapp/common/services/hybridService.js, JS/viewapp/common/services/chatDispatchService.js, JS/viewapp/common/filters.js, JS/viewapp/app.js, JS/viewapp/common/providers/languageResourceProvider.js, JS/viewapp/common/filters/translate.js, JS/Reference/widget.js, JS/modules/Pages/Catalog.js, JS/modules/Pages/CatalogShared.js, JS/modules/Widgets/AvatarImage.js, JS/modules/Widgets/DropdownMenu.js, JS/modules/Widgets/GroupImage.js, JS/modules/Widgets/HierarchicalDropdown.js, JS/modules/Widgets/ItemImage.js, JS/modules/Widgets/PlaceImage.js, JS/modules/Widgets/SurveyModal.js, JS/iFrameLogin.js, JS/viewapp/common/providers/languageResourceProvider.js

;// JS/leancore/libs/bootstrap.min.js
// leancore/libs/bootstrap.min.js
/*!
 * Bootstrap v3.3.5 (http://getbootstrap.com)
 * Copyright 2011-2015 Twitter, Inc.
 * Licensed under MIT (https://github.com/twbs/bootstrap/blob/master/LICENSE)
 */

/*!
 * Generated using the Bootstrap Customizer (http://getbootstrap.com/customize/?id=b8480475c6c7b955a207)
 * Config saved to config.json and https://gist.github.com/b8480475c6c7b955a207
 */
if ("undefined" == typeof jQuery) throw new Error("Bootstrap's JavaScript requires jQuery"); + function(t) {
    "use strict";
    var e = t.fn.jquery.split(" ")[0].split(".");
    if (e[0] < 2 && e[1] < 9 || 1 == e[0] && 9 == e[1] && e[2] < 1) throw new Error("Bootstrap's JavaScript requires jQuery version 1.9.1 or higher")
}(jQuery), + function(t) {
    "use strict";

    function e(e) {
        return this.each(function() {
            var i = t(this),
                n = i.data("bs.alert");
            n || i.data("bs.alert", n = new o(this)), "string" == typeof e && n[e].call(i)
        })
    }
    var i = '[data-dismiss="alert"]',
        o = function(e) {
            t(e).on("click", i, this.close)
        };
    o.VERSION = "3.3.5", o.TRANSITION_DURATION = 150, o.prototype.close = function(e) {
        function i() {
            a.detach().trigger("closed.bs.alert").remove()
        }
        var n = t(this),
            s = n.attr("data-target");
        s || (s = n.attr("href"), s = s && s.replace(/.*(?=#[^\s]*$)/, ""));
        var a = t(s);
        e && e.preventDefault(), a.length || (a = n.closest(".alert")), a.trigger(e = t.Event("close.bs.alert")), e.isDefaultPrevented() || (a.removeClass("in"), t.support.transition && a.hasClass("fade") ? a.one("bsTransitionEnd", i).emulateTransitionEnd(o.TRANSITION_DURATION) : i())
    };
    var n = t.fn.alert;
    t.fn.alert = e, t.fn.alert.Constructor = o, t.fn.alert.noConflict = function() {
        return t.fn.alert = n, this
    }, t(document).on("click.bs.alert.data-api", i, o.prototype.close)
}(jQuery), + function(t) {
    "use strict";

    function e(e) {
        return this.each(function() {
            var o = t(this),
                n = o.data("bs.button"),
                s = "object" == typeof e && e;
            n || o.data("bs.button", n = new i(this, s)), "toggle" == e ? n.toggle() : e && n.setState(e)
        })
    }
    var i = function(e, o) {
        this.$element = t(e), this.options = t.extend({}, i.DEFAULTS, o), this.isLoading = !1
    };
    i.VERSION = "3.3.5", i.DEFAULTS = {
        loadingText: "loading..."
    }, i.prototype.setState = function(e) {
        var i = "disabled",
            o = this.$element,
            n = o.is("input") ? "val" : "html",
            s = o.data();
        e += "Text", null == s.resetText && o.data("resetText", o[n]()), setTimeout(t.proxy(function() {
            o[n](null == s[e] ? this.options[e] : s[e]), "loadingText" == e ? (this.isLoading = !0, o.addClass(i).attr(i, i)) : this.isLoading && (this.isLoading = !1, o.removeClass(i).removeAttr(i))
        }, this), 0)
    }, i.prototype.toggle = function() {
        var t = !0,
            e = this.$element.closest('[data-toggle="buttons"]');
        if (e.length) {
            var i = this.$element.find("input");
            "radio" == i.prop("type") ? (i.prop("checked") && (t = !1), e.find(".active").removeClass("active"), this.$element.addClass("active")) : "checkbox" == i.prop("type") && (i.prop("checked") !== this.$element.hasClass("active") && (t = !1), this.$element.toggleClass("active")), i.prop("checked", this.$element.hasClass("active")), t && i.trigger("change")
        } else this.$element.attr("aria-pressed", !this.$element.hasClass("active")), this.$element.toggleClass("active")
    };
    var o = t.fn.button;
    t.fn.button = e, t.fn.button.Constructor = i, t.fn.button.noConflict = function() {
        return t.fn.button = o, this
    }, t(document).on("click.bs.button.data-api", '[data-toggle^="button"]', function(i) {
        var o = t(i.target);
        o.hasClass("btn") || (o = o.closest(".btn")), e.call(o, "toggle"), t(i.target).is('input[type="radio"]') || t(i.target).is('input[type="checkbox"]') || i.preventDefault()
    }).on("focus.bs.button.data-api blur.bs.button.data-api", '[data-toggle^="button"]', function(e) {
        t(e.target).closest(".btn").toggleClass("focus", /^focus(in)?$/.test(e.type))
    })
}(jQuery), + function(t) {
    "use strict";

    function e(e) {
        return this.each(function() {
            var o = t(this),
                n = o.data("bs.carousel"),
                s = t.extend({}, i.DEFAULTS, o.data(), "object" == typeof e && e),
                a = "string" == typeof e ? e : s.slide;
            n || o.data("bs.carousel", n = new i(this, s)), "number" == typeof e ? n.to(e) : a ? n[a]() : s.interval && n.pause().cycle()
        })
    }
    var i = function(e, i) {
        this.$element = t(e), this.$indicators = this.$element.find(".carousel-indicators"), this.options = i, this.paused = null, this.sliding = null, this.interval = null, this.$active = null, this.$items = null, this.options.keyboard && this.$element.on("keydown.bs.carousel", t.proxy(this.keydown, this)), "hover" == this.options.pause && !("ontouchstart" in document.documentElement) && this.$element.on("mouseenter.bs.carousel", t.proxy(this.pause, this)).on("mouseleave.bs.carousel", t.proxy(this.cycle, this))
    };
    i.VERSION = "3.3.5", i.TRANSITION_DURATION = 600, i.DEFAULTS = {
        interval: 5e3,
        pause: "hover",
        wrap: !0,
        keyboard: !0
    }, i.prototype.keydown = function(t) {
        if (!/input|textarea/i.test(t.target.tagName)) {
            switch (t.which) {
                case 37:
                    this.prev();
                    break;
                case 39:
                    this.next();
                    break;
                default:
                    return
            }
            t.preventDefault()
        }
    }, i.prototype.cycle = function(e) {
        return e || (this.paused = !1), this.interval && clearInterval(this.interval), this.options.interval && !this.paused && (this.interval = setInterval(t.proxy(this.next, this), this.options.interval)), this
    }, i.prototype.getItemIndex = function(t) {
        return this.$items = t.parent().children(".item"), this.$items.index(t || this.$active)
    }, i.prototype.getItemForDirection = function(t, e) {
        var i = this.getItemIndex(e),
            o = "prev" == t && 0 === i || "next" == t && i == this.$items.length - 1;
        if (o && !this.options.wrap) return e;
        var n = "prev" == t ? -1 : 1,
            s = (i + n) % this.$items.length;
        return this.$items.eq(s)
    }, i.prototype.to = function(t) {
        var e = this,
            i = this.getItemIndex(this.$active = this.$element.find(".item.active"));
        return t > this.$items.length - 1 || 0 > t ? void 0 : this.sliding ? this.$element.one("slid.bs.carousel", function() {
            e.to(t)
        }) : i == t ? this.pause().cycle() : this.slide(t > i ? "next" : "prev", this.$items.eq(t))
    }, i.prototype.pause = function(e) {
        return e || (this.paused = !0), this.$element.find(".next, .prev").length && t.support.transition && (this.$element.trigger(t.support.transition.end), this.cycle(!0)), this.interval = clearInterval(this.interval), this
    }, i.prototype.next = function() {
        return this.sliding ? void 0 : this.slide("next")
    }, i.prototype.prev = function() {
        return this.sliding ? void 0 : this.slide("prev")
    }, i.prototype.slide = function(e, o) {
        var n = this.$element.find(".item.active"),
            s = o || this.getItemForDirection(e, n),
            a = this.interval,
            r = "next" == e ? "left" : "right",
            l = this;
        if (s.hasClass("active")) return this.sliding = !1;
        var h = s[0],
            d = t.Event("slide.bs.carousel", {
                relatedTarget: h,
                direction: r
            });
        if (this.$element.trigger(d), !d.isDefaultPrevented()) {
            if (this.sliding = !0, a && this.pause(), this.$indicators.length) {
                this.$indicators.find(".active").removeClass("active");
                var p = t(this.$indicators.children()[this.getItemIndex(s)]);
                p && p.addClass("active")
            }
            var c = t.Event("slid.bs.carousel", {
                relatedTarget: h,
                direction: r
            });
            return t.support.transition && this.$element.hasClass("slide") ? (s.addClass(e), s[0].offsetWidth, n.addClass(r), s.addClass(r), n.one("bsTransitionEnd", function() {
                s.removeClass([e, r].join(" ")).addClass("active"), n.removeClass(["active", r].join(" ")), l.sliding = !1, setTimeout(function() {
                    l.$element.trigger(c)
                }, 0)
            }).emulateTransitionEnd(i.TRANSITION_DURATION)) : (n.removeClass("active"), s.addClass("active"), this.sliding = !1, this.$element.trigger(c)), a && this.cycle(), this
        }
    };
    var o = t.fn.carousel;
    t.fn.carousel = e, t.fn.carousel.Constructor = i, t.fn.carousel.noConflict = function() {
        return t.fn.carousel = o, this
    };
    var n = function(i) {
        var o, n = t(this),
            s = t(n.attr("data-target") || (o = n.attr("href")) && o.replace(/.*(?=#[^\s]+$)/, ""));
        if (s.hasClass("carousel")) {
            var a = t.extend({}, s.data(), n.data()),
                r = n.attr("data-slide-to");
            r && (a.interval = !1), e.call(s, a), r && s.data("bs.carousel").to(r), i.preventDefault()
        }
    };
    t(document).on("click.bs.carousel.data-api", "[data-slide]", n).on("click.bs.carousel.data-api", "[data-slide-to]", n), t(window).on("load", function() {
        t('[data-ride="carousel"]').each(function() {
            var i = t(this);
            e.call(i, i.data())
        })
    })
}(jQuery), + function(t) {
    "use strict";

    function e(e) {
        var i = e.attr("data-target");
        i || (i = e.attr("href"), i = i && /#[A-Za-z]/.test(i) && i.replace(/.*(?=#[^\s]*$)/, ""));
        var o = i && t(i);
        return o && o.length ? o : e.parent()
    }

    function i(i) {
        i && 3 === i.which || (t(n).remove(), t(s).each(function() {
            var o = t(this),
                n = e(o),
                s = {
                    relatedTarget: this
                };
            n.hasClass("open") && (i && "click" == i.type && /input|textarea/i.test(i.target.tagName) && t.contains(n[0], i.target) || (n.trigger(i = t.Event("hide.bs.dropdown", s)), i.isDefaultPrevented() || (o.attr("aria-expanded", "false"), n.removeClass("open").trigger("hidden.bs.dropdown", s))))
        }))
    }

    function o(e) {
        return this.each(function() {
            var i = t(this),
                o = i.data("bs.dropdown");
            o || i.data("bs.dropdown", o = new a(this)), "string" == typeof e && o[e].call(i)
        })
    }
    var n = ".dropdown-backdrop",
        s = '[data-toggle="dropdown"]',
        a = function(e) {
            t(e).on("click.bs.dropdown", this.toggle)
        };
    a.VERSION = "3.3.5", a.prototype.toggle = function(o) {
        var n = t(this);
        if (!n.is(".disabled, :disabled")) {
            var s = e(n),
                a = s.hasClass("open");
            if (i(), !a) {
                "ontouchstart" in document.documentElement && !s.closest(".navbar-nav").length && t(document.createElement("div")).addClass("dropdown-backdrop").insertAfter(t(this)).on("click", i);
                var r = {
                    relatedTarget: this
                };
                if (s.trigger(o = t.Event("show.bs.dropdown", r)), o.isDefaultPrevented()) return;
                n.trigger("focus").attr("aria-expanded", "true"), s.toggleClass("open").trigger("shown.bs.dropdown", r)
            }
            return !1
        }
    }, a.prototype.keydown = function(i) {
        if (/(38|40|27|32)/.test(i.which) && !/input|textarea/i.test(i.target.tagName)) {
            var o = t(this);
            if (i.preventDefault(), i.stopPropagation(), !o.is(".disabled, :disabled")) {
                var n = e(o),
                    a = n.hasClass("open");
                if (!a && 27 != i.which || a && 27 == i.which) return 27 == i.which && n.find(s).trigger("focus"), o.trigger("click");
                var r = " li:not(.disabled):visible a",
                    l = n.find(".dropdown-menu" + r);
                if (l.length) {
                    var h = l.index(i.target);
                    38 == i.which && h > 0 && h--, 40 == i.which && h < l.length - 1 && h++, ~h || (h = 0), l.eq(h).trigger("focus")
                }
            }
        }
    };
    var r = t.fn.dropdown;
    t.fn.dropdown = o, t.fn.dropdown.Constructor = a, t.fn.dropdown.noConflict = function() {
        return t.fn.dropdown = r, this
    }, t(document).on("click.bs.dropdown.data-api", i).on("click.bs.dropdown.data-api", ".dropdown form", function(t) {
        t.stopPropagation()
    }).on("click.bs.dropdown.data-api", s, a.prototype.toggle).on("keydown.bs.dropdown.data-api", s, a.prototype.keydown).on("keydown.bs.dropdown.data-api", ".dropdown-menu", a.prototype.keydown)
}(jQuery), + function(t) {
    "use strict";

    function e(e, o) {
        return this.each(function() {
            var n = t(this),
                s = n.data("bs.modal"),
                a = t.extend({}, i.DEFAULTS, n.data(), "object" == typeof e && e);
            s || n.data("bs.modal", s = new i(this, a)), "string" == typeof e ? s[e](o) : a.show && s.show(o)
        })
    }
    var i = function(e, i) {
        this.options = i, this.$body = t(document.body), this.$element = t(e), this.$dialog = this.$element.find(".modal-dialog"), this.$backdrop = null, this.isShown = null, this.originalBodyPad = null, this.scrollbarWidth = 0, this.ignoreBackdropClick = !1, this.options.remote && this.$element.find(".modal-content").load(this.options.remote, t.proxy(function() {
            this.$element.trigger("loaded.bs.modal")
        }, this))
    };
    i.VERSION = "3.3.5", i.TRANSITION_DURATION = 300, i.BACKDROP_TRANSITION_DURATION = 150, i.DEFAULTS = {
        backdrop: !0,
        keyboard: !0,
        show: !0
    }, i.prototype.toggle = function(t) {
        return this.isShown ? this.hide() : this.show(t)
    }, i.prototype.show = function(e) {
        var o = this,
            n = t.Event("show.bs.modal", {
                relatedTarget: e
            });
        this.$element.trigger(n), this.isShown || n.isDefaultPrevented() || (this.isShown = !0, this.checkScrollbar(), this.setScrollbar(), this.$body.addClass("modal-open"), this.escape(), this.resize(), this.$element.on("click.dismiss.bs.modal", '[data-dismiss="modal"]', t.proxy(this.hide, this)), this.$dialog.on("mousedown.dismiss.bs.modal", function() {
            o.$element.one("mouseup.dismiss.bs.modal", function(e) {
                t(e.target).is(o.$element) && (o.ignoreBackdropClick = !0)
            })
        }), this.backdrop(function() {
            var n = t.support.transition && o.$element.hasClass("fade");
            o.$element.parent().length || o.$element.appendTo(o.$body), o.$element.show().scrollTop(0), o.adjustDialog(), n && o.$element[0].offsetWidth, o.$element.addClass("in"), o.enforceFocus();
            var s = t.Event("shown.bs.modal", {
                relatedTarget: e
            });
            n ? o.$dialog.one("bsTransitionEnd", function() {
                o.$element.trigger("focus").trigger(s)
            }).emulateTransitionEnd(i.TRANSITION_DURATION) : o.$element.trigger("focus").trigger(s)
        }))
    }, i.prototype.hide = function(e) {
        e && e.preventDefault(), e = t.Event("hide.bs.modal"), this.$element.trigger(e), this.isShown && !e.isDefaultPrevented() && (this.isShown = !1, this.escape(), this.resize(), t(document).off("focusin.bs.modal"), this.$element.removeClass("in").off("click.dismiss.bs.modal").off("mouseup.dismiss.bs.modal"), this.$dialog.off("mousedown.dismiss.bs.modal"), t.support.transition && this.$element.hasClass("fade") ? this.$element.one("bsTransitionEnd", t.proxy(this.hideModal, this)).emulateTransitionEnd(i.TRANSITION_DURATION) : this.hideModal())
    }, i.prototype.enforceFocus = function() {
        t(document).off("focusin.bs.modal").on("focusin.bs.modal", t.proxy(function(t) {
            this.$element[0] === t.target || this.$element.has(t.target).length || this.$element.trigger("focus")
        }, this))
    }, i.prototype.escape = function() {
        this.isShown && this.options.keyboard ? this.$element.on("keydown.dismiss.bs.modal", t.proxy(function(t) {
            27 == t.which && this.hide()
        }, this)) : this.isShown || this.$element.off("keydown.dismiss.bs.modal")
    }, i.prototype.resize = function() {
        this.isShown ? t(window).on("resize.bs.modal", t.proxy(this.handleUpdate, this)) : t(window).off("resize.bs.modal")
    }, i.prototype.hideModal = function() {
        var t = this;
        this.$element.hide(), this.backdrop(function() {
            t.$body.removeClass("modal-open"), t.resetAdjustments(), t.resetScrollbar(), t.$element.trigger("hidden.bs.modal")
        })
    }, i.prototype.removeBackdrop = function() {
        this.$backdrop && this.$backdrop.remove(), this.$backdrop = null
    }, i.prototype.backdrop = function(e) {
        var o = this,
            n = this.$element.hasClass("fade") ? "fade" : "";
        if (this.isShown && this.options.backdrop) {
            var s = t.support.transition && n;
            if (this.$backdrop = t(document.createElement("div")).addClass("modal-backdrop " + n).appendTo(this.$body), this.$element.on("click.dismiss.bs.modal", t.proxy(function(t) {
                    return this.ignoreBackdropClick ? void(this.ignoreBackdropClick = !1) : void(t.target === t.currentTarget && ("static" == this.options.backdrop ? this.$element[0].focus() : this.hide()))
                }, this)), s && this.$backdrop[0].offsetWidth, this.$backdrop.addClass("in"), !e) return;
            s ? this.$backdrop.one("bsTransitionEnd", e).emulateTransitionEnd(i.BACKDROP_TRANSITION_DURATION) : e()
        } else if (!this.isShown && this.$backdrop) {
            this.$backdrop.removeClass("in");
            var a = function() {
                o.removeBackdrop(), e && e()
            };
            t.support.transition && this.$element.hasClass("fade") ? this.$backdrop.one("bsTransitionEnd", a).emulateTransitionEnd(i.BACKDROP_TRANSITION_DURATION) : a()
        } else e && e()
    }, i.prototype.handleUpdate = function() {
        this.adjustDialog()
    }, i.prototype.adjustDialog = function() {
        var t = this.$element[0].scrollHeight > document.documentElement.clientHeight;
        this.$element.css({
            paddingLeft: !this.bodyIsOverflowing && t ? this.scrollbarWidth : "",
            paddingRight: this.bodyIsOverflowing && !t ? this.scrollbarWidth : ""
        })
    }, i.prototype.resetAdjustments = function() {
        this.$element.css({
            paddingLeft: "",
            paddingRight: ""
        })
    }, i.prototype.checkScrollbar = function() {
        var t = window.innerWidth;
        if (!t) {
            var e = document.documentElement.getBoundingClientRect();
            t = e.right - Math.abs(e.left)
        }
        this.bodyIsOverflowing = document.body.clientWidth < t, this.scrollbarWidth = this.measureScrollbar()
    }, i.prototype.setScrollbar = function() {
        var t = parseInt(this.$body.css("padding-right") || 0, 10);
        this.originalBodyPad = document.body.style.paddingRight || "", this.bodyIsOverflowing && this.$body.css("padding-right", t + this.scrollbarWidth)
    }, i.prototype.resetScrollbar = function() {
        this.$body.css("padding-right", this.originalBodyPad)
    }, i.prototype.measureScrollbar = function() {
        var t = document.createElement("div");
        t.className = "modal-scrollbar-measure", this.$body.append(t);
        var e = t.offsetWidth - t.clientWidth;
        return this.$body[0].removeChild(t), e
    };
    var o = t.fn.modal;
    t.fn.modal = e, t.fn.modal.Constructor = i, t.fn.modal.noConflict = function() {
        return t.fn.modal = o, this
    }, t(document).on("click.bs.modal.data-api", '[data-toggle="modal"]', function(i) {
        var o = t(this),
            n = o.attr("href"),
            s = t(o.attr("data-target") || n && n.replace(/.*(?=#[^\s]+$)/, "")),
            a = s.data("bs.modal") ? "toggle" : t.extend({
                remote: !/#/.test(n) && n
            }, s.data(), o.data());
        o.is("a") && i.preventDefault(), s.one("show.bs.modal", function(t) {
            t.isDefaultPrevented() || s.one("hidden.bs.modal", function() {
                o.is(":visible") && o.trigger("focus")
            })
        }), e.call(s, a, this)
    })
}(jQuery), + function(t) {
    "use strict";

    function e(e) {
        return this.each(function() {
            var o = t(this),
                n = o.data("bs.tooltip"),
                s = "object" == typeof e && e;
            (n || !/destroy|hide/.test(e)) && (n || o.data("bs.tooltip", n = new i(this, s)), "string" == typeof e && n[e]())
        })
    }
    var i = function(t, e) {
        this.type = null, this.options = null, this.enabled = null, this.timeout = null, this.hoverState = null, this.$element = null, this.inState = null, this.init("tooltip", t, e)
    };
    i.VERSION = "3.3.5", i.TRANSITION_DURATION = 150, i.DEFAULTS = {
        animation: !0,
        placement: "top",
        selector: !1,
        template: '<div class="tooltip" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>',
        trigger: "hover focus",
        title: "",
        delay: 0,
        html: !1,
        container: !1,
        viewport: {
            selector: "body",
            padding: 0
        }
    }, i.prototype.init = function(e, i, o) {
        if (this.enabled = !0, this.type = e, this.$element = t(i), this.options = this.getOptions(o), this.$viewport = this.options.viewport && t(t.isFunction(this.options.viewport) ? this.options.viewport.call(this, this.$element) : this.options.viewport.selector || this.options.viewport), this.inState = {
                click: !1,
                hover: !1,
                focus: !1
            }, this.$element[0] instanceof document.constructor && !this.options.selector) throw new Error("`selector` option must be specified when initializing " + this.type + " on the window.document object!");
        for (var n = this.options.trigger.split(" "), s = n.length; s--;) {
            var a = n[s];
            if ("click" == a) this.$element.on("click." + this.type, this.options.selector, t.proxy(this.toggle, this));
            else if ("manual" != a) {
                var r = "hover" == a ? "mouseenter" : "focusin",
                    l = "hover" == a ? "mouseleave" : "focusout";
                this.$element.on(r + "." + this.type, this.options.selector, t.proxy(this.enter, this)), this.$element.on(l + "." + this.type, this.options.selector, t.proxy(this.leave, this))
            }
        }
        this.options.selector ? this._options = t.extend({}, this.options, {
            trigger: "manual",
            selector: ""
        }) : this.fixTitle()
    }, i.prototype.getDefaults = function() {
        return i.DEFAULTS
    }, i.prototype.getOptions = function(e) {
        return e = t.extend({}, this.getDefaults(), this.$element.data(), e), e.delay && "number" == typeof e.delay && (e.delay = {
            show: e.delay,
            hide: e.delay
        }), e
    }, i.prototype.getDelegateOptions = function() {
        var e = {},
            i = this.getDefaults();
        return this._options && t.each(this._options, function(t, o) {
            i[t] != o && (e[t] = o)
        }), e
    }, i.prototype.enter = function(e) {
        var i = e instanceof this.constructor ? e : t(e.currentTarget).data("bs." + this.type);
        return i || (i = new this.constructor(e.currentTarget, this.getDelegateOptions()), t(e.currentTarget).data("bs." + this.type, i)), e instanceof t.Event && (i.inState["focusin" == e.type ? "focus" : "hover"] = !0), i.tip().hasClass("in") || "in" == i.hoverState ? void(i.hoverState = "in") : (clearTimeout(i.timeout), i.hoverState = "in", i.options.delay && i.options.delay.show ? void(i.timeout = setTimeout(function() {
            "in" == i.hoverState && i.show()
        }, i.options.delay.show)) : i.show())
    }, i.prototype.isInStateTrue = function() {
        for (var t in this.inState)
            if (this.inState[t]) return !0;
        return !1
    }, i.prototype.leave = function(e) {
        var i = e instanceof this.constructor ? e : t(e.currentTarget).data("bs." + this.type);
        return i || (i = new this.constructor(e.currentTarget, this.getDelegateOptions()), t(e.currentTarget).data("bs." + this.type, i)), e instanceof t.Event && (i.inState["focusout" == e.type ? "focus" : "hover"] = !1), i.isInStateTrue() ? void 0 : (clearTimeout(i.timeout), i.hoverState = "out", i.options.delay && i.options.delay.hide ? void(i.timeout = setTimeout(function() {
            "out" == i.hoverState && i.hide()
        }, i.options.delay.hide)) : i.hide())
    }, i.prototype.show = function() {
        var e = t.Event("show.bs." + this.type);
        if (this.hasContent() && this.enabled) {
            this.$element.trigger(e);
            var o = t.contains(this.$element[0].ownerDocument.documentElement, this.$element[0]);
            if (e.isDefaultPrevented() || !o) return;
            var n = this,
                s = this.tip(),
                a = this.getUID(this.type);
            this.setContent(), s.attr("id", a), this.$element.attr("aria-describedby", a), this.options.animation && s.addClass("fade");
            var r = "function" == typeof this.options.placement ? this.options.placement.call(this, s[0], this.$element[0]) : this.options.placement,
                l = /\s?auto?\s?/i,
                h = l.test(r);
            h && (r = r.replace(l, "") || "top"), s.detach().css({
                top: 0,
                left: 0,
                display: "block"
            }).addClass(r).data("bs." + this.type, this), this.options.container ? s.appendTo(this.options.container) : s.insertAfter(this.$element), this.$element.trigger("inserted.bs." + this.type);
            var d = this.getPosition(),
                p = s[0].offsetWidth,
                c = s[0].offsetHeight;
            if (h) {
                var f = r,
                    u = this.getPosition(this.$viewport);
                r = "bottom" == r && d.bottom + c > u.bottom ? "top" : "top" == r && d.top - c < u.top ? "bottom" : "right" == r && d.right + p > u.width ? "left" : "left" == r && d.left - p < u.left ? "right" : r, s.removeClass(f).addClass(r)
            }
            var g = this.getCalculatedOffset(r, d, p, c);
            this.applyPlacement(g, r);
            var m = function() {
                var t = n.hoverState;
                n.$element.trigger("shown.bs." + n.type), n.hoverState = null, "out" == t && n.leave(n)
            };
            t.support.transition && this.$tip.hasClass("fade") ? s.one("bsTransitionEnd", m).emulateTransitionEnd(i.TRANSITION_DURATION) : m()
        }
    }, i.prototype.applyPlacement = function(e, i) {
        var o = this.tip(),
            n = o[0].offsetWidth,
            s = o[0].offsetHeight,
            a = parseInt(o.css("margin-top"), 10),
            r = parseInt(o.css("margin-left"), 10);
        isNaN(a) && (a = 0), isNaN(r) && (r = 0), e.top += a, e.left += r, t.offset.setOffset(o[0], t.extend({
            using: function(t) {
                o.css({
                    top: Math.round(t.top),
                    left: Math.round(t.left)
                })
            }
        }, e), 0), o.addClass("in");
        var l = o[0].offsetWidth,
            h = o[0].offsetHeight;
        "top" == i && h != s && (e.top = e.top + s - h);
        var d = this.getViewportAdjustedDelta(i, e, l, h);
        d.left ? e.left += d.left : e.top += d.top;
        var p = /top|bottom/.test(i),
            c = p ? 2 * d.left - n + l : 2 * d.top - s + h,
            f = p ? "offsetWidth" : "offsetHeight";
        o.offset(e), this.replaceArrow(c, o[0][f], p)
    }, i.prototype.replaceArrow = function(t, e, i) {
        this.arrow().css(i ? "left" : "top", 50 * (1 - t / e) + "%").css(i ? "top" : "left", "")
    }, i.prototype.setContent = function() {
        var t = this.tip(),
            e = this.getTitle();
        t.find(".tooltip-inner")[this.options.html ? "html" : "text"](e), t.removeClass("fade in top bottom left right")
    }, i.prototype.hide = function(e) {
        function o() {
            "in" != n.hoverState && s.detach(), n.$element.removeAttr("aria-describedby").trigger("hidden.bs." + n.type), e && e()
        }
        var n = this,
            s = t(this.$tip),
            a = t.Event("hide.bs." + this.type);
        return this.$element.trigger(a), a.isDefaultPrevented() ? void 0 : (s.removeClass("in"), t.support.transition && s.hasClass("fade") ? s.one("bsTransitionEnd", o).emulateTransitionEnd(i.TRANSITION_DURATION) : o(), this.hoverState = null, this)
    }, i.prototype.fixTitle = function() {
        var t = this.$element;
        (t.attr("title") || "string" != typeof t.attr("data-original-title")) && t.attr("data-original-title", t.attr("title") || "").attr("title", "")
    }, i.prototype.hasContent = function() {
        return this.getTitle()
    }, i.prototype.getPosition = function(e) {
        e = e || this.$element;
        var i = e[0],
            o = "BODY" == i.tagName,
            n = i.getBoundingClientRect();
        null == n.width && (n = t.extend({}, n, {
            width: n.right - n.left,
            height: n.bottom - n.top
        }));
        var s = o ? {
                top: 0,
                left: 0
            } : e.offset(),
            a = {
                scroll: o ? document.documentElement.scrollTop || document.body.scrollTop : e.scrollTop()
            },
            r = o ? {
                width: t(window).width(),
                height: t(window).height()
            } : null;
        return t.extend({}, n, a, r, s)
    }, i.prototype.getCalculatedOffset = function(t, e, i, o) {
        return "bottom" == t ? {
            top: e.top + e.height,
            left: e.left + e.width / 2 - i / 2
        } : "top" == t ? {
            top: e.top - o,
            left: e.left + e.width / 2 - i / 2
        } : "left" == t ? {
            top: e.top + e.height / 2 - o / 2,
            left: e.left - i
        } : {
            top: e.top + e.height / 2 - o / 2,
            left: e.left + e.width
        }
    }, i.prototype.getViewportAdjustedDelta = function(t, e, i, o) {
        var n = {
            top: 0,
            left: 0
        };
        if (!this.$viewport) return n;
        var s = this.options.viewport && this.options.viewport.padding || 0,
            a = this.getPosition(this.$viewport);
        if (/right|left/.test(t)) {
            var r = e.top - s - a.scroll,
                l = e.top + s - a.scroll + o;
            r < a.top ? n.top = a.top - r : l > a.top + a.height && (n.top = a.top + a.height - l)
        } else {
            var h = e.left - s,
                d = e.left + s + i;
            h < a.left ? n.left = a.left - h : d > a.right && (n.left = a.left + a.width - d)
        }
        return n
    }, i.prototype.getTitle = function() {
        var t, e = this.$element,
            i = this.options;
        return t = e.attr("data-original-title") || ("function" == typeof i.title ? i.title.call(e[0]) : i.title)
    }, i.prototype.getUID = function(t) {
        do t += ~~(1e6 * Math.random()); while (document.getElementById(t));
        return t
    }, i.prototype.tip = function() {
        if (!this.$tip && (this.$tip = t(this.options.template), 1 != this.$tip.length)) throw new Error(this.type + " `template` option must consist of exactly 1 top-level element!");
        return this.$tip
    }, i.prototype.arrow = function() {
        return this.$arrow = this.$arrow || this.tip().find(".tooltip-arrow")
    }, i.prototype.enable = function() {
        this.enabled = !0
    }, i.prototype.disable = function() {
        this.enabled = !1
    }, i.prototype.toggleEnabled = function() {
        this.enabled = !this.enabled
    }, i.prototype.toggle = function(e) {
        var i = this;
        e && (i = t(e.currentTarget).data("bs." + this.type), i || (i = new this.constructor(e.currentTarget, this.getDelegateOptions()), t(e.currentTarget).data("bs." + this.type, i))), e ? (i.inState.click = !i.inState.click, i.isInStateTrue() ? i.enter(i) : i.leave(i)) : i.tip().hasClass("in") ? i.leave(i) : i.enter(i)
    }, i.prototype.destroy = function() {
        var t = this;
        clearTimeout(this.timeout), this.hide(function() {
            t.$element.off("." + t.type).removeData("bs." + t.type), t.$tip && t.$tip.detach(), t.$tip = null, t.$arrow = null, t.$viewport = null
        })
    };
    var o = t.fn.tooltip;
    t.fn.tooltip = e, t.fn.tooltip.Constructor = i, t.fn.tooltip.noConflict = function() {
        return t.fn.tooltip = o, this
    }
}(jQuery), + function(t) {
    "use strict";

    function e(e) {
        return this.each(function() {
            var o = t(this),
                n = o.data("bs.popover"),
                s = "object" == typeof e && e;
            (n || !/destroy|hide/.test(e)) && (n || o.data("bs.popover", n = new i(this, s)), "string" == typeof e && n[e]())
        })
    }
    var i = function(t, e) {
        this.init("popover", t, e)
    };
    if (!t.fn.tooltip) throw new Error("Popover requires tooltip.js");
    i.VERSION = "3.3.5", i.DEFAULTS = t.extend({}, t.fn.tooltip.Constructor.DEFAULTS, {
        placement: "right",
        trigger: "click",
        content: "",
        template: '<div class="popover" role="tooltip"><div class="arrow"></div><h3 class="popover-title"></h3><div class="popover-content"></div></div>'
    }), i.prototype = t.extend({}, t.fn.tooltip.Constructor.prototype), i.prototype.constructor = i, i.prototype.getDefaults = function() {
        return i.DEFAULTS
    }, i.prototype.setContent = function() {
        var t = this.tip(),
            e = this.getTitle(),
            i = this.getContent();
        t.find(".popover-title")[this.options.html ? "html" : "text"](e), t.find(".popover-content").children().detach().end()[this.options.html ? "string" == typeof i ? "html" : "append" : "text"](i), t.removeClass("fade top bottom left right in"), t.find(".popover-title").html() || t.find(".popover-title").hide()
    }, i.prototype.hasContent = function() {
        return this.getTitle() || this.getContent()
    }, i.prototype.getContent = function() {
        var t = this.$element,
            e = this.options;
        return t.attr("data-content") || ("function" == typeof e.content ? e.content.call(t[0]) : e.content)
    }, i.prototype.arrow = function() {
        return this.$arrow = this.$arrow || this.tip().find(".arrow")
    };
    var o = t.fn.popover;
    t.fn.popover = e, t.fn.popover.Constructor = i, t.fn.popover.noConflict = function() {
        return t.fn.popover = o, this
    }
}(jQuery), + function(t) {
    "use strict";

    function e(e) {
        return this.each(function() {
            var o = t(this),
                n = o.data("bs.tab");
            n || o.data("bs.tab", n = new i(this)), "string" == typeof e && n[e]()
        })
    }
    var i = function(e) {
        this.element = t(e)
    };
    i.VERSION = "3.3.5", i.TRANSITION_DURATION = 150, i.prototype.show = function() {
        var e = this.element,
            i = e.closest("ul:not(.dropdown-menu)"),
            o = e.data("target");
        if (o || (o = e.attr("href"), o = o && o.replace(/.*(?=#[^\s]*$)/, "")), !e.parent("li").hasClass("active")) {
            var n = i.find(".active:last a"),
                s = t.Event("hide.bs.tab", {
                    relatedTarget: e[0]
                }),
                a = t.Event("show.bs.tab", {
                    relatedTarget: n[0]
                });
            if (n.trigger(s), e.trigger(a), !a.isDefaultPrevented() && !s.isDefaultPrevented()) {
                var r = t(o);
                this.activate(e.closest("li"), i), this.activate(r, r.parent(), function() {
                    n.trigger({
                        type: "hidden.bs.tab",
                        relatedTarget: e[0]
                    }), e.trigger({
                        type: "shown.bs.tab",
                        relatedTarget: n[0]
                    })
                })
            }
        }
    }, i.prototype.activate = function(e, o, n) {
        function s() {
            a.removeClass("active").find("> .dropdown-menu > .active").removeClass("active").end().find('[data-toggle="tab"]').attr("aria-expanded", !1), e.addClass("active").find('[data-toggle="tab"]').attr("aria-expanded", !0), r ? (e[0].offsetWidth, e.addClass("in")) : e.removeClass("fade"), e.parent(".dropdown-menu").length && e.closest("li.dropdown").addClass("active").end().find('[data-toggle="tab"]').attr("aria-expanded", !0), n && n()
        }
        var a = o.find("> .active"),
            r = n && t.support.transition && (a.length && a.hasClass("fade") || !!o.find("> .fade").length);
        a.length && r ? a.one("bsTransitionEnd", s).emulateTransitionEnd(i.TRANSITION_DURATION) : s(), a.removeClass("in")
    };
    var o = t.fn.tab;
    t.fn.tab = e, t.fn.tab.Constructor = i, t.fn.tab.noConflict = function() {
        return t.fn.tab = o, this
    };
    var n = function(i) {
        i.preventDefault(), e.call(t(this), "show")
    };
    t(document).on("click.bs.tab.data-api", '[data-toggle="tab"]', n).on("click.bs.tab.data-api", '[data-toggle="pill"]', n)
}(jQuery), + function(t) {
    "use strict";

    function e(e) {
        return this.each(function() {
            var o = t(this),
                n = o.data("bs.affix"),
                s = "object" == typeof e && e;
            n || o.data("bs.affix", n = new i(this, s)), "string" == typeof e && n[e]()
        })
    }
    var i = function(e, o) {
        this.options = t.extend({}, i.DEFAULTS, o), this.$target = t(this.options.target).on("scroll.bs.affix.data-api", t.proxy(this.checkPosition, this)).on("click.bs.affix.data-api", t.proxy(this.checkPositionWithEventLoop, this)), this.$element = t(e), this.affixed = null, this.unpin = null, this.pinnedOffset = null, this.checkPosition()
    };
    i.VERSION = "3.3.5", i.RESET = "affix affix-top affix-bottom", i.DEFAULTS = {
        offset: 0,
        target: window
    }, i.prototype.getState = function(t, e, i, o) {
        var n = this.$target.scrollTop(),
            s = this.$element.offset(),
            a = this.$target.height();
        if (null != i && "top" == this.affixed) return i > n ? "top" : !1;
        if ("bottom" == this.affixed) return null != i ? n + this.unpin <= s.top ? !1 : "bottom" : t - o >= n + a ? !1 : "bottom";
        var r = null == this.affixed,
            l = r ? n : s.top,
            h = r ? a : e;
        return null != i && i >= n ? "top" : null != o && l + h >= t - o ? "bottom" : !1
    }, i.prototype.getPinnedOffset = function() {
        if (this.pinnedOffset) return this.pinnedOffset;
        this.$element.removeClass(i.RESET).addClass("affix");
        var t = this.$target.scrollTop(),
            e = this.$element.offset();
        return this.pinnedOffset = e.top - t
    }, i.prototype.checkPositionWithEventLoop = function() {
        setTimeout(t.proxy(this.checkPosition, this), 1)
    }, i.prototype.checkPosition = function() {
        if (this.$element.is(":visible")) {
            var e = this.$element.height(),
                o = this.options.offset,
                n = o.top,
                s = o.bottom,
                a = Math.max(t(document).height(), t(document.body).height());
            "object" != typeof o && (s = n = o), "function" == typeof n && (n = o.top(this.$element)), "function" == typeof s && (s = o.bottom(this.$element));
            var r = this.getState(a, e, n, s);
            if (this.affixed != r) {
                null != this.unpin && this.$element.css("top", "");
                var l = "affix" + (r ? "-" + r : ""),
                    h = t.Event(l + ".bs.affix");
                if (this.$element.trigger(h), h.isDefaultPrevented()) return;
                this.affixed = r, this.unpin = "bottom" == r ? this.getPinnedOffset() : null, this.$element.removeClass(i.RESET).addClass(l).trigger(l.replace("affix", "affixed") + ".bs.affix")
            }
            "bottom" == r && this.$element.offset({
                top: a - e - s
            })
        }
    };
    var o = t.fn.affix;
    t.fn.affix = e, t.fn.affix.Constructor = i, t.fn.affix.noConflict = function() {
        return t.fn.affix = o, this
    }, t(window).on("load", function() {
        t('[data-spy="affix"]').each(function() {
            var i = t(this),
                o = i.data();
            o.offset = o.offset || {}, null != o.offsetBottom && (o.offset.bottom = o.offsetBottom), null != o.offsetTop && (o.offset.top = o.offsetTop), e.call(i, o)
        })
    })
}(jQuery), + function(t) {
    "use strict";

    function e(e) {
        var i, o = e.attr("data-target") || (i = e.attr("href")) && i.replace(/.*(?=#[^\s]+$)/, "");
        return t(o)
    }

    function i(e) {
        return this.each(function() {
            var i = t(this),
                n = i.data("bs.collapse"),
                s = t.extend({}, o.DEFAULTS, i.data(), "object" == typeof e && e);
            !n && s.toggle && /show|hide/.test(e) && (s.toggle = !1), n || i.data("bs.collapse", n = new o(this, s)), "string" == typeof e && n[e]()
        })
    }
    var o = function(e, i) {
        this.$element = t(e), this.options = t.extend({}, o.DEFAULTS, i), this.$trigger = t('[data-toggle="collapse"][href="#' + e.id + '"],[data-toggle="collapse"][data-target="#' + e.id + '"]'), this.transitioning = null, this.options.parent ? this.$parent = this.getParent() : this.addAriaAndCollapsedClass(this.$element, this.$trigger), this.options.toggle && this.toggle()
    };
    o.VERSION = "3.3.5", o.TRANSITION_DURATION = 350, o.DEFAULTS = {
        toggle: !0
    }, o.prototype.dimension = function() {
        var t = this.$element.hasClass("width");
        return t ? "width" : "height"
    }, o.prototype.show = function() {
        if (!this.transitioning && !this.$element.hasClass("in")) {
            var e, n = this.$parent && this.$parent.children(".panel").children(".in, .collapsing");
            if (!(n && n.length && (e = n.data("bs.collapse"), e && e.transitioning))) {
                var s = t.Event("show.bs.collapse");
                if (this.$element.trigger(s), !s.isDefaultPrevented()) {
                    n && n.length && (i.call(n, "hide"), e || n.data("bs.collapse", null));
                    var a = this.dimension();
                    this.$element.removeClass("collapse").addClass("collapsing")[a](0).attr("aria-expanded", !0), this.$trigger.removeClass("collapsed").attr("aria-expanded", !0), this.transitioning = 1;
                    var r = function() {
                        this.$element.removeClass("collapsing").addClass("collapse in")[a](""), this.transitioning = 0, this.$element.trigger("shown.bs.collapse")
                    };
                    if (!t.support.transition) return r.call(this);
                    var l = t.camelCase(["scroll", a].join("-"));
                    this.$element.one("bsTransitionEnd", t.proxy(r, this)).emulateTransitionEnd(o.TRANSITION_DURATION)[a](this.$element[0][l]);
                }
            }
        }
    }, o.prototype.hide = function() {
        if (!this.transitioning && this.$element.hasClass("in")) {
            var e = t.Event("hide.bs.collapse");
            if (this.$element.trigger(e), !e.isDefaultPrevented()) {
                var i = this.dimension();
                this.$element[i](this.$element[i]())[0].offsetHeight, this.$element.addClass("collapsing").removeClass("collapse in").attr("aria-expanded", !1), this.$trigger.addClass("collapsed").attr("aria-expanded", !1), this.transitioning = 1;
                var n = function() {
                    this.transitioning = 0, this.$element.removeClass("collapsing").addClass("collapse").trigger("hidden.bs.collapse")
                };
                return t.support.transition ? void this.$element[i](0).one("bsTransitionEnd", t.proxy(n, this)).emulateTransitionEnd(o.TRANSITION_DURATION) : n.call(this)
            }
        }
    }, o.prototype.toggle = function() {
        this[this.$element.hasClass("in") ? "hide" : "show"]()
    }, o.prototype.getParent = function() {
        return t(this.options.parent).find('[data-toggle="collapse"][data-parent="' + this.options.parent + '"]').each(t.proxy(function(i, o) {
            var n = t(o);
            this.addAriaAndCollapsedClass(e(n), n)
        }, this)).end()
    }, o.prototype.addAriaAndCollapsedClass = function(t, e) {
        var i = t.hasClass("in");
        t.attr("aria-expanded", i), e.toggleClass("collapsed", !i).attr("aria-expanded", i)
    };
    var n = t.fn.collapse;
    t.fn.collapse = i, t.fn.collapse.Constructor = o, t.fn.collapse.noConflict = function() {
        return t.fn.collapse = n, this
    }, t(document).on("click.bs.collapse.data-api", '[data-toggle="collapse"]', function(o) {
        var n = t(this);
        n.attr("data-target") || o.preventDefault();
        var s = e(n),
            a = s.data("bs.collapse"),
            r = a ? "toggle" : n.data();
        i.call(s, r)
    })
}(jQuery), + function(t) {
    "use strict";

    function e(i, o) {
        this.$body = t(document.body), this.$scrollElement = t(t(i).is(document.body) ? window : i), this.options = t.extend({}, e.DEFAULTS, o), this.selector = (this.options.target || "") + " .nav li > a", this.offsets = [], this.targets = [], this.activeTarget = null, this.scrollHeight = 0, this.$scrollElement.on("scroll.bs.scrollspy", t.proxy(this.process, this)), this.refresh(), this.process()
    }

    function i(i) {
        return this.each(function() {
            var o = t(this),
                n = o.data("bs.scrollspy"),
                s = "object" == typeof i && i;
            n || o.data("bs.scrollspy", n = new e(this, s)), "string" == typeof i && n[i]()
        })
    }
    e.VERSION = "3.3.5", e.DEFAULTS = {
        offset: 10
    }, e.prototype.getScrollHeight = function() {
        return this.$scrollElement[0].scrollHeight || Math.max(this.$body[0].scrollHeight, document.documentElement.scrollHeight)
    }, e.prototype.refresh = function() {
        var e = this,
            i = "offset",
            o = 0;
        this.offsets = [], this.targets = [], this.scrollHeight = this.getScrollHeight(), t.isWindow(this.$scrollElement[0]) || (i = "position", o = this.$scrollElement.scrollTop()), this.$body.find(this.selector).map(function() {
            var e = t(this),
                n = e.data("target") || e.attr("href"),
                s = /^#./.test(n) && t(n);
            return s && s.length && s.is(":visible") && [
                [s[i]().top + o, n]
            ] || null
        }).sort(function(t, e) {
            return t[0] - e[0]
        }).each(function() {
            e.offsets.push(this[0]), e.targets.push(this[1])
        })
    }, e.prototype.process = function() {
        var t, e = this.$scrollElement.scrollTop() + this.options.offset,
            i = this.getScrollHeight(),
            o = this.options.offset + i - this.$scrollElement.height(),
            n = this.offsets,
            s = this.targets,
            a = this.activeTarget;
        if (this.scrollHeight != i && this.refresh(), e >= o) return a != (t = s[s.length - 1]) && this.activate(t);
        if (a && e < n[0]) return this.activeTarget = null, this.clear();
        for (t = n.length; t--;) a != s[t] && e >= n[t] && (void 0 === n[t + 1] || e < n[t + 1]) && this.activate(s[t])
    }, e.prototype.activate = function(e) {
        this.activeTarget = e, this.clear();
        var i = this.selector + '[data-target="' + e + '"],' + this.selector + '[href="' + e + '"]',
            o = t(i).parents("li").addClass("active");
        o.parent(".dropdown-menu").length && (o = o.closest("li.dropdown").addClass("active")), o.trigger("activate.bs.scrollspy")
    }, e.prototype.clear = function() {
        t(this.selector).parentsUntil(this.options.target, ".active").removeClass("active")
    };
    var o = t.fn.scrollspy;
    t.fn.scrollspy = i, t.fn.scrollspy.Constructor = e, t.fn.scrollspy.noConflict = function() {
        return t.fn.scrollspy = o, this
    }, t(window).on("load.bs.scrollspy.data-api", function() {
        t('[data-spy="scroll"]').each(function() {
            var e = t(this);
            i.call(e, e.data())
        })
    })
}(jQuery), + function(t) {
    "use strict";

    function e() {
        var t = document.createElement("bootstrap"),
            e = {
                WebkitTransition: "webkitTransitionEnd",
                MozTransition: "transitionend",
                OTransition: "oTransitionEnd otransitionend",
                transition: "transitionend"
            };
        for (var i in e)
            if (void 0 !== t.style[i]) return {
                end: e[i]
            };
        return !1
    }
    t.fn.emulateTransitionEnd = function(e) {
        var i = !1,
            o = this;
        t(this).one("bsTransitionEnd", function() {
            i = !0
        });
        var n = function() {
            i || t(o).trigger(t.support.transition.end)
        };
        return setTimeout(n, e), this
    }, t(function() {
        t.support.transition = e(), t.support.transition && (t.event.special.bsTransitionEnd = {
            bindType: t.support.transition.end,
            delegateType: t.support.transition.end,
            handle: function(e) {
                return t(e.target).is(this) ? e.handleObj.handler.apply(this, arguments) : void 0
            }
        })
    })
}(jQuery);

;// JS/widgets/jquery.mCustomScrollbar.concat.min.js
// widgets/jquery.mCustomScrollbar.concat.min.js
/* == jquery mousewheel plugin == Version: 3.1.12, License: MIT License (MIT) */
! function(a) {
    "function" == typeof define && define.amd ? define(["jquery"], a) : "object" == typeof exports ? module.exports = a : a(jQuery)
}(function(a) {
    function b(b) {
        var g = b || window.event,
            h = i.call(arguments, 1),
            j = 0,
            l = 0,
            m = 0,
            n = 0,
            o = 0,
            p = 0;
        if (b = a.event.fix(g), b.type = "mousewheel", "detail" in g && (m = -1 * g.detail), "wheelDelta" in g && (m = g.wheelDelta), "wheelDeltaY" in g && (m = g.wheelDeltaY), "wheelDeltaX" in g && (l = -1 * g.wheelDeltaX), "axis" in g && g.axis === g.HORIZONTAL_AXIS && (l = -1 * m, m = 0), j = 0 === m ? l : m, "deltaY" in g && (m = -1 * g.deltaY, j = m), "deltaX" in g && (l = g.deltaX, 0 === m && (j = -1 * l)), 0 !== m || 0 !== l) {
            if (1 === g.deltaMode) {
                var q = a.data(this, "mousewheel-line-height");
                j *= q, m *= q, l *= q
            } else if (2 === g.deltaMode) {
                var r = a.data(this, "mousewheel-page-height");
                j *= r, m *= r, l *= r
            }
            if (n = Math.max(Math.abs(m), Math.abs(l)), (!f || f > n) && (f = n, d(g, n) && (f /= 40)), d(g, n) && (j /= 40, l /= 40, m /= 40), j = Math[j >= 1 ? "floor" : "ceil"](j / f), l = Math[l >= 1 ? "floor" : "ceil"](l / f), m = Math[m >= 1 ? "floor" : "ceil"](m / f), k.settings.normalizeOffset && this.getBoundingClientRect) {
                var s = this.getBoundingClientRect();
                o = b.clientX - s.left, p = b.clientY - s.top
            }
            return b.deltaX = l, b.deltaY = m, b.deltaFactor = f, b.offsetX = o, b.offsetY = p, b.deltaMode = 0, h.unshift(b, j, l, m), e && clearTimeout(e), e = setTimeout(c, 200), (a.event.dispatch || a.event.handle).apply(this, h)
        }
    }

    function c() {
        f = null
    }

    function d(a, b) {
        return k.settings.adjustOldDeltas && "mousewheel" === a.type && b % 120 === 0
    }
    var e, f, g = ["wheel", "mousewheel", "DOMMouseScroll", "MozMousePixelScroll"],
        h = "onwheel" in document || document.documentMode >= 9 ? ["wheel"] : ["mousewheel", "DomMouseScroll", "MozMousePixelScroll"],
        i = Array.prototype.slice;
    if (a.event.fixHooks)
        for (var j = g.length; j;) a.event.fixHooks[g[--j]] = a.event.mouseHooks;
    var k = a.event.special.mousewheel = {
        version: "3.1.12",
        setup: function() {
            if (this.addEventListener)
                for (var c = h.length; c;) this.addEventListener(h[--c], b, !1);
            else this.onmousewheel = b;
            a.data(this, "mousewheel-line-height", k.getLineHeight(this)), a.data(this, "mousewheel-page-height", k.getPageHeight(this))
        },
        teardown: function() {
            if (this.removeEventListener)
                for (var c = h.length; c;) this.removeEventListener(h[--c], b, !1);
            else this.onmousewheel = null;
            a.removeData(this, "mousewheel-line-height"), a.removeData(this, "mousewheel-page-height")
        },
        getLineHeight: function(b) {
            var c = a(b),
                d = c["offsetParent" in a.fn ? "offsetParent" : "parent"]();
            return d.length || (d = a("body")), parseInt(d.css("fontSize"), 10) || parseInt(c.css("fontSize"), 10) || 16
        },
        getPageHeight: function(b) {
            return a(b).height()
        },
        settings: {
            adjustOldDeltas: !0,
            normalizeOffset: !0
        }
    };
    a.fn.extend({
        mousewheel: function(a) {
            return a ? this.bind("mousewheel", a) : this.trigger("mousewheel")
        },
        unmousewheel: function(a) {
            return this.unbind("mousewheel", a)
        }
    })
});
/* == malihu jquery custom scrollbar plugin == Version: 3.1.0, License: MIT License (MIT) */
! function(e) {
    "undefined" != typeof module && module.exports ? module.exports = e : e(jQuery, window, document)
}(function(e) {
    ! function(t) {
        var o = "function" == typeof define && define.amd,
            a = "undefined" != typeof module && module.exports,
            n = "https:" == document.location.protocol ? "https:" : "http:",
            i = "cdnjs.cloudflare.com/ajax/libs/jquery-mousewheel/3.1.12/jquery.mousewheel.min.js";
        o || (a ? require("jquery-mousewheel")(e) : e.event.special.mousewheel || e("head").append(decodeURI("%3Cscript src=" + n + "//" + i + "%3E%3C/script%3E"))), t()
    }(function() {
        var t, o = "mCustomScrollbar",
            a = "mCS",
            n = ".mCustomScrollbar",
            i = {
                setTop: 0,
                setLeft: 0,
                axis: "y",
                scrollbarPosition: "inside",
                scrollInertia: 950,
                autoDraggerLength: !0,
                alwaysShowScrollbar: 0,
                snapOffset: 0,
                mouseWheel: {
                    enable: !0,
                    scrollAmount: "auto",
                    axis: "y",
                    deltaFactor: "auto",
                    disableOver: ["select", "option", "keygen", "datalist", "textarea"]
                },
                scrollButtons: {
                    scrollType: "stepless",
                    scrollAmount: "auto"
                },
                keyboard: {
                    enable: !0,
                    scrollType: "stepless",
                    scrollAmount: "auto"
                },
                contentTouchScroll: 25,
                advanced: {
                    autoScrollOnFocus: "input,textarea,select,button,datalist,keygen,a[tabindex],area,object,[contenteditable='true']",
                    updateOnContentResize: !0,
                    updateOnImageLoad: "auto",
                    autoUpdateTimeout: 60
                },
                theme: "light",
                callbacks: {
                    onTotalScrollOffset: 0,
                    onTotalScrollBackOffset: 0,
                    alwaysTriggerOffsets: !0
                }
            },
            r = 0,
            l = {},
            s = window.attachEvent && !window.addEventListener ? 1 : 0,
            c = !1,
            d = ["mCSB_dragger_onDrag", "mCSB_scrollTools_onDrag", "mCS_img_loaded", "mCS_disabled", "mCS_destroyed", "mCS_no_scrollbar", "mCS-autoHide", "mCS-dir-rtl", "mCS_no_scrollbar_y", "mCS_no_scrollbar_x", "mCS_y_hidden", "mCS_x_hidden", "mCSB_draggerContainer", "mCSB_buttonUp", "mCSB_buttonDown", "mCSB_buttonLeft", "mCSB_buttonRight"],
            u = {
                init: function(t) {
                    var t = e.extend(!0, {}, i, t),
                        o = f.call(this);
                    if (t.live) {
                        var s = t.liveSelector || this.selector || n,
                            c = e(s);
                        if ("off" === t.live) return void m(s);
                        l[s] = setTimeout(function() {
                            c.mCustomScrollbar(t), "once" === t.live && c.length && m(s)
                        }, 500)
                    } else m(s);
                    return t.setWidth = t.set_width ? t.set_width : t.setWidth, t.setHeight = t.set_height ? t.set_height : t.setHeight, t.axis = t.horizontalScroll ? "x" : p(t.axis), t.scrollInertia = t.scrollInertia > 0 && t.scrollInertia < 17 ? 17 : t.scrollInertia, "object" != typeof t.mouseWheel && 1 == t.mouseWheel && (t.mouseWheel = {
                        enable: !0,
                        scrollAmount: "auto",
                        axis: "y",
                        preventDefault: !1,
                        deltaFactor: "auto",
                        normalizeDelta: !1,
                        invert: !1
                    }), t.mouseWheel.scrollAmount = t.mouseWheelPixels ? t.mouseWheelPixels : t.mouseWheel.scrollAmount, t.mouseWheel.normalizeDelta = t.advanced.normalizeMouseWheelDelta ? t.advanced.normalizeMouseWheelDelta : t.mouseWheel.normalizeDelta, t.scrollButtons.scrollType = g(t.scrollButtons.scrollType), h(t), e(o).each(function() {
                        var o = e(this);
                        if (!o.data(a)) {
                            o.data(a, {
                                idx: ++r,
                                opt: t,
                                scrollRatio: {
                                    y: null,
                                    x: null
                                },
                                overflowed: null,
                                contentReset: {
                                    y: null,
                                    x: null
                                },
                                bindEvents: !1,
                                tweenRunning: !1,
                                sequential: {},
                                langDir: o.css("direction"),
                                cbOffsets: null,
                                trigger: null,
                                poll: {
                                    size: {
                                        o: 0,
                                        n: 0
                                    },
                                    img: {
                                        o: 0,
                                        n: 0
                                    },
                                    change: {
                                        o: 0,
                                        n: 0
                                    }
                                }
                            });
                            var n = o.data(a),
                                i = n.opt,
                                l = o.data("mcs-axis"),
                                s = o.data("mcs-scrollbar-position"),
                                c = o.data("mcs-theme");
                            l && (i.axis = l), s && (i.scrollbarPosition = s), c && (i.theme = c, h(i)), v.call(this), n && i.callbacks.onCreate && "function" == typeof i.callbacks.onCreate && i.callbacks.onCreate.call(this), e("#mCSB_" + n.idx + "_container img:not(." + d[2] + ")").addClass(d[2]), u.update.call(null, o)
                        }
                    })
                },
                update: function(t, o) {
                    var n = t || f.call(this);
                    return e(n).each(function() {
                        var t = e(this);
                        if (t.data(a)) {
                            var n = t.data(a),
                                i = n.opt,
                                r = e("#mCSB_" + n.idx + "_container"),
                                l = e("#mCSB_" + n.idx),
                                s = [e("#mCSB_" + n.idx + "_dragger_vertical"), e("#mCSB_" + n.idx + "_dragger_horizontal")];
                            if (!r.length) return;
                            n.tweenRunning && N(t), o && n && i.callbacks.onBeforeUpdate && "function" == typeof i.callbacks.onBeforeUpdate && i.callbacks.onBeforeUpdate.call(this), t.hasClass(d[3]) && t.removeClass(d[3]), t.hasClass(d[4]) && t.removeClass(d[4]), l.height() !== t.height() && l.css("max-height", t.height()), _.call(this), "y" === i.axis || i.advanced.autoExpandHorizontalScroll || r.css("width", x(r)), n.overflowed = y.call(this), M.call(this), i.autoDraggerLength && S.call(this), b.call(this), T.call(this);
                            var c = [Math.abs(r[0].offsetTop), Math.abs(r[0].offsetLeft)];
                            "x" !== i.axis && (n.overflowed[0] ? s[0].height() > s[0].parent().height() ? B.call(this) : (V(t, c[0].toString(), {
                                dir: "y",
                                dur: 0,
                                overwrite: "none"
                            }), n.contentReset.y = null) : (B.call(this), "y" === i.axis ? k.call(this) : "yx" === i.axis && n.overflowed[1] && V(t, c[1].toString(), {
                                dir: "x",
                                dur: 0,
                                overwrite: "none"
                            }))), "y" !== i.axis && (n.overflowed[1] ? s[1].width() > s[1].parent().width() ? B.call(this) : (V(t, c[1].toString(), {
                                dir: "x",
                                dur: 0,
                                overwrite: "none"
                            }), n.contentReset.x = null) : (B.call(this), "x" === i.axis ? k.call(this) : "yx" === i.axis && n.overflowed[0] && V(t, c[0].toString(), {
                                dir: "y",
                                dur: 0,
                                overwrite: "none"
                            }))), o && n && (2 === o && i.callbacks.onImageLoad && "function" == typeof i.callbacks.onImageLoad ? i.callbacks.onImageLoad.call(this) : 3 === o && i.callbacks.onSelectorChange && "function" == typeof i.callbacks.onSelectorChange ? i.callbacks.onSelectorChange.call(this) : i.callbacks.onUpdate && "function" == typeof i.callbacks.onUpdate && i.callbacks.onUpdate.call(this)), j.call(this)
                        }
                    })
                },
                scrollTo: function(t, o) {
                    if ("undefined" != typeof t && null != t) {
                        var n = f.call(this);
                        return e(n).each(function() {
                            var n = e(this);
                            if (n.data(a)) {
                                var i = n.data(a),
                                    r = i.opt,
                                    l = {
                                        trigger: "external",
                                        scrollInertia: r.scrollInertia,
                                        scrollEasing: "mcsEaseInOut",
                                        moveDragger: !1,
                                        timeout: 60,
                                        callbacks: !0,
                                        onStart: !0,
                                        onUpdate: !0,
                                        onComplete: !0
                                    },
                                    s = e.extend(!0, {}, l, o),
                                    c = q.call(this, t),
                                    d = s.scrollInertia > 0 && s.scrollInertia < 17 ? 17 : s.scrollInertia;
                                c[0] = Y.call(this, c[0], "y"), c[1] = Y.call(this, c[1], "x"), s.moveDragger && (c[0] *= i.scrollRatio.y, c[1] *= i.scrollRatio.x), s.dur = d, setTimeout(function() {
                                    null !== c[0] && "undefined" != typeof c[0] && "x" !== r.axis && i.overflowed[0] && (s.dir = "y", s.overwrite = "all", V(n, c[0].toString(), s)), null !== c[1] && "undefined" != typeof c[1] && "y" !== r.axis && i.overflowed[1] && (s.dir = "x", s.overwrite = "none", V(n, c[1].toString(), s))
                                }, s.timeout)
                            }
                        })
                    }
                },
                stop: function() {
                    var t = f.call(this);
                    return e(t).each(function() {
                        var t = e(this);
                        t.data(a) && N(t)
                    })
                },
                disable: function(t) {
                    var o = f.call(this);
                    return e(o).each(function() {
                        var o = e(this);
                        if (o.data(a)) {
                            {
                                o.data(a)
                            }
                            j.call(this, "remove"), k.call(this), t && B.call(this), M.call(this, !0), o.addClass(d[3])
                        }
                    })
                },
                destroy: function() {
                    var t = f.call(this);
                    return e(t).each(function() {
                        var n = e(this);
                        if (n.data(a)) {
                            var i = n.data(a),
                                r = i.opt,
                                l = e("#mCSB_" + i.idx),
                                s = e("#mCSB_" + i.idx + "_container"),
                                c = e(".mCSB_" + i.idx + "_scrollbar");
                            r.live && m(r.liveSelector || e(t).selector), j.call(this, "remove"), k.call(this), B.call(this), n.removeData(a), K(this, "mcs"), c.remove(), s.find("img." + d[2]).removeClass(d[2]), l.replaceWith(s.contents()), n.removeClass(o + " _" + a + "_" + i.idx + " " + d[6] + " " + d[7] + " " + d[5] + " " + d[3]).addClass(d[4])
                        }
                    })
                }
            },
            f = function() {
                return "object" != typeof e(this) || e(this).length < 1 ? n : this
            },
            h = function(t) {
                var o = ["rounded", "rounded-dark", "rounded-dots", "rounded-dots-dark"],
                    a = ["rounded-dots", "rounded-dots-dark", "3d", "3d-dark", "3d-thick", "3d-thick-dark", "inset", "inset-dark", "inset-2", "inset-2-dark", "inset-3", "inset-3-dark"],
                    n = ["minimal", "minimal-dark"],
                    i = ["minimal", "minimal-dark"],
                    r = ["minimal", "minimal-dark"];
                t.autoDraggerLength = e.inArray(t.theme, o) > -1 ? !1 : t.autoDraggerLength, t.autoExpandScrollbar = e.inArray(t.theme, a) > -1 ? !1 : t.autoExpandScrollbar, t.scrollButtons.enable = e.inArray(t.theme, n) > -1 ? !1 : t.scrollButtons.enable, t.autoHideScrollbar = e.inArray(t.theme, i) > -1 ? !0 : t.autoHideScrollbar, t.scrollbarPosition = e.inArray(t.theme, r) > -1 ? "outside" : t.scrollbarPosition
            },
            m = function(e) {
                l[e] && (clearTimeout(l[e]), K(l, e))
            },
            p = function(e) {
                return "yx" === e || "xy" === e || "auto" === e ? "yx" : "x" === e || "horizontal" === e ? "x" : "y"
            },
            g = function(e) {
                return "stepped" === e || "pixels" === e || "step" === e || "click" === e ? "stepped" : "stepless"
            },
            v = function() {
                var t = e(this),
                    n = t.data(a),
                    i = n.opt,
                    r = i.autoExpandScrollbar ? " " + d[1] + "_expand" : "",
                    l = ["<div id='mCSB_" + n.idx + "_scrollbar_vertical' class='mCSB_scrollTools mCSB_" + n.idx + "_scrollbar mCS-" + i.theme + " mCSB_scrollTools_vertical" + r + "'><div class='" + d[12] + "'><div id='mCSB_" + n.idx + "_dragger_vertical' class='mCSB_dragger' style='position:absolute;' oncontextmenu='return false;'><div class='mCSB_dragger_bar' /></div><div class='mCSB_draggerRail' /></div></div>", "<div id='mCSB_" + n.idx + "_scrollbar_horizontal' class='mCSB_scrollTools mCSB_" + n.idx + "_scrollbar mCS-" + i.theme + " mCSB_scrollTools_horizontal" + r + "'><div class='" + d[12] + "'><div id='mCSB_" + n.idx + "_dragger_horizontal' class='mCSB_dragger' style='position:absolute;' oncontextmenu='return false;'><div class='mCSB_dragger_bar' /></div><div class='mCSB_draggerRail' /></div></div>"],
                    s = "yx" === i.axis ? "mCSB_vertical_horizontal" : "x" === i.axis ? "mCSB_horizontal" : "mCSB_vertical",
                    c = "yx" === i.axis ? l[0] + l[1] : "x" === i.axis ? l[1] : l[0],
                    u = "yx" === i.axis ? "<div id='mCSB_" + n.idx + "_container_wrapper' class='mCSB_container_wrapper' />" : "",
                    f = i.autoHideScrollbar ? " " + d[6] : "",
                    h = "x" !== i.axis && "rtl" === n.langDir ? " " + d[7] : "";
                i.setWidth && t.css("width", i.setWidth), i.setHeight && t.css("height", i.setHeight), i.setLeft = "y" !== i.axis && "rtl" === n.langDir ? "989999px" : i.setLeft, t.addClass(o + " _" + a + "_" + n.idx + f + h).wrapInner("<div id='mCSB_" + n.idx + "' class='mCustomScrollBox mCS-" + i.theme + " " + s + "'><div id='mCSB_" + n.idx + "_container' class='mCSB_container' style='position:relative; top:" + i.setTop + "; left:" + i.setLeft + ";' dir=" + n.langDir + " /></div>");
                var m = e("#mCSB_" + n.idx),
                    p = e("#mCSB_" + n.idx + "_container");
                "y" === i.axis || i.advanced.autoExpandHorizontalScroll || p.css("width", x(p)), "outside" === i.scrollbarPosition ? ("static" === t.css("position") && t.css("position", "relative"), t.css("overflow", "visible"), m.addClass("mCSB_outside").after(c)) : (m.addClass("mCSB_inside").append(c), p.wrap(u)), w.call(this);
                var g = [e("#mCSB_" + n.idx + "_dragger_vertical"), e("#mCSB_" + n.idx + "_dragger_horizontal")];
                g[0].css("min-height", g[0].height()), g[1].css("min-width", g[1].width())
            },
            x = function(t) {
                var o = [t[0].scrollWidth, Math.max.apply(Math, t.children().map(function() {
                        return e(this).outerWidth(!0)
                    }).get())],
                    a = t.parent().width();
                return o[0] > a ? o[0] : o[1] > a ? o[1] : "100%"
            },
            _ = function() {
                var t = e(this),
                    o = t.data(a),
                    n = o.opt,
                    i = e("#mCSB_" + o.idx + "_container");
                if (n.advanced.autoExpandHorizontalScroll && "y" !== n.axis) {
                    i.css({
                        width: "auto",
                        "min-width": 0,
                        "overflow-x": "scroll"
                    });
                    var r = Math.ceil(i[0].scrollWidth);
                    3 === n.advanced.autoExpandHorizontalScroll || 2 !== n.advanced.autoExpandHorizontalScroll && r > i.parent().width() ? i.css({
                        width: r,
                        "min-width": "100%",
                        "overflow-x": "inherit"
                    }) : i.css({
                        "overflow-x": "inherit",
                        position: "absolute"
                    }).wrap("<div class='mCSB_h_wrapper' style='position:relative; left:0; width:999999px;' />").css({
                        width: Math.ceil(i[0].getBoundingClientRect().right + .4) - Math.floor(i[0].getBoundingClientRect().left),
                        "min-width": "100%",
                        position: "relative"
                    }).unwrap()
                }
            },
            w = function() {
                var t = e(this),
                    o = t.data(a),
                    n = o.opt,
                    i = e(".mCSB_" + o.idx + "_scrollbar:first"),
                    r = ee(n.scrollButtons.tabindex) ? "tabindex='" + n.scrollButtons.tabindex + "'" : "",
                    l = ["<a href='#' class='" + d[13] + "' oncontextmenu='return false;' " + r + " />", "<a href='#' class='" + d[14] + "' oncontextmenu='return false;' " + r + " />", "<a href='#' class='" + d[15] + "' oncontextmenu='return false;' " + r + " />", "<a href='#' class='" + d[16] + "' oncontextmenu='return false;' " + r + " />"],
                    s = ["x" === n.axis ? l[2] : l[0], "x" === n.axis ? l[3] : l[1], l[2], l[3]];
                n.scrollButtons.enable && i.prepend(s[0]).append(s[1]).next(".mCSB_scrollTools").prepend(s[2]).append(s[3])
            },
            S = function() {
                var t = e(this),
                    o = t.data(a),
                    n = e("#mCSB_" + o.idx),
                    i = e("#mCSB_" + o.idx + "_container"),
                    r = [e("#mCSB_" + o.idx + "_dragger_vertical"), e("#mCSB_" + o.idx + "_dragger_horizontal")],
                    l = [n.height() / i.outerHeight(!1), n.width() / i.outerWidth(!1)],
                    c = [parseInt(r[0].css("min-height")), Math.round(l[0] * r[0].parent().height()), parseInt(r[1].css("min-width")), Math.round(l[1] * r[1].parent().width())],
                    d = s && c[1] < c[0] ? c[0] : c[1],
                    u = s && c[3] < c[2] ? c[2] : c[3];
                r[0].css({
                    height: d,
                    "max-height": r[0].parent().height() - 10
                }).find(".mCSB_dragger_bar").css({
                    "line-height": c[0] + "px"
                }), r[1].css({
                    width: u,
                    "max-width": r[1].parent().width() - 10
                })
            },
            b = function() {
                var t = e(this),
                    o = t.data(a),
                    n = e("#mCSB_" + o.idx),
                    i = e("#mCSB_" + o.idx + "_container"),
                    r = [e("#mCSB_" + o.idx + "_dragger_vertical"), e("#mCSB_" + o.idx + "_dragger_horizontal")],
                    l = [i.outerHeight(!1) - n.height(), i.outerWidth(!1) - n.width()],
                    s = [l[0] / (r[0].parent().height() - r[0].height()), l[1] / (r[1].parent().width() - r[1].width())];
                o.scrollRatio = {
                    y: s[0],
                    x: s[1]
                }
            },
            C = function(e, t, o) {
                var a = o ? d[0] + "_expanded" : "",
                    n = e.closest(".mCSB_scrollTools");
                "active" === t ? (e.toggleClass(d[0] + " " + a), n.toggleClass(d[1]), e[0]._draggable = e[0]._draggable ? 0 : 1) : e[0]._draggable || ("hide" === t ? (e.removeClass(d[0]), n.removeClass(d[1])) : (e.addClass(d[0]), n.addClass(d[1])))
            },
            y = function() {
                var t = e(this),
                    o = t.data(a),
                    n = e("#mCSB_" + o.idx),
                    i = e("#mCSB_" + o.idx + "_container"),
                    r = null == o.overflowed ? i.height() : i.outerHeight(!1),
                    l = null == o.overflowed ? i.width() : i.outerWidth(!1),
                    s = i[0].scrollHeight,
                    c = i[0].scrollWidth;
                return s > r && (r = s), c > l && (l = c), [r > n.height(), l > n.width()]
            },
            B = function() {
                var t = e(this),
                    o = t.data(a),
                    n = o.opt,
                    i = e("#mCSB_" + o.idx),
                    r = e("#mCSB_" + o.idx + "_container"),
                    l = [e("#mCSB_" + o.idx + "_dragger_vertical"), e("#mCSB_" + o.idx + "_dragger_horizontal")];
                if (N(t), ("x" !== n.axis && !o.overflowed[0] || "y" === n.axis && o.overflowed[0]) && (l[0].add(r).css("top", 0), V(t, "_resetY")), "y" !== n.axis && !o.overflowed[1] || "x" === n.axis && o.overflowed[1]) {
                    var s = dx = 0;
                    "rtl" === o.langDir && (s = i.width() - r.outerWidth(!1), dx = Math.abs(s / o.scrollRatio.x)), r.css("left", s), l[1].css("left", dx), V(t, "_resetX")
                }
            },
            T = function() {
                function t() {
                    r = setTimeout(function() {
                        e.event.special.mousewheel ? (clearTimeout(r), E.call(o[0])) : t()
                    }, 100)
                }
                var o = e(this),
                    n = o.data(a),
                    i = n.opt;
                if (!n.bindEvents) {
                    if (I.call(this), i.contentTouchScroll && R.call(this), D.call(this), i.mouseWheel.enable) {
                        var r;
                        t()
                    }
                    z.call(this), P.call(this), i.advanced.autoScrollOnFocus && A.call(this), i.scrollButtons.enable && H.call(this), i.keyboard.enable && U.call(this), n.bindEvents = !0
                }
            },
            k = function() {
                var t = e(this),
                    o = t.data(a),
                    n = o.opt,
                    i = a + "_" + o.idx,
                    r = ".mCSB_" + o.idx + "_scrollbar",
                    l = e("#mCSB_" + o.idx + ",#mCSB_" + o.idx + "_container,#mCSB_" + o.idx + "_container_wrapper," + r + " ." + d[12] + ",#mCSB_" + o.idx + "_dragger_vertical,#mCSB_" + o.idx + "_dragger_horizontal," + r + ">a"),
                    s = e("#mCSB_" + o.idx + "_container");
                n.advanced.releaseDraggableSelectors && l.add(e(n.advanced.releaseDraggableSelectors)), o.bindEvents && (e(document).unbind("." + i), l.each(function() {
                    e(this).unbind("." + i)
                }), clearTimeout(t[0]._focusTimeout), K(t[0], "_focusTimeout"), clearTimeout(o.sequential.step), K(o.sequential, "step"), clearTimeout(s[0].onCompleteTimeout), K(s[0], "onCompleteTimeout"), o.bindEvents = !1)
            },
            M = function(t) {
                var o = e(this),
                    n = o.data(a),
                    i = n.opt,
                    r = e("#mCSB_" + n.idx + "_container_wrapper"),
                    l = r.length ? r : e("#mCSB_" + n.idx + "_container"),
                    s = [e("#mCSB_" + n.idx + "_scrollbar_vertical"), e("#mCSB_" + n.idx + "_scrollbar_horizontal")],
                    c = [s[0].find(".mCSB_dragger"), s[1].find(".mCSB_dragger")];
                "x" !== i.axis && (n.overflowed[0] && !t ? (s[0].add(c[0]).add(s[0].children("a")).css("display", "block"), l.removeClass(d[8] + " " + d[10])) : (i.alwaysShowScrollbar ? (2 !== i.alwaysShowScrollbar && c[0].css("display", "none"), l.removeClass(d[10])) : (s[0].css("display", "none"), l.addClass(d[10])), l.addClass(d[8]))), "y" !== i.axis && (n.overflowed[1] && !t ? (s[1].add(c[1]).add(s[1].children("a")).css("display", "block"), l.removeClass(d[9] + " " + d[11])) : (i.alwaysShowScrollbar ? (2 !== i.alwaysShowScrollbar && c[1].css("display", "none"), l.removeClass(d[11])) : (s[1].css("display", "none"), l.addClass(d[11])), l.addClass(d[9]))), n.overflowed[0] || n.overflowed[1] ? o.removeClass(d[5]) : o.addClass(d[5])
            },
            O = function(e) {
                var t = e.type;
                switch (t) {
                    case "pointerdown":
                    case "MSPointerDown":
                    case "pointermove":
                    case "MSPointerMove":
                    case "pointerup":
                    case "MSPointerUp":
                        return e.target.ownerDocument !== document ? [e.originalEvent.screenY, e.originalEvent.screenX, !1] : [e.originalEvent.pageY, e.originalEvent.pageX, !1];
                    case "touchstart":
                    case "touchmove":
                    case "touchend":
                        var o = e.originalEvent.touches[0] || e.originalEvent.changedTouches[0],
                            a = e.originalEvent.touches.length || e.originalEvent.changedTouches.length;
                        return e.target.ownerDocument !== document ? [o.screenY, o.screenX, a > 1] : [o.pageY, o.pageX, a > 1];
                    default:
                        return [e.pageY, e.pageX, !1]
                }
            },
            I = function() {
                function t(e) {
                    var t = m.find("iframe");
                    if (t.length) {
                        var o = e ? "auto" : "none";
                        t.css("pointer-events", o)
                    }
                }

                function o(e, t, o, a) {
                    if (m[0].idleTimer = u.scrollInertia < 233 ? 250 : 0, n.attr("id") === h[1]) var i = "x",
                        r = (n[0].offsetLeft - t + a) * d.scrollRatio.x;
                    else var i = "y",
                        r = (n[0].offsetTop - e + o) * d.scrollRatio.y;
                    V(l, r.toString(), {
                        dir: i,
                        drag: !0
                    })
                }
                var n, i, r, l = e(this),
                    d = l.data(a),
                    u = d.opt,
                    f = a + "_" + d.idx,
                    h = ["mCSB_" + d.idx + "_dragger_vertical", "mCSB_" + d.idx + "_dragger_horizontal"],
                    m = e("#mCSB_" + d.idx + "_container"),
                    p = e("#" + h[0] + ",#" + h[1]),
                    g = u.advanced.releaseDraggableSelectors ? p.add(e(u.advanced.releaseDraggableSelectors)) : p;
                p.bind("mousedown." + f + " touchstart." + f + " pointerdown." + f + " MSPointerDown." + f, function(o) {
                    if (o.stopImmediatePropagation(), o.preventDefault(), Z(o)) {
                        c = !0, s && (document.onselectstart = function() {
                            return !1
                        }), t(!1), N(l), n = e(this);
                        var a = n.offset(),
                            d = O(o)[0] - a.top,
                            f = O(o)[1] - a.left,
                            h = n.height() + a.top,
                            m = n.width() + a.left;
                        h > d && d > 0 && m > f && f > 0 && (i = d, r = f), C(n, "active", u.autoExpandScrollbar)
                    }
                }).bind("touchmove." + f, function(e) {
                    e.stopImmediatePropagation(), e.preventDefault();
                    var t = n.offset(),
                        a = O(e)[0] - t.top,
                        l = O(e)[1] - t.left;
                    o(i, r, a, l)
                }), e(document).bind("mousemove." + f + " pointermove." + f + " MSPointerMove." + f, function(e) {
                    if (n) {
                        var t = n.offset(),
                            a = O(e)[0] - t.top,
                            l = O(e)[1] - t.left;
                        if (i === a) return;
                        o(i, r, a, l)
                    }
                }).add(g).bind("mouseup." + f + " touchend." + f + " pointerup." + f + " MSPointerUp." + f, function(e) {
                    n && (C(n, "active", u.autoExpandScrollbar), n = null), c = !1, s && (document.onselectstart = null), t(!0)
                })
            },
            R = function() {
                function o(e) {
                    if (!$(e) || c || O(e)[2]) return void(t = 0);
                    t = 1, b = 0, C = 0, d = 1, y.removeClass("mCS_touch_action");
                    var o = I.offset();
                    u = O(e)[0] - o.top, f = O(e)[1] - o.left, A = [O(e)[0], O(e)[1]]
                }

                function n(e) {
                    if ($(e) && !c && !O(e)[2] && (e.stopImmediatePropagation(), (!C || b) && d)) {
                        g = G();
                        var t = M.offset(),
                            o = O(e)[0] - t.top,
                            a = O(e)[1] - t.left,
                            n = "mcsLinearOut";
                        if (D.push(o), E.push(a), A[2] = Math.abs(O(e)[0] - A[0]), A[3] = Math.abs(O(e)[1] - A[1]), B.overflowed[0]) var i = R[0].parent().height() - R[0].height(),
                            r = u - o > 0 && o - u > -(i * B.scrollRatio.y) && (2 * A[3] < A[2] || "yx" === T.axis);
                        if (B.overflowed[1]) var l = R[1].parent().width() - R[1].width(),
                            h = f - a > 0 && a - f > -(l * B.scrollRatio.x) && (2 * A[2] < A[3] || "yx" === T.axis);
                        r || h ? (U || e.preventDefault(), b = 1) : (C = 1, y.addClass("mCS_touch_action")), U && e.preventDefault(), w = "yx" === T.axis ? [u - o, f - a] : "x" === T.axis ? [null, f - a] : [u - o, null], I[0].idleTimer = 250, B.overflowed[0] && s(w[0], L, n, "y", "all", !0), B.overflowed[1] && s(w[1], L, n, "x", z, !0)
                    }
                }

                function i(e) {
                    if (!$(e) || c || O(e)[2]) return void(t = 0);
                    t = 1, e.stopImmediatePropagation(), N(y), p = G();
                    var o = M.offset();
                    h = O(e)[0] - o.top, m = O(e)[1] - o.left, D = [], E = []
                }

                function r(e) {
                    if ($(e) && !c && !O(e)[2]) {
                        d = 0, e.stopImmediatePropagation(), b = 0, C = 0, v = G();
                        var t = M.offset(),
                            o = O(e)[0] - t.top,
                            a = O(e)[1] - t.left;
                        if (!(v - g > 30)) {
                            _ = 1e3 / (v - p);
                            var n = "mcsEaseOut",
                                i = 2.5 > _,
                                r = i ? [D[D.length - 2], E[E.length - 2]] : [0, 0];
                            x = i ? [o - r[0], a - r[1]] : [o - h, a - m];
                            var u = [Math.abs(x[0]), Math.abs(x[1])];
                            _ = i ? [Math.abs(x[0] / 4), Math.abs(x[1] / 4)] : [_, _];
                            var f = [Math.abs(I[0].offsetTop) - x[0] * l(u[0] / _[0], _[0]), Math.abs(I[0].offsetLeft) - x[1] * l(u[1] / _[1], _[1])];
                            w = "yx" === T.axis ? [f[0], f[1]] : "x" === T.axis ? [null, f[1]] : [f[0], null], S = [4 * u[0] + T.scrollInertia, 4 * u[1] + T.scrollInertia];
                            var y = parseInt(T.contentTouchScroll) || 0;
                            w[0] = u[0] > y ? w[0] : 0, w[1] = u[1] > y ? w[1] : 0, B.overflowed[0] && s(w[0], S[0], n, "y", z, !1), B.overflowed[1] && s(w[1], S[1], n, "x", z, !1)
                        }
                    }
                }

                function l(e, t) {
                    var o = [1.5 * t, 2 * t, t / 1.5, t / 2];
                    return e > 90 ? t > 4 ? o[0] : o[3] : e > 60 ? t > 3 ? o[3] : o[2] : e > 30 ? t > 8 ? o[1] : t > 6 ? o[0] : t > 4 ? t : o[2] : t > 8 ? t : o[3]
                }

                function s(e, t, o, a, n, i) {
                    e && V(y, e.toString(), {
                        dur: t,
                        scrollEasing: o,
                        dir: a,
                        overwrite: n,
                        drag: i
                    })
                }
                var d, u, f, h, m, p, g, v, x, _, w, S, b, C, y = e(this),
                    B = y.data(a),
                    T = B.opt,
                    k = a + "_" + B.idx,
                    M = e("#mCSB_" + B.idx),
                    I = e("#mCSB_" + B.idx + "_container"),
                    R = [e("#mCSB_" + B.idx + "_dragger_vertical"), e("#mCSB_" + B.idx + "_dragger_horizontal")],
                    D = [],
                    E = [],
                    L = 0,
                    z = "yx" === T.axis ? "none" : "all",
                    A = [],
                    P = I.find("iframe"),
                    H = ["touchstart." + k + " pointerdown." + k + " MSPointerDown." + k, "touchmove." + k + " pointermove." + k + " MSPointerMove." + k, "touchend." + k + " pointerup." + k + " MSPointerUp." + k],
                    U = void 0 !== document.body.style.touchAction;
                I.bind(H[0], function(e) {
                    o(e)
                }).bind(H[1], function(e) {
                    n(e)
                }), M.bind(H[0], function(e) {
                    i(e)
                }).bind(H[2], function(e) {
                    r(e)
                }), P.length && P.each(function() {
                    e(this).load(function() {
                        W(this) && e(this.contentDocument || this.contentWindow.document).bind(H[0], function(e) {
                            o(e), i(e)
                        }).bind(H[1], function(e) {
                            n(e)
                        }).bind(H[2], function(e) {
                            r(e)
                        })
                    })
                })
            },
            D = function() {
                function o() {
                    return window.getSelection ? window.getSelection().toString() : document.selection && "Control" != document.selection.type ? document.selection.createRange().text : 0
                }

                function n(e, t, o) {
                    d.type = o && i ? "stepped" : "stepless", d.scrollAmount = 10, F(r, e, t, "mcsLinearOut", o ? 60 : null)
                }
                var i, r = e(this),
                    l = r.data(a),
                    s = l.opt,
                    d = l.sequential,
                    u = a + "_" + l.idx,
                    f = e("#mCSB_" + l.idx + "_container"),
                    h = f.parent();
                f.bind("mousedown." + u, function(e) {
                    t || i || (i = 1, c = !0)
                }).add(document).bind("mousemove." + u, function(e) {
                    if (!t && i && o()) {
                        var a = f.offset(),
                            r = O(e)[0] - a.top + f[0].offsetTop,
                            c = O(e)[1] - a.left + f[0].offsetLeft;
                        r > 0 && r < h.height() && c > 0 && c < h.width() ? d.step && n("off", null, "stepped") : ("x" !== s.axis && l.overflowed[0] && (0 > r ? n("on", 38) : r > h.height() && n("on", 40)), "y" !== s.axis && l.overflowed[1] && (0 > c ? n("on", 37) : c > h.width() && n("on", 39)))
                    }
                }).bind("mouseup." + u + " dragend." + u, function(e) {
                    t || (i && (i = 0, n("off", null)), c = !1)
                })
            },
            E = function() {
                function t(t, a) {
                    if (N(o), !L(o, t.target)) {
                        var r = "auto" !== i.mouseWheel.deltaFactor ? parseInt(i.mouseWheel.deltaFactor) : s && t.deltaFactor < 100 ? 100 : t.deltaFactor || 100;
                        if ("x" === i.axis || "x" === i.mouseWheel.axis) var d = "x",
                            u = [Math.round(r * n.scrollRatio.x), parseInt(i.mouseWheel.scrollAmount)],
                            f = "auto" !== i.mouseWheel.scrollAmount ? u[1] : u[0] >= l.width() ? .9 * l.width() : u[0],
                            h = Math.abs(e("#mCSB_" + n.idx + "_container")[0].offsetLeft),
                            m = c[1][0].offsetLeft,
                            p = c[1].parent().width() - c[1].width(),
                            g = t.deltaX || t.deltaY || a;
                        else var d = "y",
                            u = [Math.round(r * n.scrollRatio.y), parseInt(i.mouseWheel.scrollAmount)],
                            f = "auto" !== i.mouseWheel.scrollAmount ? u[1] : u[0] >= l.height() ? .9 * l.height() : u[0],
                            h = Math.abs(e("#mCSB_" + n.idx + "_container")[0].offsetTop),
                            m = c[0][0].offsetTop,
                            p = c[0].parent().height() - c[0].height(),
                            g = t.deltaY || a;
                        "y" === d && !n.overflowed[0] || "x" === d && !n.overflowed[1] || ((i.mouseWheel.invert || t.webkitDirectionInvertedFromDevice) && (g = -g), i.mouseWheel.normalizeDelta && (g = 0 > g ? -1 : 1), (g > 0 && 0 !== m || 0 > g && m !== p || i.mouseWheel.preventDefault) && (t.stopImmediatePropagation(), t.preventDefault()), V(o, (h - g * f).toString(), {
                            dir: d
                        }))
                    }
                }
                if (e(this).data(a)) {
                    var o = e(this),
                        n = o.data(a),
                        i = n.opt,
                        r = a + "_" + n.idx,
                        l = e("#mCSB_" + n.idx),
                        c = [e("#mCSB_" + n.idx + "_dragger_vertical"), e("#mCSB_" + n.idx + "_dragger_horizontal")],
                        d = e("#mCSB_" + n.idx + "_container").find("iframe");
                    d.length && d.each(function() {
                        e(this).load(function() {
                            W(this) && e(this.contentDocument || this.contentWindow.document).bind("mousewheel." + r, function(e, o) {
                                t(e, o)
                            })
                        })
                    }), l.bind("mousewheel." + r, function(e, o) {
                        t(e, o)
                    })
                }
            },
            W = function(e) {
                var t = null;
                try {
                    var o = e.contentDocument || e.contentWindow.document;
                    t = o.body.innerHTML
                } catch (a) {}
                return null !== t
            },
            L = function(t, o) {
                var n = o.nodeName.toLowerCase(),
                    i = t.data(a).opt.mouseWheel.disableOver,
                    r = ["select", "textarea"];
                return e.inArray(n, i) > -1 && !(e.inArray(n, r) > -1 && !e(o).is(":focus"))
            },
            z = function() {
                var t = e(this),
                    o = t.data(a),
                    n = a + "_" + o.idx,
                    i = e("#mCSB_" + o.idx + "_container"),
                    r = i.parent(),
                    l = e(".mCSB_" + o.idx + "_scrollbar ." + d[12]);
                l.bind("touchstart." + n + " pointerdown." + n + " MSPointerDown." + n, function(e) {
                    c = !0
                }).bind("touchend." + n + " pointerup." + n + " MSPointerUp." + n, function(e) {
                    c = !1
                }).bind("click." + n, function(a) {
                    if (e(a.target).hasClass(d[12]) || e(a.target).hasClass("mCSB_draggerRail")) {
                        N(t);
                        var n = e(this),
                            l = n.find(".mCSB_dragger");
                        if (n.parent(".mCSB_scrollTools_horizontal").length > 0) {
                            if (!o.overflowed[1]) return;
                            var s = "x",
                                c = a.pageX > l.offset().left ? -1 : 1,
                                u = Math.abs(i[0].offsetLeft) - .9 * c * r.width()
                        } else {
                            if (!o.overflowed[0]) return;
                            var s = "y",
                                c = a.pageY > l.offset().top ? -1 : 1,
                                u = Math.abs(i[0].offsetTop) - .9 * c * r.height()
                        }
                        V(t, u.toString(), {
                            dir: s,
                            scrollEasing: "mcsEaseInOut"
                        })
                    }
                })
            },
            A = function() {
                var t = e(this),
                    o = t.data(a),
                    n = o.opt,
                    i = a + "_" + o.idx,
                    r = e("#mCSB_" + o.idx + "_container"),
                    l = r.parent();
                r.bind("focusin." + i, function(o) {
                    var a = e(document.activeElement),
                        i = r.find(".mCustomScrollBox").length,
                        s = 0;
                    a.is(n.advanced.autoScrollOnFocus) && (N(t), clearTimeout(t[0]._focusTimeout), t[0]._focusTimer = i ? (s + 17) * i : 0, t[0]._focusTimeout = setTimeout(function() {
                        var e = [te(a)[0], te(a)[1]],
                            o = [r[0].offsetTop, r[0].offsetLeft],
                            i = [o[0] + e[0] >= 0 && o[0] + e[0] < l.height() - a.outerHeight(!1), o[1] + e[1] >= 0 && o[0] + e[1] < l.width() - a.outerWidth(!1)],
                            c = "yx" !== n.axis || i[0] || i[1] ? "all" : "none";
                        "x" === n.axis || i[0] || V(t, e[0].toString(), {
                            dir: "y",
                            scrollEasing: "mcsEaseInOut",
                            overwrite: c,
                            dur: s
                        }), "y" === n.axis || i[1] || V(t, e[1].toString(), {
                            dir: "x",
                            scrollEasing: "mcsEaseInOut",
                            overwrite: c,
                            dur: s
                        })
                    }, t[0]._focusTimer))
                })
            },
            P = function() {
                var t = e(this),
                    o = t.data(a),
                    n = a + "_" + o.idx,
                    i = e("#mCSB_" + o.idx + "_container").parent();
                i.bind("scroll." + n, function(t) {
                    (0 !== i.scrollTop() || 0 !== i.scrollLeft()) && e(".mCSB_" + o.idx + "_scrollbar").css("visibility", "hidden")
                })
            },
            H = function() {
                var t = e(this),
                    o = t.data(a),
                    n = o.opt,
                    i = o.sequential,
                    r = a + "_" + o.idx,
                    l = ".mCSB_" + o.idx + "_scrollbar",
                    s = e(l + ">a");
                s.bind("mousedown." + r + " touchstart." + r + " pointerdown." + r + " MSPointerDown." + r + " mouseup." + r + " touchend." + r + " pointerup." + r + " MSPointerUp." + r + " mouseout." + r + " pointerout." + r + " MSPointerOut." + r + " click." + r, function(a) {
                    function r(e, o) {
                        i.scrollAmount = n.snapAmount || n.scrollButtons.scrollAmount, F(t, e, o)
                    }
                    if (a.preventDefault(), Z(a)) {
                        var l = e(this).attr("class");
                        switch (i.type = n.scrollButtons.scrollType, a.type) {
                            case "mousedown":
                            case "touchstart":
                            case "pointerdown":
                            case "MSPointerDown":
                                if ("stepped" === i.type) return;
                                c = !0, o.tweenRunning = !1, r("on", l);
                                break;
                            case "mouseup":
                            case "touchend":
                            case "pointerup":
                            case "MSPointerUp":
                            case "mouseout":
                            case "pointerout":
                            case "MSPointerOut":
                                if ("stepped" === i.type) return;
                                c = !1, i.dir && r("off", l);
                                break;
                            case "click":
                                if ("stepped" !== i.type || o.tweenRunning) return;
                                r("on", l)
                        }
                    }
                })
            },
            U = function() {
                function t(t) {
                    function a(e, t) {
                        r.type = i.keyboard.scrollType, r.scrollAmount = i.snapAmount || i.keyboard.scrollAmount, "stepped" === r.type && n.tweenRunning || F(o, e, t)
                    }
                    switch (t.type) {
                        case "blur":
                            n.tweenRunning && r.dir && a("off", null);
                            break;
                        case "keydown":
                        case "keyup":
                            var l = t.keyCode ? t.keyCode : t.which,
                                s = "on";
                            if ("x" !== i.axis && (38 === l || 40 === l) || "y" !== i.axis && (37 === l || 39 === l)) {
                                if ((38 === l || 40 === l) && !n.overflowed[0] || (37 === l || 39 === l) && !n.overflowed[1]) return;
                                "keyup" === t.type && (s = "off"), e(document.activeElement).is(u) || (t.preventDefault(), t.stopImmediatePropagation(), a(s, l))
                            } else if (33 === l || 34 === l) {
                                if ((n.overflowed[0] || n.overflowed[1]) && (t.preventDefault(), t.stopImmediatePropagation()), "keyup" === t.type) {
                                    N(o);
                                    var f = 34 === l ? -1 : 1;
                                    if ("x" === i.axis || "yx" === i.axis && n.overflowed[1] && !n.overflowed[0]) var h = "x",
                                        m = Math.abs(c[0].offsetLeft) - .9 * f * d.width();
                                    else var h = "y",
                                        m = Math.abs(c[0].offsetTop) - .9 * f * d.height();
                                    V(o, m.toString(), {
                                        dir: h,
                                        scrollEasing: "mcsEaseInOut"
                                    })
                                }
                            } else if ((35 === l || 36 === l) && !e(document.activeElement).is(u) && ((n.overflowed[0] || n.overflowed[1]) && (t.preventDefault(), t.stopImmediatePropagation()), "keyup" === t.type)) {
                                if ("x" === i.axis || "yx" === i.axis && n.overflowed[1] && !n.overflowed[0]) var h = "x",
                                    m = 35 === l ? Math.abs(d.width() - c.outerWidth(!1)) : 0;
                                else var h = "y",
                                    m = 35 === l ? Math.abs(d.height() - c.outerHeight(!1)) : 0;
                                V(o, m.toString(), {
                                    dir: h,
                                    scrollEasing: "mcsEaseInOut"
                                })
                            }
                    }
                }
                var o = e(this),
                    n = o.data(a),
                    i = n.opt,
                    r = n.sequential,
                    l = a + "_" + n.idx,
                    s = e("#mCSB_" + n.idx),
                    c = e("#mCSB_" + n.idx + "_container"),
                    d = c.parent(),
                    u = "input,textarea,select,datalist,keygen,[contenteditable='true']",
                    f = c.find("iframe"),
                    h = ["blur." + l + " keydown." + l + " keyup." + l];
                f.length && f.each(function() {
                    e(this).load(function() {
                        W(this) && e(this.contentDocument || this.contentWindow.document).bind(h[0], function(e) {
                            t(e)
                        })
                    })
                }), s.attr("tabindex", "0").bind(h[0], function(e) {
                    t(e)
                })
            },
            F = function(t, o, n, i, r) {
                function l(e) {
                    var o = "stepped" !== f.type,
                        a = r ? r : e ? o ? p / 1.5 : g : 1e3 / 60,
                        n = e ? o ? 7.5 : 40 : 2.5,
                        s = [Math.abs(h[0].offsetTop), Math.abs(h[0].offsetLeft)],
                        d = [c.scrollRatio.y > 10 ? 10 : c.scrollRatio.y, c.scrollRatio.x > 10 ? 10 : c.scrollRatio.x],
                        u = "x" === f.dir[0] ? s[1] + f.dir[1] * d[1] * n : s[0] + f.dir[1] * d[0] * n,
                        m = "x" === f.dir[0] ? s[1] + f.dir[1] * parseInt(f.scrollAmount) : s[0] + f.dir[1] * parseInt(f.scrollAmount),
                        v = "auto" !== f.scrollAmount ? m : u,
                        x = i ? i : e ? o ? "mcsLinearOut" : "mcsEaseInOut" : "mcsLinear",
                        _ = e ? !0 : !1;
                    return e && 17 > a && (v = "x" === f.dir[0] ? s[1] : s[0]), V(t, v.toString(), {
                        dir: f.dir[0],
                        scrollEasing: x,
                        dur: a,
                        onComplete: _
                    }), e ? void(f.dir = !1) : (clearTimeout(f.step), void(f.step = setTimeout(function() {
                        l()
                    }, a)))
                }

                function s() {
                    clearTimeout(f.step), K(f, "step"), N(t)
                }
                var c = t.data(a),
                    u = c.opt,
                    f = c.sequential,
                    h = e("#mCSB_" + c.idx + "_container"),
                    m = "stepped" === f.type ? !0 : !1,
                    p = u.scrollInertia < 26 ? 26 : u.scrollInertia,
                    g = u.scrollInertia < 1 ? 17 : u.scrollInertia;
                switch (o) {
                    case "on":
                        if (f.dir = [n === d[16] || n === d[15] || 39 === n || 37 === n ? "x" : "y", n === d[13] || n === d[15] || 38 === n || 37 === n ? -1 : 1], N(t), ee(n) && "stepped" === f.type) return;
                        l(m);
                        break;
                    case "off":
                        s(), (m || c.tweenRunning && f.dir) && l(!0)
                }
            },
            q = function(t) {
                var o = e(this).data(a).opt,
                    n = [];
                return "function" == typeof t && (t = t()), t instanceof Array ? n = t.length > 1 ? [t[0], t[1]] : "x" === o.axis ? [null, t[0]] : [t[0], null] : (n[0] = t.y ? t.y : t.x || "x" === o.axis ? null : t, n[1] = t.x ? t.x : t.y || "y" === o.axis ? null : t), "function" == typeof n[0] && (n[0] = n[0]()), "function" == typeof n[1] && (n[1] = n[1]()), n
            },
            Y = function(t, o) {
                if (null != t && "undefined" != typeof t) {
                    var n = e(this),
                        i = n.data(a),
                        r = i.opt,
                        l = e("#mCSB_" + i.idx + "_container"),
                        s = l.parent(),
                        c = typeof t;
                    o || (o = "x" === r.axis ? "x" : "y");
                    var d = "x" === o ? l.outerWidth(!1) : l.outerHeight(!1),
                        f = "x" === o ? l[0].offsetLeft : l[0].offsetTop,
                        h = "x" === o ? "left" : "top";
                    switch (c) {
                        case "function":
                            return t();
                        case "object":
                            var m = t.jquery ? t : e(t);
                            if (!m.length) return;
                            return "x" === o ? te(m)[1] : te(m)[0];
                        case "string":
                        case "number":
                            if (ee(t)) return Math.abs(t);
                            if (-1 !== t.indexOf("%")) return Math.abs(d * parseInt(t) / 100);
                            if (-1 !== t.indexOf("-=")) return Math.abs(f - parseInt(t.split("-=")[1]));
                            if (-1 !== t.indexOf("+=")) {
                                var p = f + parseInt(t.split("+=")[1]);
                                return p >= 0 ? 0 : Math.abs(p)
                            }
                            if (-1 !== t.indexOf("px") && ee(t.split("px")[0])) return Math.abs(t.split("px")[0]);
                            if ("top" === t || "left" === t) return 0;
                            if ("bottom" === t) return Math.abs(s.height() - l.outerHeight(!1));
                            if ("right" === t) return Math.abs(s.width() - l.outerWidth(!1));
                            if ("first" === t || "last" === t) {
                                var m = l.find(":" + t);
                                return "x" === o ? te(m)[1] : te(m)[0]
                            }
                            return e(t).length ? "x" === o ? te(e(t))[1] : te(e(t))[0] : (l.css(h, t), void u.update.call(null, n[0]))
                    }
                }
            },
            j = function(t) {
                function o() {
                    return clearTimeout(f[0].autoUpdate), 0 === l.parents("html").length ? void(l = null) : void(f[0].autoUpdate = setTimeout(function() {
                        return c.advanced.updateOnSelectorChange && (s.poll.change.n = i(), s.poll.change.n !== s.poll.change.o) ? (s.poll.change.o = s.poll.change.n, void r(3)) : c.advanced.updateOnContentResize && (s.poll.size.n = l[0].scrollHeight + l[0].scrollWidth + f[0].offsetHeight + l[0].offsetHeight, s.poll.size.n !== s.poll.size.o) ? (s.poll.size.o = s.poll.size.n, void r(1)) : !c.advanced.updateOnImageLoad || "auto" === c.advanced.updateOnImageLoad && "y" === c.axis || (s.poll.img.n = f.find("img").length, s.poll.img.n === s.poll.img.o) ? void((c.advanced.updateOnSelectorChange || c.advanced.updateOnContentResize || c.advanced.updateOnImageLoad) && o()) : (s.poll.img.o = s.poll.img.n, void f.find("img").each(function() {
                            n(this)
                        }))
                    }, c.advanced.autoUpdateTimeout))
                }

                function n(t) {
                    function o(e, t) {
                        return function() {
                            return t.apply(e, arguments)
                        }
                    }

                    function a() {
                        this.onload = null, e(t).addClass(d[2]), r(2)
                    }
                    if (e(t).hasClass(d[2])) return void r();
                    var n = new Image;
                    n.onload = o(n, a), n.src = t.src
                }

                function i() {
                    c.advanced.updateOnSelectorChange === !0 && (c.advanced.updateOnSelectorChange = "*");
                    var e = 0,
                        t = f.find(c.advanced.updateOnSelectorChange);
                    return c.advanced.updateOnSelectorChange && t.length > 0 && t.each(function() {
                        e += this.offsetHeight + this.offsetWidth
                    }), e
                }

                function r(e) {
                    clearTimeout(f[0].autoUpdate), u.update.call(null, l[0], e)
                }
                var l = e(this),
                    s = l.data(a),
                    c = s.opt,
                    f = e("#mCSB_" + s.idx + "_container");
                return t ? (clearTimeout(f[0].autoUpdate), void K(f[0], "autoUpdate")) : void o()
            },
            X = function(e, t, o) {
                return Math.round(e / t) * t - o
            },
            N = function(t) {
                var o = t.data(a),
                    n = e("#mCSB_" + o.idx + "_container,#mCSB_" + o.idx + "_container_wrapper,#mCSB_" + o.idx + "_dragger_vertical,#mCSB_" + o.idx + "_dragger_horizontal");
                n.each(function() {
                    J.call(this)
                })
            },
            V = function(t, o, n) {
                function i(e) {
                    return s && c.callbacks[e] && "function" == typeof c.callbacks[e]
                }

                function r() {
                    return [c.callbacks.alwaysTriggerOffsets || _ >= w[0] + b, c.callbacks.alwaysTriggerOffsets || -y >= _]
                }

                function l() {
                    var e = [h[0].offsetTop, h[0].offsetLeft],
                        o = [v[0].offsetTop, v[0].offsetLeft],
                        a = [h.outerHeight(!1), h.outerWidth(!1)],
                        i = [f.height(), f.width()];
                    t[0].mcs = {
                        content: h,
                        top: e[0],
                        left: e[1],
                        draggerTop: o[0],
                        draggerLeft: o[1],
                        topPct: Math.round(100 * Math.abs(e[0]) / (Math.abs(a[0]) - i[0])),
                        leftPct: Math.round(100 * Math.abs(e[1]) / (Math.abs(a[1]) - i[1])),
                        direction: n.dir
                    }
                }
                var s = t.data(a),
                    c = s.opt,
                    d = {
                        trigger: "internal",
                        dir: "y",
                        scrollEasing: "mcsEaseOut",
                        drag: !1,
                        dur: c.scrollInertia,
                        overwrite: "all",
                        callbacks: !0,
                        onStart: !0,
                        onUpdate: !0,
                        onComplete: !0
                    },
                    n = e.extend(d, n),
                    u = [n.dur, n.drag ? 0 : n.dur],
                    f = e("#mCSB_" + s.idx),
                    h = e("#mCSB_" + s.idx + "_container"),
                    m = h.parent(),
                    p = c.callbacks.onTotalScrollOffset ? q.call(t, c.callbacks.onTotalScrollOffset) : [0, 0],
                    g = c.callbacks.onTotalScrollBackOffset ? q.call(t, c.callbacks.onTotalScrollBackOffset) : [0, 0];
                if (s.trigger = n.trigger, (0 !== m.scrollTop() || 0 !== m.scrollLeft()) && (e(".mCSB_" + s.idx + "_scrollbar").css("visibility", "visible"), m.scrollTop(0).scrollLeft(0)), "_resetY" !== o || s.contentReset.y || (i("onOverflowYNone") && c.callbacks.onOverflowYNone.call(t[0]), s.contentReset.y = 1), "_resetX" !== o || s.contentReset.x || (i("onOverflowXNone") && c.callbacks.onOverflowXNone.call(t[0]), s.contentReset.x = 1), "_resetY" !== o && "_resetX" !== o) {
                    switch (!s.contentReset.y && t[0].mcs || !s.overflowed[0] || (i("onOverflowY") && c.callbacks.onOverflowY.call(t[0]), s.contentReset.x = null), !s.contentReset.x && t[0].mcs || !s.overflowed[1] || (i("onOverflowX") && c.callbacks.onOverflowX.call(t[0]), s.contentReset.x = null), c.snapAmount && (o = X(o, c.snapAmount, c.snapOffset)), n.dir) {
                        case "x":
                            var v = e("#mCSB_" + s.idx + "_dragger_horizontal"),
                                x = "left",
                                _ = h[0].offsetLeft,
                                w = [f.width() - h.outerWidth(!1), v.parent().width() - v.width()],
                                S = [o, 0 === o ? 0 : o / s.scrollRatio.x],
                                b = p[1],
                                y = g[1],
                                B = b > 0 ? b / s.scrollRatio.x : 0,
                                T = y > 0 ? y / s.scrollRatio.x : 0;
                            break;
                        case "y":
                            var v = e("#mCSB_" + s.idx + "_dragger_vertical"),
                                x = "top",
                                _ = h[0].offsetTop,
                                w = [f.height() - h.outerHeight(!1), v.parent().height() - v.height()],
                                S = [o, 0 === o ? 0 : o / s.scrollRatio.y],
                                b = p[0],
                                y = g[0],
                                B = b > 0 ? b / s.scrollRatio.y : 0,
                                T = y > 0 ? y / s.scrollRatio.y : 0
                    }
                    S[1] < 0 || 0 === S[0] && 0 === S[1] ? S = [0, 0] : S[1] >= w[1] ? S = [w[0], w[1]] : S[0] = -S[0], t[0].mcs || (l(), i("onInit") && c.callbacks.onInit.call(t[0])), clearTimeout(h[0].onCompleteTimeout), (s.tweenRunning || !(0 === _ && S[0] >= 0 || _ === w[0] && S[0] <= w[0])) && (Q(v[0], x, Math.round(S[1]), u[1], n.scrollEasing), Q(h[0], x, Math.round(S[0]), u[0], n.scrollEasing, n.overwrite, {
                        onStart: function() {
                            n.callbacks && n.onStart && !s.tweenRunning && (i("onScrollStart") && (l(), c.callbacks.onScrollStart.call(t[0])), s.tweenRunning = !0, C(v), s.cbOffsets = r())
                        },
                        onUpdate: function() {
                            n.callbacks && n.onUpdate && i("whileScrolling") && (l(), c.callbacks.whileScrolling.call(t[0]))
                        },
                        onComplete: function() {
                            if (n.callbacks && n.onComplete) {
                                "yx" === c.axis && clearTimeout(h[0].onCompleteTimeout);
                                var e = h[0].idleTimer || 0;
                                h[0].onCompleteTimeout = setTimeout(function() {
                                    i("onScroll") && (l(), c.callbacks.onScroll.call(t[0])), i("onTotalScroll") && S[1] >= w[1] - B && s.cbOffsets[0] && (l(), c.callbacks.onTotalScroll.call(t[0])), i("onTotalScrollBack") && S[1] <= T && s.cbOffsets[1] && (l(), c.callbacks.onTotalScrollBack.call(t[0])), s.tweenRunning = !1, h[0].idleTimer = 0, C(v, "hide")
                                }, e)
                            }
                        }
                    }))
                }
            },
            Q = function(e, t, o, a, n, i, r) {
                function l() {
                    S.stop || (x || m.call(), x = G() - v, s(), x >= S.time && (S.time = x > S.time ? x + f - (x - S.time) : x + f - 1, S.time < x + 1 && (S.time = x + 1)), S.time < a ? S.id = h(l) : g.call())
                }

                function s() {
                    a > 0 ? (S.currVal = u(S.time, _, b, a, n), w[t] = Math.round(S.currVal) + "px") : w[t] = o + "px", p.call()
                }

                function c() {
                    f = 1e3 / 60, S.time = x + f, h = window.requestAnimationFrame ? window.requestAnimationFrame : function(e) {
                        return s(), setTimeout(e, .01)
                    }, S.id = h(l)
                }

                function d() {
                    null != S.id && (window.requestAnimationFrame ? window.cancelAnimationFrame(S.id) : clearTimeout(S.id), S.id = null)
                }

                function u(e, t, o, a, n) {
                    switch (n) {
                        case "linear":
                        case "mcsLinear":
                            return o * e / a + t;
                        case "mcsLinearOut":
                            return e /= a, e--, o * Math.sqrt(1 - e * e) + t;
                        case "easeInOutSmooth":
                            return e /= a / 2, 1 > e ? o / 2 * e * e + t : (e--, -o / 2 * (e * (e - 2) - 1) + t);
                        case "easeInOutStrong":
                            return e /= a / 2, 1 > e ? o / 2 * Math.pow(2, 10 * (e - 1)) + t : (e--, o / 2 * (-Math.pow(2, -10 * e) + 2) + t);
                        case "easeInOut":
                        case "mcsEaseInOut":
                            return e /= a / 2, 1 > e ? o / 2 * e * e * e + t : (e -= 2, o / 2 * (e * e * e + 2) + t);
                        case "easeOutSmooth":
                            return e /= a, e--, -o * (e * e * e * e - 1) + t;
                        case "easeOutStrong":
                            return o * (-Math.pow(2, -10 * e / a) + 1) + t;
                        case "easeOut":
                        case "mcsEaseOut":
                        default:
                            var i = (e /= a) * e,
                                r = i * e;
                            return t + o * (.499999999999997 * r * i + -2.5 * i * i + 5.5 * r + -6.5 * i + 4 * e)
                    }
                }
                e._mTween || (e._mTween = {
                    top: {},
                    left: {}
                });
                var f, h, r = r || {},
                    m = r.onStart || function() {},
                    p = r.onUpdate || function() {},
                    g = r.onComplete || function() {},
                    v = G(),
                    x = 0,
                    _ = e.offsetTop,
                    w = e.style,
                    S = e._mTween[t];
                "left" === t && (_ = e.offsetLeft);
                var b = o - _;
                S.stop = 0, "none" !== i && d(), c()
            },
            G = function() {
                return window.performance && window.performance.now ? window.performance.now() : window.performance && window.performance.webkitNow ? window.performance.webkitNow() : Date.now ? Date.now() : (new Date).getTime()
            },
            J = function() {
                var e = this;
                e._mTween || (e._mTween = {
                    top: {},
                    left: {}
                });
                for (var t = ["top", "left"], o = 0; o < t.length; o++) {
                    var a = t[o];
                    e._mTween[a].id && (window.requestAnimationFrame ? window.cancelAnimationFrame(e._mTween[a].id) : clearTimeout(e._mTween[a].id), e._mTween[a].id = null, e._mTween[a].stop = 1)
                }
            },
            K = function(e, t) {
                try {
                    delete e[t]
                } catch (o) {
                    e[t] = null
                }
            },
            Z = function(e) {
                return !(e.which && 1 !== e.which)
            },
            $ = function(e) {
                var t = e.originalEvent.pointerType;
                return !(t && "touch" !== t && 2 !== t)
            },
            ee = function(e) {
                return !isNaN(parseFloat(e)) && isFinite(e)
            },
            te = function(e) {
                var t = e.parents(".mCSB_container");
                return [e.offset().top - t.offset().top, e.offset().left - t.offset().left]
            };
        e.fn[o] = function(t) {
            return u[t] ? u[t].apply(this, Array.prototype.slice.call(arguments, 1)) : "object" != typeof t && t ? void e.error("Method " + t + " does not exist") : u.init.apply(this, arguments)
        }, e[o] = function(t) {
            return u[t] ? u[t].apply(this, Array.prototype.slice.call(arguments, 1)) : "object" != typeof t && t ? void e.error("Method " + t + " does not exist") : u.init.apply(this, arguments)
        }, e[o].defaults = i, window[o] = !0, e(window).load(function() {
            e(n)[o](), e.extend(e.expr[":"], {
                mcsInView: e.expr[":"].mcsInView || function(t) {
                    var o, a, n = e(t),
                        i = n.parents(".mCSB_container");
                    if (i.length) return o = i.parent(), a = [i[0].offsetTop, i[0].offsetLeft], a[0] + te(n)[0] >= 0 && a[0] + te(n)[0] < o.height() - n.outerHeight(!1) && a[1] + te(n)[1] >= 0 && a[1] + te(n)[1] < o.width() - n.outerWidth(!1)
                },
                mcsOverflow: e.expr[":"].mcsOverflow || function(t) {
                    var o = e(t).data(a);
                    if (o) return o.overflowed[0] || o.overflowed[1]
                }
            })
        })
    })
});

;// JS/jquery.cookie.js
// jquery.cookie.js
jQuery.cookie = function(n, t, i) {
    var f, r, e, o, u, s;
    if (typeof t != "undefined") {
        i = i || {}, t === null && (t = "", i.expires = -1), f = "", i.expires && (typeof i.expires == "number" || i.expires.toUTCString) && (typeof i.expires == "number" ? (r = new Date, r.setTime(r.getTime() + i.expires * 864e5)) : r = i.expires, f = "; expires=" + r.toUTCString());
        var h = i.path ? "; path=" + i.path : "",
            c = i.domain ? "; domain=" + i.domain : "",
            l = i.secure ? "; secure" : "";
        document.cookie = [n, "=", encodeURIComponent(t), f, h, c, l].join("")
    } else {
        if (e = null, document.cookie && document.cookie != "")
            for (o = document.cookie.split(";"), u = 0; u < o.length; u++)
                if (s = jQuery.trim(o[u]), s.substring(0, n.length + 1) == n + "=") {
                    e = decodeURIComponent(s.substring(n.length + 1));
                    break
                } return e
    }
};

;// JS/leancore/libs/underscore-min.js
// leancore/libs/underscore-min.js
(function() {
    function et(t) {
        function r(n, i, r, u, f, e) {
            for (; f >= 0 && e > f; f += t) {
                var o = u ? u[f] : f;
                r = i(r, n[o], o, n)
            }
            return r
        }
        return function(u, f, o, s) {
            f = e(f, s, 4);
            var h = !i(u) && n.keys(u),
                l = (h || u).length,
                c = t > 0 ? 0 : l - 1;
            return arguments.length < 3 && (o = u[h ? h[c] : c], c += t), r(u, f, o, h, c, l)
        }
    }

    function rt(n) {
        return function(i, r, f) {
            r = t(r, f);
            for (var o = u(i), e = n > 0 ? 0 : o - 1; e >= 0 && o > e; e += n)
                if (r(i[e], e, i)) return e;
            return -1
        }
    }

    function ut(t, i, f) {
        return function(e, o, s) {
            var c = 0,
                h = u(e);
            if ("number" == typeof s) t > 0 ? c = s >= 0 ? s : Math.max(s + h, c) : h = s >= 0 ? Math.min(s + 1, h) : s + h + 1;
            else if (f && s && h) return s = f(e, o), e[s] === o ? s : -1;
            if (o !== o) return s = i(r.call(e, c, h), n.isNaN), s >= 0 ? s + c : -1;
            for (s = t > 0 ? c : h - 1; s >= 0 && h > s; s += t)
                if (e[s] === o) return s;
            return -1
        }
    }

    function ft(t, i) {
        var u = k.length,
            f = t.constructor,
            e = n.isFunction(f) && f.prototype || w,
            r = "constructor";
        for (n.has(t, r) && !n.contains(i, r) && i.push(r); u--;) r = k[u], r in t && t[r] !== e[r] && !n.contains(i, r) && i.push(r)
    }
    var a = this,
        lt = a._,
        l = Array.prototype,
        w = Object.prototype,
        gt = Function.prototype,
        dt = l.push,
        r = l.slice,
        o = w.toString,
        wt = w.hasOwnProperty,
        pt = Array.isArray,
        ct = Object.keys,
        y = gt.bind,
        ht = Object.create,
        p = function() {},
        n = function(t) {
            return t instanceof n ? t : this instanceof n ? void(this._wrapped = t) : new n(t)
        },
        e, t, h, f, g, d, k, s, nt, c;
    "undefined" != typeof exports ? ("undefined" != typeof module && module.exports && (exports = module.exports = n), exports._ = n) : a._ = n, n.VERSION = "1.8.3", e = function(n, t, i) {
        if (t === void 0) return n;
        switch (null == i ? 3 : i) {
            case 1:
                return function(i) {
                    return n.call(t, i)
                };
            case 2:
                return function(i, r) {
                    return n.call(t, i, r)
                };
            case 3:
                return function(i, r, u) {
                    return n.call(t, i, r, u)
                };
            case 4:
                return function(i, r, u, f) {
                    return n.call(t, i, r, u, f)
                }
        }
        return function() {
            return n.apply(t, arguments)
        }
    }, t = function(t, i, r) {
        return null == t ? n.identity : n.isFunction(t) ? e(t, i, r) : n.isObject(t) ? n.matcher(t) : n.property(t)
    }, n.iteratee = function(n, i) {
        return t(n, i, 1 / 0)
    };
    var b = function(n, t) {
            return function(i) {
                var e = arguments.length,
                    r, u;
                if (2 > e || null == i) return i;
                for (r = 1; e > r; r++)
                    for (var o = arguments[r], s = n(o), h = s.length, f = 0; h > f; f++) u = s[f], t && i[u] !== void 0 || (i[u] = o[u]);
                return i
            }
        },
        st = function(t) {
            if (!n.isObject(t)) return {};
            if (ht) return ht(t);
            p.prototype = t;
            var i = new p;
            return p.prototype = null, i
        },
        ot = function(n) {
            return function(t) {
                if (null != t) return t[n]
            }
        },
        at = Math.pow(2, 53) - 1,
        u = ot("length"),
        i = function(n) {
            var t = u(n);
            return "number" == typeof t && t >= 0 && at >= t
        };
    n.each = n.forEach = function(t, r, u) {
        var f, o, s;
        if (r = e(r, u), i(t))
            for (f = 0, o = t.length; o > f; f++) r(t[f], f, t);
        else
            for (s = n.keys(t), f = 0, o = s.length; o > f; f++) r(t[s[f]], s[f], t);
        return t
    }, n.map = n.collect = function(r, u, f) {
        var s;
        u = t(u, f);
        for (var o = !i(r) && n.keys(r), h = (o || r).length, c = Array(h), e = 0; h > e; e++) s = o ? o[e] : e, c[e] = u(r[s], s, r);
        return c
    }, n.reduce = n.foldl = n.inject = et(1), n.reduceRight = n.foldr = et(-1), n.find = n.detect = function(t, r, u) {
        var f;
        return f = i(t) ? n.findIndex(t, r, u) : n.findKey(t, r, u), f !== void 0 && f !== -1 ? t[f] : void 0
    }, n.filter = n.select = function(i, r, u) {
        var f = [];
        return r = t(r, u), n.each(i, function(n, t, i) {
            r(n, t, i) && f.push(n)
        }), f
    }, n.reject = function(i, r, u) {
        return n.filter(i, n.negate(t(r)), u)
    }, n.every = n.all = function(r, u, f) {
        var s;
        u = t(u, f);
        for (var o = !i(r) && n.keys(r), h = (o || r).length, e = 0; h > e; e++)
            if (s = o ? o[e] : e, !u(r[s], s, r)) return !1;
        return !0
    }, n.some = n.any = function(r, u, f) {
        var s;
        u = t(u, f);
        for (var o = !i(r) && n.keys(r), h = (o || r).length, e = 0; h > e; e++)
            if (s = o ? o[e] : e, u(r[s], s, r)) return !0;
        return !1
    }, n.contains = n.includes = n.include = function(t, r, u, f) {
        return i(t) || (t = n.values(t)), ("number" != typeof u || f) && (u = 0), n.indexOf(t, r, u) >= 0
    }, n.invoke = function(t, i) {
        var u = r.call(arguments, 2),
            f = n.isFunction(i);
        return n.map(t, function(n) {
            var t = f ? i : n[i];
            return null == t ? t : t.apply(n, u)
        })
    }, n.pluck = function(t, i) {
        return n.map(t, n.property(i))
    }, n.where = function(t, i) {
        return n.filter(t, n.matcher(i))
    }, n.findWhere = function(t, i) {
        return n.find(t, n.matcher(i))
    }, n.max = function(r, u, f) {
        var h, o, e = -1 / 0,
            c = -1 / 0,
            s, l;
        if (null == u && null != r)
            for (r = i(r) ? r : n.values(r), s = 0, l = r.length; l > s; s++) h = r[s], h > e && (e = h);
        else u = t(u, f), n.each(r, function(n, t, i) {
            o = u(n, t, i), (o > c || o === -1 / 0 && e === -1 / 0) && (e = n, c = o)
        });
        return e
    }, n.min = function(r, u, f) {
        var h, o, e = 1 / 0,
            c = 1 / 0,
            s, l;
        if (null == u && null != r)
            for (r = i(r) ? r : n.values(r), s = 0, l = r.length; l > s; s++) h = r[s], e > h && (e = h);
        else u = t(u, f), n.each(r, function(n, t, i) {
            o = u(n, t, i), (c > o || 1 / 0 === o && 1 / 0 === e) && (e = n, c = o)
        });
        return e
    }, n.shuffle = function(t) {
        for (var u, e = i(t) ? t : n.values(t), o = e.length, f = Array(o), r = 0; o > r; r++) u = n.random(0, r), u !== r && (f[r] = f[u]), f[u] = e[r];
        return f
    }, n.sample = function(t, r, u) {
        return null == r || u ? (i(t) || (t = n.values(t)), t[n.random(t.length - 1)]) : n.shuffle(t).slice(0, Math.max(0, r))
    }, n.sortBy = function(i, r, u) {
        return r = t(r, u), n.pluck(n.map(i, function(n, t, i) {
            return {
                value: n,
                index: t,
                criteria: r(n, t, i)
            }
        }).sort(function(n, t) {
            var i = n.criteria,
                r = t.criteria;
            if (i !== r) {
                if (i > r || i === void 0) return 1;
                if (r > i || r === void 0) return -1
            }
            return n.index - t.index
        }), "value")
    }, h = function(i) {
        return function(r, u, f) {
            var e = {};
            return u = t(u, f), n.each(r, function(n, t) {
                var f = u(n, t, r);
                i(e, n, f)
            }), e
        }
    }, n.groupBy = h(function(t, i, r) {
        n.has(t, r) ? t[r].push(i) : t[r] = [i]
    }), n.indexBy = h(function(n, t, i) {
        n[i] = t
    }), n.countBy = h(function(t, i, r) {
        n.has(t, r) ? t[r]++ : t[r] = 1
    }), n.toArray = function(t) {
        return t ? n.isArray(t) ? r.call(t) : i(t) ? n.map(t, n.identity) : n.values(t) : []
    }, n.size = function(t) {
        return null == t ? 0 : i(t) ? t.length : n.keys(t).length
    }, n.partition = function(i, r, u) {
        r = t(r, u);
        var f = [],
            e = [];
        return n.each(i, function(n, t, i) {
            (r(n, t, i) ? f : e).push(n)
        }), [f, e]
    }, n.first = n.head = n.take = function(t, i, r) {
        if (null != t) return null == i || r ? t[0] : n.initial(t, t.length - i)
    }, n.initial = function(n, t, i) {
        return r.call(n, 0, Math.max(0, n.length - (null == t || i ? 1 : t)))
    }, n.last = function(t, i, r) {
        if (null != t) return null == i || r ? t[t.length - 1] : n.rest(t, Math.max(0, t.length - i))
    }, n.rest = n.tail = n.drop = function(n, t, i) {
        return r.call(n, null == t || i ? 1 : t)
    }, n.compact = function(t) {
        return n.filter(t, n.identity)
    }, f = function(t, r, e, o) {
        for (var s, l, a, h = [], v = 0, c = o || 0, y = u(t); y > c; c++)
            if (s = t[c], i(s) && (n.isArray(s) || n.isArguments(s)))
                for (r || (s = f(s, r, e)), l = 0, a = s.length, h.length += a; a > l;) h[v++] = s[l++];
            else e || (h[v++] = s);
        return h
    }, n.flatten = function(n, t) {
        return f(n, t, !1)
    }, n.without = function(t) {
        return n.difference(t, r.call(arguments, 1))
    }, n.uniq = n.unique = function(i, r, f, e) {
        var o, c;
        n.isBoolean(r) || (e = f, f = r, r = !1), null != f && (f = t(f, e));
        for (var s = [], l = [], h = 0, a = u(i); a > h; h++) o = i[h], c = f ? f(o, h, i) : o, r ? (h && l === c || s.push(o), l = c) : f ? n.contains(l, c) || (l.push(c), s.push(o)) : n.contains(s, o) || s.push(o);
        return s
    }, n.union = function() {
        return n.uniq(f(arguments, !0, !0))
    }, n.intersection = function(t) {
        for (var r, i, f = [], o = arguments.length, e = 0, s = u(t); s > e; e++)
            if (r = t[e], !n.contains(f, r)) {
                for (i = 1; o > i && n.contains(arguments[i], r); i++);
                i === o && f.push(r)
            } return f
    }, n.difference = function(t) {
        var i = f(arguments, !0, !0, 1);
        return n.filter(t, function(t) {
            return !n.contains(i, t)
        })
    }, n.zip = function() {
        return n.unzip(arguments)
    }, n.unzip = function(t) {
        for (var r = t && n.max(t, u).length || 0, f = Array(r), i = 0; r > i; i++) f[i] = n.pluck(t, i);
        return f
    }, n.object = function(n, t) {
        for (var r = {}, i = 0, f = u(n); f > i; i++) t ? r[n[i]] = t[i] : r[n[i][0]] = n[i][1];
        return r
    }, n.findIndex = rt(1), n.findLastIndex = rt(-1), n.sortedIndex = function(n, i, r, f) {
        var o;
        r = t(r, f, 1);
        for (var h = r(i), e = 0, s = u(n); s > e;) o = Math.floor((e + s) / 2), r(n[o]) < h ? e = o + 1 : s = o;
        return e
    }, n.indexOf = ut(1, n.findIndex, n.sortedIndex), n.lastIndexOf = ut(-1, n.findLastIndex), n.range = function(n, t, i) {
        null == t && (t = n || 0, n = 0), i = i || 1;
        for (var u = Math.max(Math.ceil((t - n) / i), 0), f = Array(u), r = 0; u > r; r++, n += i) f[r] = n;
        return f
    }, g = function(t, i, r, u, f) {
        if (!(u instanceof i)) return t.apply(r, f);
        var e = st(t.prototype),
            o = t.apply(e, f);
        return n.isObject(o) ? o : e
    }, n.bind = function(t, i) {
        if (y && t.bind === y) return y.apply(t, r.call(arguments, 1));
        if (!n.isFunction(t)) throw new TypeError("Bind must be called on a function");
        var f = r.call(arguments, 2),
            u = function() {
                return g(t, u, i, this, f.concat(r.call(arguments)))
            };
        return u
    }, n.partial = function(t) {
        var i = r.call(arguments, 1),
            u = function() {
                for (var f = 0, o = i.length, e = Array(o), r = 0; o > r; r++) e[r] = i[r] === n ? arguments[f++] : i[r];
                for (; f < arguments.length;) e.push(arguments[f++]);
                return g(t, u, this, this, e)
            };
        return u
    }, n.bindAll = function(t) {
        var i, r, u = arguments.length;
        if (1 >= u) throw new Error("bindAll must be passed function names");
        for (i = 1; u > i; i++) r = arguments[i], t[r] = n.bind(t[r], t);
        return t
    }, n.memoize = function(t, i) {
        var r = function(u) {
            var f = r.cache,
                e = "" + (i ? i.apply(this, arguments) : u);
            return n.has(f, e) || (f[e] = t.apply(this, arguments)), f[e]
        };
        return r.cache = {}, r
    }, n.delay = function(n, t) {
        var i = r.call(arguments, 2);
        return setTimeout(function() {
            return n.apply(null, i)
        }, t)
    }, n.defer = n.partial(n.delay, n, 1), n.throttle = function(t, i, r) {
        var f, e, s, u = null,
            o = 0,
            h;
        return r || (r = {}), h = function() {
                o = r.leading === !1 ? 0 : n.now(), u = null, s = t.apply(f, e), u || (f = e = null)
            },
            function() {
                var l = n.now(),
                    c;
                return o || r.leading !== !1 || (o = l), c = i - (l - o), f = this, e = arguments, 0 >= c || c > i ? (u && (clearTimeout(u), u = null), o = l, s = t.apply(f, e), u || (f = e = null)) : u || r.trailing === !1 || (u = setTimeout(h, c)), s
            }
    }, n.debounce = function(t, i, r) {
        var u, f, e, s, o, h = function() {
            var c = n.now() - s;
            i > c && c >= 0 ? u = setTimeout(h, i - c) : (u = null, r || (o = t.apply(e, f), u || (e = f = null)))
        };
        return function() {
            e = this, f = arguments, s = n.now();
            var c = r && !u;
            return u || (u = setTimeout(h, i)), c && (o = t.apply(e, f), e = f = null), o
        }
    }, n.wrap = function(t, i) {
        return n.partial(i, t)
    }, n.negate = function(n) {
        return function() {
            return !n.apply(this, arguments)
        }
    }, n.compose = function() {
        var n = arguments,
            t = n.length - 1;
        return function() {
            for (var r = t, i = n[t].apply(this, arguments); r--;) i = n[r].call(this, i);
            return i
        }
    }, n.after = function(n, t) {
        return function() {
            if (--n < 1) return t.apply(this, arguments)
        }
    }, n.before = function(n, t) {
        var i;
        return function() {
            return --n > 0 && (i = t.apply(this, arguments)), 1 >= n && (t = null), i
        }
    }, n.once = n.partial(n.before, 2), d = !{
        toString: null
    }.propertyIsEnumerable("toString"), k = ["valueOf", "isPrototypeOf", "toString", "propertyIsEnumerable", "hasOwnProperty", "toLocaleString"], n.keys = function(t) {
        var i, r;
        if (!n.isObject(t)) return [];
        if (ct) return ct(t);
        i = [];
        for (r in t) n.has(t, r) && i.push(r);
        return d && ft(t, i), i
    }, n.allKeys = function(t) {
        var i, r;
        if (!n.isObject(t)) return [];
        i = [];
        for (r in t) i.push(r);
        return d && ft(t, i), i
    }, n.values = function(t) {
        for (var r = n.keys(t), u = r.length, f = Array(u), i = 0; u > i; i++) f[i] = t[r[i]];
        return f
    }, n.mapObject = function(i, r, u) {
        r = t(r, u);
        for (var f, o = n.keys(i), h = o.length, s = {}, e = 0; h > e; e++) f = o[e], s[f] = r(i[f], f, i);
        return s
    }, n.pairs = function(t) {
        for (var r = n.keys(t), u = r.length, f = Array(u), i = 0; u > i; i++) f[i] = [r[i], t[r[i]]];
        return f
    }, n.invert = function(t) {
        for (var u = {}, r = n.keys(t), i = 0, f = r.length; f > i; i++) u[t[r[i]]] = r[i];
        return u
    }, n.functions = n.methods = function(t) {
        var r = [],
            i;
        for (i in t) n.isFunction(t[i]) && r.push(i);
        return r.sort()
    }, n.extend = b(n.allKeys), n.extendOwn = n.assign = b(n.keys), n.findKey = function(i, r, u) {
        r = t(r, u);
        for (var f, o = n.keys(i), e = 0, s = o.length; s > e; e++)
            if (f = o[e], r(i[f], f, i)) return f
    }, n.pick = function(t, i, r) {
        var c, o, l = {},
            u = t,
            s, v, h, a;
        if (null == u) return l;
        for (n.isFunction(i) ? (o = n.allKeys(u), c = e(i, r)) : (o = f(arguments, !1, !1, 1), c = function(n, t, i) {
                return t in i
            }, u = Object(u)), s = 0, v = o.length; v > s; s++) h = o[s], a = u[h], c(a, h, u) && (l[h] = a);
        return l
    }, n.omit = function(t, i, r) {
        if (n.isFunction(i)) i = n.negate(i);
        else {
            var u = n.map(f(arguments, !1, !1, 1), String);
            i = function(t, i) {
                return !n.contains(u, i)
            }
        }
        return n.pick(t, i, r)
    }, n.defaults = b(n.allKeys, !0), n.create = function(t, i) {
        var r = st(t);
        return i && n.extendOwn(r, i), r
    }, n.clone = function(t) {
        return n.isObject(t) ? n.isArray(t) ? t.slice() : n.extend({}, t) : t
    }, n.tap = function(n, t) {
        return t(n), n
    }, n.isMatch = function(t, i) {
        var e = n.keys(i),
            o = e.length,
            f, r, u;
        if (null == t) return !o;
        for (f = Object(t), r = 0; o > r; r++)
            if (u = e[r], i[u] !== f[u] || !(u in f)) return !1;
        return !0
    }, s = function(t, i, r, u) {
        var c, a, e, h, f, l, v;
        if (t === i) return 0 !== t || 1 / t == 1 / i;
        if (null == t || null == i) return t === i;
        if (t instanceof n && (t = t._wrapped), i instanceof n && (i = i._wrapped), c = o.call(t), c !== o.call(i)) return !1;
        switch (c) {
            case "[object RegExp]":
            case "[object String]":
                return "" + t == "" + i;
            case "[object Number]":
                return +t != +t ? +i != +i : 0 == +t ? 1 / +t == 1 / i : +t == +i;
            case "[object Date]":
            case "[object Boolean]":
                return +t == +i
        }
        if (a = "[object Array]" === c, !a && ("object" != typeof t || "object" != typeof i || (e = t.constructor, h = i.constructor, e !== h && !(n.isFunction(e) && e instanceof e && n.isFunction(h) && h instanceof h) && "constructor" in t && "constructor" in i))) return !1;
        for (r = r || [], u = u || [], f = r.length; f--;)
            if (r[f] === t) return u[f] === i;
        if (r.push(t), u.push(i), a) {
            if (f = t.length, f !== i.length) return !1;
            for (; f--;)
                if (!s(t[f], i[f], r, u)) return !1
        } else {
            if (v = n.keys(t), f = v.length, n.keys(i).length !== f) return !1;
            for (; f--;)
                if (l = v[f], !n.has(i, l) || !s(t[l], i[l], r, u)) return !1
        }
        return r.pop(), u.pop(), !0
    }, n.isEqual = function(n, t) {
        return s(n, t)
    }, n.isEmpty = function(t) {
        return null == t ? !0 : i(t) && (n.isArray(t) || n.isString(t) || n.isArguments(t)) ? 0 === t.length : 0 === n.keys(t).length
    }, n.isElement = function(n) {
        return !(!n || 1 !== n.nodeType)
    }, n.isArray = pt || function(n) {
        return "[object Array]" === o.call(n)
    }, n.isObject = function(n) {
        var t = typeof n;
        return "function" === t || "object" === t && !!n
    }, n.each(["Arguments", "Function", "String", "Number", "Date", "RegExp", "Error"], function(t) {
        n["is" + t] = function(n) {
            return o.call(n) === "[object " + t + "]"
        }
    }), n.isArguments(arguments) || (n.isArguments = function(t) {
        return n.has(t, "callee")
    }), "function" != typeof /./ && "object" != typeof Int8Array && (n.isFunction = function(n) {
        return "function" == typeof n || !1
    }), n.isFinite = function(n) {
        return isFinite(n) && !isNaN(parseFloat(n))
    }, n.isNaN = function(t) {
        return n.isNumber(t) && t !== +t
    }, n.isBoolean = function(n) {
        return n === !0 || n === !1 || "[object Boolean]" === o.call(n)
    }, n.isNull = function(n) {
        return null === n
    }, n.isUndefined = function(n) {
        return n === void 0
    }, n.has = function(n, t) {
        return null != n && wt.call(n, t)
    }, n.noConflict = function() {
        return a._ = lt, this
    }, n.identity = function(n) {
        return n
    }, n.constant = function(n) {
        return function() {
            return n
        }
    }, n.noop = function() {}, n.property = ot, n.propertyOf = function(n) {
        return null == n ? function() {} : function(t) {
            return n[t]
        }
    }, n.matcher = n.matches = function(t) {
        return t = n.extendOwn({}, t),
            function(i) {
                return n.isMatch(i, t)
            }
    }, n.times = function(n, t, i) {
        var u = Array(Math.max(0, n)),
            r;
        for (t = e(t, i, 1), r = 0; n > r; r++) u[r] = t(r);
        return u
    }, n.random = function(n, t) {
        return null == t && (t = n, n = 0), n + Math.floor(Math.random() * (t - n + 1))
    }, n.now = Date.now || function() {
        return +new Date
    };
    var it = {
            "&": "&amp;",
            "<": "&lt;",
            ">": "&gt;",
            '"': "&quot;",
            "'": "&#x27;",
            "`": "&#x60;"
        },
        yt = n.invert(it),
        tt = function(t) {
            var r = function(n) {
                    return t[n]
                },
                i = "(?:" + n.keys(t).join("|") + ")",
                u = RegExp(i),
                f = RegExp(i, "g");
            return function(n) {
                return n = null == n ? "" : "" + n, u.test(n) ? n.replace(f, r) : n
            }
        };
    n.escape = tt(it), n.unescape = tt(yt), n.result = function(t, i, r) {
        var u = null == t ? void 0 : t[i];
        return u === void 0 && (u = r), n.isFunction(u) ? u.call(t) : u
    }, nt = 0, n.uniqueId = function(n) {
        var t = ++nt + "";
        return n ? n + t : t
    }, n.templateSettings = {
        evaluate: /<%([\s\S]+?)%>/g,
        interpolate: /<%=([\s\S]+?)%>/g,
        escape: /<%-([\s\S]+?)%>/g
    };
    var v = /(.)^/,
        bt = {
            "'": "'",
            "\\": "\\",
            "\r": "r",
            "\n": "n",
            "\u2028": "u2028",
            "\u2029": "u2029"
        },
        kt = /\\|'|\r|\n|\u2028|\u2029/g,
        vt = function(n) {
            return "\\" + bt[n]
        };
    n.template = function(t, i, r) {
        var o, f, s;
        !i && r && (i = r), i = n.defaults({}, i, n.templateSettings);
        var h = RegExp([(i.escape || v).source, (i.interpolate || v).source, (i.evaluate || v).source].join("|") + "|$", "g"),
            e = 0,
            u = "__p+='";
        t.replace(h, function(n, i, r, f, o) {
            return u += t.slice(e, o).replace(kt, vt), e = o + n.length, i ? u += "'+\n((__t=(" + i + "))==null?'':_.escape(__t))+\n'" : r ? u += "'+\n((__t=(" + r + "))==null?'':__t)+\n'" : f && (u += "';\n" + f + "\n__p+='"), n
        }), u += "';\n", i.variable || (u = "with(obj||{}){\n" + u + "}\n"), u = "var __t,__p='',__j=Array.prototype.join,print=function(){__p+=__j.call(arguments,'');};\n" + u + "return __p;\n";
        try {
            o = new Function(i.variable || "obj", "_", u)
        } catch (c) {
            throw c.source = u, c;
        }
        return f = function(t) {
            return o.call(this, t, n)
        }, s = i.variable || "obj", f.source = "function(" + s + "){\n" + u + "}", f
    }, n.chain = function(t) {
        var i = n(t);
        return i._chain = !0, i
    }, c = function(t, i) {
        return t._chain ? n(i).chain() : i
    }, n.mixin = function(t) {
        n.each(n.functions(t), function(i) {
            var r = n[i] = t[i];
            n.prototype[i] = function() {
                var t = [this._wrapped];
                return dt.apply(t, arguments), c(this, r.apply(n, t))
            }
        })
    }, n.mixin(n), n.each(["pop", "push", "reverse", "shift", "sort", "splice", "unshift"], function(t) {
        var i = l[t];
        n.prototype[t] = function() {
            var n = this._wrapped;
            return i.apply(n, arguments), "shift" !== t && "splice" !== t || 0 !== n.length || delete n[0], c(this, n)
        }
    }), n.each(["concat", "join", "slice"], function(t) {
        var i = l[t];
        n.prototype[t] = function() {
            return c(this, i.apply(this._wrapped, arguments))
        }
    }), n.prototype.value = function() {
        return this._wrapped
    }, n.prototype.valueOf = n.prototype.toJSON = n.prototype.value, n.prototype.toString = function() {
        return "" + this._wrapped
    }, "function" == typeof define && define.amd && define("underscore", [], function() {
        return n
    })
}).call(this);

;// JS/angular/angular.min.js
// angular/angular.min.js
/*
 AngularJS v1.6.3
 (c) 2010-2017 Google, Inc. http://angularjs.org
 License: MIT
*/
(function(w) {
    'use strict';

    function M(a, b) {
        b = b || Error;
        return function() {
            var d = arguments[0],
                c;
            c = "[" + (a ? a + ":" : "") + d + "] http://errors.angularjs.org/1.6.3/" + (a ? a + "/" : "") + d;
            for (d = 1; d < arguments.length; d++) {
                c = c + (1 == d ? "?" : "&") + "p" + (d - 1) + "=";
                var e = encodeURIComponent,
                    f;
                f = arguments[d];
                f = "function" == typeof f ? f.toString().replace(/ \{[\s\S]*$/, "") : "undefined" == typeof f ? "undefined" : "string" != typeof f ? JSON.stringify(f) : f;
                c += e(f)
            }
            return new b(c)
        }
    }

    function me(a) {
        if (G(a)) u(a.objectMaxDepth) && (Fc.objectMaxDepth = Tb(a.objectMaxDepth) ?
            a.objectMaxDepth : NaN);
        else return Fc
    }

    function Tb(a) {
        return ba(a) && 0 < a
    }

    function ra(a) {
        if (null == a || Wa(a)) return !1;
        if (H(a) || D(a) || F && a instanceof F) return !0;
        var b = "length" in Object(a) && a.length;
        return ba(b) && (0 <= b && (b - 1 in a || a instanceof Array) || "function" === typeof a.item)
    }

    function p(a, b, d) {
        var c, e;
        if (a)
            if (E(a))
                for (c in a) "prototype" !== c && "length" !== c && "name" !== c && a.hasOwnProperty(c) && b.call(d, a[c], c, a);
            else if (H(a) || ra(a)) {
            var f = "object" !== typeof a;
            c = 0;
            for (e = a.length; c < e; c++)(f || c in a) && b.call(d,
                a[c], c, a)
        } else if (a.forEach && a.forEach !== p) a.forEach(b, d, a);
        else if (Gc(a))
            for (c in a) b.call(d, a[c], c, a);
        else if ("function" === typeof a.hasOwnProperty)
            for (c in a) a.hasOwnProperty(c) && b.call(d, a[c], c, a);
        else
            for (c in a) ua.call(a, c) && b.call(d, a[c], c, a);
        return a
    }

    function Hc(a, b, d) {
        for (var c = Object.keys(a).sort(), e = 0; e < c.length; e++) b.call(d, a[c[e]], c[e]);
        return c
    }

    function Ic(a) {
        return function(b, d) {
            a(d, b)
        }
    }

    function ne() {
        return ++rb
    }

    function Ub(a, b, d) {
        for (var c = a.$$hashKey, e = 0, f = b.length; e < f; ++e) {
            var g = b[e];
            if (G(g) || E(g))
                for (var h = Object.keys(g), k = 0, l = h.length; k < l; k++) {
                    var m = h[k],
                        n = g[m];
                    d && G(n) ? ga(n) ? a[m] = new Date(n.valueOf()) : Xa(n) ? a[m] = new RegExp(n) : n.nodeName ? a[m] = n.cloneNode(!0) : Vb(n) ? a[m] = n.clone() : (G(a[m]) || (a[m] = H(n) ? [] : {}), Ub(a[m], [n], !0)) : a[m] = n
                }
        }
        c ? a.$$hashKey = c : delete a.$$hashKey;
        return a
    }

    function R(a) {
        return Ub(a, va.call(arguments, 1), !1)
    }

    function oe(a) {
        return Ub(a, va.call(arguments, 1), !0)
    }

    function Z(a) {
        return parseInt(a, 10)
    }

    function Wb(a, b) {
        return R(Object.create(a), b)
    }

    function A() {}

    function Ya(a) {
        return a
    }

    function la(a) {
        return function() {
            return a
        }
    }

    function Xb(a) {
        return E(a.toString) && a.toString !== ma
    }

    function x(a) {
        return "undefined" === typeof a
    }

    function u(a) {
        return "undefined" !== typeof a
    }

    function G(a) {
        return null !== a && "object" === typeof a
    }

    function Gc(a) {
        return null !== a && "object" === typeof a && !Jc(a)
    }

    function D(a) {
        return "string" === typeof a
    }

    function ba(a) {
        return "number" === typeof a
    }

    function ga(a) {
        return "[object Date]" === ma.call(a)
    }

    function E(a) {
        return "function" === typeof a
    }

    function Xa(a) {
        return "[object RegExp]" ===
            ma.call(a)
    }

    function Wa(a) {
        return a && a.window === a
    }

    function Za(a) {
        return a && a.$evalAsync && a.$watch
    }

    function Ha(a) {
        return "boolean" === typeof a
    }

    function pe(a) {
        return a && ba(a.length) && qe.test(ma.call(a))
    }

    function Vb(a) {
        return !(!a || !(a.nodeName || a.prop && a.attr && a.find))
    }

    function re(a) {
        var b = {};
        a = a.split(",");
        var d;
        for (d = 0; d < a.length; d++) b[a[d]] = !0;
        return b
    }

    function wa(a) {
        return P(a.nodeName || a[0] && a[0].nodeName)
    }

    function $a(a, b) {
        var d = a.indexOf(b);
        0 <= d && a.splice(d, 1);
        return d
    }

    function sa(a, b, d) {
        function c(a,
            b, c) {
            c--;
            if (0 > c) return "...";
            var d = b.$$hashKey,
                f;
            if (H(a)) {
                f = 0;
                for (var g = a.length; f < g; f++) b.push(e(a[f], c))
            } else if (Gc(a))
                for (f in a) b[f] = e(a[f], c);
            else if (a && "function" === typeof a.hasOwnProperty)
                for (f in a) a.hasOwnProperty(f) && (b[f] = e(a[f], c));
            else
                for (f in a) ua.call(a, f) && (b[f] = e(a[f], c));
            d ? b.$$hashKey = d : delete b.$$hashKey;
            return b
        }

        function e(a, b) {
            if (!G(a)) return a;
            var d = g.indexOf(a);
            if (-1 !== d) return h[d];
            if (Wa(a) || Za(a)) throw Fa("cpws");
            var d = !1,
                e = f(a);
            void 0 === e && (e = H(a) ? [] : Object.create(Jc(a)),
                d = !0);
            g.push(a);
            h.push(e);
            return d ? c(a, e, b) : e
        }

        function f(a) {
            switch (ma.call(a)) {
                case "[object Int8Array]":
                case "[object Int16Array]":
                case "[object Int32Array]":
                case "[object Float32Array]":
                case "[object Float64Array]":
                case "[object Uint8Array]":
                case "[object Uint8ClampedArray]":
                case "[object Uint16Array]":
                case "[object Uint32Array]":
                    return new a.constructor(e(a.buffer), a.byteOffset, a.length);
                case "[object ArrayBuffer]":
                    if (!a.slice) {
                        var b = new ArrayBuffer(a.byteLength);
                        (new Uint8Array(b)).set(new Uint8Array(a));
                        return b
                    }
                    return a.slice(0);
                case "[object Boolean]":
                case "[object Number]":
                case "[object String]":
                case "[object Date]":
                    return new a.constructor(a.valueOf());
                case "[object RegExp]":
                    return b = new RegExp(a.source, a.toString().match(/[^/]*$/)[0]), b.lastIndex = a.lastIndex, b;
                case "[object Blob]":
                    return new a.constructor([a], {
                        type: a.type
                    })
            }
            if (E(a.cloneNode)) return a.cloneNode(!0)
        }
        var g = [],
            h = [];
        d = Tb(d) ? d : NaN;
        if (b) {
            if (pe(b) || "[object ArrayBuffer]" === ma.call(b)) throw Fa("cpta");
            if (a === b) throw Fa("cpi");
            H(b) ? b.length =
                0 : p(b, function(a, c) {
                    "$$hashKey" !== c && delete b[c]
                });
            g.push(a);
            h.push(b);
            return c(a, b, d)
        }
        return e(a, d)
    }

    function pa(a, b) {
        if (a === b) return !0;
        if (null === a || null === b) return !1;
        if (a !== a && b !== b) return !0;
        var d = typeof a,
            c;
        if (d === typeof b && "object" === d)
            if (H(a)) {
                if (!H(b)) return !1;
                if ((d = a.length) === b.length) {
                    for (c = 0; c < d; c++)
                        if (!pa(a[c], b[c])) return !1;
                    return !0
                }
            } else {
                if (ga(a)) return ga(b) ? pa(a.getTime(), b.getTime()) : !1;
                if (Xa(a)) return Xa(b) ? a.toString() === b.toString() : !1;
                if (Za(a) || Za(b) || Wa(a) || Wa(b) || H(b) || ga(b) ||
                    Xa(b)) return !1;
                d = V();
                for (c in a)
                    if ("$" !== c.charAt(0) && !E(a[c])) {
                        if (!pa(a[c], b[c])) return !1;
                        d[c] = !0
                    } for (c in b)
                    if (!(c in d) && "$" !== c.charAt(0) && u(b[c]) && !E(b[c])) return !1;
                return !0
            } return !1
    }

    function ab(a, b, d) {
        return a.concat(va.call(b, d))
    }

    function bb(a, b) {
        var d = 2 < arguments.length ? va.call(arguments, 2) : [];
        return !E(b) || b instanceof RegExp ? b : d.length ? function() {
            return arguments.length ? b.apply(a, ab(d, arguments, 0)) : b.apply(a, d)
        } : function() {
            return arguments.length ? b.apply(a, arguments) : b.call(a)
        }
    }

    function Kc(a,
        b) {
        var d = b;
        "string" === typeof a && "$" === a.charAt(0) && "$" === a.charAt(1) ? d = void 0 : Wa(b) ? d = "$WINDOW" : b && w.document === b ? d = "$DOCUMENT" : Za(b) && (d = "$SCOPE");
        return d
    }

    function cb(a, b) {
        if (!x(a)) return ba(b) || (b = b ? 2 : null), JSON.stringify(a, Kc, b)
    }

    function Lc(a) {
        return D(a) ? JSON.parse(a) : a
    }

    function Mc(a, b) {
        a = a.replace(se, "");
        var d = Date.parse("Jan 01, 1970 00:00:00 " + a) / 6E4;
        return da(d) ? b : d
    }

    function Yb(a, b, d) {
        d = d ? -1 : 1;
        var c = a.getTimezoneOffset();
        b = Mc(b, c);
        d *= b - c;
        a = new Date(a.getTime());
        a.setMinutes(a.getMinutes() +
            d);
        return a
    }

    function xa(a) {
        a = F(a).clone();
        try {
            a.empty()
        } catch (b) {}
        var d = F("<div>").append(a).html();
        try {
            return a[0].nodeType === Ia ? P(d) : d.match(/^(<[^>]+>)/)[1].replace(/^<([\w-]+)/, function(a, b) {
                return "<" + P(b)
            })
        } catch (c) {
            return P(d)
        }
    }

    function Nc(a) {
        try {
            return decodeURIComponent(a)
        } catch (b) {}
    }

    function Oc(a) {
        var b = {};
        p((a || "").split("&"), function(a) {
            var c, e, f;
            a && (e = a = a.replace(/\+/g, "%20"), c = a.indexOf("="), -1 !== c && (e = a.substring(0, c), f = a.substring(c + 1)), e = Nc(e), u(e) && (f = u(f) ? Nc(f) : !0, ua.call(b, e) ? H(b[e]) ?
                b[e].push(f) : b[e] = [b[e], f] : b[e] = f))
        });
        return b
    }

    function Zb(a) {
        var b = [];
        p(a, function(a, c) {
            H(a) ? p(a, function(a) {
                b.push($(c, !0) + (!0 === a ? "" : "=" + $(a, !0)))
            }) : b.push($(c, !0) + (!0 === a ? "" : "=" + $(a, !0)))
        });
        return b.length ? b.join("&") : ""
    }

    function db(a) {
        return $(a, !0).replace(/%26/gi, "&").replace(/%3D/gi, "=").replace(/%2B/gi, "+")
    }

    function $(a, b) {
        return encodeURIComponent(a).replace(/%40/gi, "@").replace(/%3A/gi, ":").replace(/%24/g, "$").replace(/%2C/gi, ",").replace(/%3B/gi, ";").replace(/%20/g, b ? "%20" : "+")
    }

    function te(a,
        b) {
        var d, c, e = Ja.length;
        for (c = 0; c < e; ++c)
            if (d = Ja[c] + b, D(d = a.getAttribute(d))) return d;
        return null
    }

    function ue(a, b) {
        var d, c, e = {};
        p(Ja, function(b) {
            b += "app";
            !d && a.hasAttribute && a.hasAttribute(b) && (d = a, c = a.getAttribute(b))
        });
        p(Ja, function(b) {
            b += "app";
            var e;
            !d && (e = a.querySelector("[" + b.replace(":", "\\:") + "]")) && (d = e, c = e.getAttribute(b))
        });
        d && (ve ? (e.strictDi = null !== te(d, "strict-di"), b(d, c ? [c] : [], e)) : w.console.error("Angular: disabling automatic bootstrap. <script> protocol indicates an extension, document.location.href does not match."))
    }

    function Pc(a, b, d) {
        G(d) || (d = {});
        d = R({
            strictDi: !1
        }, d);
        var c = function() {
                a = F(a);
                if (a.injector()) {
                    var c = a[0] === w.document ? "document" : xa(a);
                    throw Fa("btstrpd", c.replace(/</, "&lt;").replace(/>/, "&gt;"));
                }
                b = b || [];
                b.unshift(["$provide", function(b) {
                    b.value("$rootElement", a)
                }]);
                d.debugInfoEnabled && b.push(["$compileProvider", function(a) {
                    a.debugInfoEnabled(!0)
                }]);
                b.unshift("ng");
                c = eb(b, d.strictDi);
                c.invoke(["$rootScope", "$rootElement", "$compile", "$injector", function(a, b, c, d) {
                    a.$apply(function() {
                        b.data("$injector",
                            d);
                        c(b)(a)
                    })
                }]);
                return c
            },
            e = /^NG_ENABLE_DEBUG_INFO!/,
            f = /^NG_DEFER_BOOTSTRAP!/;
        w && e.test(w.name) && (d.debugInfoEnabled = !0, w.name = w.name.replace(e, ""));
        if (w && !f.test(w.name)) return c();
        w.name = w.name.replace(f, "");
        ea.resumeBootstrap = function(a) {
            p(a, function(a) {
                b.push(a)
            });
            return c()
        };
        E(ea.resumeDeferredBootstrap) && ea.resumeDeferredBootstrap()
    }

    function we() {
        w.name = "NG_ENABLE_DEBUG_INFO!" + w.name;
        w.location.reload()
    }

    function xe(a) {
        a = ea.element(a).injector();
        if (!a) throw Fa("test");
        return a.get("$$testability")
    }

    function Qc(a, b) {
        b = b || "_";
        return a.replace(ye, function(a, c) {
            return (c ? b : "") + a.toLowerCase()
        })
    }

    function ze() {
        var a;
        if (!Rc) {
            var b = sb();
            (na = x(b) ? w.jQuery : b ? w[b] : void 0) && na.fn.on ? (F = na, R(na.fn, {
                scope: Oa.scope,
                isolateScope: Oa.isolateScope,
                controller: Oa.controller,
                injector: Oa.injector,
                inheritedData: Oa.inheritedData
            }), a = na.cleanData, na.cleanData = function(b) {
                for (var c, e = 0, f; null != (f = b[e]); e++)(c = na._data(f, "events")) && c.$destroy && na(f).triggerHandler("$destroy");
                a(b)
            }) : F = W;
            ea.element = F;
            Rc = !0
        }
    }

    function fb(a,
        b, d) {
        if (!a) throw Fa("areq", b || "?", d || "required");
        return a
    }

    function tb(a, b, d) {
        d && H(a) && (a = a[a.length - 1]);
        fb(E(a), b, "not a function, got " + (a && "object" === typeof a ? a.constructor.name || "Object" : typeof a));
        return a
    }

    function Ka(a, b) {
        if ("hasOwnProperty" === a) throw Fa("badname", b);
    }

    function Sc(a, b, d) {
        if (!b) return a;
        b = b.split(".");
        for (var c, e = a, f = b.length, g = 0; g < f; g++) c = b[g], a && (a = (e = a)[c]);
        return !d && E(a) ? bb(e, a) : a
    }

    function ub(a) {
        for (var b = a[0], d = a[a.length - 1], c, e = 1; b !== d && (b = b.nextSibling); e++)
            if (c || a[e] !==
                b) c || (c = F(va.call(a, 0, e))), c.push(b);
        return c || a
    }

    function V() {
        return Object.create(null)
    }

    function $b(a) {
        if (null == a) return "";
        switch (typeof a) {
            case "string":
                break;
            case "number":
                a = "" + a;
                break;
            default:
                a = !Xb(a) || H(a) || ga(a) ? cb(a) : a.toString()
        }
        return a
    }

    function Ae(a) {
        function b(a, b, c) {
            return a[b] || (a[b] = c())
        }
        var d = M("$injector"),
            c = M("ng");
        a = b(a, "angular", Object);
        a.$$minErr = a.$$minErr || M;
        return b(a, "module", function() {
            var a = {};
            return function(f, g, h) {
                var k = {};
                if ("hasOwnProperty" === f) throw c("badname", "module");
                g && a.hasOwnProperty(f) && (a[f] = null);
                return b(a, f, function() {
                    function a(b, c, d, f) {
                        f || (f = e);
                        return function() {
                            f[d || "push"]([b, c, arguments]);
                            return p
                        }
                    }

                    function b(a, c, d) {
                        d || (d = e);
                        return function(b, e) {
                            e && E(e) && (e.$$moduleName = f);
                            d.push([a, c, arguments]);
                            return p
                        }
                    }
                    if (!g) throw d("nomod", f);
                    var e = [],
                        q = [],
                        r = [],
                        I = a("$injector", "invoke", "push", q),
                        p = {
                            _invokeQueue: e,
                            _configBlocks: q,
                            _runBlocks: r,
                            info: function(a) {
                                if (u(a)) {
                                    if (!G(a)) throw c("aobj", "value");
                                    k = a;
                                    return this
                                }
                                return k
                            },
                            requires: g,
                            name: f,
                            provider: b("$provide",
                                "provider"),
                            factory: b("$provide", "factory"),
                            service: b("$provide", "service"),
                            value: a("$provide", "value"),
                            constant: a("$provide", "constant", "unshift"),
                            decorator: b("$provide", "decorator", q),
                            animation: b("$animateProvider", "register"),
                            filter: b("$filterProvider", "register"),
                            controller: b("$controllerProvider", "register"),
                            directive: b("$compileProvider", "directive"),
                            component: b("$compileProvider", "component"),
                            config: I,
                            run: function(a) {
                                r.push(a);
                                return this
                            }
                        };
                    h && I(h);
                    return p
                })
            }
        })
    }

    function qa(a, b) {
        if (H(a)) {
            b =
                b || [];
            for (var d = 0, c = a.length; d < c; d++) b[d] = a[d]
        } else if (G(a))
            for (d in b = b || {}, a)
                if ("$" !== d.charAt(0) || "$" !== d.charAt(1)) b[d] = a[d];
        return b || a
    }

    function Be(a, b) {
        var d = [];
        Tb(b) && (a = sa(a, null, b));
        return JSON.stringify(a, function(a, b) {
            b = Kc(a, b);
            if (G(b)) {
                if (0 <= d.indexOf(b)) return "...";
                d.push(b)
            }
            return b
        })
    }

    function Ce(a) {
        R(a, {
            errorHandlingConfig: me,
            bootstrap: Pc,
            copy: sa,
            extend: R,
            merge: oe,
            equals: pa,
            element: F,
            forEach: p,
            injector: eb,
            noop: A,
            bind: bb,
            toJson: cb,
            fromJson: Lc,
            identity: Ya,
            isUndefined: x,
            isDefined: u,
            isString: D,
            isFunction: E,
            isObject: G,
            isNumber: ba,
            isElement: Vb,
            isArray: H,
            version: De,
            isDate: ga,
            lowercase: P,
            uppercase: vb,
            callbacks: {
                $$counter: 0
            },
            getTestability: xe,
            reloadWithDebugInfo: we,
            $$minErr: M,
            $$csp: Ga,
            $$encodeUriSegment: db,
            $$encodeUriQuery: $,
            $$stringify: $b
        });
        ac = Ae(w);
        ac("ng", ["ngLocale"], ["$provide", function(a) {
            a.provider({
                $$sanitizeUri: Ee
            });
            a.provider("$compile", Tc).directive({
                a: Fe,
                input: Uc,
                textarea: Uc,
                form: Ge,
                script: He,
                select: Ie,
                option: Je,
                ngBind: Ke,
                ngBindHtml: Le,
                ngBindTemplate: Me,
                ngClass: Ne,
                ngClassEven: Oe,
                ngClassOdd: Pe,
                ngCloak: Qe,
                ngController: Re,
                ngForm: Se,
                ngHide: Te,
                ngIf: Ue,
                ngInclude: Ve,
                ngInit: We,
                ngNonBindable: Xe,
                ngPluralize: Ye,
                ngRepeat: Ze,
                ngShow: $e,
                ngStyle: af,
                ngSwitch: bf,
                ngSwitchWhen: cf,
                ngSwitchDefault: df,
                ngOptions: ef,
                ngTransclude: ff,
                ngModel: gf,
                ngList: hf,
                ngChange: jf,
                pattern: Vc,
                ngPattern: Vc,
                required: Wc,
                ngRequired: Wc,
                minlength: Xc,
                ngMinlength: Xc,
                maxlength: Yc,
                ngMaxlength: Yc,
                ngValue: kf,
                ngModelOptions: lf
            }).directive({
                ngInclude: mf
            }).directive(wb).directive(Zc);
            a.provider({
                $anchorScroll: nf,
                $animate: of,
                $animateCss: pf,
                $$animateJs: qf,
                $$animateQueue: rf,
                $$AnimateRunner: sf,
                $$animateAsyncRun: tf,
                $browser: uf,
                $cacheFactory: vf,
                $controller: wf,
                $document: xf,
                $$isDocumentHidden: yf,
                $exceptionHandler: zf,
                $filter: $c,
                $$forceReflow: Af,
                $interpolate: Bf,
                $interval: Cf,
                $http: Df,
                $httpParamSerializer: Ef,
                $httpParamSerializerJQLike: Ff,
                $httpBackend: Gf,
                $xhrFactory: Hf,
                $jsonpCallbacks: If,
                $location: Jf,
                $log: Kf,
                $parse: Lf,
                $rootScope: Mf,
                $q: Nf,
                $$q: Of,
                $sce: Pf,
                $sceDelegate: Qf,
                $sniffer: Rf,
                $templateCache: Sf,
                $templateRequest: Tf,
                $$testability: Uf,
                $timeout: Vf,
                $window: Wf,
                $$rAF: Xf,
                $$jqLite: Yf,
                $$Map: Zf,
                $$cookieReader: $f
            })
        }]).info({
            angularVersion: "1.6.3"
        })
    }

    function gb(a, b) {
        return b.toUpperCase()
    }

    function xb(a) {
        return a.replace(ag, gb)
    }

    function ad(a) {
        a = a.nodeType;
        return 1 === a || !a || 9 === a
    }

    function bd(a, b) {
        var d, c, e = b.createDocumentFragment(),
            f = [];
        if (bc.test(a)) {
            d = e.appendChild(b.createElement("div"));
            c = (bg.exec(a) || ["", ""])[1].toLowerCase();
            c = ha[c] || ha._default;
            d.innerHTML = c[1] + a.replace(cg, "<$1></$2>") + c[2];
            for (c = c[0]; c--;) d = d.lastChild;
            f = ab(f, d.childNodes);
            d = e.firstChild;
            d.textContent = ""
        } else f.push(b.createTextNode(a));
        e.textContent = "";
        e.innerHTML = "";
        p(f, function(a) {
            e.appendChild(a)
        });
        return e
    }

    function W(a) {
        if (a instanceof W) return a;
        var b;
        D(a) && (a = S(a), b = !0);
        if (!(this instanceof W)) {
            if (b && "<" !== a.charAt(0)) throw cc("nosel");
            return new W(a)
        }
        if (b) {
            b = w.document;
            var d;
            a = (d = dg.exec(a)) ? [b.createElement(d[1])] : (d = bd(a, b)) ? d.childNodes : [];
            dc(this, a)
        } else E(a) ? cd(a) : dc(this, a)
    }

    function ec(a) {
        return a.cloneNode(!0)
    }

    function yb(a, b) {
        b || hb(a);
        if (a.querySelectorAll)
            for (var d =
                    a.querySelectorAll("*"), c = 0, e = d.length; c < e; c++) hb(d[c])
    }

    function dd(a, b, d, c) {
        if (u(c)) throw cc("offargs");
        var e = (c = zb(a)) && c.events,
            f = c && c.handle;
        if (f)
            if (b) {
                var g = function(b) {
                    var c = e[b];
                    u(d) && $a(c || [], d);
                    u(d) && c && 0 < c.length || (a.removeEventListener(b, f), delete e[b])
                };
                p(b.split(" "), function(a) {
                    g(a);
                    Ab[a] && g(Ab[a])
                })
            } else
                for (b in e) "$destroy" !== b && a.removeEventListener(b, f), delete e[b]
    }

    function hb(a, b) {
        var d = a.ng339,
            c = d && ib[d];
        c && (b ? delete c.data[b] : (c.handle && (c.events.$destroy && c.handle({}, "$destroy"),
            dd(a)), delete ib[d], a.ng339 = void 0))
    }

    function zb(a, b) {
        var d = a.ng339,
            d = d && ib[d];
        b && !d && (a.ng339 = d = ++eg, d = ib[d] = {
            events: {},
            data: {},
            handle: void 0
        });
        return d
    }

    function fc(a, b, d) {
        if (ad(a)) {
            var c, e = u(d),
                f = !e && b && !G(b),
                g = !b;
            a = (a = zb(a, !f)) && a.data;
            if (e) a[xb(b)] = d;
            else {
                if (g) return a;
                if (f) return a && a[xb(b)];
                for (c in b) a[xb(c)] = b[c]
            }
        }
    }

    function Bb(a, b) {
        return a.getAttribute ? -1 < (" " + (a.getAttribute("class") || "") + " ").replace(/[\n\t]/g, " ").indexOf(" " + b + " ") : !1
    }

    function Cb(a, b) {
        b && a.setAttribute && p(b.split(" "),
            function(b) {
                a.setAttribute("class", S((" " + (a.getAttribute("class") || "") + " ").replace(/[\n\t]/g, " ").replace(" " + S(b) + " ", " ")))
            })
    }

    function Db(a, b) {
        if (b && a.setAttribute) {
            var d = (" " + (a.getAttribute("class") || "") + " ").replace(/[\n\t]/g, " ");
            p(b.split(" "), function(a) {
                a = S(a); - 1 === d.indexOf(" " + a + " ") && (d += a + " ")
            });
            a.setAttribute("class", S(d))
        }
    }

    function dc(a, b) {
        if (b)
            if (b.nodeType) a[a.length++] = b;
            else {
                var d = b.length;
                if ("number" === typeof d && b.window !== b) {
                    if (d)
                        for (var c = 0; c < d; c++) a[a.length++] = b[c]
                } else a[a.length++] =
                    b
            }
    }

    function ed(a, b) {
        return Eb(a, "$" + (b || "ngController") + "Controller")
    }

    function Eb(a, b, d) {
        9 === a.nodeType && (a = a.documentElement);
        for (b = H(b) ? b : [b]; a;) {
            for (var c = 0, e = b.length; c < e; c++)
                if (u(d = F.data(a, b[c]))) return d;
            a = a.parentNode || 11 === a.nodeType && a.host
        }
    }

    function fd(a) {
        for (yb(a, !0); a.firstChild;) a.removeChild(a.firstChild)
    }

    function Fb(a, b) {
        b || yb(a);
        var d = a.parentNode;
        d && d.removeChild(a)
    }

    function fg(a, b) {
        b = b || w;
        if ("complete" === b.document.readyState) b.setTimeout(a);
        else F(b).on("load", a)
    }

    function cd(a) {
        function b() {
            w.document.removeEventListener("DOMContentLoaded",
                b);
            w.removeEventListener("load", b);
            a()
        }
        "complete" === w.document.readyState ? w.setTimeout(a) : (w.document.addEventListener("DOMContentLoaded", b), w.addEventListener("load", b))
    }

    function gd(a, b) {
        var d = Gb[b.toLowerCase()];
        return d && hd[wa(a)] && d
    }

    function gg(a, b) {
        var d = function(c, d) {
            c.isDefaultPrevented = function() {
                return c.defaultPrevented
            };
            var f = b[d || c.type],
                g = f ? f.length : 0;
            if (g) {
                if (x(c.immediatePropagationStopped)) {
                    var h = c.stopImmediatePropagation;
                    c.stopImmediatePropagation = function() {
                        c.immediatePropagationStopped = !0;
                        c.stopPropagation && c.stopPropagation();
                        h && h.call(c)
                    }
                }
                c.isImmediatePropagationStopped = function() {
                    return !0 === c.immediatePropagationStopped
                };
                var k = f.specialHandlerWrapper || hg;
                1 < g && (f = qa(f));
                for (var l = 0; l < g; l++) c.isImmediatePropagationStopped() || k(a, c, f[l])
            }
        };
        d.elem = a;
        return d
    }

    function hg(a, b, d) {
        d.call(a, b)
    }

    function ig(a, b, d) {
        var c = b.relatedTarget;
        c && (c === a || jg.call(a, c)) || d.call(a, b)
    }

    function Yf() {
        this.$get = function() {
            return R(W, {
                hasClass: function(a, b) {
                    a.attr && (a = a[0]);
                    return Bb(a, b)
                },
                addClass: function(a,
                    b) {
                    a.attr && (a = a[0]);
                    return Db(a, b)
                },
                removeClass: function(a, b) {
                    a.attr && (a = a[0]);
                    return Cb(a, b)
                }
            })
        }
    }

    function Pa(a, b) {
        var d = a && a.$$hashKey;
        if (d) return "function" === typeof d && (d = a.$$hashKey()), d;
        d = typeof a;
        return d = "function" === d || "object" === d && null !== a ? a.$$hashKey = d + ":" + (b || ne)() : d + ":" + a
    }

    function id() {
        this._keys = [];
        this._values = [];
        this._lastKey = NaN;
        this._lastIndex = -1
    }

    function jd(a) {
        a = Function.prototype.toString.call(a).replace(kg, "");
        return a.match(lg) || a.match(mg)
    }

    function ng(a) {
        return (a = jd(a)) ? "function(" +
            (a[1] || "").replace(/[\s\r\n]+/, " ") + ")" : "fn"
    }

    function eb(a, b) {
        function d(a) {
            return function(b, c) {
                if (G(b)) p(b, Ic(a));
                else return a(b, c)
            }
        }

        function c(a, b) {
            Ka(a, "service");
            if (E(b) || H(b)) b = q.instantiate(b);
            if (!b.$get) throw ya("pget", a);
            return n[a + "Provider"] = b
        }

        function e(a, b) {
            return function() {
                var c = N.invoke(b, this);
                if (x(c)) throw ya("undef", a);
                return c
            }
        }

        function f(a, b, d) {
            return c(a, {
                $get: !1 !== d ? e(a, b) : b
            })
        }

        function g(a) {
            fb(x(a) || H(a), "modulesToLoad", "not an array");
            var b = [],
                c;
            p(a, function(a) {
                function d(a) {
                    var b,
                        c;
                    b = 0;
                    for (c = a.length; b < c; b++) {
                        var e = a[b],
                            f = q.get(e[0]);
                        f[e[1]].apply(f, e[2])
                    }
                }
                if (!m.get(a)) {
                    m.set(a, !0);
                    try {
                        D(a) ? (c = ac(a), N.modules[a] = c, b = b.concat(g(c.requires)).concat(c._runBlocks), d(c._invokeQueue), d(c._configBlocks)) : E(a) ? b.push(q.invoke(a)) : H(a) ? b.push(q.invoke(a)) : tb(a, "module")
                    } catch (e) {
                        throw H(a) && (a = a[a.length - 1]), e.message && e.stack && -1 === e.stack.indexOf(e.message) && (e = e.message + "\n" + e.stack), ya("modulerr", a, e.stack || e.message || e);
                    }
                }
            });
            return b
        }

        function h(a, c) {
            function d(b, e) {
                if (a.hasOwnProperty(b)) {
                    if (a[b] ===
                        k) throw ya("cdep", b + " <- " + l.join(" <- "));
                    return a[b]
                }
                try {
                    return l.unshift(b), a[b] = k, a[b] = c(b, e), a[b]
                } catch (f) {
                    throw a[b] === k && delete a[b], f;
                } finally {
                    l.shift()
                }
            }

            function e(a, c, f) {
                var g = [];
                a = eb.$$annotate(a, b, f);
                for (var k = 0, h = a.length; k < h; k++) {
                    var l = a[k];
                    if ("string" !== typeof l) throw ya("itkn", l);
                    g.push(c && c.hasOwnProperty(l) ? c[l] : d(l, f))
                }
                return g
            }
            return {
                invoke: function(a, b, c, d) {
                    "string" === typeof c && (d = c, c = null);
                    c = e(a, c, d);
                    H(a) && (a = a[a.length - 1]);
                    d = a;
                    if (za || "function" !== typeof d) d = !1;
                    else {
                        var f = d.$$ngIsClass;
                        Ha(f) || (f = d.$$ngIsClass = /^(?:class\b|constructor\()/.test(Function.prototype.toString.call(d)));
                        d = f
                    }
                    return d ? (c.unshift(null), new(Function.prototype.bind.apply(a, c))) : a.apply(b, c)
                },
                instantiate: function(a, b, c) {
                    var d = H(a) ? a[a.length - 1] : a;
                    a = e(a, b, c);
                    a.unshift(null);
                    return new(Function.prototype.bind.apply(d, a))
                },
                get: d,
                annotate: eb.$$annotate,
                has: function(b) {
                    return n.hasOwnProperty(b + "Provider") || a.hasOwnProperty(b)
                }
            }
        }
        b = !0 === b;
        var k = {},
            l = [],
            m = new Hb,
            n = {
                $provide: {
                    provider: d(c),
                    factory: d(f),
                    service: d(function(a,
                        b) {
                        return f(a, ["$injector", function(a) {
                            return a.instantiate(b)
                        }])
                    }),
                    value: d(function(a, b) {
                        return f(a, la(b), !1)
                    }),
                    constant: d(function(a, b) {
                        Ka(a, "constant");
                        n[a] = b;
                        r[a] = b
                    }),
                    decorator: function(a, b) {
                        var c = q.get(a + "Provider"),
                            d = c.$get;
                        c.$get = function() {
                            var a = N.invoke(d, c);
                            return N.invoke(b, null, {
                                $delegate: a
                            })
                        }
                    }
                }
            },
            q = n.$injector = h(n, function(a, b) {
                ea.isString(b) && l.push(b);
                throw ya("unpr", l.join(" <- "));
            }),
            r = {},
            I = h(r, function(a, b) {
                var c = q.get(a + "Provider", b);
                return N.invoke(c.$get, c, void 0, a)
            }),
            N = I;
        n.$injectorProvider = {
            $get: la(I)
        };
        N.modules = q.modules = V();
        var t = g(a),
            N = I.get("$injector");
        N.strictDi = b;
        p(t, function(a) {
            a && N.invoke(a)
        });
        return N
    }

    function nf() {
        var a = !0;
        this.disableAutoScrolling = function() {
            a = !1
        };
        this.$get = ["$window", "$location", "$rootScope", function(b, d, c) {
            function e(a) {
                var b = null;
                Array.prototype.some.call(a, function(a) {
                    if ("a" === wa(a)) return b = a, !0
                });
                return b
            }

            function f(a) {
                if (a) {
                    a.scrollIntoView();
                    var c;
                    c = g.yOffset;
                    E(c) ? c = c() : Vb(c) ? (c = c[0], c = "fixed" !== b.getComputedStyle(c).position ? 0 : c.getBoundingClientRect().bottom) :
                        ba(c) || (c = 0);
                    c && (a = a.getBoundingClientRect().top, b.scrollBy(0, a - c))
                } else b.scrollTo(0, 0)
            }

            function g(a) {
                a = D(a) ? a : ba(a) ? a.toString() : d.hash();
                var b;
                a ? (b = h.getElementById(a)) ? f(b) : (b = e(h.getElementsByName(a))) ? f(b) : "top" === a && f(null) : f(null)
            }
            var h = b.document;
            a && c.$watch(function() {
                return d.hash()
            }, function(a, b) {
                a === b && "" === a || fg(function() {
                    c.$evalAsync(g)
                })
            });
            return g
        }]
    }

    function jb(a, b) {
        if (!a && !b) return "";
        if (!a) return b;
        if (!b) return a;
        H(a) && (a = a.join(" "));
        H(b) && (b = b.join(" "));
        return a + " " + b
    }

    function og(a) {
        D(a) &&
            (a = a.split(" "));
        var b = V();
        p(a, function(a) {
            a.length && (b[a] = !0)
        });
        return b
    }

    function ia(a) {
        return G(a) ? a : {}
    }

    function pg(a, b, d, c) {
        function e(a) {
            try {
                a.apply(null, va.call(arguments, 1))
            } finally {
                if (I--, 0 === I)
                    for (; N.length;) try {
                        N.pop()()
                    } catch (b) {
                        d.error(b)
                    }
            }
        }

        function f() {
            La = null;
            h()
        }

        function g() {
            t = B();
            t = x(t) ? null : t;
            pa(t, C) && (t = C);
            K = C = t
        }

        function h() {
            var a = K;
            g();
            if (y !== k.url() || a !== t) y = k.url(), K = t, p(J, function(a) {
                a(k.url(), t)
            })
        }
        var k = this,
            l = a.location,
            m = a.history,
            n = a.setTimeout,
            q = a.clearTimeout,
            r = {};
        k.isMock = !1;
        var I = 0,
            N = [];
        k.$$completeOutstandingRequest = e;
        k.$$incOutstandingRequestCount = function() {
            I++
        };
        k.notifyWhenNoOutstandingRequests = function(a) {
            0 === I ? a() : N.push(a)
        };
        var t, K, y = l.href,
            v = b.find("base"),
            La = null,
            B = c.history ? function() {
                try {
                    return m.state
                } catch (a) {}
            } : A;
        g();
        k.url = function(b, d, e) {
            x(e) && (e = null);
            l !== a.location && (l = a.location);
            m !== a.history && (m = a.history);
            if (b) {
                var f = K === e;
                if (y === b && (!c.history || f)) return k;
                var h = y && Aa(y) === Aa(b);
                y = b;
                K = e;
                !c.history || h && f ? (h || (La = b), d ? l.replace(b) : h ? (d = l, e = b.indexOf("#"),
                    e = -1 === e ? "" : b.substr(e), d.hash = e) : l.href = b, l.href !== b && (La = b)) : (m[d ? "replaceState" : "pushState"](e, "", b), g());
                La && (La = b);
                return k
            }
            return La || l.href.replace(/%27/g, "'")
        };
        k.state = function() {
            return t
        };
        var J = [],
            L = !1,
            C = null;
        k.onUrlChange = function(b) {
            if (!L) {
                if (c.history) F(a).on("popstate", f);
                F(a).on("hashchange", f);
                L = !0
            }
            J.push(b);
            return b
        };
        k.$$applicationDestroyed = function() {
            F(a).off("hashchange popstate", f)
        };
        k.$$checkUrlChange = h;
        k.baseHref = function() {
            var a = v.attr("href");
            return a ? a.replace(/^(https?:)?\/\/[^/]*/,
                "") : ""
        };
        k.defer = function(a, b) {
            var c;
            I++;
            c = n(function() {
                delete r[c];
                e(a)
            }, b || 0);
            r[c] = !0;
            return c
        };
        k.defer.cancel = function(a) {
            return r[a] ? (delete r[a], q(a), e(A), !0) : !1
        }
    }

    function uf() {
        this.$get = ["$window", "$log", "$sniffer", "$document", function(a, b, d, c) {
            return new pg(a, c, b, d)
        }]
    }

    function vf() {
        this.$get = function() {
            function a(a, c) {
                function e(a) {
                    a !== n && (q ? q === a && (q = a.n) : q = a, f(a.n, a.p), f(a, n), n = a, n.n = null)
                }

                function f(a, b) {
                    a !== b && (a && (a.p = b), b && (b.n = a))
                }
                if (a in b) throw M("$cacheFactory")("iid", a);
                var g = 0,
                    h =
                    R({}, c, {
                        id: a
                    }),
                    k = V(),
                    l = c && c.capacity || Number.MAX_VALUE,
                    m = V(),
                    n = null,
                    q = null;
                return b[a] = {
                    put: function(a, b) {
                        if (!x(b)) {
                            if (l < Number.MAX_VALUE) {
                                var c = m[a] || (m[a] = {
                                    key: a
                                });
                                e(c)
                            }
                            a in k || g++;
                            k[a] = b;
                            g > l && this.remove(q.key);
                            return b
                        }
                    },
                    get: function(a) {
                        if (l < Number.MAX_VALUE) {
                            var b = m[a];
                            if (!b) return;
                            e(b)
                        }
                        return k[a]
                    },
                    remove: function(a) {
                        if (l < Number.MAX_VALUE) {
                            var b = m[a];
                            if (!b) return;
                            b === n && (n = b.p);
                            b === q && (q = b.n);
                            f(b.n, b.p);
                            delete m[a]
                        }
                        a in k && (delete k[a], g--)
                    },
                    removeAll: function() {
                        k = V();
                        g = 0;
                        m = V();
                        n = q = null
                    },
                    destroy: function() {
                        m =
                            h = k = null;
                        delete b[a]
                    },
                    info: function() {
                        return R({}, h, {
                            size: g
                        })
                    }
                }
            }
            var b = {};
            a.info = function() {
                var a = {};
                p(b, function(b, e) {
                    a[e] = b.info()
                });
                return a
            };
            a.get = function(a) {
                return b[a]
            };
            return a
        }
    }

    function Sf() {
        this.$get = ["$cacheFactory", function(a) {
            return a("templates")
        }]
    }

    function Tc(a, b) {
        function d(a, b, c) {
            var d = /^\s*([@&<]|=(\*?))(\??)\s*([\w$]*)\s*$/,
                e = V();
            p(a, function(a, f) {
                if (a in n) e[f] = n[a];
                else {
                    var g = a.match(d);
                    if (!g) throw fa("iscp", b, f, a, c ? "controller bindings definition" : "isolate scope definition");
                    e[f] = {
                        mode: g[1][0],
                        collection: "*" === g[2],
                        optional: "?" === g[3],
                        attrName: g[4] || f
                    };
                    g[4] && (n[a] = e[f])
                }
            });
            return e
        }

        function c(a) {
            var b = a.charAt(0);
            if (!b || b !== P(b)) throw fa("baddir", a);
            if (a !== a.trim()) throw fa("baddir", a);
        }

        function e(a) {
            var b = a.require || a.controller && a.name;
            !H(b) && G(b) && p(b, function(a, c) {
                var d = a.match(l);
                a.substring(d[0].length) || (b[c] = d[0] + c)
            });
            return b
        }
        var f = {},
            g = /^\s*directive:\s*([\w-]+)\s+(.*)$/,
            h = /(([\w-]+)(?::([^;]+))?;?)/,
            k = re("ngSrc,ngSrcset,src,srcset"),
            l = /^(?:(\^\^?)?(\?)?(\^\^?)?)?/,
            m = /^(on[a-z]+|formaction)$/,
            n = V();
        this.directive = function y(b, d) {
            fb(b, "name");
            Ka(b, "directive");
            D(b) ? (c(b), fb(d, "directiveFactory"), f.hasOwnProperty(b) || (f[b] = [], a.factory(b + "Directive", ["$injector", "$exceptionHandler", function(a, c) {
                var d = [];
                p(f[b], function(f, g) {
                    try {
                        var h = a.invoke(f);
                        E(h) ? h = {
                            compile: la(h)
                        } : !h.compile && h.link && (h.compile = la(h.link));
                        h.priority = h.priority || 0;
                        h.index = g;
                        h.name = h.name || b;
                        h.require = e(h);
                        var k = h,
                            l = h.restrict;
                        if (l && (!D(l) || !/[EACM]/.test(l))) throw fa("badrestrict", l, b);
                        k.restrict =
                            l || "EA";
                        h.$$moduleName = f.$$moduleName;
                        d.push(h)
                    } catch (m) {
                        c(m)
                    }
                });
                return d
            }])), f[b].push(d)) : p(b, Ic(y));
            return this
        };
        this.component = function(a, b) {
            function c(a) {
                function e(b) {
                    return E(b) || H(b) ? function(c, d) {
                        return a.invoke(b, this, {
                            $element: c,
                            $attrs: d
                        })
                    } : b
                }
                var f = b.template || b.templateUrl ? b.template : "",
                    g = {
                        controller: d,
                        controllerAs: qg(b.controller) || b.controllerAs || "$ctrl",
                        template: e(f),
                        templateUrl: e(b.templateUrl),
                        transclude: b.transclude,
                        scope: {},
                        bindToController: b.bindings || {},
                        restrict: "E",
                        require: b.require
                    };
                p(b, function(a, b) {
                    "$" === b.charAt(0) && (g[b] = a)
                });
                return g
            }
            var d = b.controller || function() {};
            p(b, function(a, b) {
                "$" === b.charAt(0) && (c[b] = a, E(d) && (d[b] = a))
            });
            c.$inject = ["$injector"];
            return this.directive(a, c)
        };
        this.aHrefSanitizationWhitelist = function(a) {
            return u(a) ? (b.aHrefSanitizationWhitelist(a), this) : b.aHrefSanitizationWhitelist()
        };
        this.imgSrcSanitizationWhitelist = function(a) {
            return u(a) ? (b.imgSrcSanitizationWhitelist(a), this) : b.imgSrcSanitizationWhitelist()
        };
        var q = !0;
        this.debugInfoEnabled = function(a) {
            return u(a) ?
                (q = a, this) : q
        };
        var r = !1;
        this.preAssignBindingsEnabled = function(a) {
            return u(a) ? (r = a, this) : r
        };
        var I = 10;
        this.onChangesTtl = function(a) {
            return arguments.length ? (I = a, this) : I
        };
        var N = !0;
        this.commentDirectivesEnabled = function(a) {
            return arguments.length ? (N = a, this) : N
        };
        var t = !0;
        this.cssClassDirectivesEnabled = function(a) {
            return arguments.length ? (t = a, this) : t
        };
        this.$get = ["$injector", "$interpolate", "$exceptionHandler", "$templateRequest", "$parse", "$controller", "$rootScope", "$sce", "$animate", "$$sanitizeUri", function(a,
            b, c, e, n, L, C, z, O, X) {
            function T() {
                try {
                    if (!--ya) throw ia = void 0, fa("infchng", I);
                    C.$apply(function() {
                        for (var a = [], b = 0, c = ia.length; b < c; ++b) try {
                            ia[b]()
                        } catch (d) {
                            a.push(d)
                        }
                        ia = void 0;
                        if (a.length) throw a;
                    })
                } finally {
                    ya++
                }
            }

            function s(a, b) {
                if (b) {
                    var c = Object.keys(b),
                        d, e, f;
                    d = 0;
                    for (e = c.length; d < e; d++) f = c[d], this[f] = b[f]
                } else this.$attr = {};
                this.$$element = a
            }

            function Q(a, b, c) {
                ta.innerHTML = "<span " + b + ">";
                b = ta.firstChild.attributes;
                var d = b[0];
                b.removeNamedItem(d.name);
                d.value = c;
                a.attributes.setNamedItem(d)
            }

            function Ma(a,
                b) {
                try {
                    a.addClass(b)
                } catch (c) {}
            }

            function ca(a, b, c, d, e) {
                a instanceof F || (a = F(a));
                var f = Na(a, b, a, c, d, e);
                ca.$$addScopeClass(a);
                var g = null;
                return function(b, c, d) {
                    if (!a) throw fa("multilink");
                    fb(b, "scope");
                    e && e.needsNewScope && (b = b.$parent.$new());
                    d = d || {};
                    var h = d.parentBoundTranscludeFn,
                        k = d.transcludeControllers;
                    d = d.futureParentElement;
                    h && h.$$boundTransclude && (h = h.$$boundTransclude);
                    g || (g = (d = d && d[0]) ? "foreignobject" !== wa(d) && ma.call(d).match(/SVG/) ? "svg" : "html" : "html");
                    d = "html" !== g ? F(ha(g, F("<div>").append(a).html())) :
                        c ? Oa.clone.call(a) : a;
                    if (k)
                        for (var l in k) d.data("$" + l + "Controller", k[l].instance);
                    ca.$$addScopeInfo(d, b);
                    c && c(d, b);
                    f && f(b, d, d, h);
                    c || (a = f = null);
                    return d
                }
            }

            function Na(a, b, c, d, e, f) {
                function g(a, c, d, e) {
                    var f, k, l, m, n, q, r;
                    if (J)
                        for (r = Array(c.length), m = 0; m < h.length; m += 3) f = h[m], r[f] = c[f];
                    else r = c;
                    m = 0;
                    for (n = h.length; m < n;) k = r[h[m++]], c = h[m++], f = h[m++], c ? (c.scope ? (l = a.$new(), ca.$$addScopeInfo(F(k), l)) : l = a, q = c.transcludeOnThisElement ? ja(a, c.transclude, e) : !c.templateOnThisElement && e ? e : !e && b ? ja(a, b) : null, c(f, l,
                        k, d, q)) : f && f(a, k.childNodes, void 0, e)
                }
                for (var h = [], k = H(a) || a instanceof F, l, m, n, q, J, r = 0; r < a.length; r++) {
                    l = new s;
                    11 === za && M(a, r, k);
                    m = hc(a[r], [], l, 0 === r ? d : void 0, e);
                    (f = m.length ? W(m, a[r], l, b, c, null, [], [], f) : null) && f.scope && ca.$$addScopeClass(l.$$element);
                    l = f && f.terminal || !(n = a[r].childNodes) || !n.length ? null : Na(n, f ? (f.transcludeOnThisElement || !f.templateOnThisElement) && f.transclude : b);
                    if (f || l) h.push(r, f, l), q = !0, J = J || f;
                    f = null
                }
                return q ? g : null
            }

            function M(a, b, c) {
                var d = a[b],
                    e = d.parentNode,
                    f;
                if (d.nodeType ===
                    Ia)
                    for (;;) {
                        f = e ? d.nextSibling : a[b + 1];
                        if (!f || f.nodeType !== Ia) break;
                        d.nodeValue += f.nodeValue;
                        f.parentNode && f.parentNode.removeChild(f);
                        c && f === a[b + 1] && a.splice(b + 1, 1)
                    }
            }

            function ja(a, b, c) {
                function d(e, f, g, h, k) {
                    e || (e = a.$new(!1, k), e.$$transcluded = !0);
                    return b(e, f, {
                        parentBoundTranscludeFn: c,
                        transcludeControllers: g,
                        futureParentElement: h
                    })
                }
                var e = d.$$slots = V(),
                    f;
                for (f in b.$$slots) e[f] = b.$$slots[f] ? ja(a, b.$$slots[f], c) : null;
                return d
            }

            function hc(a, b, c, d, e) {
                var f = c.$attr,
                    g;
                switch (a.nodeType) {
                    case 1:
                        g = wa(a);
                        Y(b,
                            Ba(g), "E", d, e);
                        for (var k, l, m, n, q = a.attributes, J = 0, r = q && q.length; J < r; J++) {
                            var B = !1,
                                C = !1;
                            k = q[J];
                            l = k.name;
                            m = k.value;
                            k = Ba(l);
                            (n = Ja.test(k)) && (l = l.replace(kd, "").substr(8).replace(/_(.)/g, function(a, b) {
                                return b.toUpperCase()
                            }));
                            (k = k.match(Ka)) && Z(k[1]) && (B = l, C = l.substr(0, l.length - 5) + "end", l = l.substr(0, l.length - 6));
                            k = Ba(l.toLowerCase());
                            f[k] = l;
                            if (n || !c.hasOwnProperty(k)) c[k] = m, gd(a, k) && (c[k] = !0);
                            qa(a, b, m, k, n);
                            Y(b, k, "A", d, e, B, C)
                        }
                        "input" === g && "hidden" === a.getAttribute("type") && a.setAttribute("autocomplete",
                            "off");
                        if (!Ga) break;
                        f = a.className;
                        G(f) && (f = f.animVal);
                        if (D(f) && "" !== f)
                            for (; a = h.exec(f);) k = Ba(a[2]), Y(b, k, "C", d, e) && (c[k] = S(a[3])), f = f.substr(a.index + a[0].length);
                        break;
                    case Ia:
                        la(b, a.nodeValue);
                        break;
                    case 8:
                        if (!Fa) break;
                        kb(a, b, c, d, e)
                }
                b.sort(ea);
                return b
            }

            function kb(a, b, c, d, e) {
                try {
                    var f = g.exec(a.nodeValue);
                    if (f) {
                        var h = Ba(f[1]);
                        Y(b, h, "M", d, e) && (c[h] = S(f[2]))
                    }
                } catch (k) {}
            }

            function ld(a, b, c) {
                var d = [],
                    e = 0;
                if (b && a.hasAttribute && a.hasAttribute(b)) {
                    do {
                        if (!a) throw fa("uterdir", b, c);
                        1 === a.nodeType && (a.hasAttribute(b) &&
                            e++, a.hasAttribute(c) && e--);
                        d.push(a);
                        a = a.nextSibling
                    } while (0 < e)
                } else d.push(a);
                return F(d)
            }

            function md(a, b, c) {
                return function(d, e, f, g, h) {
                    e = ld(e[0], b, c);
                    return a(d, e, f, g, h)
                }
            }

            function ic(a, b, c, d, e, f) {
                var g;
                return a ? ca(b, c, d, e, f) : function() {
                    g || (g = ca(b, c, d, e, f), b = c = f = null);
                    return g.apply(this, arguments)
                }
            }

            function W(a, b, d, e, f, g, h, k, l) {
                function m(a, b, c, d) {
                    if (a) {
                        c && (a = md(a, c, d));
                        a.require = z.require;
                        a.directiveName = v;
                        if (C === z || z.$$isolateScope) a = ra(a, {
                            isolateScope: !0
                        });
                        h.push(a)
                    }
                    if (b) {
                        c && (b = md(b, c, d));
                        b.require =
                            z.require;
                        b.directiveName = v;
                        if (C === z || z.$$isolateScope) b = ra(b, {
                            isolateScope: !0
                        });
                        k.push(b)
                    }
                }

                function n(a, e, f, g, l) {
                    function m(a, b, c, d) {
                        var e;
                        Za(a) || (d = c, c = b, b = a, a = void 0);
                        X && (e = O);
                        c || (c = X ? v.parent() : v);
                        if (d) {
                            var f = l.$$slots[d];
                            if (f) return f(a, b, e, c, Q);
                            if (x(f)) throw fa("noslot", d, xa(v));
                        } else return l(a, b, e, c, Q)
                    }
                    var q, z, t, I, y, O, T, v;
                    b === f ? (g = d, v = d.$$element) : (v = F(f), g = new s(v, d));
                    y = e;
                    C ? I = e.$new(!0) : J && (y = e.$parent);
                    l && (T = m, T.$$boundTransclude = l, T.isSlotFilled = function(a) {
                        return !!l.$$slots[a]
                    });
                    B && (O =
                        ba(v, g, T, B, I, e, C));
                    C && (ca.$$addScopeInfo(v, I, !0, !(L && (L === C || L === C.$$originalDirective))), ca.$$addScopeClass(v, !0), I.$$isolateBindings = C.$$isolateBindings, z = na(e, g, I, I.$$isolateBindings, C), z.removeWatches && I.$on("$destroy", z.removeWatches));
                    for (q in O) {
                        z = B[q];
                        t = O[q];
                        var Ib = z.$$bindings.bindToController;
                        if (r) {
                            t.bindingInfo = Ib ? na(y, g, t.instance, Ib, z) : {};
                            var N = t();
                            N !== t.instance && (t.instance = N, v.data("$" + z.name + "Controller", N), t.bindingInfo.removeWatches && t.bindingInfo.removeWatches(), t.bindingInfo =
                                na(y, g, t.instance, Ib, z))
                        } else t.instance = t(), v.data("$" + z.name + "Controller", t.instance), t.bindingInfo = na(y, g, t.instance, Ib, z)
                    }
                    p(B, function(a, b) {
                        var c = a.require;
                        a.bindToController && !H(c) && G(c) && R(O[b].instance, U(b, c, v, O))
                    });
                    p(O, function(a) {
                        var b = a.instance;
                        if (E(b.$onChanges)) try {
                            b.$onChanges(a.bindingInfo.initialChanges)
                        } catch (d) {
                            c(d)
                        }
                        if (E(b.$onInit)) try {
                            b.$onInit()
                        } catch (e) {
                            c(e)
                        }
                        E(b.$doCheck) && (y.$watch(function() {
                            b.$doCheck()
                        }), b.$doCheck());
                        E(b.$onDestroy) && y.$on("$destroy", function() {
                            b.$onDestroy()
                        })
                    });
                    q = 0;
                    for (z = h.length; q < z; q++) t = h[q], sa(t, t.isolateScope ? I : e, v, g, t.require && U(t.directiveName, t.require, v, O), T);
                    var Q = e;
                    C && (C.template || null === C.templateUrl) && (Q = I);
                    a && a(Q, f.childNodes, void 0, l);
                    for (q = k.length - 1; 0 <= q; q--) t = k[q], sa(t, t.isolateScope ? I : e, v, g, t.require && U(t.directiveName, t.require, v, O), T);
                    p(O, function(a) {
                        a = a.instance;
                        E(a.$postLink) && a.$postLink()
                    })
                }
                l = l || {};
                for (var q = -Number.MAX_VALUE, J = l.newScopeDirective, B = l.controllerDirectives, C = l.newIsolateScopeDirective, L = l.templateDirective, t = l.nonTlbTranscludeDirective,
                        I = !1, O = !1, X = l.hasElementTranscludeDirective, y = d.$$element = F(b), z, v, T, N = e, Q, u = !1, Ma = !1, w, A = 0, D = a.length; A < D; A++) {
                    z = a[A];
                    var Na = z.$$start,
                        M = z.$$end;
                    Na && (y = ld(b, Na, M));
                    T = void 0;
                    if (q > z.priority) break;
                    if (w = z.scope) z.templateUrl || (G(w) ? ($("new/isolated scope", C || J, z, y), C = z) : $("new/isolated scope", C, z, y)), J = J || z;
                    v = z.name;
                    if (!u && (z.replace && (z.templateUrl || z.template) || z.transclude && !z.$$tlb)) {
                        for (w = A + 1; u = a[w++];)
                            if (u.transclude && !u.$$tlb || u.replace && (u.templateUrl || u.template)) {
                                Ma = !0;
                                break
                            } u = !0
                    }!z.templateUrl &&
                        z.controller && (B = B || V(), $("'" + v + "' controller", B[v], z, y), B[v] = z);
                    if (w = z.transclude)
                        if (I = !0, z.$$tlb || ($("transclusion", t, z, y), t = z), "element" === w) X = !0, q = z.priority, T = y, y = d.$$element = F(ca.$$createComment(v, d[v])), b = y[0], ka(f, va.call(T, 0), b), T[0].$$parentNode = T[0].parentNode, N = ic(Ma, T, e, q, g && g.name, {
                            nonTlbTranscludeDirective: t
                        });
                        else {
                            var ja = V();
                            if (G(w)) {
                                T = [];
                                var P = V(),
                                    kb = V();
                                p(w, function(a, b) {
                                    var c = "?" === a.charAt(0);
                                    a = c ? a.substring(1) : a;
                                    P[a] = b;
                                    ja[b] = null;
                                    kb[b] = c
                                });
                                p(y.contents(), function(a) {
                                    var b = P[Ba(wa(a))];
                                    b ? (kb[b] = !0, ja[b] = ja[b] || [], ja[b].push(a)) : T.push(a)
                                });
                                p(kb, function(a, b) {
                                    if (!a) throw fa("reqslot", b);
                                });
                                for (var gc in ja) ja[gc] && (ja[gc] = ic(Ma, ja[gc], e))
                            } else T = F(ec(b)).contents();
                            y.empty();
                            N = ic(Ma, T, e, void 0, void 0, {
                                needsNewScope: z.$$isolateScope || z.$$newScope
                            });
                            N.$$slots = ja
                        } if (z.template)
                        if (O = !0, $("template", L, z, y), L = z, w = E(z.template) ? z.template(y, d) : z.template, w = Ea(w), z.replace) {
                            g = z;
                            T = bc.test(w) ? nd(ha(z.templateNamespace, S(w))) : [];
                            b = T[0];
                            if (1 !== T.length || 1 !== b.nodeType) throw fa("tplrt", v, "");
                            ka(f, y, b);
                            D = {
                                $attr: {}
                            };
                            w = hc(b, [], D);
                            var Y = a.splice(A + 1, a.length - (A + 1));
                            (C || J) && aa(w, C, J);
                            a = a.concat(w).concat(Y);
                            da(d, D);
                            D = a.length
                        } else y.html(w);
                    if (z.templateUrl) O = !0, $("template", L, z, y), L = z, z.replace && (g = z), n = ga(a.splice(A, a.length - A), y, d, f, I && N, h, k, {
                        controllerDirectives: B,
                        newScopeDirective: J !== z && J,
                        newIsolateScopeDirective: C,
                        templateDirective: L,
                        nonTlbTranscludeDirective: t
                    }), D = a.length;
                    else if (z.compile) try {
                        Q = z.compile(y, d, N);
                        var Z = z.$$originalDirective || z;
                        E(Q) ? m(null, bb(Z, Q), Na, M) : Q && m(bb(Z, Q.pre),
                            bb(Z, Q.post), Na, M)
                    } catch (ea) {
                        c(ea, xa(y))
                    }
                    z.terminal && (n.terminal = !0, q = Math.max(q, z.priority))
                }
                n.scope = J && !0 === J.scope;
                n.transcludeOnThisElement = I;
                n.templateOnThisElement = O;
                n.transclude = N;
                l.hasElementTranscludeDirective = X;
                return n
            }

            function U(a, b, c, d) {
                var e;
                if (D(b)) {
                    var f = b.match(l);
                    b = b.substring(f[0].length);
                    var g = f[1] || f[3],
                        f = "?" === f[2];
                    "^^" === g ? c = c.parent() : e = (e = d && d[b]) && e.instance;
                    if (!e) {
                        var h = "$" + b + "Controller";
                        e = g ? c.inheritedData(h) : c.data(h)
                    }
                    if (!e && !f) throw fa("ctreq", b, a);
                } else if (H(b))
                    for (e = [], g = 0, f = b.length; g < f; g++) e[g] = U(a, b[g], c, d);
                else G(b) && (e = {}, p(b, function(b, f) {
                    e[f] = U(a, b, c, d)
                }));
                return e || null
            }

            function ba(a, b, c, d, e, f, g) {
                var h = V(),
                    k;
                for (k in d) {
                    var l = d[k],
                        m = {
                            $scope: l === g || l.$$isolateScope ? e : f,
                            $element: a,
                            $attrs: b,
                            $transclude: c
                        },
                        n = l.controller;
                    "@" === n && (n = b[l.name]);
                    m = L(n, m, !0, l.controllerAs);
                    h[l.name] = m;
                    a.data("$" + l.name + "Controller", m.instance)
                }
                return h
            }

            function aa(a, b, c) {
                for (var d = 0, e = a.length; d < e; d++) a[d] = Wb(a[d], {
                    $$isolateScope: b,
                    $$newScope: c
                })
            }

            function Y(b, c, e, g, h, k, l) {
                if (c ===
                    h) return null;
                var m = null;
                if (f.hasOwnProperty(c)) {
                    h = a.get(c + "Directive");
                    for (var n = 0, q = h.length; n < q; n++)
                        if (c = h[n], (x(g) || g > c.priority) && -1 !== c.restrict.indexOf(e)) {
                            k && (c = Wb(c, {
                                $$start: k,
                                $$end: l
                            }));
                            if (!c.$$bindings) {
                                var J = m = c,
                                    r = c.name,
                                    B = {
                                        isolateScope: null,
                                        bindToController: null
                                    };
                                G(J.scope) && (!0 === J.bindToController ? (B.bindToController = d(J.scope, r, !0), B.isolateScope = {}) : B.isolateScope = d(J.scope, r, !1));
                                G(J.bindToController) && (B.bindToController = d(J.bindToController, r, !0));
                                if (B.bindToController && !J.controller) throw fa("noctrl",
                                    r);
                                m = m.$$bindings = B;
                                G(m.isolateScope) && (c.$$isolateBindings = m.isolateScope)
                            }
                            b.push(c);
                            m = c
                        }
                }
                return m
            }

            function Z(b) {
                if (f.hasOwnProperty(b))
                    for (var c = a.get(b + "Directive"), d = 0, e = c.length; d < e; d++)
                        if (b = c[d], b.multiElement) return !0;
                return !1
            }

            function da(a, b) {
                var c = b.$attr,
                    d = a.$attr;
                p(a, function(d, e) {
                    "$" !== e.charAt(0) && (b[e] && b[e] !== d && (d = d.length ? d + (("style" === e ? ";" : " ") + b[e]) : b[e]), a.$set(e, d, !0, c[e]))
                });
                p(b, function(b, e) {
                    a.hasOwnProperty(e) || "$" === e.charAt(0) || (a[e] = b, "class" !== e && "style" !== e && (d[e] = c[e]))
                })
            }

            function ga(a, b, d, f, g, h, k, l) {
                var m = [],
                    n, q, J = b[0],
                    r = a.shift(),
                    z = Wb(r, {
                        templateUrl: null,
                        transclude: null,
                        replace: null,
                        $$originalDirective: r
                    }),
                    t = E(r.templateUrl) ? r.templateUrl(b, d) : r.templateUrl,
                    C = r.templateNamespace;
                b.empty();
                e(t).then(function(c) {
                    var e, B;
                    c = Ea(c);
                    if (r.replace) {
                        c = bc.test(c) ? nd(ha(C, S(c))) : [];
                        e = c[0];
                        if (1 !== c.length || 1 !== e.nodeType) throw fa("tplrt", r.name, t);
                        c = {
                            $attr: {}
                        };
                        ka(f, b, e);
                        var L = hc(e, [], c);
                        G(r.scope) && aa(L, !0);
                        a = L.concat(a);
                        da(d, c)
                    } else e = J, b.html(c);
                    a.unshift(z);
                    n = W(a, e, d, g, b,
                        r, h, k, l);
                    p(f, function(a, c) {
                        a === e && (f[c] = b[0])
                    });
                    for (q = Na(b[0].childNodes, g); m.length;) {
                        c = m.shift();
                        B = m.shift();
                        var I = m.shift(),
                            y = m.shift(),
                            L = b[0];
                        if (!c.$$destroyed) {
                            if (B !== J) {
                                var O = B.className;
                                l.hasElementTranscludeDirective && r.replace || (L = ec(e));
                                ka(I, F(B), L);
                                Ma(F(L), O)
                            }
                            B = n.transcludeOnThisElement ? ja(c, n.transclude, y) : y;
                            n(q, c, L, f, B)
                        }
                    }
                    m = null
                }).catch(function(a) {
                    a instanceof Error && c(a)
                });
                return function(a, b, c, d, e) {
                    a = e;
                    b.$$destroyed || (m ? m.push(b, c, d, a) : (n.transcludeOnThisElement && (a = ja(b, n.transclude,
                        e)), n(q, b, c, d, a)))
                }
            }

            function ea(a, b) {
                var c = b.priority - a.priority;
                return 0 !== c ? c : a.name !== b.name ? a.name < b.name ? -1 : 1 : a.index - b.index
            }

            function $(a, b, c, d) {
                function e(a) {
                    return a ? " (module: " + a + ")" : ""
                }
                if (b) throw fa("multidir", b.name, e(b.$$moduleName), c.name, e(c.$$moduleName), a, xa(d));
            }

            function la(a, c) {
                var d = b(c, !0);
                d && a.push({
                    priority: 0,
                    compile: function(a) {
                        a = a.parent();
                        var b = !!a.length;
                        b && ca.$$addBindingClass(a);
                        return function(a, c) {
                            var e = c.parent();
                            b || ca.$$addBindingClass(e);
                            ca.$$addBindingInfo(e, d.expressions);
                            a.$watch(d, function(a) {
                                c[0].nodeValue = a
                            })
                        }
                    }
                })
            }

            function ha(a, b) {
                a = P(a || "html");
                switch (a) {
                    case "svg":
                    case "math":
                        var c = w.document.createElement("div");
                        c.innerHTML = "<" + a + ">" + b + "</" + a + ">";
                        return c.childNodes[0].childNodes;
                    default:
                        return b
                }
            }

            function oa(a, b) {
                if ("srcdoc" === b) return z.HTML;
                var c = wa(a);
                if ("src" === b || "ngSrc" === b) {
                    if (-1 === ["img", "video", "audio", "source", "track"].indexOf(c)) return z.RESOURCE_URL
                } else if ("xlinkHref" === b || "form" === c && "action" === b || "link" === c && "href" === b) return z.RESOURCE_URL
            }

            function qa(a,
                c, d, e, f) {
                var g = oa(a, e),
                    h = k[e] || f,
                    l = b(d, !f, g, h);
                if (l) {
                    if ("multiple" === e && "select" === wa(a)) throw fa("selmulti", xa(a));
                    if (m.test(e)) throw fa("nodomevents");
                    c.push({
                        priority: 100,
                        compile: function() {
                            return {
                                pre: function(a, c, f) {
                                    c = f.$$observers || (f.$$observers = V());
                                    var k = f[e];
                                    k !== d && (l = k && b(k, !0, g, h), d = k);
                                    l && (f[e] = l(a), (c[e] || (c[e] = [])).$$inter = !0, (f.$$observers && f.$$observers[e].$$scope || a).$watch(l, function(a, b) {
                                        "class" === e && a !== b ? f.$updateClass(a, b) : f.$set(e, a)
                                    }))
                                }
                            }
                        }
                    })
                }
            }

            function ka(a, b, c) {
                var d = b[0],
                    e =
                    b.length,
                    f = d.parentNode,
                    g, h;
                if (a)
                    for (g = 0, h = a.length; g < h; g++)
                        if (a[g] === d) {
                            a[g++] = c;
                            h = g + e - 1;
                            for (var k = a.length; g < k; g++, h++) h < k ? a[g] = a[h] : delete a[g];
                            a.length -= e - 1;
                            a.context === d && (a.context = c);
                            break
                        } f && f.replaceChild(c, d);
                a = w.document.createDocumentFragment();
                for (g = 0; g < e; g++) a.appendChild(b[g]);
                F.hasData(d) && (F.data(c, F.data(d)), F(d).off("$destroy"));
                F.cleanData(a.querySelectorAll("*"));
                for (g = 1; g < e; g++) delete b[g];
                b[0] = c;
                b.length = 1
            }

            function ra(a, b) {
                return R(function() {
                        return a.apply(null, arguments)
                    },
                    a, b)
            }

            function sa(a, b, d, e, f, g) {
                try {
                    a(b, d, e, f, g)
                } catch (h) {
                    c(h, xa(d))
                }
            }

            function na(a, c, d, e, f) {
                function g(b, c, e) {
                    !E(d.$onChanges) || c === e || c !== c && e !== e || (ia || (a.$$postDigest(T), ia = []), m || (m = {}, ia.push(h)), m[b] && (e = m[b].previousValue), m[b] = new Jb(e, c))
                }

                function h() {
                    d.$onChanges(m);
                    m = void 0
                }
                var k = [],
                    l = {},
                    m;
                p(e, function(e, h) {
                    var m = e.attrName,
                        q = e.optional,
                        r, B, t, z;
                    switch (e.mode) {
                        case "@":
                            q || ua.call(c, m) || (d[h] = c[m] = void 0);
                            q = c.$observe(m, function(a) {
                                if (D(a) || Ha(a)) g(h, a, d[h]), d[h] = a
                            });
                            c.$$observers[m].$$scope =
                                a;
                            r = c[m];
                            D(r) ? d[h] = b(r)(a) : Ha(r) && (d[h] = r);
                            l[h] = new Jb(jc, d[h]);
                            k.push(q);
                            break;
                        case "=":
                            if (!ua.call(c, m)) {
                                if (q) break;
                                c[m] = void 0
                            }
                            if (q && !c[m]) break;
                            B = n(c[m]);
                            z = B.literal ? pa : function(a, b) {
                                return a === b || a !== a && b !== b
                            };
                            t = B.assign || function() {
                                r = d[h] = B(a);
                                throw fa("nonassign", c[m], m, f.name);
                            };
                            r = d[h] = B(a);
                            q = function(b) {
                                z(b, d[h]) || (z(b, r) ? t(a, b = d[h]) : d[h] = b);
                                return r = b
                            };
                            q.$stateful = !0;
                            q = e.collection ? a.$watchCollection(c[m], q) : a.$watch(n(c[m], q), null, B.literal);
                            k.push(q);
                            break;
                        case "<":
                            if (!ua.call(c, m)) {
                                if (q) break;
                                c[m] = void 0
                            }
                            if (q && !c[m]) break;
                            B = n(c[m]);
                            var C = B.literal,
                                L = d[h] = B(a);
                            l[h] = new Jb(jc, d[h]);
                            q = a.$watch(B, function(a, b) {
                                if (b === a) {
                                    if (b === L || C && pa(b, L)) return;
                                    b = L
                                }
                                g(h, a, b);
                                d[h] = a
                            }, C);
                            k.push(q);
                            break;
                        case "&":
                            B = c.hasOwnProperty(m) ? n(c[m]) : A;
                            if (B === A && q) break;
                            d[h] = function(b) {
                                return B(a, b)
                            }
                    }
                });
                return {
                    initialChanges: l,
                    removeWatches: k.length && function() {
                        for (var a = 0, b = k.length; a < b; ++a) k[a]()
                    }
                }
            }
            var Ca = /^\w/,
                ta = w.document.createElement("div"),
                Fa = N,
                Ga = t,
                ya = I,
                ia;
            s.prototype = {
                $normalize: Ba,
                $addClass: function(a) {
                    a &&
                        0 < a.length && O.addClass(this.$$element, a)
                },
                $removeClass: function(a) {
                    a && 0 < a.length && O.removeClass(this.$$element, a)
                },
                $updateClass: function(a, b) {
                    var c = od(a, b);
                    c && c.length && O.addClass(this.$$element, c);
                    (c = od(b, a)) && c.length && O.removeClass(this.$$element, c)
                },
                $set: function(a, b, d, e) {
                    var f = gd(this.$$element[0], a),
                        g = pd[a],
                        h = a;
                    f ? (this.$$element.prop(a, b), e = f) : g && (this[g] = b, h = g);
                    this[a] = b;
                    e ? this.$attr[a] = e : (e = this.$attr[a]) || (this.$attr[a] = e = Qc(a, "-"));
                    f = wa(this.$$element);
                    if ("a" === f && ("href" === a || "xlinkHref" ===
                            a) || "img" === f && "src" === a) this[a] = b = X(b, "src" === a);
                    else if ("img" === f && "srcset" === a && u(b)) {
                        for (var f = "", g = S(b), k = /(\s+\d+x\s*,|\s+\d+w\s*,|\s+,|,\s+)/, k = /\s/.test(g) ? k : /(,)/, g = g.split(k), k = Math.floor(g.length / 2), l = 0; l < k; l++) var m = 2 * l,
                            f = f + X(S(g[m]), !0),
                            f = f + (" " + S(g[m + 1]));
                        g = S(g[2 * l]).split(/\s/);
                        f += X(S(g[0]), !0);
                        2 === g.length && (f += " " + S(g[1]));
                        this[a] = b = f
                    }!1 !== d && (null === b || x(b) ? this.$$element.removeAttr(e) : Ca.test(e) ? this.$$element.attr(e, b) : Q(this.$$element[0], e, b));
                    (a = this.$$observers) && p(a[h], function(a) {
                        try {
                            a(b)
                        } catch (d) {
                            c(d)
                        }
                    })
                },
                $observe: function(a, b) {
                    var c = this,
                        d = c.$$observers || (c.$$observers = V()),
                        e = d[a] || (d[a] = []);
                    e.push(b);
                    C.$evalAsync(function() {
                        e.$$inter || !c.hasOwnProperty(a) || x(c[a]) || b(c[a])
                    });
                    return function() {
                        $a(e, b)
                    }
                }
            };
            var Aa = b.startSymbol(),
                Da = b.endSymbol(),
                Ea = "{{" === Aa && "}}" === Da ? Ya : function(a) {
                    return a.replace(/\{\{/g, Aa).replace(/}}/g, Da)
                },
                Ja = /^ngAttr[A-Z]/,
                Ka = /^(.+)Start$/;
            ca.$$addBindingInfo = q ? function(a, b) {
                var c = a.data("$binding") || [];
                H(b) ? c = c.concat(b) : c.push(b);
                a.data("$binding", c)
            } : A;
            ca.$$addBindingClass =
                q ? function(a) {
                    Ma(a, "ng-binding")
                } : A;
            ca.$$addScopeInfo = q ? function(a, b, c, d) {
                a.data(c ? d ? "$isolateScopeNoTemplate" : "$isolateScope" : "$scope", b)
            } : A;
            ca.$$addScopeClass = q ? function(a, b) {
                Ma(a, b ? "ng-isolate-scope" : "ng-scope")
            } : A;
            ca.$$createComment = function(a, b) {
                var c = "";
                q && (c = " " + (a || "") + ": ", b && (c += b + " "));
                return w.document.createComment(c)
            };
            return ca
        }]
    }

    function Jb(a, b) {
        this.previousValue = a;
        this.currentValue = b
    }

    function Ba(a) {
        return a.replace(kd, "").replace(rg, gb)
    }

    function od(a, b) {
        var d = "",
            c = a.split(/\s+/),
            e = b.split(/\s+/),
            f = 0;
        a: for (; f < c.length; f++) {
            for (var g = c[f], h = 0; h < e.length; h++)
                if (g === e[h]) continue a;
            d += (0 < d.length ? " " : "") + g
        }
        return d
    }

    function nd(a) {
        a = F(a);
        var b = a.length;
        if (1 >= b) return a;
        for (; b--;) {
            var d = a[b];
            (8 === d.nodeType || d.nodeType === Ia && "" === d.nodeValue.trim()) && sg.call(a, b, 1)
        }
        return a
    }

    function qg(a, b) {
        if (b && D(b)) return b;
        if (D(a)) {
            var d = qd.exec(a);
            if (d) return d[3]
        }
    }

    function wf() {
        var a = {},
            b = !1;
        this.has = function(b) {
            return a.hasOwnProperty(b)
        };
        this.register = function(b, c) {
            Ka(b, "controller");
            G(b) ?
                R(a, b) : a[b] = c
        };
        this.allowGlobals = function() {
            b = !0
        };
        this.$get = ["$injector", "$window", function(d, c) {
            function e(a, b, c, d) {
                if (!a || !G(a.$scope)) throw M("$controller")("noscp", d, b);
                a.$scope[b] = c
            }
            return function(f, g, h, k) {
                var l, m, n;
                h = !0 === h;
                k && D(k) && (n = k);
                if (D(f)) {
                    k = f.match(qd);
                    if (!k) throw rd("ctrlfmt", f);
                    m = k[1];
                    n = n || k[3];
                    f = a.hasOwnProperty(m) ? a[m] : Sc(g.$scope, m, !0) || (b ? Sc(c, m, !0) : void 0);
                    if (!f) throw rd("ctrlreg", m);
                    tb(f, m, !0)
                }
                if (h) return h = (H(f) ? f[f.length - 1] : f).prototype, l = Object.create(h || null), n && e(g, n,
                    l, m || f.name), R(function() {
                    var a = d.invoke(f, l, g, m);
                    a !== l && (G(a) || E(a)) && (l = a, n && e(g, n, l, m || f.name));
                    return l
                }, {
                    instance: l,
                    identifier: n
                });
                l = d.instantiate(f, g, m);
                n && e(g, n, l, m || f.name);
                return l
            }
        }]
    }

    function xf() {
        this.$get = ["$window", function(a) {
            return F(a.document)
        }]
    }

    function yf() {
        this.$get = ["$document", "$rootScope", function(a, b) {
            function d() {
                e = c.hidden
            }
            var c = a[0],
                e = c && c.hidden;
            a.on("visibilitychange", d);
            b.$on("$destroy", function() {
                a.off("visibilitychange", d)
            });
            return function() {
                return e
            }
        }]
    }

    function zf() {
        this.$get = ["$log", function(a) {
            return function(b, d) {
                a.error.apply(a, arguments)
            }
        }]
    }

    function kc(a) {
        return G(a) ? ga(a) ? a.toISOString() : cb(a) : a
    }

    function Ef() {
        this.$get = function() {
            return function(a) {
                if (!a) return "";
                var b = [];
                Hc(a, function(a, c) {
                    null === a || x(a) || (H(a) ? p(a, function(a) {
                        b.push($(c) + "=" + $(kc(a)))
                    }) : b.push($(c) + "=" + $(kc(a))))
                });
                return b.join("&")
            }
        }
    }

    function Ff() {
        this.$get = function() {
            return function(a) {
                function b(a, e, f) {
                    null === a || x(a) || (H(a) ? p(a, function(a, c) {
                        b(a, e + "[" + (G(a) ? c : "") + "]")
                    }) : G(a) && !ga(a) ? Hc(a, function(a,
                        c) {
                        b(a, e + (f ? "" : "[") + c + (f ? "" : "]"))
                    }) : d.push($(e) + "=" + $(kc(a))))
                }
                if (!a) return "";
                var d = [];
                b(a, "", !0);
                return d.join("&")
            }
        }
    }

    function lc(a, b) {
        if (D(a)) {
            var d = a.replace(tg, "").trim();
            if (d) {
                var c = b("Content-Type");
                (c = c && 0 === c.indexOf(sd)) || (c = (c = d.match(ug)) && vg[c[0]].test(d));
                c && (a = Lc(d))
            }
        }
        return a
    }

    function td(a) {
        var b = V(),
            d;
        D(a) ? p(a.split("\n"), function(a) {
            d = a.indexOf(":");
            var e = P(S(a.substr(0, d)));
            a = S(a.substr(d + 1));
            e && (b[e] = b[e] ? b[e] + ", " + a : a)
        }) : G(a) && p(a, function(a, d) {
            var f = P(d),
                g = S(a);
            f && (b[f] = b[f] ?
                b[f] + ", " + g : g)
        });
        return b
    }

    function ud(a) {
        var b;
        return function(d) {
            b || (b = td(a));
            return d ? (d = b[P(d)], void 0 === d && (d = null), d) : b
        }
    }

    function vd(a, b, d, c) {
        if (E(c)) return c(a, b, d);
        p(c, function(c) {
            a = c(a, b, d)
        });
        return a
    }

    function Df() {
        var a = this.defaults = {
                transformResponse: [lc],
                transformRequest: [function(a) {
                    return G(a) && "[object File]" !== ma.call(a) && "[object Blob]" !== ma.call(a) && "[object FormData]" !== ma.call(a) ? cb(a) : a
                }],
                headers: {
                    common: {
                        Accept: "application/json, text/plain, */*"
                    },
                    post: qa(mc),
                    put: qa(mc),
                    patch: qa(mc)
                },
                xsrfCookieName: "XSRF-TOKEN",
                xsrfHeaderName: "X-XSRF-TOKEN",
                paramSerializer: "$httpParamSerializer",
                jsonpCallbackParam: "callback"
            },
            b = !1;
        this.useApplyAsync = function(a) {
            return u(a) ? (b = !!a, this) : b
        };
        var d = this.interceptors = [];
        this.$get = ["$browser", "$httpBackend", "$$cookieReader", "$cacheFactory", "$rootScope", "$q", "$injector", "$sce", function(c, e, f, g, h, k, l, m) {
            function n(b) {
                function d(a, b) {
                    for (var c = 0, e = b.length; c < e;) {
                        var f = b[c++],
                            g = b[c++];
                        a = a.then(f, g)
                    }
                    b.length = 0;
                    return a
                }

                function e(a, b) {
                    var c, d = {};
                    p(a, function(a,
                        e) {
                        E(a) ? (c = a(b), null != c && (d[e] = c)) : d[e] = a
                    });
                    return d
                }

                function f(a) {
                    var b = R({}, a);
                    b.data = vd(a.data, a.headers, a.status, g.transformResponse);
                    a = a.status;
                    return 200 <= a && 300 > a ? b : k.reject(b)
                }
                if (!G(b)) throw M("$http")("badreq", b);
                if (!D(m.valueOf(b.url))) throw M("$http")("badreq", b.url);
                var g = R({
                    method: "get",
                    transformRequest: a.transformRequest,
                    transformResponse: a.transformResponse,
                    paramSerializer: a.paramSerializer,
                    jsonpCallbackParam: a.jsonpCallbackParam
                }, b);
                g.headers = function(b) {
                    var c = a.headers,
                        d = R({}, b.headers),
                        f, g, h, c = R({}, c.common, c[P(b.method)]);
                    a: for (f in c) {
                        g = P(f);
                        for (h in d)
                            if (P(h) === g) continue a;
                        d[f] = c[f]
                    }
                    return e(d, qa(b))
                }(b);
                g.method = vb(g.method);
                g.paramSerializer = D(g.paramSerializer) ? l.get(g.paramSerializer) : g.paramSerializer;
                c.$$incOutstandingRequestCount();
                var h = [],
                    n = [];
                b = k.resolve(g);
                p(t, function(a) {
                    (a.request || a.requestError) && h.unshift(a.request, a.requestError);
                    (a.response || a.responseError) && n.push(a.response, a.responseError)
                });
                b = d(b, h);
                b = b.then(function(b) {
                    var c = b.headers,
                        d = vd(b.data, ud(c),
                            void 0, b.transformRequest);
                    x(d) && p(c, function(a, b) {
                        "content-type" === P(b) && delete c[b]
                    });
                    x(b.withCredentials) && !x(a.withCredentials) && (b.withCredentials = a.withCredentials);
                    return q(b, d).then(f, f)
                });
                b = d(b, n);
                return b = b.finally(function() {
                    c.$$completeOutstandingRequest(A)
                })
            }

            function q(c, d) {
                function g(a) {
                    if (a) {
                        var c = {};
                        p(a, function(a, d) {
                            c[d] = function(c) {
                                function d() {
                                    a(c)
                                }
                                b ? h.$applyAsync(d) : h.$$phase ? d() : h.$apply(d)
                            }
                        });
                        return c
                    }
                }

                function l(a, c, d, e) {
                    function f() {
                        q(c, a, d, e)
                    }
                    O && (200 <= a && 300 > a ? O.put(Q, [a,
                        c, td(d), e
                    ]) : O.remove(Q));
                    b ? h.$applyAsync(f) : (f(), h.$$phase || h.$apply())
                }

                function q(a, b, d, e) {
                    b = -1 <= b ? b : 0;
                    (200 <= b && 300 > b ? C.resolve : C.reject)({
                        data: a,
                        status: b,
                        headers: ud(d),
                        config: c,
                        statusText: e
                    })
                }

                function J(a) {
                    q(a.data, a.status, qa(a.headers()), a.statusText)
                }

                function t() {
                    var a = n.pendingRequests.indexOf(c); - 1 !== a && n.pendingRequests.splice(a, 1)
                }
                var C = k.defer(),
                    z = C.promise,
                    O, X, T = c.headers,
                    s = "jsonp" === P(c.method),
                    Q = c.url;
                s ? Q = m.getTrustedResourceUrl(Q) : D(Q) || (Q = m.valueOf(Q));
                Q = r(Q, c.paramSerializer(c.params));
                s && (Q = I(Q, c.jsonpCallbackParam));
                n.pendingRequests.push(c);
                z.then(t, t);
                !c.cache && !a.cache || !1 === c.cache || "GET" !== c.method && "JSONP" !== c.method || (O = G(c.cache) ? c.cache : G(a.cache) ? a.cache : N);
                O && (X = O.get(Q), u(X) ? X && E(X.then) ? X.then(J, J) : H(X) ? q(X[1], X[0], qa(X[2]), X[3]) : q(X, 200, {}, "OK") : O.put(Q, z));
                x(X) && ((X = wd(c.url) ? f()[c.xsrfCookieName || a.xsrfCookieName] : void 0) && (T[c.xsrfHeaderName || a.xsrfHeaderName] = X), e(c.method, Q, d, l, T, c.timeout, c.withCredentials, c.responseType, g(c.eventHandlers), g(c.uploadEventHandlers)));
                return z
            }

            function r(a, b) {
                0 < b.length && (a += (-1 === a.indexOf("?") ? "?" : "&") + b);
                return a
            }

            function I(a, b) {
                if (/[&?][^=]+=JSON_CALLBACK/.test(a)) throw xd("badjsonp", a);
                if ((new RegExp("[&?]" + b + "=")).test(a)) throw xd("badjsonp", b, a);
                return a += (-1 === a.indexOf("?") ? "?" : "&") + b + "=JSON_CALLBACK"
            }
            var N = g("$http");
            a.paramSerializer = D(a.paramSerializer) ? l.get(a.paramSerializer) : a.paramSerializer;
            var t = [];
            p(d, function(a) {
                t.unshift(D(a) ? l.get(a) : l.invoke(a))
            });
            n.pendingRequests = [];
            (function(a) {
                p(arguments, function(a) {
                    n[a] =
                        function(b, c) {
                            return n(R({}, c || {}, {
                                method: a,
                                url: b
                            }))
                        }
                })
            })("get", "delete", "head", "jsonp");
            (function(a) {
                p(arguments, function(a) {
                    n[a] = function(b, c, d) {
                        return n(R({}, d || {}, {
                            method: a,
                            url: b,
                            data: c
                        }))
                    }
                })
            })("post", "put", "patch");
            n.defaults = a;
            return n
        }]
    }

    function Hf() {
        this.$get = function() {
            return function() {
                return new w.XMLHttpRequest
            }
        }
    }

    function Gf() {
        this.$get = ["$browser", "$jsonpCallbacks", "$document", "$xhrFactory", function(a, b, d, c) {
            return wg(a, c, a.defer, b, d[0])
        }]
    }

    function wg(a, b, d, c, e) {
        function f(a, b, d) {
            a = a.replace("JSON_CALLBACK",
                b);
            var f = e.createElement("script"),
                m = null;
            f.type = "text/javascript";
            f.src = a;
            f.async = !0;
            m = function(a) {
                f.removeEventListener("load", m);
                f.removeEventListener("error", m);
                e.body.removeChild(f);
                f = null;
                var g = -1,
                    r = "unknown";
                a && ("load" !== a.type || c.wasCalled(b) || (a = {
                    type: "error"
                }), r = a.type, g = "error" === a.type ? 404 : 200);
                d && d(g, r)
            };
            f.addEventListener("load", m);
            f.addEventListener("error", m);
            e.body.appendChild(f);
            return m
        }
        return function(e, h, k, l, m, n, q, r, I, N) {
            function t() {
                y && y();
                v && v.abort()
            }
            h = h || a.url();
            if ("jsonp" ===
                P(e)) var K = c.createCallback(h),
                y = f(h, K, function(a, b) {
                    var e = 200 === a && c.getResponse(K);
                    u(B) && d.cancel(B);
                    y = v = null;
                    l(a, e, "", b);
                    c.removeCallback(K)
                });
            else {
                var v = b(e, h);
                v.open(e, h, !0);
                p(m, function(a, b) {
                    u(a) && v.setRequestHeader(b, a)
                });
                v.onload = function() {
                    var a = v.statusText || "",
                        b = "response" in v ? v.response : v.responseText,
                        c = 1223 === v.status ? 204 : v.status;
                    0 === c && (c = b ? 200 : "file" === Ca(h).protocol ? 404 : 0);
                    var e = v.getAllResponseHeaders();
                    u(B) && d.cancel(B);
                    y = v = null;
                    l(c, b, e, a)
                };
                e = function() {
                    u(B) && d.cancel(B);
                    y = v = null;
                    l(-1, null, null, "")
                };
                v.onerror = e;
                v.onabort = e;
                v.ontimeout = e;
                p(I, function(a, b) {
                    v.addEventListener(b, a)
                });
                p(N, function(a, b) {
                    v.upload.addEventListener(b, a)
                });
                q && (v.withCredentials = !0);
                if (r) try {
                    v.responseType = r
                } catch (s) {
                    if ("json" !== r) throw s;
                }
                v.send(x(k) ? null : k)
            }
            if (0 < n) var B = d(t, n);
            else n && E(n.then) && n.then(t)
        }
    }

    function Bf() {
        var a = "{{",
            b = "}}";
        this.startSymbol = function(b) {
            return b ? (a = b, this) : a
        };
        this.endSymbol = function(a) {
            return a ? (b = a, this) : b
        };
        this.$get = ["$parse", "$exceptionHandler", "$sce", function(d, c,
            e) {
            function f(a) {
                return "\\\\\\" + a
            }

            function g(c) {
                return c.replace(n, a).replace(q, b)
            }

            function h(a, b, c, d) {
                var e = a.$watch(function(a) {
                    e();
                    return d(a)
                }, b, c);
                return e
            }

            function k(f, k, n, q) {
                function K(a) {
                    try {
                        var b = a;
                        a = n ? e.getTrusted(n, b) : e.valueOf(b);
                        return q && !u(a) ? a : $b(a)
                    } catch (d) {
                        c(Da.interr(f, d))
                    }
                }
                if (!f.length || -1 === f.indexOf(a)) {
                    var y;
                    k || (k = g(f), y = la(k), y.exp = f, y.expressions = [], y.$$watchDelegate = h);
                    return y
                }
                q = !!q;
                var v, p, B = 0,
                    J = [],
                    L = [];
                y = f.length;
                for (var C = [], z = []; B < y;)
                    if (-1 !== (v = f.indexOf(a, B)) && -1 !==
                        (p = f.indexOf(b, v + l))) B !== v && C.push(g(f.substring(B, v))), B = f.substring(v + l, p), J.push(B), L.push(d(B, K)), B = p + m, z.push(C.length), C.push("");
                    else {
                        B !== y && C.push(g(f.substring(B)));
                        break
                    } n && 1 < C.length && Da.throwNoconcat(f);
                if (!k || J.length) {
                    var O = function(a) {
                        for (var b = 0, c = J.length; b < c; b++) {
                            if (q && x(a[b])) return;
                            C[z[b]] = a[b]
                        }
                        return C.join("")
                    };
                    return R(function(a) {
                        var b = 0,
                            d = J.length,
                            e = Array(d);
                        try {
                            for (; b < d; b++) e[b] = L[b](a);
                            return O(e)
                        } catch (g) {
                            c(Da.interr(f, g))
                        }
                    }, {
                        exp: f,
                        expressions: J,
                        $$watchDelegate: function(a,
                            b) {
                            var c;
                            return a.$watchGroup(L, function(d, e) {
                                var f = O(d);
                                E(b) && b.call(this, f, d !== e ? c : f, a);
                                c = f
                            })
                        }
                    })
                }
            }
            var l = a.length,
                m = b.length,
                n = new RegExp(a.replace(/./g, f), "g"),
                q = new RegExp(b.replace(/./g, f), "g");
            k.startSymbol = function() {
                return a
            };
            k.endSymbol = function() {
                return b
            };
            return k
        }]
    }

    function Cf() {
        this.$get = ["$rootScope", "$window", "$q", "$$q", "$browser", function(a, b, d, c, e) {
            function f(f, k, l, m) {
                function n() {
                    q ? f.apply(null, r) : f(t)
                }
                var q = 4 < arguments.length,
                    r = q ? va.call(arguments, 4) : [],
                    I = b.setInterval,
                    p = b.clearInterval,
                    t = 0,
                    K = u(m) && !m,
                    y = (K ? c : d).defer(),
                    v = y.promise;
                l = u(l) ? l : 0;
                v.$$intervalId = I(function() {
                    K ? e.defer(n) : a.$evalAsync(n);
                    y.notify(t++);
                    0 < l && t >= l && (y.resolve(t), p(v.$$intervalId), delete g[v.$$intervalId]);
                    K || a.$apply()
                }, k);
                g[v.$$intervalId] = y;
                return v
            }
            var g = {};
            f.cancel = function(a) {
                return a && a.$$intervalId in g ? (g[a.$$intervalId].promise.catch(A), g[a.$$intervalId].reject("canceled"), b.clearInterval(a.$$intervalId), delete g[a.$$intervalId], !0) : !1
            };
            return f
        }]
    }

    function nc(a) {
        a = a.split("/");
        for (var b = a.length; b--;) a[b] =
            db(a[b]);
        return a.join("/")
    }

    function yd(a, b) {
        var d = Ca(a);
        b.$$protocol = d.protocol;
        b.$$host = d.hostname;
        b.$$port = Z(d.port) || xg[d.protocol] || null
    }

    function zd(a, b) {
        if (yg.test(a)) throw lb("badpath", a);
        var d = "/" !== a.charAt(0);
        d && (a = "/" + a);
        var c = Ca(a);
        b.$$path = decodeURIComponent(d && "/" === c.pathname.charAt(0) ? c.pathname.substring(1) : c.pathname);
        b.$$search = Oc(c.search);
        b.$$hash = decodeURIComponent(c.hash);
        b.$$path && "/" !== b.$$path.charAt(0) && (b.$$path = "/" + b.$$path)
    }

    function oc(a, b) {
        return a.slice(0, b.length) ===
            b
    }

    function ka(a, b) {
        if (oc(b, a)) return b.substr(a.length)
    }

    function Aa(a) {
        var b = a.indexOf("#");
        return -1 === b ? a : a.substr(0, b)
    }

    function mb(a) {
        return a.replace(/(#.+)|#$/, "$1")
    }

    function pc(a, b, d) {
        this.$$html5 = !0;
        d = d || "";
        yd(a, this);
        this.$$parse = function(a) {
            var d = ka(b, a);
            if (!D(d)) throw lb("ipthprfx", a, b);
            zd(d, this);
            this.$$path || (this.$$path = "/");
            this.$$compose()
        };
        this.$$compose = function() {
            var a = Zb(this.$$search),
                d = this.$$hash ? "#" + db(this.$$hash) : "";
            this.$$url = nc(this.$$path) + (a ? "?" + a : "") + d;
            this.$$absUrl = b +
                this.$$url.substr(1);
            this.$$urlUpdatedByLocation = !0
        };
        this.$$parseLinkUrl = function(c, e) {
            if (e && "#" === e[0]) return this.hash(e.slice(1)), !0;
            var f, g;
            u(f = ka(a, c)) ? (g = f, g = d && u(f = ka(d, f)) ? b + (ka("/", f) || f) : a + g) : u(f = ka(b, c)) ? g = b + f : b === c + "/" && (g = b);
            g && this.$$parse(g);
            return !!g
        }
    }

    function qc(a, b, d) {
        yd(a, this);
        this.$$parse = function(c) {
            var e = ka(a, c) || ka(b, c),
                f;
            x(e) || "#" !== e.charAt(0) ? this.$$html5 ? f = e : (f = "", x(e) && (a = c, this.replace())) : (f = ka(d, e), x(f) && (f = e));
            zd(f, this);
            c = this.$$path;
            var e = a,
                g = /^\/[A-Z]:(\/.*)/;
            oc(f,
                e) && (f = f.replace(e, ""));
            g.exec(f) || (c = (f = g.exec(c)) ? f[1] : c);
            this.$$path = c;
            this.$$compose()
        };
        this.$$compose = function() {
            var b = Zb(this.$$search),
                e = this.$$hash ? "#" + db(this.$$hash) : "";
            this.$$url = nc(this.$$path) + (b ? "?" + b : "") + e;
            this.$$absUrl = a + (this.$$url ? d + this.$$url : "");
            this.$$urlUpdatedByLocation = !0
        };
        this.$$parseLinkUrl = function(b, d) {
            return Aa(a) === Aa(b) ? (this.$$parse(b), !0) : !1
        }
    }

    function Ad(a, b, d) {
        this.$$html5 = !0;
        qc.apply(this, arguments);
        this.$$parseLinkUrl = function(c, e) {
            if (e && "#" === e[0]) return this.hash(e.slice(1)),
                !0;
            var f, g;
            a === Aa(c) ? f = c : (g = ka(b, c)) ? f = a + d + g : b === c + "/" && (f = b);
            f && this.$$parse(f);
            return !!f
        };
        this.$$compose = function() {
            var b = Zb(this.$$search),
                e = this.$$hash ? "#" + db(this.$$hash) : "";
            this.$$url = nc(this.$$path) + (b ? "?" + b : "") + e;
            this.$$absUrl = a + d + this.$$url;
            this.$$urlUpdatedByLocation = !0
        }
    }

    function Kb(a) {
        return function() {
            return this[a]
        }
    }

    function Bd(a, b) {
        return function(d) {
            if (x(d)) return this[a];
            this[a] = b(d);
            this.$$compose();
            return this
        }
    }

    function Jf() {
        var a = "!",
            b = {
                enabled: !1,
                requireBase: !0,
                rewriteLinks: !0
            };
        this.hashPrefix = function(b) {
            return u(b) ? (a = b, this) : a
        };
        this.html5Mode = function(a) {
            if (Ha(a)) return b.enabled = a, this;
            if (G(a)) {
                Ha(a.enabled) && (b.enabled = a.enabled);
                Ha(a.requireBase) && (b.requireBase = a.requireBase);
                if (Ha(a.rewriteLinks) || D(a.rewriteLinks)) b.rewriteLinks = a.rewriteLinks;
                return this
            }
            return b
        };
        this.$get = ["$rootScope", "$browser", "$sniffer", "$rootElement", "$window", function(d, c, e, f, g) {
            function h(a, b, d) {
                var e = l.url(),
                    f = l.$$state;
                try {
                    c.url(a, b, d), l.$$state = c.state()
                } catch (g) {
                    throw l.url(e), l.$$state =
                        f, g;
                }
            }

            function k(a, b) {
                d.$broadcast("$locationChangeSuccess", l.absUrl(), a, l.$$state, b)
            }
            var l, m;
            m = c.baseHref();
            var n = c.url(),
                q;
            if (b.enabled) {
                if (!m && b.requireBase) throw lb("nobase");
                q = n.substring(0, n.indexOf("/", n.indexOf("//") + 2)) + (m || "/");
                m = e.history ? pc : Ad
            } else q = Aa(n), m = qc;
            var r = q.substr(0, Aa(q).lastIndexOf("/") + 1);
            l = new m(q, r, "#" + a);
            l.$$parseLinkUrl(n, n);
            l.$$state = c.state();
            var I = /^\s*(javascript|mailto):/i;
            f.on("click", function(a) {
                var e = b.rewriteLinks;
                if (e && !a.ctrlKey && !a.metaKey && !a.shiftKey &&
                    2 !== a.which && 2 !== a.button) {
                    for (var h = F(a.target);
                        "a" !== wa(h[0]);)
                        if (h[0] === f[0] || !(h = h.parent())[0]) return;
                    if (!D(e) || !x(h.attr(e))) {
                        var e = h.prop("href"),
                            k = h.attr("href") || h.attr("xlink:href");
                        G(e) && "[object SVGAnimatedString]" === e.toString() && (e = Ca(e.animVal).href);
                        I.test(e) || !e || h.attr("target") || a.isDefaultPrevented() || !l.$$parseLinkUrl(e, k) || (a.preventDefault(), l.absUrl() !== c.url() && (d.$apply(), g.angular["ff-684208-preventDefault"] = !0))
                    }
                }
            });
            mb(l.absUrl()) !== mb(n) && c.url(l.absUrl(), !0);
            var p = !0;
            c.onUrlChange(function(a, b) {
                oc(a, r) ? (d.$evalAsync(function() {
                    var c = l.absUrl(),
                        e = l.$$state,
                        f;
                    a = mb(a);
                    l.$$parse(a);
                    l.$$state = b;
                    f = d.$broadcast("$locationChangeStart", a, c, b, e).defaultPrevented;
                    l.absUrl() === a && (f ? (l.$$parse(c), l.$$state = e, h(c, !1, e)) : (p = !1, k(c, e)))
                }), d.$$phase || d.$digest()) : g.location.href = a
            });
            d.$watch(function() {
                if (p || l.$$urlUpdatedByLocation) {
                    l.$$urlUpdatedByLocation = !1;
                    var a = mb(c.url()),
                        b = mb(l.absUrl()),
                        f = c.state(),
                        g = l.$$replace,
                        m = a !== b || l.$$html5 && e.history && f !== l.$$state;
                    if (p || m) p = !1, d.$evalAsync(function() {
                        var b = l.absUrl(),
                            c = d.$broadcast("$locationChangeStart", b, a, l.$$state, f).defaultPrevented;
                        l.absUrl() === b && (c ? (l.$$parse(a), l.$$state = f) : (m && h(b, g, f === l.$$state ? null : l.$$state), k(a, f)))
                    })
                }
                l.$$replace = !1
            });
            return l
        }]
    }

    function Kf() {
        var a = !0,
            b = this;
        this.debugEnabled = function(b) {
            return u(b) ? (a = b, this) : a
        };
        this.$get = ["$window", function(d) {
            function c(a) {
                a instanceof Error && (a.stack && f ? a = a.message && -1 === a.stack.indexOf(a.message) ? "Error: " + a.message + "\n" + a.stack : a.stack : a.sourceURL &&
                    (a = a.message + "\n" + a.sourceURL + ":" + a.line));
                return a
            }

            function e(a) {
                var b = d.console || {},
                    e = b[a] || b.log || A;
                a = !1;
                try {
                    a = !!e.apply
                } catch (f) {}
                return a ? function() {
                    var a = [];
                    p(arguments, function(b) {
                        a.push(c(b))
                    });
                    return e.apply(b, a)
                } : function(a, b) {
                    e(a, null == b ? "" : b)
                }
            }
            var f = za || /\bEdge\//.test(d.navigator && d.navigator.userAgent);
            return {
                log: e("log"),
                info: e("info"),
                warn: e("warn"),
                error: e("error"),
                debug: function() {
                    var c = e("debug");
                    return function() {
                        a && c.apply(b, arguments)
                    }
                }()
            }
        }]
    }

    function zg(a) {
        return a + ""
    }

    function Ag(a,
        b) {
        return "undefined" !== typeof a ? a : b
    }

    function Cd(a, b) {
        return "undefined" === typeof a ? b : "undefined" === typeof b ? a : a + b
    }

    function U(a, b) {
        var d, c, e;
        switch (a.type) {
            case s.Program:
                d = !0;
                p(a.body, function(a) {
                    U(a.expression, b);
                    d = d && a.expression.constant
                });
                a.constant = d;
                break;
            case s.Literal:
                a.constant = !0;
                a.toWatch = [];
                break;
            case s.UnaryExpression:
                U(a.argument, b);
                a.constant = a.argument.constant;
                a.toWatch = a.argument.toWatch;
                break;
            case s.BinaryExpression:
                U(a.left, b);
                U(a.right, b);
                a.constant = a.left.constant && a.right.constant;
                a.toWatch = a.left.toWatch.concat(a.right.toWatch);
                break;
            case s.LogicalExpression:
                U(a.left, b);
                U(a.right, b);
                a.constant = a.left.constant && a.right.constant;
                a.toWatch = a.constant ? [] : [a];
                break;
            case s.ConditionalExpression:
                U(a.test, b);
                U(a.alternate, b);
                U(a.consequent, b);
                a.constant = a.test.constant && a.alternate.constant && a.consequent.constant;
                a.toWatch = a.constant ? [] : [a];
                break;
            case s.Identifier:
                a.constant = !1;
                a.toWatch = [a];
                break;
            case s.MemberExpression:
                U(a.object, b);
                a.computed && U(a.property, b);
                a.constant = a.object.constant &&
                    (!a.computed || a.property.constant);
                a.toWatch = [a];
                break;
            case s.CallExpression:
                d = e = a.filter ? !b(a.callee.name).$stateful : !1;
                c = [];
                p(a.arguments, function(a) {
                    U(a, b);
                    d = d && a.constant;
                    a.constant || c.push.apply(c, a.toWatch)
                });
                a.constant = d;
                a.toWatch = e ? c : [a];
                break;
            case s.AssignmentExpression:
                U(a.left, b);
                U(a.right, b);
                a.constant = a.left.constant && a.right.constant;
                a.toWatch = [a];
                break;
            case s.ArrayExpression:
                d = !0;
                c = [];
                p(a.elements, function(a) {
                    U(a, b);
                    d = d && a.constant;
                    a.constant || c.push.apply(c, a.toWatch)
                });
                a.constant =
                    d;
                a.toWatch = c;
                break;
            case s.ObjectExpression:
                d = !0;
                c = [];
                p(a.properties, function(a) {
                    U(a.value, b);
                    d = d && a.value.constant && !a.computed;
                    a.value.constant || c.push.apply(c, a.value.toWatch);
                    a.computed && (U(a.key, b), a.key.constant || c.push.apply(c, a.key.toWatch))
                });
                a.constant = d;
                a.toWatch = c;
                break;
            case s.ThisExpression:
                a.constant = !1;
                a.toWatch = [];
                break;
            case s.LocalsExpression:
                a.constant = !1, a.toWatch = []
        }
    }

    function Dd(a) {
        if (1 === a.length) {
            a = a[0].expression;
            var b = a.toWatch;
            return 1 !== b.length ? b : b[0] !== a ? b : void 0
        }
    }

    function Ed(a) {
        return a.type ===
            s.Identifier || a.type === s.MemberExpression
    }

    function Fd(a) {
        if (1 === a.body.length && Ed(a.body[0].expression)) return {
            type: s.AssignmentExpression,
            left: a.body[0].expression,
            right: {
                type: s.NGValueParameter
            },
            operator: "="
        }
    }

    function Gd(a) {
        return 0 === a.body.length || 1 === a.body.length && (a.body[0].expression.type === s.Literal || a.body[0].expression.type === s.ArrayExpression || a.body[0].expression.type === s.ObjectExpression)
    }

    function Hd(a, b) {
        this.astBuilder = a;
        this.$filter = b
    }

    function Id(a, b) {
        this.astBuilder = a;
        this.$filter =
            b
    }

    function rc(a) {
        return E(a.valueOf) ? a.valueOf() : Bg.call(a)
    }

    function Lf() {
        var a = V(),
            b = {
                "true": !0,
                "false": !1,
                "null": null,
                undefined: void 0
            },
            d, c;
        this.addLiteral = function(a, c) {
            b[a] = c
        };
        this.setIdentifierFns = function(a, b) {
            d = a;
            c = b;
            return this
        };
        this.$get = ["$filter", function(e) {
            function f(a, b, c) {
                return null == a || null == b ? a === b : "object" !== typeof a || c || (a = rc(a), "object" !== typeof a) ? a === b || a !== a && b !== b : !1
            }

            function g(a, b, c, d, e) {
                var g = d.inputs,
                    h;
                if (1 === g.length) {
                    var k = f,
                        g = g[0];
                    return a.$watch(function(a) {
                        var b = g(a);
                        f(b, k, d.literal) || (h = d(a, void 0, void 0, [b]), k = b && rc(b));
                        return h
                    }, b, c, e)
                }
                for (var l = [], m = [], n = 0, L = g.length; n < L; n++) l[n] = f, m[n] = null;
                return a.$watch(function(a) {
                    for (var b = !1, c = 0, e = g.length; c < e; c++) {
                        var k = g[c](a);
                        if (b || (b = !f(k, l[c], d.literal))) m[c] = k, l[c] = k && rc(k)
                    }
                    b && (h = d(a, void 0, void 0, m));
                    return h
                }, b, c, e)
            }

            function h(a, b, c, d, e) {
                function f(a) {
                    return d(a)
                }

                function h(a, c, d) {
                    l = a;
                    E(b) && b(a, c, d);
                    u(a) && d.$$postDigest(function() {
                        u(l) && k()
                    })
                }
                var k, l;
                return k = d.inputs ? g(a, h, c, d, e) : a.$watch(f, h, c)
            }

            function k(a,
                b, c, d) {
                function e(a) {
                    var b = !0;
                    p(a, function(a) {
                        u(a) || (b = !1)
                    });
                    return b
                }
                var f, g;
                return f = a.$watch(function(a) {
                    return d(a)
                }, function(a, c, d) {
                    g = a;
                    E(b) && b(a, c, d);
                    e(a) && d.$$postDigest(function() {
                        e(g) && f()
                    })
                }, c)
            }

            function l(a, b, c, d) {
                var e = a.$watch(function(a) {
                    e();
                    return d(a)
                }, b, c);
                return e
            }

            function m(a, b) {
                if (!b) return a;
                var c = a.$$watchDelegate,
                    d = !1,
                    c = c !== k && c !== h ? function(c, e, f, g) {
                        f = d && g ? g[0] : a(c, e, f, g);
                        return b(f, c, e)
                    } : function(c, d, e, f) {
                        e = a(c, d, e, f);
                        c = b(e, c, d);
                        return u(e) ? c : e
                    },
                    d = !a.inputs;
                a.$$watchDelegate &&
                    a.$$watchDelegate !== g ? (c.$$watchDelegate = a.$$watchDelegate, c.inputs = a.inputs) : b.$stateful || (c.$$watchDelegate = g, c.inputs = a.inputs ? a.inputs : [a]);
                return c
            }
            var n = {
                csp: Ga().noUnsafeEval,
                literals: sa(b),
                isIdentifierStart: E(d) && d,
                isIdentifierContinue: E(c) && c
            };
            return function(b, c) {
                var d, f, p;
                switch (typeof b) {
                    case "string":
                        return p = b = b.trim(), d = a[p], d || (":" === b.charAt(0) && ":" === b.charAt(1) && (f = !0, b = b.substring(2)), d = new sc(n), d = (new tc(d, e, n)).parse(b), d.constant ? d.$$watchDelegate = l : f ? d.$$watchDelegate = d.literal ?
                            k : h : d.inputs && (d.$$watchDelegate = g), a[p] = d), m(d, c);
                    case "function":
                        return m(b, c);
                    default:
                        return m(A, c)
                }
            }
        }]
    }

    function Nf() {
        var a = !0;
        this.$get = ["$rootScope", "$exceptionHandler", function(b, d) {
            return Jd(function(a) {
                b.$evalAsync(a)
            }, d, a)
        }];
        this.errorOnUnhandledRejections = function(b) {
            return u(b) ? (a = b, this) : a
        }
    }

    function Of() {
        var a = !0;
        this.$get = ["$browser", "$exceptionHandler", function(b, d) {
            return Jd(function(a) {
                b.defer(a)
            }, d, a)
        }];
        this.errorOnUnhandledRejections = function(b) {
            return u(b) ? (a = b, this) : a
        }
    }

    function Jd(a,
        b, d) {
        function c() {
            return new e
        }

        function e() {
            var a = this.promise = new f;
            this.resolve = function(b) {
                k(a, b)
            };
            this.reject = function(b) {
                m(a, b)
            };
            this.notify = function(b) {
                q(a, b)
            }
        }

        function f() {
            this.$$state = {
                status: 0
            }
        }

        function g() {
            for (; !y && v.length;) {
                var a = v.shift();
                if (!a.pur) {
                    a.pur = !0;
                    var c = a.value,
                        c = "Possibly unhandled rejection: " + ("function" === typeof c ? c.toString().replace(/ \{[\s\S]*$/, "") : x(c) ? "undefined" : "string" !== typeof c ? Be(c, void 0) : c);
                    a.value instanceof Error ? b(a.value, c) : b(c)
                }
            }
        }

        function h(b) {
            !d || b.pending ||
                2 !== b.status || b.pur || (0 === y && 0 === v.length && a(g), v.push(b));
            !b.processScheduled && b.pending && (b.processScheduled = !0, ++y, a(function() {
                var c, e, f;
                f = b.pending;
                b.processScheduled = !1;
                b.pending = void 0;
                try {
                    for (var h = 0, l = f.length; h < l; ++h) {
                        b.pur = !0;
                        e = f[h][0];
                        c = f[h][b.status];
                        try {
                            E(c) ? k(e, c(b.value)) : 1 === b.status ? k(e, b.value) : m(e, b.value)
                        } catch (n) {
                            m(e, n)
                        }
                    }
                } finally {
                    --y, d && 0 === y && a(g)
                }
            }))
        }

        function k(a, b) {
            a.$$state.status || (b === a ? n(a, K("qcycle", b)) : l(a, b))
        }

        function l(a, b) {
            function c(b) {
                g || (g = !0, l(a, b))
            }

            function d(b) {
                g ||
                    (g = !0, n(a, b))
            }

            function e(b) {
                q(a, b)
            }
            var f, g = !1;
            try {
                if (G(b) || E(b)) f = b.then;
                E(f) ? (a.$$state.status = -1, f.call(b, c, d, e)) : (a.$$state.value = b, a.$$state.status = 1, h(a.$$state))
            } catch (k) {
                d(k)
            }
        }

        function m(a, b) {
            a.$$state.status || n(a, b)
        }

        function n(a, b) {
            a.$$state.value = b;
            a.$$state.status = 2;
            h(a.$$state)
        }

        function q(c, d) {
            var e = c.$$state.pending;
            0 >= c.$$state.status && e && e.length && a(function() {
                for (var a, c, f = 0, g = e.length; f < g; f++) {
                    c = e[f][0];
                    a = e[f][3];
                    try {
                        q(c, E(a) ? a(d) : d)
                    } catch (h) {
                        b(h)
                    }
                }
            })
        }

        function r(a) {
            var b = new f;
            m(b,
                a);
            return b
        }

        function I(a, b, c) {
            var d = null;
            try {
                E(c) && (d = c())
            } catch (e) {
                return r(e)
            }
            return d && E(d.then) ? d.then(function() {
                return b(a)
            }, r) : b(a)
        }

        function s(a, b, c, d) {
            var e = new f;
            k(e, a);
            return e.then(b, c, d)
        }

        function t(a) {
            if (!E(a)) throw K("norslvr", a);
            var b = new f;
            a(function(a) {
                k(b, a)
            }, function(a) {
                m(b, a)
            });
            return b
        }
        var K = M("$q", TypeError),
            y = 0,
            v = [];
        R(f.prototype, {
            then: function(a, b, c) {
                if (x(a) && x(b) && x(c)) return this;
                var d = new f;
                this.$$state.pending = this.$$state.pending || [];
                this.$$state.pending.push([d, a, b, c]);
                0 < this.$$state.status && h(this.$$state);
                return d
            },
            "catch": function(a) {
                return this.then(null, a)
            },
            "finally": function(a, b) {
                return this.then(function(b) {
                    return I(b, u, a)
                }, function(b) {
                    return I(b, r, a)
                }, b)
            }
        });
        var u = s;
        t.prototype = f.prototype;
        t.defer = c;
        t.reject = r;
        t.when = s;
        t.resolve = u;
        t.all = function(a) {
            var b = new f,
                c = 0,
                d = H(a) ? [] : {};
            p(a, function(a, e) {
                c++;
                s(a).then(function(a) {
                    d[e] = a;
                    --c || k(b, d)
                }, function(a) {
                    m(b, a)
                })
            });
            0 === c && k(b, d);
            return b
        };
        t.race = function(a) {
            var b = c();
            p(a, function(a) {
                s(a).then(b.resolve, b.reject)
            });
            return b.promise
        };
        return t
    }

    function Xf() {
        this.$get = ["$window", "$timeout", function(a, b) {
            var d = a.requestAnimationFrame || a.webkitRequestAnimationFrame,
                c = a.cancelAnimationFrame || a.webkitCancelAnimationFrame || a.webkitCancelRequestAnimationFrame,
                e = !!d,
                f = e ? function(a) {
                    var b = d(a);
                    return function() {
                        c(b)
                    }
                } : function(a) {
                    var c = b(a, 16.66, !1);
                    return function() {
                        b.cancel(c)
                    }
                };
            f.supported = e;
            return f
        }]
    }

    function Mf() {
        function a(a) {
            function b() {
                this.$$watchers = this.$$nextSibling = this.$$childHead = this.$$childTail = null;
                this.$$listeners = {};
                this.$$listenerCount = {};
                this.$$watchersCount = 0;
                this.$id = ++rb;
                this.$$ChildScope = null
            }
            b.prototype = a;
            return b
        }
        var b = 10,
            d = M("$rootScope"),
            c = null,
            e = null;
        this.digestTtl = function(a) {
            arguments.length && (b = a);
            return b
        };
        this.$get = ["$exceptionHandler", "$parse", "$browser", function(f, g, h) {
            function k(a) {
                a.currentScope.$$destroyed = !0
            }

            function l(a) {
                9 === za && (a.$$childHead && l(a.$$childHead), a.$$nextSibling && l(a.$$nextSibling));
                a.$parent = a.$$nextSibling = a.$$prevSibling = a.$$childHead = a.$$childTail =
                    a.$root = a.$$watchers = null
            }

            function m() {
                this.$id = ++rb;
                this.$$phase = this.$parent = this.$$watchers = this.$$nextSibling = this.$$prevSibling = this.$$childHead = this.$$childTail = null;
                this.$root = this;
                this.$$destroyed = !1;
                this.$$listeners = {};
                this.$$listenerCount = {};
                this.$$watchersCount = 0;
                this.$$isolateBindings = null
            }

            function n(a) {
                if (K.$$phase) throw d("inprog", K.$$phase);
                K.$$phase = a
            }

            function q(a, b) {
                do a.$$watchersCount += b; while (a = a.$parent)
            }

            function r(a, b, c) {
                do a.$$listenerCount[c] -= b, 0 === a.$$listenerCount[c] && delete a.$$listenerCount[c];
                while (a = a.$parent)
            }

            function I() {}

            function s() {
                for (; u.length;) try {
                    u.shift()()
                } catch (a) {
                    f(a)
                }
                e = null
            }

            function t() {
                null === e && (e = h.defer(function() {
                    K.$apply(s)
                }))
            }
            m.prototype = {
                constructor: m,
                $new: function(b, c) {
                    var d;
                    c = c || this;
                    b ? (d = new m, d.$root = this.$root) : (this.$$ChildScope || (this.$$ChildScope = a(this)), d = new this.$$ChildScope);
                    d.$parent = c;
                    d.$$prevSibling = c.$$childTail;
                    c.$$childHead ? (c.$$childTail.$$nextSibling = d, c.$$childTail = d) : c.$$childHead = c.$$childTail = d;
                    (b || c !== this) && d.$on("$destroy", k);
                    return d
                },
                $watch: function(a, b, d, e) {
                    var f = g(a);
                    if (f.$$watchDelegate) return f.$$watchDelegate(this, b, d, f, a);
                    var h = this,
                        k = h.$$watchers,
                        l = {
                            fn: b,
                            last: I,
                            get: f,
                            exp: e || a,
                            eq: !!d
                        };
                    c = null;
                    E(b) || (l.fn = A);
                    k || (k = h.$$watchers = [], k.$$digestWatchIndex = -1);
                    k.unshift(l);
                    k.$$digestWatchIndex++;
                    q(this, 1);
                    return function() {
                        var a = $a(k, l);
                        0 <= a && (q(h, -1), a < k.$$digestWatchIndex && k.$$digestWatchIndex--);
                        c = null
                    }
                },
                $watchGroup: function(a, b) {
                    function c() {
                        h = !1;
                        k ? (k = !1, b(e, e, g)) : b(e, d, g)
                    }
                    var d = Array(a.length),
                        e = Array(a.length),
                        f = [],
                        g = this,
                        h = !1,
                        k = !0;
                    if (!a.length) {
                        var l = !0;
                        g.$evalAsync(function() {
                            l && b(e, e, g)
                        });
                        return function() {
                            l = !1
                        }
                    }
                    if (1 === a.length) return this.$watch(a[0], function(a, c, f) {
                        e[0] = a;
                        d[0] = c;
                        b(e, a === c ? e : d, f)
                    });
                    p(a, function(a, b) {
                        var k = g.$watch(a, function(a, f) {
                            e[b] = a;
                            d[b] = f;
                            h || (h = !0, g.$evalAsync(c))
                        });
                        f.push(k)
                    });
                    return function() {
                        for (; f.length;) f.shift()()
                    }
                },
                $watchCollection: function(a, b) {
                    function c(a) {
                        e = a;
                        var b, d, g, h;
                        if (!x(e)) {
                            if (G(e))
                                if (ra(e))
                                    for (f !== n && (f = n, p = f.length = 0, l++), a = e.length, p !== a && (l++, f.length = p = a), b = 0; b < a; b++) h =
                                        f[b], g = e[b], d = h !== h && g !== g, d || h === g || (l++, f[b] = g);
                                else {
                                    f !== q && (f = q = {}, p = 0, l++);
                                    a = 0;
                                    for (b in e) ua.call(e, b) && (a++, g = e[b], h = f[b], b in f ? (d = h !== h && g !== g, d || h === g || (l++, f[b] = g)) : (p++, f[b] = g, l++));
                                    if (p > a)
                                        for (b in l++, f) ua.call(e, b) || (p--, delete f[b])
                                }
                            else f !== e && (f = e, l++);
                            return l
                        }
                    }
                    c.$stateful = !0;
                    var d = this,
                        e, f, h, k = 1 < b.length,
                        l = 0,
                        m = g(a, c),
                        n = [],
                        q = {},
                        r = !0,
                        p = 0;
                    return this.$watch(m, function() {
                        r ? (r = !1, b(e, e, d)) : b(e, h, d);
                        if (k)
                            if (G(e))
                                if (ra(e)) {
                                    h = Array(e.length);
                                    for (var a = 0; a < e.length; a++) h[a] = e[a]
                                } else
                                    for (a in h = {}, e) ua.call(e, a) && (h[a] = e[a]);
                        else h = e
                    })
                },
                $digest: function() {
                    var a, g, k, l, m, q, r, p = b,
                        t, u = [],
                        x, w;
                    n("$digest");
                    h.$$checkUrlChange();
                    this === K && null !== e && (h.defer.cancel(e), s());
                    c = null;
                    do {
                        r = !1;
                        t = this;
                        for (q = 0; q < y.length; q++) {
                            try {
                                w = y[q], l = w.fn, l(w.scope, w.locals)
                            } catch (A) {
                                f(A)
                            }
                            c = null
                        }
                        y.length = 0;
                        a: do {
                            if (q = t.$$watchers)
                                for (q.$$digestWatchIndex = q.length; q.$$digestWatchIndex--;) try {
                                    if (a = q[q.$$digestWatchIndex])
                                        if (m = a.get, (g = m(t)) !== (k = a.last) && !(a.eq ? pa(g, k) : da(g) && da(k))) r = !0, c = a, a.last = a.eq ? sa(g, null) : g, l =
                                            a.fn, l(g, k === I ? g : k, t), 5 > p && (x = 4 - p, u[x] || (u[x] = []), u[x].push({
                                                msg: E(a.exp) ? "fn: " + (a.exp.name || a.exp.toString()) : a.exp,
                                                newVal: g,
                                                oldVal: k
                                            }));
                                        else if (a === c) {
                                        r = !1;
                                        break a
                                    }
                                } catch (F) {
                                    f(F)
                                }
                            if (!(q = t.$$watchersCount && t.$$childHead || t !== this && t.$$nextSibling))
                                for (; t !== this && !(q = t.$$nextSibling);) t = t.$parent
                        } while (t = q);
                        if ((r || y.length) && !p--) throw K.$$phase = null, d("infdig", b, u);
                    } while (r || y.length);
                    for (K.$$phase = null; B < v.length;) try {
                        v[B++]()
                    } catch (La) {
                        f(La)
                    }
                    v.length = B = 0;
                    h.$$checkUrlChange()
                },
                $destroy: function() {
                    if (!this.$$destroyed) {
                        var a =
                            this.$parent;
                        this.$broadcast("$destroy");
                        this.$$destroyed = !0;
                        this === K && h.$$applicationDestroyed();
                        q(this, -this.$$watchersCount);
                        for (var b in this.$$listenerCount) r(this, this.$$listenerCount[b], b);
                        a && a.$$childHead === this && (a.$$childHead = this.$$nextSibling);
                        a && a.$$childTail === this && (a.$$childTail = this.$$prevSibling);
                        this.$$prevSibling && (this.$$prevSibling.$$nextSibling = this.$$nextSibling);
                        this.$$nextSibling && (this.$$nextSibling.$$prevSibling = this.$$prevSibling);
                        this.$destroy = this.$digest = this.$apply =
                            this.$evalAsync = this.$applyAsync = A;
                        this.$on = this.$watch = this.$watchGroup = function() {
                            return A
                        };
                        this.$$listeners = {};
                        this.$$nextSibling = null;
                        l(this)
                    }
                },
                $eval: function(a, b) {
                    return g(a)(this, b)
                },
                $evalAsync: function(a, b) {
                    K.$$phase || y.length || h.defer(function() {
                        y.length && K.$digest()
                    });
                    y.push({
                        scope: this,
                        fn: g(a),
                        locals: b
                    })
                },
                $$postDigest: function(a) {
                    v.push(a)
                },
                $apply: function(a) {
                    try {
                        n("$apply");
                        try {
                            return this.$eval(a)
                        } finally {
                            K.$$phase = null
                        }
                    } catch (b) {
                        f(b)
                    } finally {
                        try {
                            K.$digest()
                        } catch (c) {
                            throw f(c), c;
                        }
                    }
                },
                $applyAsync: function(a) {
                    function b() {
                        c.$eval(a)
                    }
                    var c = this;
                    a && u.push(b);
                    a = g(a);
                    t()
                },
                $on: function(a, b) {
                    var c = this.$$listeners[a];
                    c || (this.$$listeners[a] = c = []);
                    c.push(b);
                    var d = this;
                    do d.$$listenerCount[a] || (d.$$listenerCount[a] = 0), d.$$listenerCount[a]++; while (d = d.$parent);
                    var e = this;
                    return function() {
                        var d = c.indexOf(b); - 1 !== d && (c[d] = null, r(e, 1, a))
                    }
                },
                $emit: function(a, b) {
                    var c = [],
                        d, e = this,
                        g = !1,
                        h = {
                            name: a,
                            targetScope: e,
                            stopPropagation: function() {
                                g = !0
                            },
                            preventDefault: function() {
                                h.defaultPrevented = !0
                            },
                            defaultPrevented: !1
                        },
                        k = ab([h], arguments, 1),
                        l, m;
                    do {
                        d = e.$$listeners[a] || c;
                        h.currentScope = e;
                        l = 0;
                        for (m = d.length; l < m; l++)
                            if (d[l]) try {
                                d[l].apply(null, k)
                            } catch (n) {
                                f(n)
                            } else d.splice(l, 1), l--, m--;
                        if (g) return h.currentScope = null, h;
                        e = e.$parent
                    } while (e);
                    h.currentScope = null;
                    return h
                },
                $broadcast: function(a, b) {
                    var c = this,
                        d = this,
                        e = {
                            name: a,
                            targetScope: this,
                            preventDefault: function() {
                                e.defaultPrevented = !0
                            },
                            defaultPrevented: !1
                        };
                    if (!this.$$listenerCount[a]) return e;
                    for (var g = ab([e], arguments, 1), h, k; c = d;) {
                        e.currentScope = c;
                        d = c.$$listeners[a] || [];
                        h = 0;
                        for (k = d.length; h < k; h++)
                            if (d[h]) try {
                                d[h].apply(null, g)
                            } catch (l) {
                                f(l)
                            } else d.splice(h, 1), h--, k--;
                        if (!(d = c.$$listenerCount[a] && c.$$childHead || c !== this && c.$$nextSibling))
                            for (; c !== this && !(d = c.$$nextSibling);) c = c.$parent
                    }
                    e.currentScope = null;
                    return e
                }
            };
            var K = new m,
                y = K.$$asyncQueue = [],
                v = K.$$postDigestQueue = [],
                u = K.$$applyAsyncQueue = [],
                B = 0;
            return K
        }]
    }

    function Ee() {
        var a = /^\s*(https?|ftp|mailto|tel|file):/,
            b = /^\s*((https?|ftp|file|blob):|data:image\/)/;
        this.aHrefSanitizationWhitelist = function(b) {
            return u(b) ?
                (a = b, this) : a
        };
        this.imgSrcSanitizationWhitelist = function(a) {
            return u(a) ? (b = a, this) : b
        };
        this.$get = function() {
            return function(d, c) {
                var e = c ? b : a,
                    f;
                f = Ca(d).href;
                return "" === f || f.match(e) ? d : "unsafe:" + f
            }
        }
    }

    function Cg(a) {
        if ("self" === a) return a;
        if (D(a)) {
            if (-1 < a.indexOf("***")) throw ta("iwcard", a);
            a = Kd(a).replace(/\\\*\\\*/g, ".*").replace(/\\\*/g, "[^:/.?&;]*");
            return new RegExp("^" + a + "$")
        }
        if (Xa(a)) return new RegExp("^" + a.source + "$");
        throw ta("imatcher");
    }

    function Ld(a) {
        var b = [];
        u(a) && p(a, function(a) {
            b.push(Cg(a))
        });
        return b
    }

    function Qf() {
        this.SCE_CONTEXTS = oa;
        var a = ["self"],
            b = [];
        this.resourceUrlWhitelist = function(b) {
            arguments.length && (a = Ld(b));
            return a
        };
        this.resourceUrlBlacklist = function(a) {
            arguments.length && (b = Ld(a));
            return b
        };
        this.$get = ["$injector", function(d) {
            function c(a, b) {
                return "self" === a ? wd(b) : !!a.exec(b.href)
            }

            function e(a) {
                var b = function(a) {
                    this.$$unwrapTrustedValue = function() {
                        return a
                    }
                };
                a && (b.prototype = new a);
                b.prototype.valueOf = function() {
                    return this.$$unwrapTrustedValue()
                };
                b.prototype.toString = function() {
                    return this.$$unwrapTrustedValue().toString()
                };
                return b
            }
            var f = function(a) {
                throw ta("unsafe");
            };
            d.has("$sanitize") && (f = d.get("$sanitize"));
            var g = e(),
                h = {};
            h[oa.HTML] = e(g);
            h[oa.CSS] = e(g);
            h[oa.URL] = e(g);
            h[oa.JS] = e(g);
            h[oa.RESOURCE_URL] = e(h[oa.URL]);
            return {
                trustAs: function(a, b) {
                    var c = h.hasOwnProperty(a) ? h[a] : null;
                    if (!c) throw ta("icontext", a, b);
                    if (null === b || x(b) || "" === b) return b;
                    if ("string" !== typeof b) throw ta("itype", a);
                    return new c(b)
                },
                getTrusted: function(d, e) {
                    if (null === e || x(e) || "" === e) return e;
                    var g = h.hasOwnProperty(d) ? h[d] : null;
                    if (g && e instanceof g) return e.$$unwrapTrustedValue();
                    if (d === oa.RESOURCE_URL) {
                        var g = Ca(e.toString()),
                            n, q, r = !1;
                        n = 0;
                        for (q = a.length; n < q; n++)
                            if (c(a[n], g)) {
                                r = !0;
                                break
                            } if (r)
                            for (n = 0, q = b.length; n < q; n++)
                                if (c(b[n], g)) {
                                    r = !1;
                                    break
                                } if (r) return e;
                        throw ta("insecurl", e.toString());
                    }
                    if (d === oa.HTML) return f(e);
                    throw ta("unsafe");
                },
                valueOf: function(a) {
                    return a instanceof g ? a.$$unwrapTrustedValue() : a
                }
            }
        }]
    }

    function Pf() {
        var a = !0;
        this.enabled = function(b) {
            arguments.length && (a = !!b);
            return a
        };
        this.$get = ["$parse", "$sceDelegate", function(b, d) {
            if (a &&
                8 > za) throw ta("iequirks");
            var c = qa(oa);
            c.isEnabled = function() {
                return a
            };
            c.trustAs = d.trustAs;
            c.getTrusted = d.getTrusted;
            c.valueOf = d.valueOf;
            a || (c.trustAs = c.getTrusted = function(a, b) {
                return b
            }, c.valueOf = Ya);
            c.parseAs = function(a, d) {
                var e = b(d);
                return e.literal && e.constant ? e : b(d, function(b) {
                    return c.getTrusted(a, b)
                })
            };
            var e = c.parseAs,
                f = c.getTrusted,
                g = c.trustAs;
            p(oa, function(a, b) {
                var d = P(b);
                c[("parse_as_" + d).replace(uc, gb)] = function(b) {
                    return e(a, b)
                };
                c[("get_trusted_" + d).replace(uc, gb)] = function(b) {
                    return f(a,
                        b)
                };
                c[("trust_as_" + d).replace(uc, gb)] = function(b) {
                    return g(a, b)
                }
            });
            return c
        }]
    }

    function Rf() {
        this.$get = ["$window", "$document", function(a, b) {
            var d = {},
                c = !((!a.nw || !a.nw.process) && a.chrome && (a.chrome.app && a.chrome.app.runtime || !a.chrome.app && a.chrome.runtime && a.chrome.runtime.id)) && a.history && a.history.pushState,
                e = Z((/android (\d+)/.exec(P((a.navigator || {}).userAgent)) || [])[1]),
                f = /Boxee/i.test((a.navigator || {}).userAgent),
                g = b[0] || {},
                h = g.body && g.body.style,
                k = !1,
                l = !1;
            h && (k = !!("transition" in h || "webkitTransition" in
                h), l = !!("animation" in h || "webkitAnimation" in h));
            return {
                history: !(!c || 4 > e || f),
                hasEvent: function(a) {
                    if ("input" === a && za) return !1;
                    if (x(d[a])) {
                        var b = g.createElement("div");
                        d[a] = "on" + a in b
                    }
                    return d[a]
                },
                csp: Ga(),
                transitions: k,
                animations: l,
                android: e
            }
        }]
    }

    function Tf() {
        var a;
        this.httpOptions = function(b) {
            return b ? (a = b, this) : a
        };
        this.$get = ["$exceptionHandler", "$templateCache", "$http", "$q", "$sce", function(b, d, c, e, f) {
            function g(h, k) {
                g.totalPendingRequests++;
                if (!D(h) || x(d.get(h))) h = f.getTrustedResourceUrl(h);
                var l =
                    c.defaults && c.defaults.transformResponse;
                H(l) ? l = l.filter(function(a) {
                    return a !== lc
                }) : l === lc && (l = null);
                return c.get(h, R({
                    cache: d,
                    transformResponse: l
                }, a)).finally(function() {
                    g.totalPendingRequests--
                }).then(function(a) {
                    d.put(h, a.data);
                    return a.data
                }, function(a) {
                    k || (a = Dg("tpload", h, a.status, a.statusText), b(a));
                    return e.reject(a)
                })
            }
            g.totalPendingRequests = 0;
            return g
        }]
    }

    function Uf() {
        this.$get = ["$rootScope", "$browser", "$location", function(a, b, d) {
            return {
                findBindings: function(a, b, d) {
                    a = a.getElementsByClassName("ng-binding");
                    var g = [];
                    p(a, function(a) {
                        var c = ea.element(a).data("$binding");
                        c && p(c, function(c) {
                            d ? (new RegExp("(^|\\s)" + Kd(b) + "(\\s|\\||$)")).test(c) && g.push(a) : -1 !== c.indexOf(b) && g.push(a)
                        })
                    });
                    return g
                },
                findModels: function(a, b, d) {
                    for (var g = ["ng-", "data-ng-", "ng\\:"], h = 0; h < g.length; ++h) {
                        var k = a.querySelectorAll("[" + g[h] + "model" + (d ? "=" : "*=") + '"' + b + '"]');
                        if (k.length) return k
                    }
                },
                getLocation: function() {
                    return d.url()
                },
                setLocation: function(b) {
                    b !== d.url() && (d.url(b), a.$digest())
                },
                whenStable: function(a) {
                    b.notifyWhenNoOutstandingRequests(a)
                }
            }
        }]
    }

    function Vf() {
        this.$get = ["$rootScope", "$browser", "$q", "$$q", "$exceptionHandler", function(a, b, d, c, e) {
            function f(f, k, l) {
                E(f) || (l = k, k = f, f = A);
                var m = va.call(arguments, 3),
                    n = u(l) && !l,
                    q = (n ? c : d).defer(),
                    r = q.promise,
                    p;
                p = b.defer(function() {
                    try {
                        q.resolve(f.apply(null, m))
                    } catch (b) {
                        q.reject(b), e(b)
                    } finally {
                        delete g[r.$$timeoutId]
                    }
                    n || a.$apply()
                }, k);
                r.$$timeoutId = p;
                g[p] = q;
                return r
            }
            var g = {};
            f.cancel = function(a) {
                return a && a.$$timeoutId in g ? (g[a.$$timeoutId].promise.catch(A), g[a.$$timeoutId].reject("canceled"), delete g[a.$$timeoutId],
                    b.defer.cancel(a.$$timeoutId)) : !1
            };
            return f
        }]
    }

    function Ca(a) {
        za && (aa.setAttribute("href", a), a = aa.href);
        aa.setAttribute("href", a);
        return {
            href: aa.href,
            protocol: aa.protocol ? aa.protocol.replace(/:$/, "") : "",
            host: aa.host,
            search: aa.search ? aa.search.replace(/^\?/, "") : "",
            hash: aa.hash ? aa.hash.replace(/^#/, "") : "",
            hostname: aa.hostname,
            port: aa.port,
            pathname: "/" === aa.pathname.charAt(0) ? aa.pathname : "/" + aa.pathname
        }
    }

    function wd(a) {
        a = D(a) ? Ca(a) : a;
        return a.protocol === Md.protocol && a.host === Md.host
    }

    function Wf() {
        this.$get =
            la(w)
    }

    function Nd(a) {
        function b(a) {
            try {
                return decodeURIComponent(a)
            } catch (b) {
                return a
            }
        }
        var d = a[0] || {},
            c = {},
            e = "";
        return function() {
            var a, g, h, k, l;
            try {
                a = d.cookie || ""
            } catch (m) {
                a = ""
            }
            if (a !== e)
                for (e = a, a = e.split("; "), c = {}, h = 0; h < a.length; h++) g = a[h], k = g.indexOf("="), 0 < k && (l = b(g.substring(0, k)), x(c[l]) && (c[l] = b(g.substring(k + 1))));
            return c
        }
    }

    function $f() {
        this.$get = Nd
    }

    function $c(a) {
        function b(d, c) {
            if (G(d)) {
                var e = {};
                p(d, function(a, c) {
                    e[c] = b(c, a)
                });
                return e
            }
            return a.factory(d + "Filter", c)
        }
        this.register = b;
        this.$get = ["$injector", function(a) {
            return function(b) {
                return a.get(b + "Filter")
            }
        }];
        b("currency", Od);
        b("date", Pd);
        b("filter", Eg);
        b("json", Fg);
        b("limitTo", Gg);
        b("lowercase", Hg);
        b("number", Qd);
        b("orderBy", Rd);
        b("uppercase", Ig)
    }

    function Eg() {
        return function(a, b, d, c) {
            if (!ra(a)) {
                if (null == a) return a;
                throw M("filter")("notarray", a);
            }
            c = c || "$";
            var e;
            switch (vc(b)) {
                case "function":
                    break;
                case "boolean":
                case "null":
                case "number":
                case "string":
                    e = !0;
                case "object":
                    b = Jg(b, d, c, e);
                    break;
                default:
                    return a
            }
            return Array.prototype.filter.call(a,
                b)
        }
    }

    function Jg(a, b, d, c) {
        var e = G(a) && d in a;
        !0 === b ? b = pa : E(b) || (b = function(a, b) {
            if (x(a)) return !1;
            if (null === a || null === b) return a === b;
            if (G(b) || G(a) && !Xb(a)) return !1;
            a = P("" + a);
            b = P("" + b);
            return -1 !== a.indexOf(b)
        });
        return function(f) {
            return e && !G(f) ? Ea(f, a[d], b, d, !1) : Ea(f, a, b, d, c)
        }
    }

    function Ea(a, b, d, c, e, f) {
        var g = vc(a),
            h = vc(b);
        if ("string" === h && "!" === b.charAt(0)) return !Ea(a, b.substring(1), d, c, e);
        if (H(a)) return a.some(function(a) {
            return Ea(a, b, d, c, e)
        });
        switch (g) {
            case "object":
                var k;
                if (e) {
                    for (k in a)
                        if (k.charAt &&
                            "$" !== k.charAt(0) && Ea(a[k], b, d, c, !0)) return !0;
                    return f ? !1 : Ea(a, b, d, c, !1)
                }
                if ("object" === h) {
                    for (k in b)
                        if (f = b[k], !E(f) && !x(f) && (g = k === c, !Ea(g ? a : a[k], f, d, c, g, g))) return !1;
                    return !0
                }
                return d(a, b);
            case "function":
                return !1;
            default:
                return d(a, b)
        }
    }

    function vc(a) {
        return null === a ? "null" : typeof a
    }

    function Od(a) {
        var b = a.NUMBER_FORMATS;
        return function(a, c, e) {
            x(c) && (c = b.CURRENCY_SYM);
            x(e) && (e = b.PATTERNS[1].maxFrac);
            return null == a ? a : Sd(a, b.PATTERNS[1], b.GROUP_SEP, b.DECIMAL_SEP, e).replace(/\u00A4/g, c)
        }
    }

    function Qd(a) {
        var b =
            a.NUMBER_FORMATS;
        return function(a, c) {
            return null == a ? a : Sd(a, b.PATTERNS[0], b.GROUP_SEP, b.DECIMAL_SEP, c)
        }
    }

    function Kg(a) {
        var b = 0,
            d, c, e, f, g; - 1 < (c = a.indexOf(Td)) && (a = a.replace(Td, ""));
        0 < (e = a.search(/e/i)) ? (0 > c && (c = e), c += +a.slice(e + 1), a = a.substring(0, e)) : 0 > c && (c = a.length);
        for (e = 0; a.charAt(e) === wc; e++);
        if (e === (g = a.length)) d = [0], c = 1;
        else {
            for (g--; a.charAt(g) === wc;) g--;
            c -= e;
            d = [];
            for (f = 0; e <= g; e++, f++) d[f] = +a.charAt(e)
        }
        c > Ud && (d = d.splice(0, Ud - 1), b = c - 1, c = 1);
        return {
            d: d,
            e: b,
            i: c
        }
    }

    function Lg(a, b, d, c) {
        var e = a.d,
            f =
            e.length - a.i;
        b = x(b) ? Math.min(Math.max(d, f), c) : +b;
        d = b + a.i;
        c = e[d];
        if (0 < d) {
            e.splice(Math.max(a.i, d));
            for (var g = d; g < e.length; g++) e[g] = 0
        } else
            for (f = Math.max(0, f), a.i = 1, e.length = Math.max(1, d = b + 1), e[0] = 0, g = 1; g < d; g++) e[g] = 0;
        if (5 <= c)
            if (0 > d - 1) {
                for (c = 0; c > d; c--) e.unshift(0), a.i++;
                e.unshift(1);
                a.i++
            } else e[d - 1]++;
        for (; f < Math.max(0, b); f++) e.push(0);
        if (b = e.reduceRight(function(a, b, c, d) {
                b += a;
                d[c] = b % 10;
                return Math.floor(b / 10)
            }, 0)) e.unshift(b), a.i++
    }

    function Sd(a, b, d, c, e) {
        if (!D(a) && !ba(a) || isNaN(a)) return "";
        var f = !isFinite(a),
            g = !1,
            h = Math.abs(a) + "",
            k = "";
        if (f) k = "∞";
        else {
            g = Kg(h);
            Lg(g, e, b.minFrac, b.maxFrac);
            k = g.d;
            h = g.i;
            e = g.e;
            f = [];
            for (g = k.reduce(function(a, b) {
                    return a && !b
                }, !0); 0 > h;) k.unshift(0), h++;
            0 < h ? f = k.splice(h, k.length) : (f = k, k = [0]);
            h = [];
            for (k.length >= b.lgSize && h.unshift(k.splice(-b.lgSize, k.length).join("")); k.length > b.gSize;) h.unshift(k.splice(-b.gSize, k.length).join(""));
            k.length && h.unshift(k.join(""));
            k = h.join(d);
            f.length && (k += c + f.join(""));
            e && (k += "e+" + e)
        }
        return 0 > a && !g ? b.negPre + k + b.negSuf : b.posPre +
            k + b.posSuf
    }

    function Lb(a, b, d, c) {
        var e = "";
        if (0 > a || c && 0 >= a) c ? a = -a + 1 : (a = -a, e = "-");
        for (a = "" + a; a.length < b;) a = wc + a;
        d && (a = a.substr(a.length - b));
        return e + a
    }

    function Y(a, b, d, c, e) {
        d = d || 0;
        return function(f) {
            f = f["get" + a]();
            if (0 < d || f > -d) f += d;
            0 === f && -12 === d && (f = 12);
            return Lb(f, b, c, e)
        }
    }

    function nb(a, b, d) {
        return function(c, e) {
            var f = c["get" + a](),
                g = vb((d ? "STANDALONE" : "") + (b ? "SHORT" : "") + a);
            return e[g][f]
        }
    }

    function Vd(a) {
        var b = (new Date(a, 0, 1)).getDay();
        return new Date(a, 0, (4 >= b ? 5 : 12) - b)
    }

    function Wd(a) {
        return function(b) {
            var d =
                Vd(b.getFullYear());
            b = +new Date(b.getFullYear(), b.getMonth(), b.getDate() + (4 - b.getDay())) - +d;
            b = 1 + Math.round(b / 6048E5);
            return Lb(b, a)
        }
    }

    function xc(a, b) {
        return 0 >= a.getFullYear() ? b.ERAS[0] : b.ERAS[1]
    }

    function Pd(a) {
        function b(a) {
            var b;
            if (b = a.match(d)) {
                a = new Date(0);
                var f = 0,
                    g = 0,
                    h = b[8] ? a.setUTCFullYear : a.setFullYear,
                    k = b[8] ? a.setUTCHours : a.setHours;
                b[9] && (f = Z(b[9] + b[10]), g = Z(b[9] + b[11]));
                h.call(a, Z(b[1]), Z(b[2]) - 1, Z(b[3]));
                f = Z(b[4] || 0) - f;
                g = Z(b[5] || 0) - g;
                h = Z(b[6] || 0);
                b = Math.round(1E3 * parseFloat("0." + (b[7] ||
                    0)));
                k.call(a, f, g, h, b)
            }
            return a
        }
        var d = /^(\d{4})-?(\d\d)-?(\d\d)(?:T(\d\d)(?::?(\d\d)(?::?(\d\d)(?:\.(\d+))?)?)?(Z|([+-])(\d\d):?(\d\d))?)?$/;
        return function(c, d, f) {
            var g = "",
                h = [],
                k, l;
            d = d || "mediumDate";
            d = a.DATETIME_FORMATS[d] || d;
            D(c) && (c = Mg.test(c) ? Z(c) : b(c));
            ba(c) && (c = new Date(c));
            if (!ga(c) || !isFinite(c.getTime())) return c;
            for (; d;)(l = Ng.exec(d)) ? (h = ab(h, l, 1), d = h.pop()) : (h.push(d), d = null);
            var m = c.getTimezoneOffset();
            f && (m = Mc(f, m), c = Yb(c, f, !0));
            p(h, function(b) {
                k = Og[b];
                g += k ? k(c, a.DATETIME_FORMATS, m) :
                    "''" === b ? "'" : b.replace(/(^'|'$)/g, "").replace(/''/g, "'")
            });
            return g
        }
    }

    function Fg() {
        return function(a, b) {
            x(b) && (b = 2);
            return cb(a, b)
        }
    }

    function Gg() {
        return function(a, b, d) {
            b = Infinity === Math.abs(Number(b)) ? Number(b) : Z(b);
            if (da(b)) return a;
            ba(a) && (a = a.toString());
            if (!ra(a)) return a;
            d = !d || isNaN(d) ? 0 : Z(d);
            d = 0 > d ? Math.max(0, a.length + d) : d;
            return 0 <= b ? yc(a, d, d + b) : 0 === d ? yc(a, b, a.length) : yc(a, Math.max(0, d + b), d)
        }
    }

    function yc(a, b, d) {
        return D(a) ? a.slice(b, d) : va.call(a, b, d)
    }

    function Rd(a) {
        function b(b) {
            return b.map(function(b) {
                var c =
                    1,
                    d = Ya;
                if (E(b)) d = b;
                else if (D(b)) {
                    if ("+" === b.charAt(0) || "-" === b.charAt(0)) c = "-" === b.charAt(0) ? -1 : 1, b = b.substring(1);
                    if ("" !== b && (d = a(b), d.constant)) var e = d(),
                        d = function(a) {
                            return a[e]
                        }
                }
                return {
                    get: d,
                    descending: c
                }
            })
        }

        function d(a) {
            switch (typeof a) {
                case "number":
                case "boolean":
                case "string":
                    return !0;
                default:
                    return !1
            }
        }

        function c(a, b) {
            var c = 0,
                d = a.type,
                k = b.type;
            if (d === k) {
                var k = a.value,
                    l = b.value;
                "string" === d ? (k = k.toLowerCase(), l = l.toLowerCase()) : "object" === d && (G(k) && (k = a.index), G(l) && (l = b.index));
                k !== l && (c =
                    k < l ? -1 : 1)
            } else c = d < k ? -1 : 1;
            return c
        }
        return function(a, f, g, h) {
            if (null == a) return a;
            if (!ra(a)) throw M("orderBy")("notarray", a);
            H(f) || (f = [f]);
            0 === f.length && (f = ["+"]);
            var k = b(f),
                l = g ? -1 : 1,
                m = E(h) ? h : c;
            a = Array.prototype.map.call(a, function(a, b) {
                return {
                    value: a,
                    tieBreaker: {
                        value: b,
                        type: "number",
                        index: b
                    },
                    predicateValues: k.map(function(c) {
                        var e = c.get(a);
                        c = typeof e;
                        if (null === e) c = "string", e = "null";
                        else if ("object" === c) a: {
                            if (E(e.valueOf) && (e = e.valueOf(), d(e))) break a;Xb(e) && (e = e.toString(), d(e))
                        }
                        return {
                            value: e,
                            type: c,
                            index: b
                        }
                    })
                }
            });
            a.sort(function(a, b) {
                for (var c = 0, d = k.length; c < d; c++) {
                    var e = m(a.predicateValues[c], b.predicateValues[c]);
                    if (e) return e * k[c].descending * l
                }
                return m(a.tieBreaker, b.tieBreaker) * l
            });
            return a = a.map(function(a) {
                return a.value
            })
        }
    }

    function Qa(a) {
        E(a) && (a = {
            link: a
        });
        a.restrict = a.restrict || "AC";
        return la(a)
    }

    function Mb(a, b, d, c, e) {
        this.$$controls = [];
        this.$error = {};
        this.$$success = {};
        this.$pending = void 0;
        this.$name = e(b.name || b.ngForm || "")(d);
        this.$dirty = !1;
        this.$valid = this.$pristine = !0;
        this.$submitted =
            this.$invalid = !1;
        this.$$parentForm = Nb;
        this.$$element = a;
        this.$$animate = c;
        Xd(this)
    }

    function Xd(a) {
        a.$$classCache = {};
        a.$$classCache[Yd] = !(a.$$classCache[ob] = a.$$element.hasClass(ob))
    }

    function Zd(a) {
        function b(a, b, c) {
            c && !a.$$classCache[b] ? (a.$$animate.addClass(a.$$element, b), a.$$classCache[b] = !0) : !c && a.$$classCache[b] && (a.$$animate.removeClass(a.$$element, b), a.$$classCache[b] = !1)
        }

        function d(a, c, d) {
            c = c ? "-" + Qc(c, "-") : "";
            b(a, ob + c, !0 === d);
            b(a, Yd + c, !1 === d)
        }
        var c = a.set,
            e = a.unset;
        a.clazz.prototype.$setValidity =
            function(a, g, h) {
                x(g) ? (this.$pending || (this.$pending = {}), c(this.$pending, a, h)) : (this.$pending && e(this.$pending, a, h), $d(this.$pending) && (this.$pending = void 0));
                Ha(g) ? g ? (e(this.$error, a, h), c(this.$$success, a, h)) : (c(this.$error, a, h), e(this.$$success, a, h)) : (e(this.$error, a, h), e(this.$$success, a, h));
                this.$pending ? (b(this, "ng-pending", !0), this.$valid = this.$invalid = void 0, d(this, "", null)) : (b(this, "ng-pending", !1), this.$valid = $d(this.$error), this.$invalid = !this.$valid, d(this, "", this.$valid));
                g = this.$pending &&
                    this.$pending[a] ? void 0 : this.$error[a] ? !1 : this.$$success[a] ? !0 : null;
                d(this, a, g);
                this.$$parentForm.$setValidity(a, g, this)
            }
    }

    function $d(a) {
        if (a)
            for (var b in a)
                if (a.hasOwnProperty(b)) return !1;
        return !0
    }

    function zc(a) {
        a.$formatters.push(function(b) {
            return a.$isEmpty(b) ? b : b.toString()
        })
    }

    function Ra(a, b, d, c, e, f) {
        var g = P(b[0].type);
        if (!e.android) {
            var h = !1;
            b.on("compositionstart", function() {
                h = !0
            });
            b.on("compositionend", function() {
                h = !1;
                l()
            })
        }
        var k, l = function(a) {
            k && (f.defer.cancel(k), k = null);
            if (!h) {
                var e = b.val();
                a = a && a.type;
                "password" === g || d.ngTrim && "false" === d.ngTrim || (e = S(e));
                (c.$viewValue !== e || "" === e && c.$$hasNativeValidators) && c.$setViewValue(e, a)
            }
        };
        if (e.hasEvent("input")) b.on("input", l);
        else {
            var m = function(a, b, c) {
                k || (k = f.defer(function() {
                    k = null;
                    b && b.value === c || l(a)
                }))
            };
            b.on("keydown", function(a) {
                var b = a.keyCode;
                91 === b || 15 < b && 19 > b || 37 <= b && 40 >= b || m(a, this, this.value)
            });
            if (e.hasEvent("paste")) b.on("paste cut", m)
        }
        b.on("change", l);
        if (ae[g] && c.$$hasNativeValidators && g === d.type) b.on("keydown wheel mousedown",
            function(a) {
                if (!k) {
                    var b = this.validity,
                        c = b.badInput,
                        d = b.typeMismatch;
                    k = f.defer(function() {
                        k = null;
                        b.badInput === c && b.typeMismatch === d || l(a)
                    })
                }
            });
        c.$render = function() {
            var a = c.$isEmpty(c.$viewValue) ? "" : c.$viewValue;
            b.val() !== a && b.val(a)
        }
    }

    function Ob(a, b) {
        return function(d, c) {
            var e, f;
            if (ga(d)) return d;
            if (D(d)) {
                '"' === d.charAt(0) && '"' === d.charAt(d.length - 1) && (d = d.substring(1, d.length - 1));
                if (Pg.test(d)) return new Date(d);
                a.lastIndex = 0;
                if (e = a.exec(d)) return e.shift(), f = c ? {
                    yyyy: c.getFullYear(),
                    MM: c.getMonth() +
                        1,
                    dd: c.getDate(),
                    HH: c.getHours(),
                    mm: c.getMinutes(),
                    ss: c.getSeconds(),
                    sss: c.getMilliseconds() / 1E3
                } : {
                    yyyy: 1970,
                    MM: 1,
                    dd: 1,
                    HH: 0,
                    mm: 0,
                    ss: 0,
                    sss: 0
                }, p(e, function(a, c) {
                    c < b.length && (f[b[c]] = +a)
                }), new Date(f.yyyy, f.MM - 1, f.dd, f.HH, f.mm, f.ss || 0, 1E3 * f.sss || 0)
            }
            return NaN
        }
    }

    function pb(a, b, d, c) {
        return function(e, f, g, h, k, l, m) {
            function n(a) {
                return a && !(a.getTime && a.getTime() !== a.getTime())
            }

            function q(a) {
                return u(a) && !ga(a) ? d(a) || void 0 : a
            }
            Ac(e, f, g, h);
            Ra(e, f, g, h, k, l);
            var r = h && h.$options.getOption("timezone"),
                p;
            h.$$parserName =
                a;
            h.$parsers.push(function(a) {
                if (h.$isEmpty(a)) return null;
                if (b.test(a)) return a = d(a, p), r && (a = Yb(a, r)), a
            });
            h.$formatters.push(function(a) {
                if (a && !ga(a)) throw qb("datefmt", a);
                if (n(a)) return (p = a) && r && (p = Yb(p, r, !0)), m("date")(a, c, r);
                p = null;
                return ""
            });
            if (u(g.min) || g.ngMin) {
                var s;
                h.$validators.min = function(a) {
                    return !n(a) || x(s) || d(a) >= s
                };
                g.$observe("min", function(a) {
                    s = q(a);
                    h.$validate()
                })
            }
            if (u(g.max) || g.ngMax) {
                var t;
                h.$validators.max = function(a) {
                    return !n(a) || x(t) || d(a) <= t
                };
                g.$observe("max", function(a) {
                    t =
                        q(a);
                    h.$validate()
                })
            }
        }
    }

    function Ac(a, b, d, c) {
        (c.$$hasNativeValidators = G(b[0].validity)) && c.$parsers.push(function(a) {
            var c = b.prop("validity") || {};
            return c.badInput || c.typeMismatch ? void 0 : a
        })
    }

    function be(a) {
        a.$$parserName = "number";
        a.$parsers.push(function(b) {
            if (a.$isEmpty(b)) return null;
            if (Qg.test(b)) return parseFloat(b)
        });
        a.$formatters.push(function(b) {
            if (!a.$isEmpty(b)) {
                if (!ba(b)) throw qb("numfmt", b);
                b = b.toString()
            }
            return b
        })
    }

    function Sa(a) {
        u(a) && !ba(a) && (a = parseFloat(a));
        return da(a) ? void 0 : a
    }

    function Bc(a) {
        var b =
            a.toString(),
            d = b.indexOf(".");
        return -1 === d ? -1 < a && 1 > a && (a = /e-(\d+)$/.exec(b)) ? Number(a[1]) : 0 : b.length - d - 1
    }

    function ce(a, b, d) {
        a = Number(a);
        var c = (a | 0) !== a,
            e = (b | 0) !== b,
            f = (d | 0) !== d;
        if (c || e || f) {
            var g = c ? Bc(a) : 0,
                h = e ? Bc(b) : 0,
                k = f ? Bc(d) : 0,
                g = Math.max(g, h, k),
                g = Math.pow(10, g);
            a *= g;
            b *= g;
            d *= g;
            c && (a = Math.round(a));
            e && (b = Math.round(b));
            f && (d = Math.round(d))
        }
        return 0 === (a - b) % d
    }

    function de(a, b, d, c, e) {
        if (u(c)) {
            a = a(c);
            if (!a.constant) throw qb("constexpr", d, c);
            return a(b)
        }
        return e
    }

    function Cc(a, b) {
        function d(a, b) {
            if (!a ||
                !a.length) return [];
            if (!b || !b.length) return a;
            var c = [],
                d = 0;
            a: for (; d < a.length; d++) {
                for (var e = a[d], f = 0; f < b.length; f++)
                    if (e === b[f]) continue a;
                c.push(e)
            }
            return c
        }

        function c(a) {
            var b = a;
            H(a) ? b = a.map(c).join(" ") : G(a) && (b = Object.keys(a).filter(function(b) {
                return a[b]
            }).join(" "));
            return b
        }

        function e(a) {
            var b = a;
            if (H(a)) b = a.map(e);
            else if (G(a)) {
                var c = !1,
                    b = Object.keys(a).filter(function(b) {
                        b = a[b];
                        !c && x(b) && (c = !0);
                        return b
                    });
                c && b.push(void 0)
            }
            return b
        }
        a = "ngClass" + a;
        var f;
        return ["$parse", function(g) {
            return {
                restrict: "AC",
                link: function(h, k, l) {
                    function m(a, b) {
                        var c = [];
                        p(a, function(a) {
                            if (0 < b || K[a]) K[a] = (K[a] || 0) + b, K[a] === +(0 < b) && c.push(a)
                        });
                        return c.join(" ")
                    }

                    function n(a) {
                        if (a === b) {
                            var c = v,
                                c = m(c && c.split(" "), 1);
                            l.$addClass(c)
                        } else c = v, c = m(c && c.split(" "), -1), l.$removeClass(c);
                        y = a
                    }

                    function q(a) {
                        a = c(a);
                        a !== v && r(a)
                    }

                    function r(a) {
                        if (y === b) {
                            var c = v && v.split(" "),
                                e = a && a.split(" "),
                                f = d(c, e),
                                c = d(e, c),
                                f = m(f, -1),
                                c = m(c, 1);
                            l.$addClass(c);
                            l.$removeClass(f)
                        }
                        v = a
                    }
                    var s = l[a].trim(),
                        u = ":" === s.charAt(0) && ":" === s.charAt(1),
                        s = g(s, u ? e :
                            c),
                        t = u ? q : r,
                        K = k.data("$classCounts"),
                        y = !0,
                        v;
                    K || (K = V(), k.data("$classCounts", K));
                    "ngClass" !== a && (f || (f = g("$index", function(a) {
                        return a & 1
                    })), h.$watch(f, n));
                    h.$watch(s, t, u)
                }
            }
        }]
    }

    function Pb(a, b, d, c, e, f, g, h, k) {
        this.$modelValue = this.$viewValue = Number.NaN;
        this.$$rawModelValue = void 0;
        this.$validators = {};
        this.$asyncValidators = {};
        this.$parsers = [];
        this.$formatters = [];
        this.$viewChangeListeners = [];
        this.$untouched = !0;
        this.$touched = !1;
        this.$pristine = !0;
        this.$dirty = !1;
        this.$valid = !0;
        this.$invalid = !1;
        this.$error = {};
        this.$$success = {};
        this.$pending = void 0;
        this.$name = k(d.name || "", !1)(a);
        this.$$parentForm = Nb;
        this.$options = Qb;
        this.$$parsedNgModel = e(d.ngModel);
        this.$$parsedNgModelAssign = this.$$parsedNgModel.assign;
        this.$$ngModelGet = this.$$parsedNgModel;
        this.$$ngModelSet = this.$$parsedNgModelAssign;
        this.$$pendingDebounce = null;
        this.$$parserValid = void 0;
        this.$$currentValidationRunId = 0;
        this.$$scope = a;
        this.$$attr = d;
        this.$$element = c;
        this.$$animate = f;
        this.$$timeout = g;
        this.$$parse = e;
        this.$$q = h;
        this.$$exceptionHandler = b;
        Xd(this);
        Rg(this)
    }

    function Rg(a) {
        a.$$scope.$watch(function() {
            var b = a.$$ngModelGet(a.$$scope);
            if (b !== a.$modelValue && (a.$modelValue === a.$modelValue || b === b)) {
                a.$modelValue = a.$$rawModelValue = b;
                a.$$parserValid = void 0;
                for (var d = a.$formatters, c = d.length, e = b; c--;) e = d[c](e);
                a.$viewValue !== e && (a.$$updateEmptyClasses(e), a.$viewValue = a.$$lastCommittedViewValue = e, a.$render(), a.$$runValidators(a.$modelValue, a.$viewValue, A))
            }
            return b
        })
    }

    function Dc(a) {
        this.$$options = a
    }

    function ee(a, b) {
        p(b, function(b, c) {
            u(a[c]) || (a[c] = b)
        })
    }

    function Ta(a, b) {
        a.prop("selected", b);
        a.attr("selected", b)
    }
    var Sg = /^\/(.+)\/([a-z]*)$/,
        ua = Object.prototype.hasOwnProperty,
        Fc = {
            objectMaxDepth: 5
        },
        P = function(a) {
            return D(a) ? a.toLowerCase() : a
        },
        vb = function(a) {
            return D(a) ? a.toUpperCase() : a
        },
        za, F, na, va = [].slice,
        sg = [].splice,
        Tg = [].push,
        ma = Object.prototype.toString,
        Jc = Object.getPrototypeOf,
        Fa = M("ng"),
        ea = w.angular || (w.angular = {}),
        ac, rb = 0;
    za = w.document.documentMode;
    var da = Number.isNaN || function(a) {
        return a !== a
    };
    A.$inject = [];
    Ya.$inject = [];
    var H = Array.isArray,
        qe = /^\[object (?:Uint8|Uint8Clamped|Uint16|Uint32|Int8|Int16|Int32|Float32|Float64)Array]$/,
        S = function(a) {
            return D(a) ? a.trim() : a
        },
        Kd = function(a) {
            return a.replace(/([-()[\]{}+?*.$^|,:#<!\\])/g, "\\$1").replace(/\x08/g, "\\x08")
        },
        Ga = function() {
            if (!u(Ga.rules)) {
                var a = w.document.querySelector("[ng-csp]") || w.document.querySelector("[data-ng-csp]");
                if (a) {
                    var b = a.getAttribute("ng-csp") || a.getAttribute("data-ng-csp");
                    Ga.rules = {
                        noUnsafeEval: !b || -1 !== b.indexOf("no-unsafe-eval"),
                        noInlineStyle: !b || -1 !== b.indexOf("no-inline-style")
                    }
                } else {
                    a =
                        Ga;
                    try {
                        new Function(""), b = !1
                    } catch (d) {
                        b = !0
                    }
                    a.rules = {
                        noUnsafeEval: b,
                        noInlineStyle: !1
                    }
                }
            }
            return Ga.rules
        },
        sb = function() {
            if (u(sb.name_)) return sb.name_;
            var a, b, d = Ja.length,
                c, e;
            for (b = 0; b < d; ++b)
                if (c = Ja[b], a = w.document.querySelector("[" + c.replace(":", "\\:") + "jq]")) {
                    e = a.getAttribute(c + "jq");
                    break
                } return sb.name_ = e
        },
        se = /:/g,
        Ja = ["ng-", "data-ng-", "ng:", "x-ng-"],
        ve = function(a) {
            var b = a.currentScript;
            if (!b) return !0;
            if (!(b instanceof w.HTMLScriptElement || b instanceof w.SVGScriptElement)) return !1;
            b = b.attributes;
            return [b.getNamedItem("src"), b.getNamedItem("href"), b.getNamedItem("xlink:href")].every(function(b) {
                if (!b) return !0;
                if (!b.value) return !1;
                var c = a.createElement("a");
                c.href = b.value;
                if (a.location.origin === c.origin) return !0;
                switch (c.protocol) {
                    case "http:":
                    case "https:":
                    case "ftp:":
                    case "blob:":
                    case "file:":
                    case "data:":
                        return !0;
                    default:
                        return !1
                }
            })
        }(w.document),
        ye = /[A-Z]/g,
        Rc = !1,
        Ia = 3,
        De = {
            full: "1.6.3",
            major: 1,
            minor: 6,
            dot: 3,
            codeName: "scriptalicious-bootstrapping"
        };
    W.expando = "ng339";
    var ib = W.cache = {},
        eg = 1;
    W._data = function(a) {
        return this.cache[a[this.expando]] || {}
    };
    var ag = /-([a-z])/g,
        Ug = /^-ms-/,
        Ab = {
            mouseleave: "mouseout",
            mouseenter: "mouseover"
        },
        cc = M("jqLite"),
        dg = /^<([\w-]+)\s*\/?>(?:<\/\1>|)$/,
        bc = /<|&#?\w+;/,
        bg = /<([\w:-]+)/,
        cg = /<(?!area|br|col|embed|hr|img|input|link|meta|param)(([\w:-]+)[^>]*)\/>/gi,
        ha = {
            option: [1, '<select multiple="multiple">', "</select>"],
            thead: [1, "<table>", "</table>"],
            col: [2, "<table><colgroup>", "</colgroup></table>"],
            tr: [2, "<table><tbody>", "</tbody></table>"],
            td: [3, "<table><tbody><tr>",
                "</tr></tbody></table>"
            ],
            _default: [0, "", ""]
        };
    ha.optgroup = ha.option;
    ha.tbody = ha.tfoot = ha.colgroup = ha.caption = ha.thead;
    ha.th = ha.td;
    var jg = w.Node.prototype.contains || function(a) {
            return !!(this.compareDocumentPosition(a) & 16)
        },
        Oa = W.prototype = {
            ready: cd,
            toString: function() {
                var a = [];
                p(this, function(b) {
                    a.push("" + b)
                });
                return "[" + a.join(", ") + "]"
            },
            eq: function(a) {
                return 0 <= a ? F(this[a]) : F(this[this.length + a])
            },
            length: 0,
            push: Tg,
            sort: [].sort,
            splice: [].splice
        },
        Gb = {};
    p("multiple selected checked disabled readOnly required open".split(" "),
        function(a) {
            Gb[P(a)] = a
        });
    var hd = {};
    p("input select option textarea button form details".split(" "), function(a) {
        hd[a] = !0
    });
    var pd = {
        ngMinlength: "minlength",
        ngMaxlength: "maxlength",
        ngMin: "min",
        ngMax: "max",
        ngPattern: "pattern",
        ngStep: "step"
    };
    p({
        data: fc,
        removeData: hb,
        hasData: function(a) {
            for (var b in ib[a.ng339]) return !0;
            return !1
        },
        cleanData: function(a) {
            for (var b = 0, d = a.length; b < d; b++) hb(a[b])
        }
    }, function(a, b) {
        W[b] = a
    });
    p({
        data: fc,
        inheritedData: Eb,
        scope: function(a) {
            return F.data(a, "$scope") || Eb(a.parentNode ||
                a, ["$isolateScope", "$scope"])
        },
        isolateScope: function(a) {
            return F.data(a, "$isolateScope") || F.data(a, "$isolateScopeNoTemplate")
        },
        controller: ed,
        injector: function(a) {
            return Eb(a, "$injector")
        },
        removeAttr: function(a, b) {
            a.removeAttribute(b)
        },
        hasClass: Bb,
        css: function(a, b, d) {
            b = xb(b.replace(Ug, "ms-"));
            if (u(d)) a.style[b] = d;
            else return a.style[b]
        },
        attr: function(a, b, d) {
            var c = a.nodeType;
            if (c !== Ia && 2 !== c && 8 !== c && a.getAttribute) {
                var c = P(b),
                    e = Gb[c];
                if (u(d)) null === d || !1 === d && e ? a.removeAttribute(b) : a.setAttribute(b,
                    e ? c : d);
                else return a = a.getAttribute(b), e && null !== a && (a = c), null === a ? void 0 : a
            }
        },
        prop: function(a, b, d) {
            if (u(d)) a[b] = d;
            else return a[b]
        },
        text: function() {
            function a(a, d) {
                if (x(d)) {
                    var c = a.nodeType;
                    return 1 === c || c === Ia ? a.textContent : ""
                }
                a.textContent = d
            }
            a.$dv = "";
            return a
        }(),
        val: function(a, b) {
            if (x(b)) {
                if (a.multiple && "select" === wa(a)) {
                    var d = [];
                    p(a.options, function(a) {
                        a.selected && d.push(a.value || a.text)
                    });
                    return d
                }
                return a.value
            }
            a.value = b
        },
        html: function(a, b) {
            if (x(b)) return a.innerHTML;
            yb(a, !0);
            a.innerHTML = b
        },
        empty: fd
    }, function(a, b) {
        W.prototype[b] = function(b, c) {
            var e, f, g = this.length;
            if (a !== fd && x(2 === a.length && a !== Bb && a !== ed ? b : c)) {
                if (G(b)) {
                    for (e = 0; e < g; e++)
                        if (a === fc) a(this[e], b);
                        else
                            for (f in b) a(this[e], f, b[f]);
                    return this
                }
                e = a.$dv;
                g = x(e) ? Math.min(g, 1) : g;
                for (f = 0; f < g; f++) {
                    var h = a(this[f], b, c);
                    e = e ? e + h : h
                }
                return e
            }
            for (e = 0; e < g; e++) a(this[e], b, c);
            return this
        }
    });
    p({
        removeData: hb,
        on: function(a, b, d, c) {
            if (u(c)) throw cc("onargs");
            if (ad(a)) {
                c = zb(a, !0);
                var e = c.events,
                    f = c.handle;
                f || (f = c.handle = gg(a, e));
                c = 0 <= b.indexOf(" ") ?
                    b.split(" ") : [b];
                for (var g = c.length, h = function(b, c, g) {
                        var h = e[b];
                        h || (h = e[b] = [], h.specialHandlerWrapper = c, "$destroy" === b || g || a.addEventListener(b, f));
                        h.push(d)
                    }; g--;) b = c[g], Ab[b] ? (h(Ab[b], ig), h(b, void 0, !0)) : h(b)
            }
        },
        off: dd,
        one: function(a, b, d) {
            a = F(a);
            a.on(b, function e() {
                a.off(b, d);
                a.off(b, e)
            });
            a.on(b, d)
        },
        replaceWith: function(a, b) {
            var d, c = a.parentNode;
            yb(a);
            p(new W(b), function(b) {
                d ? c.insertBefore(b, d.nextSibling) : c.replaceChild(b, a);
                d = b
            })
        },
        children: function(a) {
            var b = [];
            p(a.childNodes, function(a) {
                1 ===
                    a.nodeType && b.push(a)
            });
            return b
        },
        contents: function(a) {
            return a.contentDocument || a.childNodes || []
        },
        append: function(a, b) {
            var d = a.nodeType;
            if (1 === d || 11 === d) {
                b = new W(b);
                for (var d = 0, c = b.length; d < c; d++) a.appendChild(b[d])
            }
        },
        prepend: function(a, b) {
            if (1 === a.nodeType) {
                var d = a.firstChild;
                p(new W(b), function(b) {
                    a.insertBefore(b, d)
                })
            }
        },
        wrap: function(a, b) {
            var d = F(b).eq(0).clone()[0],
                c = a.parentNode;
            c && c.replaceChild(d, a);
            d.appendChild(a)
        },
        remove: Fb,
        detach: function(a) {
            Fb(a, !0)
        },
        after: function(a, b) {
            var d = a,
                c = a.parentNode;
            if (c) {
                b = new W(b);
                for (var e = 0, f = b.length; e < f; e++) {
                    var g = b[e];
                    c.insertBefore(g, d.nextSibling);
                    d = g
                }
            }
        },
        addClass: Db,
        removeClass: Cb,
        toggleClass: function(a, b, d) {
            b && p(b.split(" "), function(b) {
                var e = d;
                x(e) && (e = !Bb(a, b));
                (e ? Db : Cb)(a, b)
            })
        },
        parent: function(a) {
            return (a = a.parentNode) && 11 !== a.nodeType ? a : null
        },
        next: function(a) {
            return a.nextElementSibling
        },
        find: function(a, b) {
            return a.getElementsByTagName ? a.getElementsByTagName(b) : []
        },
        clone: ec,
        triggerHandler: function(a, b, d) {
            var c, e, f = b.type || b,
                g = zb(a);
            if (g = (g = g && g.events) &&
                g[f]) c = {
                preventDefault: function() {
                    this.defaultPrevented = !0
                },
                isDefaultPrevented: function() {
                    return !0 === this.defaultPrevented
                },
                stopImmediatePropagation: function() {
                    this.immediatePropagationStopped = !0
                },
                isImmediatePropagationStopped: function() {
                    return !0 === this.immediatePropagationStopped
                },
                stopPropagation: A,
                type: f,
                target: a
            }, b.type && (c = R(c, b)), b = qa(g), e = d ? [c].concat(d) : [c], p(b, function(b) {
                c.isImmediatePropagationStopped() || b.apply(a, e)
            })
        }
    }, function(a, b) {
        W.prototype[b] = function(b, c, e) {
            for (var f, g = 0, h = this.length; g <
                h; g++) x(f) ? (f = a(this[g], b, c, e), u(f) && (f = F(f))) : dc(f, a(this[g], b, c, e));
            return u(f) ? f : this
        }
    });
    W.prototype.bind = W.prototype.on;
    W.prototype.unbind = W.prototype.off;
    var Vg = Object.create(null);
    id.prototype = {
        _idx: function(a) {
            if (a === this._lastKey) return this._lastIndex;
            this._lastKey = a;
            return this._lastIndex = this._keys.indexOf(a)
        },
        _transformKey: function(a) {
            return da(a) ? Vg : a
        },
        get: function(a) {
            a = this._transformKey(a);
            a = this._idx(a);
            if (-1 !== a) return this._values[a]
        },
        set: function(a, b) {
            a = this._transformKey(a);
            var d =
                this._idx(a); - 1 === d && (d = this._lastIndex = this._keys.length);
            this._keys[d] = a;
            this._values[d] = b
        },
        delete: function(a) {
            a = this._transformKey(a);
            a = this._idx(a);
            if (-1 === a) return !1;
            this._keys.splice(a, 1);
            this._values.splice(a, 1);
            this._lastKey = NaN;
            this._lastIndex = -1;
            return !0
        }
    };
    var Hb = id,
        Zf = [function() {
            this.$get = [function() {
                return Hb
            }]
        }],
        lg = /^([^(]+?)=>/,
        mg = /^[^(]*\(\s*([^)]*)\)/m,
        Wg = /,/,
        Xg = /^\s*(_?)(\S+?)\1\s*$/,
        kg = /((\/\/.*$)|(\/\*[\s\S]*?\*\/))/mg,
        ya = M("$injector");
    eb.$$annotate = function(a, b, d) {
        var c;
        if ("function" ===
            typeof a) {
            if (!(c = a.$inject)) {
                c = [];
                if (a.length) {
                    if (b) throw D(d) && d || (d = a.name || ng(a)), ya("strictdi", d);
                    b = jd(a);
                    p(b[1].split(Wg), function(a) {
                        a.replace(Xg, function(a, b, d) {
                            c.push(d)
                        })
                    })
                }
                a.$inject = c
            }
        } else H(a) ? (b = a.length - 1, tb(a[b], "fn"), c = a.slice(0, b)) : tb(a, "fn", !0);
        return c
    };
    var fe = M("$animate"),
        qf = function() {
            this.$get = A
        },
        rf = function() {
            var a = new Hb,
                b = [];
            this.$get = ["$$AnimateRunner", "$rootScope", function(d, c) {
                function e(a, b, c) {
                    var d = !1;
                    b && (b = D(b) ? b.split(" ") : H(b) ? b : [], p(b, function(b) {
                        b && (d = !0, a[b] = c)
                    }));
                    return d
                }

                function f() {
                    p(b, function(b) {
                        var c = a.get(b);
                        if (c) {
                            var d = og(b.attr("class")),
                                e = "",
                                f = "";
                            p(c, function(a, b) {
                                a !== !!d[b] && (a ? e += (e.length ? " " : "") + b : f += (f.length ? " " : "") + b)
                            });
                            p(b, function(a) {
                                e && Db(a, e);
                                f && Cb(a, f)
                            });
                            a.delete(b)
                        }
                    });
                    b.length = 0
                }
                return {
                    enabled: A,
                    on: A,
                    off: A,
                    pin: A,
                    push: function(g, h, k, l) {
                        l && l();
                        k = k || {};
                        k.from && g.css(k.from);
                        k.to && g.css(k.to);
                        if (k.addClass || k.removeClass)
                            if (h = k.addClass, l = k.removeClass, k = a.get(g) || {}, h = e(k, h, !0), l = e(k, l, !1), h || l) a.set(g, k), b.push(g), 1 === b.length && c.$$postDigest(f);
                        g = new d;
                        g.complete();
                        return g
                    }
                }
            }]
        },
        of = ["$provide", function(a) {
            var b = this,
                d = null;
            this.$$registeredAnimations = Object.create(null);
            this.register = function(c, d) {
                if (c && "." !== c.charAt(0)) throw fe("notcsel", c);
                var f = c + "-animation";
                b.$$registeredAnimations[c.substr(1)] = f;
                a.factory(f, d)
            };
            this.classNameFilter = function(a) {
                if (1 === arguments.length && (d = a instanceof RegExp ? a : null) && /[(\s|\/)]ng-animate[(\s|\/)]/.test(d.toString())) throw d = null, fe("nongcls", "ng-animate");
                return d
            };
            this.$get = ["$$animateQueue", function(a) {
                function b(a,
                    c, d) {
                    if (d) {
                        var e;
                        a: {
                            for (e = 0; e < d.length; e++) {
                                var l = d[e];
                                if (1 === l.nodeType) {
                                    e = l;
                                    break a
                                }
                            }
                            e = void 0
                        }!e || e.parentNode || e.previousElementSibling || (d = null)
                    }
                    d ? d.after(a) : c.prepend(a)
                }
                return {
                    on: a.on,
                    off: a.off,
                    pin: a.pin,
                    enabled: a.enabled,
                    cancel: function(a) {
                        a.end && a.end()
                    },
                    enter: function(d, g, h, k) {
                        g = g && F(g);
                        h = h && F(h);
                        g = g || h.parent();
                        b(d, g, h);
                        return a.push(d, "enter", ia(k))
                    },
                    move: function(d, g, h, k) {
                        g = g && F(g);
                        h = h && F(h);
                        g = g || h.parent();
                        b(d, g, h);
                        return a.push(d, "move", ia(k))
                    },
                    leave: function(b, d) {
                        return a.push(b, "leave",
                            ia(d),
                            function() {
                                b.remove()
                            })
                    },
                    addClass: function(b, d, e) {
                        e = ia(e);
                        e.addClass = jb(e.addclass, d);
                        return a.push(b, "addClass", e)
                    },
                    removeClass: function(b, d, e) {
                        e = ia(e);
                        e.removeClass = jb(e.removeClass, d);
                        return a.push(b, "removeClass", e)
                    },
                    setClass: function(b, d, e, k) {
                        k = ia(k);
                        k.addClass = jb(k.addClass, d);
                        k.removeClass = jb(k.removeClass, e);
                        return a.push(b, "setClass", k)
                    },
                    animate: function(b, d, e, k, l) {
                        l = ia(l);
                        l.from = l.from ? R(l.from, d) : d;
                        l.to = l.to ? R(l.to, e) : e;
                        l.tempClasses = jb(l.tempClasses, k || "ng-inline-animate");
                        return a.push(b,
                            "animate", l)
                    }
                }
            }]
        }],
        tf = function() {
            this.$get = ["$$rAF", function(a) {
                function b(b) {
                    d.push(b);
                    1 < d.length || a(function() {
                        for (var a = 0; a < d.length; a++) d[a]();
                        d = []
                    })
                }
                var d = [];
                return function() {
                    var a = !1;
                    b(function() {
                        a = !0
                    });
                    return function(d) {
                        a ? d() : b(d)
                    }
                }
            }]
        },
        sf = function() {
            this.$get = ["$q", "$sniffer", "$$animateAsyncRun", "$$isDocumentHidden", "$timeout", function(a, b, d, c, e) {
                function f(a) {
                    this.setHost(a);
                    var b = d();
                    this._doneCallbacks = [];
                    this._tick = function(a) {
                        c() ? e(a, 0, !1) : b(a)
                    };
                    this._state = 0
                }
                f.chain = function(a, b) {
                    function c() {
                        if (d ===
                            a.length) b(!0);
                        else a[d](function(a) {
                            !1 === a ? b(!1) : (d++, c())
                        })
                    }
                    var d = 0;
                    c()
                };
                f.all = function(a, b) {
                    function c(f) {
                        e = e && f;
                        ++d === a.length && b(e)
                    }
                    var d = 0,
                        e = !0;
                    p(a, function(a) {
                        a.done(c)
                    })
                };
                f.prototype = {
                    setHost: function(a) {
                        this.host = a || {}
                    },
                    done: function(a) {
                        2 === this._state ? a() : this._doneCallbacks.push(a)
                    },
                    progress: A,
                    getPromise: function() {
                        if (!this.promise) {
                            var b = this;
                            this.promise = a(function(a, c) {
                                b.done(function(b) {
                                    !1 === b ? c() : a()
                                })
                            })
                        }
                        return this.promise
                    },
                    then: function(a, b) {
                        return this.getPromise().then(a, b)
                    },
                    "catch": function(a) {
                        return this.getPromise()["catch"](a)
                    },
                    "finally": function(a) {
                        return this.getPromise()["finally"](a)
                    },
                    pause: function() {
                        this.host.pause && this.host.pause()
                    },
                    resume: function() {
                        this.host.resume && this.host.resume()
                    },
                    end: function() {
                        this.host.end && this.host.end();
                        this._resolve(!0)
                    },
                    cancel: function() {
                        this.host.cancel && this.host.cancel();
                        this._resolve(!1)
                    },
                    complete: function(a) {
                        var b = this;
                        0 === b._state && (b._state = 1, b._tick(function() {
                            b._resolve(a)
                        }))
                    },
                    _resolve: function(a) {
                        2 !== this._state && (p(this._doneCallbacks, function(b) {
                                b(a)
                            }), this._doneCallbacks.length =
                            0, this._state = 2)
                    }
                };
                return f
            }]
        },
        pf = function() {
            this.$get = ["$$rAF", "$q", "$$AnimateRunner", function(a, b, d) {
                return function(b, e) {
                    function f() {
                        a(function() {
                            g.addClass && (b.addClass(g.addClass), g.addClass = null);
                            g.removeClass && (b.removeClass(g.removeClass), g.removeClass = null);
                            g.to && (b.css(g.to), g.to = null);
                            h || k.complete();
                            h = !0
                        });
                        return k
                    }
                    var g = e || {};
                    g.$$prepared || (g = sa(g));
                    g.cleanupStyles && (g.from = g.to = null);
                    g.from && (b.css(g.from), g.from = null);
                    var h, k = new d;
                    return {
                        start: f,
                        end: f
                    }
                }
            }]
        },
        fa = M("$compile"),
        jc = new function() {};
    Tc.$inject = ["$provide", "$$sanitizeUriProvider"];
    Jb.prototype.isFirstChange = function() {
        return this.previousValue === jc
    };
    var kd = /^((?:x|data)[:\-_])/i,
        rg = /[:\-_]+(.)/g,
        rd = M("$controller"),
        qd = /^(\S+)(\s+as\s+([\w$]+))?$/,
        Af = function() {
            this.$get = ["$document", function(a) {
                return function(b) {
                    b ? !b.nodeType && b instanceof F && (b = b[0]) : b = a[0].body;
                    return b.offsetWidth + 1
                }
            }]
        },
        sd = "application/json",
        mc = {
            "Content-Type": sd + ";charset=utf-8"
        },
        ug = /^\[|^\{(?!\{)/,
        vg = {
            "[": /]$/,
            "{": /}$/
        },
        tg = /^\)]\}',?\n/,
        xd = M("$http"),
        Da =
        ea.$interpolateMinErr = M("$interpolate");
    Da.throwNoconcat = function(a) {
        throw Da("noconcat", a);
    };
    Da.interr = function(a, b) {
        return Da("interr", a, b.toString())
    };
    var If = function() {
            this.$get = function() {
                function a(a) {
                    var b = function(a) {
                        b.data = a;
                        b.called = !0
                    };
                    b.id = a;
                    return b
                }
                var b = ea.callbacks,
                    d = {};
                return {
                    createCallback: function(c) {
                        c = "_" + (b.$$counter++).toString(36);
                        var e = "angular.callbacks." + c,
                            f = a(c);
                        d[e] = b[c] = f;
                        return e
                    },
                    wasCalled: function(a) {
                        return d[a].called
                    },
                    getResponse: function(a) {
                        return d[a].data
                    },
                    removeCallback: function(a) {
                        delete b[d[a].id];
                        delete d[a]
                    }
                }
            }
        },
        Yg = /^([^?#]*)(\?([^#]*))?(#(.*))?$/,
        xg = {
            http: 80,
            https: 443,
            ftp: 21
        },
        lb = M("$location"),
        yg = /^\s*[\\/]{2,}/,
        Zg = {
            $$absUrl: "",
            $$html5: !1,
            $$replace: !1,
            absUrl: Kb("$$absUrl"),
            url: function(a) {
                if (x(a)) return this.$$url;
                var b = Yg.exec(a);
                (b[1] || "" === a) && this.path(decodeURIComponent(b[1]));
                (b[2] || b[1] || "" === a) && this.search(b[3] || "");
                this.hash(b[5] || "");
                return this
            },
            protocol: Kb("$$protocol"),
            host: Kb("$$host"),
            port: Kb("$$port"),
            path: Bd("$$path", function(a) {
                a = null !== a ? a.toString() : "";
                return "/" === a.charAt(0) ?
                    a : "/" + a
            }),
            search: function(a, b) {
                switch (arguments.length) {
                    case 0:
                        return this.$$search;
                    case 1:
                        if (D(a) || ba(a)) a = a.toString(), this.$$search = Oc(a);
                        else if (G(a)) a = sa(a, {}), p(a, function(b, c) {
                            null == b && delete a[c]
                        }), this.$$search = a;
                        else throw lb("isrcharg");
                        break;
                    default:
                        x(b) || null === b ? delete this.$$search[a] : this.$$search[a] = b
                }
                this.$$compose();
                return this
            },
            hash: Bd("$$hash", function(a) {
                return null !== a ? a.toString() : ""
            }),
            replace: function() {
                this.$$replace = !0;
                return this
            }
        };
    p([Ad, qc, pc], function(a) {
        a.prototype =
            Object.create(Zg);
        a.prototype.state = function(b) {
            if (!arguments.length) return this.$$state;
            if (a !== pc || !this.$$html5) throw lb("nostate");
            this.$$state = x(b) ? null : b;
            this.$$urlUpdatedByLocation = !0;
            return this
        }
    });
    var Ua = M("$parse"),
        Bg = {}.constructor.prototype.valueOf,
        Rb = V();
    p("+ - * / % === !== == != < > <= >= && || ! = |".split(" "), function(a) {
        Rb[a] = !0
    });
    var $g = {
            n: "\n",
            f: "\f",
            r: "\r",
            t: "\t",
            v: "\v",
            "'": "'",
            '"': '"'
        },
        sc = function(a) {
            this.options = a
        };
    sc.prototype = {
        constructor: sc,
        lex: function(a) {
            this.text = a;
            this.index =
                0;
            for (this.tokens = []; this.index < this.text.length;)
                if (a = this.text.charAt(this.index), '"' === a || "'" === a) this.readString(a);
                else if (this.isNumber(a) || "." === a && this.isNumber(this.peek())) this.readNumber();
            else if (this.isIdentifierStart(this.peekMultichar())) this.readIdent();
            else if (this.is(a, "(){}[].,;:?")) this.tokens.push({
                index: this.index,
                text: a
            }), this.index++;
            else if (this.isWhitespace(a)) this.index++;
            else {
                var b = a + this.peek(),
                    d = b + this.peek(2),
                    c = Rb[b],
                    e = Rb[d];
                Rb[a] || c || e ? (a = e ? d : c ? b : a, this.tokens.push({
                    index: this.index,
                    text: a,
                    operator: !0
                }), this.index += a.length) : this.throwError("Unexpected next character ", this.index, this.index + 1)
            }
            return this.tokens
        },
        is: function(a, b) {
            return -1 !== b.indexOf(a)
        },
        peek: function(a) {
            a = a || 1;
            return this.index + a < this.text.length ? this.text.charAt(this.index + a) : !1
        },
        isNumber: function(a) {
            return "0" <= a && "9" >= a && "string" === typeof a
        },
        isWhitespace: function(a) {
            return " " === a || "\r" === a || "\t" === a || "\n" === a || "\v" === a || " " === a
        },
        isIdentifierStart: function(a) {
            return this.options.isIdentifierStart ? this.options.isIdentifierStart(a,
                this.codePointAt(a)) : this.isValidIdentifierStart(a)
        },
        isValidIdentifierStart: function(a) {
            return "a" <= a && "z" >= a || "A" <= a && "Z" >= a || "_" === a || "$" === a
        },
        isIdentifierContinue: function(a) {
            return this.options.isIdentifierContinue ? this.options.isIdentifierContinue(a, this.codePointAt(a)) : this.isValidIdentifierContinue(a)
        },
        isValidIdentifierContinue: function(a, b) {
            return this.isValidIdentifierStart(a, b) || this.isNumber(a)
        },
        codePointAt: function(a) {
            return 1 === a.length ? a.charCodeAt(0) : (a.charCodeAt(0) << 10) + a.charCodeAt(1) -
                56613888
        },
        peekMultichar: function() {
            var a = this.text.charAt(this.index),
                b = this.peek();
            if (!b) return a;
            var d = a.charCodeAt(0),
                c = b.charCodeAt(0);
            return 55296 <= d && 56319 >= d && 56320 <= c && 57343 >= c ? a + b : a
        },
        isExpOperator: function(a) {
            return "-" === a || "+" === a || this.isNumber(a)
        },
        throwError: function(a, b, d) {
            d = d || this.index;
            b = u(b) ? "s " + b + "-" + this.index + " [" + this.text.substring(b, d) + "]" : " " + d;
            throw Ua("lexerr", a, b, this.text);
        },
        readNumber: function() {
            for (var a = "", b = this.index; this.index < this.text.length;) {
                var d = P(this.text.charAt(this.index));
                if ("." === d || this.isNumber(d)) a += d;
                else {
                    var c = this.peek();
                    if ("e" === d && this.isExpOperator(c)) a += d;
                    else if (this.isExpOperator(d) && c && this.isNumber(c) && "e" === a.charAt(a.length - 1)) a += d;
                    else if (!this.isExpOperator(d) || c && this.isNumber(c) || "e" !== a.charAt(a.length - 1)) break;
                    else this.throwError("Invalid exponent")
                }
                this.index++
            }
            this.tokens.push({
                index: b,
                text: a,
                constant: !0,
                value: Number(a)
            })
        },
        readIdent: function() {
            var a = this.index;
            for (this.index += this.peekMultichar().length; this.index < this.text.length;) {
                var b =
                    this.peekMultichar();
                if (!this.isIdentifierContinue(b)) break;
                this.index += b.length
            }
            this.tokens.push({
                index: a,
                text: this.text.slice(a, this.index),
                identifier: !0
            })
        },
        readString: function(a) {
            var b = this.index;
            this.index++;
            for (var d = "", c = a, e = !1; this.index < this.text.length;) {
                var f = this.text.charAt(this.index),
                    c = c + f;
                if (e) "u" === f ? (e = this.text.substring(this.index + 1, this.index + 5), e.match(/[\da-f]{4}/i) || this.throwError("Invalid unicode escape [\\u" + e + "]"), this.index += 4, d += String.fromCharCode(parseInt(e, 16))) : d +=
                    $g[f] || f, e = !1;
                else if ("\\" === f) e = !0;
                else {
                    if (f === a) {
                        this.index++;
                        this.tokens.push({
                            index: b,
                            text: c,
                            constant: !0,
                            value: d
                        });
                        return
                    }
                    d += f
                }
                this.index++
            }
            this.throwError("Unterminated quote", b)
        }
    };
    var s = function(a, b) {
        this.lexer = a;
        this.options = b
    };
    s.Program = "Program";
    s.ExpressionStatement = "ExpressionStatement";
    s.AssignmentExpression = "AssignmentExpression";
    s.ConditionalExpression = "ConditionalExpression";
    s.LogicalExpression = "LogicalExpression";
    s.BinaryExpression = "BinaryExpression";
    s.UnaryExpression = "UnaryExpression";
    s.CallExpression = "CallExpression";
    s.MemberExpression = "MemberExpression";
    s.Identifier = "Identifier";
    s.Literal = "Literal";
    s.ArrayExpression = "ArrayExpression";
    s.Property = "Property";
    s.ObjectExpression = "ObjectExpression";
    s.ThisExpression = "ThisExpression";
    s.LocalsExpression = "LocalsExpression";
    s.NGValueParameter = "NGValueParameter";
    s.prototype = {
        ast: function(a) {
            this.text = a;
            this.tokens = this.lexer.lex(a);
            a = this.program();
            0 !== this.tokens.length && this.throwError("is an unexpected token", this.tokens[0]);
            return a
        },
        program: function() {
            for (var a = [];;)
                if (0 < this.tokens.length && !this.peek("}", ")", ";", "]") && a.push(this.expressionStatement()), !this.expect(";")) return {
                    type: s.Program,
                    body: a
                }
        },
        expressionStatement: function() {
            return {
                type: s.ExpressionStatement,
                expression: this.filterChain()
            }
        },
        filterChain: function() {
            for (var a = this.expression(); this.expect("|");) a = this.filter(a);
            return a
        },
        expression: function() {
            return this.assignment()
        },
        assignment: function() {
            var a = this.ternary();
            if (this.expect("=")) {
                if (!Ed(a)) throw Ua("lval");
                a = {
                    type: s.AssignmentExpression,
                    left: a,
                    right: this.assignment(),
                    operator: "="
                }
            }
            return a
        },
        ternary: function() {
            var a = this.logicalOR(),
                b, d;
            return this.expect("?") && (b = this.expression(), this.consume(":")) ? (d = this.expression(), {
                type: s.ConditionalExpression,
                test: a,
                alternate: b,
                consequent: d
            }) : a
        },
        logicalOR: function() {
            for (var a = this.logicalAND(); this.expect("||");) a = {
                type: s.LogicalExpression,
                operator: "||",
                left: a,
                right: this.logicalAND()
            };
            return a
        },
        logicalAND: function() {
            for (var a = this.equality(); this.expect("&&");) a = {
                type: s.LogicalExpression,
                operator: "&&",
                left: a,
                right: this.equality()
            };
            return a
        },
        equality: function() {
            for (var a = this.relational(), b; b = this.expect("==", "!=", "===", "!==");) a = {
                type: s.BinaryExpression,
                operator: b.text,
                left: a,
                right: this.relational()
            };
            return a
        },
        relational: function() {
            for (var a = this.additive(), b; b = this.expect("<", ">", "<=", ">=");) a = {
                type: s.BinaryExpression,
                operator: b.text,
                left: a,
                right: this.additive()
            };
            return a
        },
        additive: function() {
            for (var a = this.multiplicative(), b; b = this.expect("+", "-");) a = {
                type: s.BinaryExpression,
                operator: b.text,
                left: a,
                right: this.multiplicative()
            };
            return a
        },
        multiplicative: function() {
            for (var a = this.unary(), b; b = this.expect("*", "/", "%");) a = {
                type: s.BinaryExpression,
                operator: b.text,
                left: a,
                right: this.unary()
            };
            return a
        },
        unary: function() {
            var a;
            return (a = this.expect("+", "-", "!")) ? {
                type: s.UnaryExpression,
                operator: a.text,
                prefix: !0,
                argument: this.unary()
            } : this.primary()
        },
        primary: function() {
            var a;
            this.expect("(") ? (a = this.filterChain(), this.consume(")")) : this.expect("[") ? a = this.arrayDeclaration() : this.expect("{") ?
                a = this.object() : this.selfReferential.hasOwnProperty(this.peek().text) ? a = sa(this.selfReferential[this.consume().text]) : this.options.literals.hasOwnProperty(this.peek().text) ? a = {
                    type: s.Literal,
                    value: this.options.literals[this.consume().text]
                } : this.peek().identifier ? a = this.identifier() : this.peek().constant ? a = this.constant() : this.throwError("not a primary expression", this.peek());
            for (var b; b = this.expect("(", "[", ".");) "(" === b.text ? (a = {
                    type: s.CallExpression,
                    callee: a,
                    arguments: this.parseArguments()
                }, this.consume(")")) :
                "[" === b.text ? (a = {
                    type: s.MemberExpression,
                    object: a,
                    property: this.expression(),
                    computed: !0
                }, this.consume("]")) : "." === b.text ? a = {
                    type: s.MemberExpression,
                    object: a,
                    property: this.identifier(),
                    computed: !1
                } : this.throwError("IMPOSSIBLE");
            return a
        },
        filter: function(a) {
            a = [a];
            for (var b = {
                    type: s.CallExpression,
                    callee: this.identifier(),
                    arguments: a,
                    filter: !0
                }; this.expect(":");) a.push(this.expression());
            return b
        },
        parseArguments: function() {
            var a = [];
            if (")" !== this.peekToken().text) {
                do a.push(this.filterChain()); while (this.expect(","))
            }
            return a
        },
        identifier: function() {
            var a = this.consume();
            a.identifier || this.throwError("is not a valid identifier", a);
            return {
                type: s.Identifier,
                name: a.text
            }
        },
        constant: function() {
            return {
                type: s.Literal,
                value: this.consume().value
            }
        },
        arrayDeclaration: function() {
            var a = [];
            if ("]" !== this.peekToken().text) {
                do {
                    if (this.peek("]")) break;
                    a.push(this.expression())
                } while (this.expect(","))
            }
            this.consume("]");
            return {
                type: s.ArrayExpression,
                elements: a
            }
        },
        object: function() {
            var a = [],
                b;
            if ("}" !== this.peekToken().text) {
                do {
                    if (this.peek("}")) break;
                    b = {
                        type: s.Property,
                        kind: "init"
                    };
                    this.peek().constant ? (b.key = this.constant(), b.computed = !1, this.consume(":"), b.value = this.expression()) : this.peek().identifier ? (b.key = this.identifier(), b.computed = !1, this.peek(":") ? (this.consume(":"), b.value = this.expression()) : b.value = b.key) : this.peek("[") ? (this.consume("["), b.key = this.expression(), this.consume("]"), b.computed = !0, this.consume(":"), b.value = this.expression()) : this.throwError("invalid key", this.peek());
                    a.push(b)
                } while (this.expect(","))
            }
            this.consume("}");
            return {
                type: s.ObjectExpression,
                properties: a
            }
        },
        throwError: function(a, b) {
            throw Ua("syntax", b.text, a, b.index + 1, this.text, this.text.substring(b.index));
        },
        consume: function(a) {
            if (0 === this.tokens.length) throw Ua("ueoe", this.text);
            var b = this.expect(a);
            b || this.throwError("is unexpected, expecting [" + a + "]", this.peek());
            return b
        },
        peekToken: function() {
            if (0 === this.tokens.length) throw Ua("ueoe", this.text);
            return this.tokens[0]
        },
        peek: function(a, b, d, c) {
            return this.peekAhead(0, a, b, d, c)
        },
        peekAhead: function(a, b, d, c,
            e) {
            if (this.tokens.length > a) {
                a = this.tokens[a];
                var f = a.text;
                if (f === b || f === d || f === c || f === e || !(b || d || c || e)) return a
            }
            return !1
        },
        expect: function(a, b, d, c) {
            return (a = this.peek(a, b, d, c)) ? (this.tokens.shift(), a) : !1
        },
        selfReferential: {
            "this": {
                type: s.ThisExpression
            },
            $locals: {
                type: s.LocalsExpression
            }
        }
    };
    Hd.prototype = {
        compile: function(a) {
            var b = this;
            a = this.astBuilder.ast(a);
            this.state = {
                nextId: 0,
                filters: {},
                fn: {
                    vars: [],
                    body: [],
                    own: {}
                },
                assign: {
                    vars: [],
                    body: [],
                    own: {}
                },
                inputs: []
            };
            U(a, b.$filter);
            var d = "",
                c;
            this.stage = "assign";
            if (c = Fd(a)) this.state.computing = "assign", d = this.nextId(), this.recurse(c, d), this.return_(d), d = "fn.assign=" + this.generateFunction("assign", "s,v,l");
            c = Dd(a.body);
            b.stage = "inputs";
            p(c, function(a, c) {
                var d = "fn" + c;
                b.state[d] = {
                    vars: [],
                    body: [],
                    own: {}
                };
                b.state.computing = d;
                var h = b.nextId();
                b.recurse(a, h);
                b.return_(h);
                b.state.inputs.push(d);
                a.watchId = c
            });
            this.state.computing = "fn";
            this.stage = "main";
            this.recurse(a);
            d = '"' + this.USE + " " + this.STRICT + '";\n' + this.filterPrefix() + "var fn=" + this.generateFunction("fn", "s,l,a,i") +
                d + this.watchFns() + "return fn;";
            d = (new Function("$filter", "getStringValue", "ifDefined", "plus", d))(this.$filter, zg, Ag, Cd);
            this.state = this.stage = void 0;
            d.literal = Gd(a);
            d.constant = a.constant;
            return d
        },
        USE: "use",
        STRICT: "strict",
        watchFns: function() {
            var a = [],
                b = this.state.inputs,
                d = this;
            p(b, function(b) {
                a.push("var " + b + "=" + d.generateFunction(b, "s"))
            });
            b.length && a.push("fn.inputs=[" + b.join(",") + "];");
            return a.join("")
        },
        generateFunction: function(a, b) {
            return "function(" + b + "){" + this.varsPrefix(a) + this.body(a) + "};"
        },
        filterPrefix: function() {
            var a = [],
                b = this;
            p(this.state.filters, function(d, c) {
                a.push(d + "=$filter(" + b.escape(c) + ")")
            });
            return a.length ? "var " + a.join(",") + ";" : ""
        },
        varsPrefix: function(a) {
            return this.state[a].vars.length ? "var " + this.state[a].vars.join(",") + ";" : ""
        },
        body: function(a) {
            return this.state[a].body.join("")
        },
        recurse: function(a, b, d, c, e, f) {
            var g, h, k = this,
                l, m, n;
            c = c || A;
            if (!f && u(a.watchId)) b = b || this.nextId(), this.if_("i", this.lazyAssign(b, this.computedMember("i", a.watchId)), this.lazyRecurse(a, b, d, c, e,
                !0));
            else switch (a.type) {
                case s.Program:
                    p(a.body, function(b, c) {
                        k.recurse(b.expression, void 0, void 0, function(a) {
                            h = a
                        });
                        c !== a.body.length - 1 ? k.current().body.push(h, ";") : k.return_(h)
                    });
                    break;
                case s.Literal:
                    m = this.escape(a.value);
                    this.assign(b, m);
                    c(b || m);
                    break;
                case s.UnaryExpression:
                    this.recurse(a.argument, void 0, void 0, function(a) {
                        h = a
                    });
                    m = a.operator + "(" + this.ifDefined(h, 0) + ")";
                    this.assign(b, m);
                    c(m);
                    break;
                case s.BinaryExpression:
                    this.recurse(a.left, void 0, void 0, function(a) {
                        g = a
                    });
                    this.recurse(a.right,
                        void 0, void 0,
                        function(a) {
                            h = a
                        });
                    m = "+" === a.operator ? this.plus(g, h) : "-" === a.operator ? this.ifDefined(g, 0) + a.operator + this.ifDefined(h, 0) : "(" + g + ")" + a.operator + "(" + h + ")";
                    this.assign(b, m);
                    c(m);
                    break;
                case s.LogicalExpression:
                    b = b || this.nextId();
                    k.recurse(a.left, b);
                    k.if_("&&" === a.operator ? b : k.not(b), k.lazyRecurse(a.right, b));
                    c(b);
                    break;
                case s.ConditionalExpression:
                    b = b || this.nextId();
                    k.recurse(a.test, b);
                    k.if_(b, k.lazyRecurse(a.alternate, b), k.lazyRecurse(a.consequent, b));
                    c(b);
                    break;
                case s.Identifier:
                    b = b || this.nextId();
                    d && (d.context = "inputs" === k.stage ? "s" : this.assign(this.nextId(), this.getHasOwnProperty("l", a.name) + "?l:s"), d.computed = !1, d.name = a.name);
                    k.if_("inputs" === k.stage || k.not(k.getHasOwnProperty("l", a.name)), function() {
                        k.if_("inputs" === k.stage || "s", function() {
                            e && 1 !== e && k.if_(k.isNull(k.nonComputedMember("s", a.name)), k.lazyAssign(k.nonComputedMember("s", a.name), "{}"));
                            k.assign(b, k.nonComputedMember("s", a.name))
                        })
                    }, b && k.lazyAssign(b, k.nonComputedMember("l", a.name)));
                    c(b);
                    break;
                case s.MemberExpression:
                    g = d &&
                        (d.context = this.nextId()) || this.nextId();
                    b = b || this.nextId();
                    k.recurse(a.object, g, void 0, function() {
                        k.if_(k.notNull(g), function() {
                            a.computed ? (h = k.nextId(), k.recurse(a.property, h), k.getStringValue(h), e && 1 !== e && k.if_(k.not(k.computedMember(g, h)), k.lazyAssign(k.computedMember(g, h), "{}")), m = k.computedMember(g, h), k.assign(b, m), d && (d.computed = !0, d.name = h)) : (e && 1 !== e && k.if_(k.isNull(k.nonComputedMember(g, a.property.name)), k.lazyAssign(k.nonComputedMember(g, a.property.name), "{}")), m = k.nonComputedMember(g,
                                a.property.name), k.assign(b, m), d && (d.computed = !1, d.name = a.property.name))
                        }, function() {
                            k.assign(b, "undefined")
                        });
                        c(b)
                    }, !!e);
                    break;
                case s.CallExpression:
                    b = b || this.nextId();
                    a.filter ? (h = k.filter(a.callee.name), l = [], p(a.arguments, function(a) {
                        var b = k.nextId();
                        k.recurse(a, b);
                        l.push(b)
                    }), m = h + "(" + l.join(",") + ")", k.assign(b, m), c(b)) : (h = k.nextId(), g = {}, l = [], k.recurse(a.callee, h, g, function() {
                        k.if_(k.notNull(h), function() {
                            p(a.arguments, function(b) {
                                k.recurse(b, a.constant ? void 0 : k.nextId(), void 0, function(a) {
                                    l.push(a)
                                })
                            });
                            m = g.name ? k.member(g.context, g.name, g.computed) + "(" + l.join(",") + ")" : h + "(" + l.join(",") + ")";
                            k.assign(b, m)
                        }, function() {
                            k.assign(b, "undefined")
                        });
                        c(b)
                    }));
                    break;
                case s.AssignmentExpression:
                    h = this.nextId();
                    g = {};
                    this.recurse(a.left, void 0, g, function() {
                        k.if_(k.notNull(g.context), function() {
                            k.recurse(a.right, h);
                            m = k.member(g.context, g.name, g.computed) + a.operator + h;
                            k.assign(b, m);
                            c(b || m)
                        })
                    }, 1);
                    break;
                case s.ArrayExpression:
                    l = [];
                    p(a.elements, function(b) {
                        k.recurse(b, a.constant ? void 0 : k.nextId(), void 0, function(a) {
                            l.push(a)
                        })
                    });
                    m = "[" + l.join(",") + "]";
                    this.assign(b, m);
                    c(b || m);
                    break;
                case s.ObjectExpression:
                    l = [];
                    n = !1;
                    p(a.properties, function(a) {
                        a.computed && (n = !0)
                    });
                    n ? (b = b || this.nextId(), this.assign(b, "{}"), p(a.properties, function(a) {
                        a.computed ? (g = k.nextId(), k.recurse(a.key, g)) : g = a.key.type === s.Identifier ? a.key.name : "" + a.key.value;
                        h = k.nextId();
                        k.recurse(a.value, h);
                        k.assign(k.member(b, g, a.computed), h)
                    })) : (p(a.properties, function(b) {
                        k.recurse(b.value, a.constant ? void 0 : k.nextId(), void 0, function(a) {
                            l.push(k.escape(b.key.type === s.Identifier ?
                                b.key.name : "" + b.key.value) + ":" + a)
                        })
                    }), m = "{" + l.join(",") + "}", this.assign(b, m));
                    c(b || m);
                    break;
                case s.ThisExpression:
                    this.assign(b, "s");
                    c(b || "s");
                    break;
                case s.LocalsExpression:
                    this.assign(b, "l");
                    c(b || "l");
                    break;
                case s.NGValueParameter:
                    this.assign(b, "v"), c(b || "v")
            }
        },
        getHasOwnProperty: function(a, b) {
            var d = a + "." + b,
                c = this.current().own;
            c.hasOwnProperty(d) || (c[d] = this.nextId(!1, a + "&&(" + this.escape(b) + " in " + a + ")"));
            return c[d]
        },
        assign: function(a, b) {
            if (a) return this.current().body.push(a, "=", b, ";"), a
        },
        filter: function(a) {
            this.state.filters.hasOwnProperty(a) ||
                (this.state.filters[a] = this.nextId(!0));
            return this.state.filters[a]
        },
        ifDefined: function(a, b) {
            return "ifDefined(" + a + "," + this.escape(b) + ")"
        },
        plus: function(a, b) {
            return "plus(" + a + "," + b + ")"
        },
        return_: function(a) {
            this.current().body.push("return ", a, ";")
        },
        if_: function(a, b, d) {
            if (!0 === a) b();
            else {
                var c = this.current().body;
                c.push("if(", a, "){");
                b();
                c.push("}");
                d && (c.push("else{"), d(), c.push("}"))
            }
        },
        not: function(a) {
            return "!(" + a + ")"
        },
        isNull: function(a) {
            return a + "==null"
        },
        notNull: function(a) {
            return a + "!=null"
        },
        nonComputedMember: function(a,
            b) {
            var d = /[^$_a-zA-Z0-9]/g;
            return /^[$_a-zA-Z][$_a-zA-Z0-9]*$/.test(b) ? a + "." + b : a + '["' + b.replace(d, this.stringEscapeFn) + '"]'
        },
        computedMember: function(a, b) {
            return a + "[" + b + "]"
        },
        member: function(a, b, d) {
            return d ? this.computedMember(a, b) : this.nonComputedMember(a, b)
        },
        getStringValue: function(a) {
            this.assign(a, "getStringValue(" + a + ")")
        },
        lazyRecurse: function(a, b, d, c, e, f) {
            var g = this;
            return function() {
                g.recurse(a, b, d, c, e, f)
            }
        },
        lazyAssign: function(a, b) {
            var d = this;
            return function() {
                d.assign(a, b)
            }
        },
        stringEscapeRegex: /[^ a-zA-Z0-9]/g,
        stringEscapeFn: function(a) {
            return "\\u" + ("0000" + a.charCodeAt(0).toString(16)).slice(-4)
        },
        escape: function(a) {
            if (D(a)) return "'" + a.replace(this.stringEscapeRegex, this.stringEscapeFn) + "'";
            if (ba(a)) return a.toString();
            if (!0 === a) return "true";
            if (!1 === a) return "false";
            if (null === a) return "null";
            if ("undefined" === typeof a) return "undefined";
            throw Ua("esc");
        },
        nextId: function(a, b) {
            var d = "v" + this.state.nextId++;
            a || this.current().vars.push(d + (b ? "=" + b : ""));
            return d
        },
        current: function() {
            return this.state[this.state.computing]
        }
    };
    Id.prototype = {
        compile: function(a) {
            var b = this;
            a = this.astBuilder.ast(a);
            U(a, b.$filter);
            var d, c;
            if (d = Fd(a)) c = this.recurse(d);
            d = Dd(a.body);
            var e;
            d && (e = [], p(d, function(a, c) {
                var d = b.recurse(a);
                a.input = d;
                e.push(d);
                a.watchId = c
            }));
            var f = [];
            p(a.body, function(a) {
                f.push(b.recurse(a.expression))
            });
            d = 0 === a.body.length ? A : 1 === a.body.length ? f[0] : function(a, b) {
                var c;
                p(f, function(d) {
                    c = d(a, b)
                });
                return c
            };
            c && (d.assign = function(a, b, d) {
                return c(a, d, b)
            });
            e && (d.inputs = e);
            d.literal = Gd(a);
            d.constant = a.constant;
            return d
        },
        recurse: function(a,
            b, d) {
            var c, e, f = this,
                g;
            if (a.input) return this.inputs(a.input, a.watchId);
            switch (a.type) {
                case s.Literal:
                    return this.value(a.value, b);
                case s.UnaryExpression:
                    return e = this.recurse(a.argument), this["unary" + a.operator](e, b);
                case s.BinaryExpression:
                    return c = this.recurse(a.left), e = this.recurse(a.right), this["binary" + a.operator](c, e, b);
                case s.LogicalExpression:
                    return c = this.recurse(a.left), e = this.recurse(a.right), this["binary" + a.operator](c, e, b);
                case s.ConditionalExpression:
                    return this["ternary?:"](this.recurse(a.test),
                        this.recurse(a.alternate), this.recurse(a.consequent), b);
                case s.Identifier:
                    return f.identifier(a.name, b, d);
                case s.MemberExpression:
                    return c = this.recurse(a.object, !1, !!d), a.computed || (e = a.property.name), a.computed && (e = this.recurse(a.property)), a.computed ? this.computedMember(c, e, b, d) : this.nonComputedMember(c, e, b, d);
                case s.CallExpression:
                    return g = [], p(a.arguments, function(a) {
                        g.push(f.recurse(a))
                    }), a.filter && (e = this.$filter(a.callee.name)), a.filter || (e = this.recurse(a.callee, !0)), a.filter ? function(a, c,
                        d, f) {
                        for (var n = [], q = 0; q < g.length; ++q) n.push(g[q](a, c, d, f));
                        a = e.apply(void 0, n, f);
                        return b ? {
                            context: void 0,
                            name: void 0,
                            value: a
                        } : a
                    } : function(a, c, d, f) {
                        var n = e(a, c, d, f),
                            q;
                        if (null != n.value) {
                            q = [];
                            for (var r = 0; r < g.length; ++r) q.push(g[r](a, c, d, f));
                            q = n.value.apply(n.context, q)
                        }
                        return b ? {
                            value: q
                        } : q
                    };
                case s.AssignmentExpression:
                    return c = this.recurse(a.left, !0, 1), e = this.recurse(a.right),
                        function(a, d, f, g) {
                            var n = c(a, d, f, g);
                            a = e(a, d, f, g);
                            n.context[n.name] = a;
                            return b ? {
                                value: a
                            } : a
                        };
                case s.ArrayExpression:
                    return g = [],
                        p(a.elements, function(a) {
                            g.push(f.recurse(a))
                        }),
                        function(a, c, d, e) {
                            for (var f = [], q = 0; q < g.length; ++q) f.push(g[q](a, c, d, e));
                            return b ? {
                                value: f
                            } : f
                        };
                case s.ObjectExpression:
                    return g = [], p(a.properties, function(a) {
                            a.computed ? g.push({
                                key: f.recurse(a.key),
                                computed: !0,
                                value: f.recurse(a.value)
                            }) : g.push({
                                key: a.key.type === s.Identifier ? a.key.name : "" + a.key.value,
                                computed: !1,
                                value: f.recurse(a.value)
                            })
                        }),
                        function(a, c, d, e) {
                            for (var f = {}, q = 0; q < g.length; ++q) g[q].computed ? f[g[q].key(a, c, d, e)] = g[q].value(a, c, d, e) : f[g[q].key] =
                                g[q].value(a, c, d, e);
                            return b ? {
                                value: f
                            } : f
                        };
                case s.ThisExpression:
                    return function(a) {
                        return b ? {
                            value: a
                        } : a
                    };
                case s.LocalsExpression:
                    return function(a, c) {
                        return b ? {
                            value: c
                        } : c
                    };
                case s.NGValueParameter:
                    return function(a, c, d) {
                        return b ? {
                            value: d
                        } : d
                    }
            }
        },
        "unary+": function(a, b) {
            return function(d, c, e, f) {
                d = a(d, c, e, f);
                d = u(d) ? +d : 0;
                return b ? {
                    value: d
                } : d
            }
        },
        "unary-": function(a, b) {
            return function(d, c, e, f) {
                d = a(d, c, e, f);
                d = u(d) ? -d : -0;
                return b ? {
                    value: d
                } : d
            }
        },
        "unary!": function(a, b) {
            return function(d, c, e, f) {
                d = !a(d, c, e, f);
                return b ? {
                    value: d
                } : d
            }
        },
        "binary+": function(a, b, d) {
            return function(c, e, f, g) {
                var h = a(c, e, f, g);
                c = b(c, e, f, g);
                h = Cd(h, c);
                return d ? {
                    value: h
                } : h
            }
        },
        "binary-": function(a, b, d) {
            return function(c, e, f, g) {
                var h = a(c, e, f, g);
                c = b(c, e, f, g);
                h = (u(h) ? h : 0) - (u(c) ? c : 0);
                return d ? {
                    value: h
                } : h
            }
        },
        "binary*": function(a, b, d) {
            return function(c, e, f, g) {
                c = a(c, e, f, g) * b(c, e, f, g);
                return d ? {
                    value: c
                } : c
            }
        },
        "binary/": function(a, b, d) {
            return function(c, e, f, g) {
                c = a(c, e, f, g) / b(c, e, f, g);
                return d ? {
                    value: c
                } : c
            }
        },
        "binary%": function(a, b, d) {
            return function(c, e, f, g) {
                c =
                    a(c, e, f, g) % b(c, e, f, g);
                return d ? {
                    value: c
                } : c
            }
        },
        "binary===": function(a, b, d) {
            return function(c, e, f, g) {
                c = a(c, e, f, g) === b(c, e, f, g);
                return d ? {
                    value: c
                } : c
            }
        },
        "binary!==": function(a, b, d) {
            return function(c, e, f, g) {
                c = a(c, e, f, g) !== b(c, e, f, g);
                return d ? {
                    value: c
                } : c
            }
        },
        "binary==": function(a, b, d) {
            return function(c, e, f, g) {
                c = a(c, e, f, g) == b(c, e, f, g);
                return d ? {
                    value: c
                } : c
            }
        },
        "binary!=": function(a, b, d) {
            return function(c, e, f, g) {
                c = a(c, e, f, g) != b(c, e, f, g);
                return d ? {
                    value: c
                } : c
            }
        },
        "binary<": function(a, b, d) {
            return function(c, e, f, g) {
                c =
                    a(c, e, f, g) < b(c, e, f, g);
                return d ? {
                    value: c
                } : c
            }
        },
        "binary>": function(a, b, d) {
            return function(c, e, f, g) {
                c = a(c, e, f, g) > b(c, e, f, g);
                return d ? {
                    value: c
                } : c
            }
        },
        "binary<=": function(a, b, d) {
            return function(c, e, f, g) {
                c = a(c, e, f, g) <= b(c, e, f, g);
                return d ? {
                    value: c
                } : c
            }
        },
        "binary>=": function(a, b, d) {
            return function(c, e, f, g) {
                c = a(c, e, f, g) >= b(c, e, f, g);
                return d ? {
                    value: c
                } : c
            }
        },
        "binary&&": function(a, b, d) {
            return function(c, e, f, g) {
                c = a(c, e, f, g) && b(c, e, f, g);
                return d ? {
                    value: c
                } : c
            }
        },
        "binary||": function(a, b, d) {
            return function(c, e, f, g) {
                c = a(c,
                    e, f, g) || b(c, e, f, g);
                return d ? {
                    value: c
                } : c
            }
        },
        "ternary?:": function(a, b, d, c) {
            return function(e, f, g, h) {
                e = a(e, f, g, h) ? b(e, f, g, h) : d(e, f, g, h);
                return c ? {
                    value: e
                } : e
            }
        },
        value: function(a, b) {
            return function() {
                return b ? {
                    context: void 0,
                    name: void 0,
                    value: a
                } : a
            }
        },
        identifier: function(a, b, d) {
            return function(c, e, f, g) {
                c = e && a in e ? e : c;
                d && 1 !== d && c && null == c[a] && (c[a] = {});
                e = c ? c[a] : void 0;
                return b ? {
                    context: c,
                    name: a,
                    value: e
                } : e
            }
        },
        computedMember: function(a, b, d, c) {
            return function(e, f, g, h) {
                var k = a(e, f, g, h),
                    l, m;
                null != k && (l = b(e, f, g,
                    h), l += "", c && 1 !== c && k && !k[l] && (k[l] = {}), m = k[l]);
                return d ? {
                    context: k,
                    name: l,
                    value: m
                } : m
            }
        },
        nonComputedMember: function(a, b, d, c) {
            return function(e, f, g, h) {
                e = a(e, f, g, h);
                c && 1 !== c && e && null == e[b] && (e[b] = {});
                f = null != e ? e[b] : void 0;
                return d ? {
                    context: e,
                    name: b,
                    value: f
                } : f
            }
        },
        inputs: function(a, b) {
            return function(d, c, e, f) {
                return f ? f[b] : a(d, c, e)
            }
        }
    };
    var tc = function(a, b, d) {
        this.lexer = a;
        this.$filter = b;
        this.options = d;
        this.ast = new s(a, d);
        this.astCompiler = d.csp ? new Id(this.ast, b) : new Hd(this.ast, b)
    };
    tc.prototype = {
        constructor: tc,
        parse: function(a) {
            return this.astCompiler.compile(a)
        }
    };
    var ta = M("$sce"),
        oa = {
            HTML: "html",
            CSS: "css",
            URL: "url",
            RESOURCE_URL: "resourceUrl",
            JS: "js"
        },
        uc = /_([a-z])/g,
        Dg = M("$compile"),
        aa = w.document.createElement("a"),
        Md = Ca(w.location.href);
    Nd.$inject = ["$document"];
    $c.$inject = ["$provide"];
    var Ud = 22,
        Td = ".",
        wc = "0";
    Od.$inject = ["$locale"];
    Qd.$inject = ["$locale"];
    var Og = {
            yyyy: Y("FullYear", 4, 0, !1, !0),
            yy: Y("FullYear", 2, 0, !0, !0),
            y: Y("FullYear", 1, 0, !1, !0),
            MMMM: nb("Month"),
            MMM: nb("Month", !0),
            MM: Y("Month", 2, 1),
            M: Y("Month",
                1, 1),
            LLLL: nb("Month", !1, !0),
            dd: Y("Date", 2),
            d: Y("Date", 1),
            HH: Y("Hours", 2),
            H: Y("Hours", 1),
            hh: Y("Hours", 2, -12),
            h: Y("Hours", 1, -12),
            mm: Y("Minutes", 2),
            m: Y("Minutes", 1),
            ss: Y("Seconds", 2),
            s: Y("Seconds", 1),
            sss: Y("Milliseconds", 3),
            EEEE: nb("Day"),
            EEE: nb("Day", !0),
            a: function(a, b) {
                return 12 > a.getHours() ? b.AMPMS[0] : b.AMPMS[1]
            },
            Z: function(a, b, d) {
                a = -1 * d;
                return a = (0 <= a ? "+" : "") + (Lb(Math[0 < a ? "floor" : "ceil"](a / 60), 2) + Lb(Math.abs(a % 60), 2))
            },
            ww: Wd(2),
            w: Wd(1),
            G: xc,
            GG: xc,
            GGG: xc,
            GGGG: function(a, b) {
                return 0 >= a.getFullYear() ?
                    b.ERANAMES[0] : b.ERANAMES[1]
            }
        },
        Ng = /((?:[^yMLdHhmsaZEwG']+)|(?:'(?:[^']|'')*')|(?:E+|y+|M+|L+|d+|H+|h+|m+|s+|a|Z|G+|w+))(.*)/,
        Mg = /^-?\d+$/;
    Pd.$inject = ["$locale"];
    var Hg = la(P),
        Ig = la(vb);
    Rd.$inject = ["$parse"];
    var Fe = la({
            restrict: "E",
            compile: function(a, b) {
                if (!b.href && !b.xlinkHref) return function(a, b) {
                    if ("a" === b[0].nodeName.toLowerCase()) {
                        var e = "[object SVGAnimatedString]" === ma.call(b.prop("href")) ? "xlink:href" : "href";
                        b.on("click", function(a) {
                            b.attr(e) || a.preventDefault()
                        })
                    }
                }
            }
        }),
        wb = {};
    p(Gb, function(a, b) {
        function d(a,
            d, e) {
            a.$watch(e[c], function(a) {
                e.$set(b, !!a)
            })
        }
        if ("multiple" !== a) {
            var c = Ba("ng-" + b),
                e = d;
            "checked" === a && (e = function(a, b, e) {
                e.ngModel !== e[c] && d(a, b, e)
            });
            wb[c] = function() {
                return {
                    restrict: "A",
                    priority: 100,
                    link: e
                }
            }
        }
    });
    p(pd, function(a, b) {
        wb[b] = function() {
            return {
                priority: 100,
                link: function(a, c, e) {
                    if ("ngPattern" === b && "/" === e.ngPattern.charAt(0) && (c = e.ngPattern.match(Sg))) {
                        e.$set("ngPattern", new RegExp(c[1], c[2]));
                        return
                    }
                    a.$watch(e[b], function(a) {
                        e.$set(b, a)
                    })
                }
            }
        }
    });
    p(["src", "srcset", "href"], function(a) {
        var b =
            Ba("ng-" + a);
        wb[b] = function() {
            return {
                priority: 99,
                link: function(d, c, e) {
                    var f = a,
                        g = a;
                    "href" === a && "[object SVGAnimatedString]" === ma.call(c.prop("href")) && (g = "xlinkHref", e.$attr[g] = "xlink:href", f = null);
                    e.$observe(b, function(b) {
                        b ? (e.$set(g, b), za && f && c.prop(f, e[g])) : "href" === a && e.$set(g, null)
                    })
                }
            }
        }
    });
    var Nb = {
        $addControl: A,
        $$renameControl: function(a, b) {
            a.$name = b
        },
        $removeControl: A,
        $setValidity: A,
        $setDirty: A,
        $setPristine: A,
        $setSubmitted: A
    };
    Mb.$inject = ["$element", "$attrs", "$scope", "$animate", "$interpolate"];
    Mb.prototype = {
        $rollbackViewValue: function() {
            p(this.$$controls, function(a) {
                a.$rollbackViewValue()
            })
        },
        $commitViewValue: function() {
            p(this.$$controls, function(a) {
                a.$commitViewValue()
            })
        },
        $addControl: function(a) {
            Ka(a.$name, "input");
            this.$$controls.push(a);
            a.$name && (this[a.$name] = a);
            a.$$parentForm = this
        },
        $$renameControl: function(a, b) {
            var d = a.$name;
            this[d] === a && delete this[d];
            this[b] = a;
            a.$name = b
        },
        $removeControl: function(a) {
            a.$name && this[a.$name] === a && delete this[a.$name];
            p(this.$pending, function(b, d) {
                this.$setValidity(d,
                    null, a)
            }, this);
            p(this.$error, function(b, d) {
                this.$setValidity(d, null, a)
            }, this);
            p(this.$$success, function(b, d) {
                this.$setValidity(d, null, a)
            }, this);
            $a(this.$$controls, a);
            a.$$parentForm = Nb
        },
        $setDirty: function() {
            this.$$animate.removeClass(this.$$element, Va);
            this.$$animate.addClass(this.$$element, Sb);
            this.$dirty = !0;
            this.$pristine = !1;
            this.$$parentForm.$setDirty()
        },
        $setPristine: function() {
            this.$$animate.setClass(this.$$element, Va, Sb + " ng-submitted");
            this.$dirty = !1;
            this.$pristine = !0;
            this.$submitted = !1;
            p(this.$$controls,
                function(a) {
                    a.$setPristine()
                })
        },
        $setUntouched: function() {
            p(this.$$controls, function(a) {
                a.$setUntouched()
            })
        },
        $setSubmitted: function() {
            this.$$animate.addClass(this.$$element, "ng-submitted");
            this.$submitted = !0;
            this.$$parentForm.$setSubmitted()
        }
    };
    Zd({
        clazz: Mb,
        set: function(a, b, d) {
            var c = a[b];
            c ? -1 === c.indexOf(d) && c.push(d) : a[b] = [d]
        },
        unset: function(a, b, d) {
            var c = a[b];
            c && ($a(c, d), 0 === c.length && delete a[b])
        }
    });
    var ge = function(a) {
            return ["$timeout", "$parse", function(b, d) {
                function c(a) {
                    return "" === a ? d('this[""]').assign :
                        d(a).assign || A
                }
                return {
                    name: "form",
                    restrict: a ? "EAC" : "E",
                    require: ["form", "^^?form"],
                    controller: Mb,
                    compile: function(d, f) {
                        d.addClass(Va).addClass(ob);
                        var g = f.name ? "name" : a && f.ngForm ? "ngForm" : !1;
                        return {
                            pre: function(a, d, e, f) {
                                var n = f[0];
                                if (!("action" in e)) {
                                    var q = function(b) {
                                        a.$apply(function() {
                                            n.$commitViewValue();
                                            n.$setSubmitted()
                                        });
                                        b.preventDefault()
                                    };
                                    d[0].addEventListener("submit", q);
                                    d.on("$destroy", function() {
                                        b(function() {
                                            d[0].removeEventListener("submit", q)
                                        }, 0, !1)
                                    })
                                }(f[1] || n.$$parentForm).$addControl(n);
                                var r = g ? c(n.$name) : A;
                                g && (r(a, n), e.$observe(g, function(b) {
                                    n.$name !== b && (r(a, void 0), n.$$parentForm.$$renameControl(n, b), r = c(n.$name), r(a, n))
                                }));
                                d.on("$destroy", function() {
                                    n.$$parentForm.$removeControl(n);
                                    r(a, void 0);
                                    R(n, Nb)
                                })
                            }
                        }
                    }
                }
            }]
        },
        Ge = ge(),
        Se = ge(!0),
        Pg = /^\d{4,}-[01]\d-[0-3]\dT[0-2]\d:[0-5]\d:[0-5]\d\.\d+(?:[+-][0-2]\d:[0-5]\d|Z)$/,
        ah = /^[a-z][a-z\d.+-]*:\/*(?:[^:@]+(?::[^@]+)?@)?(?:[^\s:/?#]+|\[[a-f\d:]+])(?::\d+)?(?:\/[^?#]*)?(?:\?[^#]*)?(?:#.*)?$/i,
        bh = /^(?=.{1,254}$)(?=.{1,64}@)[-!#$%&'*+/0-9=?A-Z^_`a-z{|}~]+(\.[-!#$%&'*+/0-9=?A-Z^_`a-z{|}~]+)*@[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?(\.[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?)*$/,
        Qg = /^\s*(-|\+)?(\d+|(\d*(\.\d*)))([eE][+-]?\d+)?\s*$/,
        he = /^(\d{4,})-(\d{2})-(\d{2})$/,
        ie = /^(\d{4,})-(\d\d)-(\d\d)T(\d\d):(\d\d)(?::(\d\d)(\.\d{1,3})?)?$/,
        Ec = /^(\d{4,})-W(\d\d)$/,
        je = /^(\d{4,})-(\d\d)$/,
        ke = /^(\d\d):(\d\d)(?::(\d\d)(\.\d{1,3})?)?$/,
        ae = V();
    p(["date", "datetime-local", "month", "time", "week"], function(a) {
        ae[a] = !0
    });
    var le = {
            text: function(a, b, d, c, e, f) {
                Ra(a, b, d, c, e, f);
                zc(c)
            },
            date: pb("date", he, Ob(he, ["yyyy", "MM", "dd"]), "yyyy-MM-dd"),
            "datetime-local": pb("datetimelocal", ie, Ob(ie, "yyyy MM dd HH mm ss sss".split(" ")),
                "yyyy-MM-ddTHH:mm:ss.sss"),
            time: pb("time", ke, Ob(ke, ["HH", "mm", "ss", "sss"]), "HH:mm:ss.sss"),
            week: pb("week", Ec, function(a, b) {
                if (ga(a)) return a;
                if (D(a)) {
                    Ec.lastIndex = 0;
                    var d = Ec.exec(a);
                    if (d) {
                        var c = +d[1],
                            e = +d[2],
                            f = d = 0,
                            g = 0,
                            h = 0,
                            k = Vd(c),
                            e = 7 * (e - 1);
                        b && (d = b.getHours(), f = b.getMinutes(), g = b.getSeconds(), h = b.getMilliseconds());
                        return new Date(c, 0, k.getDate() + e, d, f, g, h)
                    }
                }
                return NaN
            }, "yyyy-Www"),
            month: pb("month", je, Ob(je, ["yyyy", "MM"]), "yyyy-MM"),
            number: function(a, b, d, c, e, f) {
                Ac(a, b, d, c);
                be(c);
                Ra(a, b, d, c, e, f);
                var g,
                    h;
                if (u(d.min) || d.ngMin) c.$validators.min = function(a) {
                    return c.$isEmpty(a) || x(g) || a >= g
                }, d.$observe("min", function(a) {
                    g = Sa(a);
                    c.$validate()
                });
                if (u(d.max) || d.ngMax) c.$validators.max = function(a) {
                    return c.$isEmpty(a) || x(h) || a <= h
                }, d.$observe("max", function(a) {
                    h = Sa(a);
                    c.$validate()
                });
                if (u(d.step) || d.ngStep) {
                    var k;
                    c.$validators.step = function(a, b) {
                        return c.$isEmpty(b) || x(k) || ce(b, g || 0, k)
                    };
                    d.$observe("step", function(a) {
                        k = Sa(a);
                        c.$validate()
                    })
                }
            },
            url: function(a, b, d, c, e, f) {
                Ra(a, b, d, c, e, f);
                zc(c);
                c.$$parserName =
                    "url";
                c.$validators.url = function(a, b) {
                    var d = a || b;
                    return c.$isEmpty(d) || ah.test(d)
                }
            },
            email: function(a, b, d, c, e, f) {
                Ra(a, b, d, c, e, f);
                zc(c);
                c.$$parserName = "email";
                c.$validators.email = function(a, b) {
                    var d = a || b;
                    return c.$isEmpty(d) || bh.test(d)
                }
            },
            radio: function(a, b, d, c) {
                var e = !d.ngTrim || "false" !== S(d.ngTrim);
                x(d.name) && b.attr("name", ++rb);
                b.on("click", function(a) {
                    var g;
                    b[0].checked && (g = d.value, e && (g = S(g)), c.$setViewValue(g, a && a.type))
                });
                c.$render = function() {
                    var a = d.value;
                    e && (a = S(a));
                    b[0].checked = a === c.$viewValue
                };
                d.$observe("value", c.$render)
            },
            range: function(a, b, d, c, e, f) {
                function g(a, c) {
                    b.attr(a, d[a]);
                    d.$observe(a, c)
                }

                function h(a) {
                    n = Sa(a);
                    da(c.$modelValue) || (m ? (a = b.val(), n > a && (a = n, b.val(a)), c.$setViewValue(a)) : c.$validate())
                }

                function k(a) {
                    q = Sa(a);
                    da(c.$modelValue) || (m ? (a = b.val(), q < a && (b.val(q), a = q < n ? n : q), c.$setViewValue(a)) : c.$validate())
                }

                function l(a) {
                    r = Sa(a);
                    da(c.$modelValue) || (m && c.$viewValue !== b.val() ? c.$setViewValue(b.val()) : c.$validate())
                }
                Ac(a, b, d, c);
                be(c);
                Ra(a, b, d, c, e, f);
                var m = c.$$hasNativeValidators &&
                    "range" === b[0].type,
                    n = m ? 0 : void 0,
                    q = m ? 100 : void 0,
                    r = m ? 1 : void 0,
                    p = b[0].validity;
                a = u(d.min);
                e = u(d.max);
                f = u(d.step);
                var s = c.$render;
                c.$render = m && u(p.rangeUnderflow) && u(p.rangeOverflow) ? function() {
                    s();
                    c.$setViewValue(b.val())
                } : s;
                a && (c.$validators.min = m ? function() {
                    return !0
                } : function(a, b) {
                    return c.$isEmpty(b) || x(n) || b >= n
                }, g("min", h));
                e && (c.$validators.max = m ? function() {
                    return !0
                } : function(a, b) {
                    return c.$isEmpty(b) || x(q) || b <= q
                }, g("max", k));
                f && (c.$validators.step = m ? function() {
                    return !p.stepMismatch
                } : function(a,
                    b) {
                    return c.$isEmpty(b) || x(r) || ce(b, n || 0, r)
                }, g("step", l))
            },
            checkbox: function(a, b, d, c, e, f, g, h) {
                var k = de(h, a, "ngTrueValue", d.ngTrueValue, !0),
                    l = de(h, a, "ngFalseValue", d.ngFalseValue, !1);
                b.on("click", function(a) {
                    c.$setViewValue(b[0].checked, a && a.type)
                });
                c.$render = function() {
                    b[0].checked = c.$viewValue
                };
                c.$isEmpty = function(a) {
                    return !1 === a
                };
                c.$formatters.push(function(a) {
                    return pa(a, k)
                });
                c.$parsers.push(function(a) {
                    return a ? k : l
                })
            },
            hidden: A,
            button: A,
            submit: A,
            reset: A,
            file: A
        },
        Uc = ["$browser", "$sniffer", "$filter",
            "$parse",
            function(a, b, d, c) {
                return {
                    restrict: "E",
                    require: ["?ngModel"],
                    link: {
                        pre: function(e, f, g, h) {
                            h[0] && (le[P(g.type)] || le.text)(e, f, g, h[0], b, a, d, c)
                        }
                    }
                }
            }
        ],
        ch = /^(true|false|\d+)$/,
        kf = function() {
            function a(a, d, c) {
                var e = u(c) ? c : 9 === za ? "" : null;
                a.prop("value", e);
                d.$set("value", c)
            }
            return {
                restrict: "A",
                priority: 100,
                compile: function(b, d) {
                    return ch.test(d.ngValue) ? function(b, d, f) {
                        b = b.$eval(f.ngValue);
                        a(d, f, b)
                    } : function(b, d, f) {
                        b.$watch(f.ngValue, function(b) {
                            a(d, f, b)
                        })
                    }
                }
            }
        },
        Ke = ["$compile", function(a) {
            return {
                restrict: "AC",
                compile: function(b) {
                    a.$$addBindingClass(b);
                    return function(b, c, e) {
                        a.$$addBindingInfo(c, e.ngBind);
                        c = c[0];
                        b.$watch(e.ngBind, function(a) {
                            c.textContent = $b(a)
                        })
                    }
                }
            }
        }],
        Me = ["$interpolate", "$compile", function(a, b) {
            return {
                compile: function(d) {
                    b.$$addBindingClass(d);
                    return function(c, d, f) {
                        c = a(d.attr(f.$attr.ngBindTemplate));
                        b.$$addBindingInfo(d, c.expressions);
                        d = d[0];
                        f.$observe("ngBindTemplate", function(a) {
                            d.textContent = x(a) ? "" : a
                        })
                    }
                }
            }
        }],
        Le = ["$sce", "$parse", "$compile", function(a, b, d) {
            return {
                restrict: "A",
                compile: function(c,
                    e) {
                    var f = b(e.ngBindHtml),
                        g = b(e.ngBindHtml, function(b) {
                            return a.valueOf(b)
                        });
                    d.$$addBindingClass(c);
                    return function(b, c, e) {
                        d.$$addBindingInfo(c, e.ngBindHtml);
                        b.$watch(g, function() {
                            var d = f(b);
                            c.html(a.getTrustedHtml(d) || "")
                        })
                    }
                }
            }
        }],
        jf = la({
            restrict: "A",
            require: "ngModel",
            link: function(a, b, d, c) {
                c.$viewChangeListeners.push(function() {
                    a.$eval(d.ngChange)
                })
            }
        }),
        Ne = Cc("", !0),
        Pe = Cc("Odd", 0),
        Oe = Cc("Even", 1),
        Qe = Qa({
            compile: function(a, b) {
                b.$set("ngCloak", void 0);
                a.removeClass("ng-cloak")
            }
        }),
        Re = [function() {
            return {
                restrict: "A",
                scope: !0,
                controller: "@",
                priority: 500
            }
        }],
        Zc = {},
        dh = {
            blur: !0,
            focus: !0
        };
    p("click dblclick mousedown mouseup mouseover mouseout mousemove mouseenter mouseleave keydown keyup keypress submit focus blur copy cut paste".split(" "), function(a) {
        var b = Ba("ng-" + a);
        Zc[b] = ["$parse", "$rootScope", function(d, c) {
            return {
                restrict: "A",
                compile: function(e, f) {
                    var g = d(f[b]);
                    return function(b, d) {
                        d.on(a, function(d) {
                            var e = function() {
                                g(b, {
                                    $event: d
                                })
                            };
                            dh[a] && c.$$phase ? b.$evalAsync(e) : b.$apply(e)
                        })
                    }
                }
            }
        }]
    });
    var Ue = ["$animate", "$compile",
            function(a, b) {
                return {
                    multiElement: !0,
                    transclude: "element",
                    priority: 600,
                    terminal: !0,
                    restrict: "A",
                    $$tlb: !0,
                    link: function(d, c, e, f, g) {
                        var h, k, l;
                        d.$watch(e.ngIf, function(d) {
                            d ? k || g(function(d, f) {
                                k = f;
                                d[d.length++] = b.$$createComment("end ngIf", e.ngIf);
                                h = {
                                    clone: d
                                };
                                a.enter(d, c.parent(), c)
                            }) : (l && (l.remove(), l = null), k && (k.$destroy(), k = null), h && (l = ub(h.clone), a.leave(l).done(function(a) {
                                !1 !== a && (l = null)
                            }), h = null))
                        })
                    }
                }
            }
        ],
        Ve = ["$templateRequest", "$anchorScroll", "$animate", function(a, b, d) {
            return {
                restrict: "ECA",
                priority: 400,
                terminal: !0,
                transclude: "element",
                controller: ea.noop,
                compile: function(c, e) {
                    var f = e.ngInclude || e.src,
                        g = e.onload || "",
                        h = e.autoscroll;
                    return function(c, e, m, n, q) {
                        var r = 0,
                            p, s, t, x = function() {
                                s && (s.remove(), s = null);
                                p && (p.$destroy(), p = null);
                                t && (d.leave(t).done(function(a) {
                                    !1 !== a && (s = null)
                                }), s = t, t = null)
                            };
                        c.$watch(f, function(f) {
                            var m = function(a) {
                                    !1 === a || !u(h) || h && !c.$eval(h) || b()
                                },
                                s = ++r;
                            f ? (a(f, !0).then(function(a) {
                                if (!c.$$destroyed && s === r) {
                                    var b = c.$new();
                                    n.template = a;
                                    a = q(b, function(a) {
                                        x();
                                        d.enter(a, null, e).done(m)
                                    });
                                    p = b;
                                    t = a;
                                    p.$emit("$includeContentLoaded", f);
                                    c.$eval(g)
                                }
                            }, function() {
                                c.$$destroyed || s !== r || (x(), c.$emit("$includeContentError", f))
                            }), c.$emit("$includeContentRequested", f)) : (x(), n.template = null)
                        })
                    }
                }
            }
        }],
        mf = ["$compile", function(a) {
            return {
                restrict: "ECA",
                priority: -400,
                require: "ngInclude",
                link: function(b, d, c, e) {
                    ma.call(d[0]).match(/SVG/) ? (d.empty(), a(bd(e.template, w.document).childNodes)(b, function(a) {
                        d.append(a)
                    }, {
                        futureParentElement: d
                    })) : (d.html(e.template), a(d.contents())(b))
                }
            }
        }],
        We = Qa({
            priority: 450,
            compile: function() {
                return {
                    pre: function(a,
                        b, d) {
                        a.$eval(d.ngInit)
                    }
                }
            }
        }),
        hf = function() {
            return {
                restrict: "A",
                priority: 100,
                require: "ngModel",
                link: function(a, b, d, c) {
                    var e = d.ngList || ", ",
                        f = "false" !== d.ngTrim,
                        g = f ? S(e) : e;
                    c.$parsers.push(function(a) {
                        if (!x(a)) {
                            var b = [];
                            a && p(a.split(g), function(a) {
                                a && b.push(f ? S(a) : a)
                            });
                            return b
                        }
                    });
                    c.$formatters.push(function(a) {
                        if (H(a)) return a.join(e)
                    });
                    c.$isEmpty = function(a) {
                        return !a || !a.length
                    }
                }
            }
        },
        ob = "ng-valid",
        Yd = "ng-invalid",
        Va = "ng-pristine",
        Sb = "ng-dirty",
        qb = M("ngModel");
    Pb.$inject = "$scope $exceptionHandler $attrs $element $parse $animate $timeout $q $interpolate".split(" ");
    Pb.prototype = {
        $$initGetterSetters: function() {
            if (this.$options.getOption("getterSetter")) {
                var a = this.$$parse(this.$$attr.ngModel + "()"),
                    b = this.$$parse(this.$$attr.ngModel + "($$$p)");
                this.$$ngModelGet = function(b) {
                    var c = this.$$parsedNgModel(b);
                    E(c) && (c = a(b));
                    return c
                };
                this.$$ngModelSet = function(a, c) {
                    E(this.$$parsedNgModel(a)) ? b(a, {
                        $$$p: c
                    }) : this.$$parsedNgModelAssign(a, c)
                }
            } else if (!this.$$parsedNgModel.assign) throw qb("nonassign", this.$$attr.ngModel, xa(this.$$element));
        },
        $render: A,
        $isEmpty: function(a) {
            return x(a) ||
                "" === a || null === a || a !== a
        },
        $$updateEmptyClasses: function(a) {
            this.$isEmpty(a) ? (this.$$animate.removeClass(this.$$element, "ng-not-empty"), this.$$animate.addClass(this.$$element, "ng-empty")) : (this.$$animate.removeClass(this.$$element, "ng-empty"), this.$$animate.addClass(this.$$element, "ng-not-empty"))
        },
        $setPristine: function() {
            this.$dirty = !1;
            this.$pristine = !0;
            this.$$animate.removeClass(this.$$element, Sb);
            this.$$animate.addClass(this.$$element, Va)
        },
        $setDirty: function() {
            this.$dirty = !0;
            this.$pristine = !1;
            this.$$animate.removeClass(this.$$element,
                Va);
            this.$$animate.addClass(this.$$element, Sb);
            this.$$parentForm.$setDirty()
        },
        $setUntouched: function() {
            this.$touched = !1;
            this.$untouched = !0;
            this.$$animate.setClass(this.$$element, "ng-untouched", "ng-touched")
        },
        $setTouched: function() {
            this.$touched = !0;
            this.$untouched = !1;
            this.$$animate.setClass(this.$$element, "ng-touched", "ng-untouched")
        },
        $rollbackViewValue: function() {
            this.$$timeout.cancel(this.$$pendingDebounce);
            this.$viewValue = this.$$lastCommittedViewValue;
            this.$render()
        },
        $validate: function() {
            if (!da(this.$modelValue)) {
                var a =
                    this.$$lastCommittedViewValue,
                    b = this.$$rawModelValue,
                    d = this.$valid,
                    c = this.$modelValue,
                    e = this.$options.getOption("allowInvalid"),
                    f = this;
                this.$$runValidators(b, a, function(a) {
                    e || d === a || (f.$modelValue = a ? b : void 0, f.$modelValue !== c && f.$$writeModelToScope())
                })
            }
        },
        $$runValidators: function(a, b, d) {
            function c() {
                var c = !0;
                p(k.$validators, function(d, e) {
                    var g = Boolean(d(a, b));
                    c = c && g;
                    f(e, g)
                });
                return c ? !0 : (p(k.$asyncValidators, function(a, b) {
                    f(b, null)
                }), !1)
            }

            function e() {
                var c = [],
                    d = !0;
                p(k.$asyncValidators, function(e,
                    g) {
                    var k = e(a, b);
                    if (!k || !E(k.then)) throw qb("nopromise", k);
                    f(g, void 0);
                    c.push(k.then(function() {
                        f(g, !0)
                    }, function() {
                        d = !1;
                        f(g, !1)
                    }))
                });
                c.length ? k.$$q.all(c).then(function() {
                    g(d)
                }, A) : g(!0)
            }

            function f(a, b) {
                h === k.$$currentValidationRunId && k.$setValidity(a, b)
            }

            function g(a) {
                h === k.$$currentValidationRunId && d(a)
            }
            this.$$currentValidationRunId++;
            var h = this.$$currentValidationRunId,
                k = this;
            (function() {
                var a = k.$$parserName || "parse";
                if (x(k.$$parserValid)) f(a, null);
                else return k.$$parserValid || (p(k.$validators, function(a,
                    b) {
                    f(b, null)
                }), p(k.$asyncValidators, function(a, b) {
                    f(b, null)
                })), f(a, k.$$parserValid), k.$$parserValid;
                return !0
            })() ? c() ? e() : g(!1): g(!1)
        },
        $commitViewValue: function() {
            var a = this.$viewValue;
            this.$$timeout.cancel(this.$$pendingDebounce);
            if (this.$$lastCommittedViewValue !== a || "" === a && this.$$hasNativeValidators) this.$$updateEmptyClasses(a), this.$$lastCommittedViewValue = a, this.$pristine && this.$setDirty(), this.$$parseAndValidate()
        },
        $$parseAndValidate: function() {
            var a = this.$$lastCommittedViewValue,
                b = this;
            if (this.$$parserValid =
                x(a) ? void 0 : !0)
                for (var d = 0; d < this.$parsers.length; d++)
                    if (a = this.$parsers[d](a), x(a)) {
                        this.$$parserValid = !1;
                        break
                    } da(this.$modelValue) && (this.$modelValue = this.$$ngModelGet(this.$$scope));
            var c = this.$modelValue,
                e = this.$options.getOption("allowInvalid");
            this.$$rawModelValue = a;
            e && (this.$modelValue = a, b.$modelValue !== c && b.$$writeModelToScope());
            this.$$runValidators(a, this.$$lastCommittedViewValue, function(d) {
                e || (b.$modelValue = d ? a : void 0, b.$modelValue !== c && b.$$writeModelToScope())
            })
        },
        $$writeModelToScope: function() {
            this.$$ngModelSet(this.$$scope,
                this.$modelValue);
            p(this.$viewChangeListeners, function(a) {
                try {
                    a()
                } catch (b) {
                    this.$$exceptionHandler(b)
                }
            }, this)
        },
        $setViewValue: function(a, b) {
            this.$viewValue = a;
            this.$options.getOption("updateOnDefault") && this.$$debounceViewValueCommit(b)
        },
        $$debounceViewValueCommit: function(a) {
            var b = this.$options.getOption("debounce");
            ba(b[a]) ? b = b[a] : ba(b["default"]) && (b = b["default"]);
            this.$$timeout.cancel(this.$$pendingDebounce);
            var d = this;
            0 < b ? this.$$pendingDebounce = this.$$timeout(function() {
                    d.$commitViewValue()
                }, b) :
                this.$$scope.$root.$$phase ? this.$commitViewValue() : this.$$scope.$apply(function() {
                    d.$commitViewValue()
                })
        },
        $overrideModelOptions: function(a) {
            this.$options = this.$options.createChild(a)
        }
    };
    Zd({
        clazz: Pb,
        set: function(a, b) {
            a[b] = !0
        },
        unset: function(a, b) {
            delete a[b]
        }
    });
    var gf = ["$rootScope", function(a) {
            return {
                restrict: "A",
                require: ["ngModel", "^?form", "^?ngModelOptions"],
                controller: Pb,
                priority: 1,
                compile: function(b) {
                    b.addClass(Va).addClass("ng-untouched").addClass(ob);
                    return {
                        pre: function(a, b, e, f) {
                            var g = f[0];
                            b =
                                f[1] || g.$$parentForm;
                            if (f = f[2]) g.$options = f.$options;
                            g.$$initGetterSetters();
                            b.$addControl(g);
                            e.$observe("name", function(a) {
                                g.$name !== a && g.$$parentForm.$$renameControl(g, a)
                            });
                            a.$on("$destroy", function() {
                                g.$$parentForm.$removeControl(g)
                            })
                        },
                        post: function(b, c, e, f) {
                            function g() {
                                h.$setTouched()
                            }
                            var h = f[0];
                            if (h.$options.getOption("updateOn")) c.on(h.$options.getOption("updateOn"), function(a) {
                                h.$$debounceViewValueCommit(a && a.type)
                            });
                            c.on("blur", function() {
                                h.$touched || (a.$$phase ? b.$evalAsync(g) : b.$apply(g))
                            })
                        }
                    }
                }
            }
        }],
        Qb, eh = /(\s+|^)default(\s+|$)/;
    Dc.prototype = {
        getOption: function(a) {
            return this.$$options[a]
        },
        createChild: function(a) {
            var b = !1;
            a = R({}, a);
            p(a, function(d, c) {
                "$inherit" === d ? "*" === c ? b = !0 : (a[c] = this.$$options[c], "updateOn" === c && (a.updateOnDefault = this.$$options.updateOnDefault)) : "updateOn" === c && (a.updateOnDefault = !1, a[c] = S(d.replace(eh, function() {
                    a.updateOnDefault = !0;
                    return " "
                })))
            }, this);
            b && (delete a["*"], ee(a, this.$$options));
            ee(a, Qb.$$options);
            return new Dc(a)
        }
    };
    Qb = new Dc({
        updateOn: "",
        updateOnDefault: !0,
        debounce: 0,
        getterSetter: !1,
        allowInvalid: !1,
        timezone: null
    });
    var lf = function() {
            function a(a, d) {
                this.$$attrs = a;
                this.$$scope = d
            }
            a.$inject = ["$attrs", "$scope"];
            a.prototype = {
                $onInit: function() {
                    var a = this.parentCtrl ? this.parentCtrl.$options : Qb,
                        d = this.$$scope.$eval(this.$$attrs.ngModelOptions);
                    this.$options = a.createChild(d)
                }
            };
            return {
                restrict: "A",
                priority: 10,
                require: {
                    parentCtrl: "?^^ngModelOptions"
                },
                bindToController: !0,
                controller: a
            }
        },
        Xe = Qa({
            terminal: !0,
            priority: 1E3
        }),
        fh = M("ngOptions"),
        gh = /^\s*([\s\S]+?)(?:\s+as\s+([\s\S]+?))?(?:\s+group\s+by\s+([\s\S]+?))?(?:\s+disable\s+when\s+([\s\S]+?))?\s+for\s+(?:([$\w][$\w]*)|(?:\(\s*([$\w][$\w]*)\s*,\s*([$\w][$\w]*)\s*\)))\s+in\s+([\s\S]+?)(?:\s+track\s+by\s+([\s\S]+?))?$/,
        ef = ["$compile", "$document", "$parse", function(a, b, d) {
            function c(a, b, c) {
                function e(a, b, c, d, f) {
                    this.selectValue = a;
                    this.viewValue = b;
                    this.label = c;
                    this.group = d;
                    this.disabled = f
                }

                function f(a) {
                    var b;
                    if (!p && ra(a)) b = a;
                    else {
                        b = [];
                        for (var c in a) a.hasOwnProperty(c) && "$" !== c.charAt(0) && b.push(c)
                    }
                    return b
                }
                var n = a.match(gh);
                if (!n) throw fh("iexp", a, xa(b));
                var q = n[5] || n[7],
                    p = n[6];
                a = / as /.test(n[0]) && n[1];
                var s = n[9];
                b = d(n[2] ? n[1] : q);
                var u = a && d(a) || b,
                    t = s && d(s),
                    x = s ? function(a, b) {
                        return t(c, b)
                    } : function(a) {
                        return Pa(a)
                    },
                    y = function(a, b) {
                        return x(a, C(a, b))
                    },
                    v = d(n[2] || n[1]),
                    w = d(n[3] || ""),
                    B = d(n[4] || ""),
                    J = d(n[8]),
                    L = {},
                    C = p ? function(a, b) {
                        L[p] = b;
                        L[q] = a;
                        return L
                    } : function(a) {
                        L[q] = a;
                        return L
                    };
                return {
                    trackBy: s,
                    getTrackByValue: y,
                    getWatchables: d(J, function(a) {
                        var b = [];
                        a = a || [];
                        for (var d = f(a), e = d.length, g = 0; g < e; g++) {
                            var h = a === d ? g : d[g],
                                l = a[h],
                                h = C(l, h),
                                l = x(l, h);
                            b.push(l);
                            if (n[2] || n[1]) l = v(c, h), b.push(l);
                            n[4] && (h = B(c, h), b.push(h))
                        }
                        return b
                    }),
                    getOptions: function() {
                        for (var a = [], b = {}, d = J(c) || [], g = f(d), h = g.length, n = 0; n < h; n++) {
                            var q = d ===
                                g ? n : g[n],
                                p = C(d[q], q),
                                r = u(c, p),
                                q = x(r, p),
                                t = v(c, p),
                                L = w(c, p),
                                p = B(c, p),
                                r = new e(q, r, t, L, p);
                            a.push(r);
                            b[q] = r
                        }
                        return {
                            items: a,
                            selectValueMap: b,
                            getOptionFromViewValue: function(a) {
                                return b[y(a)]
                            },
                            getViewValueFromOption: function(a) {
                                return s ? sa(a.viewValue) : a.viewValue
                            }
                        }
                    }
                }
            }
            var e = w.document.createElement("option"),
                f = w.document.createElement("optgroup");
            return {
                restrict: "A",
                terminal: !0,
                require: ["select", "ngModel"],
                link: {
                    pre: function(a, b, c, d) {
                        d[0].registerOption = A
                    },
                    post: function(d, h, k, l) {
                        function m(a) {
                            var b = (a = v.getOptionFromViewValue(a)) &&
                                a.element;
                            b && !b.selected && (b.selected = !0);
                            return a
                        }

                        function n(a, b) {
                            a.element = b;
                            b.disabled = a.disabled;
                            a.label !== b.label && (b.label = a.label, b.textContent = a.label);
                            b.value = a.selectValue
                        }

                        function q() {
                            var a = v && r.readValue();
                            if (v)
                                for (var b = v.items.length - 1; 0 <= b; b--) {
                                    var c = v.items[b];
                                    u(c.group) ? Fb(c.element.parentNode) : Fb(c.element)
                                }
                            v = A.getOptions();
                            var d = {};
                            y && h.prepend(r.emptyOption);
                            v.items.forEach(function(a) {
                                var b;
                                if (u(a.group)) {
                                    b = d[a.group];
                                    b || (b = f.cloneNode(!1), B.appendChild(b), b.label = null === a.group ?
                                        "null" : a.group, d[a.group] = b);
                                    var c = e.cloneNode(!1)
                                } else b = B, c = e.cloneNode(!1);
                                b.appendChild(c);
                                n(a, c)
                            });
                            h[0].appendChild(B);
                            s.$render();
                            s.$isEmpty(a) || (b = r.readValue(), (A.trackBy || x ? pa(a, b) : a === b) || (s.$setViewValue(b), s.$render()))
                        }
                        var r = l[0],
                            s = l[1],
                            x = k.multiple;
                        l = 0;
                        for (var t = h.children(), w = t.length; l < w; l++)
                            if ("" === t[l].value) {
                                r.hasEmptyOption = !0;
                                r.emptyOption = t.eq(l);
                                break
                            } var y = !!r.emptyOption;
                        F(e.cloneNode(!1)).val("?");
                        var v, A = c(k.ngOptions, h, d),
                            B = b[0].createDocumentFragment();
                        r.generateUnknownOptionValue =
                            function(a) {
                                return "?"
                            };
                        x ? (r.writeValue = function(a) {
                            var b = a && a.map(m) || [];
                            v.items.forEach(function(a) {
                                a.element.selected && -1 === Array.prototype.indexOf.call(b, a) && (a.element.selected = !1)
                            })
                        }, r.readValue = function() {
                            var a = h.val() || [],
                                b = [];
                            p(a, function(a) {
                                (a = v.selectValueMap[a]) && !a.disabled && b.push(v.getViewValueFromOption(a))
                            });
                            return b
                        }, A.trackBy && d.$watchCollection(function() {
                            if (H(s.$viewValue)) return s.$viewValue.map(function(a) {
                                return A.getTrackByValue(a)
                            })
                        }, function() {
                            s.$render()
                        })) : (r.writeValue =
                            function(a) {
                                var b = v.selectValueMap[h.val()],
                                    c = v.getOptionFromViewValue(a);
                                b && b.element.removeAttribute("selected");
                                c ? (h[0].value !== c.selectValue && (r.removeUnknownOption(), r.unselectEmptyOption(), h[0].value = c.selectValue, c.element.selected = !0), c.element.setAttribute("selected", "selected")) : y ? r.selectEmptyOption() : r.unknownOption.parent().length ? r.updateUnknownOption(a) : r.renderUnknownOption(a)
                            }, r.readValue = function() {
                                var a = v.selectValueMap[h.val()];
                                return a && !a.disabled ? (r.unselectEmptyOption(), r.removeUnknownOption(),
                                    v.getViewValueFromOption(a)) : null
                            }, A.trackBy && d.$watch(function() {
                                return A.getTrackByValue(s.$viewValue)
                            }, function() {
                                s.$render()
                            }));
                        y && (r.emptyOption.remove(), a(r.emptyOption)(d), 8 === r.emptyOption[0].nodeType ? (r.hasEmptyOption = !1, r.registerOption = function(a, b) {
                            "" === b.val() && (r.hasEmptyOption = !0, r.emptyOption = b, r.emptyOption.removeClass("ng-scope"), s.$render(), b.on("$destroy", function() {
                                r.hasEmptyOption = !1;
                                r.emptyOption = void 0
                            }))
                        }) : r.emptyOption.removeClass("ng-scope"));
                        h.empty();
                        q();
                        d.$watchCollection(A.getWatchables,
                            q)
                    }
                }
            }
        }],
        Ye = ["$locale", "$interpolate", "$log", function(a, b, d) {
            var c = /{}/g,
                e = /^when(Minus)?(.+)$/;
            return {
                link: function(f, g, h) {
                    function k(a) {
                        g.text(a || "")
                    }
                    var l = h.count,
                        m = h.$attr.when && g.attr(h.$attr.when),
                        n = h.offset || 0,
                        q = f.$eval(m) || {},
                        r = {},
                        s = b.startSymbol(),
                        u = b.endSymbol(),
                        t = s + l + "-" + n + u,
                        w = ea.noop,
                        y;
                    p(h, function(a, b) {
                        var c = e.exec(b);
                        c && (c = (c[1] ? "-" : "") + P(c[2]), q[c] = g.attr(h.$attr[b]))
                    });
                    p(q, function(a, d) {
                        r[d] = b(a.replace(c, t))
                    });
                    f.$watch(l, function(b) {
                        var c = parseFloat(b),
                            e = da(c);
                        e || c in q || (c = a.pluralCat(c -
                            n));
                        c === y || e && da(y) || (w(), e = r[c], x(e) ? (null != b && d.debug("ngPluralize: no rule defined for '" + c + "' in " + m), w = A, k()) : w = f.$watch(e, k), y = c)
                    })
                }
            }
        }],
        Ze = ["$parse", "$animate", "$compile", function(a, b, d) {
            var c = M("ngRepeat"),
                e = function(a, b, c, d, e, m, n) {
                    a[c] = d;
                    e && (a[e] = m);
                    a.$index = b;
                    a.$first = 0 === b;
                    a.$last = b === n - 1;
                    a.$middle = !(a.$first || a.$last);
                    a.$odd = !(a.$even = 0 === (b & 1))
                };
            return {
                restrict: "A",
                multiElement: !0,
                transclude: "element",
                priority: 1E3,
                terminal: !0,
                $$tlb: !0,
                compile: function(f, g) {
                    var h = g.ngRepeat,
                        k = d.$$createComment("end ngRepeat",
                            h),
                        l = h.match(/^\s*([\s\S]+?)\s+in\s+([\s\S]+?)(?:\s+as\s+([\s\S]+?))?(?:\s+track\s+by\s+([\s\S]+?))?\s*$/);
                    if (!l) throw c("iexp", h);
                    var m = l[1],
                        n = l[2],
                        q = l[3],
                        r = l[4],
                        l = m.match(/^(?:(\s*[$\w]+)|\(\s*([$\w]+)\s*,\s*([$\w]+)\s*\))$/);
                    if (!l) throw c("iidexp", m);
                    var s = l[3] || l[1],
                        u = l[2];
                    if (q && (!/^[$a-zA-Z_][$a-zA-Z0-9_]*$/.test(q) || /^(null|undefined|this|\$index|\$first|\$middle|\$last|\$even|\$odd|\$parent|\$root|\$id)$/.test(q))) throw c("badident", q);
                    var t, x, y, v, w = {
                        $id: Pa
                    };
                    r ? t = a(r) : (y = function(a, b) {
                            return Pa(b)
                        },
                        v = function(a) {
                            return a
                        });
                    return function(a, d, f, g, l) {
                        t && (x = function(b, c, d) {
                            u && (w[u] = b);
                            w[s] = c;
                            w.$index = d;
                            return t(a, w)
                        });
                        var m = V();
                        a.$watchCollection(n, function(f) {
                            var g, n, r = d[0],
                                t, w = V(),
                                A, E, F, D, G, C, H;
                            q && (a[q] = f);
                            if (ra(f)) G = f, n = x || y;
                            else
                                for (H in n = x || v, G = [], f) ua.call(f, H) && "$" !== H.charAt(0) && G.push(H);
                            A = G.length;
                            H = Array(A);
                            for (g = 0; g < A; g++)
                                if (E = f === G ? g : G[g], F = f[E], D = n(E, F, g), m[D]) C = m[D], delete m[D], w[D] = C, H[g] = C;
                                else {
                                    if (w[D]) throw p(H, function(a) {
                                        a && a.scope && (m[a.id] = a)
                                    }), c("dupes", h, D, F);
                                    H[g] = {
                                        id: D,
                                        scope: void 0,
                                        clone: void 0
                                    };
                                    w[D] = !0
                                } for (t in m) {
                                C = m[t];
                                D = ub(C.clone);
                                b.leave(D);
                                if (D[0].parentNode)
                                    for (g = 0, n = D.length; g < n; g++) D[g].$$NG_REMOVED = !0;
                                C.scope.$destroy()
                            }
                            for (g = 0; g < A; g++)
                                if (E = f === G ? g : G[g], F = f[E], C = H[g], C.scope) {
                                    t = r;
                                    do t = t.nextSibling; while (t && t.$$NG_REMOVED);
                                    C.clone[0] !== t && b.move(ub(C.clone), null, r);
                                    r = C.clone[C.clone.length - 1];
                                    e(C.scope, g, s, F, u, E, A)
                                } else l(function(a, c) {
                                    C.scope = c;
                                    var d = k.cloneNode(!1);
                                    a[a.length++] = d;
                                    b.enter(a, null, r);
                                    r = d;
                                    C.clone = a;
                                    w[C.id] = C;
                                    e(C.scope, g, s, F, u, E, A)
                                });
                            m =
                                w
                        })
                    }
                }
            }
        }],
        $e = ["$animate", function(a) {
            return {
                restrict: "A",
                multiElement: !0,
                link: function(b, d, c) {
                    b.$watch(c.ngShow, function(b) {
                        a[b ? "removeClass" : "addClass"](d, "ng-hide", {
                            tempClasses: "ng-hide-animate"
                        })
                    })
                }
            }
        }],
        Te = ["$animate", function(a) {
            return {
                restrict: "A",
                multiElement: !0,
                link: function(b, d, c) {
                    b.$watch(c.ngHide, function(b) {
                        a[b ? "addClass" : "removeClass"](d, "ng-hide", {
                            tempClasses: "ng-hide-animate"
                        })
                    })
                }
            }
        }],
        af = Qa(function(a, b, d) {
            a.$watch(d.ngStyle, function(a, d) {
                    d && a !== d && p(d, function(a, c) {
                        b.css(c, "")
                    });
                    a && b.css(a)
                },
                !0)
        }),
        bf = ["$animate", "$compile", function(a, b) {
            return {
                require: "ngSwitch",
                controller: ["$scope", function() {
                    this.cases = {}
                }],
                link: function(d, c, e, f) {
                    var g = [],
                        h = [],
                        k = [],
                        l = [],
                        m = function(a, b) {
                            return function(c) {
                                !1 !== c && a.splice(b, 1)
                            }
                        };
                    d.$watch(e.ngSwitch || e.on, function(c) {
                        for (var d, e; k.length;) a.cancel(k.pop());
                        d = 0;
                        for (e = l.length; d < e; ++d) {
                            var s = ub(h[d].clone);
                            l[d].$destroy();
                            (k[d] = a.leave(s)).done(m(k, d))
                        }
                        h.length = 0;
                        l.length = 0;
                        (g = f.cases["!" + c] || f.cases["?"]) && p(g, function(c) {
                            c.transclude(function(d, e) {
                                l.push(e);
                                var f = c.element;
                                d[d.length++] = b.$$createComment("end ngSwitchWhen");
                                h.push({
                                    clone: d
                                });
                                a.enter(d, f.parent(), f)
                            })
                        })
                    })
                }
            }
        }],
        cf = Qa({
            transclude: "element",
            priority: 1200,
            require: "^ngSwitch",
            multiElement: !0,
            link: function(a, b, d, c, e) {
                a = d.ngSwitchWhen.split(d.ngSwitchWhenSeparator).sort().filter(function(a, b, c) {
                    return c[b - 1] !== a
                });
                p(a, function(a) {
                    c.cases["!" + a] = c.cases["!" + a] || [];
                    c.cases["!" + a].push({
                        transclude: e,
                        element: b
                    })
                })
            }
        }),
        df = Qa({
            transclude: "element",
            priority: 1200,
            require: "^ngSwitch",
            multiElement: !0,
            link: function(a,
                b, d, c, e) {
                c.cases["?"] = c.cases["?"] || [];
                c.cases["?"].push({
                    transclude: e,
                    element: b
                })
            }
        }),
        hh = M("ngTransclude"),
        ff = ["$compile", function(a) {
            return {
                restrict: "EAC",
                terminal: !0,
                compile: function(b) {
                    var d = a(b.contents());
                    b.empty();
                    return function(a, b, f, g, h) {
                        function k() {
                            d(a, function(a) {
                                b.append(a)
                            })
                        }
                        if (!h) throw hh("orphan", xa(b));
                        f.ngTransclude === f.$attr.ngTransclude && (f.ngTransclude = "");
                        f = f.ngTransclude || f.ngTranscludeSlot;
                        h(function(a, c) {
                            var d;
                            if (d = a.length) a: {
                                d = 0;
                                for (var f = a.length; d < f; d++) {
                                    var g = a[d];
                                    if (g.nodeType !==
                                        Ia || g.nodeValue.trim()) {
                                        d = !0;
                                        break a
                                    }
                                }
                                d = void 0
                            }
                            d ? b.append(a) : (k(), c.$destroy())
                        }, null, f);
                        f && !h.isSlotFilled(f) && k()
                    }
                }
            }
        }],
        He = ["$templateCache", function(a) {
            return {
                restrict: "E",
                terminal: !0,
                compile: function(b, d) {
                    "text/ng-template" === d.type && a.put(d.id, b[0].text)
                }
            }
        }],
        ih = {
            $setViewValue: A,
            $render: A
        },
        jh = ["$element", "$scope", function(a, b) {
            function d() {
                g || (g = !0, b.$$postDigest(function() {
                    g = !1;
                    e.ngModelCtrl.$render()
                }))
            }

            function c(a) {
                h || (h = !0, b.$$postDigest(function() {
                    b.$$destroyed || (h = !1, e.ngModelCtrl.$setViewValue(e.readValue()),
                        a && e.ngModelCtrl.$render())
                }))
            }
            var e = this,
                f = new Hb;
            e.selectValueMap = {};
            e.ngModelCtrl = ih;
            e.multiple = !1;
            e.unknownOption = F(w.document.createElement("option"));
            e.hasEmptyOption = !1;
            e.emptyOption = void 0;
            e.renderUnknownOption = function(b) {
                b = e.generateUnknownOptionValue(b);
                e.unknownOption.val(b);
                a.prepend(e.unknownOption);
                Ta(e.unknownOption, !0);
                a.val(b)
            };
            e.updateUnknownOption = function(b) {
                b = e.generateUnknownOptionValue(b);
                e.unknownOption.val(b);
                Ta(e.unknownOption, !0);
                a.val(b)
            };
            e.generateUnknownOptionValue =
                function(a) {
                    return "? " + Pa(a) + " ?"
                };
            e.removeUnknownOption = function() {
                e.unknownOption.parent() && e.unknownOption.remove()
            };
            e.selectEmptyOption = function() {
                e.emptyOption && (a.val(""), Ta(e.emptyOption, !0))
            };
            e.unselectEmptyOption = function() {
                e.hasEmptyOption && e.emptyOption.removeAttr("selected")
            };
            b.$on("$destroy", function() {
                e.renderUnknownOption = A
            });
            e.readValue = function() {
                var b = a.val(),
                    b = b in e.selectValueMap ? e.selectValueMap[b] : b;
                return e.hasOption(b) ? b : null
            };
            e.writeValue = function(b) {
                var c = a[0].options[a[0].selectedIndex];
                c && Ta(F(c), !1);
                e.hasOption(b) ? (e.removeUnknownOption(), c = Pa(b), a.val(c in e.selectValueMap ? c : b), Ta(F(a[0].options[a[0].selectedIndex]), !0)) : null == b && e.emptyOption ? (e.removeUnknownOption(), e.selectEmptyOption()) : e.unknownOption.parent().length ? e.updateUnknownOption(b) : e.renderUnknownOption(b)
            };
            e.addOption = function(a, b) {
                if (8 !== b[0].nodeType) {
                    Ka(a, '"option value"');
                    "" === a && (e.hasEmptyOption = !0, e.emptyOption = b);
                    var c = f.get(a) || 0;
                    f.set(a, c + 1);
                    d()
                }
            };
            e.removeOption = function(a) {
                var b = f.get(a);
                b && (1 === b ? (f.delete(a),
                    "" === a && (e.hasEmptyOption = !1, e.emptyOption = void 0)) : f.set(a, b - 1))
            };
            e.hasOption = function(a) {
                return !!f.get(a)
            };
            var g = !1,
                h = !1;
            e.registerOption = function(a, b, f, g, h) {
                if (f.$attr.ngValue) {
                    var p, s = NaN;
                    f.$observe("value", function(a) {
                        var d, f = b.prop("selected");
                        u(s) && (e.removeOption(p), delete e.selectValueMap[s], d = !0);
                        s = Pa(a);
                        p = a;
                        e.selectValueMap[s] = a;
                        e.addOption(a, b);
                        b.attr("value", s);
                        d && f && c()
                    })
                } else g ? f.$observe("value", function(a) {
                    e.readValue();
                    var d, f = b.prop("selected");
                    u(p) && (e.removeOption(p), d = !0);
                    p =
                        a;
                    e.addOption(a, b);
                    d && f && c()
                }) : h ? a.$watch(h, function(a, d) {
                    f.$set("value", a);
                    var g = b.prop("selected");
                    d !== a && e.removeOption(d);
                    e.addOption(a, b);
                    d && g && c()
                }) : e.addOption(f.value, b);
                f.$observe("disabled", function(a) {
                    if ("true" === a || a && b.prop("selected")) e.multiple ? c(!0) : (e.ngModelCtrl.$setViewValue(null), e.ngModelCtrl.$render())
                });
                b.on("$destroy", function() {
                    var a = e.readValue(),
                        b = f.value;
                    e.removeOption(b);
                    d();
                    (e.multiple && a && -1 !== a.indexOf(b) || a === b) && c(!0)
                })
            }
        }],
        Ie = function() {
            return {
                restrict: "E",
                require: ["select",
                    "?ngModel"
                ],
                controller: jh,
                priority: 1,
                link: {
                    pre: function(a, b, d, c) {
                        var e = c[0],
                            f = c[1];
                        if (f) {
                            if (e.ngModelCtrl = f, b.on("change", function() {
                                    e.removeUnknownOption();
                                    a.$apply(function() {
                                        f.$setViewValue(e.readValue())
                                    })
                                }), d.multiple) {
                                e.multiple = !0;
                                e.readValue = function() {
                                    var a = [];
                                    p(b.find("option"), function(b) {
                                        b.selected && !b.disabled && (b = b.value, a.push(b in e.selectValueMap ? e.selectValueMap[b] : b))
                                    });
                                    return a
                                };
                                e.writeValue = function(a) {
                                    p(b.find("option"), function(b) {
                                        var c = !!a && (-1 !== Array.prototype.indexOf.call(a,
                                            b.value) || -1 !== Array.prototype.indexOf.call(a, e.selectValueMap[b.value]));
                                        c !== b.selected && Ta(F(b), c)
                                    })
                                };
                                var g, h = NaN;
                                a.$watch(function() {
                                    h !== f.$viewValue || pa(g, f.$viewValue) || (g = qa(f.$viewValue), f.$render());
                                    h = f.$viewValue
                                });
                                f.$isEmpty = function(a) {
                                    return !a || 0 === a.length
                                }
                            }
                        } else e.registerOption = A
                    },
                    post: function(a, b, d, c) {
                        var e = c[1];
                        if (e) {
                            var f = c[0];
                            e.$render = function() {
                                f.writeValue(e.$viewValue)
                            }
                        }
                    }
                }
            }
        },
        Je = ["$interpolate", function(a) {
            return {
                restrict: "E",
                priority: 100,
                compile: function(b, d) {
                    var c, e;
                    u(d.ngValue) ||
                        (u(d.value) ? c = a(d.value, !0) : (e = a(b.text(), !0)) || d.$set("value", b.text()));
                    return function(a, b, d) {
                        var k = b.parent();
                        (k = k.data("$selectController") || k.parent().data("$selectController")) && k.registerOption(a, b, d, c, e)
                    }
                }
            }
        }],
        Wc = function() {
            return {
                restrict: "A",
                require: "?ngModel",
                link: function(a, b, d, c) {
                    c && (d.required = !0, c.$validators.required = function(a, b) {
                        return !d.required || !c.$isEmpty(b)
                    }, d.$observe("required", function() {
                        c.$validate()
                    }))
                }
            }
        },
        Vc = function() {
            return {
                restrict: "A",
                require: "?ngModel",
                link: function(a,
                    b, d, c) {
                    if (c) {
                        var e, f = d.ngPattern || d.pattern;
                        d.$observe("pattern", function(a) {
                            D(a) && 0 < a.length && (a = new RegExp("^" + a + "$"));
                            if (a && !a.test) throw M("ngPattern")("noregexp", f, a, xa(b));
                            e = a || void 0;
                            c.$validate()
                        });
                        c.$validators.pattern = function(a, b) {
                            return c.$isEmpty(b) || x(e) || e.test(b)
                        }
                    }
                }
            }
        },
        Yc = function() {
            return {
                restrict: "A",
                require: "?ngModel",
                link: function(a, b, d, c) {
                    if (c) {
                        var e = -1;
                        d.$observe("maxlength", function(a) {
                            a = Z(a);
                            e = da(a) ? -1 : a;
                            c.$validate()
                        });
                        c.$validators.maxlength = function(a, b) {
                            return 0 > e || c.$isEmpty(b) ||
                                b.length <= e
                        }
                    }
                }
            }
        },
        Xc = function() {
            return {
                restrict: "A",
                require: "?ngModel",
                link: function(a, b, d, c) {
                    if (c) {
                        var e = 0;
                        d.$observe("minlength", function(a) {
                            e = Z(a) || 0;
                            c.$validate()
                        });
                        c.$validators.minlength = function(a, b) {
                            return c.$isEmpty(b) || b.length >= e
                        }
                    }
                }
            }
        };
    w.angular.bootstrap ? w.console && console.log("WARNING: Tried to load angular more than once.") : (ze(), Ce(ea), ea.module("ngLocale", [], ["$provide", function(a) {
        function b(a) {
            a += "";
            var b = a.indexOf(".");
            return -1 == b ? 0 : a.length - b - 1
        }
        a.value("$locale", {
            DATETIME_FORMATS: {
                AMPMS: ["AM",
                    "PM"
                ],
                DAY: "Sunday Monday Tuesday Wednesday Thursday Friday Saturday".split(" "),
                ERANAMES: ["Before Christ", "Anno Domini"],
                ERAS: ["BC", "AD"],
                FIRSTDAYOFWEEK: 6,
                MONTH: "January February March April May June July August September October November December".split(" "),
                SHORTDAY: "Sun Mon Tue Wed Thu Fri Sat".split(" "),
                SHORTMONTH: "Jan Feb Mar Apr May Jun Jul Aug Sep Oct Nov Dec".split(" "),
                STANDALONEMONTH: "January February March April May June July August September October November December".split(" "),
                WEEKENDRANGE: [5,
                    6
                ],
                fullDate: "EEEE, MMMM d, y",
                longDate: "MMMM d, y",
                medium: "MMM d, y h:mm:ss a",
                mediumDate: "MMM d, y",
                mediumTime: "h:mm:ss a",
                "short": "M/d/yy h:mm a",
                shortDate: "M/d/yy",
                shortTime: "h:mm a"
            },
            NUMBER_FORMATS: {
                CURRENCY_SYM: "$",
                DECIMAL_SEP: ".",
                GROUP_SEP: ",",
                PATTERNS: [{
                    gSize: 3,
                    lgSize: 3,
                    maxFrac: 3,
                    minFrac: 0,
                    minInt: 1,
                    negPre: "-",
                    negSuf: "",
                    posPre: "",
                    posSuf: ""
                }, {
                    gSize: 3,
                    lgSize: 3,
                    maxFrac: 2,
                    minFrac: 2,
                    minInt: 1,
                    negPre: "-¤",
                    negSuf: "",
                    posPre: "¤",
                    posSuf: ""
                }]
            },
            id: "en-us",
            localeID: "en_US",
            pluralCat: function(a,
                c) {
                var e = a | 0,
                    f = c;
                void 0 === f && (f = Math.min(b(a), 3));
                Math.pow(10, f);
                return 1 == e && 0 == f ? "one" : "other"
            }
        })
    }]), F(function() {
        ue(w.document, Pc)
    }))
})(window);
!window.angular.$$csp().noInlineStyle && window.angular.element(document.head).prepend('<style type="text/css">@charset "UTF-8";[ng\\:cloak],[ng-cloak],[data-ng-cloak],[x-ng-cloak],.ng-cloak,.x-ng-cloak,.ng-hide:not(.ng-hide-animate){display:none !important;}ng\\:form{display:block;}.ng-animate-shim{visibility:hidden;}.ng-anchor{position:absolute;}</style>');

;// JS/angular/angular-sanitize.min.js
// angular/angular-sanitize.min.js
/*
 AngularJS v1.2.21
 (c) 2010-2014 Google, Inc. http://angularjs.org
 License: MIT
*/
(function(q, g, r) {
    'use strict';

    function F(a) {
        var d = [];
        t(d, g.noop).chars(a);
        return d.join("")
    }

    function m(a) {
        var d = {};
        a = a.split(",");
        var b;
        for (b = 0; b < a.length; b++) d[a[b]] = !0;
        return d
    }

    function G(a, d) {
        function b(a, c, b, h) {
            c = g.lowercase(c);
            if (u[c])
                for (; f.last() && v[f.last()];) e("", f.last());
            w[c] && f.last() == c && e("", c);
            (h = x[c] || !!h) || f.push(c);
            var n = {};
            b.replace(H, function(a, c, d, b, e) {
                n[c] = s(d || b || e || "")
            });
            d.start && d.start(c, n, h)
        }

        function e(a, c) {
            var b = 0,
                e;
            if (c = g.lowercase(c))
                for (b = f.length - 1; 0 <= b && f[b] != c; b--);
            if (0 <= b) {
                for (e = f.length - 1; e >= b; e--) d.end && d.end(f[e]);
                f.length = b
            }
        }
        var c, l, f = [],
            n = a,
            h;
        for (f.last = function() {
                return f[f.length - 1]
            }; a;) {
            h = "";
            l = !0;
            if (f.last() && y[f.last()]) a = a.replace(RegExp("(.*)<\\s*\\/\\s*" + f.last() + "[^>]*>", "i"), function(c, a) {
                a = a.replace(I, "$1").replace(J, "$1");
                d.chars && d.chars(s(a));
                return ""
            }), e("", f.last());
            else {
                if (0 === a.indexOf("\x3c!--")) c = a.indexOf("--", 4), 0 <= c && a.lastIndexOf("--\x3e", c) === c && (d.comment && d.comment(a.substring(4, c)), a = a.substring(c + 3), l = !1);
                else if (z.test(a)) {
                    if (c =
                        a.match(z)) a = a.replace(c[0], ""), l = !1
                } else if (K.test(a)) {
                    if (c = a.match(A)) a = a.substring(c[0].length), c[0].replace(A, e), l = !1
                } else L.test(a) && ((c = a.match(B)) ? (c[4] && (a = a.substring(c[0].length), c[0].replace(B, b)), l = !1) : (h += "<", a = a.substring(1)));
                l && (c = a.indexOf("<"), h += 0 > c ? a : a.substring(0, c), a = 0 > c ? "" : a.substring(c), d.chars && d.chars(s(h)))
            }
            if (a == n) throw M("badparse", a);
            n = a
        }
        e()
    }

    function s(a) {
        if (!a) return "";
        var d = N.exec(a);
        a = d[1];
        var b = d[3];
        if (d = d[2]) p.innerHTML = d.replace(/</g, "&lt;"), d = "textContent" in p ?
            p.textContent : p.innerText;
        return a + d + b
    }

    function C(a) {
        return a.replace(/&/g, "&amp;").replace(O, function(a) {
            var b = a.charCodeAt(0);
            a = a.charCodeAt(1);
            return "&#" + (1024 * (b - 55296) + (a - 56320) + 65536) + ";"
        }).replace(P, function(a) {
            return "&#" + a.charCodeAt(0) + ";"
        }).replace(/</g, "&lt;").replace(/>/g, "&gt;")
    }

    function t(a, d) {
        var b = !1,
            e = g.bind(a, a.push);
        return {
            start: function(a, l, f) {
                a = g.lowercase(a);
                !b && y[a] && (b = a);
                b || !0 !== D[a] || (e("<"), e(a), g.forEach(l, function(b, f) {
                    var k = g.lowercase(f),
                        l = "img" === a && "src" === k || "background" ===
                        k;
                    !0 !== Q[k] || !0 === E[k] && !d(b, l) || (e(" "), e(f), e('="'), e(C(b)), e('"'))
                }), e(f ? "/>" : ">"))
            },
            end: function(a) {
                a = g.lowercase(a);
                b || !0 !== D[a] || (e("</"), e(a), e(">"));
                a == b && (b = !1)
            },
            chars: function(a) {
                b || e(C(a))
            }
        }
    }
    var M = g.$$minErr("$sanitize"),
        B = /^<((?:[a-zA-Z])[\w:-]*)((?:\s+[\w:-]+(?:\s*=\s*(?:(?:"[^"]*")|(?:'[^']*')|[^>\s]+))?)*)\s*(\/?)\s*(>?)/,
        A = /^<\/\s*([\w:-]+)[^>]*>/,
        H = /([\w:-]+)(?:\s*=\s*(?:(?:"((?:[^"])*)")|(?:'((?:[^'])*)')|([^>\s]+)))?/g,
        L = /^</,
        K = /^<\//,
        I = /\x3c!--(.*?)--\x3e/g,
        z = /<!DOCTYPE([^>]*?)>/i,
        J = /<!\[CDATA\[(.*?)]]\x3e/g,
        O = /[\uD800-\uDBFF][\uDC00-\uDFFF]/g,
        P = /([^\#-~| |!])/g,
        x = m("area,br,col,hr,img,wbr");
    q = m("colgroup,dd,dt,li,p,tbody,td,tfoot,th,thead,tr");
    r = m("rp,rt");
    var w = g.extend({}, r, q),
        u = g.extend({}, q, m("address,article,aside,blockquote,caption,center,del,dir,div,dl,figure,figcaption,footer,h1,h2,h3,h4,h5,h6,header,hgroup,hr,ins,map,menu,nav,ol,pre,script,section,table,ul")),
        v = g.extend({}, r, m("a,abbr,acronym,b,bdi,bdo,big,br,cite,code,del,dfn,em,font,i,img,ins,kbd,label,map,mark,q,ruby,rp,rt,s,samp,small,span,strike,strong,sub,sup,time,tt,u,var")),
        y = m("script,style"),
        D = g.extend({}, x, u, v, w),
        E = m("background,cite,href,longdesc,src,usemap"),
        Q = g.extend({}, E, m("abbr,align,alt,axis,bgcolor,border,cellpadding,cellspacing,class,clear,color,cols,colspan,compact,coords,dir,face,headers,height,hreflang,hspace,ismap,lang,language,nohref,nowrap,rel,rev,rows,rowspan,rules,scope,scrolling,shape,size,span,start,summary,target,title,type,valign,value,vspace,width")),
        p = document.createElement("pre"),
        N = /^(\s*)([\s\S]*?)(\s*)$/;
    g.module("ngSanitize", []).provider("$sanitize",
        function() {
            this.$get = ["$$sanitizeUri", function(a) {
                return function(d) {
                    var b = [];
                    G(d, t(b, function(b, c) {
                        return !/^unsafe/.test(a(b, c))
                    }));
                    return b.join("")
                }
            }]
        });
    g.module("ngSanitize").filter("linky", ["$sanitize", function(a) {
        var d = /((ftp|https?):\/\/|(mailto:)?[A-Za-z0-9._%+-]+@)\S*[^\s.;,(){}<>]/,
            b = /^mailto:/;
        return function(e, c) {
            function l(a) {
                a && k.push(F(a))
            }

            function f(a, b) {
                k.push("<a ");
                g.isDefined(c) && (k.push('target="'), k.push(c), k.push('" '));
                k.push('href="');
                k.push(a);
                k.push('">');
                l(b);
                k.push("</a>")
            }
            if (!e) return e;
            for (var n, h = e, k = [], m, p; n = h.match(d);) m = n[0], n[2] == n[3] && (m = "mailto:" + m), p = n.index, l(h.substr(0, p)), f(m, n[0].replace(b, "")), h = h.substring(p + n[0].length);
            l(h);
            return a(k.join(""))
        }
    }])
})(window, window.angular);

;// JS/angular/angular-ui-router.min.js
// angular/angular-ui-router.min.js
/**
 * State-based routing for AngularJS
 * @version v0.2.11
 * @link http://angular-ui.github.com/
 * @license MIT License, http://www.opensource.org/licenses/MIT
 */
"undefined" != typeof module && "undefined" != typeof exports && module.exports === exports && (module.exports = "ui.router"),
    function(a, b, c) {
        "use strict";

        function d(a, b) {
            return J(new(J(function() {}, {
                prototype: a
            })), b)
        }

        function e(a) {
            return I(arguments, function(b) {
                b !== a && I(b, function(b, c) {
                    a.hasOwnProperty(c) || (a[c] = b)
                })
            }), a
        }

        function f(a, b) {
            var c = [];
            for (var d in a.path) {
                if (a.path[d] !== b.path[d]) break;
                c.push(a.path[d])
            }
            return c
        }

        function g(a) {
            if (Object.keys) return Object.keys(a);
            var c = [];
            return b.forEach(a, function(a, b) {
                c.push(b)
            }), c
        }

        function h(a, b) {
            if (Array.prototype.indexOf) return a.indexOf(b, Number(arguments[2]) || 0);
            var c = a.length >>> 0,
                d = Number(arguments[2]) || 0;
            for (d = 0 > d ? Math.ceil(d) : Math.floor(d), 0 > d && (d += c); c > d; d++)
                if (d in a && a[d] === b) return d;
            return -1
        }

        function i(a, b, c, d) {
            var e, i = f(c, d),
                j = {},
                k = [];
            for (var l in i)
                if (i[l].params && (e = g(i[l].params), e.length))
                    for (var m in e) h(k, e[m]) >= 0 || (k.push(e[m]), j[e[m]] = a[e[m]]);
            return J({}, j, b)
        }

        function j(a, b, c) {
            if (!c) {
                c = [];
                for (var d in a) c.push(d)
            }
            for (var e = 0; e < c.length; e++) {
                var f = c[e];
                if (a[f] != b[f]) return !1
            }
            return !0
        }

        function k(a, b) {
            var c = {};
            return I(a, function(a) {
                c[a] = b[a]
            }), c
        }

        function l(a, b) {
            var d = 1,
                f = 2,
                g = {},
                h = [],
                i = g,
                j = J(a.when(g), {
                    $$promises: g,
                    $$values: g
                });
            this.study = function(g) {
                function k(a, c) {
                    if (o[c] !== f) {
                        if (n.push(c), o[c] === d) throw n.splice(0, n.indexOf(c)), new Error("Cyclic dependency: " + n.join(" -> "));
                        if (o[c] = d, F(a)) m.push(c, [function() {
                            return b.get(a)
                        }], h);
                        else {
                            var e = b.annotate(a);
                            I(e, function(a) {
                                a !== c && g.hasOwnProperty(a) && k(g[a], a)
                            }), m.push(c, a, e)
                        }
                        n.pop(), o[c] = f
                    }
                }

                function l(a) {
                    return G(a) && a.then && a.$$promises
                }
                if (!G(g)) throw new Error("'invocables' must be an object");
                var m = [],
                    n = [],
                    o = {};
                return I(g, k), g = n = o = null,
                    function(d, f, g) {
                        function h() {
                            --s || (t || e(r, f.$$values), p.$$values = r, p.$$promises = !0, delete p.$$inheritedValues, o.resolve(r))
                        }

                        function k(a) {
                            p.$$failure = a, o.reject(a)
                        }

                        function n(c, e, f) {
                            function i(a) {
                                l.reject(a), k(a)
                            }

                            function j() {
                                if (!D(p.$$failure)) try {
                                    l.resolve(b.invoke(e, g, r)), l.promise.then(function(a) {
                                        r[c] = a, h()
                                    }, i)
                                } catch (a) {
                                    i(a)
                                }
                            }
                            var l = a.defer(),
                                m = 0;
                            I(f, function(a) {
                                q.hasOwnProperty(a) && !d.hasOwnProperty(a) && (m++, q[a].then(function(b) {
                                    r[a] = b, --m || j()
                                }, i))
                            }), m || j(), q[c] = l.promise
                        }
                        if (l(d) && g === c && (g = f, f = d, d = null), d) {
                            if (!G(d)) throw new Error("'locals' must be an object")
                        } else d = i;
                        if (f) {
                            if (!l(f)) throw new Error("'parent' must be a promise returned by $resolve.resolve()")
                        } else f = j;
                        var o = a.defer(),
                            p = o.promise,
                            q = p.$$promises = {},
                            r = J({}, d),
                            s = 1 + m.length / 3,
                            t = !1;
                        if (D(f.$$failure)) return k(f.$$failure), p;
                        f.$$inheritedValues && e(r, f.$$inheritedValues), f.$$values ? (t = e(r, f.$$values), p.$$inheritedValues = f.$$values, h()) : (f.$$inheritedValues && (p.$$inheritedValues = f.$$inheritedValues), J(q, f.$$promises), f.then(h, k));
                        for (var u = 0, v = m.length; v > u; u += 3) d.hasOwnProperty(m[u]) ? h() : n(m[u], m[u + 1], m[u + 2]);
                        return p
                    }
            }, this.resolve = function(a, b, c, d) {
                return this.study(a)(b, c, d)
            }
        }

        function m(a, b, c) {
            this.fromConfig = function(a, b, c) {
                return D(a.template) ? this.fromString(a.template, b) : D(a.templateUrl) ? this.fromUrl(a.templateUrl, b) : D(a.templateProvider) ? this.fromProvider(a.templateProvider, b, c) : null
            }, this.fromString = function(a, b) {
                return E(a) ? a(b) : a
            }, this.fromUrl = function(c, d) {
                return E(c) && (c = c(d)), null == c ? null : a.get(c, {
                    cache: b
                }).then(function(a) {
                    return a.data
                })
            }, this.fromProvider = function(a, b, d) {
                return c.invoke(a, null, d || {
                    params: b
                })
            }
        }

        function n(a, d) {
            function e(a) {
                return D(a) ? this.type.decode(a) : p.$$getDefaultValue(this)
            }

            function f(b, c, d) {
                if (!/^\w+(-+\w+)*$/.test(b)) throw new Error("Invalid parameter name '" + b + "' in pattern '" + a + "'");
                if (n[b]) throw new Error("Duplicate parameter name '" + b + "' in pattern '" + a + "'");
                n[b] = J({
                    type: c || new o,
                    $value: e
                }, d)
            }

            function g(a, b, c) {
                var d = a.replace(/[\\\[\]\^$*+?.()|{}]/g, "\\$&");
                if (!b) return d;
                var e = c ? "?" : "";
                return d + e + "(" + b + ")" + e
            }

            function h(a) {
                if (!d.params || !d.params[a]) return {};
                var b = d.params[a];
                return G(b) ? b : {
                    value: b
                }
            }
            d = b.isObject(d) ? d : {};
            var i, j = /([:*])(\w+)|\{(\w+)(?:\:((?:[^{}\\]+|\\.|\{(?:[^{}\\]+|\\.)*\})+))?\}/g,
                k = "^",
                l = 0,
                m = this.segments = [],
                n = this.params = {};
            this.source = a;
            for (var q, r, s, t, u;
                (i = j.exec(a)) && (q = i[2] || i[3], r = i[4] || ("*" == i[1] ? ".*" : "[^/]*"), s = a.substring(l, i.index), t = this.$types[r] || new o({
                    pattern: new RegExp(r)
                }), u = h(q), !(s.indexOf("?") >= 0));) k += g(s, t.$subPattern(), D(u.value)), f(q, t, u), m.push(s), l = j.lastIndex;
            s = a.substring(l);
            var v = s.indexOf("?");
            if (v >= 0) {
                var w = this.sourceSearch = s.substring(v);
                s = s.substring(0, v), this.sourcePath = a.substring(0, l + v), I(w.substring(1).split(/[&?]/), function(a) {
                    f(a, null, h(a))
                })
            } else this.sourcePath = a, this.sourceSearch = "";
            k += g(s) + (d.strict === !1 ? "/?" : "") + "$", m.push(s), this.regexp = new RegExp(k, d.caseInsensitive ? "i" : c), this.prefix = m[0]
        }

        function o(a) {
            J(this, a)
        }

        function p() {
            function a() {
                return {
                    strict: f,
                    caseInsensitive: e
                }
            }

            function b(a) {
                return E(a) || H(a) && E(a[a.length - 1])
            }

            function c() {
                I(h, function(a) {
                    if (n.prototype.$types[a.name]) throw new Error("A type named '" + a.name + "' has already been defined.");
                    var c = new o(b(a.def) ? d.invoke(a.def) : a.def);
                    n.prototype.$types[a.name] = c
                })
            }
            var d, e = !1,
                f = !0,
                g = !0,
                h = [],
                i = {
                    "int": {
                        decode: function(a) {
                            return parseInt(a, 10)
                        },
                        is: function(a) {
                            return D(a) ? this.decode(a.toString()) === a : !1
                        },
                        pattern: /\d+/
                    },
                    bool: {
                        encode: function(a) {
                            return a ? 1 : 0
                        },
                        decode: function(a) {
                            return 0 === parseInt(a, 10) ? !1 : !0
                        },
                        is: function(a) {
                            return a === !0 || a === !1
                        },
                        pattern: /0|1/
                    },
                    string: {
                        pattern: /[^\/]*/
                    },
                    date: {
                        equals: function(a, b) {
                            return a.toISOString() === b.toISOString()
                        },
                        decode: function(a) {
                            return new Date(a)
                        },
                        encode: function(a) {
                            return [a.getFullYear(), ("0" + (a.getMonth() + 1)).slice(-2), ("0" + a.getDate()).slice(-2)].join("-")
                        },
                        pattern: /[0-9]{4}-(?:0[1-9]|1[0-2])-(?:0[1-9]|[1-2][0-9]|3[0-1])/
                    }
                };
            p.$$getDefaultValue = function(a) {
                if (!b(a.value)) return a.value;
                if (!d) throw new Error("Injectable functions cannot be called at configuration time");
                return d.invoke(a.value)
            }, this.caseInsensitive = function(a) {
                e = a
            }, this.strictMode = function(a) {
                f = a
            }, this.compile = function(b, c) {
                return new n(b, J(a(), c))
            }, this.isMatcher = function(a) {
                if (!G(a)) return !1;
                var b = !0;
                return I(n.prototype, function(c, d) {
                    E(c) && (b = b && D(a[d]) && E(a[d]))
                }), b
            }, this.type = function(a, b) {
                return D(b) ? (h.push({
                    name: a,
                    def: b
                }), g || c(), this) : n.prototype.$types[a]
            }, this.$get = ["$injector", function(a) {
                return d = a, g = !1, n.prototype.$types = {}, c(), I(i, function(a, b) {
                    n.prototype.$types[b] || (n.prototype.$types[b] = new o(a))
                }), this
            }]
        }

        function q(a, b) {
            function d(a) {
                var b = /^\^((?:\\[^a-zA-Z0-9]|[^\\\[\]\^$*+?.()|{}]+)*)/.exec(a.source);
                return null != b ? b[1].replace(/\\(.)/g, "$1") : ""
            }

            function e(a, b) {
                return a.replace(/\$(\$|\d{1,2})/, function(a, c) {
                    return b["$" === c ? 0 : Number(c)]
                })
            }

            function f(a, b, c) {
                if (!c) return !1;
                var d = a.invoke(b, b, {
                    $match: c
                });
                return D(d) ? d : !0
            }

            function g(b, c, d, e) {
                function f(a, b, c) {
                    return "/" === m ? a : b ? m.slice(0, -1) + a : c ? m.slice(1) + a : a
                }

                function g(a) {
                    function c(a) {
                        var c = a(d, b);
                        return c ? (F(c) && b.replace().url(c), !0) : !1
                    }
                    if (!a || !a.defaultPrevented) {
                        var e, f = i.length;
                        for (e = 0; f > e; e++)
                            if (c(i[e])) return;
                        j && c(j)
                    }
                }

                function l() {
                    return h = h || c.$on("$locationChangeSuccess", g)
                }
                var m = e.baseHref(),
                    n = b.url();
                return k || l(), {
                    sync: function() {
                        g()
                    },
                    listen: function() {
                        return l()
                    },
                    update: function(a) {
                        return a ? void(n = b.url()) : void(b.url() !== n && (b.url(n), b.replace()))
                    },
                    push: function(a, c, d) {
                        b.url(a.format(c || {})), d && d.replace && b.replace()
                    },
                    href: function(c, d, e) {
                        if (!c.validates(d)) return null;
                        var g = a.html5Mode(),
                            h = c.format(d);
                        if (e = e || {}, g || null === h || (h = "#" + a.hashPrefix() + h), h = f(h, g, e.absolute), !e.absolute || !h) return h;
                        var i = !g && h ? "/" : "",
                            j = b.port();
                        return j = 80 === j || 443 === j ? "" : ":" + j, [b.protocol(), "://", b.host(), j, i, h].join("")
                    }
                }
            }
            var h, i = [],
                j = null,
                k = !1;
            this.rule = function(a) {
                if (!E(a)) throw new Error("'rule' must be a function");
                return i.push(a), this
            }, this.otherwise = function(a) {
                if (F(a)) {
                    var b = a;
                    a = function() {
                        return b
                    }
                } else if (!E(a)) throw new Error("'rule' must be a function");
                return j = a, this
            }, this.when = function(a, c) {
                var g, h = F(c);
                if (F(a) && (a = b.compile(a)), !h && !E(c) && !H(c)) throw new Error("invalid 'handler' in when()");
                var i = {
                        matcher: function(a, c) {
                            return h && (g = b.compile(c), c = ["$match", function(a) {
                                return g.format(a)
                            }]), J(function(b, d) {
                                return f(b, c, a.exec(d.path(), d.search()))
                            }, {
                                prefix: F(a.prefix) ? a.prefix : ""
                            })
                        },
                        regex: function(a, b) {
                            if (a.global || a.sticky) throw new Error("when() RegExp must not be global or sticky");
                            return h && (g = b, b = ["$match", function(a) {
                                return e(g, a)
                            }]), J(function(c, d) {
                                return f(c, b, a.exec(d.path()))
                            }, {
                                prefix: d(a)
                            })
                        }
                    },
                    j = {
                        matcher: b.isMatcher(a),
                        regex: a instanceof RegExp
                    };
                for (var k in j)
                    if (j[k]) return this.rule(i[k](a, c));
                throw new Error("invalid 'what' in when()")
            }, this.deferIntercept = function(a) {
                a === c && (a = !0), k = a
            }, this.$get = g, g.$inject = ["$location", "$rootScope", "$injector", "$browser"]
        }

        function r(a, e) {
            function f(a) {
                return 0 === a.indexOf(".") || 0 === a.indexOf("^")
            }

            function h(a, b) {
                if (!a) return c;
                var d = F(a),
                    e = d ? a : a.name,
                    g = f(e);
                if (g) {
                    if (!b) throw new Error("No reference point given for path '" + e + "'");
                    for (var h = e.split("."), i = 0, j = h.length, k = b; j > i; i++)
                        if ("" !== h[i] || 0 !== i) {
                            if ("^" !== h[i]) break;
                            if (!k.parent) throw new Error("Path '" + e + "' not valid for state '" + b.name + "'");
                            k = k.parent
                        } else k = b;
                    h = h.slice(i).join("."), e = k.name + (k.name && h ? "." : "") + h
                }
                var l = v[e];
                return !l || !d && (d || l !== a && l.self !== a) ? c : l
            }

            function l(a, b) {
                w[a] || (w[a] = []), w[a].push(b)
            }

            function m(b) {
                b = d(b, {
                    self: b,
                    resolve: b.resolve || {},
                    toString: function() {
                        return this.name
                    }
                });
                var c = b.name;
                if (!F(c) || c.indexOf("@") >= 0) throw new Error("State must have a valid name");
                if (v.hasOwnProperty(c)) throw new Error("State '" + c + "'' is already defined");
                var e = -1 !== c.indexOf(".") ? c.substring(0, c.lastIndexOf(".")) : F(b.parent) ? b.parent : "";
                if (e && !v[e]) return l(e, b.self);
                for (var f in y) E(y[f]) && (b[f] = y[f](b, y.$delegates[f]));
                if (v[c] = b, !b[x] && b.url && a.when(b.url, ["$match", "$stateParams", function(a, c) {
                        u.$current.navigable == b && j(a, c) || u.transitionTo(b, a, {
                            location: !1
                        })
                    }]), w[c])
                    for (var g = 0; g < w[c].length; g++) m(w[c][g]);
                return b
            }

            function n(a) {
                return a.indexOf("*") > -1
            }

            function o(a) {
                var b = a.split("."),
                    c = u.$current.name.split(".");
                if ("**" === b[0] && (c = c.slice(c.indexOf(b[1])), c.unshift("**")), "**" === b[b.length - 1] && (c.splice(c.indexOf(b[b.length - 2]) + 1, Number.MAX_VALUE), c.push("**")), b.length != c.length) return !1;
                for (var d = 0, e = b.length; e > d; d++) "*" === b[d] && (c[d] = "*");
                return c.join("") === b.join("")
            }

            function p(a, b) {
                return F(a) && !D(b) ? y[a] : E(b) && F(a) ? (y[a] && !y.$delegates[a] && (y.$delegates[a] = y[a]), y[a] = b, this) : this
            }

            function q(a, b) {
                return G(a) ? b = a : b.name = a, m(b), this
            }

            function r(a, e, f, l, m, p, q) {
                function r(b, c, d, f) {
                    var g = a.$broadcast("$stateNotFound", b, c, d);
                    if (g.defaultPrevented) return q.update(), A;
                    if (!g.retry) return null;
                    if (f.$retry) return q.update(), B;
                    var h = u.transition = e.when(g.retry);
                    return h.then(function() {
                        return h !== u.transition ? y : (b.options.$retry = !0, u.transitionTo(b.to, b.toParams, b.options))
                    }, function() {
                        return A
                    }), q.update(), h
                }

                function w(a, c, d, h, i) {
                    var j = d ? c : k(g(a.params), c),
                        n = {
                            $stateParams: j
                        };
                    i.resolve = m.resolve(a.resolve, n, i.resolve, a);
                    var o = [i.resolve.then(function(a) {
                        i.globals = a
                    })];
                    return h && o.push(h), I(a.views, function(c, d) {
                        var e = c.resolve && c.resolve !== a.resolve ? c.resolve : {};
                        e.$template = [function() {
                            return f.load(d, {
                                view: c,
                                locals: n,
                                params: j
                            }) || ""
                        }], o.push(m.resolve(e, n, i.resolve, a).then(function(f) {
                            if (E(c.controllerProvider) || H(c.controllerProvider)) {
                                var g = b.extend({}, e, n);
                                f.$$controller = l.invoke(c.controllerProvider, null, g)
                            } else f.$$controller = c.controller;
                            f.$$state = a, f.$$controllerAs = c.controllerAs, i[d] = f
                        }))
                    }), e.all(o).then(function() {
                        return i
                    })
                }
                var y = e.reject(new Error("transition superseded")),
                    z = e.reject(new Error("transition prevented")),
                    A = e.reject(new Error("transition aborted")),
                    B = e.reject(new Error("transition failed"));
                return t.locals = {
                    resolve: null,
                    globals: {
                        $stateParams: {}
                    }
                }, u = {
                    params: {},
                    current: t.self,
                    $current: t,
                    transition: null
                }, u.reload = function() {
                    u.transitionTo(u.current, p, {
                        reload: !0,
                        inherit: !1,
                        notify: !1
                    })
                }, u.go = function(a, b, c) {
                    return u.transitionTo(a, b, J({
                        inherit: !0,
                        relative: u.$current
                    }, c))
                }, u.transitionTo = function(b, c, f) {
                    c = c || {}, f = J({
                        location: !0,
                        inherit: !1,
                        relative: null,
                        notify: !0,
                        reload: !1,
                        $retry: !1
                    }, f || {});
                    var m, n = u.$current,
                        o = u.params,
                        v = n.path,
                        A = h(b, f.relative);
                    if (!D(A)) {
                        var B = {
                                to: b,
                                toParams: c,
                                options: f
                            },
                            C = r(B, n.self, o, f);
                        if (C) return C;
                        if (b = B.to, c = B.toParams, f = B.options, A = h(b, f.relative), !D(A)) {
                            if (!f.relative) throw new Error("No such state '" + b + "'");
                            throw new Error("Could not resolve '" + b + "' from state '" + f.relative + "'")
                        }
                    }
                    if (A[x]) throw new Error("Cannot transition to abstract state '" + b + "'");
                    f.inherit && (c = i(p, c || {}, u.$current, A)), b = A;
                    var E = b.path,
                        F = 0,
                        G = E[F],
                        H = t.locals,
                        I = [];
                    if (!f.reload)
                        for (; G && G === v[F] && j(c, o, G.ownParams);) H = I[F] = G.locals, F++, G = E[F];
                    if (s(b, n, H, f)) return b.self.reloadOnSearch !== !1 && q.update(), u.transition = null, e.when(u.current);
                    if (c = k(g(b.params), c || {}), f.notify && a.$broadcast("$stateChangeStart", b.self, c, n.self, o).defaultPrevented) return q.update(), z;
                    for (var L = e.when(H), M = F; M < E.length; M++, G = E[M]) H = I[M] = d(H), L = w(G, c, G === b, L, H);
                    var N = u.transition = L.then(function() {
                        var d, e, g;
                        if (u.transition !== N) return y;
                        for (d = v.length - 1; d >= F; d--) g = v[d], g.self.onExit && l.invoke(g.self.onExit, g.self, g.locals.globals), g.locals = null;
                        for (d = F; d < E.length; d++) e = E[d], e.locals = I[d], e.self.onEnter && l.invoke(e.self.onEnter, e.self, e.locals.globals);
                        return u.transition !== N ? y : (u.$current = b, u.current = b.self, u.params = c, K(u.params, p), u.transition = null, f.location && b.navigable && q.push(b.navigable.url, b.navigable.locals.globals.$stateParams, {
                            replace: "replace" === f.location
                        }), f.notify && a.$broadcast("$stateChangeSuccess", b.self, c, n.self, o), q.update(!0), u.current)
                    }, function(d) {
                        return u.transition !== N ? y : (u.transition = null, m = a.$broadcast("$stateChangeError", b.self, c, n.self, o, d), m.defaultPrevented || q.update(), e.reject(d))
                    });
                    return N
                }, u.is = function(a, d) {
                    var e = h(a);
                    return D(e) ? u.$current !== e ? !1 : D(d) && null !== d ? b.equals(p, d) : !0 : c
                }, u.includes = function(a, b) {
                    if (F(a) && n(a)) {
                        if (!o(a)) return !1;
                        a = u.$current.name
                    }
                    var d = h(a);
                    return D(d) ? D(u.$current.includes[d.name]) ? j(b, p) : !1 : c
                }, u.href = function(a, b, c) {
                    c = J({
                        lossy: !0,
                        inherit: !1,
                        absolute: !1,
                        relative: u.$current
                    }, c || {});
                    var d = h(a, c.relative);
                    if (!D(d)) return null;
                    c.inherit && (b = i(p, b || {}, u.$current, d));
                    var e = d && c.lossy ? d.navigable : d;
                    return e && e.url ? q.href(e.url, k(g(d.params), b || {}), {
                        absolute: c.absolute
                    }) : null
                }, u.get = function(a, b) {
                    if (0 === arguments.length) return g(v).map(function(a) {
                        return v[a].self
                    });
                    var c = h(a, b);
                    return c && c.self ? c.self : null
                }, u
            }

            function s(a, b, c, d) {
                return a !== b || (c !== b.locals || d.reload) && a.self.reloadOnSearch !== !1 ? void 0 : !0
            }
            var t, u, v = {},
                w = {},
                x = "abstract",
                y = {
                    parent: function(a) {
                        if (D(a.parent) && a.parent) return h(a.parent);
                        var b = /^(.+)\.[^.]+$/.exec(a.name);
                        return b ? h(b[1]) : t
                    },
                    data: function(a) {
                        return a.parent && a.parent.data && (a.data = a.self.data = J({}, a.parent.data, a.data)), a.data
                    },
                    url: function(a) {
                        var b = a.url,
                            c = {
                                params: a.params || {}
                            };
                        if (F(b)) return "^" == b.charAt(0) ? e.compile(b.substring(1), c) : (a.parent.navigable || t).url.concat(b, c);
                        if (!b || e.isMatcher(b)) return b;
                        throw new Error("Invalid url '" + b + "' in state '" + a + "'")
                    },
                    navigable: function(a) {
                        return a.url ? a : a.parent ? a.parent.navigable : null
                    },
                    params: function(a) {
                        return a.params ? a.params : a.url ? a.url.params : a.parent.params
                    },
                    views: function(a) {
                        var b = {};
                        return I(D(a.views) ? a.views : {
                            "": a
                        }, function(c, d) {
                            d.indexOf("@") < 0 && (d += "@" + a.parent.name), b[d] = c
                        }), b
                    },
                    ownParams: function(a) {
                        if (a.params = a.params || {}, !a.parent) return g(a.params);
                        var b = {};
                        I(a.params, function(a, c) {
                            b[c] = !0
                        }), I(a.parent.params, function(c, d) {
                            if (!b[d]) throw new Error("Missing required parameter '" + d + "' in state '" + a.name + "'");
                            b[d] = !1
                        });
                        var c = [];
                        return I(b, function(a, b) {
                            a && c.push(b)
                        }), c
                    },
                    path: function(a) {
                        return a.parent ? a.parent.path.concat(a) : []
                    },
                    includes: function(a) {
                        var b = a.parent ? J({}, a.parent.includes) : {};
                        return b[a.name] = !0, b
                    },
                    $delegates: {}
                };
            t = m({
                name: "",
                url: "^",
                views: null,
                "abstract": !0
            }), t.navigable = null, this.decorator = p, this.state = q, this.$get = r, r.$inject = ["$rootScope", "$q", "$view", "$injector", "$resolve", "$stateParams", "$urlRouter"]
        }

        function s() {
            function a(a, b) {
                return {
                    load: function(c, d) {
                        var e, f = {
                            template: null,
                            controller: null,
                            view: null,
                            locals: null,
                            notify: !0,
                            async: !0,
                            params: {}
                        };
                        return d = J(f, d), d.view && (e = b.fromConfig(d.view, d.params, d.locals)), e && d.notify && a.$broadcast("$viewContentLoading", d), e
                    }
                }
            }
            this.$get = a, a.$inject = ["$rootScope", "$templateFactory"]
        }

        function t() {
            var a = !1;
            this.useAnchorScroll = function() {
                a = !0
            }, this.$get = ["$anchorScroll", "$timeout", function(b, c) {
                return a ? b : function(a) {
                    c(function() {
                        a[0].scrollIntoView()
                    }, 0, !1)
                }
            }]
        }

        function u(a, c, d) {
            function e() {
                return c.has ? function(a) {
                    return c.has(a) ? c.get(a) : null
                } : function(a) {
                    try {
                        return c.get(a)
                    } catch (b) {
                        return null
                    }
                }
            }

            function f(a, b) {
                var c = function() {
                    return {
                        enter: function(a, b, c) {
                            b.after(a), c()
                        },
                        leave: function(a, b) {
                            a.remove(), b()
                        }
                    }
                };
                if (i) return {
                    enter: function(a, b, c) {
                        i.enter(a, null, b, c)
                    },
                    leave: function(a, b) {
                        i.leave(a, b)
                    }
                };
                if (h) {
                    var d = h && h(b, a);
                    return {
                        enter: function(a, b, c) {
                            d.enter(a, null, b), c()
                        },
                        leave: function(a, b) {
                            d.leave(a), b()
                        }
                    }
                }
                return c()
            }
            var g = e(),
                h = g("$animator"),
                i = g("$animate"),
                j = {
                    restrict: "ECA",
                    terminal: !0,
                    priority: 400,
                    transclude: "element",
                    compile: function(c, e, g) {
                        return function(c, e, h) {
                            function i() {
                                k && (k.remove(), k = null), m && (m.$destroy(), m = null), l && (q.leave(l, function() {
                                    k = null
                                }), k = l, l = null)
                            }

                            function j(f) {
                                var j = c.$new(),
                                    k = w(h, e.inheritedData("$uiView")),
                                    r = k && a.$current && a.$current.locals[k];
                                if (f || r !== n) {
                                    n = a.$current.locals[k];
                                    var s = g(j, function(a) {
                                        q.enter(a, e, function() {
                                            (b.isDefined(p) && !p || c.$eval(p)) && d(a)
                                        }), i()
                                    });
                                    l = s, m = j, m.$emit("$viewContentLoaded"), m.$eval(o)
                                }
                            }
                            var k, l, m, n, o = h.onload || "",
                                p = h.autoscroll,
                                q = f(h, c);
                            c.$on("$stateChangeSuccess", function() {
                                j(!1)
                            }), c.$on("$viewContentLoading", function() {
                                j(!1)
                            }), j(!0)
                        }
                    }
                };
            return j
        }

        function v(a, b, c) {
            return {
                restrict: "ECA",
                priority: -400,
                compile: function(d) {
                    var e = d.html();
                    return function(d, f, g) {
                        var h = c.$current,
                            i = w(g, f.inheritedData("$uiView")),
                            j = h && h.locals[i];
                        if (j) {
                            f.data("$uiView", {
                                name: i,
                                state: j.$$state
                            }), f.html(j.$template ? j.$template : e);
                            var k = a(f.contents());
                            if (j.$$controller) {
                                j.$scope = d;
                                var l = b(j.$$controller, j);
                                j.$$controllerAs && (d[j.$$controllerAs] = l), f.data("$ngControllerController", l), f.children().data("$ngControllerController", l)
                            }
                            k(d)
                        }
                    }
                }
            }
        }

        function w(a, b) {
            var c = a.uiView || a.name || "";
            return c.indexOf("@") >= 0 ? c : c + "@" + (b ? b.state.name : "")
        }

        function x(a, b) {
            var c, d = a.match(/^\s*({[^}]*})\s*$/);
            if (d && (a = b + "(" + d[1] + ")"), c = a.replace(/\n/g, " ").match(/^([^(]+?)\s*(\((.*)\))?$/), !c || 4 !== c.length) throw new Error("Invalid state ref '" + a + "'");
            return {
                state: c[1],
                paramExpr: c[3] || null
            }
        }

        function y(a) {
            var b = a.parent().inheritedData("$uiView");
            return b && b.state && b.state.name ? b.state : void 0
        }

        function z(a, c) {
            var d = ["location", "inherit", "reload"];
            return {
                restrict: "A",
                require: ["?^uiSrefActive", "?^uiSrefActiveEq"],
                link: function(e, f, g, h) {
                    var i = x(g.uiSref, a.current.name),
                        j = null,
                        k = y(f) || a.$current,
                        l = "FORM" === f[0].nodeName,
                        m = l ? "action" : "href",
                        n = !0,
                        o = {
                            relative: k,
                            inherit: !0
                        },
                        p = e.$eval(g.uiSrefOpts) || {};
                    b.forEach(d, function(a) {
                        a in p && (o[a] = p[a])
                    });
                    var q = function(b) {
                        if (b && (j = b), n) {
                            var c = a.href(i.state, j, o),
                                d = h[1] || h[0];
                            return d && d.$$setStateInfo(i.state, j), null === c ? (n = !1, !1) : void(f[0][m] = c)
                        }
                    };
                    i.paramExpr && (e.$watch(i.paramExpr, function(a) {
                        a !== j && q(a)
                    }, !0), j = e.$eval(i.paramExpr)), q(), l || f.bind("click", function(b) {
                        var d = b.which || b.button;
                        if (!(d > 1 || b.ctrlKey || b.metaKey || b.shiftKey || f.attr("target"))) {
                            var e = c(function() {
                                a.go(i.state, j, o)
                            });
                            b.preventDefault(), b.preventDefault = function() {
                                c.cancel(e)
                            }
                        }
                    })
                }
            }
        }

        function A(a, b, c) {
            return {
                restrict: "A",
                controller: ["$scope", "$element", "$attrs", function(d, e, f) {
                    function g() {
                        h() ? e.addClass(m) : e.removeClass(m)
                    }

                    function h() {
                        return "undefined" != typeof f.uiSrefActiveEq ? a.$current.self === k && i() : a.includes(k.name) && i()
                    }

                    function i() {
                        return !l || j(l, b)
                    }
                    var k, l, m;
                    m = c(f.uiSrefActiveEq || f.uiSrefActive || "", !1)(d), this.$$setStateInfo = function(b, c) {
                        k = a.get(b, y(e)), l = c, g()
                    }, d.$on("$stateChangeSuccess", g)
                }]
            }
        }

        function B(a) {
            return function(b) {
                return a.is(b)
            }
        }

        function C(a) {
            return function(b) {
                return a.includes(b)
            }
        }
        var D = b.isDefined,
            E = b.isFunction,
            F = b.isString,
            G = b.isObject,
            H = b.isArray,
            I = b.forEach,
            J = b.extend,
            K = b.copy;
        b.module("ui.router.util", ["ng"]), b.module("ui.router.router", ["ui.router.util"]), b.module("ui.router.state", ["ui.router.router", "ui.router.util"]), b.module("ui.router", ["ui.router.state"]), b.module("ui.router.compat", ["ui.router"]), l.$inject = ["$q", "$injector"], b.module("ui.router.util").service("$resolve", l), m.$inject = ["$http", "$templateCache", "$injector"], b.module("ui.router.util").service("$templateFactory", m), n.prototype.concat = function(a, b) {
            return new n(this.sourcePath + a + this.sourceSearch, b)
        }, n.prototype.toString = function() {
            return this.source
        }, n.prototype.exec = function(a, b) {
            var c = this.regexp.exec(a);
            if (!c) return null;
            b = b || {};
            var d, e, f, g = this.parameters(),
                h = g.length,
                i = this.segments.length - 1,
                j = {};
            if (i !== c.length - 1) throw new Error("Unbalanced capture group in route '" + this.source + "'");
            for (d = 0; i > d; d++) f = g[d], e = this.params[f], j[f] = e.$value(c[d + 1]);
            for (; h > d; d++) f = g[d], e = this.params[f], j[f] = e.$value(b[f]);
            return j
        }, n.prototype.parameters = function(a) {
            return D(a) ? this.params[a] || null : g(this.params)
        }, n.prototype.validates = function(a) {
            var b, c, d = !0,
                e = this;
            return I(a, function(a, f) {
                e.params[f] && (c = e.params[f], b = !a && D(c.value), d = d && (b || c.type.is(a)))
            }), d
        }, n.prototype.format = function(a) {
            var b = this.segments,
                c = this.parameters();
            if (!a) return b.join("").replace("//", "/");
            var d, e, f, g, h, i, j = b.length - 1,
                k = c.length,
                l = b[0];
            if (!this.validates(a)) return null;
            for (d = 0; j > d; d++) g = c[d], f = a[g], h = this.params[g], (D(f) || "/" !== b[d] && "/" !== b[d + 1]) && (null != f && (l += encodeURIComponent(h.type.encode(f))), l += b[d + 1]);
            for (; k > d; d++) g = c[d], f = a[g], null != f && (i = H(f), i && (f = f.map(encodeURIComponent).join("&" + g + "=")), l += (e ? "&" : "?") + g + "=" + (i ? f : encodeURIComponent(f)), e = !0);
            return l
        }, n.prototype.$types = {}, o.prototype.is = function() {
            return !0
        }, o.prototype.encode = function(a) {
            return a
        }, o.prototype.decode = function(a) {
            return a
        }, o.prototype.equals = function(a, b) {
            return a == b
        }, o.prototype.$subPattern = function() {
            var a = this.pattern.toString();
            return a.substr(1, a.length - 2)
        }, o.prototype.pattern = /.*/, b.module("ui.router.util").provider("$urlMatcherFactory", p), q.$inject = ["$locationProvider", "$urlMatcherFactoryProvider"], b.module("ui.router.router").provider("$urlRouter", q), r.$inject = ["$urlRouterProvider", "$urlMatcherFactoryProvider"], b.module("ui.router.state").value("$stateParams", {}).provider("$state", r), s.$inject = [], b.module("ui.router.state").provider("$view", s), b.module("ui.router.state").provider("$uiViewScroll", t), u.$inject = ["$state", "$injector", "$uiViewScroll"], v.$inject = ["$compile", "$controller", "$state"], b.module("ui.router.state").directive("uiView", u), b.module("ui.router.state").directive("uiView", v), z.$inject = ["$state", "$timeout"], A.$inject = ["$state", "$stateParams", "$interpolate"], b.module("ui.router.state").directive("uiSref", z).directive("uiSrefActive", A).directive("uiSrefActiveEq", A), B.$inject = ["$state"], C.$inject = ["$state"], b.module("ui.router.state").filter("isState", B).filter("includedByState", C)
    }(window, window.angular);

;// JS/angular/ui-bootstrap-0.11.2.js
// angular/ui-bootstrap-0.11.2.js
angular.module("ui.bootstrap", ["ui.bootstrap.transition", "ui.bootstrap.collapse", "ui.bootstrap.accordion", "ui.bootstrap.alert", "ui.bootstrap.bindHtml", "ui.bootstrap.buttons", "ui.bootstrap.carousel", "ui.bootstrap.dateparser", "ui.bootstrap.position", "ui.bootstrap.datepicker", "ui.bootstrap.dropdown", "ui.bootstrap.modal", "ui.bootstrap.pagination", "ui.bootstrap.tooltip", "ui.bootstrap.popover", "ui.bootstrap.progressbar", "ui.bootstrap.rating", "ui.bootstrap.tabs", "ui.bootstrap.timepicker", "ui.bootstrap.typeahead"]), angular.module("ui.bootstrap.transition", []).factory("$transition", ["$q", "$timeout", "$rootScope", function(n, t, i) {
    function u(n) {
        for (var t in n)
            if (f.style[t] !== undefined) return n[t]
    }
    var r = function(u, f, e) {
            e = e || {};
            var s = n.defer(),
                o = r[e.animation ? "animationEndEventName" : "transitionEndEventName"],
                h = function() {
                    i.$apply(function() {
                        u.unbind(o, h), s.resolve(u)
                    })
                };
            return o && u.bind(o, h), t(function() {
                angular.isString(f) ? u.addClass(f) : angular.isFunction(f) ? f(u) : angular.isObject(f) && u.css(f), o || s.resolve(u)
            }), s.promise.cancel = function() {
                o && u.unbind(o, h), s.reject("Transition cancelled")
            }, s.promise
        },
        f = document.createElement("trans"),
        e = {
            WebkitTransition: "webkitTransitionEnd",
            MozTransition: "transitionend",
            OTransition: "oTransitionEnd",
            transition: "transitionend"
        },
        o = {
            WebkitTransition: "webkitAnimationEnd",
            MozTransition: "animationend",
            OTransition: "oAnimationEnd",
            transition: "animationend"
        };
    return r.transitionEndEventName = u(e), r.animationEndEventName = u(o), r
}]), angular.module("ui.bootstrap.collapse", ["ui.bootstrap.transition"]).directive("collapse", ["$transition", function(n) {
    return {
        link: function(t, i, r) {
            function e(t) {
                function f() {
                    u === r && (u = undefined)
                }
                var r = n(i, t);
                return u && u.cancel(), u = r, r.then(f, f), r
            }

            function h() {
                f ? (f = !1, o()) : (i.removeClass("collapse").addClass("collapsing"), e({
                    height: i[0].scrollHeight + "px"
                }).then(o))
            }

            function o() {
                i.removeClass("collapsing"), i.addClass("collapse in"), i.css({
                    height: "auto"
                })
            }

            function c() {
                if (f) f = !1, s(), i.css({
                    height: 0
                });
                else {
                    i.css({
                        height: i[0].scrollHeight + "px"
                    });
                    var n = i[0].offsetWidth;
                    i.removeClass("collapse in").addClass("collapsing"), e({
                        height: 0
                    }).then(s)
                }
            }

            function s() {
                i.removeClass("collapsing"), i.addClass("collapse")
            }
            var f = !0,
                u;
            t.$watch(r.collapse, function(n) {
                n ? c() : h()
            })
        }
    }
}]), angular.module("ui.bootstrap.accordion", ["ui.bootstrap.collapse"]).constant("accordionConfig", {
    closeOthers: !0
}).controller("AccordionController", ["$scope", "$attrs", "accordionConfig", function(n, t, i) {
    this.groups = [], this.closeOthers = function(r) {
        var u = angular.isDefined(t.closeOthers) ? n.$eval(t.closeOthers) : i.closeOthers;
        u && angular.forEach(this.groups, function(n) {
            n !== r && (n.isOpen = !1)
        })
    }, this.addGroup = function(n) {
        var t = this;
        this.groups.push(n), n.$on("$destroy", function() {
            t.removeGroup(n)
        })
    }, this.removeGroup = function(n) {
        var t = this.groups.indexOf(n);
        t !== -1 && this.groups.splice(t, 1)
    }
}]).directive("accordion", function() {
    return {
        restrict: "EA",
        controller: "AccordionController",
        transclude: !0,
        replace: !1,
        templateUrl: "template/accordion/accordion.html"
    }
}).directive("accordionGroup", function() {
    return {
        require: "^accordion",
        restrict: "EA",
        transclude: !0,
        replace: !0,
        templateUrl: "template/accordion/accordion-group.html",
        scope: {
            heading: "@",
            isOpen: "=?",
            isDisabled: "=?"
        },
        controller: function() {
            this.setHeading = function(n) {
                this.heading = n
            }
        },
        link: function(n, t, i, r) {
            r.addGroup(n), n.$watch("isOpen", function(t) {
                t && r.closeOthers(n)
            }), n.toggleOpen = function() {
                n.isDisabled || (n.isOpen = !n.isOpen)
            }
        }
    }
}).directive("accordionHeading", function() {
    return {
        restrict: "EA",
        transclude: !0,
        template: "",
        replace: !0,
        require: "^accordionGroup",
        link: function(n, t, i, r, u) {
            r.setHeading(u(n, function() {}))
        }
    }
}).directive("accordionTransclude", function() {
    return {
        require: "^accordionGroup",
        link: function(n, t, i, r) {
            n.$watch(function() {
                return r[i.accordionTransclude]
            }, function(n) {
                n && (t.html(""), t.append(n))
            })
        }
    }
}), angular.module("ui.bootstrap.alert", []).controller("AlertController", ["$scope", "$attrs", function(n, t) {
    n.closeable = "close" in t
}]).directive("alert", function() {
    return {
        restrict: "EA",
        controller: "AlertController",
        templateUrl: "template/alert/alert.html",
        transclude: !0,
        replace: !0,
        scope: {
            type: "@",
            close: "&"
        }
    }
}), angular.module("ui.bootstrap.bindHtml", []).directive("bindHtmlUnsafe", function() {
    return function(n, t, i) {
        t.addClass("ng-binding").data("$binding", i.bindHtmlUnsafe), n.$watch(i.bindHtmlUnsafe, function(n) {
            t.html(n || "")
        })
    }
}), angular.module("ui.bootstrap.buttons", []).constant("buttonConfig", {
    activeClass: "active",
    toggleEvent: "click"
}).controller("ButtonsController", ["buttonConfig", function(n) {
    this.activeClass = n.activeClass || "active", this.toggleEvent = n.toggleEvent || "click"
}]).directive("btnRadio", function() {
    return {
        require: ["btnRadio", "ngModel"],
        controller: "ButtonsController",
        link: function(n, t, i, r) {
            var f = r[0],
                u = r[1];
            u.$render = function() {
                t.toggleClass(f.activeClass, angular.equals(u.$modelValue, n.$eval(i.btnRadio)))
            }, t.bind(f.toggleEvent, function() {
                var r = t.hasClass(f.activeClass);
                (!r || angular.isDefined(i.uncheckable)) && n.$apply(function() {
                    u.$setViewValue(r ? null : n.$eval(i.btnRadio)), u.$render()
                })
            })
        }
    }
}).directive("btnCheckbox", function() {
    return {
        require: ["btnCheckbox", "ngModel"],
        controller: "ButtonsController",
        link: function(n, t, i, r) {
            function e() {
                return o(i.btnCheckboxTrue, !0)
            }

            function s() {
                return o(i.btnCheckboxFalse, !1)
            }

            function o(t, i) {
                var r = n.$eval(t);
                return angular.isDefined(r) ? r : i
            }
            var f = r[0],
                u = r[1];
            u.$render = function() {
                t.toggleClass(f.activeClass, angular.equals(u.$modelValue, e()))
            }, t.bind(f.toggleEvent, function() {
                n.$apply(function() {
                    u.$setViewValue(t.hasClass(f.activeClass) ? s() : e()), u.$render()
                })
            })
        }
    }
}), angular.module("ui.bootstrap.carousel", ["ui.bootstrap.transition"]).controller("CarouselController", ["$scope", "$timeout", "$transition", function(n, t, i) {
    function s() {
        c();
        var i = +n.interval;
        !isNaN(i) && i >= 0 && (e = t(l, i))
    }

    function c() {
        e && (t.cancel(e), e = null)
    }

    function l() {
        o ? (n.next(), s()) : n.pause()
    }
    var r = this,
        u = r.slides = n.slides = [],
        f = -1,
        e, o, h;
    r.currentSlide = null, h = !1, r.select = n.select = function(e, o) {
        function a() {
            if (!h) {
                if (r.currentSlide && angular.isString(o) && !n.noTransition && e.$element) {
                    e.$element.addClass(o);
                    var t = e.$element[0].offsetWidth;
                    angular.forEach(u, function(n) {
                            angular.extend(n, {
                                direction: "",
                                entering: !1,
                                leaving: !1,
                                active: !1
                            })
                        }), angular.extend(e, {
                            direction: o,
                            active: !0,
                            entering: !0
                        }), angular.extend(r.currentSlide || {}, {
                            direction: o,
                            leaving: !0
                        }), n.$currentTransition = i(e.$element, {}),
                        function(t, i) {
                            n.$currentTransition.then(function() {
                                c(t, i)
                            }, function() {
                                c(t, i)
                            })
                        }(e, r.currentSlide)
                } else c(e, r.currentSlide);
                r.currentSlide = e, f = l, s()
            }
        }

        function c(t, i) {
            angular.extend(t, {
                direction: "",
                active: !0,
                leaving: !1,
                entering: !1
            }), angular.extend(i || {}, {
                direction: "",
                active: !1,
                leaving: !1,
                entering: !1
            }), n.$currentTransition = null
        }
        var l = u.indexOf(e);
        o === undefined && (o = l > f ? "next" : "prev"), e && e !== r.currentSlide && (n.$currentTransition ? (n.$currentTransition.cancel(), t(a)) : a())
    }, n.$on("$destroy", function() {
        h = !0
    }), r.indexOfSlide = function(n) {
        return u.indexOf(n)
    }, n.next = function() {
        var t = (f + 1) % u.length;
        if (!n.$currentTransition) return r.select(u[t], "next")
    }, n.prev = function() {
        var t = f - 1 < 0 ? u.length - 1 : f - 1;
        if (!n.$currentTransition) return r.select(u[t], "prev")
    }, n.isActive = function(n) {
        return r.currentSlide === n
    }, n.$watch("interval", s), n.$on("$destroy", c), n.play = function() {
        o || (o = !0, s())
    }, n.pause = function() {
        n.noPause || (o = !1, c())
    }, r.addSlide = function(t, i) {
        t.$element = i, u.push(t), u.length === 1 || t.active ? (r.select(u[u.length - 1]), u.length == 1 && n.play()) : t.active = !1
    }, r.removeSlide = function(n) {
        var t = u.indexOf(n);
        u.splice(t, 1), u.length > 0 && n.active ? t >= u.length ? r.select(u[t - 1]) : r.select(u[t]) : f > t && f--
    }
}]).directive("carousel", [function() {
    return {
        restrict: "EA",
        transclude: !0,
        replace: !0,
        controller: "CarouselController",
        require: "carousel",
        templateUrl: "template/carousel/carousel.html",
        scope: {
            interval: "=",
            noTransition: "=",
            noPause: "="
        }
    }
}]).directive("slide", function() {
    return {
        require: "^carousel",
        restrict: "EA",
        transclude: !0,
        replace: !0,
        templateUrl: "template/carousel/slide.html",
        scope: {
            active: "=?"
        },
        link: function(n, t, i, r) {
            r.addSlide(n, t), n.$on("$destroy", function() {
                r.removeSlide(n)
            }), n.$watch("active", function(t) {
                t && r.select(n)
            })
        }
    }
}), angular.module("ui.bootstrap.dateparser", []).service("dateParser", ["$locale", "orderByFilter", function(n, t) {
    function r(n) {
        var u = [],
            r = n.split("");
        return angular.forEach(i, function(t, i) {
            var f = n.indexOf(i),
                e, o;
            if (f > -1) {
                for (n = n.split(""), r[f] = "(" + t.regex + ")", n[f] = "$", e = f + 1, o = f + i.length; e < o; e++) r[e] = "", n[e] = "$";
                n = n.join(""), u.push({
                    index: f,
                    apply: t.apply
                })
            }
        }), {
            regex: new RegExp("^" + r.join("") + "$"),
            map: t(u, "index")
        }
    }

    function u(n, t, i) {
        return t === 1 && i > 28 ? i === 29 && (n % 4 == 0 && n % 100 != 0 || n % 400 == 0) : t === 3 || t === 5 || t === 8 || t === 10 ? i < 31 : !0
    }
    this.parsers = {};
    var i = {
        yyyy: {
            regex: "\\d{4}",
            apply: function(n) {
                this.year = +n
            }
        },
        yy: {
            regex: "\\d{2}",
            apply: function(n) {
                this.year = +n + 2e3
            }
        },
        y: {
            regex: "\\d{1,4}",
            apply: function(n) {
                this.year = +n
            }
        },
        MMMM: {
            regex: n.DATETIME_FORMATS.MONTH.join("|"),
            apply: function(t) {
                this.month = n.DATETIME_FORMATS.MONTH.indexOf(t)
            }
        },
        MMM: {
            regex: n.DATETIME_FORMATS.SHORTMONTH.join("|"),
            apply: function(t) {
                this.month = n.DATETIME_FORMATS.SHORTMONTH.indexOf(t)
            }
        },
        MM: {
            regex: "0[1-9]|1[0-2]",
            apply: function(n) {
                this.month = n - 1
            }
        },
        M: {
            regex: "[1-9]|1[0-2]",
            apply: function(n) {
                this.month = n - 1
            }
        },
        dd: {
            regex: "[0-2][0-9]{1}|3[0-1]{1}",
            apply: function(n) {
                this.date = +n
            }
        },
        d: {
            regex: "[1-2]?[0-9]{1}|3[0-1]{1}",
            apply: function(n) {
                this.date = +n
            }
        },
        EEEE: {
            regex: n.DATETIME_FORMATS.DAY.join("|")
        },
        EEE: {
            regex: n.DATETIME_FORMATS.SHORTDAY.join("|")
        }
    };
    this.parse = function(t, i) {
        var f, c, e, l, s;
        if (!angular.isString(t) || !i) return t;
        i = n.DATETIME_FORMATS[i] || i, this.parsers[i] || (this.parsers[i] = r(i));
        var h = this.parsers[i],
            a = h.regex,
            v = h.map,
            o = t.match(a);
        if (o && o.length) {
            for (f = {
                    year: 1900,
                    month: 0,
                    date: 1,
                    hours: 0
                }, e = 1, l = o.length; e < l; e++) s = v[e - 1], s.apply && s.apply.call(f, o[e]);
            return u(f.year, f.month, f.date) && (c = new Date(f.year, f.month, f.date, f.hours)), c
        }
    }
}]), angular.module("ui.bootstrap.position", []).factory("$position", ["$document", "$window", function(n, t) {
    function i(n, i) {
        return n.currentStyle ? n.currentStyle[i] : t.getComputedStyle ? t.getComputedStyle(n)[i] : n.style[i]
    }

    function r(n) {
        return (i(n, "position") || "static") === "static"
    }
    var u = function(t) {
        for (var u = n[0], i = t.offsetParent || u; i && i !== u && r(i);) i = i.offsetParent;
        return i || u
    };
    return {
        position: function(t) {
            var e = this.offset(t),
                r = {
                    top: 0,
                    left: 0
                },
                i = u(t[0]),
                f;
            return i != n[0] && (r = this.offset(angular.element(i)), r.top += i.clientTop - i.scrollTop, r.left += i.clientLeft - i.scrollLeft), f = t[0].getBoundingClientRect(), {
                width: f.width || t.prop("offsetWidth"),
                height: f.height || t.prop("offsetHeight"),
                top: e.top - r.top,
                left: e.left - r.left
            }
        },
        offset: function(i) {
            var r = i[0].getBoundingClientRect();
            return {
                width: r.width || i.prop("offsetWidth"),
                height: r.height || i.prop("offsetHeight"),
                top: r.top + (t.pageYOffset || n[0].documentElement.scrollTop),
                left: r.left + (t.pageXOffset || n[0].documentElement.scrollLeft)
            }
        },
        positionElements: function(n, t, i, r) {
            var a = i.split("-"),
                h = a[0],
                e = a[1] || "center",
                u, c, l, f, o, s;
            u = r ? this.offset(n) : this.position(n), c = t.prop("offsetWidth"), l = t.prop("offsetHeight"), o = {
                center: function() {
                    return u.left + u.width / 2 - c / 2
                },
                left: function() {
                    return u.left
                },
                right: function() {
                    return u.left + u.width
                }
            }, s = {
                center: function() {
                    return u.top + u.height / 2 - l / 2
                },
                top: function() {
                    return u.top
                },
                bottom: function() {
                    return u.top + u.height
                }
            };
            switch (h) {
                case "right":
                    f = {
                        top: s[e](),
                        left: o[h]()
                    };
                    break;
                case "left":
                    f = {
                        top: s[e](),
                        left: u.left - c
                    };
                    break;
                case "bottom":
                    f = {
                        top: s[h](),
                        left: o[e]()
                    };
                    break;
                default:
                    f = {
                        top: u.top - l,
                        left: o[e]()
                    }
            }
            return f
        }
    }
}]), angular.module("ui.bootstrap.datepicker", ["ui.bootstrap.dateparser", "ui.bootstrap.position"]).constant("datepickerConfig", {
    formatDay: "dd",
    formatMonth: "MMMM",
    formatYear: "yyyy",
    formatDayHeader: "EEE",
    formatDayTitle: "MMMM yyyy",
    formatMonthTitle: "yyyy",
    datepickerMode: "day",
    minMode: "day",
    maxMode: "year",
    showWeeks: !0,
    startingDay: 0,
    yearRange: 20,
    minDate: null,
    maxDate: null
}).controller("DatepickerController", ["$scope", "$attrs", "$parse", "$interpolate", "$timeout", "$log", "dateFilter", "datepickerConfig", function(n, t, i, r, u, f, e, o) {
    var s = this,
        h = {
            $setViewValue: angular.noop
        },
        c;
    this.modes = ["day", "month", "year"], angular.forEach(["formatDay", "formatMonth", "formatYear", "formatDayHeader", "formatDayTitle", "formatMonthTitle", "minMode", "maxMode", "showWeeks", "startingDay", "yearRange"], function(i, u) {
        s[i] = angular.isDefined(t[i]) ? u < 8 ? r(t[i])(n.$parent) : n.$parent.$eval(t[i]) : o[i]
    }), angular.forEach(["minDate", "maxDate"], function(r) {
        t[r] ? n.$parent.$watch(i(t[r]), function(n) {
            s[r] = n ? new Date(n) : null, s.refreshView()
        }) : s[r] = o[r] ? new Date(o[r]) : null
    }), n.datepickerMode = n.datepickerMode || o.datepickerMode, n.uniqueId = "datepicker-" + n.$id + "-" + Math.floor(Math.random() * 1e4), this.activeDate = angular.isDefined(t.initDate) ? n.$parent.$eval(t.initDate) : new Date, n.isActive = function(t) {
        return s.compare(t.date, s.activeDate) === 0 ? (n.activeDateId = t.uid, !0) : !1
    }, this.init = function(n) {
        h = n, h.$render = function() {
            s.render()
        }
    }, this.render = function() {
        if (h.$modelValue) {
            var n = new Date(h.$modelValue),
                t = !isNaN(n);
            t ? this.activeDate = n : f.error('Datepicker directive: "ng-model" value must be a Date object, a number of milliseconds since 01.01.1970 or a string representing an RFC2822 or ISO 8601 date.'), h.$setValidity("date", t)
        }
        this.refreshView()
    }, this.refreshView = function() {
        if (this.element) {
            this._refreshView();
            var n = h.$modelValue ? new Date(h.$modelValue) : null;
            h.$setValidity("date-disabled", !n || this.element && !this.isDisabled(n))
        }
    }, this.createDateObject = function(n, t) {
        var i = h.$modelValue ? new Date(h.$modelValue) : null;
        return {
            date: n,
            label: e(n, t),
            selected: i && this.compare(n, i) === 0,
            disabled: this.isDisabled(n),
            current: this.compare(n, new Date) === 0
        }
    }, this.isDisabled = function(i) {
        return this.minDate && this.compare(i, this.minDate) < 0 || this.maxDate && this.compare(i, this.maxDate) > 0 || t.dateDisabled && n.dateDisabled({
            date: i,
            mode: n.datepickerMode
        })
    }, this.split = function(n, t) {
        for (var i = []; n.length > 0;) i.push(n.splice(0, t));
        return i
    }, n.select = function(t) {
        if (n.datepickerMode === s.minMode) {
            var i = h.$modelValue ? new Date(h.$modelValue) : new Date(0, 0, 0, 0, 0, 0, 0);
            i.setFullYear(t.getFullYear(), t.getMonth(), t.getDate()), h.$setViewValue(i), h.$render()
        } else s.activeDate = t, n.datepickerMode = s.modes[s.modes.indexOf(n.datepickerMode) - 1]
    }, n.move = function(n) {
        var t = s.activeDate.getFullYear() + n * (s.step.years || 0),
            i = s.activeDate.getMonth() + n * (s.step.months || 0);
        s.activeDate.setFullYear(t, i, 1), s.refreshView()
    }, n.toggleMode = function(t) {
        (t = t || 1, (n.datepickerMode !== s.maxMode || t !== 1) && (n.datepickerMode !== s.minMode || t !== -1)) && (n.datepickerMode = s.modes[s.modes.indexOf(n.datepickerMode) + t])
    }, n.keys = {
        13: "enter",
        32: "space",
        33: "pageup",
        34: "pagedown",
        35: "end",
        36: "home",
        37: "left",
        38: "up",
        39: "right",
        40: "down"
    }, c = function() {
        u(function() {
            s.element[0].focus()
        }, 0, !1)
    }, n.$on("datepicker.focus", c), n.keydown = function(t) {
        var i = n.keys[t.which];
        if (i && !t.shiftKey && !t.altKey)
            if (t.preventDefault(), t.stopPropagation(), i === "enter" || i === "space") {
                if (s.isDisabled(s.activeDate)) return;
                n.select(s.activeDate), c()
            } else t.ctrlKey && (i === "up" || i === "down") ? (n.toggleMode(i === "up" ? 1 : -1), c()) : (s.handleKeyDown(i, t), s.refreshView())
    }
}]).directive("datepicker", function() {
    return {
        restrict: "EA",
        replace: !0,
        templateUrl: "template/datepicker/datepicker.html",
        scope: {
            datepickerMode: "=?",
            dateDisabled: "&"
        },
        require: ["datepicker", "?^ngModel"],
        controller: "DatepickerController",
        link: function(n, t, i, r) {
            var f = r[0],
                u = r[1];
            u && f.init(u)
        }
    }
}).directive("daypicker", ["dateFilter", function(n) {
    return {
        restrict: "EA",
        replace: !0,
        templateUrl: "template/datepicker/day.html",
        require: "^datepicker",
        link: function(t, i, r, u) {
            function f(n, t) {
                return t === 1 && n % 4 == 0 && (n % 100 != 0 || n % 400 == 0) ? 29 : e[t]
            }

            function o(n, t) {
                var r = new Array(t),
                    i = new Date(n),
                    u = 0;
                for (i.setHours(12); u < t;) r[u++] = new Date(i), i.setDate(i.getDate() + 1);
                return r
            }

            function s(n) {
                var t = new Date(n),
                    i;
                return t.setDate(t.getDate() + 4 - (t.getDay() || 7)), i = t.getTime(), t.setMonth(0), t.setDate(1), Math.floor(Math.round((i - t) / 864e5) / 7) + 1
            }
            t.showWeeks = u.showWeeks, u.step = {
                months: 1
            }, u.element = i;
            var e = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
            u._refreshView = function() {
                var p = u.activeDate.getFullYear(),
                    h = u.activeDate.getMonth(),
                    c = new Date(p, h, 1),
                    e = u.startingDay - c.getDay(),
                    l = e > 0 ? 7 - e : -e,
                    a = new Date(c),
                    i, r, f, v, y;
                for (l > 0 && a.setDate(-l + 1), i = o(a, 42), r = 0; r < 42; r++) i[r] = angular.extend(u.createDateObject(i[r], u.formatDay), {
                    secondary: i[r].getMonth() !== h,
                    uid: t.uniqueId + "-" + r
                });
                for (t.labels = new Array(7), f = 0; f < 7; f++) t.labels[f] = {
                    abbr: n(i[f].date, u.formatDayHeader),
                    full: n(i[f].date, "EEEE")
                };
                if (t.title = n(u.activeDate, u.formatDayTitle), t.rows = u.split(i, 7), t.showWeeks)
                    for (t.weekNumbers = [], v = s(t.rows[0][0].date), y = t.rows.length; t.weekNumbers.push(v++) < y;);
            }, u.compare = function(n, t) {
                return new Date(n.getFullYear(), n.getMonth(), n.getDate()) - new Date(t.getFullYear(), t.getMonth(), t.getDate())
            }, u.handleKeyDown = function(n) {
                var i = u.activeDate.getDate(),
                    r;
                n === "left" ? i = i - 1 : n === "up" ? i = i - 7 : n === "right" ? i = i + 1 : n === "down" ? i = i + 7 : n === "pageup" || n === "pagedown" ? (r = u.activeDate.getMonth() + (n === "pageup" ? -1 : 1), u.activeDate.setMonth(r, 1), i = Math.min(f(u.activeDate.getFullYear(), u.activeDate.getMonth()), i)) : n === "home" ? i = 1 : n === "end" && (i = f(u.activeDate.getFullYear(), u.activeDate.getMonth())), u.activeDate.setDate(i)
            }, u.refreshView()
        }
    }
}]).directive("monthpicker", ["dateFilter", function(n) {
    return {
        restrict: "EA",
        replace: !0,
        templateUrl: "template/datepicker/month.html",
        require: "^datepicker",
        link: function(t, i, r, u) {
            u.step = {
                years: 1
            }, u.element = i, u._refreshView = function() {
                for (var r = new Array(12), f = u.activeDate.getFullYear(), i = 0; i < 12; i++) r[i] = angular.extend(u.createDateObject(new Date(f, i, 1), u.formatMonth), {
                    uid: t.uniqueId + "-" + i
                });
                t.title = n(u.activeDate, u.formatMonthTitle), t.rows = u.split(r, 3)
            }, u.compare = function(n, t) {
                return new Date(n.getFullYear(), n.getMonth()) - new Date(t.getFullYear(), t.getMonth())
            }, u.handleKeyDown = function(n) {
                var i = u.activeDate.getMonth(),
                    r;
                n === "left" ? i = i - 1 : n === "up" ? i = i - 3 : n === "right" ? i = i + 1 : n === "down" ? i = i + 3 : n === "pageup" || n === "pagedown" ? (r = u.activeDate.getFullYear() + (n === "pageup" ? -1 : 1), u.activeDate.setFullYear(r)) : n === "home" ? i = 0 : n === "end" && (i = 11), u.activeDate.setMonth(i)
            }, u.refreshView()
        }
    }
}]).directive("yearpicker", ["dateFilter", function() {
    return {
        restrict: "EA",
        replace: !0,
        templateUrl: "template/datepicker/year.html",
        require: "^datepicker",
        link: function(n, t, i, r) {
            function f(n) {
                return parseInt((n - 1) / u, 10) * u + 1
            }
            var u = r.yearRange;
            r.step = {
                years: u
            }, r.element = t, r._refreshView = function() {
                for (var i = new Array(u), t = 0, e = f(r.activeDate.getFullYear()); t < u; t++) i[t] = angular.extend(r.createDateObject(new Date(e + t, 0, 1), r.formatYear), {
                    uid: n.uniqueId + "-" + t
                });
                n.title = [i[0].label, i[u - 1].label].join(" - "), n.rows = r.split(i, 5)
            }, r.compare = function(n, t) {
                return n.getFullYear() - t.getFullYear()
            }, r.handleKeyDown = function(n) {
                var i = r.activeDate.getFullYear();
                n === "left" ? i = i - 1 : n === "up" ? i = i - 5 : n === "right" ? i = i + 1 : n === "down" ? i = i + 5 : n === "pageup" || n === "pagedown" ? i += (n === "pageup" ? -1 : 1) * r.step.years : n === "home" ? i = f(r.activeDate.getFullYear()) : n === "end" && (i = f(r.activeDate.getFullYear()) + u - 1), r.activeDate.setFullYear(i)
            }, r.refreshView()
        }
    }
}]).constant("datepickerPopupConfig", {
    datepickerPopup: "yyyy-MM-dd",
    currentText: "Today",
    clearText: "Clear",
    closeText: "Done",
    closeOnDateSelection: !0,
    appendToBody: !1,
    showButtonBar: !0
}).directive("datepickerPopup", ["$compile", "$parse", "$document", "$position", "dateFilter", "dateParser", "datepickerPopupConfig", function(n, t, i, r, u, f, e) {
    return {
        restrict: "EA",
        require: "ngModel",
        scope: {
            isOpen: "=?",
            currentText: "@",
            clearText: "@",
            closeText: "@",
            dateDisabled: "&"
        },
        link: function(o, s, h, c) {
            function k(n) {
                return n.replace(/([A-Z])/g, function(n) {
                    return "-" + n.toLowerCase()
                })
            }

            function d(n) {
                if (n) {
                    if (angular.isDate(n) && !isNaN(n)) return c.$setValidity("date", !0), n;
                    if (angular.isString(n)) {
                        var t = f.parse(n, p) || new Date(n);
                        return isNaN(t) ? (c.$setValidity("date", !1), undefined) : (c.$setValidity("date", !0), t)
                    }
                    return c.$setValidity("date", !1), undefined
                }
                return c.$setValidity("date", !0), null
            }
            var p, g = angular.isDefined(h.closeOnDateSelection) ? o.$parent.$eval(h.closeOnDateSelection) : e.closeOnDateSelection,
                b = angular.isDefined(h.datepickerAppendToBody) ? o.$parent.$eval(h.datepickerAppendToBody) : e.appendToBody,
                l, a, v, w, y;
            o.showButtonBar = angular.isDefined(h.showButtonBar) ? o.$parent.$eval(h.showButtonBar) : e.showButtonBar, o.getText = function(n) {
                return o[n + "Text"] || e[n + "Text"]
            }, h.$observe("datepickerPopup", function(n) {
                p = n || e.datepickerPopup, c.$render()
            }), l = angular.element("<div datepicker-popup-wrap><div datepicker></div></div>"), l.attr({
                "ng-model": "date",
                "ng-change": "dateSelection()"
            }), a = angular.element(l.children()[0]), h.datepickerOptions && angular.forEach(o.$parent.$eval(h.datepickerOptions), function(n, t) {
                a.attr(k(t), n)
            }), o.watchData = {}, angular.forEach(["minDate", "maxDate", "datepickerMode"], function(n) {
                var i, r;
                h[n] && (i = t(h[n]), o.$parent.$watch(i, function(t) {
                    o.watchData[n] = t
                }), a.attr(k(n), "watchData." + n), n === "datepickerMode" && (r = i.assign, o.$watch("watchData." + n, function(n, t) {
                    n !== t && r(o.$parent, n)
                })))
            }), h.dateDisabled && a.attr("date-disabled", "dateDisabled({ date: date, mode: mode })"), c.$parsers.unshift(d), o.dateSelection = function(n) {
                angular.isDefined(n) && (o.date = n), c.$setViewValue(o.date), c.$render(), g && (o.isOpen = !1, s[0].focus())
            }, s.bind("input change keyup", function() {
                o.$apply(function() {
                    o.date = c.$modelValue
                })
            }), c.$render = function() {
                var n = c.$viewValue ? u(c.$viewValue, p) : "";
                s.val(n), o.date = d(c.$modelValue)
            }, v = function(n) {
                o.isOpen && n.target !== s[0] && o.$apply(function() {
                    o.isOpen = !1
                })
            }, w = function(n) {
                o.keydown(n)
            }, s.bind("keydown", w), o.keydown = function(n) {
                n.which === 27 ? (n.preventDefault(), n.stopPropagation(), o.close()) : n.which !== 40 || o.isOpen || (o.isOpen = !0)
            }, o.$watch("isOpen", function(n) {
                n ? (o.$broadcast("datepicker.focus"), o.position = b ? r.offset(s) : r.position(s), o.position.top = o.position.top + s.prop("offsetHeight"), i.bind("click", v)) : i.unbind("click", v)
            }), o.select = function(n) {
                if (n === "today") {
                    var t = new Date;
                    angular.isDate(c.$modelValue) ? (n = new Date(c.$modelValue), n.setFullYear(t.getFullYear(), t.getMonth(), t.getDate())) : n = new Date(t.setHours(0, 0, 0, 0))
                }
                o.dateSelection(n)
            }, o.close = function() {
                o.isOpen = !1, s[0].focus()
            }, y = n(l)(o), l.remove(), b ? i.find("body").append(y) : s.after(y), o.$on("$destroy", function() {
                y.remove(), s.unbind("keydown", w), i.unbind("click", v)
            })
        }
    }
}]).directive("datepickerPopupWrap", function() {
    return {
        restrict: "EA",
        replace: !0,
        transclude: !0,
        templateUrl: "template/datepicker/popup.html",
        link: function(n, t) {
            t.bind("click", function(n) {
                n.preventDefault(), n.stopPropagation()
            })
        }
    }
}), angular.module("ui.bootstrap.dropdown", []).constant("dropdownConfig", {
    openClass: "open"
}).service("dropdownService", ["$document", function(n) {
    var t = null,
        i, r;
    this.open = function(u) {
        t || (n.bind("click", i), n.bind("keydown", r)), t && t !== u && (t.isOpen = !1), t = u
    }, this.close = function(u) {
        t === u && (t = null, n.unbind("click", i), n.unbind("keydown", r))
    }, i = function(n) {
        var i = t.getToggleElement();
        n && i && i[0].contains(n.target) || t.$apply(function() {
            t.isOpen = !1
        })
    }, r = function(n) {
        n.which === 27 && (t.focusToggleElement(), i())
    }
}]).controller("DropdownController", ["$scope", "$attrs", "$parse", "dropdownConfig", "dropdownService", "$animate", function(n, t, i, r, u, f) {
    var o = this,
        e = n.$new(),
        c = r.openClass,
        s, h = angular.noop,
        l = t.onToggle ? i(t.onToggle) : angular.noop;
    this.init = function(r) {
        o.$element = r, t.isOpen && (s = i(t.isOpen), h = s.assign, n.$watch(s, function(n) {
            e.isOpen = !!n
        }))
    }, this.toggle = function(n) {
        return e.isOpen = arguments.length ? !!n : !e.isOpen
    }, this.isOpen = function() {
        return e.isOpen
    }, e.getToggleElement = function() {
        return o.toggleElement
    }, e.focusToggleElement = function() {
        o.toggleElement && o.toggleElement[0].focus()
    }, e.$watch("isOpen", function(t, i) {
        f[t ? "addClass" : "removeClass"](o.$element, c), t ? (e.focusToggleElement(), u.open(e)) : u.close(e), h(n, t), angular.isDefined(t) && t !== i && l(n, {
            open: !!t
        })
    }), n.$on("$locationChangeSuccess", function() {
        e.isOpen = !1
    }), n.$on("$destroy", function() {
        e.$destroy()
    })
}]).directive("dropdown", function() {
    return {
        restrict: "CA",
        controller: "DropdownController",
        link: function(n, t, i, r) {
            r.init(t)
        }
    }
}).directive("dropdownToggle", function() {
    return {
        restrict: "CA",
        require: "?^dropdown",
        link: function(n, t, i, r) {
            if (r) {
                r.toggleElement = t;
                var u = function(u) {
                    u.preventDefault(), t.hasClass("disabled") || i.disabled || n.$apply(function() {
                        r.toggle()
                    })
                };
                t.bind("click", u), t.attr({
                    "aria-haspopup": !0,
                    "aria-expanded": !1
                }), n.$watch(r.isOpen, function(n) {
                    t.attr("aria-expanded", !!n)
                }), n.$on("$destroy", function() {
                    t.unbind("click", u)
                })
            }
        }
    }
}), angular.module("ui.bootstrap.modal", ["ui.bootstrap.transition"]).factory("$$stackedMap", function() {
    return {
        createNew: function() {
            var n = [];
            return {
                add: function(t, i) {
                    n.push({
                        key: t,
                        value: i
                    })
                },
                get: function(t) {
                    for (var i = 0; i < n.length; i++)
                        if (t == n[i].key) return n[i]
                },
                keys: function() {
                    for (var i = [], t = 0; t < n.length; t++) i.push(n[t].key);
                    return i
                },
                top: function() {
                    return n[n.length - 1]
                },
                remove: function(t) {
                    for (var r = -1, i = 0; i < n.length; i++)
                        if (t == n[i].key) {
                            r = i;
                            break
                        } return n.splice(r, 1)[0]
                },
                removeTop: function() {
                    return n.splice(n.length - 1, 1)[0]
                },
                length: function() {
                    return n.length
                }
            }
        }
    }
}).directive("modalBackdrop", ["$timeout", function(n) {
    return {
        restrict: "EA",
        replace: !0,
        templateUrl: "template/modal/backdrop.html",
        link: function(t, i, r) {
            t.backdropClass = r.backdropClass || "", t.animate = !1, n(function() {
                t.animate = !0
            })
        }
    }
}]).directive("modalWindow", ["$modalStack", "$timeout", function(n, t) {
    return {
        restrict: "EA",
        scope: {
            index: "@",
            animate: "="
        },
        replace: !0,
        transclude: !0,
        templateUrl: function(n, t) {
            return t.templateUrl || "template/modal/window.html"
        },
        link: function(i, r, u) {
            r.addClass(u.windowClass || ""), i.size = u.size, t(function() {
                i.animate = !0, r[0].querySelectorAll("[autofocus]").length || r[0].focus()
            }), i.close = function(t) {
                var i = n.getTop();
                i && i.value.backdrop && i.value.backdrop != "static" && t.target === t.currentTarget && (t.preventDefault(), t.stopPropagation(), n.dismiss(i.key, "backdrop click"))
            }
        }
    }
}]).directive("modalTransclude", function() {
    return {
        link: function(n, t, i, r, u) {
            u(n.$parent, function(n) {
                t.empty(), t.append(n)
            })
        }
    }
}).factory("$modalStack", ["$transition", "$timeout", "$document", "$compile", "$rootScope", "$$stackedMap", function(n, t, i, r, u, f) {
    function c() {
        for (var i = -1, t = e.keys(), n = 0; n < t.length; n++) e.get(t[n]).value.backdrop && (i = n);
        return i
    }

    function a(n) {
        var r = i.find("body").eq(0),
            t = e.get(n).value;
        e.remove(n), v(t.modalDomEl, t.modalScope, 300, function() {
            t.modalScope.$destroy(), r.toggleClass(l, e.length() > 0), y()
        })
    }

    function y() {
        if (h && c() == -1) {
            var n = o;
            v(h, o, 150, function() {
                n.$destroy(), n = null
            }), h = undefined, o = undefined
        }
    }

    function v(i, r, u, f) {
        function e() {
            e.done || (e.done = !0, i.remove(), f && f())
        }
        var o, s;
        r.animate = !1, o = n.transitionEndEventName, o ? (s = t(e, u), i.bind(o, function() {
            t.cancel(s), e(), r.$apply()
        })) : t(e)
    }
    var l = "modal-open",
        h, o, e = f.createNew(),
        s = {};
    return u.$watch(c, function(n) {
        o && (o.index = n)
    }), i.bind("keydown", function(n) {
        var t;
        n.which === 27 && (t = e.top(), t && t.value.keyboard && (n.preventDefault(), u.$apply(function() {
            s.dismiss(t.key, "escape key press")
        })))
    }), s.open = function(n, t) {
        var f, s, a, v, y;
        e.add(n, {
            deferred: t.deferred,
            modalScope: t.scope,
            backdrop: t.backdrop,
            keyboard: t.keyboard
        }), f = i.find("body").eq(0), s = c(), s >= 0 && !h && (o = u.$new(!0), o.index = s, a = angular.element("<div modal-backdrop></div>"), a.attr("backdrop-class", t.backdropClass), h = r(a)(o), f.append(h)), v = angular.element("<div modal-window></div>"), v.attr({
            "template-url": t.windowTemplateUrl,
            "window-class": t.windowClass,
            size: t.size,
            index: e.length() - 1,
            animate: "animate"
        }).html(t.content), y = r(v)(t.scope), e.top().value.modalDomEl = y, f.append(y), f.addClass(l)
    }, s.close = function(n, t) {
        var i = e.get(n);
        i && (i.value.deferred.resolve(t), a(n))
    }, s.dismiss = function(n, t) {
        var i = e.get(n);
        i && (i.value.deferred.reject(t), a(n))
    }, s.dismissAll = function(n) {
        for (var t = this.getTop(); t;) this.dismiss(t.key, n), t = this.getTop()
    }, s.getTop = function() {
        return e.top()
    }, s
}]).provider("$modal", function() {
    var n = {
        options: {
            backdrop: !0,
            keyboard: !0
        },
        $get: ["$injector", "$rootScope", "$q", "$http", "$templateCache", "$controller", "$modalStack", function(t, i, r, u, f, e, o) {
            function h(n) {
                return n.template ? r.when(n.template) : u.get(angular.isFunction(n.templateUrl) ? n.templateUrl() : n.templateUrl, {
                    cache: f
                }).then(function(n) {
                    return n.data
                })
            }

            function c(n) {
                var i = [];
                return angular.forEach(n, function(n) {
                    (angular.isFunction(n) || angular.isArray(n)) && i.push(r.when(t.invoke(n)))
                }), i
            }
            var s = {};
            return s.open = function(t) {
                var f = r.defer(),
                    s = r.defer(),
                    u = {
                        result: f.promise,
                        opened: s.promise,
                        close: function(n) {
                            o.close(u, n)
                        },
                        dismiss: function(n) {
                            o.dismiss(u, n)
                        }
                    },
                    l;
                if (t = angular.extend({}, n.options, t), t.resolve = t.resolve || {}, !t.template && !t.templateUrl) throw new Error("One of template or templateUrl options is required.");
                return l = r.all([h(t)].concat(c(t.resolve))), l.then(function(n) {
                    var r = (t.scope || i).$new(),
                        h, s, c;
                    r.$close = u.close, r.$dismiss = u.dismiss, s = {}, c = 1, t.controller && (s.$scope = r, s.$modalInstance = u, angular.forEach(t.resolve, function(t, i) {
                        s[i] = n[c++]
                    }), h = e(t.controller, s), t.controllerAs && (r[t.controllerAs] = h)), o.open(u, {
                        scope: r,
                        deferred: f,
                        content: n[0],
                        backdrop: t.backdrop,
                        keyboard: t.keyboard,
                        backdropClass: t.backdropClass,
                        windowClass: t.windowClass,
                        windowTemplateUrl: t.windowTemplateUrl,
                        size: t.size
                    })
                }, function(n) {
                    f.reject(n)
                }), l.then(function() {
                    s.resolve(!0)
                }, function() {
                    s.reject(!1)
                }), u
            }, s
        }]
    };
    return n
}), angular.module("ui.bootstrap.pagination", []).controller("PaginationController", ["$scope", "$attrs", "$parse", function(n, t, i) {
    var u = this,
        r = {
            $setViewValue: angular.noop
        },
        f = t.numPages ? i(t.numPages).assign : angular.noop;
    this.init = function(f, e) {
        r = f, this.config = e, r.$render = function() {
            u.render()
        }, t.itemsPerPage ? n.$parent.$watch(i(t.itemsPerPage), function(t) {
            u.itemsPerPage = parseInt(t, 10), n.totalPages = u.calculateTotalPages()
        }) : this.itemsPerPage = e.itemsPerPage
    }, this.calculateTotalPages = function() {
        var t = this.itemsPerPage < 1 ? 1 : Math.ceil(n.totalItems / this.itemsPerPage);
        return Math.max(t || 0, 1)
    }, this.render = function() {
        n.page = parseInt(r.$viewValue, 10) || 1
    }, n.selectPage = function(t) {
        n.page !== t && t > 0 && t <= n.totalPages && (r.$setViewValue(t), r.$render())
    }, n.getText = function(t) {
        return n[t + "Text"] || u.config[t + "Text"]
    }, n.noPrevious = function() {
        return n.page === 1
    }, n.noNext = function() {
        return n.page === n.totalPages
    }, n.$watch("totalItems", function() {
        n.totalPages = u.calculateTotalPages()
    }), n.$watch("totalPages", function(t) {
        f(n.$parent, t), n.page > t ? n.selectPage(t) : r.$render()
    })
}]).constant("paginationConfig", {
    itemsPerPage: 10,
    boundaryLinks: !1,
    directionLinks: !0,
    firstText: "First",
    previousText: "Previous",
    nextText: "Next",
    lastText: "Last",
    rotate: !0
}).directive("pagination", ["$parse", "paginationConfig", function(n, t) {
    return {
        restrict: "EA",
        scope: {
            totalItems: "=",
            firstText: "@",
            previousText: "@",
            nextText: "@",
            lastText: "@"
        },
        require: ["pagination", "?ngModel"],
        controller: "PaginationController",
        templateUrl: "template/pagination/pagination.html",
        replace: !0,
        link: function(i, r, u, f) {
            function h(n, t, i) {
                return {
                    number: n,
                    text: t,
                    active: i
                }
            }

            function a(n, t) {
                var f = [],
                    i = 1,
                    r = t,
                    o = angular.isDefined(e) && e < t,
                    u, c, l, a;
                for (o && (s ? (i = Math.max(n - Math.floor(e / 2), 1), r = i + e - 1, r > t && (r = t, i = r - e + 1)) : (i = (Math.ceil(n / e) - 1) * e + 1, r = Math.min(i + e - 1, t))), u = i; u <= r; u++) c = h(u, u, u === n), f.push(c);
                return o && !s && (i > 1 && (l = h(i - 1, "...", !1), f.unshift(l)), r < t && (a = h(r + 1, "...", !1), f.push(a))), f
            }
            var o = f[0],
                c = f[1],
                e, s, l;
            c && (e = angular.isDefined(u.maxSize) ? i.$parent.$eval(u.maxSize) : t.maxSize, s = angular.isDefined(u.rotate) ? i.$parent.$eval(u.rotate) : t.rotate, i.boundaryLinks = angular.isDefined(u.boundaryLinks) ? i.$parent.$eval(u.boundaryLinks) : t.boundaryLinks, i.directionLinks = angular.isDefined(u.directionLinks) ? i.$parent.$eval(u.directionLinks) : t.directionLinks, o.init(c, t), u.maxSize && i.$parent.$watch(n(u.maxSize), function(n) {
                e = parseInt(n, 10), o.render()
            }), l = o.render, o.render = function() {
                l(), i.page > 0 && i.page <= i.totalPages && (i.pages = a(i.page, i.totalPages))
            })
        }
    }
}]).constant("pagerConfig", {
    itemsPerPage: 10,
    previousText: "« Previous",
    nextText: "Next »",
    align: !0
}).directive("pager", ["pagerConfig", function(n) {
    return {
        restrict: "EA",
        scope: {
            totalItems: "=",
            previousText: "@",
            nextText: "@"
        },
        require: ["pager", "?ngModel"],
        controller: "PaginationController",
        templateUrl: "template/pagination/pager.html",
        replace: !0,
        link: function(t, i, r, u) {
            var e = u[0],
                f = u[1];
            f && (t.align = angular.isDefined(r.align) ? t.$parent.$eval(r.align) : n.align, e.init(f, n))
        }
    }
}]), angular.module("ui.bootstrap.tooltip", ["ui.bootstrap.position", "ui.bootstrap.bindHtml"]).provider("$tooltip", function() {
    function r(n) {
        var t = /[A-Z]/g,
            i = "-";
        return n.replace(t, function(n, t) {
            return (t ? i : "") + n.toLowerCase()
        })
    }
    var i = {
            placement: "top",
            animation: !0,
            popupDelay: 0
        },
        n = {
            mouseenter: "mouseleave",
            click: "click",
            focus: "blur"
        },
        t = {};
    this.options = function(n) {
        angular.extend(t, n)
    }, this.setTriggers = function(t) {
        angular.extend(n, t)
    }, this.$get = ["$window", "$compile", "$timeout", "$parse", "$document", "$position", "$interpolate", function(u, f, e, o, s, h, c) {
        return function(u, l, a) {
            function w(t) {
                var i = t || v.trigger || a,
                    r = n[i] || i;
                return {
                    show: i,
                    hide: r
                }
            }
            var v = angular.extend({}, i, t),
                b = r(u),
                y = c.startSymbol(),
                p = c.endSymbol(),
                k = "<div " + b + '-popup title="' + y + "tt_title" + p + '" content="' + y + "tt_content" + p + '" placement="' + y + "tt_placement" + p + '" animation="tt_animation" is-open="tt_isOpen"></div>';
            return {
                restrict: "EA",
                scope: !0,
                compile: function() {
                    var i = f(k);
                    return function(n, t, r) {
                        function ut() {
                            n.tt_isOpen ? nt() : g()
                        }

                        function g() {
                            (!ft || n.$eval(r[l + "Enable"])) && (n.tt_popupDelay ? y || (y = e(rt, n.tt_popupDelay, !1), y.then(function(n) {
                                n()
                            })) : rt()())
                        }

                        function nt() {
                            n.$apply(function() {
                                tt()
                            })
                        }

                        function rt() {
                            return (y = null, a && (e.cancel(a), a = null), !n.tt_content) ? angular.noop : (et(), f.css({
                                top: 0,
                                left: 0,
                                display: "block"
                            }), p ? s.find("body").append(f) : t.after(f), it(), n.tt_isOpen = !0, n.$digest(), it)
                        }

                        function tt() {
                            n.tt_isOpen = !1, e.cancel(y), y = null, n.tt_animation ? a || (a = e(b, 500)) : b()
                        }

                        function et() {
                            f && b(), f = i(n, function() {}), n.$digest()
                        }

                        function b() {
                            a = null, f && (f.remove(), f = null)
                        }
                        var f, a, y, p = angular.isDefined(v.appendToBody) ? v.appendToBody : !1,
                            c = w(undefined),
                            ft = angular.isDefined(r[l + "Enable"]),
                            it = function() {
                                var i = h.positionElements(t, f, n.tt_placement, p);
                                i.top += "px", i.left += "px", f.css(i)
                            },
                            d, k;
                        n.tt_isOpen = !1, r.$observe(u, function(t) {
                            n.tt_content = t, !t && n.tt_isOpen && tt()
                        }), r.$observe(l + "Title", function(t) {
                            n.tt_title = t
                        }), r.$observe(l + "Placement", function(t) {
                            n.tt_placement = angular.isDefined(t) ? t : v.placement
                        }), r.$observe(l + "PopupDelay", function(t) {
                            var i = parseInt(t, 10);
                            n.tt_popupDelay = isNaN(i) ? v.popupDelay : i
                        }), d = function() {
                            t.unbind(c.show, g), t.unbind(c.hide, nt)
                        }, r.$observe(l + "Trigger", function(n) {
                            d(), c = w(n), c.show === c.hide ? t.bind(c.show, ut) : (t.bind(c.show, g), t.bind(c.hide, nt))
                        }), k = n.$eval(r[l + "Animation"]), n.tt_animation = angular.isDefined(k) ? !!k : v.animation, r.$observe(l + "AppendToBody", function(t) {
                            p = angular.isDefined(t) ? o(t)(n) : p
                        }), p && n.$on("$locationChangeSuccess", function() {
                            n.tt_isOpen && tt()
                        }), n.$on("$destroy", function() {
                            e.cancel(a), e.cancel(y), d(), b()
                        })
                    }
                }
            }
        }
    }]
}).directive("tooltipPopup", function() {
    return {
        restrict: "EA",
        replace: !0,
        scope: {
            content: "@",
            placement: "@",
            animation: "&",
            isOpen: "&"
        },
        templateUrl: "template/tooltip/tooltip-popup.html"
    }
}).directive("tooltip", ["$tooltip", function(n) {
    return n("tooltip", "tooltip", "mouseenter")
}]).directive("tooltipHtmlUnsafePopup", function() {
    return {
        restrict: "EA",
        replace: !0,
        scope: {
            content: "@",
            placement: "@",
            animation: "&",
            isOpen: "&"
        },
        templateUrl: "template/tooltip/tooltip-html-unsafe-popup.html"
    }
}).directive("tooltipHtmlUnsafe", ["$tooltip", function(n) {
    return n("tooltipHtmlUnsafe", "tooltip", "mouseenter")
}]), angular.module("ui.bootstrap.popover", ["ui.bootstrap.tooltip"]).directive("popoverPopup", function() {
    return {
        restrict: "EA",
        replace: !0,
        scope: {
            title: "@",
            content: "@",
            placement: "@",
            animation: "&",
            isOpen: "&"
        },
        templateUrl: "template/popover/popover.html"
    }
}).directive("popover", ["$tooltip", function(n) {
    return n("popover", "popover", "click")
}]), angular.module("ui.bootstrap.progressbar", []).constant("progressConfig", {
    animate: !0,
    max: 100
}).controller("ProgressController", ["$scope", "$attrs", "progressConfig", function(n, t, i) {
    var r = this,
        u = angular.isDefined(t.animate) ? n.$parent.$eval(t.animate) : i.animate;
    this.bars = [], n.max = angular.isDefined(t.max) ? n.$parent.$eval(t.max) : i.max, this.addBar = function(t, i) {
        u || i.css({
            transition: "none"
        }), this.bars.push(t), t.$watch("value", function(i) {
            t.percent = +(100 * i / n.max).toFixed(2)
        }), t.$on("$destroy", function() {
            i = null, r.removeBar(t)
        })
    }, this.removeBar = function(n) {
        this.bars.splice(this.bars.indexOf(n), 1)
    }
}]).directive("progress", function() {
    return {
        restrict: "EA",
        replace: !0,
        transclude: !0,
        controller: "ProgressController",
        require: "progress",
        scope: {},
        templateUrl: "template/progressbar/progress.html"
    }
}).directive("bar", function() {
    return {
        restrict: "EA",
        replace: !0,
        transclude: !0,
        require: "^progress",
        scope: {
            value: "=",
            type: "@"
        },
        templateUrl: "template/progressbar/bar.html",
        link: function(n, t, i, r) {
            r.addBar(n, t)
        }
    }
}).directive("progressbar", function() {
    return {
        restrict: "EA",
        replace: !0,
        transclude: !0,
        controller: "ProgressController",
        scope: {
            value: "=",
            type: "@"
        },
        templateUrl: "template/progressbar/progressbar.html",
        link: function(n, t, i, r) {
            r.addBar(n, angular.element(t.children()[0]))
        }
    }
}), angular.module("ui.bootstrap.rating", []).constant("ratingConfig", {
    max: 5,
    stateOn: null,
    stateOff: null
}).controller("RatingController", ["$scope", "$attrs", "ratingConfig", function(n, t, i) {
    var r = {
        $setViewValue: angular.noop
    };
    this.init = function(u) {
        r = u, r.$render = this.render, this.stateOn = angular.isDefined(t.stateOn) ? n.$parent.$eval(t.stateOn) : i.stateOn, this.stateOff = angular.isDefined(t.stateOff) ? n.$parent.$eval(t.stateOff) : i.stateOff;
        var f = angular.isDefined(t.ratingStates) ? n.$parent.$eval(t.ratingStates) : new Array(angular.isDefined(t.max) ? n.$parent.$eval(t.max) : i.max);
        n.range = this.buildTemplateObjects(f)
    }, this.buildTemplateObjects = function(n) {
        for (var t = 0, i = n.length; t < i; t++) n[t] = angular.extend({
            index: t
        }, {
            stateOn: this.stateOn,
            stateOff: this.stateOff
        }, n[t]);
        return n
    }, n.rate = function(t) {
        !n.readonly && t >= 0 && t <= n.range.length && (r.$setViewValue(t), r.$render())
    }, n.enter = function(t) {
        n.readonly || (n.value = t);
        n.onHover({
            value: t
        })
    }, n.reset = function() {
        n.value = r.$viewValue, n.onLeave()
    }, n.onKeydown = function(t) {
        /(37|38|39|40)/.test(t.which) && (t.preventDefault(), t.stopPropagation(), n.rate(n.value + (t.which === 38 || t.which === 39 ? 1 : -1)))
    }, this.render = function() {
        n.value = r.$viewValue
    }
}]).directive("rating", function() {
    return {
        restrict: "EA",
        require: ["rating", "ngModel"],
        scope: {
            readonly: "=?",
            onHover: "&",
            onLeave: "&"
        },
        controller: "RatingController",
        templateUrl: "template/rating/rating.html",
        replace: !0,
        link: function(n, t, i, r) {
            var f = r[0],
                u = r[1];
            u && f.init(u)
        }
    }
}), angular.module("ui.bootstrap.tabs", []).controller("TabsetController", ["$scope", function(n) {
    var i = this,
        t = i.tabs = n.tabs = [];
    i.select = function(n) {
        angular.forEach(t, function(t) {
            t.active && t !== n && (t.active = !1, t.onDeselect())
        }), n.active = !0, n.onSelect()
    }, i.addTab = function(n) {
        t.push(n), t.length === 1 ? n.active = !0 : n.active && i.select(n)
    }, i.removeTab = function(n) {
        var r = t.indexOf(n),
            u;
        n.active && t.length > 1 && (u = r == t.length - 1 ? r - 1 : r + 1, i.select(t[u])), t.splice(r, 1)
    }
}]).directive("tabset", function() {
    return {
        restrict: "EA",
        transclude: !0,
        replace: !0,
        scope: {
            type: "@"
        },
        controller: "TabsetController",
        templateUrl: "template/tabs/tabset.html",
        link: function(n, t, i) {
            n.vertical = angular.isDefined(i.vertical) ? n.$parent.$eval(i.vertical) : !1, n.justified = angular.isDefined(i.justified) ? n.$parent.$eval(i.justified) : !1
        }
    }
}).directive("tab", ["$parse", function(n) {
    return {
        require: "^tabset",
        restrict: "EA",
        replace: !0,
        templateUrl: "template/tabs/tab.html",
        transclude: !0,
        scope: {
            active: "=?",
            heading: "@",
            onSelect: "&select",
            onDeselect: "&deselect"
        },
        controller: function() {},
        compile: function(t, i, r) {
            return function(t, i, u, f) {
                t.$watch("active", function(n) {
                    n && f.select(t)
                }), t.disabled = !1, u.disabled && t.$parent.$watch(n(u.disabled), function(n) {
                    t.disabled = !!n
                }), t.select = function() {
                    t.disabled || (t.active = !0)
                }, f.addTab(t), t.$on("$destroy", function() {
                    f.removeTab(t)
                }), t.$transcludeFn = r
            }
        }
    }
}]).directive("tabHeadingTransclude", [function() {
    return {
        restrict: "A",
        require: "^tab",
        link: function(n, t) {
            n.$watch("headingElement", function(n) {
                n && (t.html(""), t.append(n))
            })
        }
    }
}]).directive("tabContentTransclude", function() {
    function n(n) {
        return n.tagName && (n.hasAttribute("tab-heading") || n.hasAttribute("data-tab-heading") || n.tagName.toLowerCase() === "tab-heading" || n.tagName.toLowerCase() === "data-tab-heading")
    }
    return {
        restrict: "A",
        require: "^tabset",
        link: function(t, i, r) {
            var u = t.$eval(r.tabContentTransclude);
            u.$transcludeFn(u.$parent, function(t) {
                angular.forEach(t, function(t) {
                    n(t) ? u.headingElement = t : i.append(t)
                })
            })
        }
    }
}), angular.module("ui.bootstrap.timepicker", []).constant("timepickerConfig", {
    hourStep: 1,
    minuteStep: 1,
    showMeridian: !0,
    meridians: null,
    readonlyInput: !1,
    mousewheel: !0
}).controller("TimepickerController", ["$scope", "$attrs", "$parse", "$log", "$locale", "timepickerConfig", function(n, t, i, r, u, f) {
    function p() {
        var t = parseInt(n.hours, 10),
            i = n.showMeridian ? t > 0 && t < 13 : t >= 0 && t < 24;
        return i ? (n.showMeridian && (t === 12 && (t = 0), n.meridian === y[1] && (t = t + 12)), t) : undefined
    }

    function w() {
        var t = parseInt(n.minutes, 10);
        return t >= 0 && t < 60 ? t : undefined
    }

    function l(n) {
        return angular.isDefined(n) && n.toString().length < 2 ? "0" + n : n
    }

    function a(n) {
        b(), o.$setViewValue(new Date(e)), v(n)
    }

    function b() {
        o.$setValidity("time", !0), n.invalidHours = !1, n.invalidMinutes = !1
    }

    function v(t) {
        var i = e.getHours(),
            r = e.getMinutes();
        n.showMeridian && (i = i === 0 || i === 12 ? 12 : i % 12), n.hours = t === "h" ? i : l(i), n.minutes = t === "m" ? r : l(r), n.meridian = e.getHours() < 12 ? y[0] : y[1]
    }

    function s(n) {
        var t = new Date(e.getTime() + n * 6e4);
        e.setHours(t.getHours(), t.getMinutes()), a()
    }
    var e = new Date,
        o = {
            $setViewValue: angular.noop
        },
        y = angular.isDefined(t.meridians) ? n.$parent.$eval(t.meridians) : f.meridians || u.DATETIME_FORMATS.AMPMS,
        h, c;
    this.init = function(i, r) {
        o = i, o.$render = this.render;
        var u = r.eq(0),
            e = r.eq(1),
            s = angular.isDefined(t.mousewheel) ? n.$parent.$eval(t.mousewheel) : f.mousewheel;
        s && this.setupMousewheelEvents(u, e), n.readonlyInput = angular.isDefined(t.readonlyInput) ? n.$parent.$eval(t.readonlyInput) : f.readonlyInput, this.setupInputEvents(u, e)
    }, h = f.hourStep, t.hourStep && n.$parent.$watch(i(t.hourStep), function(n) {
        h = parseInt(n, 10)
    }), c = f.minuteStep, t.minuteStep && n.$parent.$watch(i(t.minuteStep), function(n) {
        c = parseInt(n, 10)
    }), n.showMeridian = f.showMeridian, t.showMeridian && n.$parent.$watch(i(t.showMeridian), function(t) {
        if (n.showMeridian = !!t, o.$error.time) {
            var i = p(),
                r = w();
            angular.isDefined(i) && angular.isDefined(r) && (e.setHours(i), a())
        } else v()
    }), this.setupMousewheelEvents = function(t, i) {
        var r = function(n) {
            n.originalEvent && (n = n.originalEvent);
            var t = n.wheelDelta ? n.wheelDelta : -n.deltaY;
            return n.detail || t > 0
        };
        t.bind("mousewheel wheel", function(t) {
            n.$apply(r(t) ? n.incrementHours() : n.decrementHours()), t.preventDefault()
        }), i.bind("mousewheel wheel", function(t) {
            n.$apply(r(t) ? n.incrementMinutes() : n.decrementMinutes()), t.preventDefault()
        })
    }, this.setupInputEvents = function(t, i) {
        if (n.readonlyInput) {
            n.updateHours = angular.noop, n.updateMinutes = angular.noop;
            return
        }
        var r = function(t, i) {
            o.$setViewValue(null), o.$setValidity("time", !1), angular.isDefined(t) && (n.invalidHours = t), angular.isDefined(i) && (n.invalidMinutes = i)
        };
        n.updateHours = function() {
            var n = p();
            angular.isDefined(n) ? (e.setHours(n), a("h")) : r(!0)
        }, t.bind("blur", function() {
            !n.invalidHours && n.hours < 10 && n.$apply(function() {
                n.hours = l(n.hours)
            })
        }), n.updateMinutes = function() {
            var n = w();
            angular.isDefined(n) ? (e.setMinutes(n), a("m")) : r(undefined, !0)
        }, i.bind("blur", function() {
            !n.invalidMinutes && n.minutes < 10 && n.$apply(function() {
                n.minutes = l(n.minutes)
            })
        })
    }, this.render = function() {
        var n = o.$modelValue ? new Date(o.$modelValue) : null;
        isNaN(n) ? (o.$setValidity("time", !1), r.error('Timepicker directive: "ng-model" value must be a Date object, a number of milliseconds since 01.01.1970 or a string representing an RFC2822 or ISO 8601 date.')) : (n && (e = n), b(), v())
    }, n.incrementHours = function() {
        s(h * 60)
    }, n.decrementHours = function() {
        s(-h * 60)
    }, n.incrementMinutes = function() {
        s(c)
    }, n.decrementMinutes = function() {
        s(-c)
    }, n.toggleMeridian = function() {
        s(720 * (e.getHours() < 12 ? 1 : -1))
    }
}]).directive("timepicker", function() {
    return {
        restrict: "EA",
        require: ["timepicker", "?^ngModel"],
        controller: "TimepickerController",
        replace: !0,
        scope: {},
        templateUrl: "template/timepicker/timepicker.html",
        link: function(n, t, i, r) {
            var f = r[0],
                u = r[1];
            u && f.init(u, t.find("input"))
        }
    }
}), angular.module("ui.bootstrap.typeahead", ["ui.bootstrap.position", "ui.bootstrap.bindHtml"]).factory("typeaheadParser", ["$parse", function(n) {
    var t = /^\s*([\s\S]+?)(?:\s+as\s+([\s\S]+?))?\s+for\s+(?:([\$\w][\$\w\d]*))\s+in\s+([\s\S]+?)$/;
    return {
        parse: function(i) {
            var r = i.match(t);
            if (!r) throw new Error('Expected typeahead specification in form of "_modelValue_ (as _label_)? for _item_ in _collection_" but got "' + i + '".');
            return {
                itemName: r[3],
                source: n(r[4]),
                viewMapper: n(r[2] || r[1]),
                modelMapper: n(r[1])
            }
        }
    }
}]).directive("typeahead", ["$compile", "$parse", "$q", "$timeout", "$document", "$position", "typeaheadParser", function(n, t, i, r, u, f, e) {
    var o = [9, 13, 27, 38, 40];
    return {
        require: "ngModel",
        link: function(s, h, c, l) {
            var ct = s.$eval(c.typeaheadMinLength) || 1,
                ut = s.$eval(c.typeaheadWaitMs) || 0,
                lt = s.$eval(c.typeaheadEditable) !== !1,
                w = t(c.typeaheadLoading).assign || angular.noop,
                ht = t(c.typeaheadOnSelect),
                et = c.typeaheadInputFormatter ? t(c.typeaheadInputFormatter) : undefined,
                ft = c.typeaheadAppendToBody ? s.$eval(c.typeaheadAppendToBody) : !1,
                st = t(c.ngModel).assign,
                v = e.parse(c.typeahead),
                it, a = s.$new(),
                p, b, y, nt, g, d, ot, k, rt, tt;
            s.$on("$destroy", function() {
                a.$destroy()
            }), p = "typeahead-" + a.$id + "-" + Math.floor(Math.random() * 1e4), h.attr({
                "aria-autocomplete": "list",
                "aria-expanded": !1,
                "aria-owns": p
            }), b = angular.element("<div typeahead-popup></div>"), b.attr({
                id: p,
                matches: "matches",
                active: "activeIdx",
                select: "select(activeIdx)",
                query: "query",
                position: "position"
            }), angular.isDefined(c.typeaheadTemplateUrl) && b.attr("template-url", c.typeaheadTemplateUrl), y = function() {
                a.matches = [], a.activeIdx = -1, h.attr("aria-expanded", !1)
            }, nt = function(n) {
                return p + "-option-" + n
            }, a.$watch("activeIdx", function(n) {
                n < 0 ? h.removeAttr("aria-activedescendant") : h.attr("aria-activedescendant", nt(n))
            }), g = function(n) {
                var t = {
                    $viewValue: n
                };
                w(s, !0), i.when(v.source(s, t)).then(function(i) {
                    var u = n === l.$viewValue,
                        r;
                    if (u && it)
                        if (i.length > 0) {
                            for (a.activeIdx = 0, a.matches.length = 0, r = 0; r < i.length; r++) t[v.itemName] = i[r], a.matches.push({
                                id: nt(r),
                                label: v.viewMapper(a, t),
                                model: i[r]
                            });
                            a.query = n, a.position = ft ? f.offset(h) : f.position(h), a.position.top = a.position.top + h.prop("offsetHeight"), h.attr("aria-expanded", !0)
                        } else y();
                    u && w(s, !1)
                }, function() {
                    y(), w(s, !1)
                })
            }, y(), a.query = undefined, ot = function(n) {
                d = r(function() {
                    g(n)
                }, ut)
            }, k = function() {
                d && r.cancel(d)
            }, l.$parsers.unshift(function(n) {
                return it = !0, n && n.length >= ct ? ut > 0 ? (k(), ot(n)) : g(n) : (w(s, !1), k(), y()), lt ? n : n ? (l.$setValidity("editable", !1), undefined) : (l.$setValidity("editable", !0), n)
            }), l.$formatters.push(function(n) {
                var i, r, t = {};
                return et ? (t.$model = n, et(s, t)) : (t[v.itemName] = n, i = v.viewMapper(s, t), t[v.itemName] = undefined, r = v.viewMapper(s, t), i !== r ? i : n)
            }), a.select = function(n) {
                var t = {},
                    i, u;
                t[v.itemName] = u = a.matches[n].model, i = v.modelMapper(s, t), st(s, i), l.$setValidity("editable", !0), ht(s, {
                    $item: u,
                    $model: i,
                    $label: v.viewMapper(s, t)
                }), y(), r(function() {
                    h[0].focus()
                }, 0, !1)
            }, h.bind("keydown", function(n) {
                a.matches.length !== 0 && o.indexOf(n.which) !== -1 && (n.preventDefault(), n.which === 40 ? (a.activeIdx = (a.activeIdx + 1) % a.matches.length, a.$digest()) : n.which === 38 ? (a.activeIdx = (a.activeIdx ? a.activeIdx : a.matches.length) - 1, a.$digest()) : n.which === 13 || n.which === 9 ? a.$apply(function() {
                    a.select(a.activeIdx)
                }) : n.which === 27 && (n.stopPropagation(), y(), a.$digest()))
            }), h.bind("blur", function() {
                it = !1
            }), rt = function(n) {
                h[0] !== n.target && (y(), a.$digest())
            }, u.bind("click", rt), s.$on("$destroy", function() {
                u.unbind("click", rt)
            }), tt = n(b)(a), ft ? u.find("body").append(tt) : h.after(tt)
        }
    }
}]).directive("typeaheadPopup", function() {
    return {
        restrict: "EA",
        scope: {
            matches: "=",
            query: "=",
            active: "=",
            position: "=",
            select: "&"
        },
        replace: !0,
        templateUrl: "template/typeahead/typeahead-popup.html",
        link: function(n, t, i) {
            n.templateUrl = i.templateUrl, n.isOpen = function() {
                return n.matches.length > 0
            }, n.isActive = function(t) {
                return n.active == t
            }, n.selectActive = function(t) {
                n.active = t
            }, n.selectMatch = function(t) {
                n.select({
                    activeIdx: t
                })
            }
        }
    }
}).directive("typeaheadMatch", ["$http", "$templateCache", "$compile", "$parse", function(n, t, i, r) {
    return {
        restrict: "EA",
        scope: {
            index: "=",
            match: "=",
            query: "="
        },
        link: function(u, f, e) {
            var o = r(e.templateUrl)(u.$parent) || "template/typeahead/typeahead-match.html";
            n.get(o, {
                cache: t
            }).success(function(n) {
                f.replaceWith(i(n.trim())(u))
            })
        }
    }
}]).filter("typeaheadHighlight", function() {
    function n(n) {
        return n.replace(/([.?*+^$[\]\\(){}|-])/g, "\\$1")
    }
    return function(t, i) {
        return i ? ("" + t).replace(new RegExp(n(i), "gi"), "<strong>$&</strong>") : t
    }
});

;// JS/angular/ui-bootstrap-custom-tpls-2.5.0.min.js
// angular/ui-bootstrap-custom-tpls-2.5.0.min.js
/*
 * angular-ui-bootstrap
 * http://angular-ui.github.io/bootstrap/

 * Version: 2.5.0 - 2017-01-28
 * License: MIT
 */
angular.module("ui.bootstrap.v2", ["ui.bootstrap.tpls", "ui.bootstrap.modal.v2", "ui.bootstrap.multiMap.v2", "ui.bootstrap.stackedMap.v2", "ui.bootstrap.position.v2", "ui.bootstrap.tooltip.v2"]), angular.module("ui.bootstrap.tpls", ["uib/template/modal/window.html", "uib/template/tooltip/tooltip-html-popup.html", "uib/template/tooltip/tooltip-popup.html", "uib/template/tooltip/tooltip-template-popup.html"]), angular.module("ui.bootstrap.modal.v2", ["ui.bootstrap.multiMap.v2", "ui.bootstrap.stackedMap.v2", "ui.bootstrap.position.v2"]).provider("$uibResolve", function() {
    var t = this;
    this.resolver = null, this.setResolver = function(t) {
        this.resolver = t
    }, this.$get = ["$injector", "$q", function(e, o) {
        var i = t.resolver ? e.get(t.resolver) : null;
        return {
            resolve: function(t, n, r, a) {
                if (i) return i.resolve(t, n, r, a);
                var l = [];
                return angular.forEach(t, function(t) {
                    l.push(angular.isFunction(t) || angular.isArray(t) ? o.resolve(e.invoke(t)) : angular.isString(t) ? o.resolve(e.get(t)) : o.resolve(t))
                }), o.all(l).then(function(e) {
                    var o = {},
                        i = 0;
                    return angular.forEach(t, function(t, n) {
                        o[n] = e[i++]
                    }), o
                })
            }
        }
    }]
}).directive("uibModalBackdrop", ["$animate", "$injector", "$uibModalStack", function(t, e, o) {
    function i(e, i, n) {
        n.modalInClass && (t.addClass(i, n.modalInClass), e.$on(o.NOW_CLOSING_EVENT, function(o, r) {
            var a = r();
            e.modalOptions.animation ? t.removeClass(i, n.modalInClass).then(a) : a()
        }))
    }
    return {
        restrict: "A",
        compile: function(t, e) {
            return t.addClass(e.backdropClass), i
        }
    }
}]).directive("uibModalWindow", ["$uibModalStack", "$q", "$animateCss", "$document", function(t, e, o, i) {
    return {
        scope: {
            index: "@"
        },
        restrict: "A",
        transclude: !0,
        templateUrl: function(t, e) {
            return e.templateUrl || "uib/template/modal/window.html"
        },
        link: function(n, r, a) {
            r.addClass(a.windowTopClass || ""), n.size = a.size, n.close = function(e) {
                var o = t.getTop();
                o && o.value.backdrop && "static" !== o.value.backdrop && e.target === e.currentTarget && (e.preventDefault(), e.stopPropagation(), t.dismiss(o.key, "backdrop click"))
            }, r.on("click", n.close), n.$isRendered = !0;
            var l = e.defer();
            n.$$postDigest(function() {
                l.resolve()
            }), l.promise.then(function() {
                var l = null;
                a.modalInClass && (l = o(r, {
                    addClass: a.modalInClass
                }).start(), n.$on(t.NOW_CLOSING_EVENT, function(t, e) {
                    var i = e();
                    o(r, {
                        removeClass: a.modalInClass
                    }).start().then(i)
                })), e.when(l).then(function() {
                    var e = t.getTop();
                    if (e && t.modalRendered(e.key), !i[0].activeElement || !r[0].contains(i[0].activeElement)) {
                        var o = r[0].querySelector("[autofocus]");
                        o ? o.focus() : r[0].focus()
                    }
                })
            })
        }
    }
}]).directive("uibModalAnimationClass", function() {
    return {
        compile: function(t, e) {
            e.modalAnimation && t.addClass(e.uibModalAnimationClass)
        }
    }
}).directive("uibModalTransclude", ["$animate", function(t) {
    return {
        link: function(e, o, i, n, r) {
            r(e.$parent, function(e) {
                o.empty(), t.enter(e, o)
            })
        }
    }
}]).factory("$uibModalStack", ["$animate", "$animateCss", "$document", "$compile", "$rootScope", "$q", "$$multiMap", "$$stackedMap", "$uibPosition", function(t, e, o, i, n, r, a, l, p) {
    function s(t) {
        var e = "-";
        return t.replace(O, function(t, o) {
            return (o ? e : "") + t.toLowerCase()
        })
    }

    function u(t) {
        return !!(t.offsetWidth || t.offsetHeight || t.getClientRects().length)
    }

    function c() {
        for (var t = -1, e = k.keys(), o = 0; o < e.length; o++) k.get(e[o]).value.backdrop && (t = o);
        return t > -1 && S > t && (t = S), t
    }

    function d(t, e) {
        var o = k.get(t).value,
            i = o.appendTo;
        k.remove(t), x = k.top(), x && (S = parseInt(x.value.modalDomEl.attr("index"), 10)), h(o.modalDomEl, o.modalScope, function() {
            var e = o.openedClass || C;
            T.remove(e, t);
            var n = T.hasKey(e);
            i.toggleClass(e, n), !n && y && y.heightOverflow && y.scrollbarWidth && (i.css(y.originalRight ? {
                paddingRight: y.originalRight + "px"
            } : {
                paddingRight: ""
            }), y = null), f(!0)
        }, o.closedDeferred), m(), e && e.focus ? e.focus() : i.focus && i.focus()
    }

    function f(t) {
        var e;
        k.length() > 0 && (e = k.top().value, e.modalDomEl.toggleClass(e.windowTopClass || "", t))
    }

    function m() {
        if (w && -1 === c()) {
            var t = $;
            h(w, $, function() {
                t = null
            }), w = void 0, $ = void 0
        }
    }

    function h(e, o, i, n) {
        function a() {
            a.done || (a.done = !0, t.leave(e).then(function() {
                i && i(), e.remove(), n && n.resolve()
            }), o.$destroy())
        }
        var l, p = null,
            s = function() {
                return l || (l = r.defer(), p = l.promise),
                    function() {
                        l.resolve()
                    }
            };
        return o.$broadcast(E.NOW_CLOSING_EVENT, s), r.when(p).then(a)
    }

    function b(t) {
        if (t.isDefaultPrevented()) return t;
        var e = k.top();
        if (e) switch (t.which) {
            case 27:
                e.value.keyboard && (t.preventDefault(), n.$apply(function() {
                    E.dismiss(e.key, "escape key press")
                }));
                break;
            case 9:
                var o = E.loadFocusElementList(e),
                    i = !1;
                t.shiftKey ? (E.isFocusInFirstItem(t, o) || E.isModalFocused(t, e)) && (i = E.focusLastFocusableElement(o)) : E.isFocusInLastItem(t, o) && (i = E.focusFirstFocusableElement(o)), i && (t.preventDefault(), t.stopPropagation())
        }
    }

    function v(t, e, o) {
        return !t.value.modalScope.$broadcast("modal.closing", e, o).defaultPrevented
    }

    function g() {
        Array.prototype.forEach.call(document.querySelectorAll("[" + D + "]"), function(t) {
            var e = parseInt(t.getAttribute(D), 10),
                o = e - 1;
            t.setAttribute(D, o), o || (t.removeAttribute(D), t.removeAttribute("aria-hidden"))
        })
    }
    var w, $, y, C = "modal-open",
        k = l.createNew(),
        T = a.createNew(),
        E = {
            NOW_CLOSING_EVENT: "modal.stack.now-closing"
        },
        S = 0,
        x = null,
        D = "data-bootstrap-modal-aria-hidden-count",
        M = "a[href], area[href], input:not([disabled]):not([tabindex='-1']), button:not([disabled]):not([tabindex='-1']),select:not([disabled]):not([tabindex='-1']), textarea:not([disabled]):not([tabindex='-1']), iframe, object, embed, *[tabindex]:not([tabindex='-1']), *[contenteditable=true]",
        O = /[A-Z]/g;
    return n.$watch(c, function(t) {
        $ && ($.index = t)
    }), o.on("keydown", b), n.$on("$destroy", function() {
        o.off("keydown", b)
    }), E.open = function(e, r) {
        function a(t) {
            function e(t) {
                var e = t.parent() ? t.parent().children() : [];
                return Array.prototype.filter.call(e, function(e) {
                    return e !== t[0]
                })
            }
            if (t && "BODY" !== t[0].tagName) return e(t).forEach(function(t) {
                var e = "true" === t.getAttribute("aria-hidden"),
                    o = parseInt(t.getAttribute(D), 10);
                o || (o = e ? 1 : 0), t.setAttribute(D, o + 1), t.setAttribute("aria-hidden", "true")
            }), a(t.parent())
        }
        var l = o[0].activeElement,
            u = r.openedClass || C;
        f(!1), x = k.top(), k.add(e, {
            deferred: r.deferred,
            renderDeferred: r.renderDeferred,
            closedDeferred: r.closedDeferred,
            modalScope: r.scope,
            backdrop: r.backdrop,
            keyboard: r.keyboard,
            openedClass: r.openedClass,
            windowTopClass: r.windowTopClass,
            animation: r.animation,
            appendTo: r.appendTo
        }), T.put(u, e);
        var d = r.appendTo,
            m = c();
        m >= 0 && !w && ($ = n.$new(!0), $.modalOptions = r, $.index = m, w = angular.element('<div uib-modal-backdrop="modal-backdrop"></div>'), w.attr({
            "class": "modal-backdrop",
            "ng-style": "{'z-index': 1040 + (index && 1 || 0) + index*10}",
            "uib-modal-animation-class": "fade",
            "modal-in-class": "in"
        }), r.backdropClass && w.addClass(r.backdropClass), r.animation && w.attr("modal-animation", "true"), i(w)($), t.enter(w, d), p.isScrollable(d) && (y = p.scrollbarPadding(d), y.heightOverflow && y.scrollbarWidth && d.css({
            paddingRight: y.right + "px"
        })));
        var h;
        r.component ? (h = document.createElement(s(r.component.name)), h = angular.element(h), h.attr({
            resolve: "$resolve",
            "modal-instance": "$uibModalInstance",
            close: "$close($value)",
            dismiss: "$dismiss($value)"
        })) : h = r.content, S = x ? parseInt(x.value.modalDomEl.attr("index"), 10) + 1 : 0;
        var b = angular.element('<div uib-modal-window="modal-window"></div>');
        b.attr({
            "class": "modal",
            "template-url": r.windowTemplateUrl,
            "window-top-class": r.windowTopClass,
            role: "dialog",
            "aria-labelledby": r.ariaLabelledBy,
            "aria-describedby": r.ariaDescribedBy,
            size: r.size,
            index: S,
            animate: "animate",
            "ng-style": "{'z-index': 1050 + $$topModalIndex*10, display: 'block'}",
            tabindex: -1,
            "uib-modal-animation-class": "fade",
            "modal-in-class": "in"
        }).append(h), r.windowClass && b.addClass(r.windowClass), r.animation && b.attr("modal-animation", "true"), d.addClass(u), r.scope && (r.scope.$$topModalIndex = S), t.enter(i(b)(r.scope), d), k.top().value.modalDomEl = b, k.top().value.modalOpener = l, a(b)
    }, E.close = function(t, e) {
        var o = k.get(t);
        return g(), o && v(o, e, !0) ? (o.value.modalScope.$$uibDestructionScheduled = !0, o.value.deferred.resolve(e), d(t, o.value.modalOpener), !0) : !o
    }, E.dismiss = function(t, e) {
        var o = k.get(t);
        return g(), o && v(o, e, !1) ? (o.value.modalScope.$$uibDestructionScheduled = !0, o.value.deferred.reject(e), d(t, o.value.modalOpener), !0) : !o
    }, E.dismissAll = function(t) {
        for (var e = this.getTop(); e && this.dismiss(e.key, t);) e = this.getTop()
    }, E.getTop = function() {
        return k.top()
    }, E.modalRendered = function(t) {
        var e = k.get(t);
        e && e.value.renderDeferred.resolve()
    }, E.focusFirstFocusableElement = function(t) {
        return t.length > 0 ? (t[0].focus(), !0) : !1
    }, E.focusLastFocusableElement = function(t) {
        return t.length > 0 ? (t[t.length - 1].focus(), !0) : !1
    }, E.isModalFocused = function(t, e) {
        if (t && e) {
            var o = e.value.modalDomEl;
            if (o && o.length) return (t.target || t.srcElement) === o[0]
        }
        return !1
    }, E.isFocusInFirstItem = function(t, e) {
        return e.length > 0 ? (t.target || t.srcElement) === e[0] : !1
    }, E.isFocusInLastItem = function(t, e) {
        return e.length > 0 ? (t.target || t.srcElement) === e[e.length - 1] : !1
    }, E.loadFocusElementList = function(t) {
        if (t) {
            var e = t.value.modalDomEl;
            if (e && e.length) {
                var o = e[0].querySelectorAll(M);
                return o ? Array.prototype.filter.call(o, function(t) {
                    return u(t)
                }) : o
            }
        }
    }, E
}]).provider("$uibModal", function() {
    var t = {
        options: {
            animation: !0,
            backdrop: !0,
            keyboard: !0
        },
        $get: ["$rootScope", "$q", "$document", "$templateRequest", "$controller", "$uibResolve", "$uibModalStack", function(e, o, i, n, r, a, l) {
            function p(t) {
                return t.template ? o.when(t.template) : n(angular.isFunction(t.templateUrl) ? t.templateUrl() : t.templateUrl)
            }
            var s = {},
                u = null;
            return s.getPromiseChain = function() {
                return u
            }, s.open = function(n) {
                function s() {
                    return b
                }
                var c = o.defer(),
                    d = o.defer(),
                    f = o.defer(),
                    m = o.defer(),
                    h = {
                        result: c.promise,
                        opened: d.promise,
                        closed: f.promise,
                        rendered: m.promise,
                        close: function(t) {
                            return l.close(h, t)
                        },
                        dismiss: function(t) {
                            return l.dismiss(h, t)
                        }
                    };
                if (n = angular.extend({}, t.options, n), n.resolve = n.resolve || {}, n.appendTo = n.appendTo || i.find("body").eq(0), !n.appendTo.length) throw new Error("appendTo element not found. Make sure that the element passed is in DOM.");
                if (!n.component && !n.template && !n.templateUrl) throw new Error("One of component or template or templateUrl options is required.");
                var b;
                b = n.component ? o.when(a.resolve(n.resolve, {}, null, null)) : o.all([p(n), a.resolve(n.resolve, {}, null, null)]);
                var v;
                return v = u = o.all([u]).then(s, s).then(function(t) {
                    function o(e, o, i, n) {
                        e.$scope = a, e.$scope.$resolve = {}, i ? e.$scope.$uibModalInstance = h : e.$uibModalInstance = h;
                        var r = o ? t[1] : t;
                        angular.forEach(r, function(t, o) {
                            n && (e[o] = t), e.$scope.$resolve[o] = t
                        })
                    }
                    var i = n.scope || e,
                        a = i.$new();
                    a.$close = h.close, a.$dismiss = h.dismiss, a.$on("$destroy", function() {
                        a.$$uibDestructionScheduled || a.$dismiss("$uibUnscheduledDestruction")
                    });
                    var p, s, u = {
                            scope: a,
                            deferred: c,
                            renderDeferred: m,
                            closedDeferred: f,
                            animation: n.animation,
                            backdrop: n.backdrop,
                            keyboard: n.keyboard,
                            backdropClass: n.backdropClass,
                            windowTopClass: n.windowTopClass,
                            windowClass: n.windowClass,
                            windowTemplateUrl: n.windowTemplateUrl,
                            ariaLabelledBy: n.ariaLabelledBy,
                            ariaDescribedBy: n.ariaDescribedBy,
                            size: n.size,
                            openedClass: n.openedClass,
                            appendTo: n.appendTo
                        },
                        b = {},
                        v = {};
                    n.component ? (o(b, !1, !0, !1), b.name = n.component, u.component = b) : n.controller && (o(v, !0, !1, !0), s = r(n.controller, v, !0, n.controllerAs), n.controllerAs && n.bindToController && (p = s.instance, p.$close = a.$close, p.$dismiss = a.$dismiss, angular.extend(p, {
                        $resolve: v.$scope.$resolve
                    }, i)), p = s(), angular.isFunction(p.$onInit) && p.$onInit()), n.component || (u.content = t[0]), l.open(h, u), d.resolve(!0)
                }, function(t) {
                    d.reject(t), c.reject(t)
                })["finally"](function() {
                    u === v && (u = null)
                }), h
            }, s
        }]
    };
    return t
}), angular.module("ui.bootstrap.multiMap.v2", []).factory("$$multiMap", function() {
    return {
        createNew: function() {
            var t = {};
            return {
                entries: function() {
                    return Object.keys(t).map(function(e) {
                        return {
                            key: e,
                            value: t[e]
                        }
                    })
                },
                get: function(e) {
                    return t[e]
                },
                hasKey: function(e) {
                    return !!t[e]
                },
                keys: function() {
                    return Object.keys(t)
                },
                put: function(e, o) {
                    t[e] || (t[e] = []), t[e].push(o)
                },
                remove: function(e, o) {
                    var i = t[e];
                    if (i) {
                        var n = i.indexOf(o); - 1 !== n && i.splice(n, 1), i.length || delete t[e]
                    }
                }
            }
        }
    }
}), angular.module("ui.bootstrap.stackedMap.v2", []).factory("$$stackedMap", function() {
    return {
        createNew: function() {
            var t = [];
            return {
                add: function(e, o) {
                    t.push({
                        key: e,
                        value: o
                    })
                },
                get: function(e) {
                    for (var o = 0; o < t.length; o++)
                        if (e === t[o].key) return t[o]
                },
                keys: function() {
                    for (var e = [], o = 0; o < t.length; o++) e.push(t[o].key);
                    return e
                },
                top: function() {
                    return t[t.length - 1]
                },
                remove: function(e) {
                    for (var o = -1, i = 0; i < t.length; i++)
                        if (e === t[i].key) {
                            o = i;
                            break
                        } return t.splice(o, 1)[0]
                },
                removeTop: function() {
                    return t.pop()
                },
                length: function() {
                    return t.length
                }
            }
        }
    }
}), angular.module("ui.bootstrap.position.v2", []).factory("$uibPosition", ["$document", "$window", function(t, e) {
    var o, i, n = {
            normal: /(auto|scroll)/,
            hidden: /(auto|scroll|hidden)/
        },
        r = {
            auto: /\s?auto?\s?/i,
            primary: /^(top|bottom|left|right)$/,
            secondary: /^(top|bottom|left|right|center)$/,
            vertical: /^(top|bottom)$/
        },
        a = /(HTML|BODY)/;
    return {
        getRawNode: function(t) {
            return t.nodeName ? t : t[0] || t
        },
        parseStyle: function(t) {
            return t = parseFloat(t), isFinite(t) ? t : 0
        },
        offsetParent: function(o) {
            function i(t) {
                return "static" === (e.getComputedStyle(t).position || "static")
            }
            o = this.getRawNode(o);
            for (var n = o.offsetParent || t[0].documentElement; n && n !== t[0].documentElement && i(n);) n = n.offsetParent;
            return n || t[0].documentElement
        },
        scrollbarWidth: function(n) {
            if (n) {
                if (angular.isUndefined(i)) {
                    var r = t.find("body");
                    r.addClass("uib-position-body-scrollbar-measure"), i = e.innerWidth - r[0].clientWidth, i = isFinite(i) ? i : 0, r.removeClass("uib-position-body-scrollbar-measure")
                }
                return i
            }
            if (angular.isUndefined(o)) {
                var a = angular.element('<div class="uib-position-scrollbar-measure"></div>');
                t.find("body").append(a), o = a[0].offsetWidth - a[0].clientWidth, o = isFinite(o) ? o : 0, a.remove()
            }
            return o
        },
        scrollbarPadding: function(t) {
            t = this.getRawNode(t);
            var o = e.getComputedStyle(t),
                i = this.parseStyle(o.paddingRight),
                n = this.parseStyle(o.paddingBottom),
                r = this.scrollParent(t, !1, !0),
                l = this.scrollbarWidth(a.test(r.tagName));
            return {
                scrollbarWidth: l,
                widthOverflow: r.scrollWidth > r.clientWidth,
                right: i + l,
                originalRight: i,
                heightOverflow: r.scrollHeight > r.clientHeight,
                bottom: n + l,
                originalBottom: n
            }
        },
        isScrollable: function(t, o) {
            t = this.getRawNode(t);
            var i = o ? n.hidden : n.normal,
                r = e.getComputedStyle(t);
            return i.test(r.overflow + r.overflowY + r.overflowX)
        },
        scrollParent: function(o, i, r) {
            o = this.getRawNode(o);
            var a = i ? n.hidden : n.normal,
                l = t[0].documentElement,
                p = e.getComputedStyle(o);
            if (r && a.test(p.overflow + p.overflowY + p.overflowX)) return o;
            var s = "absolute" === p.position,
                u = o.parentElement || l;
            if (u === l || "fixed" === p.position) return l;
            for (; u.parentElement && u !== l;) {
                var c = e.getComputedStyle(u);
                if (s && "static" !== c.position && (s = !1), !s && a.test(c.overflow + c.overflowY + c.overflowX)) break;
                u = u.parentElement
            }
            return u
        },
        position: function(o, i) {
            o = this.getRawNode(o);
            var n = this.offset(o);
            if (i) {
                var r = e.getComputedStyle(o);
                n.top -= this.parseStyle(r.marginTop), n.left -= this.parseStyle(r.marginLeft)
            }
            var a = this.offsetParent(o),
                l = {
                    top: 0,
                    left: 0
                };
            return a !== t[0].documentElement && (l = this.offset(a), l.top += a.clientTop - a.scrollTop, l.left += a.clientLeft - a.scrollLeft), {
                width: Math.round(angular.isNumber(n.width) ? n.width : o.offsetWidth),
                height: Math.round(angular.isNumber(n.height) ? n.height : o.offsetHeight),
                top: Math.round(n.top - l.top),
                left: Math.round(n.left - l.left)
            }
        },
        offset: function(o) {
            o = this.getRawNode(o);
            var i = o.getBoundingClientRect();
            return {
                width: Math.round(angular.isNumber(i.width) ? i.width : o.offsetWidth),
                height: Math.round(angular.isNumber(i.height) ? i.height : o.offsetHeight),
                top: Math.round(i.top + (e.pageYOffset || t[0].documentElement.scrollTop)),
                left: Math.round(i.left + (e.pageXOffset || t[0].documentElement.scrollLeft))
            }
        },
        viewportOffset: function(o, i, n) {
            o = this.getRawNode(o), n = n !== !1 ? !0 : !1;
            var r = o.getBoundingClientRect(),
                a = {
                    top: 0,
                    left: 0,
                    bottom: 0,
                    right: 0
                },
                l = i ? t[0].documentElement : this.scrollParent(o),
                p = l.getBoundingClientRect();
            if (a.top = p.top + l.clientTop, a.left = p.left + l.clientLeft, l === t[0].documentElement && (a.top += e.pageYOffset, a.left += e.pageXOffset), a.bottom = a.top + l.clientHeight, a.right = a.left + l.clientWidth, n) {
                var s = e.getComputedStyle(l);
                a.top += this.parseStyle(s.paddingTop), a.bottom -= this.parseStyle(s.paddingBottom), a.left += this.parseStyle(s.paddingLeft), a.right -= this.parseStyle(s.paddingRight)
            }
            return {
                top: Math.round(r.top - a.top),
                bottom: Math.round(a.bottom - r.bottom),
                left: Math.round(r.left - a.left),
                right: Math.round(a.right - r.right)
            }
        },
        parsePlacement: function(t) {
            var e = r.auto.test(t);
            return e && (t = t.replace(r.auto, "")), t = t.split("-"), t[0] = t[0] || "top", r.primary.test(t[0]) || (t[0] = "top"), t[1] = t[1] || "center", r.secondary.test(t[1]) || (t[1] = "center"), t[2] = e ? !0 : !1, t
        },
        positionElements: function(t, o, i, n) {
            t = this.getRawNode(t), o = this.getRawNode(o);
            var a = angular.isDefined(o.offsetWidth) ? o.offsetWidth : o.prop("offsetWidth"),
                l = angular.isDefined(o.offsetHeight) ? o.offsetHeight : o.prop("offsetHeight");
            i = this.parsePlacement(i);
            var p = n ? this.offset(t) : this.position(t),
                s = {
                    top: 0,
                    left: 0,
                    placement: ""
                };
            if (i[2]) {
                var u = this.viewportOffset(t, n),
                    c = e.getComputedStyle(o),
                    d = {
                        width: a + Math.round(Math.abs(this.parseStyle(c.marginLeft) + this.parseStyle(c.marginRight))),
                        height: l + Math.round(Math.abs(this.parseStyle(c.marginTop) + this.parseStyle(c.marginBottom)))
                    };
                if (i[0] = "top" === i[0] && d.height > u.top && d.height <= u.bottom ? "bottom" : "bottom" === i[0] && d.height > u.bottom && d.height <= u.top ? "top" : "left" === i[0] && d.width > u.left && d.width <= u.right ? "right" : "right" === i[0] && d.width > u.right && d.width <= u.left ? "left" : i[0], i[1] = "top" === i[1] && d.height - p.height > u.bottom && d.height - p.height <= u.top ? "bottom" : "bottom" === i[1] && d.height - p.height > u.top && d.height - p.height <= u.bottom ? "top" : "left" === i[1] && d.width - p.width > u.right && d.width - p.width <= u.left ? "right" : "right" === i[1] && d.width - p.width > u.left && d.width - p.width <= u.right ? "left" : i[1], "center" === i[1])
                    if (r.vertical.test(i[0])) {
                        var f = p.width / 2 - a / 2;
                        u.left + f < 0 && d.width - p.width <= u.right ? i[1] = "left" : u.right + f < 0 && d.width - p.width <= u.left && (i[1] = "right")
                    } else {
                        var m = p.height / 2 - d.height / 2;
                        u.top + m < 0 && d.height - p.height <= u.bottom ? i[1] = "top" : u.bottom + m < 0 && d.height - p.height <= u.top && (i[1] = "bottom")
                    }
            }
            switch (i[0]) {
                case "top":
                    s.top = p.top - l;
                    break;
                case "bottom":
                    s.top = p.top + p.height;
                    break;
                case "left":
                    s.left = p.left - a;
                    break;
                case "right":
                    s.left = p.left + p.width
            }
            switch (i[1]) {
                case "top":
                    s.top = p.top;
                    break;
                case "bottom":
                    s.top = p.top + p.height - l;
                    break;
                case "left":
                    s.left = p.left;
                    break;
                case "right":
                    s.left = p.left + p.width - a;
                    break;
                case "center":
                    r.vertical.test(i[0]) ? s.left = p.left + p.width / 2 - a / 2 : s.top = p.top + p.height / 2 - l / 2
            }
            return s.top = Math.round(s.top), s.left = Math.round(s.left), s.placement = "center" === i[1] ? i[0] : i[0] + "-" + i[1], s
        },
        adjustTop: function(t, e, o, i) {
            return -1 !== t.indexOf("top") && o !== i ? {
                top: e.top - i + "px"
            } : void 0
        },
        positionArrow: function(t, o) {
            t = this.getRawNode(t);
            var i = t.querySelector(".tooltip-inner, .popover-inner");
            if (i) {
                var n = angular.element(i).hasClass("tooltip-inner"),
                    a = t.querySelector(n ? ".tooltip-arrow" : ".arrow");
                if (a) {
                    var l = {
                        top: "",
                        bottom: "",
                        left: "",
                        right: ""
                    };
                    if (o = this.parsePlacement(o), "center" === o[1]) return void angular.element(a).css(l);
                    var p = "border-" + o[0] + "-width",
                        s = e.getComputedStyle(a)[p],
                        u = "border-";
                    u += r.vertical.test(o[0]) ? o[0] + "-" + o[1] : o[1] + "-" + o[0], u += "-radius";
                    var c = e.getComputedStyle(n ? i : t)[u];
                    switch (o[0]) {
                        case "top":
                            l.bottom = n ? "0" : "-" + s;
                            break;
                        case "bottom":
                            l.top = n ? "0" : "-" + s;
                            break;
                        case "left":
                            l.right = n ? "0" : "-" + s;
                            break;
                        case "right":
                            l.left = n ? "0" : "-" + s
                    }
                    l[o[1]] = c, angular.element(a).css(l)
                }
            }
        }
    }
}]), angular.module("ui.bootstrap.tooltip.v2", ["ui.bootstrap.position.v2", "ui.bootstrap.stackedMap.v2"]).provider("$uibTooltip", function() {
    function t(t) {
        var e = /[A-Z]/g,
            o = "-";
        return t.replace(e, function(t, e) {
            return (e ? o : "") + t.toLowerCase()
        })
    }
    var e = {
            placement: "top",
            placementClassPrefix: "",
            animation: !0,
            popupDelay: 0,
            popupCloseDelay: 0,
            useContentExp: !1
        },
        o = {
            mouseenter: "mouseleave",
            click: "click",
            outsideClick: "outsideClick",
            focus: "blur",
            none: ""
        },
        i = {};
    this.options = function(t) {
        angular.extend(i, t)
    }, this.setTriggers = function(t) {
        angular.extend(o, t)
    }, this.$get = ["$window", "$compile", "$timeout", "$document", "$uibPosition", "$interpolate", "$rootScope", "$parse", "$$stackedMap", function(n, r, a, l, p, s, u, c, d) {
        function f(t) {
            if (27 === t.which) {
                var e = m.top();
                e && (e.value.close(), e = null)
            }
        }
        var m = d.createNew();
        return l.on("keyup", f), u.$on("$destroy", function() {
                l.off("keyup", f)
            }),
            function(n, u, d, f) {
                function h(t) {
                    var e = (t || f.trigger || d).split(" "),
                        i = e.map(function(t) {
                            return o[t] || t
                        });
                    return {
                        show: e,
                        hide: i
                    }
                }
                f = angular.extend({}, e, i, f);
                var b = t(n),
                    v = s.startSymbol(),
                    g = s.endSymbol(),
                    w = "<div " + b + '-popup uib-title="' + v + "title" + g + '" ' + (f.useContentExp ? 'content-exp="contentExp()" ' : 'content="' + v + "content" + g + '" ') + 'origin-scope="origScope" class="uib-position-measure ' + u + '" tooltip-animation-class="fade"uib-tooltip-classes ng-class="{ in: isOpen }" ></div>';
                return {
                    compile: function() {
                        var t = r(w);
                        return function(e, o, i) {
                            function r() {
                                H.isOpen ? d() : s()
                            }

                            function s() {
                                (!B || e.$eval(i[u + "Enable"])) && (w(), C(), H.popupDelay ? A || (A = a(b, H.popupDelay, !1)) : b())
                            }

                            function d() {
                                v(), H.popupCloseDelay ? P || (P = a(g, H.popupCloseDelay, !1)) : g()
                            }

                            function b() {
                                return v(), w(), H.content ? ($(), void H.$evalAsync(function() {
                                    H.isOpen = !0, k(!0), _()
                                })) : angular.noop
                            }

                            function v() {
                                A && (a.cancel(A), A = null), I && (a.cancel(I), I = null)
                            }

                            function g() {
                                H && H.$evalAsync(function() {
                                    H && (H.isOpen = !1, k(!1), H.animation ? N || (N = a(y, 150, !1)) : y())
                                })
                            }

                            function w() {
                                P && (a.cancel(P), P = null), N && (a.cancel(N), N = null)
                            }

                            function $() {
                                M || (O = H.$new(), M = t(O, function(t) {
                                    L ? l.find("body").append(t) : o.after(t)
                                }), m.add(H, {
                                    close: g
                                }), T())
                            }

                            function y() {
                                v(), w(), E(), M && (M.remove(), M = null, R && a.cancel(R)), m.remove(H), O && (O.$destroy(), O = null)
                            }

                            function C() {
                                H.title = i[u + "Title"], H.content = j ? j(e) : i[n], H.popupClass = i[u + "Class"], H.placement = angular.isDefined(i[u + "Placement"]) ? i[u + "Placement"] : f.placement;
                                var t = p.parsePlacement(H.placement);
                                F = t[1] ? t[0] + "-" + t[1] : t[0];
                                var o = parseInt(i[u + "PopupDelay"], 10),
                                    r = parseInt(i[u + "PopupCloseDelay"], 10);
                                H.popupDelay = isNaN(o) ? f.popupDelay : o, H.popupCloseDelay = isNaN(r) ? f.popupCloseDelay : r
                            }

                            function k(t) {
                                q && angular.isFunction(q.assign) && q.assign(e, t)
                            }

                            function T() {
                                z.length = 0, j ? (z.push(e.$watch(j, function(t) {
                                    H.content = t, !t && H.isOpen && g()
                                })), z.push(O.$watch(function() {
                                    U || (U = !0, O.$$postDigest(function() {
                                        U = !1, H && H.isOpen && _()
                                    }))
                                }))) : z.push(i.$observe(n, function(t) {
                                    H.content = t, !t && H.isOpen ? g() : _()
                                })), z.push(i.$observe(u + "Title", function(t) {
                                    H.title = t, H.isOpen && _()
                                })), z.push(i.$observe(u + "Placement", function(t) {
                                    H.placement = t ? t : f.placement, H.isOpen && _()
                                }))
                            }

                            function E() {
                                z.length && (angular.forEach(z, function(t) {
                                    t()
                                }), z.length = 0)
                            }

                            function S(t) {
                                H && H.isOpen && M && (o[0].contains(t.target) || M[0].contains(t.target) || d())
                            }

                            function x(t) {
                                27 === t.which && d()
                            }

                            function D() {
                                var t = [],
                                    n = [],
                                    a = e.$eval(i[u + "Trigger"]);
                                Y(), angular.isObject(a) ? (Object.keys(a).forEach(function(e) {
                                    t.push(e), n.push(a[e])
                                }), W = {
                                    show: t,
                                    hide: n
                                }) : W = h(a), "none" !== W.show && W.show.forEach(function(t, e) {
                                    "outsideClick" === t ? (o.on("click", r), l.on("click", S)) : t === W.hide[e] ? o.on(t, r) : t && (o.on(t, s), o.on(W.hide[e], d)), o.on("keypress", x)
                                })
                            }
                            var M, O, N, A, P, I, R, F, L = angular.isDefined(f.appendToBody) ? f.appendToBody : !1,
                                W = h(void 0),
                                B = angular.isDefined(i[u + "Enable"]),
                                H = e.$new(!0),
                                U = !1,
                                q = angular.isDefined(i[u + "IsOpen"]) ? c(i[u + "IsOpen"]) : !1,
                                j = f.useContentExp ? c(i[n]) : !1,
                                z = [],
                                _ = function() {
                                    M && M.html() && (I || (I = a(function() {
                                        var t = p.positionElements(o, M, H.placement, L),
                                            e = angular.isDefined(M.offsetHeight) ? M.offsetHeight : M.prop("offsetHeight"),
                                            i = L ? p.offset(o) : p.position(o);
                                        M.css({
                                            top: t.top + "px",
                                            left: t.left + "px"
                                        });
                                        var n = t.placement.split("-");
                                        M.hasClass(n[0]) || (M.removeClass(F.split("-")[0]), M.addClass(n[0])), M.hasClass(f.placementClassPrefix + t.placement) || (M.removeClass(f.placementClassPrefix + F), M.addClass(f.placementClassPrefix + t.placement)), R = a(function() {
                                            var t = angular.isDefined(M.offsetHeight) ? M.offsetHeight : M.prop("offsetHeight"),
                                                o = p.adjustTop(n, i, e, t);
                                            o && M.css(o), R = null
                                        }, 0, !1), M.hasClass("uib-position-measure") ? (p.positionArrow(M, t.placement), M.removeClass("uib-position-measure")) : F !== t.placement && p.positionArrow(M, t.placement), F = t.placement, I = null
                                    }, 0, !1)))
                                };
                            H.origScope = e, H.isOpen = !1, H.contentExp = function() {
                                return H.content
                            }, i.$observe("disabled", function(t) {
                                t && v(), t && H.isOpen && g()
                            }), q && e.$watch(q, function(t) {
                                H && !t === H.isOpen && r()
                            });
                            var Y = function() {
                                W.show.forEach(function(t) {
                                    "outsideClick" === t ? o.off("click", r) : (o.off(t, s), o.off(t, r)), o.off("keypress", x)
                                }), W.hide.forEach(function(t) {
                                    "outsideClick" === t ? l.off("click", S) : o.off(t, d)
                                })
                            };
                            D();
                            var X = e.$eval(i[u + "Animation"]);
                            H.animation = angular.isDefined(X) ? !!X : f.animation;
                            var G, V = u + "AppendToBody";
                            G = V in i && void 0 === i[V] ? !0 : e.$eval(i[V]), L = angular.isDefined(G) ? G : L, e.$on("$destroy", function() {
                                Y(), y(), H = null
                            })
                        }
                    }
                }
            }
    }]
}).directive("uibTooltipTemplateTransclude", ["$animate", "$sce", "$compile", "$templateRequest", function(t, e, o, i) {
    return {
        link: function(n, r, a) {
            var l, p, s, u = n.$eval(a.tooltipTemplateTranscludeScope),
                c = 0,
                d = function() {
                    p && (p.remove(), p = null), l && (l.$destroy(), l = null), s && (t.leave(s).then(function() {
                        p = null
                    }), p = s, s = null)
                };
            n.$watch(e.parseAsResourceUrl(a.uibTooltipTemplateTransclude), function(e) {
                var a = ++c;
                e ? (i(e, !0).then(function(i) {
                    if (a === c) {
                        var n = u.$new(),
                            p = i,
                            f = o(p)(n, function(e) {
                                d(), t.enter(e, r)
                            });
                        l = n, s = f, l.$emit("$includeContentLoaded", e)
                    }
                }, function() {
                    a === c && (d(), n.$emit("$includeContentError", e))
                }), n.$emit("$includeContentRequested", e)) : d()
            }), n.$on("$destroy", d)
        }
    }
}]).directive("uibTooltipClasses", ["$uibPosition", function(t) {
    return {
        restrict: "A",
        link: function(e, o, i) {
            if (e.placement) {
                var n = t.parsePlacement(e.placement);
                o.addClass(n[0])
            }
            e.popupClass && o.addClass(e.popupClass), e.animation && o.addClass(i.tooltipAnimationClass)
        }
    }
}]).directive("uibTooltipPopup", function() {
    return {
        restrict: "A",
        scope: {
            content: "@"
        },
        templateUrl: "uib/template/tooltip/tooltip-popup.html"
    }
}).directive("uibTooltip", ["$uibTooltip", function(t) {
    return t("uibTooltip", "tooltip", "mouseenter")
}]).directive("uibTooltipTemplatePopup", function() {
    return {
        restrict: "A",
        scope: {
            contentExp: "&",
            originScope: "&"
        },
        templateUrl: "uib/template/tooltip/tooltip-template-popup.html"
    }
}).directive("uibTooltipTemplate", ["$uibTooltip", function(t) {
    return t("uibTooltipTemplate", "tooltip", "mouseenter", {
        useContentExp: !0
    })
}]).directive("uibTooltipHtmlPopup", function() {
    return {
        restrict: "A",
        scope: {
            contentExp: "&"
        },
        templateUrl: "uib/template/tooltip/tooltip-html-popup.html"
    }
}).directive("uibTooltipHtml", ["$uibTooltip", function(t) {
    return t("uibTooltipHtml", "tooltip", "mouseenter", {
        useContentExp: !0
    })
}]), angular.module("uib/template/modal/window.html", []).run(["$templateCache", function(t) {
    t.put("uib/template/modal/window.html", "<div class=\"modal-dialog {{size ? 'modal-' + size : ''}}\"><div class=\"modal-content\" uib-modal-transclude></div></div>\n")
}]), angular.module("uib/template/tooltip/tooltip-html-popup.html", []).run(["$templateCache", function(t) {
    t.put("uib/template/tooltip/tooltip-html-popup.html", '<div class="tooltip-arrow"></div>\n<div class="tooltip-inner" ng-bind-html="contentExp()"></div>\n')
}]), angular.module("uib/template/tooltip/tooltip-popup.html", []).run(["$templateCache", function(t) {
    t.put("uib/template/tooltip/tooltip-popup.html", '<div class="tooltip-arrow"></div>\n<div class="tooltip-inner" ng-bind="content"></div>\n')
}]), angular.module("uib/template/tooltip/tooltip-template-popup.html", []).run(["$templateCache", function(t) {
    t.put("uib/template/tooltip/tooltip-template-popup.html", '<div class="tooltip-arrow"></div>\n<div class="tooltip-inner"\n  uib-tooltip-template-transclude="contentExp()"\n  tooltip-template-transclude-scope="originScope()"></div>\n')
}]), angular.module("ui.bootstrap.position.v2").run(function() {
    !angular.$$csp().noInlineStyle && !angular.$$uibPositionCss && angular.element(document).find("head").prepend('<style type="text/css">.uib-position-measure{display:block !important;visibility:hidden !important;position:absolute !important;top:-9999px !important;left:-9999px !important;}.uib-position-scrollbar-measure{position:absolute !important;top:-9999px !important;width:50px !important;height:50px !important;overflow:scroll !important;}.uib-position-body-scrollbar-measure{overflow:scroll !important;}</style>'), angular.$$uibPositionCss = !0
}), angular.module("ui.bootstrap.tooltip.v2").run(function() {
    !angular.$$csp().noInlineStyle && !angular.$$uibTooltipCss && angular.element(document).find("head").prepend('<style type="text/css">[uib-tooltip-popup].tooltip.top-left > .tooltip-arrow,[uib-tooltip-popup].tooltip.top-right > .tooltip-arrow,[uib-tooltip-popup].tooltip.bottom-left > .tooltip-arrow,[uib-tooltip-popup].tooltip.bottom-right > .tooltip-arrow,[uib-tooltip-popup].tooltip.left-top > .tooltip-arrow,[uib-tooltip-popup].tooltip.left-bottom > .tooltip-arrow,[uib-tooltip-popup].tooltip.right-top > .tooltip-arrow,[uib-tooltip-popup].tooltip.right-bottom > .tooltip-arrow,[uib-tooltip-html-popup].tooltip.top-left > .tooltip-arrow,[uib-tooltip-html-popup].tooltip.top-right > .tooltip-arrow,[uib-tooltip-html-popup].tooltip.bottom-left > .tooltip-arrow,[uib-tooltip-html-popup].tooltip.bottom-right > .tooltip-arrow,[uib-tooltip-html-popup].tooltip.left-top > .tooltip-arrow,[uib-tooltip-html-popup].tooltip.left-bottom > .tooltip-arrow,[uib-tooltip-html-popup].tooltip.right-top > .tooltip-arrow,[uib-tooltip-html-popup].tooltip.right-bottom > .tooltip-arrow,[uib-tooltip-template-popup].tooltip.top-left > .tooltip-arrow,[uib-tooltip-template-popup].tooltip.top-right > .tooltip-arrow,[uib-tooltip-template-popup].tooltip.bottom-left > .tooltip-arrow,[uib-tooltip-template-popup].tooltip.bottom-right > .tooltip-arrow,[uib-tooltip-template-popup].tooltip.left-top > .tooltip-arrow,[uib-tooltip-template-popup].tooltip.left-bottom > .tooltip-arrow,[uib-tooltip-template-popup].tooltip.right-top > .tooltip-arrow,[uib-tooltip-template-popup].tooltip.right-bottom > .tooltip-arrow,[uib-popover-popup].popover.top-left > .arrow,[uib-popover-popup].popover.top-right > .arrow,[uib-popover-popup].popover.bottom-left > .arrow,[uib-popover-popup].popover.bottom-right > .arrow,[uib-popover-popup].popover.left-top > .arrow,[uib-popover-popup].popover.left-bottom > .arrow,[uib-popover-popup].popover.right-top > .arrow,[uib-popover-popup].popover.right-bottom > .arrow,[uib-popover-html-popup].popover.top-left > .arrow,[uib-popover-html-popup].popover.top-right > .arrow,[uib-popover-html-popup].popover.bottom-left > .arrow,[uib-popover-html-popup].popover.bottom-right > .arrow,[uib-popover-html-popup].popover.left-top > .arrow,[uib-popover-html-popup].popover.left-bottom > .arrow,[uib-popover-html-popup].popover.right-top > .arrow,[uib-popover-html-popup].popover.right-bottom > .arrow,[uib-popover-template-popup].popover.top-left > .arrow,[uib-popover-template-popup].popover.top-right > .arrow,[uib-popover-template-popup].popover.bottom-left > .arrow,[uib-popover-template-popup].popover.bottom-right > .arrow,[uib-popover-template-popup].popover.left-top > .arrow,[uib-popover-template-popup].popover.left-bottom > .arrow,[uib-popover-template-popup].popover.right-top > .arrow,[uib-popover-template-popup].popover.right-bottom > .arrow{top:auto;bottom:auto;left:auto;right:auto;margin:0;}[uib-popover-popup].popover,[uib-popover-html-popup].popover,[uib-popover-template-popup].popover{display:block !important;}</style>'), angular.$$uibTooltipCss = !0
});

;// JS/templateApp.js
angular.module("templateApp", []);


;// JS/viewapp/common/services/robloxService.js
// ~/viewapp/common/services/robloxService.js
var robloxAppService = angular.module("robloxApp.services", []);

;// JS/viewapp/common/services/httpService.js
// ~/viewapp/common/services/httpService.js
robloxAppService.factory("httpService", ["$http", "$q", "$log", function(n, t, i) {
    function r(n, t) {
        return t.withCredentials && (n.withCredentials = t.withCredentials), t.retryable && (n.retryable = t.retryable), t.noCache && (n.headers = {
            "Cache-Control": "no-cache, no-store, must-revalidate",
            Pragma: "no-cache",
            Expires: 0
        }), t.headers && (n.headers = angular.extend(n.headers || {}, t.headers || {})), t.withFile && (n.transformRequest = function(n) {
            var t = new FormData;
            return angular.forEach(n, function(n, i) {
                t.append(i, n)
            }), t
        }, n.headers = angular.extend(n.headers || {}, {
            "Content-Type": undefined
        })), n
    }

    function f(n, t) {
        var i = {
            method: "GET",
            url: n.url,
            params: t
        };
        return i = r(i, n)
    }

    function e(n, t) {
        var i = {
            method: "POST",
            url: n.url,
            data: t
        };
        return i = r(i, n)
    }

    function o(n) {
        var i = {
            method: "DELETE",
            url: n.url
        };
        return i = r(i, n)
    }

    function s(n, t) {
        var i = {
            method: "PATCH",
            url: n.url,
            data: t
        };
        return i = r(i, n)
    }

    function u(r) {
        var u = t.defer();
        return n(r).then(function(n) {
            var t = n.data;
            t === "null" && (t = null), u.resolve(t)
        }, function(n) {
            var t = n.data;
            i.debug("Error: unable to send " + r.url + " request."), u.reject(t)
        }), u.promise
    }
    return {
        methods: {
            get: "GET",
            post: "POST",
            "delete": "DELETE",
            patch: "PATCH"
        },
        httpGet: function(t, i, r) {
            if (!t) return !1;
            var e = f(t, i);
            return r ? n(e) : u(e)
        },
        httpPost: function(t, i, r) {
            if (!t) return !1;
            var f = e(t, i);
            return r ? n(f) : u(f)
        },
        httpDelete: function(n, t) {
            if (!n) return !1;
            var i = o(n, t);
            return u(i)
        },
        httpPatch: function(n, t) {
            if (!n) return !1;
            var i = s(n, t);
            return u(i)
        },
        buildBatchPromises: function(r, o, l, a, c) {
            if (!o || 0 === o.length) return t.when([]);
            for (var h = [], p = 0; p < o.length; p += l) h.push(o.slice(p, p + l));
            var d = this;
            return t.all(h.map(function(n) {
                var t = {};
                t[a] = n;
                return c && c.toUpperCase() === "POST" ? d.httpPost(r, t) : d.httpGet(r, t)
            }))
        }
    }
}]);

;// JS/viewapp/common/services/urlService.js
// ~/viewapp/common/services/urlService.js
robloxAppService.factory("urlService", [function() {
    function n(n) {
        return Roblox && Roblox.Endpoints ? Roblox.Endpoints.getAbsoluteUrl(n) : n
    }
    return {
        getAbsoluteUrl: n
    }
}]);

;// JS/viewapp/common/services/userService.js
// ~/viewapp/common/services/userService.js
robloxAppService.factory("userService", ["$http", function(n) {
    function i(i) {
        var r = t("/thumbnail/avatar-headshot"),
            u = {
                userId: i
            };
        return n({
            method: "GET",
            url: r,
            params: u,
            withCredentials: !0,
            retryable: !0
        })
    }

    function r(i) {
        var r = t("/presence/user"),
            u = {
                userId: i
            };
        return n({
            method: "GET",
            url: r,
            params: u,
            withCredentials: !0,
            retryable: !0
        })
    }

    function u(i) {
        var r = t("/thumbnail/avatar-headshots"),
            u = {
                userIds: i
            };
        return n({
            method: "GET",
            url: r,
            params: u,
            withCredentials: !0,
            retryable: !0
        })
    }

    function f(i) {
        var r = t("/presence/users"),
            u = {
                userIds: i
            };
        return n({
            method: "GET",
            url: r,
            params: u,
            withCredentials: !0,
            retryable: !0
        })
    }
    var t = function(n) {
        return Roblox && Roblox.Endpoints ? Roblox.Endpoints.getAbsoluteUrl(n) : n
    };
    return {
        getUserAvatar: i,
        getUserPresence: r,
        getMultiUserAvatar: u,
        getMultiUserPresence: f
    }
}]);

;// JS/viewapp/common/services/eventStreamService.js
// ~/viewapp/common/services/eventStreamService.js
robloxAppService.factory("eventStreamService", ["$log", function() {
    function t() {
        return Roblox && Roblox.EventStream
    }
    return {
        targetTypes: t() ? {
            DEFAULT: Roblox.EventStream.TargetTypes.DEFAULT,
            WWW: Roblox.EventStream.TargetTypes.WWW,
            STUDIO: Roblox.EventStream.TargetTypes.STUDIO,
            DIAGNOSTIC: Roblox.EventStream.TargetTypes.DIAGNOSTIC
        } : {
            DEFAULT: 0,
            WWW: 1,
            STUDIO: 2,
            DIAGNOSTIC: 3
        },
        eventNames: {
            notificationStream: {
                openFromNewIntro: "nsOpenFromNewIntro",
                openContent: "nsOpenContent",
                acceptFriendRequest: "nsAcceptFriendRequest",
                ignoreFriendRequest: "nsIgnoreFriendRequest",
                viewAllFriendRequests: "nsViewAllFriendRequests",
                chat: "nsChat",
                goToProfilePage: "nsGoToProfilePage",
                goToSettingPage: "nsGoToSettingPage"
            }
        },
        sendEventWithTarget: function(n, i, r, u) {
            t() && Roblox.EventStream.SendEventWithTarget && (u = u ? u : this.targetTypes.WWW, Roblox.EventStream.SendEventWithTarget(n, i, r, u))
        }
    }
}]);

;// JS/viewapp/common/services/hybridService.js
// ~/viewapp/common/services/hybridService.js
robloxAppService.factory("hybridService", ["$log", function() {
    function t() {
        return Roblox && Roblox.Hybrid
    }
    return {
        startChatConversation: function(n, i) {
            t() && Roblox.Hybrid.Chat && (angular.isUndefined(i) && (i = function() {}), Roblox.Hybrid.Chat.startChatConversation(n, i))
        },
        startWebChatConversation: function(n, i) {
            t() && Roblox.Hybrid.Navigation && (angular.isUndefined(i) && (i = function() {}), Roblox.Hybrid.Navigation.startWebChatConversation(n, i))
        },
        navigateToFeature: function(n, i) {
            t() && Roblox.Hybrid.Navigation && (angular.isUndefined(i) && (i = function() {}), Roblox.Hybrid.Navigation.navigateToFeature(n, i))
        },
        openUserProfile: function(n, i) {
            t() && Roblox.Hybrid.Navigation && (angular.isUndefined(i) && (i = function() {}), Roblox.Hybrid.Navigation.openUserProfile(n, i))
        }
    }
}]);

;// JS/viewapp/common/services/chatDispatchService.js
robloxAppService.factory("chatDispatchService", ["hybridService", "$document", "$log", function(n, t) {
        return {
            startChat:function(i, r) {
                if(r.androidApp.isEnabled&&r.androidApp.hybridRequired) {
                    var u= {
                        userIds:[]
                    }

                    ; u.userIds.push(i), n.startChatConversation(u)
                }

                else r.iOSApp.isEnabled&&r.iOSApp.hybridRequired?n.startWebChatConversation(i):r.uwpApp.isEnabled&&r.uwpApp.hybridRequired?n.startWebChatConversation(i):t.triggerHandler("Roblox.Chat.StartChat", {
                    userId:i
                })
        }

        , buildPermissionVerifier:function(n) {
            return {
                androidApp: {
                    isEnabled:n.inAndroidApp, hybridRequired: !0
                }

                , iOSApp: {
                    isEnabled:n.iniOSApp, hybridRequired: !0
                }

                , uwpApp: {
                    isEnabled:n.inUWPApp, hybridRequired: !1
                }
            }
        }
    }
}

]);

;// JS/viewapp/common/filters.js
// ~/viewapp/common/filters.js
var robloxFilters = angular.module("robloxApp.filters", []).filter("getPercentage", function() {
    return function(n, t) {
        var i = n + t;
        return n * 100 / i + "%"
    }
}).filter("htmlToPlaintext", function() {
    return function(n) {
        return String(n).replace(/<[^>]+>/gm, "")
    }
}).filter("isEmpty", function() {
    return function(n, t, i) {
        return i === "" || i === null || typeof i == "undefined" ? t : n
    }
}).filter("positive", function() {
    return function(n) {
        return n ? Math.abs(n) : 0
    }
}).filter("startsWith", function() {
    return function(n, t, i) {
        var u = [],
            r, f;
        if (n)
            for (r = 0; r < n.length; r++) f = i ? n[r][i].toLowerCase() : n[r].toLowerCase(), f.indexOf(t.toLowerCase()) === 0 && t.length < f.length && u.push(n[r]);
        return u.length === 0 && (u = n), u
    }
}).filter("firstLetter", function() {
    return function(n) {
        return n != null ? n.substring(0, 1).toLowerCase() : ""
    }
}).filter("reverse", function() {
    return function(n) {
        if (n && n.length > 0) return n.slice().reverse()
    }
}).filter("orderList", function() {
    return function(n, t) {
        var i = [],
            r;
        for (r in t) i.push(n[t[r]]);
        return i
    }
}).filter("abbreviate", ["$filter", function(n) {
    var i = null,
        u = ["thousand", "million", "billion"],
        t = {
            thousand: 1e3,
            million: 1e6,
            billion: 1e9
        },
        f = {
            thousand: "K+",
            million: "M+",
            billion: "B+"
        },
        r = function(r, u) {
            return i && u === i ? n("number")(r) : n("number")((r / t[u]).toFixed(0), 0) + f[u]
        };
    return function(f, e) {
        return (typeof e != "undefined" && (i = u[e]), f < t.thousand * 10) ? n("number")(f) : f < t.million ? r(f, "thousand") : f < t.billion ? r(f, "million") : r(f, "billion")
    }
}]).filter("parseTimeStamp", function() {
    return function(n) {
        return n ? parseInt(typeof n == "string" && n.search("Date") > -1 ? n.slice(6, -2) : n) : null
    }
}).filter("capitalize", function() {
    return function(n) {
        var i, r, t, u;
        if (n != null) {
            for (i = n.split(" "), r = [], t = 0; t < i.length; t++) u = i[t].toLowerCase(), r.push(u.substring(0, 1).toUpperCase() + u.substring(1));
            return r.join(" ")
        }
    }
});

;// JS/viewapp/app.js
// ~/viewapp/app.js
var robloxApp = (function() {
    try { return angular.module("robloxApp"); } catch(e) { return null; }
})() || angular.module("robloxApp", ["ngSanitize", "ui.router", "robloxApp.services", "robloxApp.filters", "templateApp"]).config(["$httpProvider", function(n) {
    var r = "X-CSRF-TOKEN",
        u = 403,
        t = angular.element("#http-retry-data"),
        f = t && t.data("http-retry-base-timeout") ? t.data("http-retry-base-timeout") : 1e3,
        e = t && t.data("http-retry-max-timeout") ? t.data("http-retry-max-timeout") : 8e3,
        i;
    n.interceptors.push(["$q", "$injector", function(n, t) {
        return {
            request: function(n) {
                return n.method.toLowerCase() === "post" && Roblox.XsrfToken.getToken() && (i || (i = Roblox.XsrfToken.getToken()), n.headers[r] = i), n
            },
            responseError: function(f) {
                var o = f.status,
                    s = t.get("$http"),
                    e;
                return o === u && f.headers(r) && (e = f.headers(r), e) ? (i = e, s(f.config)) : n.reject(f)
            }
        }
    }]), n.interceptors.push(["$q", "$injector", "$log", function(n, t, i) {
        function r(n) {
            var i = t.get("$timeout");
            return i(function() {
                n.incrementalTimeout = n.incrementalTimeout * 2;
                var i = t.get("$http");
                return i(n)
            }, n.incrementalTimeout)
        }
        return {
            responseError: function(t) {
                var o = t.status;
                return o !== u && angular.isDefined(t.config.retryable) && t.config.retryable ? (t.config.incrementalTimeout || (t.config.incrementalTimeout = f), i.debug("---- rejection.config.url ------" + t.config.url), i.debug("---- incrementalTimeout ------" + t.config.incrementalTimeout), t.config.incrementalTimeout <= e ? (i.debug("---- retry ------"), r(t.config)) : (t.config.incrementalTimeout = f, i.debug("---- failure promise ------"), n.reject(t))) : n.reject(t)
            }
        }
    }]), n.interceptors.push(["$q", "$injector", function() {
        return {
            request: function(n) {
                if (angular.isDefined(Roblox) && angular.isDefined(Roblox.Endpoints)) {
                    var t = Roblox.Endpoints.generateAbsoluteUrl(n.url, n.data, n.withCredentials);
                    n.url = t, Roblox.Endpoints.addCrossDomainOptionsToAllRequests && n.url.indexOf("rbxcdn.com") < 0 && n.url.indexOf("s3.amazonaws.com") < 0 && (n.withCredentials = !0)
                }
                return n
            }
        }
    }])
}]).config(["$logProvider", function(n) {
    var t = angular.isDefined(Roblox) && angular.isDefined(Roblox.jsConsoleEnabled) ? Roblox.jsConsoleEnabled : !1;
    n.debugEnabled(t)
}]).constant("_", window._ || {});

;// JS/viewapp/common/providers/languageResourceProvider.js
// ~/viewapp/common/providers/languageResourceProvider.js
angular.module("robloxApp").provider("languageResource", function() {
    var i = {},
        n = {},
        t, r = new Roblox.Intl,
        u = !1,
        f = function(n, t) {
            var u = i[n];
            return u ? t && Object.keys(t).length > 0 && (u = r.f(u, t)) : (console.warn("Language key '" + n + "' not found. Please check for any typo or a missing key."), u = ""), u
        },
        e = function(i, r, u) {
            if (u && typeof u == "string") {
                if (n[u]) return n[u].get(i, r);
                throw new Error("Provided NameSpace '" + u + "' is not found or is not set");
            }
            return n[t].get(i, r)
        };
    this.setLanguageKeysFromFile = function(n) {
        n && typeof n == "object" && !Array.isArray(n) && angular.extend(i, n)
    }, this.setTranslationResources = function(i) {
        angular.forEach(i, function(i) {
            i instanceof Roblox.TranslationResource && (n[i.nameSpace] = i, u = !0, t || (t = i.nameSpace))
        })
    }, this.$get = ["$log", function() {
        return {
            get: u ? e : f,
            intl: r,
            setLanguageKeysFromFile: function(n) {
                n && typeof n == "object" && !Array.isArray(n) && angular.extend(i, n)
            }
        }
    }]
});

;// JS/viewapp/common/filters/translate.js
// ~/viewapp/common/filters/translate.js
robloxApp.filter("translate", ["languageResource", "$log", function(n, t) {
    return function(i, r, u) {
        var f = n.get(i, r, u);
        return f ? f : (t.debug("Unable to translate key:" + i), "")
    }
}]);

;// JS/Reference/widget.js
// Reference/widget.js
var Roblox = Roblox || {};
Roblox.BootstrapWidgets = function() {
    function a() {
        $("#horizontal-tabs a").on("click", function(n) {
            n.preventDefault(), $(this).tab("show")
        });
        $("#horizontal-tabs a").on("touchstart", function(n) {
            n.preventDefault(), $(this).trigger("click")
        });
        $("#vertical-tabs a").click(function(n) {
            n.preventDefault(), $(this).tab("show")
        })
    }

    function tt() {
        $('[data-toggle="dropdown-menu"] li').click(function(n) {
            var t = $(n.currentTarget);
            return t.closest(".input-group-btn").find('[data-bind="label"]').text(t.text()).end().toggleClass("open"), t.hasClass("rbx-clickable-li") ? void 0 : !1
        })
    }

    function t(n, t) {
        var i = n.data("expanded-icon") || "icon-up-16x16",
            r = n.data("collapsed-icon") || "icon-down-16x16",
            f = t ? i : r,
            u = t ? r : i;
        n.prev(".panel-heading").find("." + u).removeClass(u).addClass(f)
    }

    function nt() {
        $('[data-toggle="collapsible-element"]').on("show.bs.collapse", function(n) {
            t($(n.target), !0)
        });
        $('[data-toggle="collapsible-element"]').on("hide.bs.collapse", function(n) {
            t($(n.target), !1)
        })
    }

    function g(n) {
        $(n).collapse("show")
    }

    function d() {
        if ("ontouchstart" in window) $('[data-toggle-mobile="true"]').tooltip({
            placement: "bottom",
            trigger: "manual"
        }).unbind().on("touchstart", function() {
            $(this).tooltip("toggle")
        });
        else $('[data-toggle="tooltip"]').tooltip({
            placement: "bottom"
        })
    }

    function k(n, t) {
        $(n).attr("title", t).tooltip("fixTitle")
    }

    function b() {
        $("body").on("click touchstart", function(n) {
            $('[data-toggle="tooltip"]').each(function() {
                if (!$(this).is(n.target) && $(this).has(n.target).length === 0) {
                    var t = n.type === "click" ? !0 : $(".tooltip").has(n.target).length === 0;
                    if (t) try {
                        $(this).tooltip("hide")
                    } catch (n) {
                        return !1
                    }
                }
            })
        })
    }

    function w(n, t) {
        n || (n = "bottom"), t || (t = {
            selector: "body",
            padding: 4
        });
        $("[data-toggle='popover']").popover({
            trigger: "manual",
            html: !0,
            placement: n,
            viewport: t,
            content: function() {
                var n = $(this).attr("data-bind");
                return $('[data-toggle="' + n + '"]').html()
            }
        }).unbind().on("click", function(e) {
            e.stopPropagation(), e.stopImmediatePropagation(), e.preventDefault();
            var $el = $(this), isOpen = $el.data("bs.popover") && $el.data("bs.popover").tip() && $el.data("bs.popover").tip().hasClass("in");
            isOpen ? $el.popover("hide") : $el.popover("show")
        })
    }

    function p() {
        $("body").on("click touchstart", function(n) {
            $('[data-toggle="popover"]').each(function() {
                if (!$(this).is(n.target) && $(this).has(n.target).length === 0) {
                    var t = $(".popover").has(n.target).length === 0;
                    n.type === "touchstart" && $(".popover").has(n.target).length > 0 ? t = !0 : n.type === "click" && (t = !0), t && $(this).popover("hide")
                }
            })
        })
    }

    function y() {
        $('[data-toggle="scrollbar"]').not('[data-mcs-init]').attr('data-mcs-init', '1').mCustomScrollbar({
            autoHideScrollbar: !1,
            autoExpandScrollbar: !1,
            scrollInertia: 0,
            mouseWheel: {
                preventDefault: !0
            }
        })
    }

    function v() {
        var n = $('[data-toggle="pagination"]'),
            t = $('[data-toggle="pager"]');
        (n.twbsPagination || t.twbsPagination) && (n.twbsPagination({
            totalPages: 35,
            visiblePages: 7,
            first: 1,
            last: 35,
            prev: '<span class="icon-left"></span>',
            next: '<span class="icon-right"></span>'
        }), t.twbsPagination({
            isPager: !0,
            totalPages: 35,
            visiblePages: 7,
            first: '<span class="icon-first-page"></span>',
            last: '<span class="icon-last-page"></span>',
            prev: '<span class="icon-left"></span>',
            next: '<span class="icon-right"></span>'
        }))
    }

    function n(n, t, i, r) {
        if (typeof n != "undefined") {
            var u, f;
            r && (u = n.clone(), u.html(r), n.after(u), f = n.detach()), t = typeof t == "undefined" ? 200 : t, i = typeof i == "undefined" ? 3e3 : i, setTimeout(function() {
                u ? u.addClass("on") : n.addClass("on")
            }, t), setTimeout(function() {
                u ? u.removeClass("on") : n.removeClass("on"), u && f && (u.after(f), u.remove())
            }, i)
        }
    }

    function l() {
        $("#toggle-alert-loading").click(function() {
            n($(".sg-alert-section .alert-loading"), 100, 1e3)
        }), $("#toggle-alert-success").click(function() {
            n($(".sg-alert-section .alert-success"), 100, 1e3)
        }), $("#toggle-alert-warning").click(function() {
            var n = $(".sg-alert-section .alert-warning"),
                t;
            setTimeout(function() {
                n.addClass("on")
            }, 100), t = $(".alert-system-feedback #close"), t.click(function() {
                n.removeClass("on")
            })
        })
    }

    function c() {
        $("input[placeholder]").focus(function() {
            var n = $(this);
            n.val() == n.attr("placeholder") && (n.val(""), n.removeClass("rbx-placeholder"))
        }).blur(function() {
            var n = $(this);
            (n.val() == "" || n.val() == n.attr("placeholder")) && (n.addClass("rbx-placeholder"), n.val(n.attr("placeholder")))
        })
    }

    function s() {
        h.each(function() {
            var t = $(this),
                n = $(this).clone().hide().height("auto");
            n.width(t.width()), $("body").append(n), n.height() <= t.height() && (t.removeClass(i), $(this).find(".toggle-para").hide()), n.remove()
        })
    }

    function o(n, t) {
        var i = "para-overflow-toggle",
            r = $("." + i),
            u = "para-height",
            f = "para-overflow-page-loading";
        n = n ? n : 24, t = t ? t : 5, $(".toggle-para").show(), r.each(function() {
            var r = $(this),
                e = $(this).clone().hide().height("auto"),
                o;
            e.width(r.width()), $("body").append(e), o = n * t, (e.height() <= o || e.height() <= r.height()) && (r.removeClass(i).removeClass(u), r.find(".toggle-para").last().hide()), r.removeClass(f), e.remove()
        })
    }

    function e(n, t) {
        var i = "para-overflow-toggle-off",
            r = "para-height";
        n || (n = "Read More"), t || (t = "Show Less"), $(".toggle-para").bind("click touchstart", function() {
            var u = $(".para-overflow-toggle");
            $(this).text() === n ? (u.removeClass(r).addClass(i), $(this).text(t)) : (u.removeClass(i).addClass(r), $(this).text(n))
        })
    }

    function f() {
        var n = "content-overflow-toggle",
            t = $("." + n),
            i = "content-height",
            r = "content-overflow-page-loading";
        $(".toggle-content").removeClass("hidden"), t.each(function() {
            var t = $(this),
                u = $(this).clone().hide().height("auto").width(t.width());
            t.parent().append(u), u.css("font-weight", t.css("font-weight"));
            var f = t.attr("id"),
                e = $(".toggle-content[data-container-id='" + f + "']"),
                o = $(".show-more-end[data-container-id='" + f + "']");
            o.removeClass("hide"), (u.height() <= t.height() || !e.is(":visible")) && (t.removeClass(n).removeClass(i), e.hide(), o.addClass("hide")), t.removeClass(r), u.remove()
        })
    }

    function u() {
        var n = "content-overflow-toggle-off",
            t = "content-height",
            i = "Read More",
            r = "Show Less",
            u = function() {
                $(this).unbind("click"), $(this).bind("click", function() {
                    var f = $(this).data("container-id"),
                        u = $("#" + f);
                    $(this).text() === i ? (u.removeClass(t).addClass(n), $(this).text(r), u.find(".show-more-end").addClass("hide")) : (u.removeClass(n).addClass(t), $(this).text(i), u.find(".show-more-end").removeClass("hide"))
                })
            };
        $(".toggle-content").each(u)
    }

    function r(n) {
        n = n ? n : "#carousel", $(n).carousel({
            interval: 6e3,
            pause: "hover"
        })
    }

    function it() {
        $(".btn-toggle").bind("click", function() {
            if ($(this).hasClass("disabled")) return !1;
            $(this).toggleClass("on"), $(this).trigger("toggleBtnClick", {
                id: $(this).attr("id"),
                toggleOn: $(this).hasClass("on")
            })
        })
    }

    function rt() {
        var i = 0,
            r = 0,
            u = ".menu-secondary-container",
            n = $(".submenus"),
            f = n.find("li"),
            t = n.find("li " + u),
            e = n.find("li " + u + "[hover=true]");
        t.on("mouseover touchstart", function() {
            $(this).attr("hover", "true")
        });
        t.mouseout(function() {
            $(this).attr("hover", "false")
        });
        f.on("mouseover touchstart", function() {
            var i = $(this).data("delay"),
                f;
            e.length === 0 && ($(this).attr("hover", "true"), i !== "never" && (r === 1 || i === "always") ? window.setTimeout(function() {
                if (e.length === 0) {
                    var i = n.find("li[hover=true] " + u);
                    t.hide(), i.length !== 0 && i.show()
                }
            }, 1e3) : (t.hide(), f = $(this).find(u), f.show()))
        });
        f.mouseout(function() {
            $(this).removeAttr("hover")
        }), n.mouseleave(function() {
            window.setTimeout(function() {
                t.hide()
            }, 100), i = 0, r = 0
        }), n.mousemove(function(n) {
            var t = i;
            i = n.pageX, (t === i || t === 0) && (r = 0), r = t < i ? 1 : -1
        });
        $("body").on("touchstart", function(i) {
            n.is(i.target) || n.has(i.target).length !== 0 || t.hide()
        })
    }
    var i = "para-overflow",
        h = $("." + i);
    return {
        SetupTabs: a,
        SetupDropdown: tt,
        SetupAccordion: nt,
        ShowAccordionMenu: g,
        SetupTooltip: d,
        UpdateTooltip: k,
        CloseTooltip: b,
        SetupPopover: w,
        ClosePopover: p,
        SetupScrollbar: y,
        SetupPagination: v,
        Placeholder: c,
        IsTruncated: s,
        TruncateParagraph: o,
        ToggleParagraph: e,
        SetupCarousel: r,
        SetupToggleButton: it,
        SetupSystemFeedback: l,
        ToggleSystemMessage: n,
        SetupVerticalMenu: rt,
        TruncateContent: f,
        ToggleContent: u
    }
}(), $(function() {
    Roblox.BootstrapWidgets.SetupTabs(), Roblox.BootstrapWidgets.SetupDropdown(), Roblox.BootstrapWidgets.SetupAccordion(), Roblox.BootstrapWidgets.SetupTooltip(), Roblox.BootstrapWidgets.CloseTooltip(), Roblox.BootstrapWidgets.SetupPopover(), Roblox.BootstrapWidgets.ClosePopover(), Roblox.BootstrapWidgets.SetupScrollbar(), Roblox.BootstrapWidgets.SetupPagination(), typeof Modernizr == "undefined" || Modernizr.input.placeholder || Roblox.BootstrapWidgets.Placeholder(), Roblox.BootstrapWidgets.IsTruncated(), Roblox.BootstrapWidgets.TruncateParagraph(), Roblox.BootstrapWidgets.ToggleParagraph(), Roblox.BootstrapWidgets.SetupCarousel(), Roblox.BootstrapWidgets.SetupToggleButton(), Roblox.BootstrapWidgets.SetupSystemFeedback(), Roblox.BootstrapWidgets.ToggleSystemMessage(), Roblox.BootstrapWidgets.SetupVerticalMenu(), Roblox.BootstrapWidgets.TruncateContent(), Roblox.BootstrapWidgets.ToggleContent()
});

;// JS/modules/Pages/Catalog.js
Roblox.define("Pages.Catalog", ["Widgets.ItemImage", "Widgets.HierarchicalDropdown", "Pages.CatalogShared"], function(n) {
        function l(t, f, l) {
            o=l, n.populate(), $(".roblox-item-image[data-retry-url]").loadRobloxThumbnails(), i=t, i.TotalNumberOfPages=f, i.EmptyStringSearchEnabled=$("#catalog").data("empty-search-enabled"), v(), $(".Paging_Input").keypress(function(n) {
                    n.which=="13" &&y()

                }), $("#keywordTextbox").keypress(function(n) {
                    if(n.which=="13")return e(), !1

                }), $("#creatorTextbox").keypress(function(n) {
                    if(n.which=="13")return s(), !1

                }), $(".pxInput").keypress(function(n) {
                    if(n.which=="13")return h(), !1

                }), $("select#categoriesForKeyword").change(function() {
                    i.EmptyStringSearchEnabled&&e( !1)

                }); var a=$("#legendcontent").css("display") !="none"; $("#legendheader").click(function() {
                    a?($("#legendcontent").hide(), $(this).removeClass("expanded")):($("#legendcontent").show(), $(this).addClass("expanded")), a= !a

                }); $(".assetTypeFilter").on("click", function() {
                    var t=$(this).data("category"), f=$(this).data("keepfilters"), n; return t !==undefined&&(f !==undefined?u({
                            types: !0, category: !0
                        }):c(), i.Category=t), n=$(this).data("types"), n !==undefined&&(i.Subcategory=n), r( !1), !1

            }); $(".gearFilter").click(function() {
                var n=$(this).data("types"), t=$(this).data("category"); t !==undefined?(c(), i.Category=t, n !="All" &&(i.Gears=n)):i.Gears=n=="All" ?null:n, r( !1)

            }), $(".genreFilter").click(function() {
                return i.Genres=$("input.genreFilter:checked").map(function() {
                        return $(this).data("genreid")
                    }).get().toString().split(","), i.Genres=="" &&(i.Genres=null), r( !1), !0

            }), $(".creatorFilter").click(function() {
                var n=$(this).data("creatorid"); i.CreatorID=n, r( !1)

            }), $(".breadCrumbFilter").click(function() {
                var n=$(this).data("filter"); switch(n) {
                    case"category":u({
                        types: !0, gears: !0, genres: !0, creator: !0, prices: !0, keyword: !0

                    }); break; case"subcategory":u({
                    gears: !0, genres: !0, creator: !0, prices: !0, keyword: !0

                }); break; case"gears":u({
                genres: !0, creator: !0, prices: !0, keyword: !0

            }); break; case"genres":u({
            creator: !0, prices: !0, keyword: !0

        }); break; case"creator":u({
        prices: !0, keyword: !0

    }); break; case"px":u({
    keyword: !0
})
}

r( !1)

}), $(".priceFilter").click(function() {
        i.CurrencyType=$(this).data("currencytype"), r( !1)

    }), $("#submitCreatorButton").click(s), $("#creatorTextbox").focus(function() {
        $(this).val()=="Name" &&$(this).val(""), $(this).removeClass("Watermark")

    }), $("#creatorTextbox").blur(function() {
        $(this).val()=="" &&($(this).val("Name"), $(this).addClass("Watermark"))

    }), $(".pxInput").focus(function() {
        ($(this).val()=="Min" ||$(this).val()=="Max")&&$(this).val(""), $(this).removeClass("Watermark")

    }), $(".pxInput").blur(function() {
        var n=$(this).data("watermarktext"); $(this).val()=="" &&($(this).val(n), $(this).addClass("Watermark"))

    }), $("#submitPxButton").click(h), $("a#submitSearchButton").click(e), $("select#SortMain").change(function() {
        i.SortType=document.getElementById("SortMain").value, r( !1)

    }), $("select#SortAggregation").change(function() {
        i.SortAggregation=document.getElementById("SortAggregation").value, i.SortCurrency=null, r( !1)

    }), $("select#SortCurrency").change(function() {
        i.SortCurrency=document.getElementById("SortCurrency").value, i.SortAggregation=null, r( !1)

    }), $("#includeNotForSaleCheckbox").change(function() {
        i.IncludeNotForSale=$(this).prop("checked"), r( !1)

    }), $("#pagingprevious").click(function() {
        $(this).hasClass("disabled")||(i.PageNumber--, i.PageNumber>=1&&r( !0))

    }), $("#pagingnext").click(function() {
        $(this).hasClass("disabled")||(i.PageNumber++, r( !0))
    }), Roblox.AdsHelper !=undefined&&Roblox.AdsHelper.AdRefresher !=undefined&&Roblox.AdsHelper.AdRefresher.registerAd("AdvertisingLeaderboard")
}

function s() {
    f=document.getElementById("creatorTextbox").value, f !="" &&(i.CreatorID=null, r( !1))
}

function h() {
    i.CurrencyType=$("#submitPxButton").data("currencytype"); var t=document.getElementById("pxMinInput").value, n=document.getElementById("pxMaxInput").value, u=isNaN(n); i.PxMin=t !="" &&parseInt(t)>0?t:null, i.PxMax=n=="" ||n=="0" ||u?null:n, r( !1)
}

function e(n) {
    if(n=typeof n=="undefined" ?o:n, i.Keyword=encodeURIComponent(document.getElementById("keywordTextbox").value), i.Keyword=="" && !i.EmptyStringSearchEnabled)return !1; var t=$("#categoriesForKeyword").val(); return n?t=="Custom" ?u({
        sorts: !0

    }):(u({
        category: !0, types: !0, gears: !0

    }), i.Category=t):t=="Custom" ?u({
    genres: !0, creator: !0, prices: !0, sorts: !0

}):(u({
        category: !0, types: !0, gears: !0, genres: !0, creator: !0, prices: !0
    }), i.Category=t), r( !1), !1
}

function a() {
    u({
        genres: !0
    }), r( !1)
}

function c() {
    u({
        category: !0, types: !0, gears: !0, genres: !0, creator: !0, prices: !0, keyword: !0
    })
}

function u(n) {
    n.category&&(i.Category=""), n.types&&(i.Subcategory=""), n.gears&&(i.Gears=null), n.genres&&(i.Genres=null), n.creator&&(i.CreatorID=null), n.prices&&(i.CurrencyType=null, i.PxMin=null, i.PxMax=null, i.IncludeNotForSale=null), n.keyword&&(i.Keyword=null), n.sorts&&(i.SortType=null, i.SortAggregation=null, i.SortCurrency=null)
}

function v() {
    i.PageNumber==1?$("#pagingprevious").addClass("disabled"):i.PageNumber==i.TotalNumberOfPages&&$("#pagingnext").addClass("disabled")
}

function y() {
    i.PageNumber=Math.round($("input.Paging_Input").val()), i.PageNumber>=1&&(i.PageNumber>i.TotalNumberOfPages&&(i.PageNumber=i.TotalNumberOfPages), r( !0))
}

function r(n) {
    var t="/catalog/browse.aspx?", u, o= !1, r, e; if(Roblox.CatalogValues&&(Roblox.CatalogValues.CatalogContentsUrl&&Roblox.CatalogValues.ContainerID&&(u=$("#" +Roblox.CatalogValues.ContainerID), u.length !==0&&(t=Roblox.CatalogValues.CatalogContentsUrl+"?", o= !0)), Roblox.CatalogValues.CatalogContext !==undefined&&(t+="CatalogContext=" +Roblox.CatalogValues.CatalogContext+"&")), i.Subcategory !=null&&i.Subcategory !="" &&(t+="Subcategory=" +i.Subcategory+"&"), i.Gears !=null&&(t+="Gears=" +i.Gears+"&"), i.Genres !=null)for(r=0; r<i.Genres.length; r++)t+="Genres=" +i.Genres[r]+"&"; i.CreatorID !=null&&i.CreatorID !=0?t+="CreatorID=" +i.CreatorID+"&":f !=null&&(t+="CreatorName=" +f+"&"), i.Keyword !=null&&i.Keyword !="" &&(t+="Keyword=" +i.Keyword+"&"), i.CurrencyType !=null&&i.CurrencyType !==0&&i.CurrencyType !=="0" &&(t+="CurrencyType=" +i.CurrencyType+"&"), i.PxMin !=null&&i.PxMin !==0&&i.PxMin !=="0" &&(t+="pxMin=" +i.PxMin+"&"), i.PxMax !=null&&i.PxMax !==0&&i.PxMax !=="0" &&(t+="pxMax=" +i.PxMax+"&"), i.SortType !=null&&i.SortType !==0&&i.SortType !=="0" &&(t+="SortType=" +i.SortType+"&"), i.SortAggregation !=null&&(t+="SortAggregation=" +i.SortAggregation+"&"), i.SortCurrency !=null&&i.SortCurrency !==0&&i.SortCurrency !=="0" &&(t+="SortCurrency=" +i.SortCurrency+"&"), n&&i.PageNumber>=0&&(t+="PageNumber=" +i.PageNumber+"&"), i.IncludeNotForSale !=null&&$("#includeNotForSaleCheckbox").length !=0&&i.IncludeNotForSale != !1&&(t+="IncludeNotForSale=" +i.IncludeNotForSale+"&"), e=($("#legendcontent").css("display") !="none").toString(), e !="false" &&(t+="LegendExpanded=" +e+"&"), t+="Category=" +i.Category, o?Roblox.CatalogShared.LoadCatalogAjax(t, null, u):window.location=t
}

var i, f, o= !1; return {
    ClearGenres:a, pagestate:i, init:l
}
});

;// JS/modules/Pages/CatalogShared.js
typeof Roblox==typeof undefined&&(Roblox= {}),
Roblox.CatalogShared=Roblox.CatalogShared|| {}

,
Roblox.CatalogSharedConstructor=function(n) {
    function u(n, t, u, f, e) {
        if(n&&u&&u.length !==0) {
            i+=1;
            var o=i;

            Roblox.AjaxPageLoadEvent&&Roblox.AjaxPageLoadEvent.SendEvent("legacyCatalog", n),
            u.find(".loading").length<1&&u.find(".right-content").append($('<div class="loading">')),
            u.find(".subcategories [hover='true']").hide(),
            u.css("cursor", "progress"),
            $.ajax({
                method:"GET", params:t, url:n, crossDomain: !0, xhrFields: {
                    withCredentials: !0
                }

            }).done(function(t) {
                if(i==o&&(u.html(t), u.css("cursor", "default"), !f)) {
                    var s=$.Event(r, {
                        url:n, replaceCurrentState:e
                    }); u.trigger(s)
            }

        }).fail(function() {
            if(u.find(".error-message").length<1) {
                var n=$("<div>Catalog temporarily unavailable, please try again later.</div>").addClass("error-message"); u.prepend(n)
            }

            u.find(".loading").remove()
        })
}
}

function f(i) {
    var r,
    u;
    !t&&i.clickTargetID&&(doNotUpdateHistory= !0, i.clickTargetID==="catalog" ?(r=i.url?i.url.split("?")[1]:n.URL.split("?")[1], r&&Roblox.CatalogValues&&Roblox.CatalogValues.CatalogContentsUrl?(u=$("#" +Roblox.CatalogValues.ContainerID), Roblox.CatalogShared.LoadCatalogAjax(Roblox.CatalogValues.CatalogContentsUrl+"?" +r, null, u, !0)):window.location.href=i.url):$("#" +i.clickTargetID).click(), doNotUpdateHistory= !1)
}

function e(i) {
    var u,
    r,
    o,
    s,
    f,
    e;

    Roblox.AdsHelper&&Roblox.AdsHelper.AdRefresher&&Roblox.AdsHelper.AdRefresher.refreshAds(),
    i.url&&(u=i.url.split("?")[1], u&&(r=n.URL.split("?")[0].toLowerCase(), r=r.indexOf("#")===-1?r:r.split("#")[0], r=r.replace("catalog/default.aspx", "catalog/"), r.indexOf("browse.aspx")<0&&r.indexOf("/develop/library")<0&&(o=r.length, s=r.lastIndexOf("/")===o-1, r+=s?"browse.aspx":"/browse.aspx"), f=r+"?" +u, $("#LibraryTabLink").attr("data-query-params", u), e=r.indexOf("/develop/library")>=0?"/develop/library/?" +u:"/catalog/?" +u, GoogleAnalyticsEvents&&GoogleAnalyticsEvents.ViewVirtual(e), t= !0, i.replaceCurrentState?History.replaceState({
                clickTargetID:"catalog", url:f
            }

            , n.title, f):History.pushState({
            clickTargetID:"catalog", url:f
        }

        , n.title, f), t= !1))
}

var r="CatalogLoadedViaAjax",
t= !1,
i=0;

return {
    LoadCatalogAjax: u, CatalogLoadedViaAjaxEventName:r, handleURLChange:f, handleCatalogLoadedViaAjaxEvent:e
}
}

,
Roblox.CatalogShared=Roblox.CatalogSharedConstructor(document);

;// JS/modules/Widgets/AvatarImage.js
// modules/Widgets/AvatarImage.js
Roblox.define("Widgets.AvatarImage", [], function() {
    function i(n) {
        var t = $(n);
        return {
            imageSize: t.attr("data-image-size") || "medium",
            noClick: typeof t.attr("data-no-click") != "undefined",
            noOverlays: typeof t.attr("data-no-overlays") != "undefined",
            userId: t.attr("data-user-id") || 0,
            userOutfitId: t.attr("data-useroutfit-id") || 0,
            name: t.attr("data-useroutfit-name") || ""
        }
    }

    function r(n, t) {
        if (t.bcOverlayUrl != null) {
            var i = $("<img>").attr("src", t.bcOverlayUrl).attr("alt", "Builders Club").css("position", "absolute").css("left", "0").css("bottom", "0").attr("border", 0).addClass("bc-overlay");
            n.after(i)
        }
    }

    function t(u, f) {
        for ($.type(u) !== "array" && (u = [u]); u.length > 0;) {
            for (var o = u.splice(0, 10), s = [], e = 0; e < o.length; e++) s.push(i(o[e]));
            $.getJSON(n.endpoint, {
                params: JSON.stringify(s)
            }, function(n, i) {
                return function(u) {
                    for (var v = [], e, a, h, o = 0; o < u.length; o++)
                        if (e = u[o], e != null) {
                            var c = n[o],
                                s = $(c),
                                l = $("<div>").css("position", "relative");
                            s.html(l), s = l, i[o].noClick || (a = $("<a>").attr("href", e.url), s.append(a), s = a), h = $("<img>").attr("title", e.name).attr("alt", e.name).attr("border", 0), h.load(function(n, t, i, u) {
                                return function() {
                                    n.width(t.width), n.height(t.height), r(i, u)
                                }
                            }(l, c, h, e)), s.append(h), h.attr("src", e.thumbnailUrl), e.thumbnailFinal || v.push(c)
                        } f = f || 1, f < 4 && window.setTimeout(function() {
                        t(v, f + 1)
                    }, f * 2e3)
                }
            }(o, s))
        }
    }

    function u() {
        t($(n.selector + ":empty").toArray())
    }
    var n = {
        selector: ".roblox-avatar-image",
        endpoint: "/avatar-thumbnails?jsoncallback=?"
    };
    return {
        config: n,
        load: t,
        populate: u
    }
});

;// JS/modules/Widgets/DropdownMenu.js
Roblox.define("Widgets.DropdownMenu", [], function() {
        function t(n) {
            $(n).on("click", ".button", function() {
                    var n=$(this), i, t; return n.hasClass("init")||(i=$(this).outerWidth()-parseInt(n.css("border-left-width"))-parseInt(n.css("border-right-width")), n.siblings(".dropdown-list").css("min-width", i), t=n.siblings('.dropdown-list[data-align="right"]').first(), t.css("right", 0), n.addClass("init")), n.hasClass("active")?(n.removeClass("active"), n.siblings(".dropdown-list").hide()):(n.addClass("active"), n.siblings(".dropdown-list").show()), $(document).click(function() {
                            $(".button.init.active").removeClass("active"), $(".dropdown-list").hide()
                        }), !1
                })
        }

        function n() {
            var n=$(".button").not(".init"); n.each(function() {
                    var t=$(this).outerWidth()-parseInt($(this).css("border-left-width"))-parseInt($(this).css("border-right-width")), n; $(this).siblings(".dropdown-list").css("min-width", t), n=$(this).siblings('.dropdown-list[data-align="right"]').first(), n.css("right", 0)

                }), $(".dropdown-list").hide(), n.click(function() {
                    return $(this).hasClass("active")?($(this).removeClass("active"), $(this).siblings(".dropdown-list").hide()):($(this).addClass("active"), $(this).siblings(".dropdown-list").show()), !1

                }), $(document).click(function() {
                    n.removeClass("active"), $(".dropdown-list").hide()
                }), n.addClass("init")
        }

        return {
            InitializeDropdown:n, LazyInitializeDropdown:t
        }
    });

;// JS/modules/Widgets/GroupImage.js
Roblox.define("Widgets.GroupImage", [], function() {
        function r(n) {
            var t=$(n); return {
                imageSize:t.attr("data-image-size")||"medium", noClick:typeof t.attr("data-no-click") !="undefined", groupId:t.attr("data-group-id")||0
            }
        }

        function n(i, u) {
            for($.type(i) !=="array" &&(i=[i]); i.length>0; ) {
                for(var o=i.splice(0, 10), e=[], f=0; f<o.length; f++)e.push(r(o[f])); $.getJSON(t.endpoint, {
                    params:JSON.stringify(e)
                }

                , function(t, i) {
                    return function(r) {
                        for(var a=[], f, h, s, e=0; e<r.length; e++)if(f=r[e], f !=null) {
                            var c=t[e], o=$(c), l=$("<div>").css("position", "relative"); o.html(l), o=l, i[e].noClick||(h=$("<a>").attr("href", f.url), o.append(h), o=h), s=$("<img>").attr("title", f.name).attr("alt", f.name).attr("border", 0), s.load(function(n, t) {
                                    return function() {
                                        n.width(t.width), n.height(t.height)
                                    }
                                }

                                (l, c, s, f)), o.append(s), s.attr("src", f.thumbnailUrl), f.thumbnailFinal||a.push(c)
                        }

                        u=u||1, u<4&&window.setTimeout(function() {
                                n(a, u+1)
                            }

                            , u*2e3)
                    }
                }

                (o, e))
        }
    }

    function i() {
        n($(t.selector+":empty").toArray())
    }

    var t= {
        selector:".roblox-group-image", endpoint:"/group-thumbnails?jsoncallback=?"
    }

    ; return {
        config:t, load:n, populate:i
    }
});

;// JS/modules/Widgets/HierarchicalDropdown.js
Roblox.define("Widgets.HierarchicalDropdown", [], function() {
        function n(n) {
            var t=n.width(); n.find("li").each(function(n, i) {
                    i=$(i), i.outerWidth()>t&&(t=i.outerWidth())

                }), n.find("li").each(function(n, i) {
                    i=$(i), i.width()<t&&i.width(t)
                })
        }

        function t() {
            var i=0, r=0, t=$(".roblox-hierarchicaldropdown"), f=t.find("li"), u=t.find("li ul"), e=t.find("li ul[hover=true]"); u.mouseover(function() {
                    $(this).attr("hover", "true")

                }), u.mouseout(function() {
                    $(this).attr("hover", "false")

                }), f.mouseover(function() {
                    var i=$(this).data("delay"), f; i !="ignore" &&e.length==0&&($(this).attr("hover", "true"), i !="never" &&(r==1||i=="always")?window.setTimeout(function() {
                                if(e.length==0) {
                                    var i=t.find("li[hover=true] ul"); u.hide(), i.length !=0&&(i.show(), n(i))
                                }
                            }

                            , 1e3):(u.hide(), f=$(this).find("ul"), f.show(), n(f)))

                }), f.mouseout(function() {
                    $(this).removeAttr("hover")

                }), t.mouseleave(function() {
                    window.setTimeout(function() {
                            u.hide()
                        }

                        , 100), i=0, r=0

                }), t.mousemove(function(n) {
                    var t=i; i=n.pageX, (t==i||t==0)&&(r=0), r=t<i?1:-1
                })
        }

        return {
            init:t
        }
    });

;// JS/modules/Widgets/ItemImage.js
Roblox.define("Widgets.ItemImage", [], function() {
        function i(n) {
            var t=$(n); return {
                imageSize:t.attr("data-image-size")||"large", noClick:typeof t.attr("data-no-click") !="undefined", noOverlays:typeof t.attr("data-no-overlays") !="undefined", assetId:t.attr("data-item-id")||0
            }
        }

        function t(r, u) {
            for($.type(r) !=="array" &&(r=[r]); r.length>0; ) {
                for(var e=r.splice(0, 10), o=[], f=0; f<e.length; f++)o.push(i(e[f])); $.getJSON(n.endpoint, {
                    params:JSON.stringify(o)
                }

                , function(n, i) {
                    return function(r) {
                        for(var a=[], f, l, s, e=0; e<r.length; e++)if(f=r[e], f !=null) {
                            var h=n[e], o=$(h), c=$("<div>").css("position", "relative").css("overflow", "hidden"); o.html(c), o=c, i[e].noClick||(l=$("<a>").attr("href", f.url), o.append(l), o=l), s=$("<img>").attr("title", f.name).attr("alt", f.name).attr("border", 0).addClass("original-image modal-thumb"), s.load(function(n, t) {
                                    return function() {
                                        n.width(t.width), n.height(t.height)
                                    }
                                }

                                (c, h, s, f)), o.append(s), s.attr("src", f.thumbnailUrl), f.thumbnailFinal||a.push(h)
                        }

                        u=u||1, u<4&&window.setTimeout(function() {
                                t(a, u+1)
                            }

                            , u*2e3)
                    }
                }

                (e, o))
        }
    }

    function r() {
        t($(n.selector+":empty").toArray())
    }

    var n= {
        selector:".roblox-item-image", endpoint:"/item-thumbnails?jsoncallback=?"
    }

    ; return {
        config:n, load:t, populate:r
    }
});

;// JS/modules/Widgets/PlaceImage.js
Roblox.define("Widgets.PlaceImage", [], function() {
        function i(n) {
            var t=$(n); return {
                imageSize:t.attr("data-image-size")||"large", noClick:typeof t.attr("data-no-click") !="undefined", noOverlays:typeof t.attr("data-no-overlays") !="undefined", placeId:t.attr("data-place-id")||0
            }
        }

        function r(n, t) {
            if(t.bcOverlayUrl !=null) {
                var i=$("<img>").attr("src", t.bcOverlayUrl).attr("alt", "Builders Club").css("position", "absolute").css("left", "0").css("bottom", "0").attr("border", 0); n.after(i)
            }
        }

        function t(u, f) {
            for($.type(u) !=="array" &&(u=[u]); u.length>0; ) {
                for(var o=u.splice(0, 10), s=[], e=0; e<o.length; e++)s.push(i(o[e])); $.getJSON(n.endpoint, {
                    params:JSON.stringify(s)
                }

                , function(n, i) {
                    return function(u) {
                        var v=[], o, a, h; for(e=0; e<u.length; e++)if(o=u[e], o !=null) {
                            var c=n[e], s=$(c), l=$("<div>").css("position", "relative"); s.html(l), s=l, i[e].noClick||(a=$("<a>").attr("href", o.url), s.append(a), s=a), h=$("<img>").attr("title", o.name).attr("alt", o.name).attr("border", 0), h.load(function(n, t, i, u) {
                                    return function() {
                                        n.width(t.width), n.height(t.height), r(i, u)
                                    }
                                }

                                (l, c, h, o)), s.append(h), h.attr("src", o.thumbnailUrl), o.thumbnailFinal||v.push(c)
                        }

                        f=f||1, f<4&&window.setTimeout(function() {
                                t(v, f+1)
                            }

                            , f*2e3)
                    }
                }

                (o, s))
        }
    }

    function u() {
        t($(n.selector+":empty").toArray())
    }

    var n= {
        selector:".roblox-place-image", endpoint:"/place-thumbnails?jsoncallback=?"
    }

    ; return {
        config:n, load:t, populate:u
    }
});

;// JS/modules/Widgets/SurveyModal.js
typeof Roblox=="undefined" &&(Roblox= {}),
typeof Roblox.SurveyModal=="undefined" &&(Roblox.SurveyModal=function() {
        function t() {
            $('[data-modal-handle="survey"]').find("iframe").show(), $('[data-modal-handle="survey"]').modal(i)
        }

        function n() {
            $.modal.close(), $('[data-modal-handle="survey"]').find("iframe").hide()
        }

        var i= {
            overlayClose: !0, escClose: !0, opacity:80, overlayCss: {
                backgroundColor:"#000"
            }

            , onClose:n
        }

        ; return {
            open:t
        }
    }

    ());

;// JS/iFrameLogin.js
// iFrameLogin.js
typeof Roblox == "undefined" && (Roblox = {}), Roblox.iFrameLogin = new function() {
    function e() {
        var o = $(document.body).data("captchaon"),
            s = !1,
            l = !0,
            v = function(n) {
                var t = $(document.body).data("parent-url");
                $.postMessage("resize," + n, t, parent)
            },
            vt = function() {
                try {
                    var de = document.documentElement;
                    var calc = Math.max(
                        $("#LoginForm").outerHeight(true) || 0,
                        document.body ? document.body.scrollHeight : 0,
                        document.body ? document.body.offsetHeight : 0,
                        de ? de.clientHeight : 0,
                        de ? de.scrollHeight : 0,
                        de ? de.offsetHeight : 0
                    );
                    var h = calc;
                    if (h && h > 0) {
                        v(h + "px");
                    }
                } catch (ex) {
                    // ignore
                }
            },
            h = null;
        // initial resize to content and schedule a few follow-ups
        vt();
        setTimeout(vt, 0);
        setTimeout(vt, 200);
        setTimeout(vt, 500);
        var y = function() {
                var n = $(document.body).data("parent-url");
                n.indexOf("#") != -1 && (n = n.split("#")[0]), n += n.indexOf("?") == -1 ? "?nl=true" : "&nl=true", window.parent.location = n
            },
            c = function(n) {
                if (n) {
                    $("#LoggingInStatus").addClass("active").show();
                } else {
                    $("#LoggingInStatus").removeClass("active").hide();
                }
            },
            w = function() {
                var n = !1,
                    t = [$("#Password"), $("#UserName")];
                return o && t.push($("#recaptcha_response_field")), jQuery.each(t, function() {
                    var t = $(this);
                    t.val() == "" ? (e(t, !0), n = !0) : e(t, !1)
                }), n
            },
            e = function(n, t) {
                s = !1, c(!1), t ? n.css({
                    "background-color": "#FDD"
                }) : n.css({
                    "background-color": "white"
                })
            },
            p = function(n, t, i, r) {
                var u = Roblox.iFrameLogin.Resources.requestCodeUnauthenticatedPath,
                    f = {
                        username: n,
                        password: t,
                        actionType: Roblox.TwoStepVerificationModal.ActionTypes.SignIn
                    };
                $.ajax({
                    type: "POST",
                    url: u,
                    data: f,
                    crossDomain: !0,
                    xhrFields: {
                        withCredentials: !0
                    },
                    success: i,
                    error: r
                })
            },
            b = function() {
                var n = function() {
                        var n, t;
                        $("#TwoStepVerificationNewCodeButton").hide(), $("#TwoStepVerificationSubmitButton").show(), n = $("#TwoStepVerificationMessage"), n.text(Roblox.iFrameLogin.Resources.enterTwoStepCodeMessage), t = $("#TwoStepVerificationCodeInput"), t.css("background-color", "white")
                    },
                    t = function(n) {
                        var i = $("#TwoStepVerificationNewCodeButton"),
                            t = null,
                            r;
                        switch (n.status) {
                            case 403:
                                r = JSON.parse(n.responseText), r.message === "Flooded" ? (t = Roblox.iFrameLogin.Resources.floodedTwoStepMessage, i.addClass("disabled")) : r.message === "VerifyEmail" ? (t = Roblox.iFrameLogin.Resources.verifyEmailMessage, i.addClass("disabled")) : (t = Roblox.iFrameLogin.Resources.unknownErrorText, i.addClass("disabled"));
                                break;
                            default:
                                t = Roblox.iFrameLogin.Resources.unknownErrorText, i.addClass("disabled")
                        }
                        $("#TwoStepVerificationMessage").text(t)
                    },
                    i = $("#UserName").val();
                p(i, h, n, t)
            },
            k = function() {
                var n = function() {
                        $("#Password").val(h), a()
                    },
                    t = function(n) {
                        var t, r;
                        $("#TwoStepVerificationSubmitButton").hide(), t = $("#TwoStepVerificationNewCodeButton"), t.show();
                        var u = $("#TwoStepVerificationCodeInput"),
                            i = null,
                            f = !1;
                        switch (n.status) {
                            case 403:
                                r = JSON.parse(n.responseText), r.message === "Flooded" ? (i = Roblox.iFrameLogin.Resources.floodedTwoStepMessage, t.addClass("disabled")) : r.message === "VerifyEmail" ? (i = Roblox.iFrameLogin.Resources.verifyEmailMessage, t.addClass("disabled")) : r.message === "InvalidCode" ? (i = Roblox.iFrameLogin.Resources.invalidCodeMessage, f = !0) : (i = Roblox.iFrameLogin.Resources.unknownErrorText, t.addClass("disabled"));
                                break;
                            default:
                                i = Roblox.iFrameLogin.Resources.unknownErrorText, t.addClass("disabled")
                        }
                        u.val(""), f && u.css("background-color", "#FDD"), $("#TwoStepVerificationMessage").text(i)
                    },
                    i = Roblox.iFrameLogin.Resources.verifyCodeUnauthenticatedPath,
                    r = {
                        username: $("#UserName").val(),
                        password: h,
                        actionType: Roblox.TwoStepVerificationModal.ActionTypes.SignIn,
                        code: $("#TwoStepVerificationCodeInput").val()
                    };
                $.ajax({
                    type: "POST",
                    url: i,
                    data: r,
                    crossDomain: !0,
                    xhrFields: {
                        withCredentials: !0
                    },
                    success: n,
                    error: t
                })
            },
            a = function() {
                var v, b, d;
                if (w()) return !1;
                if (l) return e($("#UserName"), !0), !1;
                s = !0, c(!0);
                var nt = $("#UserName"),
                    tt = $("#Password"),
                    a = nt.val(),
                    k = tt.val();
                if (h = k, v = "", b = "", o && (v = $("#recaptcha_challenge_field").val(), b = $("#recaptcha_response_field").val(), v == "" || b == "")) return e($("#recaptcha_response_field"), !0), !1;
                if (o && $("#Captcha_upBadCaptcha").text(""), Roblox.iFrameLogin.Resources.useSignOnApi) {
                    var it = {
                            username: a,
                            password: k,
                            recaptcha_challenge_field: v,
                            recaptcha_response_field: b
                        },
                        d = function() {
                            y()
                        },
                        g = function(n) {
                            var t, i, r;
                            if (n.status === 403) {
                                t = JSON.parse(n.responseText);
                                switch (t.message) {
                                    case "Credentials":
                                        e($("#Password"), !0), $("#NotAMemberLink").hide(), $("#ForgotPasswordLink").show();
                                        break;
                                    case "CaptchaIncorrect":
                                        e($("#Password"), !1), $("#Captcha_upBadCaptcha").show(), $("#Captcha_upBadCaptcha").css("color", "red"), $("#Captcha_upBadCaptcha").text(Roblox.iFrameLogin.Resources.invalidCaptchaEntry);
                                        break;
                                    case "CaptchaMissing":
                                        e($("#Password"), !1), $("#Captcha_upBadCaptcha").show(), $("#Captcha_upBadCaptcha").css("color", "red");
                                        break;
                                    case "TwoStepVerification":
                                        i = function() {
                                            $("#credentials-section").hide(), $("#two-step-verification-section").show()
                                        }, r = function(n) {
                                            var t, i;
                                            $("#credentials-section").hide(), $("#two-step-verification-section").show(), t = null;
                                            switch (n.status) {
                                                case 403:
                                                    i = JSON.parse(n.responseText), t = i.message === "Flooded" ? Roblox.iFrameLogin.Resources.floodedTwoStepMessage : i.message === "VerifyEmail" ? Roblox.iFrameLogin.Resources.verifyEmailMessage : Roblox.iFrameLogin.Resources.unknownErrorText;
                                                    break;
                                                default:
                                                    t = Roblox.iFrameLogin.Resources.unknownErrorText
                                            }
                                            $("#TwoStepVerificationMessage").text(t), $("#TwoStepVerificationSubmitButton").addClass("disabled")
                                        }, p(a, k, i, r)
                                }
                            }
                            return o && Recaptcha.reload("t"), $("#Password").val(""), $("#Password").focus(), s = !1, c(!1), !1
                        };
                    $.ajax({
                        type: "POST",
                        url: Roblox.iFrameLogin.Resources.signOnApiPath,
                        data: it,
                        crossDomain: !0,
                        xhrFields: {
                            withCredentials: !0
                        },
                        success: d,
                        error: g
                    })
                } else d = g = function(h) {
                    if (h.IsValid) y();
                    else return h.ErrorCode.indexOf(f) !== -1 ? (window.parent.location = "/Login/ResetPasswordRequest.aspx?needsReset=1", !1) : h.ErrorCode.indexOf(r) != -1 ? (a != "" && window.location.href.indexOf("username") == -1 ? window.location.href = window.location.href + "&username=" + a : window.location.reload(), !1) : (h.ErrorCode.indexOf(t) != -1 && (window.parent.location = "/login/twofactorauth?username=" + encodeURIComponent(a)), h.ErrorCode.indexOf(u) != -1 ? (e($("#Password"), !0), $("#NotAMemberLink").hide(), $("#ForgotPasswordLink").show()) : h.ErrorCode.indexOf(n) != -1 ? $("#ErrorMessage").text(h.Message) : h.ErrorCode.indexOf(i) != -1 ? $("#ErrorMessage").text(h.Message) : (e($("#Password"), !1), $("#Captcha_upBadCaptcha").show(), $("#Captcha_upBadCaptcha").css("color", "red"), h.Message == "incorrect-captcha-sol" ? $("#Captcha_upBadCaptcha").text(Roblox.iFrameLogin.Resources.invalidCaptchaEntry) : $("#Captcha_upBadCaptcha").text(h.Message)), o && Recaptcha.reload("t"), $("#Password").val(""), $("#Password").focus(), s = !1, c(!1), !1)
                }, Roblox.Website.Services.Secure.LoginService.ValidateLogin(a, k, o, v, b, d, g)
            },
            d = function() {
                var n = $("#UserName").val(),
                    t = onError = function(n) {
                        e($("#UserName"), !n.success), l = !n.success, n.success || ($("#NotAMemberLink").show(), $("#ForgotPasswordLink").show())
                    };
                n != "" && $.ajax({
                    type: "GET",
                    url: "/UserCheck/doesusernameexist?username=" + n,
                    success: t,
                    error: onError
                })
            };
        $("#LoginButton").click(function() {
            a()
        }), $("#TwoStepVerificationSubmitButton").click(function() {
            k()
        }), $("#TwoStepVerificationCancelButton").click(function() {
            var n, t;
            $("#TwoStepVerificationNewCodeButton").hide(), n = $("#TwoStepVerificationSubmitButton"), n.show(), n.removeClass("disabled"), $("#TwoStepVerificationMessage").text(Roblox.iFrameLogin.Resources.enterTwoStepCodeMessage), t = $("#TwoStepVerificationCodeInput"), t.val(""), t.css("background-color", "white"), $("#two-step-verification-section").hide(), $("#credentials-section").show()
        }), $("#TwoStepVerificationNewCodeButton").click(function() {
            b()
        }), $("#UserName").blur(function() {
            d()
        }), $(document).keydown(function(n) {
            if (n.which == 13 && !s) return a(), !1
        }), $(function() {
            var n = 1;
            $("input,select").each(function() {
                if (this.type != "hidden") {
                    var t = $(this);
                    t.attr("tabindex", n), n++
                }
            });
            vt();
        }), $(function() {
            $("#UserName").val() != "" || $("#UserName").val() != undefined, l = !1
        }), $(function() {
            $("#CaptchaContainer").css({
                "margin-left": "0",
                "margin-top": "8px",
                "margin-bottom": "5px",
                width: "none"
            });
            vt();
        }), $(window).resize(function() {
            vt();
        });
        // Observe DOM changes to adjust height dynamically
        if (window.MutationObserver) {
            try {
                var observer = new MutationObserver(function() { vt(); });
                observer.observe(document.body, { attributes: true, childList: true, subtree: true });
            } catch (eobs) { /* ignore */ }
        }
    }

    var o = "1",
        s = "2",
        n = "3",
        t = "4",
        i = "5",
        r = "6",
        u = "7",
        h = "8",
        f = "10";
    return {
        init: e
    }
};

;// JS/viewapp/common/providers/languageResourceProvider.js
// ~/viewapp/common/providers/languageResourceProvider.js
angular.module("robloxApp").provider("languageResource", function() {
    var i = {},
        n = {},
        t, r = new Roblox.Intl,
        u = !1,
        f = function(n, t) {
            var u = i[n];
            return u ? t && Object.keys(t).length > 0 && (u = r.f(u, t)) : (console.warn("Language key '" + n + "' not found. Please check for any typo or a missing key."), u = ""), u
        },
        e = function(i, r, u) {
            if (u && typeof u == "string") {
                if (n[u]) return n[u].get(i, r);
                throw new Error("Provided NameSpace '" + u + "' is not found or is not set");
            }
            return n[t].get(i, r)
        };
    this.setLanguageKeysFromFile = function(n) {
        n && typeof n == "object" && !Array.isArray(n) && angular.extend(i, n)
    }, this.setTranslationResources = function(i) {
        angular.forEach(i, function(i) {
            i instanceof Roblox.TranslationResource && (n[i.nameSpace] = i, u = !0, t || (t = i.nameSpace))
        })
    }, this.$get = ["$log", function() {
        return {
            get: u ? e : f,
            intl: r,
            setLanguageKeysFromFile: function(n) {
                n && typeof n == "object" && !Array.isArray(n) && angular.extend(i, n)
            }
        }
    }]
});

