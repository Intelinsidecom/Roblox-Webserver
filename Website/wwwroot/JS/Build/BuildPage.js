// Build/BuildPage.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.BuildPage == "undefined" && (Roblox.BuildPage = {}), $(function () {
    function i() {
        // console.log('[BuildPage.js] i() closeDropdown called; current gear element t =', t);
        if (t) {
            var n = $(t);
            // console.log('[BuildPage.js] i() removing gear-open from', n.get(0));
            n.removeClass("gear-open"), n.parent().css({
                "background-color": "#FFFFFF",
                "border-color": "white",
                "z-index": "0"
            }), t = null
        }
        // console.log('[BuildPage.js] i() hiding dropdown menus c, s, and tshirtDropdown');

        c.hide();
        s.hide();
        $("#tshirt-dropdown-menu").hide();
        return !1
    }

    function o(t, i) {
        var f = i.find(n).data("fetchplaceurl"),
            h = i.find(n).data("universeid"),
            c2 = i.find(n).data("fetchuniverseplaces"),
            l = i.find(r).val(),
            a = {
                creationContext: l,
                assetLinksEnabled: k,
                universeId: h,
                fetchUniversePlaces: c2
            },
            e = "?",
            u, o2, s2;
        console.log('[BuildPage.js] o() building fetch URL; isLoadMore:', !!t, 'fetchplaceurl:', f, 'universeId:', h, 'fetchUniversePlaces:', c2, 'creationContext:', l);
        f.indexOf("?") != -1 && (e = "&"), u = f + e + $.param(a), t && (o2 = i.find("table.item-table").length, s2 = "&startRow=" + o2, u += s2, console.log('[BuildPage.js] o() appending startRow, item-table count =', o2));
        console.log('[BuildPage.js] o() final URL:', u);
        return u
    }

    function e(n2, t2) {
        console.log('[BuildPage.js] e() loadContext called; isContextGame:', !!n2, 'tabRoot element id:', t2 && t2.attr('id'));
        // Spinner disabled (intentionally left disabled; only logging here)
        // var f = t2.find(".build-loading-container").html(),
        //     u;
        // t2.find(".items-container").html(f);
        var e2 = t2.find(r).val(),
            i2 = t2.find(".content-area .content-title .aside-text"),
            s2 = t2.find(".breadCrumb .breadCrumbContext"),
            h2 = t2.find(r + " option:selected").text();
        console.log('[BuildPage.js] e() creationContext value:', e2, 'selected text:', h2);
        console.log('[BuildPage.js] e() before breadcrumb toggle; has .context-game-separator:', t2.find('.context-game-separator').length, 'has .breadCrumbGame:', t2.find('.breadCrumbGame').length);
        t2.find(".context-game-separator").hide(), t2.find(".breadCrumbGame").hide(), e2 != "NonGameCreation" ? (t2.find(".show-active-places-only").hide(), t2.find(".creation-context-breadcrumb").show(), s2.text(h2), i2.hide(), n2 && (t2.find(".context-game-separator").show(), t2.find(".breadCrumbGame").show())) : (t2.find(".creation-context-breadcrumb").hide(), i2.show(), t2.find(".show-active-places-only").show());
        console.log('[BuildPage.js] e() after breadcrumb toggle; .creation-context-breadcrumb visible?:', t2.find('.creation-context-breadcrumb').is(':visible'));
        u = o(!1, t2);
        console.log('[BuildPage.js] e() loading items-container via .load() from URL:', u);
        t2.find(".items-container").load(u, function () {
            console.log('[BuildPage.js] e() .items-container load callback fired for URL:', u, 'items-container child count:', t2.find('.items-container').children().length);
        })
    }

    function f(t3, i3) {
        console.log('[BuildPage.js] f() resetUniverseAndLoad called; isContextGame:', !!t3, 'tabRoot id:', i3 && i3.attr('id'));
        i3.find(n).data("universeid", 0), i3.find(n).data("fetchuniverseplaces", !1);
        console.log('[BuildPage.js] f() reset universeId and fetchUniversePlaces to default for', i3 && i3.attr('id'));
        e(t3, i3)
    }

    function u(n4, t4, i4) {
        console.log('[BuildPage.js] u() loadMore click handler called; link element:', n4 && n4.get(0), 'url:', t4, 'tabRoot id:', i4 && i4.attr('id'));
        n4.hide();
        $.ajax({
            url: t4,
            cache: !1,
            dataType: "html",
            success: function (t5) {
                console.log('[BuildPage.js] u() ajax success for url:', t4);
                n4.remove();
                var u2 = i4.find(".items-container"),
                    r2 = $(t5).hide();
                u2.append(r2);
                r2.fadeIn();
                console.log('[BuildPage.js] u() appended new content; items-container child count now:', u2.children().length);
                r2.find("a[data-retry-url]").loadRobloxThumbnails()
            },
            fail: function () {
                console.log('[BuildPage.js] u() ajax fail for url:', t4, ' - re-showing load-more link');
                n4.show()
            }
        })
    }

    function l(n, t) {
        Roblox.GenericConfirmation.open({
            titleText: "Shut Down Servers",
            bodyContent: t.hasOwnProperty("placeId") ? "Are you sure you want to shut down all servers for this place?" : "Are you sure you want to shut down all servers in all places in this game?",
            onAccept: function () {
                $.ajax({
                    type: "POST",
                    url: n,
                    data: t,
                    error: function () {
                        Roblox.GenericConfirmation.open({
                            titleText: Roblox.BuildPage.Resources.errorOccurred,
                            bodyContent: "An error occured while shutting down servers.",
                            acceptText: Roblox.BuildPage.Resources.ok,
                            acceptColor: Roblox.GenericConfirmation.blue,
                            declineColor: Roblox.GenericConfirmation.none,
                            allowHtmlContentInBody: !0,
                            dismissable: !0
                        })
                    }
                })
            },
            acceptColor: Roblox.GenericConfirmation.blue,
            acceptText: "Yes",
            declineText: "No",
            allowHtmlContentInBody: !0
        })
    }

    function a(n5, r5) {
        var u5;
        // console.log('[BuildPage.js] a() openDropdown called for gear button; current t:', t, 'event target:', this);
        if (t == this) return /* console.log('[BuildPage.js] a() same gear button clicked; closing dropdown via i()'), */ i();
        t && (/* console.log('[BuildPage.js] a() closing previously open dropdown before opening new one'), */ i());

        t = this, u5 = $(this), u5.addClass("gear-open");
        var e = u5.closest("table"),
            f = e.data("item-id"),
            o = e.data("item-moderation-approved"),
            isTShirt = e.data("type") === "tshirts",
            dropdown = isTShirt ? $("#tshirt-dropdown-menu") : r5;

        if (isTShirt) {
            // Point the Configure link to /my/item.aspx?id=<item-id> for the current T-shirt row
            dropdown.find("a[data-action='configure']").attr("href", "/my/item.aspx?id=" + f);
        }

        if (!isTShirt) {
            o = o === "True";
            dropdown.find("a").each(function () {
                var n = $(this),
                    i = n.hasClass("advertise-link"),
                    r = n.data("href-template"),
                    s, t;
                r && (s = r.replace(/=0/g, "=" + f).replace(/\/0\//g, "/" + f + "/").replace(/\/0$/, "/" + f), n.attr("href", s));
                n.attr("data-place-id") && n.attr("data-place-id", f);
                n.attr("data-item-id") && n.attr("data-item-id", f);
                e.data("runnable") === "False" && n.data("ad-activate-link") === "Run" ? n.hide() : e.data("runnable") === "True" && n.data("ad-activate-link") === "Run" && n.show();
                u5.data("is-sponsored-game") && (n.data("parent-sponsored-game-element", u5.parents(".sponsored-game")), n.hasClass("dropdown-item-run-sponsored-game") && n.toggle(u5.data("show-run")), n.hasClass("dropdown-item-stop-sponsored-game") && n.toggle(u5.data("show-stop")));
                n.data("href-reference") && n.attr("href", e.data(n.data("href-reference")));
                n.hasClass("shutdown-all-servers-button") && (e.data("type") == "universes" ? n.attr("data-universe-id", f).removeAttr("data-place-id") : n.attr("data-place-id", f).removeAttr("data-universe-id"));
                t = e.data("rootplace-id");
                n.data("require-root-place") && !t ? n.hide() : n.data("require-root-place") && t && (n.show(), n.data("configure-place-template") && n.attr("href", n.data("configure-place-template").replace(/\/\d+\//, "/" + t + "/")));
                i && !o ? n.hide() : i && o && n.show();
            });
            $("#configure-localization-link").click(function () {
                Roblox && Roblox.EventStream && Roblox.EventStream.SendEventWithTarget("formInteraction", "Create", {
                    universeId: f
                }, Roblox.EventStream.TargetTypes.WWW)
            });
        }

        var s3 = dropdown.parent().offset(),
            c3 = dropdown.outerWidth(),
            h3 = u5.offset(),
            finalTop,
            finalLeft;
        if (isTShirt) {
            // Ensure the T-shirt dropdown is anchored to the page so we can use absolute offsets
            if (!dropdown.parent().is('body')) {
                dropdown.appendTo('body');
            }
            // Recompute parent offset after potential move (body is usually {top:0,left:0})
            s3 = dropdown.parent().offset() || { top: 0, left: 0 };

            // Detailed debug for T-shirt dropdown positioning
            // console.log('[BuildPage.js] a() T-shirt positioning raw values (anchored to body):', {
            //     parentOffsetTop: s3.top,
            //     parentOffsetLeft: s3.left,
            //     buttonOffsetTop: h3.top,
            //     buttonOffsetLeft: h3.left,
            //     buttonOuterHeight: u5.outerHeight(),
            //     buttonOuterWidth: u5.outerWidth(),
            //     dropdownWidth: c3
            // });
            // Vertically: just below the gear button, with a small padding
            finalTop = h3.top + u5.outerHeight() + 4;
            // Horizontally: align the dropdown's right edge with the gear button's right edge
            finalLeft = h3.left + u5.outerWidth() - c3;
        } else {
            finalTop = h3.top - s3.top + 21 + u5.outerHeight() + 9;
            finalLeft = h3.left - s3.left + 15 - c3 + u5.outerWidth();
        }

        // console.log('[BuildPage.js] a() positioning dropdown; isTShirt:', isTShirt, 'parent offset:', s3, 'dropdown width:', c3, 'button offset:', h3, 'finalTop:', finalTop, 'finalLeft:', finalLeft);

        return dropdown.css({
            position: "absolute",
            top: finalTop + "px",
            left: finalLeft + "px"
        }).show(), u5.parent().css({
            "background-color": "#EFEFEF",
            "border-color": "gray",
            "z-index": 999
        }), n5.preventDefault(), !1
    }

    function v(n, t, r, u) {
        i();
        var f = {
            adid: n,
            bidAmount: t,
            confirmed: r,
            useGroupFunds: u
        },
            e = "/user-sponsorship/processadpurchase";
        $.post(e, f, function (i) {
            i.success ? Roblox.GenericConfirmation.open({
                titleText: Roblox.BuildPage.Resources.purchaseComplete,
                bodyContent: Roblox.BuildPage.Resources.youHaveBid + "<span class='currency CurrencyColor1'>" + t + "</span> .",
                acceptText: Roblox.BuildPage.Resources.ok,
                acceptColor: Roblox.GenericConfirmation.blue,
                declineColor: Roblox.GenericConfirmation.none,
                onAccept: function () {
                    window.location.reload()
                },
                allowHtmlContentInBody: !0,
                dismissable: !0
            }) : i.requireConfirmation ? Roblox.GenericConfirmation.open({
                titleText: Roblox.BuildPage.Resources.confirmBid,
                bodyContent: i.error,
                acceptText: Roblox.BuildPage.Resources.placeBid,
                declineText: Roblox.BuildPage.Resources.cancel,
                acceptColor: Roblox.GenericConfirmation.blue,
                declineColor: Roblox.GenericConfirmation.gray,
                onAccept: function () {
                    v(n, t, !0, u)
                },
                allowHtmlContentInBody: !0,
                dismissable: !0
            }) : Roblox.GenericConfirmation.open({
                titleText: Roblox.BuildPage.Resources.errorOccurred,
                bodyContent: i.error,
                acceptText: Roblox.BuildPage.Resources.ok,
                acceptColor: Roblox.GenericConfirmation.blue,
                declineColor: Roblox.GenericConfirmation.none,
                allowHtmlContentInBody: !0,
                dismissable: !0
            })
        })
    }

    function b(n) {
        i();
        var t = {
            adid: n
        },
            r = "/user-sponsorship/deletead";
        $.post(r, t, function (n) {
            n.success ? Roblox.GenericConfirmation.open({
                titleText: Roblox.BuildPage.Resources.adDeleted,
                bodyContent: Roblox.BuildPage.Resources.theAdWasDeleted,
                acceptText: Roblox.BuildPage.Resources.ok,
                acceptColor: Roblox.GenericConfirmation.blue,
                declineColor: Roblox.GenericConfirmation.none,
                onAccept: function () {
                    window.location.reload()
                },
                allowHtmlContentInBody: !0,
                dismissable: !0
            }) : Roblox.GenericConfirmation.open({
                titleText: Roblox.BuildPage.Resources.errorOccurred,
                bodyContent: n.error,
                acceptText: Roblox.BuildPage.Resources.ok,
                acceptColor: Roblox.GenericConfirmation.blue,
                declineColor: Roblox.GenericConfirmation.none,
                allowHtmlContentInBody: !0,
                dismissable: !0
            })
        })
    }

    function h(n) {
        i();
        var t = n.find("tr.bid-now-row"),
            r = n.find("a[data-ad-status-toggle]").hasClass("runnable");
        return r && t.show(), !1
    }
    var n = ".creation-context-filters-and-sorts",
        r = ".place-creationcontext-drop-down",
        w = $("#MyCreationsTab " + r),
        p2 = $("#GroupCreationsTab " + r),
        c = $("#MyCreationsTab #build-dropdown-menu"),
        s = $("#GroupCreationsTab #build-dropdown-menu"),
        tshirtDropdown = $("#tshirt-dropdown-menu"),
        t = null,
        k = $("#assetLinks").data("asset-links-enabled"),
        y;
    // console.log('[BuildPage.js] Document ready for BuildPage; initial selectors:', {
    //     myDropDownExists: w.length,
    //     groupDropDownExists: p2.length,
    //     myDropdownMenuExists: c.length,
    //     groupDropdownMenuExists: s.length,
    //     assetLinksEnabled: k
    // });
    $("a[data-retry-url]").loadRobloxThumbnails();
    w.change(function () {
        console.log('[BuildPage.js] #MyCreationsTab place-creationcontext-drop-down change event');
        f(!0, $("#MyCreationsTab"))
    });
    p2.change(function () {
        console.log('[BuildPage.js] #GroupCreationsTab place-creationcontext-drop-down change event');
        f(!0, $("#GroupCreationsTab"))
    });
    $("#MyCreationsTab .items-container").on("click", ".view-places-button", function () {
        var t6 = $("#MyCreationsTab"),
            i6 = $(this),
            r6 = i6.data("universeid"),
            u6 = i6.data("universename");
        console.log('[BuildPage.js] MyCreationsTab .view-places-button clicked; universeId:', r6, 'universeName:', u6);
        return t6.find(n).data("universeid", r6), t6.find(n).data("fetchuniverseplaces", !0), t6.find(".breadCrumbGame").text(u6), t6.find(".context-game-separator").show(), t6.find(".breadCrumbGame").show(), e(!0, t6), !1
    });
    $("#GroupCreationsTab .items-container").on("click", ".view-places-button", function () {
        var t7 = $("#GroupCreationsTab"),
            i7 = $(this),
            r7 = i7.data("universeid"),
            u7 = i7.data("universename");
        console.log('[BuildPage.js] GroupCreationsTab .view-places-button clicked; universeId:', r7, 'universeName:', u7);
        return t7.find(n).data("universeid", r7), t7.find(n).data("fetchuniverseplaces", !0), t7.find(".breadCrumbGame").text(u7), t7.find(".context-game-separator").show(), t7.find(".breadCrumbGame").show(), e(!0, t7), !1
    });
    $("#MyCreationsTab").on("click", ".breadCrumbContext", function () {
        console.log('[BuildPage.js] MyCreationsTab .breadCrumbContext clicked - resetting context');
        return f(!1, $("#MyCreationsTab")), !1
    });
    $("#GroupCreationsTab").on("click", ".breadCrumbContext", function () {
        console.log('[BuildPage.js] GroupCreationsTab .breadCrumbContext clicked - resetting context');
        return f(!1, $("#GroupCreationsTab")), !1
    });
    $("#MyCreationsTab .items-container").on("click", ".load-more-places", function () {
        var t8 = $(this),
            n8 = $("#MyCreationsTab"),
            i8 = o(!0, n8);
        console.log('[BuildPage.js] MyCreationsTab .load-more-places clicked; url:', i8);
        return u(t8, i8, n8), !1
    });
    $("#GroupCreationsTab .items-container").on("click", ".load-more-places", function () {
        var t9 = $(this),
            n9 = $("#GroupCreationsTab"),
            i9 = o(!0, n9);
        console.log('[BuildPage.js] GroupCreationsTab .load-more-places clicked; url:', i9);
        return u(t9, i9, n9), !1
    });
    $(".BuildPageContent").on("click", "a.roblox-edit-button", function () {
        var n, t, i, r;
        $(".build-page").data("edit-opens-studio") != "False" || Roblox.Client.isIDE() ? (n = $(this).closest("table"), t = n.data("rootplace-id") || n.data("item-id"), window.play_placeId = t, i = n.data("universeid") || n.data("item-id"), r = $("#PlaceLauncherStatusPanel").data("is-protocol-handler-launch-enabled") == "True", n.data("type") == "app-place" ? Roblox.GenericConfirmation.open({
            titleText: Roblox.BuildPage.Resources.appStudioTitle,
            bodyContent: Roblox.BuildPage.Resources.appStudioBody,
            acceptText: Roblox.BuildPage.Resources.continueText,
            acceptColor: Roblox.GenericConfirmation.blue,
            declineText: Roblox.BuildPage.Resources.cancel,
            imageUrl: "/images/Icons/img-alert.png",
            onAccept: function () {
                r ? Roblox.GameLauncher.editGameInStudio(t, i, !0) : editGameInStudio(t)
            }
        }) : r ? Roblox.GameLauncher.editGameInStudio(t, i, !0) : editGameInStudio(t)) : Roblox.GenericConfirmation.open({
            titleText: Roblox.BuildPage.Resources.editGame,
            bodyContent: Roblox.BuildPage.Resources.openIn + "<a target='_blank' href='http://wiki.roblox.com/index.php/Studio'>" + Roblox.BuildPage.Resources.robloxStudio + "</a>.",
            acceptText: Roblox.BuildPage.Resources.ok,
            acceptColor: Roblox.GenericConfirmation.blue,
            declineColor: Roblox.GenericConfirmation.none,
            imageUrl: "/images/Icons/img-alert.png",
            allowHtmlContentInBody: !0,
            dismissable: !0
        })
    });
    $("body").on("click", "a.shutdown-all-servers-button[data-universe-id]", function () {
        return l("/universes/shutdown-all-games", {
            universeId: $(this).data("universe-id")
        }), !1
    });
    $("body").on("click", "a.shutdown-all-servers-button[data-place-id]", function () {
        return l("/games/shutdown-all-instances", {
            placeId: $(this).data("place-id")
        }), !1
    });
    $(".BuildPageContent").on("mouseover", "a.gear-button", function () {
        $(this).addClass("gear-hover")
    });
    $(".BuildPageContent").on("mouseout", "a.gear-button", function () {
        $(this).removeClass("gear-hover")
    });
    $("#MyCreationsTab").on("click", "a.gear-button", function (n) {
        return a.apply(this, [n, c])
    });
    $("#GroupCreationsTab").on("click", "a.gear-button", function (n) {
        return a.apply(this, [n, s])
    });
    $(document).click(function (evt) {
        // console.log('[BuildPage.js] document click handler fired; target:', evt.target, 'current gear element t =', t);
        i()
    }), $(window).resize(i), $("input[data-bid-now-amount]").filter_input({
        regex: "[0-9]"
    });
    $(".items-container").on("click", "a.runnable[data-ad-status-toggle]", function () {
        var n = $(this).closest("table.item-table");
        return h(n), !1
    });
    $("a[data-ad-activate-link]").click(function () {
        var n = $(t).closest("table.item-table");
        return h(n), !1
    }), $("a[data-ad-remove-link]").click(function () {
        var n = $(t).closest("table.item-table"),
            i = n.data("item-id");
        return Roblox.GenericConfirmation.open({
            titleText: Roblox.BuildPage.Resources.confirmDelete,
            bodyContent: Roblox.BuildPage.Resources.areYouSureDelete,
            acceptText: Roblox.BuildPage.Resources.ok,
            declineText: Roblox.BuildPage.Resources.cancel,
            acceptColor: Roblox.GenericConfirmation.blue,
            declineColor: Roblox.GenericConfirmation.gray,
            onAccept: function () {
                b(i)
            },
            allowHtmlContentInBody: !1,
            dismissable: !0
        }), !1
    });
    $(".items-container").on("click", "a.cancel-ad-buy", function () {
        var n = $(this).closest("table.item-table"),
            t = n.find("tr.bid-now-row"),
            i = n.find("input[data-bid-now-amount]");
        return i.val(""), t.hide(), !1
    });
    $(".items-container").on("click", "a.process-ad-buy", function () {
        var t = $(this).closest("table.item-table"),
            f = t.find("input[data-bid-now-amount]"),
            e = $("#dataHolder"),
            i = e.data("minrobuxbid"),
            n = f.val(),
            o = t.data("item-id"),
            s = t.find("input[data-use-group-funds]").is(":checked");
        if (n < i || isNaN(n)) return Roblox.GenericConfirmation.open({
            titleText: Roblox.BuildPage.Resources.yourRejected,
            bodyContent: Roblox.BuildPage.Resources.bidRange2 + "<span class='currency CurrencyColor1'>" + i + "</span>.",
            acceptText: Roblox.BuildPage.Resources.ok,
            acceptColor: Roblox.GenericConfirmation.blue,
            declineColor: Roblox.GenericConfirmation.none,
            allowHtmlContentInBody: !0,
            dismissable: !0
        }), !1;
        var r = t.data("cost-per-impression"),
            h = '<img class="tooltip-bottom"  src="' + Roblox.BuildPage.Resources.questionmarkImgUrl + '" alt="Help" title="' + Roblox.BuildPage.Resources.estimatorExplanation + '"/>',
            u = "";
        return r != "" && (u = "<br />" + Roblox.BuildPage.Resources.estimatedImpressions + h + ": " + Math.round(n / r)), Roblox.GenericConfirmation.open({
            titleText: Roblox.BuildPage.Resources.makeAdBid,
            bodyContent: Roblox.BuildPage.Resources.wouldYouLikeToBid + "<span class='currency CurrencyColor1'>" + n + "</span> ?" + u,
            acceptText: Roblox.BuildPage.Resources.placeBid,
            declineText: Roblox.BuildPage.Resources.cancel,
            acceptColor: Roblox.GenericConfirmation.blue,
            declineColor: Roblox.GenericConfirmation.gray,
            onAccept: function () {
                v(o, n, !1, s)
            },
            onDecline: "",
            allowHtmlContentInBody: !0,
            dismissable: !0
        }), !1
    });
    $("#MyCreationsTab .items-container").on("click", ".load-more-ads", function () {
        var n = $(this),
            t = $("#MyCreationsTab"),
            i = n.attr("data-next-start"),
            r = "/build/ads?startRow=" + i;
        return u(n, r, t), !1
    });
    $("#GroupCreationsTab .items-container").on("click", ".load-more-ads", function () {
        var n = $(this),
            t = $("#GroupCreationsTab"),
            i = n.attr("data-next-start"),
            r = t.find(".BuildPageContent").data("groupid"),
            f = "/build/ads?startRow=" + i + "&groupId=" + r;
        return u(n, f, t), !1
    });
    $("a.item-image.ad-image").click(function () {
        var t = $(this).closest("table.item-table"),
            i = t.data("item-id"),
            r = "/user-sponsorship/getadimage?adId=" + i;
        $.ajax({
            url: r,
            success: function (n) {
                $("#AdPreviewContainer").html(n)
            },
            cache: !1
        }), $("span[data-retry-url]").loadRobloxThumbnails();
        var u = ["30%", "30%"],
            f = ["10%", "45%"],
            e = ["30%", "40%"],
            o = t.data("ad-type"),
            n = ["10%", "30%"];
        switch (o) {
            case "Banner":
                n = u;
                break;
            case "Box":
                n = e;
                break;
            case "Skyscraper":
                n = f
        }
        return $("#AdPreviewModal").modal({
            overlayClose: !0,
            escClose: !0,
            opacity: 80,
            overlayCss: {
                backgroundColor: "#000"
            },
            position: n
        }), !1
    }), y = window.location.search;
    $("#GroupCreationsTab .groups-dropdown-container").on("change", "select", function () {
        var n = $(this).val(),
            t = $("#GroupCreationsTab .groups-dropdown-container").data("get-url");
        window.location = t + "/" + n + y
    });
})
;