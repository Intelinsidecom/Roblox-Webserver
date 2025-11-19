// Develop/Develop.js
var Roblox = Roblox || {};
Roblox.DevelopPage = function() {
    function y(r) {
        !i && r.clickTargetID && (u = !0, r.clickTargetID === "catalog" || r.clickTargetID === h ? (t.hasClass("tab-active") || ($("div.tab-active").removeClass("tab-active"), t.addClass("tab-active"), n.addClass("tab-active")), Roblox.CatalogShared && Roblox.CatalogShared.handleURLChange(r)) : $("#" + r.clickTargetID).click(), u = !1)
    }

    function p(t) {
        var s = t.data("url"),
            o = s,
            w = t.attr("id"),
            b = s === c,
            h, y, p;
        s === f ? (w = "catalog", $("#LibraryTab #catalog").length == 0 ? (e = !0, Roblox.CatalogShared.LoadCatalogAjax(t.data("library-get-url"), null, n, !1, !0)) : r ? o += "?" + r : (h = $("#LibraryTabLink").data("query-params"), h && (o += "?" + h))) : (b || s === l) && (y = parseInt($("#assetTypeId").val()), p = $(b ? "#GroupCreationsTab" : "#MyCreationsTab"), y === a ? Roblox.BuildPage.ItemLoader.loadBadges(p, "") : y === v && Roblox.BuildPage.ItemLoader.loadGamePasses(p, 0)), u || (i = !0, o = o.indexOf("#") === -1 ? o : o.split("#")[1], History.pushState({
            clickTargetID: w
        }, document.title, o), i = !1)
    }
    var i = !1,
        u = !1,
        e = !1,
        n, t, o, s, f, h, r, c, l, a = 21,
        v = 34;
    $(function() {
        var u, a;
        o = $("#GroupCreationsTabLink"), s = $("#MyCreationsTabLink"), t = $("#LibraryTabLink"), n = $("#LibraryTab"), c = o.data("url"), l = s.data("url"), f = t.data("url"), h = t.attr("id"), u = $("#DevelopTabs .tab-active"), i = !0, a = document.URL.indexOf("#") === -1 ? document.URL : document.URL.split("#")[1], History.replaceState({
            clickTargetID: u.attr("id"),
            url: document.URL
        }, document.title, a), i = !1, document.URL.indexOf(f) !== -1 && (r = document.URL.split("?")[1], r && Roblox.CatalogValues && Roblox.CatalogValues.CatalogContentsUrl ? Roblox.CatalogShared.LoadCatalogAjax(Roblox.CatalogValues.CatalogContentsUrl + "?" + r, null, n, !0) : (e = !0, Roblox.CatalogShared.LoadCatalogAjax(t.data("library-get-url"), null, n, !1, !0))), History.Adapter.bind(window, "statechange", function() {
            y(History.getState().data)
        });
        $("#DevelopTabs").on("tabsactivate", function() {
            p($(this).find(".tab-active"))
        });
        n.on(Roblox.CatalogShared.CatalogLoadedViaAjaxEventName, null, null, Roblox.CatalogShared.handleCatalogLoadedViaAjaxEvent);
        $(".develop-experimental-label").click(function() {
            var n = $(".develop-experimental-label").data("learn-more-url");
            Roblox.Dialog.open({
                titleText: "Experimental Mode Games",
                bodyContent: "Experimental mode games are games made by new developers. These games may not be stable and unexpected things could happen here.<br/><a class='text-link' target='_blank' href='" + n + "'>Learn more from Wiki</a>",
                showAccept: !1,
                declineText: "OK",
                allowHtmlContentInBody: !0,
                xToCancel: !0
            })
        })
    })
}();