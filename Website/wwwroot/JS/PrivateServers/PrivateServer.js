// PrivateServers/PrivateServer.js
var Roblox = Roblox || {};
Roblox.PrivateServer = function() {
    var h = ".tab-server-only .rbx-vip-server-item-container",
        g = "#rbx-vip-servers .rbx-vip-server-template",
        it = ".rbx-refresh",
        u = Roblox.Endpoints.getAbsoluteUrl("/images/transparent.gif"),
        c = 0,
        ft = $("#rbx-vip-servers").attr("data-slow-game-fps-threshold"),
        t = $(".rbx-vip-server-create"),
        ut = t.data("expected-price"),
        o = $("#rbx-vip-servers").attr("data-instance-list-url"),
        e = ($("#rbx-vip-servers").attr("data-is-user-authenticated") || "").toLowerCase() === "true",
        rt = $("#rbx-vip-servers").attr("data-renew-url"),
        f, n = $(h),
        s = {
            Active: 1,
            Inactive: 2,
            Canceled: 3
        },
        nt = {
            UserInitiated: 1,
            InsufficientFunds: 2,
            DisallowedByGameDeveloper: 3
        },
        r = !1;
    $(document).ready(function() {
        Roblox.PrivateServer.initCreateVipServerBtn()
    });
    $(".rbx-tab a[href='#game-instances']").on("shown.bs.tab", function() {
        Roblox.PrivateServer.initServerTab()
    });
    $(it).on("click", function() {
        Roblox.FriendsRunningGameInstances && Roblox.FriendsRunningGameInstances.fetchFirstServers(), Roblox.AllRunningGameInstances && Roblox.AllRunningGameInstances.fetchFirstServers(), Roblox.PrivateServer && Roblox.PrivateServer.fetchFirstServers()
    });
    var d = function() {
            if (Roblox.PrivateServer.canInit() !== !1) {
                n.on("click", ".rbx-vip-server-configure", function(n) {
                    n.preventDefault();
                    var t = $(n.target).parents(".rbx-vip-server-item").attr("data-server-id"),
                        i = $("#rbx-vip-servers").attr("data-configure-base-url"),
                        r = i.format(t);
                    window.location.href = r
                });
                n.on("click", ".rbx-vip-server-shutdown", function(n) {
                    n.preventDefault();
                    var t = $("#rbx-vip-servers").attr("data-placeid"),
                        i = $(n.target).parents(".rbx-vip-server-item").attr("data-gameinstance-id"),
                        r = $(n.target).parents(".rbx-vip-server-item").attr("data-server-id");
                    Roblox.GameInstance.shutdownInstance(t, i, r), $(this).parents(".rbx-vip-server-menu").find(".rbx-menu-item").popover("hide")
                });
                $(".rbx-vip-servers-footer").on("click", ".rbx-vip-servers-load-more", function(n) {
                    n.preventDefault();
                    var r = $("#rbx-vip-servers").attr("data-universeid"),
                        t = $(h),
                        u = t.attr("data-current-page"),
                        f = t.attr("data-total-pages"),
                        i = u + 1;
                    i <= f && Roblox.PrivateServer.fetchServers(r, i)
                });
                Roblox.PrivateServer.fetchFirstServers()
            }
        },
        k = function() {
            t.on("click", function(n) {
                n.preventDefault();
                var i;
                try {
                    i = $("#private-server-purchase-body-content").html(), t.attr("data-purchase-body-content", i), $("#ItemPurchaseAjaxData").attr("data-footer-text", t.attr("data-footer-text")), Roblox.PrivateServer.privateServerPurchaseItem.openPurchaseVerificationView("#rbx-vip-servers .rbx-vip-server-create"), f = $(".modal-body .private-server-name"), $(".modal-body .modal-top-body").addClass("private-server-purchase-modal")
                } catch (n) {}
            })
        },
        b = function() {
            return c > 0 ? !1 : (c++, e)
        },
        w = function() {
            n.empty()
        },
        p = function(t, i) {
            o && e && (i = i == undefined || i < 1 ? 1 : i, $.ajax({
                type: "GET",
                url: o,
                data: {
                    universeId: t,
                    page: i
                },
                cache: !1,
                contentType: "application/json; charset=utf-8",
                success: function(t) {
                    var e = t.Instances,
                        i = $("<div></div>"),
                        u = t.CurrentPage,
                        o = t.TotalPages,
                        s = $(".rbx-vip-servers-load-more"),
                        h, c, f;
                    n.attr("data-current-page", u), n.attr("data-total-pages", o), $.each(e, function(n, t) {
                        i.append(v(t))
                    }), u === 1 ? n.html(i.html()) : n.append(i.html()), c = function(n, t) {
                        if (r = !1, t && t.success) {
                            var i = $("#rbx-vip-servers").attr("data-configure-base-url");
                            i && (document.location.href = i.format(n))
                        } else f()
                    }, f = function() {
                        r = !1;
                        var n = $(".rbx-vip-server-create").attr("data-continueshopping-url");
                        n && (document.location.href = n)
                    }, h = function(n) {
                        r || (r = !0, $.ajax({
                            type: "POST",
                            url: rt,
                            data: {
                                __RequestVerificationToken: $("[name=__RequestVerificationToken]").val(),
                                privateServerId: n,
                                expectedPrice: ut
                            },
                            success: function(t) {
                                c(n, t)
                            },
                            error: f
                        }))
                    };
                    n.find(".rbx-vip-server-renew").on("click", function(n) {
                        var t = $(n.target).parents(".rbx-vip-server-item").attr("data-server-id");
                        n.preventDefault(), Roblox.Dialog.open({
                            titleText: Roblox.PrivateServers.RenewRecurringTitle,
                            bodyContent: Roblox.PrivateServers.RenewRecurringBody,
                            acceptText: Roblox.PrivateServers.RenewRecurringAcceptText,
                            acceptColor: Roblox.Dialog.green,
                            onAccept: function() {
                                h(t)
                            },
                            declineText: Roblox.PrivateServers.RenewRecurringDeclineText,
                            allowHtmlContentInBody: !0,
                            dismissable: !0
                        })
                    });
                    e.length === 0 && n.append($("<p class='section-content-off'> No VIP Server Instances Found. </p>")), u < o ? s.removeClass("hidden") : s.addClass("hidden"), Roblox.PrivateServer.populateAvatarImages(), Roblox.PrivateServer.bindPopovers()
                },
                error: function() {
                    n.find(".loading").remove().append("<p class='empty-server-list'>Sorry, something went wrong loading places.</p>")
                }
            }))
        },
        y = function() {
            var n = $("#rbx-vip-servers").attr("data-universeid");
            Roblox.PrivateServer.fetchServers(n, 1)
        },
        v = function(n) {
            var t = $(g).clone(),
                w = n.PlaceCapacity,
                f = [],
                r, l;
            n.GameInstance != null && (f = n.GameInstance.PlayerIds);
            var v = f.length,
                i = Math.floor(Math.random() * 1e6),
                a = '<a class="rbx-menu-item" data-toggle="popover-dynamic" data-bind="game-vip-server-context-menu-' + i + '" data-original-title="" title="" data-viewport=".rbx-vip-server-item" ><span class="icon-more"></span></a><div class="rbx-popover-content" data-toggle="game-vip-server-context-menu-' + i + '"><ul class="dropdown-menu" role="menu"><li><a href="#" class="rbx-vip-server-configure">Configure</a></li><li><a href="#" class="rbx-vip-server-shutdown rbx-vip-server-shutdown">Shut Down This Server</a></li></ul>',
                d = n.Name,
                c = n.PrivateServer.OwnerUserId,
                h = n.PrivateServerOwnerName,
                o = n.PrivateServer.StatusType === s.Canceled && n.MostRecentPrivateServerStatusChangeReasonType === nt.InsufficientFunds,
                y = n.PrivateServer.StatusType === s.Inactive;
            t.find(".rbx-vip-server-item .font-bold").text(d), t.find(".rbx-vip-server-status").text(v + " of " + w + " Players Max"), t.find(".rbx-vip-server-join").attr("data-placeid", n.PlaceId), t.find(".rbx-vip-server-item").attr("data-universeid", n.PrivateServer.UniverseId).attr("data-does-belong-to-user", n.DoesBelongToUser).attr("data-server-id", n.PrivateServer.Id).attr("data-is-cancelled-insufficient-funds", o), n.GameInstance && t.find(".rbx-vip-server-item").attr("data-gameinstance-id", n.GameInstance.Id), n.UserCanConfigure && n.UserCanShutdown && t.find(".rbx-vip-server-menu").html(a), t.find(".rbx-menu-item").attr("data-bind", "game-vip-server-context-menu-" + i), t.find(".rbx-popover-content").attr("data-toggle", "game-vip-server-context-menu-" + i), n.PrivateServer && n.PrivateServer.StatusType !== 1 ? t.find(".rbx-vip-server-join").hide() : t.find(".rbx-vip-server-join").attr("onclick", n.JoinScript);
            var p = "data-retry-url='/avatar-thumbnail/json?userId=" + c + "&width=100&height=100&format=PNG'",
                e = Roblox.PrivateServers.UserProfileAbsoluteUrlPattern.replace("/users/0/profile", "/users/" + c + "/profile"),
                b = "<a class='avatar avatar-card-fullbody owner-avatar' href='" + Roblox.Endpoints.getAbsoluteUrl(e) + "' " + p + " title='" + h + "'><img class='avatar-card-image' src='" + u + "'></a>",
                k = "<a href='" + Roblox.Endpoints.getAbsoluteUrl(e) + "' class='text-name'>" + h + "</a>";
            return t.find(".rbx-vip-owner").html(b + k), n.GamesInstance != null && n.GamesInstance.Fps > ft ? t.find(".rbx-vip-server-alert").addClass("hidden") : n.GamesInstance == null && t.find(".rbx-vip-server-alert").addClass("hidden"), (!n.DoesBelongToUser || n.IsPrivateServerSubscriptionActive) && t.find(".rbx-vip-server-subscription-alert").addClass("hidden"), n.CanRenew || t.find(".rbx-vip-server-renew").addClass("hidden"), o && n.DoesBelongToUser ? t.find(".rbx-vip-server-item").addClass("rbx-vip-server-cancelled-insufficient-funds") : t.find(".rbx-vip-server-insufficient-funds").addClass("hidden"), y && n.DoesBelongToUser ? t.find(".rbx-vip-server-item").addClass("rbx-vip-server-inactive") : t.find(".rbx-vip-server-inactive").addClass("hidden"), r = "", l = "headshot-thumbnail", $.each(f, function(n, t) {
                var i = "data-retry-url='/" + l + "/json?userId=" + t + "&width=48&height=48&format=PNG'";
                r += t > 0 ? "<span class='avatar avatar-headshot-sm player-avatar'><a class='avatar-card-link' href='" + Roblox.Endpoints.getAbsoluteUrl("/users/" + t + "/profile") + "' " + i + " ><img class='avatar-card-image' src='" + u + "'></a></span>" : "<span class='avatar avatar-headshot-sm player-avatar'><a class='avatar-card-link' " + i + " ><img class='avatar-card-image' src='" + u + "'></a></span>"
            }), t.find(".rbx-vip-server-players").html(r), t = t.html()
        },
        a = function() {
            $("#rbx-vip-servers .player-avatar a").loadRobloxThumbnails(), $("#rbx-vip-servers .owner-avatar").loadRobloxThumbnails()
        },
        l = function() {
            $('#rbx-vip-servers [data-toggle="popover-dynamic"]').popover({
                html: !0,
                placement: "bottom",
                content: function() {
                    var n = $(this).attr("data-bind");
                    return $('[data-toggle="' + n + '"]').html()
                }
            })
        },
        tt = function(n) {
            $(".modal-body .private-server-name-input .form-group").addClass("form-has-error form-has-feedback"), $(".modal-body .private-server-name-error-message").text(n).show()
        },
        i = Roblox.ItemPurchase(function() {}, function(n) {
            var o, r, u, s, e, h;
            return (u = $("#rbx-vip-servers"), r = f.val(), e = u.attr("data-private-server-name-max-length"), h = u.attr("data-private-server-name-error-text"), r.trim().length < 1 || r.length > e) ? (tt(h.format(e)), !1) : (o = t.attr("data-purchase-url"), s = {
                __RequestVerificationToken: $("[name=__RequestVerificationToken]").val(),
                universeId: t.attr("data-universe-id"),
                privateServerName: r,
                productId: n.productId,
                expectedCurrency: n.expectedCurrency,
                expectedPrice: n.expectedPrice,
                expectedSellerId: n.expectedSellerId
            }, $.ajax({
                type: "POST",
                url: o,
                data: s,
                success: function(n) {
                    var t;
                    Roblox.Dialog.toggleProcessing(!0, "PurchaseVerificationView"), "statusCode" in n && n.statusCode !== 200 ? i.openErrorView(n) : ("status" in n || (n = {
                        status: "error",
                        showDivID: "TransactionFailureView",
                        title: "Transaction Failed",
                        errorMsg: "Transaction encountered an unexpected error."
                    }), n.status === "success" ? (t = $("#rbx-vip-servers").attr("data-configure-base-url").format(n.PrivateServerId), $("#ItemPurchaseAjaxData").attr("data-continueshopping-url", t), i.openPurchaseConfirmationView(n)) : i.openErrorView(n))
                },
                error: function(n) {
                    var t = $.parseJSON(n.responseText);
                    i.openErrorView(t)
                }
            }), !1)
        });
    return {
        initServerTab: d,
        canInit: b,
        initCreateVipServerBtn: k,
        fetchServers: p,
        fetchFirstServers: y,
        populateAvatarImages: a,
        bindPopovers: l,
        clearInstances: w,
        privateServerPurchaseItem: i
    }
}();