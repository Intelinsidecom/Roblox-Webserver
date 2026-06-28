// Legacy/IDE/Welcome.js
$(function() {
    function i() {
        var t = $(this),
            i;
        !n && t.hasClass("app-place") && Roblox.GenericModal.open(Roblox.IDEWelcome.Resources.appStudioTitle, "/images/Icons/img-alert.png", Roblox.IDEWelcome.Resources.appStudioBody), Roblox.Client.isIDE() ? (i = t.attr("data-placeid"), window.play_placeId = i, window.editGameInStudio(i)) : Roblox.GenericModal.open(Roblox.IDEWelcome.Resources.editPlace, "/images/Icons/img-alert.png", Roblox.IDEWelcome.Resources.toEdit + t.find("p").text() + Roblox.IDEWelcome.Resources.openPage + "<a target='_blank' href='http://wiki.roblox.com/index.php/Studio'>" + Roblox.IDEWelcome.Resources.robloxStudio + "</a>.")
    }

    function u(n) {
        var t = "/ide/placelist",
            i, r;
        return n && (i = $("div.place").length, r = "?startRow=" + i, t += r), t
    }

    function r(r, u) {
        $.ajax({
            url: u,
            cache: !1,
            dataType: "html",
            success: function(u) {
                var f;
                f = n ? r.parent() : $("#assetList"), r.remove(), f.append($(u)), t(), $(u).animate({
                    opacity: 1
                }, "fast"), n || ($(".place").unbind("click"), $(".place").click(i)), $(".place a").removeAttr("href")
            }
        })
    }
    var t = function() {
            $(".item-name-container").each(function(n, t) {
                t = $(t), t.html(fitStringToWidthSafe(t.html(), 200, ""))
            })
        },
        n;
    if ($(window).resize(function() {
            var n = $(".main div.welcome-content-area:visible");
            $(window).height() < n.height() ? $(".navbar").height(n.height()) : $(".navbar").height($(window).height() - 124), n.height($(window).height() - 170)
        }), $(".navbar").height($(window).height() - 124), $("ul.filelist li a").each(function() {
            this.innerHTML = fitStringToWidthSafe($(this).text(), $(".navlist li p").width())
        }), $("#PublishedProjects").length > 0 ? $("#MyProjects").addClass("navselected") : $(".navlist li").first().addClass("navselected"), $("ul.filelist li a").click(function() {
            Roblox.Client.isIDE() ? window.external.OpenRecentFile($(this).data("js-file")) : Roblox.GenericModal.open(Roblox.IDEWelcome.Resources.openProject, "/images/Icons/img-alert.png", Roblox.IDEWelcome.Resources.openProjectText + " <a target='_blank' href='https://web.archive.org/web/20190716171201/http://wiki.roblox.com/index.php/Studio'>" + Roblox.IDEWelcome.Resources.robloxStudio + "</a>.")
        }), $("#studio-header-signup").click(function() {
            window.open(Roblox.Endpoints.getAbsoluteUrl("/Login/NewAge.aspx"))
        }), $("#HeaderHome").click(function() {
            window.location = Roblox.Endpoints.getAbsoluteUrl("/Home/Default.aspx")
        }), n = $("#GamesView").length > 0, $("#MyProjects").click(function() {
            $("#TemplatesView").hide(), $("#GamesView").hide(), $("#MyProjectsView").show(), $(".navlist li.navselected").removeClass("navselected"), $(this).addClass("navselected")
        }), $("#NewProject").click(function() {
            $("#TemplatesView").show(), $("#GamesView").hide(), $("#MyProjectsView").hide(), $(".navlist li.navselected").removeClass("navselected"), $(this).addClass("navselected")
        }), $("#GamesToggle").click(function() {
            $("#TemplatesView").hide(), $("#GamesView").show(), $("#MyProjectsView").hide(), $(".navlist li.navselected").removeClass("navselected"), $(this).addClass("navselected")
        }), $(".item-box-image").hover(function() {
            $(this).attr("src", $(this).data("animated-src"))
        }, function() {
            $(this).attr("src", $(this).data("static-src"))
        }), n) {
        $("#assetList").on("click", ".place", i);
        $("#universeList").on("click", ".place", function() {
            window.external.OpenUniverse($(this).data("universeid"))
        });
        $("#groupUniverseList").on("click", ".place", function() {
            window.external.OpenUniverse($(this).data("universeid"))
        });
        $("#GamesView .group-select").on("change", "select", function() {
            var i = $(this).find(":selected").val(),
                n = $("#groupUniverseList");
            n.find(".loading").length == 0 && n.append("<div class='loading'></div>"), $.ajax({
                url: "/ide/gamelist/" + i,
                cache: !1,
                dataType: "html",
                success: function(i) {
                    n.find(".gameplace").remove(), n.find(".loading").remove(), n.append($(i)), t(), $(i).animate({
                        opacity: 1
                    }, "fast");
                    var r = $(i).find("a[data-retry-url]");
                    r.length > 0 && r.loadRobloxThumbnails()
                }
            })
        })
    } else $(".place").click(i);
    $(".place a").removeAttr("href"), $("ul.navlist li").last().addClass("lastnav"), $("#StudioRecentFiles").length == 0 && $("ul.navlist").css("border-bottom", "none");
    $("#assetList").on("click", "#load-more-assets", function() {
        var n = $(this).parent(),
            t = u(!0);
        r(n, t)
    });
    if (n) {
        $("#universeList").on("click", ".load-more-universes", function() {
            var n = $(this).parent(),
                t = "/ide/gamelist?page=" + $(this).data("next-page");
            r(n, t)
        });
        $("#groupUniverseList").on("click", ".load-more-universes", function() {
            var n = $("#GamesView .group-select select").find(":selected").val(),
                t = $(this).parent(),
                i = "/ide/gamelist/" + n + "?page=" + $(this).data("next-page");
            r(t, i)
        })
    }
    $("#MyProjectsView .group-select").on("change", "select", function() {
        var i = $(this).find(":selected").val(),
            n = $("#groupAssetList .content");
        n.append("<div class='loading'></div>"), $.ajax({
            url: "/ide/placelist/" + i,
            cache: !1,
            dataType: "html",
            success: function(i) {
                n.find(".loading").remove(), n.find(".gameplace").remove(), n.append($(i)), t(), $(i).animate({
                    opacity: 1
                }, "fast");
                var r = $(i).find("a[data-retry-url]");
                r.length > 0 && r.loadRobloxThumbnails(), $(".place a").removeAttr("href")
            }
        })
    });
    $("#groupAssetList").on("click", ".place.asset", i);
    t(), $("[data-retry-url]").loadRobloxThumbnails()
});