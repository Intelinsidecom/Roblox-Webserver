// Studio/Plugins/Manage.js
var Roblox = Roblox || {};
typeof Roblox.Plugins == "undefined" && (Roblox.Plugins = {}), Roblox.Plugins.Manage = function() {
    var n = [],
        f = function() {
            var u = window.external.GetInstalledPlugins(),
                i = 0,
                t;
            if (u.length > 0) {
                n = JSON.parse(window.external.GetInstalledPlugins());
                for (t in n) {
                    if (i >= 100) break;
                    e(t, n[t].AssetVersion), i++
                }
            }
            i == 0 && r()
        },
        e = function(r, f) {
            var e = $("#assetId").clone(),
                o;
            e.attr("id", r), o = e.find(".switch"), o.click(function(n) {
                n.preventDefault();
                var t;
                $(this).hasClass("active") ? ($(this).removeClass("active").addClass("inactive"), $(this).text("Inactive"), t = !1) : ($(this).removeClass("inactive").addClass("active"), $(this).text("Active"), t = !0), i(r, t)
            }), n[r].Enabled ? (o.removeClass("inactive").addClass("active"), o.text("Active")) : (o.removeClass("active").addClass("inactive"), o.text("Inactive")), $("#assetId").after(e), e.find(".close-x").click(function(n) {
                n.preventDefault();
                var i = e.find(".plugin-title");
                i.length > 0 && Roblox.GenericConfirmation.open({
                    titleText: Roblox.Plugins.Manage.Resources.removePluginTitle,
                    bodyContent: Roblox.Plugins.Manage.Resources.removePluginText.replace("{0}", i.text().replace(/\$/g, "&#36;")),
                    acceptColor: Roblox.GenericConfirmation.blue,
                    acceptText: Roblox.Plugins.Manage.Resources.accept,
                    declineText: Roblox.Plugins.Manage.Resources.decline,
                    dismissable: !1,
                    onAccept: function() {
                        t(e)
                    }
                })
            }), $.ajax({
                type: "GET",
                url: e.data("thumbnail-url") + "?assetId=" + r,
                success: function(n) {
                    var i, t;
                    e.prepend(n), Roblox.Plugins.PluginInfo.init(e.find(".more-less")), i = e.find(".asset-version-id").val(), i > f && (t = e.find(".update-button"), t.click(function(n) {
                        n.preventDefault(), $(this).unbind("click"), $(this).addClass("btn-disabled-neutral"), $(this).text(Roblox.Plugins.Manage.Resources.updatingText), window.external.PluginInstallComplete.connect(u), window.external.InstallPlugin(r, i)
                    }), e.find("span.no-updates").hide(), e.find("span.updates").css("display", "inline-block").show(), t.show())
                }
            }), e.show()
        },
        u = function(n, t) {
            var r = $("#" + t),
                i = r.find(".update-button");
            n ? (i.removeClass("btn-disabled-neutral").addClass("btn-disabled-primary"), i.text(Roblox.Plugins.Manage.Resources.updatedText), r.find("span.updates").hide()) : (i.removeClass("btn-disabled-neutral"), i.text(Roblox.Plugins.Manage.Resources.updateText), i.click(function(n) {
                n.preventDefault(), $(this).unbind("click"), $(this).addClass("btn-disabled-neutral"), $(this).text(Roblox.Plugins.Manage.Resources.updatingText), window.external.PluginInstallComplete.connect(u), window.external.InstallPlugin(t, r.find(".asset-version-id").val())
            }))
        },
        t = function(t) {
            delete n[t.attr("id")], window.external.UninstallPlugin(t.attr("id")), t.fadeOut(600, function() {
                var i = $(".error-bar").first().clone();
                i.find("span").text(Roblox.Plugins.Manage.Resources.pluginRemoveSuccessText), t.after(i), t.remove(), i.show(), $.map(n, function(n, t) {
                    return t
                }).length == 0 && r(), setTimeout(function() {
                    i.fadeOut(1e3, function() {
                        i.text(""), i.hide(), i.remove()
                    })
                }, 5e3)
            })
        },
        i = function(t, i) {
            window.external.SetPluginEnabled(t, i), n[t].enabled = i
        },
        r = function() {
            var n = $(document.createElement("span"));
            n.html(Roblox.Plugins.Manage.Resources.noPluginsFoundText), $("#assetId").after(n), n.show()
        };
    return {
        init: f,
        removePlugin: t,
        togglePlugin: i
    }
}();