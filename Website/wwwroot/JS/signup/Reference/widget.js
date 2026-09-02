// Reference/widget.js
var Freebloxia = Freebloxia || {};
Freebloxia.BootstrapWidgets = function() {
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
    Freebloxia.BootstrapWidgets.SetupTabs(), Freebloxia.BootstrapWidgets.SetupDropdown(), Freebloxia.BootstrapWidgets.SetupAccordion(), Freebloxia.BootstrapWidgets.SetupTooltip(), Freebloxia.BootstrapWidgets.CloseTooltip(), Freebloxia.BootstrapWidgets.SetupPopover(), Freebloxia.BootstrapWidgets.ClosePopover(), Freebloxia.BootstrapWidgets.SetupScrollbar(), Freebloxia.BootstrapWidgets.SetupPagination(), typeof Modernizr == "undefined" || Modernizr.input.placeholder || Freebloxia.BootstrapWidgets.Placeholder(), Freebloxia.BootstrapWidgets.IsTruncated(), Freebloxia.BootstrapWidgets.TruncateParagraph(), Freebloxia.BootstrapWidgets.ToggleParagraph(), Freebloxia.BootstrapWidgets.SetupCarousel(), Freebloxia.BootstrapWidgets.SetupToggleButton(), Freebloxia.BootstrapWidgets.SetupSystemFeedback(), Freebloxia.BootstrapWidgets.ToggleSystemMessage(), Freebloxia.BootstrapWidgets.SetupVerticalMenu(), Freebloxia.BootstrapWidgets.TruncateContent(), Freebloxia.BootstrapWidgets.ToggleContent()
});