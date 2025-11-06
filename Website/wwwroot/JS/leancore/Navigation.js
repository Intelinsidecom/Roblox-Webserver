// leancore/Navigation.js
$(function() {
    "use strict";

    function k() {
        return $("#nav-friends") && $("#nav-friends").data() ? $("#nav-friends").data().count : 0
    }

    function p() {
        return $("#nav-message") && $("#nav-message").data() ? $("#nav-message").data().count : 0
    }

    function h(n, t) {
        if (n === 0) return "";
        if (n < t) return n.toString();
        switch (t) {
            case c:
                return "1k+";
            case o:
                return "99+";
            default:
                return ""
        }
    }

    function s() {
        var r = k(),
            u = p(),
            t = u + r,
            n = $(".rbx-nav-collapse .notification-red"),
            i;
        if (t == 0 && !n.hasClass("hide")) {
            n.addClass("hide");
            return
        }
        i = h(t, o), n.html(i), t > 0 && n.removeClass("hide"), n.attr("title", t)
    }

    function l() {
        var n = "/navigation/getCount";
        $.ajax({
            url: n,
            success: function(n) {
                var i = $("#nav-friends"),
                    t = i.find(".notification-blue");
                i.attr("href", n.FriendNavigationUrl), i.data("count", n.TotalFriendRequests), t.html(n.DisplayCountFriendRequests), t.attr("title", n.TotalFriendRequests), n.TotalFriendRequests > 0 ? t.removeClass("hide") : t.addClass("hide"), s(n.TotalFriendRequests)
            }
        })
    }

    function v() {
        if (Roblox && Roblox.RealTime) {
            var n = Roblox.RealTime.Factory.GetClient();
            n.Subscribe("FriendshipNotifications", function(n) {
                switch (n.Type) {
                    case "FriendshipCreated":
                    case "FriendshipDestroyed":
                    case "FriendshipDeclined":
                    case "FriendshipRequested":
                        l()
                }
            })
        }
    }

    function b(n) {
        var i = $('[data-behavior="univeral-search"] .rbx-navbar-search-option'),
            t = -1;
        $.each(i, function(n, i) {
            $(i).hasClass("selected") && ($(i).removeClass("selected"), t = n)
        }), t += n.which === 38 ? i.length - 1 : 1, t %= i.length, $(i[t]).addClass("selected")
    }
    var i, e;
    ($(".rbx-left-col").length == 0 || $(".rbx-left-col").width() == 0) && ($("#header-login").length != 0 || $("#GamesListsContainer").length != 0) && ($("#navContent").css({
        "margin-left": "0px",
        width: "100%"
    }), $("#navContent").addClass("nav-no-left")), $(window).resize(function() {
        ($(".rbx-left-col").length == 0 || $(".rbx-left-col").width() == 0) && ($("#header-login").length != 0 || $("#GamesListsContainer").length != 0) ? ($("#navContent").css({
            "margin-left": "0px",
            width: "100%"
        }), $("#navContent").addClass("nav-no-left")) : $("#navContent").css({
            "margin-left": "",
            width: ""
        })
    });
    var o = 99,
        c = 1e3,
        r = 1;
    $(document).on("Roblox.Messages.CountChanged", function() {
        var n = Roblox.websiteLinks.GetMyUnreadMessagesCountLink;
        $.ajax({
            url: n,
            success: function(n) {
                var r = $("#nav-message"),
                    t = $("#nav-message span.notification-blue"),
                    i;
                r.data("count", n.count), i = h(n.count, c), t.html(i), t.attr("title", n.count), n.count > 0 ? t.removeClass("hide") : t.addClass("hide"), s()
            }
        })
    });
    i = $("#header"), i && i.data("isfriendshiprealtimeupdateenabled") && v();
    $(document).on("Roblox.Friends.CountChanged", function() {
        l()
    });
    $('[data-behavior="nav-notification"]').click(function() {
        $('[data-behavior="left-col"]').toggleClass("nav-show", 100)
    });
    var t = $("#navbar-universal-search"),
        n = $("#navbar-universal-search #navbar-search-input"),
        u = $("#navbar-universal-search .rbx-navbar-search-option"),
        w = $("#navbar-universal-search #navbar-search-btn");
    n.on("keydown", function(n) {
        var t = $(this).val();
        (n.which === 9 || n.which === 38 || n.which === 40) && t.length > 0 && (n.stopPropagation(), n.preventDefault(), b(n))
    });
    n.on("keyup", function(n) {
        var i = $(this).val(),
            u, f;
        n.which === 13 ? (n.stopPropagation(), n.preventDefault(), u = t.find(".rbx-navbar-search-option.selected"), f = u.data("searchurl"), i.length >= r && (window.location = f + encodeURIComponent(i))) : i.length > 0 ? (t.toggleClass("rbx-navbar-search-open", !0), $('[data-toggle="dropdown-menu"] .rbx-navbar-search-string').text('"' + i + '"')) : t.toggleClass("rbx-navbar-search-open", !1)
    });
    w.click(function(t) {
        t.stopPropagation(), t.preventDefault();
        var i = n.val(),
            u = $("#navbar-universal-search .rbx-navbar-search-option.selected"),
            f = u.data("searchurl");
        i.length >= r && (window.location = f + encodeURIComponent(i))
    });
    u.on("click touchstart", function(t) {
        var i, u;
        t.stopPropagation(), i = n.val(), i.length >= r && (u = $(this).data("searchurl"), window.location = u + encodeURIComponent(i))
    });
    u.on("mouseover", function() {
        u.removeClass("selected"), $(this).addClass("selected")
    });
    n.on("focus", function() {
        var i = n.val();
        i.length > 0 && t.addClass("rbx-navbar-search-open")
    });
    $('[data-toggle="toggle-search"]').on("click touchstart", function(n) {
        return n.stopPropagation(), $('[data-behavior="univeral-search"]').toggleClass("show"), !1
    });
    $(".rbx-navbar-right").on("click touchstart", '[data-behavior="logout"]', function(n) {
        var i, t;
        n.stopPropagation(), n.preventDefault(), i = $(this), typeof angular == "undefined" || angular.isUndefined(angular.element("#chat-container").scope()) || (t = angular.element("#chat-container").scope(), t.$digest(t.$broadcast("Roblox.Chat.destroyChatCookie"))), $.post(i.attr("data-bind"), {
            redirectTohome: !1
        }, function() {
            var t = Roblox && Roblox.Endpoints ? Roblox.Endpoints.getAbsoluteUrl("/") : "/";
            window.location.href = t
        })
    });
    $("#nav-robux-icon").on("show.bs.popover", function() {
        $("body").scrollLeft(0)
    });
    e = function(n) {
        var t, i;
        if (n.indexOf("resize") != -1) {
            t = n.split(",");
            var h = t[1];
            $("#iframe-login").css({ height: h });
            $("#iFrameLogin").css({ height: h });
        }
        n.indexOf("fbRegister") != -1 && (t = n.split("^"), i = "&fbname=" + encodeURIComponent(t[1]) + "&fbem=" + encodeURIComponent(t[2]) + "&fbdt=" + encodeURIComponent(t[3]), window.location.href = "../Login/Default.aspx?iFrameFacebookSync=true" + i)
    }, $.receiveMessage(function(n) {
        e(n.data)
    });
    $("body").on("click touchstart", function(n) {
        $('[data-behavior="univeral-search"]').each(function() {
            $(this).is(n.target) || $(this).has(n.target).length !== 0 || $(this).removeClass("rbx-navbar-search-open"), $(this).has(n.target).length === 0 && $('[data-toggle="toggle-search"]').has(n.target).length === 0 && $('[data-behavior="univeral-search"]').css("display") === "block" && $('[data-behavior="univeral-search"]').removeClass("show")
        }), $(n.target).closest("#iFrameLogin").length || $(n.target).is("#iFrameLogin") || $(n.target).closest("#head-login").length || $(n.target).is("#head-login") || $("#iFrameLogin").hasClass("show") && $("#iFrameLogin").removeClass("show")
    });
    var f = function() {
            $("#header-login").click(function() {
                var t = {
                    onSignupSuccess: function() {
                        window.location.reload()
                    },
                    onLoginSuccess: function() {
                        window.location.reload()
                    },
                    sectionType: Roblox.SignupOrLogin.SectionType.login
                };
                if (window.Roblox && Roblox.SignupOrLoginModal && typeof Roblox.SignupOrLoginModal.show === "function") {
                    Roblox.SignupOrLoginModal.show(t)
                } else {
                    // Fallback to legacy iframe login toggle: directly toggle #iFrameLogin like y()
                    $("#iFrameLogin").toggleClass("show");
                    if ($("#iFrameLogin").hasClass("show")) {
                        var leftOffset = $("#header-login").offset().left - $("#iFrameLogin").offset().left - 250;
                        if (leftOffset > 0) {
                            $("#iFrameLogin").css("left", leftOffset)
                        }
                    }
                }
            }), $("#header-signup").click(function() {
                var t = {
                    onSignupSuccess: function() {
                        window.location.reload()
                    },
                    onLoginSuccess: function() {
                        window.location.reload()
                    },
                    sectionType: Roblox.SignupOrLogin.SectionType.signup
                };
                Roblox.SignupOrLoginModal.show(t)
            })
        },
        y = function() {
            function adjustIframeHeight() {
                try {
                    var ifr = document.getElementById('iframe-login');
                    if (ifr && ifr.contentWindow && ifr.contentWindow.document) {
                        var doc = ifr.contentWindow.document;
                        var de = doc.documentElement;
                        var h = Math.max(
                            doc.body ? doc.body.scrollHeight : 0,
                            doc.body ? doc.body.offsetHeight : 0,
                            de ? de.clientHeight : 0,
                            de ? de.scrollHeight : 0,
                            de ? de.offsetHeight : 0
                        );
                        if (h && h > 0) {
                            $("#iframe-login").css({ height: h + 'px' });
                            $("#iFrameLogin").css({ height: h + 'px' });
                        }
                    }
                } catch (e) { /* ignore */ }
            }
            $("#head-login").click(function() {
                if ($("#iFrameLogin").toggleClass("show"), $("#iFrameLogin").hasClass("show")) {
                    var t = $("#head-login").offset().left - $("#iFrameLogin").offset().left - 250;
                    t > 0 && $("#iFrameLogin").css("left", t);
                    // same-origin sizing fallback
                    adjustIframeHeight();
                    setTimeout(adjustIframeHeight, 50);
                    setTimeout(adjustIframeHeight, 200);
                    var ifr = document.getElementById('iframe-login');
                    if (ifr) {
                        $(ifr).on('load', function(){ adjustIframeHeight(); });
                    }
                }
            })
        },
        a = $("#signupOrLoginIframe");
    a.length ? a.load(function() {
        f()
    }) : f(), y()
});