// Item/Resellers.js
"use strict";
var Roblox = window.Roblox || {};
Roblox.Resellers = function() {
    function w(t) {
        n.successBanner.text(t), Roblox.BootstrapWidgets.ToggleSystemMessage(n.successBanner, p, f)
    }

    function i(t) {
        n.errorBanner.text(t), Roblox.BootstrapWidgets.ToggleSystemMessage(n.errorBanner, p, f)
    }

    function k(i, r) {
        var u = n.resellerTemplate.clone().removeClass("reseller-template"),
            f, e, o;
        return u[0] ? (f = "Serial ", f += i.SerialNumber ? "#" + Roblox.NumberFormatting.commas(i.SerialNumber) + " of " + Roblox.NumberFormatting.commas(r) : "N/A", u.find(".avatar").attr("href", i.ProfileUrl), u.find(".avatar-card-image").attr("src", i.Thumbnail.Url).attr("alt", i.SellerName), u.find(".username").attr("href", i.ProfileUrl).text(i.SellerName), u.find(".serial-number").text(f), e = i.Price, u.find(".text-robux").text(e < 1e9 ? Roblox.NumberFormatting.commas(e) : Roblox.NumberFormatting.abbreviatedFormat(e)), o = u.find(".PurchaseButton"), o.attr({
            "data-expected-price": i.Price,
            "data-expected-seller-id": i.SellerId,
            "data-seller-name": i.SellerName,
            "data-userasset-id": i.UserAssetId,
            "data-bc-requirement": t.bcRequirement,
            "data-product-id": t.productId,
            "data-item-id": t.assetId,
            "data-item-name": t.name,
            "data-asset-type": t.assetType,
            "data-expected-currency": d
        }), i.SellerId === l && (o.attr("class", "remove-sale btn-control-md btn-fixed-width"), o.text("Remove")), u) : ""
    }

    function a(t) {
        t ? n.moreButton.show() : n.moreButton.hide()
    }

    function c() {
        n.spinner.show(), $.get(Roblox.Endpoints.getAbsoluteUrl("/asset/resellers"), {
            productId: t.productId,
            startIndex: u,
            maxRows: y
        }, function(t) {
            var i, r;
            if (t.isValid && t.data) {
                if (i = t.data, i.Resellers.length > 0)
                    for (n.content.show(), n.contentMessage.hide(), r = 0; r < i.Resellers.length; r++) n.list.append(k(i.Resellers[r], i.TotalAvailable));
                else u === 0 && (n.content.hide(), n.contentMessage.show());
                a(i.AreMoreAvailable)
            }
            n.spinner.hide()
        })
    }

    function h() {
        a(!1), u += y, c()
    }

    function s() {
        var t = $("#header").height();
        $(".resale-pricechart-tabs .nav-tabs").is(":visible") && $("[href=#resellers]").tab("show"), $("html, body").animate({
            scrollTop: n.container.offset().top - t
        }, 1e3, "swing")
    }

    function tt() {
        var i = n.sellSelection.find("option").length;
        i > 0 ? (n.sellSelectionContainer.toggleClass("hidden", !t.isLimitedUnique || i <= 1), n.sellContextButton.removeClass("hidden")) : n.sellContextButton.addClass("hidden")
    }

    function it() {
        var i = n.takeOffSaleSelection.find("option").length;
        i > 0 ? (n.takeOffSaleSelectionContainer.toggleClass("hidden", !t.isLimitedUnique || i <= 1), n.takeOffSaleContextButton.removeClass("hidden")) : n.takeOffSaleContextButton.addClass("hidden")
    }

    function o(n) {
        if (typeof n != "number" || isNaN(n) || n <= 0) {
            i(v);
            return
        }
        e(n, !1, 0, function(n) {
            n ? w(g) : i(v)
        })
    }

    function rt(n, t) {
        if (typeof t != "number" || isNaN(t) || t <= 0) {
            i(r);
            return
        }
        if (n = Number(n), isNaN(n) || n <= 0) {
            i(r);
            return
        }
        e(t, !0, n, function(n) {
            n ? w(nt) : i(r)
        })
    }

    function e(i, r, u, f) {
        var e = $("button[data-button-type='reseller'][data-userasset-id='" + i + "']");
        $.post(Roblox.Endpoints.getAbsoluteUrl("/asset/toggle-sale"), {
            assetId: t.assetId,
            userAssetId: i,
            price: u,
            sell: r
        }, function(t) {
            if (t.isValid) {
                var o = r ? n.sellSelection.find("option[value='" + i + "']") : n.takeOffSaleSelection.find("option[value='" + i + "']");
                r ? (o.length > 0 && n.takeOffSaleSelection.append(o), e.length > 0 && (e.removeAttr("disabled"), e.parent().slideDown(), e.parent().find(".text-robux").text(u < 1e9 ? Roblox.NumberFormatting.commas(u) : Roblox.NumberFormatting.abbreviatedFormat(u)))) : (o.length > 0 && n.sellSelection.append(o), e.length > 0 && e.parent().slideUp()), tt(), it(), f(!0)
            } else f(!1)
        }).fail(function() {
            f(!1)
        })
    }

    function ut() {
        n.container = $("#resellers"), n.list = n.container.find(".vlist"), n.moreButton = n.container.find(".see-more-resellers"), n.spinner = n.container.find(".loading-animated"), n.errorBanner = $(".content .alert-warning"), n.successBanner = $(".content .alert-success"), n.content = n.container.find(".section-content"), n.contentMessage = n.container.find(".section-content-off"), n.sellContextButton = $("#sell").parent(), n.takeOffSaleContextButton = $("#take-off-sale").parent(), n.sellSelection = $("#sell-modal-content .serial-dropdown"), n.takeOffSaleSelection = $("#take-off-sale-modal-content .serial-dropdown"), n.resellerTemplate = $(".reseller-template").clone(), n.sellSelectionContainer = n.sellSelection.parent().parent(), n.takeOffSaleSelectionContainer = n.takeOffSaleSelection.parent().parent(), $(".reseller-template .PurchaseButton").remove()
    }

    function b() {
        var n = $("#item-container");
        t.isPurchasingEnabled = n.data("is-purchase-enabled"), t.productId = n.data("product-id"), t.assetId = n.data("item-id"), t.bcRequirement = n.data("bc-requirement"), t.name = n.data("item-name"), t.assetType = n.data("asset-type"), t.isLimitedUnique = n.data("is-limited-unique"), l = n.data("user-id")
    }

    function ft() {
        ut(), b(), t.isPurchasingEnabled ? c() : (n.content.hide(), n.contentMessage.text("Purchasing is temporarily unavailable. Please try again later.").show()), $("#resellersLink").click(s);
        $("body").on("click", ".remove-sale[data-userasset-id]", function() {
            o($(this).data("userasset-id"))
        });
        n.moreButton.click(h)
    }
    var u = 0,
        y = 10,
        l = 0,
        t = {},
        n = {},
        f = 2e3,
        p = 100,
        r = "Failed to place on sale",
        nt = "Successfully placed on sale",
        v = "Failed to take off sale",
        g = "Successfully taken off sale",
        d = 1;
    return {
        init: ft,
        takeItemOffSale: o,
        sellItem: rt,
        loadMoreResellers: h,
        scrollToResellers: s
    }
}();