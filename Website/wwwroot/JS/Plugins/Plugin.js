// Plugins/Plugin.js
var Roblox = Roblox || {};
typeof Roblox.Plugins == "undefined" && (Roblox.Plugins = {}), Roblox.Plugins.Init = function() {
    function u() {
        $(".InstallButton").each(function(n, t) {
            $(t).unbind().click(function() {
                return $(t).hasClass("btn-disabled-primary") || e(t), !1
            })
        }), function() { try { f() } catch(e) {} }(), $(".btn-disabled-primary").removeClass("Button").tipsy({
            gravity: "s"
        }).attr("href", "javascript: return false;")
    }

    function n() {
        window.location.href = "/login/Default.aspx?ReturnUrl=" + encodeURIComponent(location.pathname + location.search)
    }

    function f() {
        var r = Roblox.Plugins.Resources.reinstall,
            u = Roblox.Plugins.Resources.updateText,
            n = $(".InstallButton"),
            t, i;
        if (!n.is(":visible") || typeof window.external == "undefined" || typeof window.external.InstallPlugin != "function") return;
        try {
            t = JSON.parse(window.external.GetInstalledPlugins());
            if (t.hasOwnProperty(n.data("item-id"))) {
                i = t[n.data("item-id")].AssetVersion;
                i != "undefined" && i < n.data("item-version-id") ? (n.text(u), n.unbind(), n.removeClass("InstallButton").addClass("UpdateButton"), $(".PluginMessageContainer").show(), $(".UpdateButton").click(function() {
                    var n = $(this);
                    return n.hasClass("btn-disabled-primary") || h(n), !1
                })) : (n.text(r), n.unbind(), n.removeClass("InstallButton").addClass("ReinstallButton"), $(".ReinstallButton").click(function() {
                    var n = $(this);
                    return n.hasClass("btn-disabled-primary") || s(n), !1
                }))
            } else {
                n.removeClass("btn-disabled-primary")
            }
        } catch(e) {
            n.removeClass("btn-disabled-primary")
        }
    }

    function e(r) {
        t("InstallingPluginView");
        var u = $(r);
        if (!u.hasClass("btn-disabled-primary")) {
            if (u.data("authenticateduser-isnull") === "True") {
                n();
                return
            }
            $.ajax({
                type: "POST",
                url: u.data("install-url"),
                success: function() {
                    window.external.PluginInstallComplete.connect(i), window.external.InstallPlugin(u.data("item-id"), u.data("item-version-id"))
                },
                error: function() {
                    $.modal.close(".InstallingPluginView"), Roblox.GenericConfirmation.open({
                        titleText: Roblox.Plugins.Resources.errorTitle,
                        bodyContent: Roblox.Plugins.Resources.errorBody,
                        imageUrl: Roblox.Plugins.Resources.alertImageUrl,
                        acceptColor: Roblox.GenericConfirmation.blue,
                        acceptText: Roblox.Plugins.Resources.ok,
                        declineColor: Roblox.GenericConfirmation.none,
                        dismissable: !1
                    })
                }
            })
        }
    }

    function i(n) {
        if ($.modal.close(".InstallingPluginView"), n) {
            var t = $(".InstallButton");
            t.length == 0 && (t = $(".ReinstallButton")), r(t), Roblox.GenericConfirmation.open({
                titleText: Roblox.Plugins.Resources.successTitle,
                bodyContent: t.data("item-name") + Roblox.Plugins.Resources.successBody,
                onAccept: function() {
                    window.location.reload()
                },
                acceptColor: Roblox.GenericConfirmation.blue,
                acceptText: Roblox.Plugins.Resources.ok,
                declineColor: Roblox.GenericConfirmation.none,
                dismissable: !1
            })
        } else Roblox.GenericConfirmation.open({
            titleText: Roblox.Plugins.Resources.errorTitle,
            bodyContent: Roblox.Plugins.Resources.errorBody,
            imageUrl: Roblox.Plugins.Resources.alertImageUrl,
            acceptColor: Roblox.GenericConfirmation.blue,
            acceptText: Roblox.Plugins.Resources.ok,
            declineColor: Roblox.GenericConfirmation.none,
            dismissable: !1
        })
    }

    function o(n) {
        if ($.modal.close(".UpdatingPluginView"), n) {
            var t = $(".UpdateButton");
            r(t), Roblox.GenericConfirmation.open({
                titleText: Roblox.Plugins.Resources.updateSuccessTitle,
                bodyContent: t.data("item-name") + Roblox.Plugins.Resources.updateSuccessBody,
                onAccept: function() {
                    window.location.reload()
                },
                acceptColor: Roblox.GenericConfirmation.blue,
                acceptText: Roblox.Plugins.Resources.ok,
                declineColor: Roblox.GenericConfirmation.none,
                dismissable: !1
            })
        } else Roblox.GenericConfirmation.open({
            titleText: Roblox.Plugins.Resources.updateErrorTitle,
            bodyContent: Roblox.Plugins.Resources.updateErrorBody,
            imageUrl: Roblox.Plugins.Resources.alertImageUrl,
            acceptColor: Roblox.GenericConfirmation.blue,
            acceptText: Roblox.Plugins.Resources.ok,
            declineColor: Roblox.GenericConfirmation.none,
            dismissable: !1
        })
    }

    function r(n) {
        var r = n.attr("data-product-id"),
            u = parseInt(n.attr("data-expected-price")),
            f = n.attr("data-expected-currency"),
            t = n.attr("data-placeproductpromotion-id"),
            e = n.attr("data-expected-seller-id"),
            i = n.attr("data-userasset-id");
        $.ajax({
            type: "POST",
            url: "/API/Item.ashx?rqtype=purchase&productID=" + r + "&expectedCurrency=" + f + "&expectedPrice=" + u + (t === undefined ? "" : "&expectedPromoID=" + t) + "&expectedSellerID=" + e + (i === undefined ? "" : "&userAssetID=" + i),
            contentType: "application/json; charset=utf-8"
        })
    }

    function s(r) {
        t("InstallingPluginView");
        var u = $(r);
        if (!u.hasClass("btn-disabled-primary")) {
            if (u.data("authenticateduser-isnull") === "True") {
                n();
                return
            }
            $.ajax({
                type: "POST",
                url: u.data("install-url"),
                success: function() {
                    window.external.PluginInstallComplete.connect(i), window.external.InstallPlugin(u.data("item-id"), u.data("item-version-id"))
                },
                error: function() {
                    $.modal.close(".InstallingPluginView"), Roblox.GenericConfirmation.open({
                        titleText: Roblox.Plugins.Resources.errorTitle,
                        bodyContent: Roblox.Plugins.Resources.errorBody,
                        imageUrl: Roblox.Plugins.Resources.alertImageUrl,
                        acceptColor: Roblox.GenericConfirmation.blue,
                        acceptText: Roblox.Plugins.Resources.ok,
                        declineColor: Roblox.GenericConfirmation.none,
                        dismissable: !1
                    })
                }
            })
        }
    }

    function h(i) {
        t("UpdatingPluginView");
        var r = $(i);
        if (!r.hasClass("btn-disabled-primary")) {
            if (r.data("authenticateduser-isnull") === "True") {
                n();
                return
            }
            $.ajax({
                type: "POST",
                url: r.data("install-url"),
                success: function() {
                    window.external.PluginInstallComplete.connect(o), window.external.InstallPlugin(r.data("item-id"), r.data("item-version-id"))
                },
                error: function() {
                    $.modal.close(".UpdatingPluginView"), Roblox.GenericConfirmation.open({
                        titleText: Roblox.Plugins.Resources.updateErrorTitle,
                        bodyContent: Roblox.Plugins.Resources.updateErrorBody,
                        imageUrl: Roblox.Plugins.Resources.alertImageUrl,
                        acceptColor: Roblox.GenericConfirmation.blue,
                        acceptText: Roblox.Plugins.Resources.ok,
                        declineColor: Roblox.GenericConfirmation.none,
                        dismissable: !1
                    })
                }
            })
        }
    }

    function t(n) {
        var t = {
            overlayClose: !1,
            opacity: 80,
            overlayCss: {
                backgroundColor: "#000"
            },
            escClose: !1
        };
        typeof n != "undefined" && n !== "" && $.modal.close("." + n), $("#" + n).modal(t)
    }
    return {
        init: u
    }
}();