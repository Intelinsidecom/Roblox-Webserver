// Build/BuildPage.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.BuildPage == "undefined" && (Roblox.BuildPage = {}), $(function() {
    function i() {
        if (t) {
            var n = $(t);
            n.removeClass("gear-open"), n.parent().css({
                "background-color": "#FFFFFF",
                "border-color": "white",
                "z-index": "0"
            }), t = null
        }
        return c.hide(), s.hide(), !1
    }

    function o(t, i) {
        var f = i.find(n).data("fetchplaceurl"),
            h = i.find(n).data("universeid"),
            c = i.find(n).data("fetchuniverseplaces"),
            l = i.find(r).val(),
            a = {
                creationContext: l,
                assetLinksEnabled: k,
                universeId: h,
                fetchUniversePlaces: c
            },
            e = "?",
            u, o, s;
        return f.indexOf("?") != -1 && (e = "&"), u = f + e + $.param(a), t && (o = i.find("table.item-table").length, s = "&startRow=" + o, u += s), u
    }

    function e(n, t) {
        var f = t.find(".build-loading-container").html(),
            u;
        t.find(".items-container").html(f);
        var e = t.find(r).val(),
            i = t.find(".content-area .content-title .aside-text"),
            s = t.find(".breadCrumb .breadCrumbContext"),
            h = t.find(r + " option:selected").text();
        t.find(".context-game-separator").hide(), t.find(".breadCrumbGame").hide(), e != "NonGameCreation" ? (t.find(".show-active-places-only").hide(), t.find(".creation-context-breadcrumb").show(), s.text(h), i.hide(), n && (t.find(".context-game-separator").show(), t.find(".breadCrumbGame").show())) : (t.find(".creation-context-breadcrumb").hide(), i.show(), t.find(".show-active-places-only").show()), u = o(!1, t), t.find(".items-container").load(u)
    }

    function f(t, i) {
        i.find(n).data("universeid", 0), i.find(n).data("fetchuniverseplaces", !1), e(t, i)
    }

    function u(n, t, i) {
        n.hide(), $.ajax({
            url: t,
            cache: !1,
            dataType: "html",
            success: function(t) {
                n.remove();
                var u = i.find(".items-container"),
                    r = $(t).hide();
                u.append(r), r.fadeIn(), r.find("a[data-retry-url]").loadRobloxThumbnails()
            },
            fail: function() {
                n.show()
            }
        })
    }

    function l(n, t) {
        Roblox.GenericConfirmation.open({
            titleText: "Shut Down Servers",
            bodyContent: t.hasOwnProperty("placeId") ? "Are you sure you want to shut down all servers for this place?" : "Are you sure you want to shut down all servers in all places in this game?",
            onAccept: function() {
                $.ajax({
                    type: "POST",
                    url: n,
                    data: t,
                    error: function() {
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

    function a(n, r) {
        var u;
        if (t == this) return i();
        t && i(), t = this, u = $(this), u.addClass("gear-open");
        var e = u.closest("table"),
            f = e.data("item-id"),
            o = e.data("item-moderation-approved");
        o = o === "True", r.find("a").each(function() {
            var n = $(this),
                i = n.hasClass("advertise-link"),
                r = n.data("href-template"),
                s, t;
            r && (s = r.replace(/=0/g, "=" + f).replace(/\/0\//g, "/" + f + "/").replace(/\/0$/, "/" + f), n.attr("href", s)), n.attr("data-place-id") && n.attr("data-place-id", f), n.attr("data-item-id") && n.attr("data-item-id", f), e.data("runnable") === "False" && n.data("ad-activate-link") === "Run" ? n.hide() : e.data("runnable") === "True" && n.data("ad-activate-link") === "Run" && n.show(), u.data("is-sponsored-game") && (n.data("parent-sponsored-game-element", u.parents(".sponsored-game")), n.hasClass("dropdown-item-run-sponsored-game") && n.toggle(u.data("show-run")), n.hasClass("dropdown-item-stop-sponsored-game") && n.toggle(u.data("show-stop"))), n.data("href-reference") && n.attr("href", e.data(n.data("href-reference"))), n.hasClass("shutdown-all-servers-button") && (e.data("type") == "universes" ? n.attr("data-universe-id", f).removeAttr("data-place-id") : n.attr("data-place-id", f).removeAttr("data-universe-id")), t = e.data("rootplace-id"), n.data("require-root-place") && !t ? n.hide() : n.data("require-root-place") && t && (n.show(), n.data("configure-place-template") && n.attr("href", n.data("configure-place-template").replace(/\/\d+\//, "/" + t + "/"))), i && !o ? n.hide() : i && o && n.show()
        }), $("#configure-localization-link").click(function() {
            Roblox && Roblox.EventStream && Roblox.EventStream.SendEventWithTarget("formInteraction", "Create", {
                universeId: f
            }, Roblox.EventStream.TargetTypes.WWW)
        });
        var s = r.parent().offset(),
            c = r.outerWidth(),
            h = u.offset();
        return r.css({
            top: h.top - s.top + 21 + u.outerHeight() + 9 + "px",
            left: h.left - s.left + 15 - c + u.outerWidth() + "px"
        }).show(), u.parent().css({
            "background-color": "#EFEFEF",
            "border-color": "gray",
            "z-index": 999
        }), n.preventDefault(), !1
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
        $.post(e, f, function(i) {
            i.success ? Roblox.GenericConfirmation.open({
                titleText: Roblox.BuildPage.Resources.purchaseComplete,
                bodyContent: Roblox.BuildPage.Resources.youHaveBid + "<span class='currency CurrencyColor1'>" + t + "</span> .",
                acceptText: Roblox.BuildPage.Resources.ok,
                acceptColor: Roblox.GenericConfirmation.blue,
                declineColor: Roblox.GenericConfirmation.none,
                onAccept: function() {
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
                onAccept: function() {
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
        $.post(r, t, function(n) {
            n.success ? Roblox.GenericConfirmation.open({
                titleText: Roblox.BuildPage.Resources.adDeleted,
                bodyContent: Roblox.BuildPage.Resources.theAdWasDeleted,
                acceptText: Roblox.BuildPage.Resources.ok,
                acceptColor: Roblox.GenericConfirmation.blue,
                declineColor: Roblox.GenericConfirmation.none,
                onAccept: function() {
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
        p = $("#GroupCreationsTab " + r),
        c = $("#MyCreationsTab #build-dropdown-menu"),
        s = $("#GroupCreationsTab #build-dropdown-menu"),
        t = null,
        k = $("#assetLinks").data("asset-links-enabled"),
        y;
    $("a[data-retry-url]").loadRobloxThumbnails(), w.change(function() {
        f(!0, $("#MyCreationsTab"))
    }), p.change(function() {
        f(!0, $("#GroupCreationsTab"))
    });
    $("#MyCreationsTab .items-container").on("click", ".view-places-button", function() {
        var t = $("#MyCreationsTab"),
            i = $(this),
            r = i.data("universeid"),
            u = i.data("universename");
        return t.find(n).data("universeid", r), t.find(n).data("fetchuniverseplaces", !0), t.find(".breadCrumbGame").text(u), t.find(".context-game-separator").show(), t.find(".breadCrumbGame").show(), e(!0, t), !1
    });
    $("#GroupCreationsTab .items-container").on("click", ".view-places-button", function() {
        var t = $("#GroupCreationsTab"),
            i = $(this),
            r = i.data("universeid"),
            u = i.data("universename");
        return t.find(n).data("universeid", r), t.find(n).data("fetchuniverseplaces", !0), t.find(".breadCrumbGame").text(u), t.find(".context-game-separator").show(), t.find(".breadCrumbGame").show(), e(!0, t), !1
    });
    $("#MyCreationsTab").on("click", ".breadCrumbContext", function() {
        return f(!1, $("#MyCreationsTab")), !1
    });
    $("#GroupCreationsTab").on("click", ".breadCrumbContext", function() {
        return f(!1, $("#GroupCreationsTab")), !1
    });
    $("#MyCreationsTab .items-container").on("click", ".load-more-places", function() {
        var t = $(this),
            n = $("#MyCreationsTab"),
            i = o(!0, n);
        return u(t, i, n), !1
    });
    $("#GroupCreationsTab .items-container").on("click", ".load-more-places", function() {
        var t = $(this),
            n = $("#GroupCreationsTab"),
            i = o(!0, n);
        return u(t, i, n), !1
    });
    $(".BuildPageContent").on("click", "a.roblox-edit-button", function() {
        var n, t, i, r;
        $(".build-page").data("edit-opens-studio") != "False" || Roblox.Client.isIDE() ? (n = $(this).closest("table"), t = n.data("rootplace-id") || n.data("item-id"), window.play_placeId = t, i = n.data("universeid") || n.data("item-id"), r = $("#PlaceLauncherStatusPanel").data("is-protocol-handler-launch-enabled") == "True", n.data("type") == "app-place" ? Roblox.GenericConfirmation.open({
            titleText: Roblox.BuildPage.Resources.appStudioTitle,
            bodyContent: Roblox.BuildPage.Resources.appStudioBody,
            acceptText: Roblox.BuildPage.Resources.continueText,
            acceptColor: Roblox.GenericConfirmation.blue,
            declineText: Roblox.BuildPage.Resources.cancel,
            imageUrl: "/images/Icons/img-alert.png",
            onAccept: function() {
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
    $("body").on("click", "a.shutdown-all-servers-button[data-universe-id]", function() {
        return l("/universes/shutdown-all-games", {
            universeId: $(this).data("universe-id")
        }), !1
    });
    $("body").on("click", "a.shutdown-all-servers-button[data-place-id]", function() {
        return l("/games/shutdown-all-instances", {
            placeId: $(this).data("place-id")
        }), !1
    });
    $(".BuildPageContent").on("mouseover", "a.gear-button", function() {
        $(this).addClass("gear-hover")
    });
    $(".BuildPageContent").on("mouseout", "a.gear-button", function() {
        $(this).removeClass("gear-hover")
    });
    $("#MyCreationsTab").on("click", "a.gear-button", function(n) {
        return a.apply(this, [n, c])
    });
    $("#GroupCreationsTab").on("click", "a.gear-button", function(n) {
        return a.apply(this, [n, s])
    });
    $(document).click(function() {
        i()
    }), $(window).resize(i), $("input[data-bid-now-amount]").filter_input({
        regex: "[0-9]"
    });
    $(".items-container").on("click", "a.runnable[data-ad-status-toggle]", function() {
        var n = $(this).closest("table.item-table");
        return h(n), !1
    });
    $("a[data-ad-activate-link]").click(function() {
        var n = $(t).closest("table.item-table");
        return h(n), !1
    }), $("a[data-ad-remove-link]").click(function() {
        var n = $(t).closest("table.item-table"),
            i = n.data("item-id");
        return Roblox.GenericConfirmation.open({
            titleText: Roblox.BuildPage.Resources.confirmDelete,
            bodyContent: Roblox.BuildPage.Resources.areYouSureDelete,
            acceptText: Roblox.BuildPage.Resources.ok,
            declineText: Roblox.BuildPage.Resources.cancel,
            acceptColor: Roblox.GenericConfirmation.blue,
            declineColor: Roblox.GenericConfirmation.gray,
            onAccept: function() {
                b(i)
            },
            allowHtmlContentInBody: !1,
            dismissable: !0
        }), !1
    });
    $(".items-container").on("click", "a.cancel-ad-buy", function() {
        var n = $(this).closest("table.item-table"),
            t = n.find("tr.bid-now-row"),
            i = n.find("input[data-bid-now-amount]");
        return i.val(""), t.hide(), !1
    });
    $(".items-container").on("click", "a.process-ad-buy", function() {
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
            onAccept: function() {
                v(o, n, !1, s)
            },
            onDecline: "",
            allowHtmlContentInBody: !0,
            dismissable: !0
        }), !1
    });
    $("#MyCreationsTab .items-container").on("click", ".load-more-ads", function() {
        var n = $(this),
            t = $("#MyCreationsTab"),
            i = n.attr("data-next-start"),
            r = "/build/ads?startRow=" + i;
        return u(n, r, t), !1
    });
    $("#GroupCreationsTab .items-container").on("click", ".load-more-ads", function() {
        var n = $(this),
            t = $("#GroupCreationsTab"),
            i = n.attr("data-next-start"),
            r = t.find(".BuildPageContent").data("groupid"),
            f = "/build/ads?startRow=" + i + "&groupId=" + r;
        return u(n, f, t), !1
    });
    $("a.item-image.ad-image").click(function() {
        var t = $(this).closest("table.item-table"),
            i = t.data("item-id"),
            r = "/user-sponsorship/getadimage?adId=" + i;
        $.ajax({
            url: r,
            success: function(n) {
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
    $("#GroupCreationsTab .groups-dropdown-container").on("change", "select", function() {
        var n = $(this).val(),
            t = $("#GroupCreationsTab .groups-dropdown-container").data("get-url");
        window.location = t + "/" + n + y
    })
});