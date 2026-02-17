// ItemPurchaseForDialog/ItemPurchaseForDialog.js
var Roblox = Roblox || {};
Roblox.ItemPurchase = function(n, t, i) {
    function h(n) {
        n += "";
        for (var i = n.split("."), t = i[0], u = i.length > 1 ? "." + i[1] : "", r = /(\d+)(\d{3})/; r.test(t);) t = t.replace(r, "$1,$2");
        return t + u
    }

    function k(n) {
        return n < 1 ? n + "" : n < 1e4 ? h(n) : n >= 1e6 ? Math.floor(n / 1e6) + "M+" : Math.floor(n / 1e3) + "K+"
    }

    function l() {
        window.location.href = Roblox.Endpoints.getAbsoluteUrl("/login/Default.aspx") + "?ReturnUrl=" + encodeURIComponent(location.pathname + location.search)
    }

    function d(n) {
        return n.toUpperCase() === a ? a : n
    }

    function s(n, f) {
        var e = $(n),
            b;
        if (e.attr("data-modal-field-validation-required") === "true" && (f = ""), Roblox.Dialog.toggleProcessing(!0), !e.hasClass("btn-disabled-primary")) {
            var c = e.attr("data-product-id"),
                l = parseInt(e.attr("data-expected-price")),
                a = e.attr("data-expected-currency"),
                s = e.attr("data-placeproductpromotion-id"),
                p = e.attr("data-expected-seller-id"),
                h = e.attr("data-userasset-id");
            if (t) return t({
                productId: c,
                expectedPrice: l,
                expectedCurrency: a,
                expectedPromoId: s,
                expectedSellerId: p,
                userAssetId: h
            });
            b = Roblox.Endpoints.getAbsoluteUrl("/API/Item.ashx") + "?rqtype=purchase&productID=" + c + "&expectedCurrency=" + a + "&expectedPrice=" + l + (s === undefined ? "" : "&expectedPromoID=" + s) + "&expectedSellerID=" + p + (h === undefined ? "" : "&userAssetID=" + h), $.ajax({
                type: "POST",
                url: b,
                contentType: "application/json; charset=utf-8",
                success: function(n) {
                    n.statusCode == 500 ? (Roblox.Dialog.toggleProcessing(!1, f), o(n)) : (Roblox.Dialog.toggleProcessing(!1, f), i ? w() : y(n))
                },
                error: function(n) {
                    if (Roblox.Dialog.toggleProcessing(!1, f), $.modal.close(".ProcessingView"), n.responseText === "Bad Request") Roblox.Dialog.open({
                        titleText: r ? r.f(u["Heading.ErrorOccured"]) : Roblox.ItemPurchase.strings.errorOccured,
                        bodyContent: r ? r.f(u["Message.PurchasingUnavailable"]) : Roblox.ItemPurchase.strings.purchasingUnavailable,
                        imageUrl: v,
                        acceptText: r ? r.f(u["Action.Ok"]) : Roblox.ItemPurchase.strings.okText,
                        acceptColor: Roblox.Dialog.white,
                        declineColor: Roblox.Dialog.none,
                        dismissable: !0,
                        allowHtmlContentInBody: !0
                    });
                    else {
                        var t = $.parseJSON(n.responseText);
                        o(t)
                    }
                }
            })
        }
    }

    function o(n) {
        var i, t;
        if (n.showDivID === "TransactionFailureView") Roblox.Dialog.open({
            titleText: n.title,
            bodyContent: n.errorMsg,
            imageUrl: v,
            acceptText: r ? r.f(u["Action.Ok"]) : Roblox.ItemPurchase.strings.okText,
            acceptColor: Roblox.Dialog.white,
            declineColor: Roblox.Dialog.none,
            dismissable: !0,
            allowHtmlContentInBody: !0
        });
        else if (n.showDivID === "InsufficientFundsView") i = "<span class='icon-robux-16x16'></span><span class='text-robux'>" + Roblox.NumberFormatting.commas(n.shortfallPrice) + "</span>", Roblox.Dialog.open({
            titleText: r ? r.f(u["Heading.InsufficientFunds"]) : Roblox.ItemPurchase.strings.insufficientFundsTitle,
            bodyContent: r ? r.f(u["Message.InsufficientFunds"], {
                robux: i
            }) : Roblox.ItemPurchase.strings.insufficientFundsText.format(Roblox.NumberFormatting.commas(n.shortfallPrice)),
            declineText: r ? r.f(u["Action.Cancel"]) : Roblox.ItemPurchase.strings.cancelText,
            acceptText: r ? r.f(u["Action.BuyRobux"]) : Roblox.ItemPurchase.strings.buyText + " Robux",
            acceptColor: Roblox.Dialog.green,
            onAccept: function() {
                return window.location = Roblox.Endpoints.getAbsoluteUrl("/Upgrades/Robux.aspx") + "?ctx=" + n.source, !1
            },
            imageUrl: b,
            allowHtmlContentInBody: !0,
            allowHtmlContentInFooter: !1,
            fieldValidationRequired: !0,
            dismissable: !0,
            xToCancel: !0
        });
        else if (n.showDivID === "PriceChangedView") {
            t = "targetSelector" in n ? $(n.targetSelector) : $(".PurchaseButton[data-item-id=" + n.AssetID + "][data-expected-currency=" + n.expectedCurrency + "][data-expected-price=" + n.expectedPrice + "]");
            var f = function() {
                    t.attr("data-expected-price", n.currentPrice), t.attr("data-expected-currency", n.currentCurrency), s(t, "PurchaseVerificationView")
                },
                e = "<span class='icon-robux-gray-16x16'></span>" + n.expectedPrice,
                o = "<span class='icon-robux-gray-16x16'></span>" + n.currentPrice,
                h = "<span class='icon-robux-gray-16x16'></span>" + n.balanceAfterSale;
            Roblox.Dialog.open({
                titleText: r ? r.f(u["Heading.PriceChanged"]) : Roblox.ItemPurchase.strings.priceChangeTitle,
                bodyContent: r ? r.f(u["Message.PriceChanged"], {
                    robuxBefore: e,
                    robuxAfter: o
                }) : Roblox.ItemPurchase.strings.priceChangeText.format(n.expectedPrice, n.currentPrice),
                acceptText: r ? r.f(u["Action.BuyNow"]) : Roblox.ItemPurchase.strings.buyNowText,
                acceptColor: Roblox.Dialog.green,
                onAccept: f,
                declineText: r ? r.f(u["Action.Cancel"]) : Roblox.ItemPurchase.strings.cancelText,
                footerText: r ? r.f(u["Message.BalanceAfter"], {
                    robuxBalance: h
                }) : Roblox.ItemPurchase.strings.balanceText.format(n.balanceAfterSale),
                allowHtmlContentInBody: !0,
                allowHtmlContentInFooter: !0,
                dismissable: !0,
                checkboxAgreementText: r ? r.f(u["Label.AgreeAndPay"]) : Roblox.ItemPurchase.strings.agreeAndPayText,
                checkboxAgreementRequired: !0
            })
        }
    }

    function tt(n, t) {
        var i, gt, yt, ni, a, st, e, ht, w, at, b, ot, y, lt, ct, pt, ti;
        if (t = typeof t != "undefined" ? t : "item", i = $(n), !i.hasClass("btn-disabled-primary")) {
            if (it) {
                o({
                    showDivID: "TransactionFailureView",
                    title: "Error",
                    errorMsg: p
                });
                return
            }
            if (gt = i.attr("data-bc-requirement"), gt > g && c === "False") {
                showBCOnlyModal("BCOnlyModal");
                return
            }
            var dt = i.attr("data-item-name").escapeHTML(),
                v = parseInt(i.attr("data-expected-price")),
                ri = i.attr("data-expected-currency"),
                kt = d(i.attr("data-seller-name")),
                ut = i.attr("data-asset-type"),
                ii = i.attr("data-item-id"),
                bt = $("#ItemPurchaseAjaxData").attr("data-footer-text"),
                ft = bt == null ? "" : bt;
            if ($("#ItemPurchaseAjaxData").attr("data-footer-text", null), f = ut == "Place", c === "True") {
                l();
                return
            }
            var wt = i.hasClass("rentable"),
                et = parseInt(nt) - v,
                k = "",
                tt = "",
                vt = "",
                rt = "";
            if (wt ? (k = Roblox.ItemPurchase.strings.rentText, tt = r ? r.f(u["Heading.RentItem"]) : Roblox.ItemPurchase.strings.rentItemTitle, rt = r ? r.f(u["Action.RentNow"]) : Roblox.ItemPurchase.strings.rentNowText) : v === 0 ? (k = Roblox.ItemPurchase.strings.getText, tt = r ? r.f(u["Heading.GetItem"]) : Roblox.ItemPurchase.strings.getItemTitle, rt = r ? r.f(u["Action.GetNow"]) : Roblox.ItemPurchase.strings.getNowText) : (k = Roblox.ItemPurchase.strings.buyTextLower, tt = r ? r.f(u["Heading.BuyItem"]) : Roblox.ItemPurchase.strings.buyItemTitle, rt = r ? r.f(u["Action.BuyNow"]) : Roblox.ItemPurchase.strings.buyNowText), et < 0) {
                yt = {
                    shortfallPrice: 0 - et,
                    currentCurrency: ri,
                    showDivID: "InsufficientFundsView",
                    isPlace: f,
                    source: t
                }, o(yt);
                return
            }
            ni = $("#ItemPurchaseAjaxData").attr("data-imageurl"), a = "", v == 0 ? (st = r ? r.f(u["Label.Free"]) : Roblox.ItemPurchase.strings.freeText, a = "<span class='text-robux'>" + st + "</span>") : a = "<span class='icon-robux-16x16'></span><span class='text-robux'>" + Roblox.NumberFormatting.commas(v) + "</span>", e = "", e += h(et), ht = function() {
                return s(n, "PurchaseVerificationView")
            }, w = i.attr("data-purchase-title-text"), w = w ? w : tt, r && (at = "<span class='font-bold'>" + dt + "</span>", ot = {
                assetName: at,
                assetType: ut,
                seller: kt,
                robux: a
            }, b = wt ? f ? "Message.PromptRentAccess" : "Message.PromptRent" : v === 0 ? f ? "Message.PromptGetAccess" : "Message.PromptGet" : f ? "Message.PromptBuyAccess" : "Message.PromptBuy", vt = r.f(u[b], ot)), y = i.attr("data-purchase-body-content"), lt = r ? vt : Roblox.ItemPurchase.strings.buyItemText.format(k, dt, ut, kt, a, f ? Roblox.ItemPurchase.strings.accessText : ""), y = y ? y.format(a) : lt, ct = i.attr("data-modal-field-validation-required") === "true", pt = r ? r.f(u["Action.BuyAccess"]) : Roblox.ItemPurchase.strings.buyAccessText, r && ft.length === 0 && (e = "<span class='icon-robux-gray-16x16'></span>" + e), ti = r ? r.f(u["Message.BalanceAfter"], {
                robuxBalance: e
            }) : Roblox.ItemPurchase.strings.balanceText.format(e), Roblox.Dialog.open({
                titleText: w,
                bodyContent: y,
                imageUrl: ni,
                xToCancel: !0,
                acceptText: f ? pt : rt,
                acceptColor: Roblox.Dialog.green,
                onAccept: ht,
                declineText: r ? r.f(u["Action.Cancel"]) : Roblox.ItemPurchase.strings.cancelText,
                footerText: ft.length === 0 ? ti : ft.format(e),
                allowHtmlContentInBody: !0,
                allowHtmlContentInFooter: !0,
                dismissable: !0,
                fieldValidationRequired: ct,
                cssClass: "need-padding",
                onOpenCallback: function() {
                    if ($(".modal-confirmation .roblox-item-image").html("").attr("data-item-id", ii), Roblox.require("Widgets.ItemImage", function(n) {
                            n.load($(".modal-confirmation .roblox-item-image"))
                        }), Roblox.ModalEvents && i) {
                        var n = i.attr("data-button-type");
                        n && Roblox.ModalEvents.SendAssetPurchaseConfirmationShown("assetPurchaseConfirmation", n)
                    }
                }
            })
        }
    }

    function w() {
        var n = $(".system-feedback .alert-success");
        n.length > 0 && Roblox.BootstrapWidgets && (Roblox.BootstrapWidgets.ToggleSystemMessage(n, 100, 1e3), setTimeout(function() {
            window.location.reload()
        }, 1e3))
    }

    function y(t) {
        var e, a, l, v, y, i;
        t.Price == 0 ? (a = r ? r.f(u["Label.Free"]) : Roblox.ItemPurchase.strings.freeText, e = "<span class='text-robux'>" + a + "</span>") : e = "<span class='icon-robux-16x16'></span><span class='text-robux'>" + t.Price + "</span>";
        var p = $("#ItemPurchaseAjaxData").attr("data-imageurl"),
            w = function() {
                var n = $("#ItemPurchaseAjaxData").attr("data-continueshopping-url");
                n != undefined && n != "" && (document.location.href = n)
            },
            b = function() {
                var n = Roblox.Endpoints.getAbsoluteUrl("/my/avatar");
                n != undefined && n != "" && (document.location.href = n)
            },
            o = "",
            s = r ? r.f(u["Action.Continue"]) : Roblox.ItemPurchase.strings.continueText,
            h = Roblox.Dialog.none,
            k = f ? Roblox.ItemPurchase.strings.accessText : "",
            c;
        t.AssetType === "VIP Server" ? (o = r ? r.f(u["Action.Configure"]) : Roblox.ItemPurchase.strings.continueShoppingText, s = r ? r.f(u["Action.NotNow"]) : Roblox.ItemPurchase.strings.notNowText, h = Roblox.Dialog.blue, c = w) : t.AssetIsWearable && (o = r ? r.f(u["Action.Customize"]) : Roblox.ItemPurchase.strings.customizeCharacterText, h = Roblox.Dialog.white, s = r ? r.f(u["Action.NotNow"]) : Roblox.ItemPurchase.strings.notNowText, c = b), l = Roblox.ItemPurchase.strings.purchaseCompleteText.format(t.TransactionVerb, t.AssetName.escapeHTML(), t.AssetType, t.SellerName, e, k), r && (v = "<span class='font-bold'>" + t.AssetName.escapeHTML() + "</span>", y = {
            assetName: v,
            assetType: t.AssetType,
            seller: t.SellerName,
            robux: e
        }, i = t.TransactionVerb === "bought" ? f ? "Message.SuccessfullyBoughtAccess" : "Message.SuccessfullyBought" : t.TransactionVerb === "rented" ? f ? "Message.SuccessfullyRentedAccess" : "Message.SuccessfullyRented" : t.TransactionVerb === "renewed" ? f ? "Message.SuccessfullyRenewedAccess" : "Message.SuccessfullyRenewed" : f ? "Message.SuccessfullyAcquiredAccess" : "Message.SuccessfullyAcquired", l = r.f(u[i], y)), Roblox.Dialog.open({
            titleText: r ? r.f(u["Heading.PurchaseComplete"]) : Roblox.ItemPurchase.strings.purchaseCompleteTitle,
            bodyContent: l,
            imageUrl: p,
            acceptText: o,
            declineText: s,
            xToCancel: !0,
            onAccept: c,
            onDecline: function() {
                window.location.reload()
            },
            acceptColor: h,
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
    var r = Roblox.I18nData && (Roblox.I18nData.isI18nForItemEnabled || Roblox.I18nData.isI18nForGameDetailEnabled) ? new Roblox.Intl : !1,
        u = Roblox.I18nData && (Roblox.I18nData.isI18nForItemEnabled || Roblox.I18nData.isI18nForGameDetailEnabled) ? Roblox.Lang.itemPurchaseForDialog : null,
        e = $("#ItemPurchaseAjaxData"),
        c = e.attr("data-authenticateduser-isnull"),
        nt = e.attr("data-user-balance-robux"),
        g = e.attr("data-user-bc"),
        v = e.attr("data-alerturl"),
        b = e.attr("data-inSufficentFundsurl"),
        f = !1,
        it = e.attr("data-has-currency-service-error") === "True",
        p = e.attr("data-currency-service-error-message"),
        a = "ROBLOX";
    return {
        purchaseItem: s,
        openPurchaseVerificationView: tt,
        openPurchaseConfirmationView: y,
        redirectToLogin: l,
        purchaseConfirmationCallback: n,
        openErrorView: o,
        addCommasToMoney: h,
        formatMoney: k
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
    purchasingUnavailable: "Purchasing is temporarily unavailable. Please try again later.",
    agreeAndPayText: "Agree and Pay"
};