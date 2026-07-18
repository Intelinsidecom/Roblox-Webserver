// MyMoney.js
$(function() {
    function r(r) {
        $.address.hash(r.attr("contentid")), $(".WhiteSquareTabsContainer .selected, #TabsContentContainer .selected").removeClass("selected"), r.addClass("selected"), $("#" + r.attr("contentid")).addClass("selected");
        var u = r.attr("contentid");
        u == "Summary_tab" ? o() : u == "MyTransactions_tab" ? ($(".TransactionsContainer table tr.datarow,.TransactionsContainer table tr.morerecords").remove(), n = 0, i()) : u == "TradeItems_tab" ? ($(".TradeItemsContainer table tr.datarow,.TradeItemsContainer table tr.morerecords").remove(), t()) : u == "Promotion_tab" && f()
    }

    function e(n) {
        return n = n == "" ? "&nbsp;" : n
    }

    function c(n) {
        return n.replace("-", ""), n == "" ? "&nbsp;" : n
    }

    function o() {
        $("#Summary_tab").append('<div class="loading"></div>');
        var n = $("#TimePeriod").val();
        $.ajax({
            type: "POST",
            url: "Money.aspx/GetSummary",
            data: JSON.stringify({
                timePeriod: n
            }),
            contentType: "application/json; charset=utf-8",
            success: function(n) {
                var t = $.parseJSON(n.d),
                    i;
                if ("HasCurrencyOperationError" in t && t.HasCurrencyOperationError) {
                    $("#Summary_tab .loading").addClass("error").text(t.CurrencyOperationErrorMessage);
                    return
                }
                $("#Summary_tab .loading").remove(), $("#Summary_tab td.Credit").text(" ");
                for (i in t) t.hasOwnProperty(i) && $("td.Credit." + i).html(e(t[i]));
                $(".RobuxColumn .total .money").html(t.Total === "" ? "0" : e(t.Total)), t.BCStipendBonus === "" ? $(".BCStipendBonus").parent(".datarow").hide() : $(".BCStipendBonus").parent(".datarow").show()
            },
            error: function() {
                $("#Summary_tab .loading").addClass("error").text("Sorry, something went wrong. Please try again.")
            }
        })
    }

    function s(n, t) {
        var i = t.clone(),
            r;
        return i.find(".Date").text(n.Date), i.find(".Member").append($("<span></span>").html(fitStringToWidthSafe(n.Member, 90))), i.find(".Amount span").html(c(n.Amount)).addClass(n.Amount_Class), i.find(".Description").text(n.Description + " "), n.Successful_Payments == 0 && n.Has_Payments && i.find(".Description").prepend($("<a href='#'>▾<a>").attr("onClick", '$(".payment' + n.Sale_ID + '").toggle();return false;')), n.Successful_Payments > 0 && i.addClass("payment" + n.Sale_ID), n.Member_ID == "1" ? i.find(".Member").prepend($("<div></div>").attr("class", "Roblox")) : n.MemberIsGroup == "True" ? i.find(".Member").prepend($("<div></div>").attr("class", "roblox-group-image").attr("data-group-id", n.Group_ID).attr("data-image-size", "small").attr("data-no-overlays", "true")) : i.find(".Member").prepend($("<div></div>").attr("class", "avatar avatar-headshot-sm roblox-avatar-image").attr("data-user-id", n.Member_ID).attr("data-image-size", "tiny").attr("data-no-overlays", "true")), n.Item_Url != undefined && n.Item_Url != "" ? (r = i.find(".Description").append($("<a></a>").attr("href", n.Item_Url).html(n.Item_Name)), n.EventDate != undefined && r.append(" on " + n.EventDate)) : i.find(".Description").html(" " + n.Item_Name), i
    }

    function i() {
        var t = $("#MyTransactions_TransactionTypeSelect").val();
        $(".TransactionsContainer table tr.morerecords").remove(), $(".TransactionsContainer table .loading").parent().remove(), $("#MyTransactions_tab table").append('<tr class="datarow"><td class="loading" colspan=4 ></td></tr>'), $.ajax({
            type: "POST",
            url: "Money.aspx/GetMyTransactions",
            data: JSON.stringify({
                transactiontype: t,
                startindex: n
            }),
            contentType: "application/json; charset=utf-8",
            success: function(r) {
                var f = $.parseJSON(r.d),
                    e, o, h, c, l;
                if (f.Data.length == 0) e = p, t === "purchase" ? e = a : t === "sale" ? e = v : t === "grouppayout" && (e = nt), $(".TransactionsContainer table .loading").html(e).removeClass("loading").addClass("empty");
                else {
                    for ($(".TransactionsContainer table tr .loading").parent().remove(), o = 0; o < f.Data.length; o++) t === "devex" ? $(".TransactionsContainer table").append(w($.parseJSON(f.Data[o]), u)) : (h = $.parseJSON(f.Data[o]), h.Successful_Payments == 0 ? $(".TransactionsContainer table").append(s(h, u)) : $(".TransactionsContainer table").append(s(h, k)));
                    Roblox.Widgets.AvatarImage.populate(), Roblox.Widgets.GroupImage.populate()
                }
                n = parseInt(f.StartIndex), c = parseInt(f.TotalCount), n < c && (l = "More Records", $(".TransactionsContainer table").append('<tr class="morerecords"><td colspan=4><a class="btn-control btn-control-small">' + l + "</a></td></tr>"), $(".TransactionsContainer table .morerecords a").bind("click", i))
            },
            error: function() {
                var i = "Sorry, something went wrong. Please try again.";
                $(".TransactionsContainer .loading").addClass("error").removeClass("loading").text(i)
            }
        })
    }

    function y(n) {
        var t = $("#TradeItems_tab .template tr").clone();
        return t.find(".Date").text(n.Date), n.Expires !== "" ? t.find(".Expires").text(n.Expires) : t.find(".Expires").text("  "), t.find(".Status").text(n.Status + n.StatusAddon), t.find(".Action").html('<a class="text-link ViewTradeLink" tradePartnerID="' + n.TradePartnerID + '" tradeSessionID="' + n.TradeSessionID + '" >View Details</a>'), t.find(".TradePartner").append($("<span></span>").html(fitStringToWidthSafe(n.TradePartner, 90))).attr("TradePartnerName", n.TradePartner), t.find(".TradePartner").prepend($("<div></div>").addClass("avatar avatar-headshot-sm roblox-avatar-image").attr("data-user-id", n.TradePartnerID).attr("data-image-size", "tiny").attr("data-no-overlays", "true")), Roblox.Widgets.AvatarImage.load(t.find(".roblox-avatar-image")), t
    }

    function t() {
        var i = $(".TradeItemsContainer table .datarow").length,
            n = $("#TradeItems_TradeType").val();
        $(".TradeItemsContainer table tr.morerecords").remove(), $(".TradeItemsContainer table .loading").parent().remove(), $(".TradeItemsContainer table").append('<tr class="datarow"><td class="loading" colspan=4 ></td></tr>'), $.ajax({
            type: "POST",
            url: "Money.aspx/GetMyItemTrades",
            data: JSON.stringify({
                statustype: n,
                startindex: i
            }),
            contentType: "application/json; charset=utf-8",
            success: function(i) {
                var r = $.parseJSON(i.d),
                    f = r.tradeWriteEnabled === "False" && (n === "inbound" || n === "outbound"),
                    e, o, u, s;
                if ($("[data-js-trade-write-disabled]").toggle(f), $("#trade-help-link").toggle(!f), r.Data.length == 0) e = "You have no ", o = " trade requests.", $(".TradeItemsContainer table .loading").html(e + n + o).removeClass("loading").addClass("empty");
                else
                    for ($(".TradeItemsContainer table tr .loading").parent().remove(), u = 0; u < r.Data.length; u++) $(".TradeItemsContainer table").append(y($.parseJSON(r.Data[u])));
                $(".TradeItemsContainer table .datarow").length < r.totalCount && (s = "More Records", $(".TradeItemsContainer table").append('<tr class="morerecords"><td colspan=4><a class="btn-control btn-control-small">' + s + "</a></td></tr>"), $(".TradeItemsContainer table .morerecords a").bind("click", t))
            },
            error: function() {
                var i = "Sorry, something went wrong. Please try again.";
                $(".TradeItemsContainer .loading").addClass("error").removeClass("loading").text(i)
            }
        })
    }

    function w(n, t) {
        var i = t.clone();
        return i.find(".Date").text(n.Date), i.find(".Member").append($("<span></span>").html(fitStringToWidthSafe(n.Member, 90))), i.find(".Description").text(n.Description + " "), i.find(".Amount span").html(c(n.Amount)).addClass(n.Amount_Class), i.find(".Member").prepend($("<div></div>").attr("class", "Roblox")), i
    }

    function b() {
        var r = $("#LinkGeneratorInput"),
            t = r.val(),
            u = r.data("rbx-id"),
            n = "Please link to a page on www.roblox.com!",
            f, e, i, o;
        t || $("#LinkGeneratorOutput").text(n), f = /^(http:\/\/|https:\/\/|)(www.|)roblox.com($|[^(.a-zA-Z0-9)])/, e = t.match(f), e && (n = t, i = n.indexOf("#"), i > -1 && (n = n.substr(0, i)), o = n.indexOf("?"), n += o > -1 ? "&rbxp=" + u : "?rbxp=" + u), $("#LinkGeneratorOutput").text(n)
    }

    function f() {
        var n = $("#Promotion_tab .nav-pills .active").data("rbx-time");
        $("#Promotion_tab [data-rbx-time=" + n + "]").each(function() {
            var t = $(this),
                i;
            if (t.show(), t.siblings("[data-rbx-organic-acquisition-type]").hide(), i = t.find(".stats-chart"), i.hasClass("loading")) {
                var r = t.data("rbx-series-names"),
                    u = t.data("rbx-series-units"),
                    f = {
                        chartType: "line",
                        seriesNames: r,
                        seriesUnits: u
                    },
                    e = "/promoter-stats?type=" + t.data("rbx-organic-acquisition-type") + "&timeframe=" + t.data("rbx-time");
                Roblox.FlotGraphing.DrawChartFromEndpoint(e, "#" + i.attr("id"), function(i, r) {
                    $(r).removeClass("loading"), d(n, t.data("rbx-organic-acquisition-type"), i)
                }, function(n, t) {
                    $(t).removeClass("loading");
                    var i = n.message ? n.message : "Sorry, an error occurred. Please try again later.";
                    $(t).text(i)
                }, f)
            }
        })
    }

    function d(n, t, i) {
        var r, u;
        i = i[0];
        var f = $("#promotion-data-table tr." + n).length > 0,
            e = $("#promotion-data-table tr.table-header [data-rbx-organic-acquisition-type=" + t + "]"),
            o = $("#promotion-data-table tr.table-header th").index(e);
        for (r = i.length - 1; r >= 0; r--) f ? u = $("#promotion-data-table tr." + n + '[data-rbx-time="' + i[r][0] + '"]') : (u = $('<tr class="' + n + '" data-rbx-time="' + i[r][0] + '"><td class="first">' + Roblox.FlotGraphing.ConvertTimeToReadableString(i[r][0]) + '</td><td class="acquisitions"></td><td class="conversions"></td><td class="revenue"></td></tr>'), u.appendTo($("#promotion-data-table"))), $($("td", u)[o]).text(i[r][1])
    }

    function g(n) {
        n.addClass("active"), n.siblings().each(function() {
            $(this).removeClass("active")
        });
        var t = $("#Promotion_tab .nav-pills .active").data("rbx-time");
        $("#promotion-data-table tr").each(function() {
            var n = $(this);
            n.toggle(n.hasClass(t) || n.hasClass("table-header"))
        }), f()
    }

    function h() {
        var t = $.address.hash().replace("/", "").escapeHTML(),
            n, i;
        return t != "" && (n = $("[contentid=" + t + "]"), typeof n[0] != "undefined" && (i = $(".SquareTabGray.selected"), i.attr("contentid") !== n.attr("contentid"))) ? (r(n), !0) : !1
    }
    var u = $('<tr class="datarow"></tr>').append('<td class="Date"></td><td class="Member"></td><td class="Description"></td><td class="Amount"><span>&nbsp;</span></td>'),
        k = $('<tr class="datarow paymentrow"></tr>').append('<td class="Date"></td><td class="Member"></td><td class="Description"></td><td class="Amount"><span>&nbsp;</span></td>'),
        p = "No records found.",
        v = "You have not sold any items!",
        a = "You have not purchased any items! Browse the <a href='" + (Roblox && Roblox.Endpoints ? Roblox.Endpoints.getAbsoluteUrl("/Catalog") : "/Catalog") + "'>Catalog</a> to buy items.",
        nt = "You have not received any Group Payouts.",
        n = 0,
        l;
    $(".MyMoneyPage .WhiteSquareTabsContainer li").bind("click", function() {
        r($(this))
    }), $(".MyMoneyPage #TimePeriod").bind("change", o), $("#MyTransactions_TransactionTypeSelect").bind("change", function() {
        $(".TransactionsContainer table tr.datarow,.TransactionsContainer table tr.morerecords").remove(), n = 0, i()
    }), $("#TradeItems_TradeType").bind("change", function() {
        $(".TradeItemsContainer table tr.datarow,.TradeItemsContainer table tr.morerecords").remove(), t()
    }), $(".MyMoneyPage .tooltip").tipsy({
        gravity: "s",
        fade: !0,
        delayOut: 1e3,
        html: !0
    }), h() || (l = $(".SquareTabGray.selected"), r(l)), $(document).bind("TradeUpdate", function() {
        $(".TradeItemsContainer table tr.datarow,.TradeItemsContainer table tr.morerecords").remove(), t()
    });
    $("#LinkGeneratorInput").on("keyup paste", b);
    $("#Promotion_tab").on("click", ".nav-pills li", function() {
        g($(this))
    });
    $.address.externalChange(h)
});