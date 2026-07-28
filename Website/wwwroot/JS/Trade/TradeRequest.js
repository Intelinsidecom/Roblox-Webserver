// Trade/TradeRequest.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.Trade == "undefined" && (Roblox.Trade = {}), typeof Roblox.Trade.TradeRequestModal == "undefined" && (Roblox.Trade.TradeRequestModal = function() {
    function et() {
        n.modal(c), i.hide(), v = $(this).attr("tradesessionid"), o = $(this).attr("tradepartnerid"), s = ot($(this).parents("tr").find(".TradePartner").attr("tradepartnername")), n.find("div.roblox-avatar-image").attr("data-user-id", o), Roblox.Widgets.AvatarImage.load($("#TradeRequest .roblox-avatar-image").toArray()), e(f.pull)
    }

    function ot(n) {
        return n.length > 10 ? n.slice(0, 10) + "..." : n
    }

    function h() {
        n.modal(c), e(f.pull)
    }

    function t() {
        n.find(".OfferItems").html(""), n.find(".ViewButtonContainer").hide(), n.find(".ActionButtonContainer").hide(), n.find(".ReviewButtonContainer").hide(), n.find(".TradeRequestText").hide(), n.find(".OfferValue").text(""), n.find('.roblox-avatar-image[data-image-size="medium"]').html(""), $.modal.close()
    }

    function ct(n) {
        e(f.accept, n)
    }

    function p() {
        e(f.decline)
    }

    function lt(t) {
        var u, o, f, h, i, r, s, c, v, e, p, d, g, k;
        if (typeof t != "undefined") {
            for (n.data("trade-json", t), u = $.parseJSON(t), b = !1, vt(u.Expiration), o = 0, p = n.find(".unifiedModalContent"); typeof u.AgentOfferList[o] != "undefined";) {
                r = u.AgentOfferList[o], f = "", e = r.AgentID === Number(rt) ? 0 : 1, c = p.find(".OfferItems")[e], $(c).html(""), v = $(p.find(".OfferValue")[e]), v.text(""), v.text(r.OfferValue);
                for (s in r.OfferList) r.OfferList[s].UserAssetID === ut ? i = u.StatusType === "Finished" ? tt.clone() : it.clone() : (i = nt.clone(), h = new Roblox.InventoryItem(i), h.display(r.OfferList[s]), i.find('[fieldname = "InventoryItemSize"]').addClass(h.largeClassName), i.find(".FooterButtonPlaceHolder").remove(), i.find(".HeaderButtonPlaceHolder").replaceWith($(".RemoveFromOffertemplate").html()), i.find(".InventoryItemContainerOuter").attr("userassetid", r.OfferList[s].UserAssetID)), f += i.html();
                if (typeof r.OfferRobux != "undefined" && (r.OfferRobux === 0 ? (e === 1 && $('[data-js="feenote"]').hide(), i = l.clone(), f += i.html()) : (i = y.clone(), e === 1 ? ($('[data-js="feenote"]').show(), i.find(".RobuxItemAsterisk").show(), d = Number(r.OfferRobux) - Math.ceil(Number(r.OfferRobux) * Number(w)), i.find(".RobuxAmount").text(d)) : (i.find(".RobuxItemAsterisk").hide(), i.find(".RobuxAmount").text(r.OfferRobux)), f += i.html(), b = !0)), r.OfferList.length < a)
                    for (g = a - r.OfferList.length, k = 0; k < g; k++) i = l.clone(), f += i.html();
                $(c).html(f), o++
            }
            var isOutbound = Number(rt) === u.SenderID;
            at(u.StatusType, u.IsActive, isOutbound)
        }
    }

    function at(t, i, isOutbound) {
        var c, f, e, h;
        n.find(".ViewButtonContainer").toggle(!i || isOutbound), n.find(".ActionButtonContainer").toggle(i && !isOutbound), n.find(".ReviewButtonContainer").hide(), c = "Trade with ", f = {
            wouldHaveGiven: "ITEMS YOU WOULD HAVE GIVEN",
            wouldHaveReceived: "ITEMS YOU WOULD HAVE RECEIVED",
            gave: "ITEMS YOU GAVE",
            received: "ITEMS YOU RECEIVED",
            willGive: "ITEMS YOU WILL GIVE",
            willReceive: "ITEMS YOU WILL RECEIVE"
        }, n.find("p.TradeRequestText .TradePartnerName").attr("href", "/users/" + o + "/profile").text(s), e = n.find("p.TradeRequestText .TradeStatusText"), h = n.find("p.TradeExpiration");
        switch (t) {
            case "Open":
                e.text("has been opened."), n.find(".ReviewButtonContainer").toggle(!i), isOutbound || n.find(".ViewButtonContainer").hide(), u.text(f.willGive), r.text(f.willReceive), h.show();
                break;
            case "Finished":
                u.text(f.gave), r.text(f.received), e.text("was completed!"), h.hide();
                break;
            case "Expired":
                u.text(f.wouldHaveGiven), r.text(f.wouldHaveReceived), e.text("has expired."), h.hide();
                break;
            case "Pending":
                u.text(f.willGive), r.text(f.willReceive), e.text("is pending."), h.show();
                break;
            case "Rejected":
                u.text(f.wouldHaveGiven), r.text(f.wouldHaveReceived), e.text("was rejected."), h.hide();
                break;
            case "Declined":
                u.text(f.wouldHaveGiven), r.text(f.wouldHaveReceived), e.text("was declined."), h.hide();
                break;
            case "Countered":
                u.text(f.wouldHaveGiven), r.text(f.wouldHaveReceived), e.text("was countered."), h.hide()
        }
        g || (n.find(".ViewButtonContainer").show(), n.find(".ActionButtonContainer").hide(), n.find(".ReviewButtonContainer").hide()), n.find(".TradeRequestText").show()
    }

    function vt(t) {
        var i, r, f, u;
        t = new Date(Number(t.substring(6, 19))), f = +new Date, u = t - f, i = Math.floor(u / k), r = Math.floor(u / yt);
        var e = "in ",
            o = " days.",
            s = " hours.",
            h = " soon.";
        i > 1 ? n.find("span#TradeRequestExpiration").text(e + i + o) : r > 1 ? n.find("span#TradeRequestExpiration").text(e + r + s) : n.find("span#TradeRequestExpiration").text(h)
    }

    function ft(n, t, i) {
        a = n, g = t, w = i
    }

    function e(n, r) {
        var u = {
            TradeID: v,
            cmd: n
        };
        n === "maketrade" && (u.TradeJSON = r), $.ajax({
            type: "POST",
            url: ht,
            data: u,
            dataType: "json",
            success: function(r) {
                var u, e;
                typeof r != "undefined" && (d.toggle(!r.success), r.success === !0 ? n === f.pull ? lt(r.data) : n === f.accept ? (u = "You have accepted " + s + "'s trade request. The trade is now being processed by our system.", t(), i.text(u), i.show(), $(document).trigger("TradeUpdate")) : n === f.decline ? (e = "You have declined " + s + "'s trade request.", t(), i.text(e), i.show(), $(document).trigger("TradeUpdate")) : $(document).trigger("TradeUpdate") : n !== f.pull ? (i.text(r.msg), i.show()) : (t(), Roblox.GenericModal.open("Trade Error", st, r.msg + ".  Please try again later.", function() {})))
            }
        })
    }
    var c = {
            overlayClose: !0,
            escClose: !0,
            opacity: 80,
            overlayCss: {
                backgroundColor: "#000"
            }
        },
        ht = "/Trade/TradeHandler.ashx",
        st = "/images/Icons/img-alert.png",
        k = 864e5,
        yt = k / 24,
        ut = 0,
        f = {
            pull: "pull",
            decline: "decline",
            accept: "maketrade"
        },
        o, v, a, l, y, it, tt, nt, s, n, rt, u, r, d, i, b, g, w;
    return $(function() {
        n = $("#TradeRequest"), rt = n.attr("UserID"), u = n.find('[list-id="OfferList0"] h3.OfferHeader'), r = n.find('[list-id="OfferList1"] h3.OfferHeader'), d = n.find(".GenericModalErrorMessage"), i = $("#TradeItems_tab .status-confirm"), n.find("#ButtonAcceptTrade").live("click", function() {
            var i = n.data("trade-json");
            t(), Roblox.GenericConfirmation.open({
                titleText: "Accept Request",
                bodyContent: "Are you sure you want to accept this Trade?",
                onAccept: function() {
                    ct(i)
                },
                onDecline: h
            })
        }), n.find("#ButtonCounterTrade").live("click", function() {
            t(), window.open("/Trade/TradeWindow.aspx?TradeSessionId=" + v + "&TradePartnerID=" + o, "_blank", "scrollbars=0, height=608, width=914")
        }), n.find("#ButtonDeclineTrade").live("click", function() {
            t(), Roblox.GenericConfirmation.open({
                titleText: "Decline Request",
                bodyContent: "Are you sure you want to decline this Trade?",
                onAccept: p,
                onDecline: h,
                footerText: "Tired of lowball trades?<br />Update your Trade Quality setting on the Account page.",
                allowHtmlContentInFooter: !0
            })
        }), n.find("#ButtonCancelTrade").live("click", function() {
            t(), Roblox.GenericConfirmation.open({
                titleText: "Decline Request",
                bodyContent: "Are you sure you want to decline this Trade?",
                onAccept: p,
                onDecline: h
            })
        }), $("#TradeRequest [roblox-ok]").live("click", t), $(".ViewTradeLink").live("click", et), $("[TradeUpdater]").bind("click", function() {
            $(document).trigger("TradeUpdate")
        }), l = $("#BlankTemplate"), it = $("[missing-user-asset-template]"), tt = $("[deleted-user-asset-template]"), nt = $("#InventoryItemTemplate"), y = $("#RobuxTemplate")
    }), c.onClose = t, {
        initialize: ft
    }
}());