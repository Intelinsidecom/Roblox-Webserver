// Places/DeveloperProductsListing.js
$(function() {
    Roblox || (Roblox = {}), Roblox.DeveloperProductsListing || (Roblox.DeveloperProductsListing = {}), Roblox.DeveloperProductsListing.init = function() {
        var n = $("#DevProducts").parent();
        n.trigger("onViewChange", ["listing"]), $("#DevProducts").bind("click", function(n) {
            var t = $(n.target);
            return (t.is("span.next") || t.is("span.previous") ? t = t.closest("a") : t.is("td.edit") ? t = $("a.edit", t) : t.is("div.createNewButtonSection") && (t = $("#createNewButton", t)), t.is(".nextPager") || t.is(".prevPager") || t.is("a.edit") || t.is("#createNewButton")) ? (t.data("url").length > 0 && (Roblox.DeveloperProductsListing.onAjaxStart(), $.ajax({
                cache: !1,
                type: "GET",
                url: t.data("url")
            }).done(function(n) {
                Roblox.DeveloperProductsListing.onDeveloperProductsReceived(n, $("#DevProducts").parent())
            }).fail(function() {
                $("#DeveloperProductsLoading").hide(), $("#DeveloperProductsError").show()
            })), !1) : !0
        }), n.unbind("onRefreshed").bind("onRefreshed", function() {
            var t = $(this);
            Roblox.DeveloperProductsListing.onAjaxStart(), $.ajax({
                cache: !1,
                type: "GET",
                url: t.attr("src")
            }).done(function(n) {
                Roblox.DeveloperProductsListing.onDeveloperProductsReceived(n, t)
            }).fail(function() {
                $("#DeveloperProductsLoading").hide(), $("#DeveloperProductsError").height($("#DeveloperProductsInnerContainer").height()), $("#DeveloperProductsError").show()
            })
        }), n.unbind("onActionComplete").bind("onActionComplete", function(n, t) {
            Roblox.DeveloperProductsListing.onDeveloperProductsReceived(t, $(this));
            Roblox.DeveloperProductsListing.init()
        })
    }, Roblox.DeveloperProductsListing.init(), Roblox.DeveloperProductsListing.onAjaxStart = function() {
        var n = $("#DeveloperProductsInnerContainer");
        n.hide(), $("#DeveloperProductsLoading").height(n.height()), $("#DeveloperProductsLoading").show(), $("#DeveloperProductsError").hide()
    }, Roblox.DeveloperProductsListing.onDeveloperProductsReceived = function(n, t) {
        $("#DeveloperProductsLoading").hide(), t.html(n), Roblox.DeveloperProductsForm.init(), Roblox.DeveloperProductsListing.init()
    }
});