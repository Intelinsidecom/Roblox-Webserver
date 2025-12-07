// My/myitem.js
var MyItemPage = new function() {
    function s() {
        var r, u, o = e,
            f;
        return u = n != t ? $(".PricingField_Robux [type=text]").val() : n, f = Math.round(u * a), r = f > o ? f : o
    }

    function g(i) {
        var r;
        return r = n != t ? $(".PricingField_Robux [type=text]").val() : n, r - s(i)
    }

    function o(n) {
        n ? ($("#PlayerLimitDefault").hide(), $("#PlayerLimitOptions").show("clip", "slow")) : ($("#PlayerLimitOptions").hide(), $("#PlayerLimitDefault").show("clip", "slow"))
    }

    function p(n, t, i) {
        var r = $(n).val();
        $("#PricingError").hide(), $("#PricingErrorMax").hide(), isNaN(r) ? ($(n).val(r.replace(/\D/g, "")), $("#PricingError").show()) : r < t ? ($(n).val(t), $("#PricingError").show()) : i != null && r > i && ($(n).val(i), $("#PricingErrorMax").show())
    }

    function f() {
        n != t && ($('.PricingField_Robux [type="text"]').prop("disabled", !1), p('.PricingField_Robux [type="text"]', Math.max(e, n), t)), $(".MarketplaceFeeInRobuxLabel").html(s("Robux")), $(".UserProfitInRobuxLabel").html(g("Robux"))
    }

    function u() {
        if (!$(".PricingField_Robux [type=checkbox]").attr("checked")) {
            var t = $('.PricingField_Robux [type="text"]'),
                i = t.val();
            r || (r = i ? i : n), t.val(""), t.prop("disabled", !0), $(".MarketplaceFeeInRobuxLabel").text("0"), $(".UserProfitInRobuxLabel").text("0")
        }
    }

    function w() {
        k(".PricingField_Robux")
    }

    function b() {
        d(".PricingField_Robux")
    }

    function k(n) {
        $("input", n).prop("disabled") || $("input", n).prop("disabled", !0)
    }

    function d(n) {
        $("input", n).prop("disabled") && $("input", n).prop("disabled", !1)
    }

    function y() {
        v || $(".PricingField_Robux input").prop("readonly", !0)
    }

    function h() {
        Roblox.GenericConfirmation.open({
            titleText: Roblox.MyItem.strings.PriceChangingThrottledTitleText,
            bodyContent: Roblox.MyItem.strings.PriceChangingThrottledBodyContent,
            acceptText: Roblox.MyItem.strings.OKText,
            declineColor: Roblox.GenericConfirmation.none,
            imageUrl: Roblox.MyItem.AlertImageUrl
        })
    }
    var e, n, a, t, l, c, i, v, r;
    ResizeFieldSet = function(n, t) {
        var i = {
            to: {
                height: t
            }
        };
        $("#PlaceAccessOptionsField").css({
            height: t,
            display: "block"
        })
    }, this.SelectPlaceType_Public = function(n, t, i, r, u) {
        setTimeout(function() {
            o(!0)
        }, 1), $(n).hide(), $(t).show("clip", "fast"), $("#" + u).hide(), $(i).attr("checked") === !1 && $(r).attr("checked") === !1 && $(i).attr("checked", !0), $("#SellThisItem").show()
    }, this.SelectPlaceType_Personal = function(n, t, i, r) {
        return n ? (setTimeout(function() {
            o(!1)
        }, 1), $(i).hide("slow", function() {
            $(t).show("fast")
        }), $("#" + r).show(), $("#SellThisItem").hide(), !0) : (showBCOnlyModal("BCOnlyModalPersonalServer"), !1)
    }, this.Initialize = function(o, s, p, k, d, g, nt, tt) {
        var et;
        e = o, n = p, a = s, t = k, l = d, c = g, i = nt, v = tt;
        var ft = $(".SellThisItemRow").children('[type="checkbox"]'),
            ut = $(".PricingPanel"),
            it = $(".PricingField_Robux [type=checkbox]"),
            rt = $(".PricingField_Robux [type=text]"),
            st = $(".MarketplaceFeeInRobuxLabel"),
            ht = $(".UserProfitInRobuxLabel"),
            ot = $("#PayToPlayFAQ");
        r = rt.val() ? rt.val() : n, ft.attr("checked") ? (ut.show(), it.attr("checked") ? f() : u(), i && ($(".PlaceTypeOptions").hide(), $("#PlaceAccess").hide(), $("#PlaceOptions").hide(), $("#PlaceCopyProtection").hide(), $("#Comments").hide(), $(".BCOptions").hide())) : (ut.hide(), rt.attr("checked", !1), i && ($(".PlaceTypeOptions").show(), $("#PlaceAccess").show(), $("#PlaceOptions").show(), $("#PlaceCopyProtection").show(), $("#Comments").show(), $(".BCOptions").show())), y(), et = !1, $(".PersonalServerAccessCtrls input,select").change(function() {
            et = !0
        }), $(".papListRemoveUserIcon").click(function() {
            et = !0
        }), $(".SaveButton").click(function() {
            window.onbeforeunload = null
        }), window.onbeforeunload = function() {
            if (et) return "Your changes have not been saved."
        }, it.change(function() {
            if ($(this).prop("readonly")) return h(), !1;
            $(this).attr("checked") ? (rt.val(r), f()) : (r = rt.val(), u())
        }), rt.change(function() {
            f()
        }), rt.click(function() {
            if ($(this).prop("readonly")) return h(), !1
        }), $('.PublicDomainRow [type="checkbox"]').click(function() {
            $(this).attr("checked") ? (ft.attr("checked", !1), it.attr("checked", !1), ut.hide(), u()) : ft.attr("checked") && ut.show()
        }), ft.click(function() {
            if (!l) return showBCOnlyModal("BCOnlyModalSelling"), !1;
            if ($(this).attr("checked")) {
                if (c) return Roblox.GenericConfirmation.open({
                    titleText: Roblox.MyItem.strings.SellingSuspendedTitleText,
                    bodyContent: Roblox.MyItem.strings.SellingSuspendedBodyContent,
                    acceptText: Roblox.MyItem.strings.OKText,
                    declineColor: Roblox.GenericConfirmation.none,
                    imageUrl: Roblox.MyItem.AlertImageUrl
                }), !1;
                $('.PublicDomainRow [type="checkbox"]').attr("checked", !1), ut.show(), b(), it.prop("readonly") || u(), f(), i && ($(".PlaceTypeOptions").hide(), $("#PlaceAccess").hide(), $("#PlaceOptions").hide(), $("#PlaceCopyProtection").hide(), $("#Comments").hide(), $(".BCOptions").hide()), it.prop("readonly") || $(".PricingField_Robux").css("display") !== "none" || it.attr("checked", !0).change()
            } else w(), it.prop("readonly") || it.attr("checked", !1).change(), ut.hide(), i && ($(".PlaceTypeOptions").show(), $("#PlaceAccess").show(), $("#PlaceOptions").show(), $("#PlaceCopyProtection").show(), $("#Comments").show(), $(".BCOptions").show())
        }), ot.click(function() {
            return window.open(this.href, "PayToPlayFAQ", "left=100,top=100,width=500,height=500,toolbar=0,resizable=0,scrollbars=1"), !1
        })
    }
};