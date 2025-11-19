// angular/ui-bootstrap-custom-tpls-2.5.0.js
angular.module("ui.bootstrap", ["ui.bootstrap.tpls", "ui.bootstrap.carousel", "ui.bootstrap.position", "ui.bootstrap.dropdown", "ui.bootstrap.multiMap", "ui.bootstrap.stackedMap", "ui.bootstrap.modal", "ui.bootstrap.tooltip", "ui.bootstrap.popover"]), angular.module("ui.bootstrap.tpls", ["uib/template/carousel/carousel.html", "uib/template/carousel/slide.html", "uib/template/modal/window.html", "uib/template/tooltip/tooltip-html-popup.html", "uib/template/tooltip/tooltip-popup.html", "uib/template/tooltip/tooltip-template-popup.html", "uib/template/popover/popover-html.html", "uib/template/popover/popover-template.html", "uib/template/popover/popover.html"]), angular.module("ui.bootstrap.carousel", []).controller("UibCarouselController", ["$scope", "$element", "$interval", "$timeout", "$animate", function(n, t, i, r, u) {
    function s(n) {
        for (var t = 0; t < f.length; t++) f[t].slide.active = t === n
    }

    function w(i, r, h) {
        if (!y) {
            if (angular.extend(i, {
                    direction: h
                }), angular.extend(f[o].slide || {}, {
                    direction: h
                }), u.enabled(t) && !n.$currentTransition && f[r].element && e.slides.length > 1) {
                f[r].element.data(p, i.direction);
                var c = e.getCurrentIndex();
                angular.isNumber(c) && f[c].element && f[c].element.data(p, i.direction), n.$currentTransition = !0;
                u.on("addClass", f[r].element, function(t, i) {
                    i === "close" && (n.$currentTransition = null, u.off("addClass", t))
                })
            }
            n.active = i.index, o = i.index, s(r), v()
        }
    }

    function l(n) {
        for (var t = 0; t < f.length; t++)
            if (f[t].slide === n) return t
    }

    function a() {
        c && (i.cancel(c), c = null)
    }

    function b(t) {
        t.length || (n.$currentTransition = null)
    }

    function v() {
        a();
        var t = +n.interval;
        !isNaN(t) && t > 0 && (c = i(k, t))
    }

    function k() {
        var t = +n.interval;
        h && !isNaN(t) && t > 0 && f.length ? n.next() : n.pause()
    }
    var e = this,
        f = e.slides = n.slides = [],
        p = "uib-slideDirection",
        o = n.active,
        c, h, y = !1;
    t.addClass("carousel"), e.addSlide = function(t, i) {
        f.push({
            slide: t,
            element: i
        }), f.sort(function(n, t) {
            return +n.slide.index - +t.slide.index
        }), t.index !== n.active && (f.length !== 1 || angular.isNumber(n.active)) || (n.$currentTransition && (n.$currentTransition = null), o = t.index, n.active = t.index, s(o), e.select(f[l(t)]), f.length === 1 && n.play())
    }, e.getCurrentIndex = function() {
        for (var n = 0; n < f.length; n++)
            if (f[n].slide.index === o) return n
    }, e.next = n.next = function() {
        var t = (e.getCurrentIndex() + 1) % f.length;
        if (t === 0 && n.noWrap()) {
            n.pause();
            return
        }
        return e.select(f[t], "next")
    }, e.prev = n.prev = function() {
        var t = e.getCurrentIndex() - 1 < 0 ? f.length - 1 : e.getCurrentIndex() - 1;
        if (n.noWrap() && t === f.length - 1) {
            n.pause();
            return
        }
        return e.select(f[t], "prev")
    }, e.removeSlide = function(t) {
        var i = l(t);
        f.splice(i, 1), f.length > 0 && o === i ? i >= f.length ? (o = f.length - 1, n.active = o, s(o), e.select(f[f.length - 1])) : (o = i, n.active = o, s(o), e.select(f[i])) : o > i && (o--, n.active = o), f.length === 0 && (o = null, n.active = null)
    }, e.select = n.select = function(t, i) {
        var r = l(t.slide);
        i === undefined && (i = r > e.getCurrentIndex() ? "next" : "prev"), t.slide.index === o || n.$currentTransition || w(t.slide, r, i)
    }, n.indexOfSlide = function(n) {
        return +n.slide.index
    }, n.isActive = function(t) {
        return n.active === t.slide.index
    }, n.isPrevDisabled = function() {
        return n.active === 0 && n.noWrap()
    }, n.isNextDisabled = function() {
        return n.active === f.length - 1 && n.noWrap()
    }, n.pause = function() {
        n.noPause || (h = !1, a())
    }, n.play = function() {
        h || (h = !0, v())
    };
    t.on("mouseenter", n.pause);
    t.on("mouseleave", n.play);
    n.$on("$destroy", function() {
        y = !0, a()
    }), n.$watch("noTransition", function(n) {
        u.enabled(t, !n)
    }), n.$watch("interval", v), n.$watchCollection("slides", b), n.$watch("active", function(n) {
        var t, i;
        if (angular.isNumber(n) && o !== n) {
            for (t = 0; t < f.length; t++)
                if (f[t].slide.index === n) {
                    n = t;
                    break
                } i = f[n], i && (s(n), e.select(f[n]), o = n)
        }
    })
}]).directive("uibCarousel", function() {
    return {
        transclude: !0,
        controller: "UibCarouselController",
        controllerAs: "carousel",
        restrict: "A",
        templateUrl: function(n, t) {
            return t.templateUrl || "uib/template/carousel/carousel.html"
        },
        scope: {
            active: "=",
            interval: "=",
            noTransition: "=",
            noPause: "=",
            noWrap: "&"
        }
    }
}).directive("uibSlide", ["$animate", function(n) {
    return {
        require: "^uibCarousel",
        restrict: "A",
        transclude: !0,
        templateUrl: function(n, t) {
            return t.templateUrl || "uib/template/carousel/slide.html"
        },
        scope: {
            actual: "=?",
            index: "=?"
        },
        link: function(t, i, r, u) {
            i.addClass("item"), u.addSlide(t, i), t.$on("$destroy", function() {
                u.removeSlide(t)
            }), t.$watch("active", function(t) {
                n[t ? "addClass" : "removeClass"](i, "active")
            })
        }
    }
}]).animation(".item", ["$animateCss", function(n) {
    function i(n, t, i) {
        n.removeClass(t), i && i()
    }
    var t = "uib-slideDirection";
    return {
        beforeAddClass: function(r, u, f) {
            if (u === "active") {
                var s = !1,
                    e = r.data(t),
                    o = e === "next" ? "left" : "right",
                    h = i.bind(this, r, o + " " + e, f);
                return r.addClass(e), n(r, {
                        addClass: o
                    }).start().done(h),
                    function() {
                        s = !0
                    }
            }
            f()
        },
        beforeRemoveClass: function(r, u, f) {
            if (u === "active") {
                var o = !1,
                    s = r.data(t),
                    e = s === "next" ? "left" : "right",
                    h = i.bind(this, r, e, f);
                return n(r, {
                        addClass: e
                    }).start().done(h),
                    function() {
                        o = !0
                    }
            }
            f()
        }
    }
}]), angular.module("ui.bootstrap.position", []).factory("$uibPosition", ["$document", "$window", function(n, t) {
    var r, u, f = {
            normal: /(auto|scroll)/,
            hidden: /(auto|scroll|hidden)/
        },
        i = {
            auto: /\s?auto?\s?/i,
            primary: /^(top|bottom|left|right)$/,
            secondary: /^(top|bottom|left|right|center)$/,
            vertical: /^(top|bottom)$/
        },
        e = /(HTML|BODY)/;
    return {
        getRawNode: function(n) {
            return n.nodeName ? n : n[0] || n
        },
        parseStyle: function(n) {
            return n = parseFloat(n), isFinite(n) ? n : 0
        },
        offsetParent: function(i) {
            function u(n) {
                return (t.getComputedStyle(n).position || "static") === "static"
            }
            i = this.getRawNode(i);
            for (var r = i.offsetParent || n[0].documentElement; r && r !== n[0].documentElement && u(r);) r = r.offsetParent;
            return r || n[0].documentElement
        },
        scrollbarWidth: function(i) {
            var e, f;
            return i ? (angular.isUndefined(u) && (e = n.find("body"), e.addClass("uib-position-body-scrollbar-measure"), u = t.innerWidth - e[0].clientWidth, u = isFinite(u) ? u : 0, e.removeClass("uib-position-body-scrollbar-measure")), u) : (angular.isUndefined(r) && (f = angular.element('<div class="uib-position-scrollbar-measure"></div>'), n.find("body").append(f), r = f[0].offsetWidth - f[0].clientWidth, r = isFinite(r) ? r : 0, f.remove()), r)
        },
        scrollbarPadding: function(n) {
            n = this.getRawNode(n);
            var u = t.getComputedStyle(n),
                f = this.parseStyle(u.paddingRight),
                o = this.parseStyle(u.paddingBottom),
                i = this.scrollParent(n, !1, !0),
                r = this.scrollbarWidth(e.test(i.tagName));
            return {
                scrollbarWidth: r,
                widthOverflow: i.scrollWidth > i.clientWidth,
                right: f + r,
                originalRight: f,
                heightOverflow: i.scrollHeight > i.clientHeight,
                bottom: o + r,
                originalBottom: o
            }
        },
        isScrollable: function(n, i) {
            n = this.getRawNode(n);
            var u = i ? f.hidden : f.normal,
                r = t.getComputedStyle(n);
            return u.test(r.overflow + r.overflowY + r.overflowX)
        },
        scrollParent: function(i, r, u) {
            var c, e, s;
            i = this.getRawNode(i);
            var l = r ? f.hidden : f.normal,
                h = n[0].documentElement,
                o = t.getComputedStyle(i);
            if (u && l.test(o.overflow + o.overflowY + o.overflowX)) return i;
            if (c = o.position === "absolute", e = i.parentElement || h, e === h || o.position === "fixed") return h;
            while (e.parentElement && e !== h) {
                if (s = t.getComputedStyle(e), c && s.position !== "static" && (c = !1), !c && l.test(s.overflow + s.overflowY + s.overflowX)) break;
                e = e.parentElement
            }
            return e
        },
        position: function(i, r) {
            var u, o, f, e;
            return i = this.getRawNode(i), u = this.offset(i), r && (o = t.getComputedStyle(i), u.top -= this.parseStyle(o.marginTop), u.left -= this.parseStyle(o.marginLeft)), f = this.offsetParent(i), e = {
                top: 0,
                left: 0
            }, f !== n[0].documentElement && (e = this.offset(f), e.top += f.clientTop - f.scrollTop, e.left += f.clientLeft - f.scrollLeft), {
                width: Math.round(angular.isNumber(u.width) ? u.width : i.offsetWidth),
                height: Math.round(angular.isNumber(u.height) ? u.height : i.offsetHeight),
                top: Math.round(u.top - e.top),
                left: Math.round(u.left - e.left)
            }
        },
        offset: function(i) {
            i = this.getRawNode(i);
            var r = i.getBoundingClientRect();
            return {
                width: Math.round(angular.isNumber(r.width) ? r.width : i.offsetWidth),
                height: Math.round(angular.isNumber(r.height) ? r.height : i.offsetHeight),
                top: Math.round(r.top + (t.pageYOffset || n[0].documentElement.scrollTop)),
                left: Math.round(r.left + (t.pageXOffset || n[0].documentElement.scrollLeft))
            }
        },
        viewportOffset: function(i, r, u) {
            var o;
            i = this.getRawNode(i), u = u !== !1 ? !0 : !1;
            var s = i.getBoundingClientRect(),
                f = {
                    top: 0,
                    left: 0,
                    bottom: 0,
                    right: 0
                },
                e = r ? n[0].documentElement : this.scrollParent(i),
                h = e.getBoundingClientRect();
            return f.top = h.top + e.clientTop, f.left = h.left + e.clientLeft, e === n[0].documentElement && (f.top += t.pageYOffset, f.left += t.pageXOffset), f.bottom = f.top + e.clientHeight, f.right = f.left + e.clientWidth, u && (o = t.getComputedStyle(e), f.top += this.parseStyle(o.paddingTop), f.bottom -= this.parseStyle(o.paddingBottom), f.left += this.parseStyle(o.paddingLeft), f.right -= this.parseStyle(o.paddingRight)), {
                top: Math.round(s.top - f.top),
                bottom: Math.round(f.bottom - s.bottom),
                left: Math.round(s.left - f.left),
                right: Math.round(f.right - s.right)
            }
        },
        parsePlacement: function(n) {
            var t = i.auto.test(n);
            return t && (n = n.replace(i.auto, "")), n = n.split("-"), n[0] = n[0] || "top", i.primary.test(n[0]) || (n[0] = "top"), n[1] = n[1] || "center", i.secondary.test(n[1]) || (n[1] = "center"), n[2] = t ? !0 : !1, n
        },
        positionElements: function(n, r, u, f) {
            var c, l, e, h, v, y;
            if (n = this.getRawNode(n), r = this.getRawNode(r), c = angular.isDefined(r.offsetWidth) ? r.offsetWidth : r.prop("offsetWidth"), l = angular.isDefined(r.offsetHeight) ? r.offsetHeight : r.prop("offsetHeight"), u = this.parsePlacement(u), e = f ? this.offset(n) : this.position(n), h = {
                    top: 0,
                    left: 0,
                    placement: ""
                }, u[2]) {
                var o = this.viewportOffset(n, f),
                    a = t.getComputedStyle(r),
                    s = {
                        width: c + Math.round(Math.abs(this.parseStyle(a.marginLeft) + this.parseStyle(a.marginRight))),
                        height: l + Math.round(Math.abs(this.parseStyle(a.marginTop) + this.parseStyle(a.marginBottom)))
                    };
                u[0] = u[0] === "top" && s.height > o.top && s.height <= o.bottom ? "bottom" : u[0] === "bottom" && s.height > o.bottom && s.height <= o.top ? "top" : u[0] === "left" && s.width > o.left && s.width <= o.right ? "right" : u[0] === "right" && s.width > o.right && s.width <= o.left ? "left" : u[0], u[1] = u[1] === "top" && s.height - e.height > o.bottom && s.height - e.height <= o.top ? "bottom" : u[1] === "bottom" && s.height - e.height > o.top && s.height - e.height <= o.bottom ? "top" : u[1] === "left" && s.width - e.width > o.right && s.width - e.width <= o.left ? "right" : u[1] === "right" && s.width - e.width > o.left && s.width - e.width <= o.right ? "left" : u[1], u[1] === "center" && (i.vertical.test(u[0]) ? (v = e.width / 2 - c / 2, o.left + v < 0 && s.width - e.width <= o.right ? u[1] = "left" : o.right + v < 0 && s.width - e.width <= o.left && (u[1] = "right")) : (y = e.height / 2 - s.height / 2, o.top + y < 0 && s.height - e.height <= o.bottom ? u[1] = "top" : o.bottom + y < 0 && s.height - e.height <= o.top && (u[1] = "bottom")))
            }
            switch (u[0]) {
                case "top":
                    h.top = e.top - l;
                    break;
                case "bottom":
                    h.top = e.top + e.height;
                    break;
                case "left":
                    h.left = e.left - c;
                    break;
                case "right":
                    h.left = e.left + e.width
            }
            switch (u[1]) {
                case "top":
                    h.top = e.top;
                    break;
                case "bottom":
                    h.top = e.top + e.height - l;
                    break;
                case "left":
                    h.left = e.left;
                    break;
                case "right":
                    h.left = e.left + e.width - c;
                    break;
                case "center":
                    i.vertical.test(u[0]) ? h.left = e.left + e.width / 2 - c / 2 : h.top = e.top + e.height / 2 - l / 2
            }
            return h.top = Math.round(h.top), h.left = Math.round(h.left), h.placement = u[1] === "center" ? u[0] : u[0] + "-" + u[1], h
        },
        adjustTop: function(n, t, i, r) {
            if (n.indexOf("top") !== -1 && i !== r) return {
                top: t.top - r + "px"
            }
        },
        positionArrow: function(n, r) {
            var o, f, e, u, c;
            if ((n = this.getRawNode(n), o = n.querySelector(".tooltip-inner, .popover-inner"), o) && (f = angular.element(o).hasClass("tooltip-inner"), e = f ? n.querySelector(".tooltip-arrow") : n.querySelector(".arrow"), e)) {
                if (u = {
                        top: "",
                        bottom: "",
                        left: "",
                        right: ""
                    }, r = this.parsePlacement(r), r[1] === "center") {
                    angular.element(e).css(u);
                    return
                }
                var l = "border-" + r[0] + "-width",
                    s = t.getComputedStyle(e)[l],
                    h = "border-";
                h += i.vertical.test(r[0]) ? r[0] + "-" + r[1] : r[1] + "-" + r[0], h += "-radius", c = t.getComputedStyle(f ? o : n)[h];
                switch (r[0]) {
                    case "top":
                        u.bottom = f ? "0" : "-" + s;
                        break;
                    case "bottom":
                        u.top = f ? "0" : "-" + s;
                        break;
                    case "left":
                        u.right = f ? "0" : "-" + s;
                        break;
                    case "right":
                        u.left = f ? "0" : "-" + s
                }
                u[r[1]] = c, angular.element(e).css(u)
            }
        }
    }
}]), angular.module("ui.bootstrap.dropdown", ["ui.bootstrap.multiMap", "ui.bootstrap.position"]).constant("uibDropdownConfig", {
    appendToOpenClass: "uib-dropdown-open",
    openClass: "open"
}).service("uibDropdownService", ["$document", "$rootScope", "$$multiMap", function(n, t, i) {
    var r = null,
        u = i.createNew(),
        f;
    this.isOnlyOpen = function(n, t) {
        var i = u.get(t),
            r;
        return i && (r = i.reduce(function(t, i) {
            return i.scope === n ? i : t
        }, {}), r) ? i.length === 1 : !1
    }, this.open = function(t, i, e) {
        var o, s;
        if (!r) n.on("click", f);
        (r && r !== t && (r.isOpen = !1), r = t, e) && (o = u.get(e), o ? (s = o.map(function(n) {
            return n.scope
        }), s.indexOf(t) === -1 && u.put(e, {
            scope: t
        })) : u.put(e, {
            scope: t
        }))
    }, this.close = function(t, i, e) {
        var o, s;
        (r === t && (n.off("click", f), n.off("keydown", this.keybindFilter), r = null), e) && (o = u.get(e), o && (s = o.reduce(function(n, i) {
            return i.scope === t ? i : n
        }, {}), s && u.remove(e, s)))
    }, f = function(n) {
        var i, u;
        r && r.isOpen && (n && r.getAutoClose() === "disabled" || n && n.which === 3 || (i = r.getToggleElement(), n && i && i[0].contains(n.target)) || (u = r.getDropdownElement(), n && r.getAutoClose() === "outsideClick" && u && u[0].contains(n.target)) || (r.focusToggleElement(), r.isOpen = !1, t.$$phase || r.$apply()))
    }, this.keybindFilter = function(n) {
        if (r) {
            var t = r.getDropdownElement(),
                i = r.getToggleElement(),
                u = t && t[0].contains(n.target),
                e = i && i[0].contains(n.target);
            n.which === 27 ? (n.stopPropagation(), r.focusToggleElement(), f()) : r.isKeynavEnabled() && [38, 40].indexOf(n.which) !== -1 && r.isOpen && (u || e) && (n.preventDefault(), n.stopPropagation(), r.focusDropdownEntry(n.which))
        }
    }
}]).controller("UibDropdownController", ["$scope", "$element", "$attrs", "$parse", "uibDropdownConfig", "uibDropdownService", "$animate", "$uibPosition", "$document", "$compile", "$templateRequest", function(n, t, i, r, u, f, e, o, s, h, c) {
    function w() {
        t.append(l.dropdownMenu)
    }
    var l = this,
        a = n.$new(),
        y, k = u.appendToOpenClass,
        d = u.openClass,
        p, v = angular.noop,
        g = i.onToggle ? r(i.onToggle) : angular.noop,
        b = !1,
        tt = null,
        nt = s.find("body");
    t.addClass("dropdown"), this.init = function() {
        i.isOpen && (p = r(i.isOpen), v = p.assign, n.$watch(p, function(n) {
            a.isOpen = !!n
        })), b = angular.isDefined(i.keyboardNav)
    }, this.toggle = function(n) {
        return a.isOpen = arguments.length ? !!n : !a.isOpen, angular.isFunction(v) && v(a, a.isOpen), a.isOpen
    }, this.isOpen = function() {
        return a.isOpen
    }, a.getToggleElement = function() {
        return l.toggleElement
    }, a.getAutoClose = function() {
        return i.autoClose || "always"
    }, a.getElement = function() {
        return t
    }, a.isKeynavEnabled = function() {
        return b
    }, a.focusDropdownEntry = function(n) {
        var i = l.dropdownMenu ? angular.element(l.dropdownMenu).find("a") : t.find("ul").eq(0).find("a");
        switch (n) {
            case 40:
                l.selectedOption = angular.isNumber(l.selectedOption) ? l.selectedOption === i.length - 1 ? l.selectedOption : l.selectedOption + 1 : 0;
                break;
            case 38:
                l.selectedOption = angular.isNumber(l.selectedOption) ? l.selectedOption === 0 ? 0 : l.selectedOption - 1 : i.length - 1
        }
        i[l.selectedOption].focus()
    }, a.getDropdownElement = function() {
        return l.dropdownMenu
    }, a.focusToggleElement = function() {
        l.toggleElement && l.toggleElement[0].focus()
    }, a.$watch("isOpen", function(u, p) {
        var b = null,
            st = !1,
            ot, lt, it, tt, et, ut, ft, rt, ht, ct;
        if (angular.isDefined(i.dropdownAppendTo) && (ot = r(i.dropdownAppendTo)(a), ot && (b = angular.element(ot))), angular.isDefined(i.dropdownAppendToBody) && (lt = r(i.dropdownAppendToBody)(a), lt !== !1 && (st = !0)), st && !b && (b = nt), b && l.dropdownMenu)
            if (u) {
                b.append(l.dropdownMenu);
                t.on("$destroy", w)
            } else t.off("$destroy", w), w();
        b && l.dropdownMenu && (it = o.positionElements(t, l.dropdownMenu, "bottom-left", !0), ft = 0, tt = {
            top: it.top + "px",
            display: u ? "block" : "none"
        }, et = l.dropdownMenu.hasClass("dropdown-menu-right"), et ? (tt.left = "auto", ut = o.scrollbarPadding(b), ut.heightOverflow && ut.scrollbarWidth && (ft = ut.scrollbarWidth), tt.right = window.innerWidth - ft - (it.left + t.prop("offsetWidth")) + "px") : (tt.left = it.left + "px", tt.right = "auto"), st || (rt = o.offset(b), tt.top = it.top - rt.top + "px", et ? tt.right = window.innerWidth - (it.left - rt.left + t.prop("offsetWidth")) + "px" : tt.left = it.left - rt.left + "px"), l.dropdownMenu.css(tt));
        var at = b ? b : t,
            vt = b ? k : d,
            yt = at.hasClass(vt),
            pt = f.isOnlyOpen(n, b);
        if (yt === !u && (ht = b ? pt ? "removeClass" : "addClass" : u ? "addClass" : "removeClass", e[ht](at, vt).then(function() {
                angular.isDefined(u) && u !== p && g(n, {
                    open: !!u
                })
            })), u) {
            if (l.dropdownMenuTemplateUrl) c(l.dropdownMenuTemplateUrl).then(function(n) {
                y = a.$new(), h(n.trim())(y, function(n) {
                    var t = n;
                    l.dropdownMenu.replaceWith(t), l.dropdownMenu = t;
                    s.on("keydown", f.keybindFilter)
                })
            });
            else s.on("keydown", f.keybindFilter);
            a.focusToggleElement(), f.open(a, t, b)
        } else f.close(a, t, b), l.dropdownMenuTemplateUrl && (y && y.$destroy(), ct = angular.element('<ul class="dropdown-menu"></ul>'), l.dropdownMenu.replaceWith(ct), l.dropdownMenu = ct), l.selectedOption = null;
        angular.isFunction(v) && v(n, u)
    })
}]).directive("uibDropdown", function() {
    return {
        controller: "UibDropdownController",
        link: function(n, t, i, r) {
            r.init()
        }
    }
}).directive("uibDropdownMenu", function() {
    return {
        restrict: "A",
        require: "?^uibDropdown",
        link: function(n, t, i, r) {
            if (r && !angular.isDefined(i.dropdownNested)) {
                t.addClass("dropdown-menu");
                var u = i.templateUrl;
                u && (r.dropdownMenuTemplateUrl = u), r.dropdownMenu || (r.dropdownMenu = t)
            }
        }
    }
}).directive("uibDropdownToggle", function() {
    return {
        require: "?^uibDropdown",
        link: function(n, t, i, r) {
            if (r) {
                t.addClass("dropdown-toggle"), r.toggleElement = t;
                var u = function(u) {
                    u.preventDefault(), t.hasClass("disabled") || i.disabled || n.$apply(function() {
                        r.toggle()
                    })
                };
                t.on("click", u);
                t.attr({
                    "aria-haspopup": !0,
                    "aria-expanded": !1
                }), n.$watch(r.isOpen, function(n) {
                    t.attr("aria-expanded", !!n)
                }), n.$on("$destroy", function() {
                    t.off("click", u)
                })
            }
        }
    }
}), angular.module("ui.bootstrap.multiMap", []).factory("$$multiMap", function() {
    return {
        createNew: function() {
            var n = {};
            return {
                entries: function() {
                    return Object.keys(n).map(function(t) {
                        return {
                            key: t,
                            value: n[t]
                        }
                    })
                },
                get: function(t) {
                    return n[t]
                },
                hasKey: function(t) {
                    return !!n[t]
                },
                keys: function() {
                    return Object.keys(n)
                },
                put: function(t, i) {
                    n[t] || (n[t] = []), n[t].push(i)
                },
                remove: function(t, i) {
                    var r = n[t],
                        u;
                    r && (u = r.indexOf(i), u !== -1 && r.splice(u, 1), r.length || delete n[t])
                }
            }
        }
    }
}), angular.module("ui.bootstrap.stackedMap", []).factory("$$stackedMap", function() {
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
                        if (t === n[i].key) return n[i]
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
                        if (t === n[i].key) {
                            r = i;
                            break
                        } return n.splice(r, 1)[0]
                },
                removeTop: function() {
                    return n.pop()
                },
                length: function() {
                    return n.length
                }
            }
        }
    }
}), angular.module("ui.bootstrap.modal", ["ui.bootstrap.multiMap", "ui.bootstrap.stackedMap", "ui.bootstrap.position"]).provider("$uibResolve", function() {
    var n = this;
    this.resolver = null, this.setResolver = function(n) {
        this.resolver = n
    }, this.$get = ["$injector", "$q", function(t, i) {
        var r = n.resolver ? t.get(n.resolver) : null;
        return {
            resolve: function(n, u, f, e) {
                if (r) return r.resolve(n, u, f, e);
                var o = [];
                return angular.forEach(n, function(n) {
                    angular.isFunction(n) || angular.isArray(n) ? o.push(i.resolve(t.invoke(n))) : angular.isString(n) ? o.push(i.resolve(t.get(n))) : o.push(i.resolve(n))
                }), i.all(o).then(function(t) {
                    var i = {},
                        r = 0;
                    return angular.forEach(n, function(n, u) {
                        i[u] = t[r++]
                    }), i
                })
            }
        }
    }]
}).directive("uibModalBackdrop", ["$animate", "$injector", "$uibModalStack", function(n, t, i) {
    function r(t, r, u) {
        u.modalInClass && (n.addClass(r, u.modalInClass), t.$on(i.NOW_CLOSING_EVENT, function(i, f) {
            var e = f();
            t.modalOptions.animation ? n.removeClass(r, u.modalInClass).then(e) : e()
        }))
    }
    return {
        restrict: "A",
        compile: function(n, t) {
            return n.addClass(t.backdropClass), r
        }
    }
}]).directive("uibModalWindow", ["$uibModalStack", "$q", "$animateCss", "$document", function(n, t, i, r) {
    return {
        scope: {
            index: "@"
        },
        restrict: "A",
        transclude: !0,
        templateUrl: function(n, t) {
            return t.templateUrl || "uib/template/modal/window.html"
        },
        link: function(u, f, e) {
            f.addClass(e.windowTopClass || ""), u.size = e.size, u.close = function(t) {
                var i = n.getTop();
                i && i.value.backdrop && i.value.backdrop !== "static" && t.target === t.currentTarget && (t.preventDefault(), t.stopPropagation(), n.dismiss(i.key, "backdrop click"))
            };
            f.on("click", u.close);
            u.$isRendered = !0;
            var o = t.defer();
            u.$$postDigest(function() {
                o.resolve()
            }), o.promise.then(function() {
                var o = null;
                e.modalInClass && (o = i(f, {
                    addClass: e.modalInClass
                }).start(), u.$on(n.NOW_CLOSING_EVENT, function(n, t) {
                    var r = t();
                    i(f, {
                        removeClass: e.modalInClass
                    }).start().then(r)
                })), t.when(o).then(function() {
                    var i = n.getTop(),
                        t;
                    i && n.modalRendered(i.key), r[0].activeElement && f[0].contains(r[0].activeElement) || (t = f[0].querySelector("[autofocus]"), t ? t.focus() : f[0].focus())
                })
            })
        }
    }
}]).directive("uibModalAnimationClass", function() {
    return {
        compile: function(n, t) {
            t.modalAnimation && n.addClass(t.uibModalAnimationClass)
        }
    }
}).directive("uibModalTransclude", ["$animate", function(n) {
    return {
        link: function(t, i, r, u, f) {
            f(t.$parent, function(t) {
                i.empty(), n.enter(t, i)
            })
        }
    }
}]).factory("$uibModalStack", ["$animate", "$animateCss", "$document", "$compile", "$rootScope", "$q", "$$multiMap", "$$stackedMap", "$uibPosition", function(n, t, i, r, u, f, e, o, s) {
    function ot(n) {
        var t = "-";
        return n.replace(et, function(n, i) {
            return (i ? t : "") + n.toLowerCase()
        })
    }

    function st(n) {
        return !!(n.offsetWidth || n.offsetHeight || n.getClientRects().length)
    }

    function k() {
        for (var n = -1, i = c.keys(), t = 0; t < i.length; t++) c.get(i[t]).value.backdrop && (n = t);
        return n > -1 && n < w && (n = w), n
    }

    function g(n, t) {
        var i = c.get(n).value,
            r = i.appendTo;
        c.remove(n), p = c.top(), p && (w = parseInt(p.value.modalDomEl.attr("index"), 10)), tt(i.modalDomEl, i.modalScope, function() {
            var t = i.openedClass || d,
                u;
            b.remove(t, n), u = b.hasKey(t), r.toggleClass(t, u), !u && l && l.heightOverflow && l.scrollbarWidth && (l.originalRight ? r.css({
                paddingRight: l.originalRight + "px"
            }) : r.css({
                paddingRight: ""
            }), l = null), nt(!0)
        }, i.closedDeferred), ht(), t && t.focus ? t.focus() : r.focus && r.focus()
    }

    function nt(n) {
        var t;
        c.length() > 0 && (t = c.top().value, t.modalDomEl.toggleClass(t.windowTopClass || "", n))
    }

    function ht() {
        if (a && k() === -1) {
            var n = v;
            tt(a, v, function() {
                n = null
            }), a = undefined, v = undefined
        }
    }

    function tt(t, i, r, u) {
        function o() {
            o.done || (o.done = !0, n.leave(t).then(function() {
                r && r(), t.remove(), u && u.resolve()
            }), i.$destroy())
        }
        var e, s = null,
            c = function() {
                return e || (e = f.defer(), s = e.promise),
                    function() {
                        e.resolve()
                    }
            };
        return i.$broadcast(h.NOW_CLOSING_EVENT, c), f.when(s).then(o)
    }

    function it(n) {
        var t, i, r;
        if (n.isDefaultPrevented()) return n;
        if (t = c.top(), t) switch (n.which) {
            case 27:
                t.value.keyboard && (n.preventDefault(), u.$apply(function() {
                    h.dismiss(t.key, "escape key press")
                }));
                break;
            case 9:
                i = h.loadFocusElementList(t), r = !1, n.shiftKey ? (h.isFocusInFirstItem(n, i) || h.isModalFocused(n, t)) && (r = h.focusLastFocusableElement(i)) : h.isFocusInLastItem(n, i) && (r = h.focusFirstFocusableElement(i)), r && (n.preventDefault(), n.stopPropagation())
        }
    }

    function rt(n, t, i) {
        return !n.value.modalScope.$broadcast("modal.closing", t, i).defaultPrevented
    }

    function ut() {
        Array.prototype.forEach.call(document.querySelectorAll("[" + y + "]"), function(n) {
            var i = parseInt(n.getAttribute(y), 10),
                t = i - 1;
            n.setAttribute(y, t), t || (n.removeAttribute(y), n.removeAttribute("aria-hidden"))
        })
    }
    var d = "modal-open",
        a, v, c = o.createNew(),
        b = e.createNew(),
        h = {
            NOW_CLOSING_EVENT: "modal.stack.now-closing"
        },
        w = 0,
        p = null,
        y = "data-bootstrap-modal-aria-hidden-count",
        ft = "a[href], area[href], input:not([disabled]):not([tabindex='-1']), button:not([disabled]):not([tabindex='-1']),select:not([disabled]):not([tabindex='-1']), textarea:not([disabled]):not([tabindex='-1']), iframe, object, embed, *[tabindex]:not([tabindex='-1']), *[contenteditable=true]",
        l, et = /[A-Z]/g;
    u.$watch(k, function(n) {
        v && (v.index = n)
    });
    i.on("keydown", it);
    return u.$on("$destroy", function() {
        i.off("keydown", it)
    }), h.open = function(t, f) {
        function it(n) {
            function t(n) {
                var t = n.parent() ? n.parent().children() : [];
                return Array.prototype.filter.call(t, function(t) {
                    return t !== n[0]
                })
            }
            if (n && n[0].tagName !== "BODY") return t(n).forEach(function(n) {
                var i = n.getAttribute("aria-hidden") === "true",
                    t = parseInt(n.getAttribute(y), 10);
                t || (t = i ? 1 : 0), n.setAttribute(y, t + 1), n.setAttribute("aria-hidden", "true")
            }), it(n.parent())
        }
        var rt = i[0].activeElement,
            tt = f.openedClass || d,
            e, g, h, o;
        nt(!1), p = c.top(), c.add(t, {
            deferred: f.deferred,
            renderDeferred: f.renderDeferred,
            closedDeferred: f.closedDeferred,
            modalScope: f.scope,
            backdrop: f.backdrop,
            keyboard: f.keyboard,
            openedClass: f.openedClass,
            windowTopClass: f.windowTopClass,
            animation: f.animation,
            appendTo: f.appendTo
        }), b.put(tt, t), e = f.appendTo, g = k(), g >= 0 && !a && (v = u.$new(!0), v.modalOptions = f, v.index = g, a = angular.element('<div uib-modal-backdrop="modal-backdrop"></div>'), a.attr({
            "class": "modal-backdrop",
            "ng-style": "{'z-index': 1040 + (index && 1 || 0) + index*10}",
            "uib-modal-animation-class": "fade",
            "modal-in-class": "in"
        }), f.backdropClass && a.addClass(f.backdropClass), f.animation && a.attr("modal-animation", "true"), r(a)(v), n.enter(a, e), s.isScrollable(e) && (l = s.scrollbarPadding(e), l.heightOverflow && l.scrollbarWidth && e.css({
            paddingRight: l.right + "px"
        }))), f.component ? (h = document.createElement(ot(f.component.name)), h = angular.element(h), h.attr({
            resolve: "$resolve",
            "modal-instance": "$uibModalInstance",
            close: "$close($value)",
            dismiss: "$dismiss($value)"
        })) : h = f.content, w = p ? parseInt(p.value.modalDomEl.attr("index"), 10) + 1 : 0, o = angular.element('<div uib-modal-window="modal-window"></div>'), o.attr({
            "class": "modal",
            "template-url": f.windowTemplateUrl,
            "window-top-class": f.windowTopClass,
            role: "dialog",
            "aria-labelledby": f.ariaLabelledBy,
            "aria-describedby": f.ariaDescribedBy,
            size: f.size,
            index: w,
            animate: "animate",
            "ng-style": "{'z-index': 1050 + $$topModalIndex*10, display: 'block'}",
            tabindex: -1,
            "uib-modal-animation-class": "fade",
            "modal-in-class": "in"
        }).append(h), f.windowClass && o.addClass(f.windowClass), f.animation && o.attr("modal-animation", "true"), e.addClass(tt), f.scope && (f.scope.$$topModalIndex = w), n.enter(r(o)(f.scope), e), c.top().value.modalDomEl = o, c.top().value.modalOpener = rt, it(o)
    }, h.close = function(n, t) {
        var i = c.get(n);
        return (ut(), i && rt(i, t, !0)) ? (i.value.modalScope.$$uibDestructionScheduled = !0, i.value.deferred.resolve(t), g(n, i.value.modalOpener), !0) : !i
    }, h.dismiss = function(n, t) {
        var i = c.get(n);
        return (ut(), i && rt(i, t, !1)) ? (i.value.modalScope.$$uibDestructionScheduled = !0, i.value.deferred.reject(t), g(n, i.value.modalOpener), !0) : !i
    }, h.dismissAll = function(n) {
        for (var t = this.getTop(); t && this.dismiss(t.key, n);) t = this.getTop()
    }, h.getTop = function() {
        return c.top()
    }, h.modalRendered = function(n) {
        var t = c.get(n);
        t && t.value.renderDeferred.resolve()
    }, h.focusFirstFocusableElement = function(n) {
        return n.length > 0 ? (n[0].focus(), !0) : !1
    }, h.focusLastFocusableElement = function(n) {
        return n.length > 0 ? (n[n.length - 1].focus(), !0) : !1
    }, h.isModalFocused = function(n, t) {
        if (n && t) {
            var i = t.value.modalDomEl;
            if (i && i.length) return (n.target || n.srcElement) === i[0]
        }
        return !1
    }, h.isFocusInFirstItem = function(n, t) {
        return t.length > 0 ? (n.target || n.srcElement) === t[0] : !1
    }, h.isFocusInLastItem = function(n, t) {
        return t.length > 0 ? (n.target || n.srcElement) === t[t.length - 1] : !1
    }, h.loadFocusElementList = function(n) {
        var t, i;
        if (n && (t = n.value.modalDomEl, t && t.length)) return i = t[0].querySelectorAll(ft), i ? Array.prototype.filter.call(i, function(n) {
            return st(n)
        }) : i
    }, h
}]).provider("$uibModal", function() {
    var n = {
        options: {
            animation: !0,
            backdrop: !0,
            keyboard: !0
        },
        $get: ["$rootScope", "$q", "$document", "$templateRequest", "$controller", "$uibResolve", "$uibModalStack", function(t, i, r, u, f, e, o) {
            function c(n) {
                return n.template ? i.when(n.template) : u(angular.isFunction(n.templateUrl) ? n.templateUrl() : n.templateUrl)
            }
            var h = {},
                s = null;
            return h.getPromiseChain = function() {
                return s
            }, h.open = function(u) {
                function w() {
                    return v
                }
                var l = i.defer(),
                    a = i.defer(),
                    y = i.defer(),
                    p = i.defer(),
                    h = {
                        result: l.promise,
                        opened: a.promise,
                        closed: y.promise,
                        rendered: p.promise,
                        close: function(n) {
                            return o.close(h, n)
                        },
                        dismiss: function(n) {
                            return o.dismiss(h, n)
                        }
                    },
                    v, b;
                if (u = angular.extend({}, n.options, u), u.resolve = u.resolve || {}, u.appendTo = u.appendTo || r.find("body").eq(0), !u.appendTo.length) throw new Error("appendTo element not found. Make sure that the element passed is in DOM.");
                if (!u.component && !u.template && !u.templateUrl) throw new Error("One of component or template or templateUrl options is required.");
                return v = u.component ? i.when(e.resolve(u.resolve, {}, null, null)) : i.all([c(u), e.resolve(u.resolve, {}, null, null)]), b = s = i.all([s]).then(w, w).then(function(n) {
                    function b(t, r, u, f) {
                        t.$scope = i, t.$scope.$resolve = {}, u ? t.$scope.$uibModalInstance = h : t.$uibModalInstance = h;
                        var e = r ? n[1] : n;
                        angular.forEach(e, function(n, i) {
                            f && (t[i] = n), t.$scope.$resolve[i] = n
                        })
                    }
                    var w = u.scope || t,
                        i = w.$new();
                    i.$close = h.close, i.$dismiss = h.dismiss, i.$on("$destroy", function() {
                        i.$$uibDestructionScheduled || i.$dismiss("$uibUnscheduledDestruction")
                    });
                    var e = {
                            scope: i,
                            deferred: l,
                            renderDeferred: p,
                            closedDeferred: y,
                            animation: u.animation,
                            backdrop: u.backdrop,
                            keyboard: u.keyboard,
                            backdropClass: u.backdropClass,
                            windowTopClass: u.windowTopClass,
                            windowClass: u.windowClass,
                            windowTemplateUrl: u.windowTemplateUrl,
                            ariaLabelledBy: u.ariaLabelledBy,
                            ariaDescribedBy: u.ariaDescribedBy,
                            size: u.size,
                            openedClass: u.openedClass,
                            appendTo: u.appendTo
                        },
                        s = {},
                        r, c, v = {};
                    u.component ? (b(s, !1, !0, !1), s.name = u.component, e.component = s) : u.controller && (b(v, !0, !1, !0), c = f(u.controller, v, !0, u.controllerAs), u.controllerAs && u.bindToController && (r = c.instance, r.$close = i.$close, r.$dismiss = i.$dismiss, angular.extend(r, {
                        $resolve: v.$scope.$resolve
                    }, w)), r = c(), angular.isFunction(r.$onInit) && r.$onInit()), u.component || (e.content = n[0]), o.open(h, e), a.resolve(!0)
                }, function(n) {
                    a.reject(n), l.reject(n)
                })["finally"](function() {
                    s === b && (s = null)
                }), h
            }, h
        }]
    };
    return n
}), angular.module("ui.bootstrap.tooltip", ["ui.bootstrap.position", "ui.bootstrap.stackedMap"]).provider("$uibTooltip", function() {
    function r(n) {
        var t = /[A-Z]/g,
            i = "-";
        return n.replace(t, function(n, t) {
            return (t ? i : "") + n.toLowerCase()
        })
    }
    var i = {
            placement: "top",
            placementClassPrefix: "",
            animation: !0,
            popupDelay: 0,
            popupCloseDelay: 0,
            useContentExp: !1
        },
        n = {
            mouseenter: "mouseleave",
            click: "click",
            outsideClick: "outsideClick",
            focus: "blur",
            none: ""
        },
        t = {};
    this.options = function(n) {
        angular.extend(t, n)
    }, this.setTriggers = function(t) {
        angular.extend(n, t)
    }, this.$get = ["$window", "$compile", "$timeout", "$document", "$uibPosition", "$interpolate", "$rootScope", "$parse", "$$stackedMap", function(u, f, e, o, s, h, c, l, a) {
        function y(n) {
            if (n.which === 27) {
                var t = v.top();
                t && (t.value.close(), t = null)
            }
        }
        var v = a.createNew();
        o.on("keyup", y);
        return c.$on("$destroy", function() {
                o.off("keyup", y)
            }),
            function(u, c, a, y) {
                function p(t) {
                    var i = (t || y.trigger || a).split(" "),
                        r = i.map(function(t) {
                            return n[t] || t
                        });
                    return {
                        show: i,
                        hide: r
                    }
                }
                y = angular.extend({}, i, t, y);
                var k = r(u),
                    w = h.startSymbol(),
                    b = h.endSymbol(),
                    d = "<div " + k + '-popup uib-title="' + w + "title" + b + '" ' + (y.useContentExp ? 'content-exp="contentExp()" ' : 'content="' + w + "content" + b + '" ') + 'origin-scope="origScope" class="uib-position-measure ' + c + '" tooltip-animation-class="fade"uib-tooltip-classes ng-class="{ in: isOpen }" ></div>';
                return {
                    compile: function() {
                        var i = f(d);
                        return function(n, t, r) {
                            function tt() {
                                h.isOpen ? nt() : kt()
                            }

                            function kt() {
                                (!ui || n.$eval(r[c + "Enable"])) && (dt(), oi(), h.popupDelay ? st || (st = e(ni, h.popupDelay, !1)) : ni())
                            }

                            function nt() {
                                vt(), h.popupCloseDelay ? ot || (ot = e(d, h.popupCloseDelay, !1)) : d()
                            }

                            function ni() {
                                if (vt(), dt(), !h.content) return angular.noop;
                                fi(), h.$evalAsync(function() {
                                    h.isOpen = !0, ti(!0), ut()
                                })
                            }

                            function vt() {
                                st && (e.cancel(st), st = null), g && (e.cancel(g), g = null)
                            }

                            function d() {
                                h && h.$evalAsync(function() {
                                    h && (h.isOpen = !1, ti(!1), h.animation ? ft || (ft = e(pt, 150, !1)) : pt())
                                })
                            }

                            function dt() {
                                ot && (e.cancel(ot), ot = null), ft && (e.cancel(ft), ft = null)
                            }

                            function fi() {
                                a || (k = h.$new(), a = i(k, function(n) {
                                    et ? o.find("body").append(n) : t.after(n)
                                }), v.add(h, {
                                    close: d
                                }), ei())
                            }

                            function pt() {
                                vt(), dt(), si(), a && (a.remove(), a = null, ct && e.cancel(ct)), v.remove(h), k && (k.$destroy(), k = null)
                            }

                            function oi() {
                                var t, i, f;
                                h.title = r[c + "Title"], h.content = ht ? ht(n) : r[u], h.popupClass = r[c + "Class"], h.placement = angular.isDefined(r[c + "Placement"]) ? r[c + "Placement"] : y.placement, t = s.parsePlacement(h.placement), it = t[1] ? t[0] + "-" + t[1] : t[0], i = parseInt(r[c + "PopupDelay"], 10), f = parseInt(r[c + "PopupCloseDelay"], 10), h.popupDelay = isNaN(i) ? y.popupDelay : i, h.popupCloseDelay = isNaN(f) ? y.popupCloseDelay : f
                            }

                            function ti(t) {
                                rt && angular.isFunction(rt.assign) && rt.assign(n, t)
                            }

                            function ei() {
                                w.length = 0, ht ? (w.push(n.$watch(ht, function(n) {
                                    h.content = n, !n && h.isOpen && d()
                                })), w.push(k.$watch(function() {
                                    wt || (wt = !0, k.$$postDigest(function() {
                                        wt = !1, h && h.isOpen && ut()
                                    }))
                                }))) : w.push(r.$observe(u, function(n) {
                                    h.content = n, !n && h.isOpen ? d() : ut()
                                })), w.push(r.$observe(c + "Title", function(n) {
                                    h.title = n, h.isOpen && ut()
                                })), w.push(r.$observe(c + "Placement", function(n) {
                                    h.placement = n ? n : y.placement, h.isOpen && ut()
                                }))
                            }

                            function si() {
                                w.length && (angular.forEach(w, function(n) {
                                    n()
                                }), w.length = 0)
                            }

                            function ii(n) {
                                h && h.isOpen && a && (t[0].contains(n.target) || a[0].contains(n.target) || nt())
                            }

                            function gt(n) {
                                n.which === 27 && nt()
                            }

                            function ri() {
                                var f = [],
                                    u = [],
                                    i = n.$eval(r[c + "Trigger"]);
                                bt(), angular.isObject(i) ? (Object.keys(i).forEach(function(n) {
                                    f.push(n), u.push(i[n])
                                }), b = {
                                    show: f,
                                    hide: u
                                }) : b = p(i), b.show !== "none" && b.show.forEach(function(n, i) {
                                    if (n === "outsideClick") {
                                        t.on("click", tt);
                                        o.on("click", ii)
                                    } else if (n === b.hide[i]) t.on(n, tt);
                                    else if (n) {
                                        t.on(n, kt);
                                        t.on(b.hide[i], nt)
                                    }
                                    t.on("keypress", gt)
                                })
                            }
                            var a, k, ft, st, ot, g, ct, et = angular.isDefined(y.appendToBody) ? y.appendToBody : !1,
                                b = p(undefined),
                                ui = angular.isDefined(r[c + "Enable"]),
                                h = n.$new(!0),
                                wt = !1,
                                rt = angular.isDefined(r[c + "IsOpen"]) ? l(r[c + "IsOpen"]) : !1,
                                ht = y.useContentExp ? l(r[u]) : !1,
                                w = [],
                                it, ut = function() {
                                    a && a.html() && (g || (g = e(function() {
                                        var n = s.positionElements(t, a, h.placement, et),
                                            r = angular.isDefined(a.offsetHeight) ? a.offsetHeight : a.prop("offsetHeight"),
                                            u = et ? s.offset(t) : s.position(t),
                                            i;
                                        a.css({
                                            top: n.top + "px",
                                            left: n.left + "px"
                                        }), i = n.placement.split("-"), a.hasClass(i[0]) || (a.removeClass(it.split("-")[0]), a.addClass(i[0])), a.hasClass(y.placementClassPrefix + n.placement) || (a.removeClass(y.placementClassPrefix + it), a.addClass(y.placementClassPrefix + n.placement)), ct = e(function() {
                                            var t = angular.isDefined(a.offsetHeight) ? a.offsetHeight : a.prop("offsetHeight"),
                                                n = s.adjustTop(i, u, r, t);
                                            n && a.css(n), ct = null
                                        }, 0, !1), a.hasClass("uib-position-measure") ? (s.positionArrow(a, n.placement), a.removeClass("uib-position-measure")) : it !== n.placement && s.positionArrow(a, n.placement), it = n.placement, g = null
                                    }, 0, !1)))
                                },
                                bt, yt, at, lt;
                            h.origScope = n, h.isOpen = !1, h.contentExp = function() {
                                return h.content
                            }, r.$observe("disabled", function(n) {
                                n && vt(), n && h.isOpen && d()
                            }), rt && n.$watch(rt, function(n) {
                                h && !n === h.isOpen && tt()
                            }), bt = function() {
                                b.show.forEach(function(n) {
                                    n === "outsideClick" ? t.off("click", tt) : (t.off(n, kt), t.off(n, tt)), t.off("keypress", gt)
                                }), b.hide.forEach(function(n) {
                                    n === "outsideClick" ? o.off("click", ii) : t.off(n, nt)
                                })
                            }, ri(), yt = n.$eval(r[c + "Animation"]), h.animation = angular.isDefined(yt) ? !!yt : y.animation, lt = c + "AppendToBody", at = lt in r && r[lt] === undefined ? !0 : n.$eval(r[lt]), et = angular.isDefined(at) ? at : et, n.$on("$destroy", function() {
                                bt(), pt(), h = null
                            })
                        }
                    }
                }
            }
    }]
}).directive("uibTooltipTemplateTransclude", ["$animate", "$sce", "$compile", "$templateRequest", function(n, t, i, r) {
    return {
        link: function(u, f, e) {
            var a = u.$eval(e.tooltipTemplateTranscludeScope),
                l = 0,
                o, s, h, c = function() {
                    s && (s.remove(), s = null), o && (o.$destroy(), o = null), h && (n.leave(h).then(function() {
                        s = null
                    }), s = h, h = null)
                };
            u.$watch(t.parseAsResourceUrl(e.uibTooltipTemplateTransclude), function(t) {
                var e = ++l;
                t ? (r(t, !0).then(function(r) {
                    if (e === l) {
                        var u = a.$new(),
                            s = r,
                            v = i(s)(u, function(t) {
                                c(), n.enter(t, f)
                            });
                        o = u, h = v, o.$emit("$includeContentLoaded", t)
                    }
                }, function() {
                    e === l && (c(), u.$emit("$includeContentError", t))
                }), u.$emit("$includeContentRequested", t)) : c()
            }), u.$on("$destroy", c)
        }
    }
}]).directive("uibTooltipClasses", ["$uibPosition", function(n) {
    return {
        restrict: "A",
        link: function(t, i, r) {
            if (t.placement) {
                var u = n.parsePlacement(t.placement);
                i.addClass(u[0])
            }
            t.popupClass && i.addClass(t.popupClass), t.animation && i.addClass(r.tooltipAnimationClass)
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
}).directive("uibTooltip", ["$uibTooltip", function(n) {
    return n("uibTooltip", "tooltip", "mouseenter")
}]).directive("uibTooltipTemplatePopup", function() {
    return {
        restrict: "A",
        scope: {
            contentExp: "&",
            originScope: "&"
        },
        templateUrl: "uib/template/tooltip/tooltip-template-popup.html"
    }
}).directive("uibTooltipTemplate", ["$uibTooltip", function(n) {
    return n("uibTooltipTemplate", "tooltip", "mouseenter", {
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
}).directive("uibTooltipHtml", ["$uibTooltip", function(n) {
    return n("uibTooltipHtml", "tooltip", "mouseenter", {
        useContentExp: !0
    })
}]), angular.module("ui.bootstrap.popover", ["ui.bootstrap.tooltip"]).directive("uibPopoverTemplatePopup", function() {
    return {
        restrict: "A",
        scope: {
            uibTitle: "@",
            contentExp: "&",
            originScope: "&"
        },
        templateUrl: "uib/template/popover/popover-template.html"
    }
}).directive("uibPopoverTemplate", ["$uibTooltip", function(n) {
    return n("uibPopoverTemplate", "popover", "click", {
        useContentExp: !0
    })
}]).directive("uibPopoverHtmlPopup", function() {
    return {
        restrict: "A",
        scope: {
            contentExp: "&",
            uibTitle: "@"
        },
        templateUrl: "uib/template/popover/popover-html.html"
    }
}).directive("uibPopoverHtml", ["$uibTooltip", function(n) {
    return n("uibPopoverHtml", "popover", "click", {
        useContentExp: !0
    })
}]).directive("uibPopoverPopup", function() {
    return {
        restrict: "A",
        scope: {
            uibTitle: "@",
            content: "@"
        },
        templateUrl: "uib/template/popover/popover.html"
    }
}).directive("uibPopover", ["$uibTooltip", function(n) {
    return n("uibPopover", "popover", "click")
}]), angular.module("uib/template/carousel/carousel.html", []).run(["$templateCache", function(n) {
    n.put("uib/template/carousel/carousel.html", '<div class="carousel-inner" ng-transclude></div>\n<a role="button" href class="left carousel-control" ng-click="prev()" ng-class="{ disabled: isPrevDisabled() }" ng-show="slides.length > 1">\n  <span aria-hidden="true" class="glyphicon glyphicon-chevron-left"></span>\n  <span class="sr-only">previous</span>\n</a>\n<a role="button" href class="right carousel-control" ng-click="next()" ng-class="{ disabled: isNextDisabled() }" ng-show="slides.length > 1">\n  <span aria-hidden="true" class="glyphicon glyphicon-chevron-right"></span>\n  <span class="sr-only">next</span>\n</a>\n<ol class="carousel-indicators" ng-show="slides.length > 1">\n  <li ng-repeat="slide in slides | orderBy:indexOfSlide track by $index" ng-class="{ active: isActive(slide) }" ng-click="select(slide)">\n    <span class="sr-only">slide {{ $index + 1 }} of {{ slides.length }}<span ng-if="isActive(slide)">, currently active</span></span>\n  </li>\n</ol>\n')
}]), angular.module("uib/template/carousel/slide.html", []).run(["$templateCache", function(n) {
    n.put("uib/template/carousel/slide.html", '<div class="text-center" ng-transclude></div>\n')
}]), angular.module("uib/template/modal/window.html", []).run(["$templateCache", function(n) {
    n.put("uib/template/modal/window.html", "<div class=\"modal-dialog {{size ? 'modal-' + size : ''}}\"><div class=\"modal-content\" uib-modal-transclude></div></div>\n")
}]), angular.module("uib/template/tooltip/tooltip-html-popup.html", []).run(["$templateCache", function(n) {
    n.put("uib/template/tooltip/tooltip-html-popup.html", '<div class="tooltip-arrow"></div>\n<div class="tooltip-inner" ng-bind-html="contentExp()"></div>\n')
}]), angular.module("uib/template/tooltip/tooltip-popup.html", []).run(["$templateCache", function(n) {
    n.put("uib/template/tooltip/tooltip-popup.html", '<div class="tooltip-arrow"></div>\n<div class="tooltip-inner" ng-bind="content"></div>\n')
}]), angular.module("uib/template/tooltip/tooltip-template-popup.html", []).run(["$templateCache", function(n) {
    n.put("uib/template/tooltip/tooltip-template-popup.html", '<div class="tooltip-arrow"></div>\n<div class="tooltip-inner"\n  uib-tooltip-template-transclude="contentExp()"\n  tooltip-template-transclude-scope="originScope()"></div>\n')
}]), angular.module("uib/template/popover/popover-html.html", []).run(["$templateCache", function(n) {
    n.put("uib/template/popover/popover-html.html", '<div class="arrow"></div>\n\n<div class="popover-inner">\n    <h3 class="popover-title" ng-bind="uibTitle" ng-if="uibTitle"></h3>\n    <div class="popover-content" ng-bind-html="contentExp()"></div>\n</div>\n')
}]), angular.module("uib/template/popover/popover-template.html", []).run(["$templateCache", function(n) {
    n.put("uib/template/popover/popover-template.html", '<div class="arrow"></div>\n\n<div class="popover-inner">\n    <h3 class="popover-title" ng-bind="uibTitle" ng-if="uibTitle"></h3>\n    <div class="popover-content"\n      uib-tooltip-template-transclude="contentExp()"\n      tooltip-template-transclude-scope="originScope()"></div>\n</div>\n')
}]), angular.module("uib/template/popover/popover.html", []).run(["$templateCache", function(n) {
    n.put("uib/template/popover/popover.html", '<div class="arrow"></div>\n\n<div class="popover-inner">\n    <h3 class="popover-title" ng-bind="uibTitle" ng-if="uibTitle"></h3>\n    <div class="popover-content" ng-bind="content"></div>\n</div>\n')
}]), angular.module("ui.bootstrap.carousel").run(function() {
    !angular.$$csp().noInlineStyle && !angular.$$uibCarouselCss && angular.element(document).find("head").prepend('<style type="text/css">.ng-animate.item:not(.left):not(.right){-webkit-transition:0s ease-in-out left;transition:0s ease-in-out left}</style>'), angular.$$uibCarouselCss = !0
}), angular.module("ui.bootstrap.position").run(function() {
    !angular.$$csp().noInlineStyle && !angular.$$uibPositionCss && angular.element(document).find("head").prepend('<style type="text/css">.uib-position-measure{display:block !important;visibility:hidden !important;position:absolute !important;top:-9999px !important;left:-9999px !important;}.uib-position-scrollbar-measure{position:absolute !important;top:-9999px !important;width:50px !important;height:50px !important;overflow:scroll !important;}.uib-position-body-scrollbar-measure{overflow:scroll !important;}</style>'), angular.$$uibPositionCss = !0
}), angular.module("ui.bootstrap.tooltip").run(function() {
    !angular.$$csp().noInlineStyle && !angular.$$uibTooltipCss && angular.element(document).find("head").prepend('<style type="text/css">[uib-tooltip-popup].tooltip.top-left > .tooltip-arrow,[uib-tooltip-popup].tooltip.top-right > .tooltip-arrow,[uib-tooltip-popup].tooltip.bottom-left > .tooltip-arrow,[uib-tooltip-popup].tooltip.bottom-right > .tooltip-arrow,[uib-tooltip-popup].tooltip.left-top > .tooltip-arrow,[uib-tooltip-popup].tooltip.left-bottom > .tooltip-arrow,[uib-tooltip-popup].tooltip.right-top > .tooltip-arrow,[uib-tooltip-popup].tooltip.right-bottom > .tooltip-arrow,[uib-tooltip-html-popup].tooltip.top-left > .tooltip-arrow,[uib-tooltip-html-popup].tooltip.top-right > .tooltip-arrow,[uib-tooltip-html-popup].tooltip.bottom-left > .tooltip-arrow,[uib-tooltip-html-popup].tooltip.bottom-right > .tooltip-arrow,[uib-tooltip-html-popup].tooltip.left-top > .tooltip-arrow,[uib-tooltip-html-popup].tooltip.left-bottom > .tooltip-arrow,[uib-tooltip-html-popup].tooltip.right-top > .tooltip-arrow,[uib-tooltip-html-popup].tooltip.right-bottom > .tooltip-arrow,[uib-tooltip-template-popup].tooltip.top-left > .tooltip-arrow,[uib-tooltip-template-popup].tooltip.top-right > .tooltip-arrow,[uib-tooltip-template-popup].tooltip.bottom-left > .tooltip-arrow,[uib-tooltip-template-popup].tooltip.bottom-right > .tooltip-arrow,[uib-tooltip-template-popup].tooltip.left-top > .tooltip-arrow,[uib-tooltip-template-popup].tooltip.left-bottom > .tooltip-arrow,[uib-tooltip-template-popup].tooltip.right-top > .tooltip-arrow,[uib-tooltip-template-popup].tooltip.right-bottom > .tooltip-arrow,[uib-popover-popup].popover.top-left > .arrow,[uib-popover-popup].popover.top-right > .arrow,[uib-popover-popup].popover.bottom-left > .arrow,[uib-popover-popup].popover.bottom-right > .arrow,[uib-popover-popup].popover.left-top > .arrow,[uib-popover-popup].popover.left-bottom > .arrow,[uib-popover-popup].popover.right-top > .arrow,[uib-popover-popup].popover.right-bottom > .arrow,[uib-popover-html-popup].popover.top-left > .arrow,[uib-popover-html-popup].popover.top-right > .arrow,[uib-popover-html-popup].popover.bottom-left > .arrow,[uib-popover-html-popup].popover.bottom-right > .arrow,[uib-popover-html-popup].popover.left-top > .arrow,[uib-popover-html-popup].popover.left-bottom > .arrow,[uib-popover-html-popup].popover.right-top > .arrow,[uib-popover-html-popup].popover.right-bottom > .arrow,[uib-popover-template-popup].popover.top-left > .arrow,[uib-popover-template-popup].popover.top-right > .arrow,[uib-popover-template-popup].popover.bottom-left > .arrow,[uib-popover-template-popup].popover.bottom-right > .arrow,[uib-popover-template-popup].popover.left-top > .arrow,[uib-popover-template-popup].popover.left-bottom > .arrow,[uib-popover-template-popup].popover.right-top > .arrow,[uib-popover-template-popup].popover.right-bottom > .arrow{top:auto;bottom:auto;left:auto;right:auto;margin:0;}[uib-popover-popup].popover,[uib-popover-html-popup].popover,[uib-popover-template-popup].popover{display:block !important;}</style>'), angular.$$uibTooltipCss = !0
});