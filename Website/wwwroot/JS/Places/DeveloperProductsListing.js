// Places/DeveloperProductsListing.js
$(function () {
    Roblox || (Roblox = {}), Roblox.DeveloperProductsListing || (Roblox.DeveloperProductsListing = {}),
        Roblox.DeveloperProductsListing.currentPage = 1;
    Roblox.DeveloperProductsListing.init = function () {
        var n = $("#DevProducts");
        n.trigger("onViewChange", ["listing"]);
        n.unbind("click");
        n.bind("click", function (event) {
            var t = $(event.target);
            if (t.is(".pager") || t.closest(".pager").length > 0) {
                var pagerElement = t.is(".pager") ? t : t.closest(".pager");
                if (!pagerElement.hasClass("disabled")) {
                    var targetPage = pagerElement.data("page");
                    var newPage;

                    if (targetPage !== undefined && targetPage !== null) {
                        newPage = parseInt(targetPage);
                    } else {
                        var currentPage = Roblox.DeveloperProductsListing.currentPage || parseInt($(".robloxDeveloperProductsPageNum").text()) || 1;
                        newPage = currentPage;
                        if (pagerElement.is(".pager.first")) {
                            newPage = 1;
                        } else if (pagerElement.is(".pager.previous")) {
                            newPage = Math.max(1, currentPage - 1);
                        } else if (pagerElement.is(".pager.next")) {
                            newPage = currentPage + 1;
                        } else if (pagerElement.is(".pager.last")) {
                            var pageText = $(".robloxDeveloperProductsPageNum").text();
                            var totalPagesText = pageText.split(" of ")[1];
                            var totalPages = parseInt(totalPagesText) || 1;
                            newPage = totalPages;
                        }
                    }

                    Roblox.DeveloperProductsListing.currentPage = newPage;

                    var baseUrl = $("#DevProducts").data("load-url");
                    var universeId = $("#DevProducts").data("universe-id");
                    if (baseUrl && universeId) {
                        var url = baseUrl + "?page=" + newPage;

                        Roblox.DeveloperProductsListing.onAjaxStart();
                        $.ajax({
                            cache: !1,
                            type: "GET",
                            url: url
                        }).done(function (n) {
                            Roblox.DeveloperProductsListing.onDeveloperProductsReceived(n, $("#DevProducts"))
                        }).fail(function () {
                            $("#DeveloperProductsLoading").hide(), $("#DeveloperProductsError").show()
                        });
                    }
                    return !1;
                }
                return !1;
            }

            if (t.is("a.edit") || t.closest("a.edit").length > 0) {
                var editElement = t.is("a.edit") ? t : t.closest("a.edit");
                if (editElement.length > 0 && editElement.data("url").length > 0) {
                    Roblox.DeveloperProductsListing.onAjaxStart();
                    $.ajax({
                        cache: !1,
                        type: "GET",
                        url: editElement.data("url")
                    }).done(function (n) {
                        Roblox.DeveloperProductsListing.onDeveloperProductsReceived(n, $("#DevProducts"))
                    }).fail(function () {
                        $("#DeveloperProductsLoading").hide(), $("#DeveloperProductsError").show()
                    });
                    return !1;
                }
            }

            if (t.is("div.createNewButtonSection") || t.closest("div.createNewButtonSection").length > 0) {
                var createButton = t.is("#createNewButton") ? t : t.closest("div.createNewButtonSection").find("#createNewButton");
                if (createButton.length > 0 && createButton.data("url").length > 0) {
                    Roblox.DeveloperProductsListing.onAjaxStart();
                    $.ajax({
                        cache: !1,
                        type: "GET",
                        url: createButton.data("url")
                    }).done(function (n) {
                        Roblox.DeveloperProductsListing.onDeveloperProductsReceived(n, $("#DevProducts"))
                    }).fail(function () {
                        $("#DeveloperProductsLoading").hide(), $("#DeveloperProductsError").show()
                    });
                    return !1;
                }
            }

            return !0;
        });

        n.unbind("onRefreshed").bind("onRefreshed", function () {
            var t = $(this);
            var loadUrl = $("#DevProducts").data("load-url");
            if (!loadUrl) return;
            Roblox.DeveloperProductsListing.onAjaxStart(), $.ajax({
                cache: !1,
                type: "GET",
                url: loadUrl
            }).done(function (n) {
                Roblox.DeveloperProductsListing.onDeveloperProductsReceived(n, t)
            }).fail(function () {
                $("#DeveloperProductsLoading").hide(), $("#DeveloperProductsError").height($("#DeveloperProductsInnerContainer").height()), $("#DeveloperProductsError").show()
            })
        });

        n.unbind("onActionComplete").bind("onActionComplete", function (n, t) {
            Roblox.DeveloperProductsListing.onDeveloperProductsReceived(t, $(this));
        });
    }, Roblox.DeveloperProductsListing.onAjaxStart = function () {
        var n = $("#DeveloperProductsLoading");
        if (n.length > 0) {
            n.show();
        }
        $("#DeveloperProductsContent, #DeveloperProductsTableContainer").hide();
        $("#DeveloperProductsError").hide()
    }, Roblox.DeveloperProductsListing.onDeveloperProductsReceived = function (n, t) {
        if (typeof n !== 'string' || n.length === 0) {
            return;
        }

        $("#DeveloperProductsLoading").hide();
        t.html(n);

        t.find("#DeveloperProductsContent, #DeveloperProductsTableContainer, #DeveloperProductsInnerContainer").show();
        t.find(".developerProductsContainer").show();

        Roblox.DeveloperProductsForm.init();

        t.find(".tooltip").tipsy();
        t.find(".tooltip-top").tipsy({ gravity: "s" });
        t.find(".tooltip-right").tipsy({ gravity: "w" });
        t.find(".tooltip-left").tipsy({ gravity: "e" });
        t.find(".tooltip-bottom").tipsy({ gravity: "n" });
        t.find(".TipsyImg").tipsy({ gravity: "w" });

        var domPage = parseInt($(".robloxDeveloperProductsPageNum").text()) || 1;
        Roblox.DeveloperProductsListing.currentPage = domPage;
        Roblox.DeveloperProductsListing.init();
    },

    Roblox.DeveloperProductsListing.loadContent = function () {
        var devProductsEl = $("#DevProducts");
        var loadUrl = devProductsEl.data("load-url");
        if (!loadUrl || devProductsEl.length === 0) {
            return;
        }

        Roblox.DeveloperProductsListing.onAjaxStart();
        $.ajax({
            cache: false,
            type: "GET",
            url: loadUrl
        }).done(function (n) {
            Roblox.DeveloperProductsListing.onDeveloperProductsReceived(n, devProductsEl)
        }).fail(function (xhr, status, error) {
            $("#DeveloperProductsLoading").hide();
            $("#DeveloperProductsError").show()
        });
    }
});
