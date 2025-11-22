// common/localeSwitcher.js
var Roblox = Roblox || {};
Roblox.LanguageSwitcher = function() {
    var t = function() {
            var t = Roblox.Lang && Roblox.Lang["CommonUI.Features"],
                n = {},
                i;
            n = t ? {
                heading: t["Heading.UnsupportedLanguage"],
                body: t["Description.UnsupportedLanguage"],
                ok: t["Action.Ok"]
            } : {
                heading: "Unsupported Language",
                body: "While some games may use the selected language, it is not fully supported by roblox.com.",
                ok: "OK"
            }, i = function() {
                location.reload()
            }, Roblox.Dialog.open({
                titleText: n.heading,
                bodyContent: n.body,
                acceptText: n.ok,
                showDecline: !1,
                onAccept: i,
                onCancel: i,
                onCloseCallback: i
            })
        },
        n = function(n, i) {
            var r = document.querySelector('meta[name="page-meta"]'),
                u = "",
                f, e, o;
            if (r && r.dataset && r.dataset.internalPageName && (u = r.dataset.internalPageName), u === "RollerCoaster" || u === "Landing") {
                f = Roblox.UrlParser.addOrUpdateQueryStringParameter(location.search, "locale", n), e = Roblox.Endpoints.getAbsoluteUrl(location.pathname + f), location.href = e;
                return
            }
            o = Roblox.EnvironmentUrls.localeApi + "/v1/locales/set-user-supported-locale", $.post(o, {
                supportedLocaleCode: n
            }).then(function(n) {
                n.success && (i ? location.reload() : t())
            }, function(n) {
                console.debug(n)
            })
        };
    $(function() {
        $("#language-switcher").change(function() {
            var t = $(this).val(),
                i = $("option:selected", this).data("isSupported");
            n(t, i)
        }), $(".locale-option").click(function() {
            var t = $(this).data("locale"),
                i = $(this).data("isSupported");
            n(t, i)
        })
    })
}();