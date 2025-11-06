// Reference/widget.js
var Roblox = Roblox || {};
Roblox.BootstrapWidgets = function() {
    function i() {
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

    function b() {
        $('[data-toggle="dropdown-menu"] li').click(function(n) {
            var t = $(n.currentTarget);
            return t.closest(".input-group-btn").find('[data-bind="label"]').text(t.text()).end().toggleClass("open"), !1
        })
    }

    function w() {
        $('[data-toggle="collapsible-element"]').on("show.bs.collapse", function(n) {
            $(n.target).prev(".panel-heading").find(".icon-down-16x16").removeClass("icon-down-16x16").addClass("icon-up-16x16")
        });
        $('[data-toggle="collapsible-element"]').on("hide.bs.collapse", function(n) {
            $(n.target).prev(".panel-heading").find(".icon-up-16x16").removeClass("icon-up-16x16").addClass("icon-down-16x16")
        })
    }

    function p() {
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

    function y(n, t) {
        $(n).attr("title", t).tooltip("fixTitle")
    }

    function v() {
        $("body").on("click touchstart", function(n) {
            $('[data-toggle="tooltip"]').each(function() {
                if (!$(this).is(n.target) && $(this).has(n.target).length === 0) {
                    var t = n.type === "click" ? !0 : $(".tooltip").has(n.target).length === 0;
                    t && $(this).tooltip("hide")
                }
            })
        })
    }

    function a(n, t) {
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
        }).unbind().on("click", function() {
            $(this).popover("toggle")
        })
    }

    function l() {
        $("body").on("click touchstart", function(n) {
            $('[data-toggle="popover"]').each(function() {
                if (!$(this).is(n.target) && $(this).has(n.target).length === 0) {
                    var t = n.type === "click" ? !0 : $(".popover").has(n.target).length === 0;
                    t && $(this).popover("hide")
                }
            })
        })
    }

    function c() {
        $('[data-toggle="scrollbar"]').mCustomScrollbar({
            autoHideScrollbar: !1,
            autoExpandScrollbar: !1,
            scrollInertia: 0,
            mouseWheel: {
                preventDefault: !0
            }
        })
    }

    function h() {
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

    function s() {
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

    function o() {
        $("input[placeholder]").focus(function() {
            var n = $(this);
            n.val() == n.attr("placeholder") && (n.val(""), n.removeClass("rbx-placeholder"))
        }).blur(function() {
            var n = $(this);
            (n.val() == "" || n.val() == n.attr("placeholder")) && (n.addClass("rbx-placeholder"), n.val(n.attr("placeholder")))
        })
    }

    function f() {
        e.each(function() {
            var i = $(this),
                n = $(this).clone().hide().height("auto");
            n.width(i.width()), $("body").append(n), n.height() <= i.height() && (i.removeClass(t), $(this).find(".toggle-para").hide()), n.remove()
        })
    }

    function u(n, t) {
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

    function r(n, t) {
        var i = "para-overflow-toggle-off",
            r = "para-height";
        n || (n = "Read More"), t || (t = "Show Less"), $(".toggle-para").bind("click touchstart", function() {
            var u = $(".para-overflow-toggle");
            $(this).text() === n ? (u.removeClass(r).addClass(i), $(this).text(t)) : (u.removeClass(i).addClass(r), $(this).text(n))
        })
    }

    function k(n) {
        n = n ? n : "#carousel", $(n).carousel({
            interval: 6e3,
            pause: "hover"
        })
    }

    function d() {
        $(".btn-toggle").bind("click", function() {
            if ($(this).hasClass("disabled")) return !1;
            $(this).toggleClass("on"), $(this).trigger("toggleBtnClick", {
                id: $(this).attr("id"),
                toggleOn: $(this).hasClass("on")
            })
        })
    }
    var t = "para-overflow",
        e = $("." + t);
    return {
        SetupTabs: i,
        SetupDropdown: b,
        SetupAccordion: w,
        SetupTooltip: p,
        UpdateTooltip: y,
        CloseTooltip: v,
        SetupPopover: a,
        ClosePopover: l,
        SetupScrollbar: c,
        SetupPagination: h,
        Placeholder: o,
        IsTruncated: f,
        TruncateParagraph: u,
        ToggleParagraph: r,
        SetupCarousel: k,
        SetupToggleButton: d,
        SetupSystemFeedback: s,
        ToggleSystemMessage: n
    }
}(), $(function() {
    Roblox.BootstrapWidgets.SetupTabs(),
    Roblox.BootstrapWidgets.SetupDropdown(),
    Roblox.BootstrapWidgets.SetupAccordion(),
    Roblox.BootstrapWidgets.SetupTooltip(),
    (Roblox.BootstrapWidgets && typeof Roblox.BootstrapWidgets.CloseTooltip == "function" && Roblox.BootstrapWidgets.CloseTooltip()),
    Roblox.BootstrapWidgets.SetupPopover(),
    (Roblox.BootstrapWidgets && typeof Roblox.BootstrapWidgets.ClosePopover == "function" && Roblox.BootstrapWidgets.ClosePopover()),
    Roblox.BootstrapWidgets.SetupScrollbar(),
    Roblox.BootstrapWidgets.SetupPagination(),
    (typeof Modernizr == "undefined" || Modernizr.input.placeholder || Roblox.BootstrapWidgets.Placeholder()),
    Roblox.BootstrapWidgets.IsTruncated(),
    Roblox.BootstrapWidgets.TruncateParagraph(),
    Roblox.BootstrapWidgets.ToggleParagraph(),
    Roblox.BootstrapWidgets.SetupCarousel(),
    Roblox.BootstrapWidgets.SetupToggleButton(),
    Roblox.BootstrapWidgets.SetupSystemFeedback(),
    Roblox.BootstrapWidgets.ToggleSystemMessage()
});