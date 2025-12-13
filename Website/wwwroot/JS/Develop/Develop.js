// Develop/Develop.js
var Roblox = Roblox || {};
Roblox.DevelopPage = function() {
    function y(r) {
        console.log('[Develop.js] History state change handler y() called with data:', r);
        !i && r.clickTargetID && (u = !0,
            console.log('[Develop.js] y() handling clickTargetID:', r.clickTargetID, ' historyInFlight(i):', i, ' url:', r.url),
            r.clickTargetID === "catalog" || r.clickTargetID === h
                ? (console.log('[Develop.js] y() switching to catalog tab'),
                   t.hasClass("tab-active") || (
                       console.log('[Develop.js] y() making LibraryTabLink and LibraryTab active'),
                       $("div.tab-active").removeClass("tab-active"),
                       t.addClass("tab-active"),
                       n.addClass("tab-active")
                   ),
                   Roblox.CatalogShared && (console.log('[Develop.js] y() delegating to Roblox.CatalogShared.handleURLChange'), Roblox.CatalogShared.handleURLChange(r))
                  )
                : (console.log('[Develop.js] y() triggering click on element with id', r.clickTargetID),
                   $("#" + r.clickTargetID).click()
                  ),
            u = !1,
            console.log('[Develop.js] y() finished handling state, flag u reset to', u)
        )
    }

    function p(t) {
        var s = t.data("url"),
            o = s,
            w = t.attr("id"),
            b = s === c,
            h, y, p;
        console.log('[Develop.js] p() called for tab', w, 'url:', s, 'data-url:', s, 'isGroupUrl:', b);
        s === f
            ? (console.log('[Develop.js] p() handling LibraryTab navigation'),
               w = "catalog",
               $("#LibraryTab #catalog").length == 0
                   ? (console.log('[Develop.js] p() initial catalog load via Ajax; library-get-url:', t.data("library-get-url")),
                      e = !0,
                      Roblox.CatalogShared.LoadCatalogAjax(t.data("library-get-url"), null, n, !1, !0)
                     )
                   : r
                        ? (console.log('[Develop.js] p() reusing existing catalog; query string r:', r), o += "?" + r)
                        : (h = $("#LibraryTabLink").data("query-params"),
                           console.log('[Develop.js] p() using LibraryTabLink query-params:', h),
                           h && (o += "?" + h)
                          )
              )
            : (b || s === l) && (
                  console.log('[Develop.js] p() handling My/Group Creations navigation; isGroup:', b, 'assetTypeId:', $("#assetTypeId").val()),
                  y = parseInt($("#assetTypeId").val()),
                  p = $(b ? "#GroupCreationsTab" : "#MyCreationsTab"),
                  y === a
                      ? (console.log('[Develop.js] p() loading badges via ItemLoader for tab container', b ? '#GroupCreationsTab' : '#MyCreationsTab'), Roblox.BuildPage.ItemLoader.loadBadges(p, ""))
                      : y === v && (console.log('[Develop.js] p() loading game passes via ItemLoader for tab container', b ? '#GroupCreationsTab' : '#MyCreationsTab'), Roblox.BuildPage.ItemLoader.loadGamePasses(p, 0))
              ),
        u || (
            i = !0,
            o = o.indexOf("#") === -1 ? o : o.split("#")[1],
            console.log('[Develop.js] p() pushing new History state. clickTargetID:', w, 'finalUrl:', o),
            History.pushState({
            clickTargetID: w
        }, document.title, o),
            i = !1,
            console.log('[Develop.js] p() History.pushState complete; historyInFlight(i) reset to', i)
        )
    }
    var i = !1,
        u = !1,
        e = !1,
        n, t, o, s, f, h, r, c, l, a = 21,
        v = 34;
    $(function() {
        var u, a;
        console.log('[Develop.js] jQuery ready; initializing DevelopPage tab wiring');
        o = $("#GroupCreationsTabLink"),
        s = $("#MyCreationsTabLink"),
        t = $("#LibraryTabLink"),
        n = $("#LibraryTab"),
        c = o.data("url"),
        l = s.data("url"),
        f = t.data("url"),
        h = t.attr("id"),
        u = $("#DevelopTabs .tab-active"),
        console.log('[Develop.js] Initial active tab in #DevelopTabs:', u.attr('id'), 'urls:', { group: c, my: l, library: f });
        i = !0,
        a = document.URL.indexOf("#") === -1 ? document.URL : document.URL.split("#")[1],
        console.log('[Develop.js] Calling History.replaceState with clickTargetID:', u.attr('id'), 'url a:', a);
        History.replaceState({
            clickTargetID: u.attr("id"),
            url: document.URL
        }, document.title, a),
        i = !1,
        console.log('[Develop.js] History.replaceState complete; historyInFlight(i):', i);

        if (document.URL.indexOf(f) !== -1) {
            console.log('[Develop.js] URL contains LibraryTab URL; ensuring catalog is loaded. full URL:', document.URL);
            r = document.URL.split("?")[1];
            if (r && Roblox.CatalogValues && Roblox.CatalogValues.CatalogContentsUrl) {
                console.log('[Develop.js] Loading catalog via CatalogValues URL + query:', Roblox.CatalogValues.CatalogContentsUrl + '?' + r);
                Roblox.CatalogShared.LoadCatalogAjax(Roblox.CatalogValues.CatalogContentsUrl + "?" + r, null, n, !0);
            } else {
                console.log('[Develop.js] No query string or CatalogValues missing; using library-get-url from LibraryTabLink');
                e = !0;
                Roblox.CatalogShared.LoadCatalogAjax(t.data("library-get-url"), null, n, !1, !0);
            }
        }

        History.Adapter.bind(window, "statechange", function() {
            var state = History.getState();
            console.log('[Develop.js] window statechange event fired; state.data:', state && state.data);
            y(state.data);
        });

        $("#DevelopTabs").on("tabsactivate", function() {
            var activeTab = $(this).find(".tab-active");
            console.log('[Develop.js] #DevelopTabs tabsactivate fired; active tab id:', activeTab.attr('id'));
            p(activeTab);
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

        var search = window.location.search || "";
        var match = search.match(/[?&](?:view|View)=(\d+)/);
        var viewValue = match && match[1] ? match[1] : null;
        if (!viewValue) {
            viewValue = "2";
        }
        if (viewValue === "2") {
            console.log('[Develop.js] Detected T-Shirts view; loading content from /develop/asset-list/2');
            $("#MyCreationsTab .items-container").load("/develop/asset-list/2");
        } else if (viewValue === "12") {
            console.log('[Develop.js] Detected Pants view; loading content from /develop/asset-list/12');
            $("#MyCreationsTab .items-container").load("/develop/asset-list/12");
        }
    })
}();