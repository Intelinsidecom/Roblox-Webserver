// Item/ContextMenu.js
$(function() {
    function w() {
        var t = $("#sell-content-wrapper"),
            n = t.find(".sell-price").val(),
            i = t.find(".price-form"),
            u = i.find(".form-control-label"),
            f = t.find(".commission"),
            e = t.find(".after-commission");
        if (!$.isNumeric(n) || Math.floor(n) != n || n <= 0) return i.addClass("form-has-error"), u.removeClass("invisible"), f.text(""), e.text(""), !1;
        i.removeClass("form-has-error"), u.addClass("invisible");
        var o = Math.round(n * h),
            r = o > 1 ? o : 1,
            s = n - r;
        return f.text(r > 0 ? Roblox.NumberFormatting.commas(r) : ""), e.text(s >= 0 ? Roblox.NumberFormatting.commas(s) : ""), !0
    }

    function u(n) {
        c.text(n), Roblox.BootstrapWidgets.ToggleSystemMessage(c, e, o)
    }

    function t(n) {
        f.text(n), Roblox.BootstrapWidgets.ToggleSystemMessage(f, e, o)
    }

    function r(n) {
        u(n), setTimeout(function() {
            window.location.reload()
        }, 1e3)
    }

    function b(n, t, i, r, f) {
        u(n);
        var e = $(".rbx-popover-content").find(i);
        t ? e.attr("data-toggle", "False").text(r) : e.attr("data-toggle", "True").text(f)
    }

    function k() {
        $.post(v, a, function(n) {
            n.isValid ? r("Successfully removed from your inventory") : t("Failed to delete item from inventory")
        })
    }

    function d() {
        var n = $("#item-container").data("item-id");
        $.post("/badges/enable?badgeId=" + n, null, function(n) {
            n.success ? r("Successfully enabled the badge") : t("Failed to enable badge")
        })
    }

    function g() {
        var n = $("#item-container").data("item-id");
        $.post("/badges/disable?badgeId=" + n, null, function(n) {
            n.success ? r("Successfully disabled the badge") : t("Failed to disable badge")
        })
    }

    function nt() {
        var r = $("#sell-content-wrapper"),
            n = r.find($(".sell-price")).val();
        if (!$.isNumeric(n) || Math.floor(n) != n || n <= 0) return !1;
        var u = parseInt($(".below-market-warning").attr("data-low-price-warning-percentage")),
            t = parseInt($("#item-average-price").text().replace(/,/g, "")),
            f = t * u / 100,
            i = Number($("#sell-content-wrapper .serial-dropdown").val());
        return n < t - f ? Roblox.Dialog.open({
            titleText: "Sell Your Collectible Item",
            bodyContent: $("#confirm-sell-modal").clone().attr("id", "confirm-sell-content-wrapper").removeClass("hidden"),
            onAccept: function() {
                Roblox.Resellers.sellItem(n, i)
            },
            fieldValidationRequired: !0,
            allowHtmlContentInBody: !0,
            acceptText: "Yes",
            declineText: "Cancel",
            xToCancel: !0,
            onOpenCallback: function() {
                var i = $("#confirm-sell-content-wrapper");
                i.find(".attempted-sell-price").text(Roblox.NumberFormatting.commas(parseInt(n))), i.find(".average-price").text(Roblox.NumberFormatting.commas(parseInt(t)))
            }
        }) : (Roblox.Dialog.close(), Roblox.Resellers.sellItem(n, i)), !1
    }

    function p() {
        window.location = Roblox.Endpoints.getAbsoluteUrl("/premium/membership")
    }
    var c = $(".content .alert-success"),
        o = 2e3,
        e = 100,
        h = .25,
        s = $("#sell-modal-content .commission-label").data("rate"),
        i = $("#item-container"),
        tt = i.data("item-id"),
        v = "/asset/delete-from-inventory",
        a = {
            assetId: tt
        },
        l = i.data("delete-url"),
        y, f, n;
    l && (v = l, y = i.data("delete-id"), a = {
        id: y
    }), f = $(".content .alert-warning"), $.isNumeric(s) && (h = s);
    $("body").on("keyup", ".sell-price", function() {
        w()
    });
    n = !1;
    $("#item-context-menu").on("click", ".popover-content .toggle-wear", function(r) {
        if (r.preventDefault(), !n) {
            n = !0;
            var f = $(".popover-content .toggle-wear").data("toggle");
            $("#item-context-menu").find(".rbx-menu-item").popover("hide"), $.ajax({
                type: "POST",
                url: i.data(f ? "avatar-remove-url" : "avatar-wear-url"),
                success: function() {
                    $(".rbx-popover-content .toggle-wear").attr("data-toggle", !f).text(f ? "Wear" : "Take Off"), u(f ? "Removed from your avatar" : "Added to your avatar"), n = !1
                },
                error: function(i) {
                    n = !1;
                    var r = i.responseJSON.errors && i.responseJSON.errors.length > 0 ? i.responseJSON.errors[0].message : "Error occured";
                    t(r)
                }
            })
        }
    });
    $("#item-context-menu").on("click", ".popover-content .toggle-profile", function(n) {
        n.preventDefault();
        var i = $(".popover-content").find(".toggle-profile").attr("data-toggle") === "True",
            r = $("#item-container").attr("data-item-id");
        $("#item-context-menu").find(".rbx-menu-item").popover("hide"), $.post("/asset/toggle-profile", {
            assetId: r,
            addToProfile: !i
        }, function(n) {
            if (n.isValid) {
                var r = i ? "Removed from your profile" : "Added to your profile";
                b(r, i, ".toggle-profile", "Add to Profile", "Remove from Profile")
            } else t(i ? "Failed to remove from profile" : "Failed to add to profile")
        })
    });
    $("#item-context-menu").on("click", ".popover-content #delete-item", function() {
        Roblox.Dialog.open({
            titleText: "Delete Item",
            bodyContent: "Are you sure you want to permanently <span class='font-bold'>DELETE</span> this item from your inventory?",
            onAccept: k,
            xToCancel: !0,
            allowHtmlContentInBody: !0
        })
    });
    $("#item-context-menu").on("click", ".popover-content #enable-badge", function() {
        Roblox.Dialog.open({
            titleText: "Enable Badge",
            bodyContent: "Are you sure you want to enable this Badge?",
            onAccept: d,
            xToCancel: !0
        })
    });
    $("#item-context-menu").on("click", ".popover-content #disable-badge", function() {
        Roblox.Dialog.open({
            titleText: "Disable Badge",
            bodyContent: "Are you sure you want to disable this Badge?",
            onAccept: g,
            xToCancel: !0
        })
    });
    $("#item-context-menu").on("click", ".popover-content #sell", function() {
        var n = $("#ItemPurchaseAjaxData");
        n.attr("data-user-bc") == "0" ? Roblox.Dialog.open({
            titleText: "Sell Your Collectible Item",
            bodyContent: "Only Builders Club members can re-sell collectible items. Get Builders Club today!",
            onAccept: p,
            acceptText: "Upgrade",
            declineText: "Cancel",
            xToCancel: !0
        }) : ($("#sell-modal-content").find(".form-control-label").addClass("invisible"), Roblox.Dialog.open({
            titleText: "Sell Your Collectible Item",
            bodyContent: $("#sell-modal-content").clone().attr("id", "sell-content-wrapper").removeClass("hidden"),
            onAccept: nt,
            fieldValidationRequired: !0,
            allowHtmlContentInBody: !0,
            acceptText: "Sell Now",
            declineText: "Cancel",
            xToCancel: !0
        }))
    });
    $("#item-context-menu").on("click", ".popover-content #take-off-sale", function() {
        Roblox.Dialog.open({
            titleText: "Take Off Sale",
            bodyContent: $("#take-off-sale-modal-content").clone().attr("id", "take-off-sale-content-wrapper").removeClass("hidden"),
            onAccept: function() {
                var n = Number($("#take-off-sale-content-wrapper .serial-dropdown").val());
                Roblox.Resellers.takeItemOffSale(n)
            },
            fieldValidationRequired: !0,
            allowHtmlContentInBody: !0,
            acceptText: "Take Off Sale",
            declineText: "Cancel",
            xToCancel: !0
        })
    })
});