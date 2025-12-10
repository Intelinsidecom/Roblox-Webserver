// ItemPurchaseForDialog.js
var Roblox = Roblox || {};
Roblox.ItemPurchase = function(n, t, i) {
    function o(n) {
        n += "";
        for (var i = n.split("."), t = i[0], u = i.length > 1 ? "." + i[1] : "", r = /(\d+)(\d{3})/; r.test(t);) t = t.replace(r, "$1,$2");
        return t + u
    }

    function w(n) {
        return n < 1 ? n + "" : n < 1e4 ? o(n) : n >= 1e6 ? Math.floor(n / 1e6) + "M+" : Math.floor(n / 1e3) + "K+"
    }

    function s() {
        window.location.href = Roblox.Endpoints.getAbsoluteUrl("/login/Default.aspx") + "?ReturnUrl=" + encodeURIComponent(location.pathname + location.search)
    }

    function k(n) {
        return n.toUpperCase() === h ? h : n
    }

    function e(n, r) {
        var f = $(n),
            p;
        if (f.attr("data-modal-field-validation-required") === "true" && (r = ""), Roblox.Dialog.toggleProcessing(!0), !f.hasClass("btn-disabled-primary")) {
            var s = f.attr("data-product-id"),
                h = parseInt(f.attr("data-expected-price")),
                a = f.attr("data-expected-currency"),
                e = f.attr("data-placeproductpromotion-id"),
                y = f.attr("data-expected-seller-id"),
                o = f.attr("data-userasset-id");
            if (t) return t({
                productId: s,
                expectedPrice: h,
                expectedCurrency: a,
                expectedPromoId: e,
                expectedSellerId: y,
                userAssetId: o
            });
            p = Roblox.Endpoints.getAbsoluteUrl("/API/Item.ashx") + "?rqtype=purchase&productID=" + s + "&expectedCurrency=" + a + "&expectedPrice=" + h + (e === undefined ? "" : "&expectedPromoID=" + e) + "&expectedSellerID=" + y + (o === undefined ? "" : "&userAssetID=" + o), $.ajax({
                type: "POST",
                url: p,
                contentType: "application/json; charset=utf-8",
                success: function(n) {
                    n.statusCode == 500 ? (Roblox.Dialog.toggleProcessing(!1, r), u(n)) : (Roblox.Dialog.toggleProcessing(!1, r), i ? v() : c(n))
                },
                error: function(n) {
                    if (Roblox.Dialog.toggleProcessing(!1, r), $.modal.close(".ProcessingView"), n.responseText === "Bad Request") Roblox.Dialog.open({
                        titleText: Roblox.ItemPurchase.strings.errorOccured,
                        bodyContent: Roblox.ItemPurchase.strings.purchasingUnavailable,
                        imageUrl: l,
                        acceptText: Roblox.ItemPurchase.strings.okText,
                        acceptColor: Roblox.Dialog.white,
                        declineColor: Roblox.Dialog.none,
                        dismissable: !0,
                        allowHtmlContentInBody: !0
                    });
                    else {
                        var t = $.parseJSON(n.responseText);
                        u(t)
                    }
                }
            })
        }
    }

    function u(n) {
        var t, r;
        if (n.showDivID === "TransactionFailureView") Roblox.Dialog.open({
            titleText: n.title,
            bodyContent: n.errorMsg,
            imageUrl: l,
            acceptText: Roblox.ItemPurchase.strings.okText,
            acceptColor: Roblox.Dialog.white,
            declineColor: Roblox.Dialog.none,
            dismissable: !0,
            allowHtmlContentInBody: !0
        });
        else if (n.showDivID === "InsufficientFundsView") {
            var i = "",
                u = "",
                f = !1;
            i = n.currentCurrency == 1 ? Roblox.ItemPurchase.strings.buyText + " ROBUX" : Roblox.ItemPurchase.strings.tradeCurrencyText, Roblox.Dialog.open({
                titleText: Roblox.ItemPurchase.strings.insufficientFundsTitle,
                bodyContent: Roblox.ItemPurchase.strings.insufficientFundsText.format(Roblox.NumberFormatting.commas(n.shortfallPrice)),
                acceptText: i,
                acceptColor: Roblox.Dialog.green,
                onAccept: function() {
                    return window.location = n.currentCurrency == 1 ? Roblox.Endpoints.getAbsoluteUrl("/Upgrades/Robux.aspx") + "?ctx=" + n.source : Roblox.Endpoints.getAbsoluteUrl("/My/Money.aspx") + "?tab=TradeCurrency", !1
                },
                declineText: Roblox.ItemPurchase.strings.cancelText,
                imageUrl: nt,
                footerText: u,
                allowHtmlContentInBody: !0,
                allowHtmlContentInFooter: f,
                fieldValidationRequired: !0,
                dismissable: !0,
                xToCancel: !0
            })
        } else n.showDivID === "PriceChangedView" && (t = "targetSelector" in n ? $(n.targetSelector) : $(".PurchaseButton[data-item-id=" + n.AssetID + "][data-expected-currency=" + n.expectedCurrency + "][data-expected-price=" + n.expectedPrice + "]"), r = function() {
            t.attr("data-expected-price", n.currentPrice), t.attr("data-expected-currency", n.currentCurrency), e(t, "PurchaseVerificationView")
        }, Roblox.Dialog.open({
            titleText: Roblox.ItemPurchase.strings.priceChangeTitle,
            bodyContent: Roblox.ItemPurchase.strings.priceChangeText.format(n.expectedPrice, n.currentPrice),
            acceptText: Roblox.ItemPurchase.strings.buyNowText,
            acceptColor: Roblox.Dialog.green,
            onAccept: r,
            declineText: Roblox.ItemPurchase.strings.cancelText,
            footerText: Roblox.ItemPurchase.strings.balanceText.format(n.balanceAfterSale),
            allowHtmlContentInBody: !0,
            allowHtmlContentInFooter: !0,
            dismissable: !0
        }))
    }

    function g(n, t) {
        var i, lt, ut, rt, ot, st, h, nt, ct, r, c, vt;
        if (t = typeof t != "undefined" ? t : "item", i = $(n), !i.hasClass("btn-disabled-primary")) {
            if (tt) {
                u({
                    showDivID: "TransactionFailureView",
                    title: "Error",
                    errorMsg: y
                });
                return
            }
            if (lt = i.attr("data-bc-requirement"), lt > p && a === "False") {
                showBCOnlyModal("BCOnlyModal");
                return
            }
            var wt = i.attr("data-item-name").escapeHTML(),
                l = parseInt(i.attr("data-expected-price")),
                ht = i.attr("data-expected-currency"),
                pt = k(i.attr("data-seller-name")),
                et = i.attr("data-asset-type"),
                yt = i.attr("data-item-id"),
                ft = $("#ItemPurchaseAjaxData").attr("data-footer-text"),
                at = ft == null ? "" : ft;
            if ($("#ItemPurchaseAjaxData").attr("data-footer-text", null), f = et == "Place", a === "True") {
                s();
                return
            }
            ut = !1, i.hasClass("rentable") && (ut = !0), rt = ht == "1" ? parseInt(d) : parseInt(b);
            var it = rt - l,
                g = "",
                w = "",
                v = "";
            if (ut ? (g = Roblox.ItemPurchase.strings.rentText, w = Roblox.ItemPurchase.strings.rentItemTitle, v = Roblox.ItemPurchase.strings.rentNowText) : l == 0 ? (g = Roblox.ItemPurchase.strings.getText, w = Roblox.ItemPurchase.strings.getItemTitle, v = Roblox.ItemPurchase.strings.getNowText) : (g = Roblox.ItemPurchase.strings.buyTextLower, w = Roblox.ItemPurchase.strings.buyItemTitle, v = Roblox.ItemPurchase.strings.buyNowText), it < 0) {
                ot = {
                    shortfallPrice: 0 - it,
                    currentCurrency: ht,
                    showDivID: "InsufficientFundsView",
                    isPlace: f,
                    source: t
                }, u(ot);
                return
            }
            st = $("#ItemPurchaseAjaxData").attr("data-imageurl"), h = "", h = l == 0 ? "<span class='text-robux'>" + Roblox.ItemPurchase.strings.freeText + "</span>" : "<span class='icon-robux-16x16'></span><span class='text-robux'>" + Roblox.NumberFormatting.commas(l) + "</span>", nt = "", nt += o(it), ct = function() {
                return e(n, "PurchaseVerificationView")
            }, r = i.attr("data-purchase-title-text"), r = r ? r : w, c = i.attr("data-purchase-body-content"), c = c ? c.format(h) : Roblox.ItemPurchase.strings.buyItemText.format(g, wt, et, pt, h, f ? Roblox.ItemPurchase.strings.accessText : ""), vt = i.attr("data-modal-field-validation-required") === "true", Roblox.Dialog.open({
                titleText: r,
                bodyContent: c,
                imageUrl: st,
                xToCancel: !0,
                acceptText: f ? Roblox.ItemPurchase.strings.buyAccessText : v,
                acceptColor: Roblox.Dialog.green,
                onAccept: ct,
                declineText: Roblox.ItemPurchase.strings.cancelText,
                footerText: (at.length == 0 ? Roblox.ItemPurchase.strings.balanceText : at).format(nt),
                allowHtmlContentInBody: !0,
                allowHtmlContentInFooter: !0,
                dismissable: !0,
                fieldValidationRequired: vt,
                cssClass: "need-padding",
                onOpenCallback: function() {
                    $(".modal-confirmation .roblox-item-image").html("").attr("data-item-id", yt), Roblox.require("Widgets.ItemImage", function(n) {
                        n.load($(".modal-confirmation .roblox-item-image"))
                    })
                }
            })
        }
    }

    function v() {
        var n = $(".system-feedback .alert-success");
        n.length > 0 && Roblox.BootstrapWidgets && (Roblox.BootstrapWidgets.ToggleSystemMessage(n, 100, 1e3), setTimeout(function() {
            window.location.reload()
        }, 1e3))
    }

    function c(t) {
        var i;
        i = t.Price == 0 ? "<span class='text-robux'>" + Roblox.ItemPurchase.strings.freeText + "</span>" : "<span class='icon-robux-16x16'></span><span class='text-robux'>" + t.Price + "</span>";
        var s = $("#ItemPurchaseAjaxData").attr("data-imageurl"),
            h = function() {
                var n = $("#ItemPurchaseAjaxData").attr("data-continueshopping-url");
                n != undefined && n != "" && (document.location.href = n)
            },
            c = function() {
                var n = Roblox.Endpoints.getAbsoluteUrl("/My/Character.aspx");
                n != undefined && n != "" && (document.location.href = n)
            },
            r = "",
            u = Roblox.ItemPurchase.strings.continueText,
            e = Roblox.Dialog.none,
            l = f ? Roblox.ItemPurchase.strings.accessText : "",
            o;
        t.AssetType === "VIP Server" ? (r = Roblox.ItemPurchase.strings.continueShoppingText, u = Roblox.ItemPurchase.strings.notNowText, e = Roblox.Dialog.blue, o = h) : t.AssetIsWearable && (r = Roblox.ItemPurchase.strings.customizeCharacterText, e = Roblox.Dialog.white, u = Roblox.ItemPurchase.strings.notNowText, o = c), Roblox.Dialog.open({
            titleText: Roblox.ItemPurchase.strings.purchaseCompleteTitle,
            bodyContent: Roblox.ItemPurchase.strings.purchaseCompleteText.format(t.TransactionVerb, t.AssetName, t.AssetType, t.SellerName, i, l),
            imageUrl: s,
            acceptText: r,
            declineText: u,
            xToCancel: !0,
            onAccept: o,
            onDecline: function() {
                window.location.reload()
            },
            acceptColor: e,
            declineColor: Roblox.Dialog.white,
            allowHtmlContentInBody: !0,
            dismissable: !0,
            onOpenCallback: function() {
                $(".modal-confirmation .roblox-item-image").html("").attr("data-item-id", t.AssetID), Roblox.require("Widgets.ItemImage", function(n) {
                    n.load($(".modal-confirmation .roblox-item-image"))
                })
            }
        }), n(t)
    }
    if (!(this instanceof Roblox.ItemPurchase)) return new Roblox.ItemPurchase(n, t, i);
    t = typeof t == "undefined" ? !1 : t;
    var r = $("#ItemPurchaseAjaxData"),
        a = r.attr("data-authenticateduser-isnull"),
        d = r.attr("data-user-balance-robux"),
        b = r.attr("data-user-balance-tickets"),
        p = r.attr("data-user-bc"),
        l = r.attr("data-alerturl"),
        nt = r.attr("data-inSufficentFundsurl"),
        f = !1,
        tt = r.attr("data-has-currency-service-error") === "True",
        y = r.attr("data-currency-service-error-message"),
        h = "ROBLOX";
    return {
        purchaseItem: e,
        openPurchaseVerificationView: g,
        openPurchaseConfirmationView: c,
        redirectToLogin: s,
        purchaseConfirmationCallback: n,
        openErrorView: u,
        addCommasToMoney: o,
        formatMoney: w
    }
}, Roblox.ItemPurchase.ModalClose = function(n) {
    $.modal.close("." + n)
}, Roblox.ItemPurchase.strings = {
    insufficientFundsTitle: "Insufficient Funds",
    insufficientFundsText: "You need <span class='icon-robux-16x16'></span><span class='text-robux'>{0}</span> more to purchase this item. ",
    cancelText: "Cancel",
    okText: "OK",
    buyText: "Buy",
    buyTextLower: "buy",
    tradeCurrencyText: "Trade Currency",
    priceChangeTitle: "Item Price Has Changed",
    priceChangeText: "While you were shopping, the price of this item changed from <span class='icon-robux-gray-16x16'></span>{0} to <span class='icon-robux-gray-16x16'></span>{1}.",
    buyNowText: "Buy Now",
    buyAccessText: "Buy Access",
    buildersClubOnlyTitle: "{0} Only",
    buildersClubOnlyText: "You need {0} to buy this item.",
    buyItemTitle: "Buy Item",
    buyItemText: "Would you like to {0} {5}the {2}: <span class='font-bold'>{1}</span> from {3} for {4}?",
    balanceText: "Your balance after this transaction will be <span class='icon-robux-gray-16x16'></span>{0}",
    freeText: "Free",
    purchaseCompleteTitle: "Purchase Complete",
    purchaseCompleteText: "You have successfully {0} {5}the <span class='font-bold'>{1}</span> {2} from {3} for {4}.",
    continueShoppingText: "Configure",
    notNowText: "Not Now",
    continueText: "Continue",
    customizeCharacterText: "Customize",
    orText: "or",
    rentItemTitle: "Rent Item",
    rentText: "rent",
    rentNowText: "Rent Now",
    getItemTitle: "Get Item",
    getText: "get",
    getNowText: "Get Now",
    accessText: "access to ",
    invalidLinkTitle: "Invalid Link",
    invalidLinkText: "This VIP Server link is no longer valid.",
    errorOccured: "Error Occured",
    purchasingUnavailable: "Purchasing is temporarily unavailable. Please try again later."
};