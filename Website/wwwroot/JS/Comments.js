// Comments.js
var Roblox = Roblox || {};
Roblox.Comments = function() {
    function k() {
        Roblox.Captcha.render(o, Roblox.Captcha.types.postComment, Roblox.Comments.makeComment), Roblox.Captcha.execute()
    }

    function g() {
        var n = $("#" + o);
        return n[0].children.length > 0 ? (Roblox.Captcha.reset(Roblox.Captcha.types.postComment, Roblox.Comments.makeComment), !0) : !1
    }
    var b = !1,
        f = Roblox && Roblox.Endpoints ? Roblox.Endpoints.getAbsoluteUrl("/") : "/",
        ut = Roblox && Roblox.Endpoints ? Roblox.Endpoints.getAbsoluteUrl("/comments/post") : "/comments/post",
        rt = Roblox && Roblox.Endpoints ? Roblox.Endpoints.getAbsoluteUrl("/comments/delete") : "/comments/delete",
        n = $("#AjaxCommentsContainer"),
        h = $(".comment-form"),
        u = n.find(".rbx-comment-error"),
        e = n.find(".rbx-comment-count"),
        i = h.find(".rbx-post-comment"),
        t = h.find(".rbx-comment-input"),
        it = $(".loader-template").children(":first"),
        tt = function() {
            // Re-select elements now that the DOM (including comments markup) is fully loaded.
            n = $("#AjaxCommentsContainer");
            h = n.find(".comment-form");
            u = n.find(".rbx-comment-error");
            e = n.find(".rbx-comment-count");
            i = h.find(".rbx-post-comment");
            t = h.find(".rbx-comment-input");
            it = $(".loader-template").children(":first");
            r(u, "", !0, !0), d(), c(), f = n.data("signin-url")
        },
        o = "send-comment-captcha",
        d = function() {
            i.click(function() {
                var i = t.val();
                if (i.trim() != "") {
                    if (n.attr("data-is-user-authenticated").toLowerCase() == "false") {
                        Roblox.Dialog.open({
                            titleText: "Login to Comment",
                            bodyContent: "<div>You must login to comment.</div> <div>Please <a href='" + f + "'>login or register</a> to continue.</div>",
                            onAccept: function() {
                                window.location.href = f
                            },
                            acceptText: "Login",
                            declineText: "Cancel",
                            allowHtmlContentInBody: !0
                        });
                        return
                    }
                    if (Roblox.Comments.FilterIsEnabled && l(i)) {
                        Roblox.Dialog.open({
                            titleText: Roblox.Comments.Resources.linksNotAllowedTitle,
                            bodyContent: Roblox.Comments.Resources.linksNotAllowedMessage,
                            acceptText: "Ok",
                            declineText: "Cancel",
                            allowHtmlContentInBody: !0
                        });
                        return
                    }
                    Roblox.Comments.makeComment()
                }
            });
            t.on({
                focus: function() {
                    $(this).removeClass("blur")
                },
                blur: function() {
                    $(this).addClass("blur")
                }
            });
            t.on("input propertychange", function() {
                var f = t[0],
                    i = Roblox.Comments.Limits,
                    e, n;
                for (r(u, "", !0, !0), e = !1, n = 0; n < i.length; ++n) v(f.value, i[n].limit, i[n].character) || (f.value = nt(f.value, i[n].limit, i[n].character), r(u, i[n].message, !1), e = !0), e || typeof i[n].character != "undefined" || a(f.value, i[n].limit)
            });
            $(".comments").on("click", ".rbx-comment-remove", function() {
                elem = $(this), Roblox.Comments.deleteComment(elem.attr("data-comment-id")), elem.parents(".comment-item").remove()
            });
            $(".rbx-comments-see-more").on("click", function() {
                var n = $(".comments .comment-item").length;
                Roblox.Comments.getComments(n)
            });
            $(window).on("scroll resize", function() {
                Roblox.Comments.loadCommentsIfReady()
            })
        },
        c = function() {
            Roblox.Comments && !Roblox.Comments.gotFirstComments && Roblox.Comments.isCommentBlockInViewport() && (Roblox.Comments.getComments(0), Roblox.Comments.gotFirstComments = !0)
        },
        ft = function() {
            // Re-select the comments container in case it was not present when this
            // script first ran, and guard against missing/hidden elements.
            var e = $("#AjaxCommentsContainer");
            if (!e.length || !e.offset()) return !1;
            var r = $(window),
                t = r.scrollTop(),
                u = t + r.height(),
                i = e.offset().top,
                f = i + e.height();
            return i >= t && i <= u || f >= t && f <= u
        },
        s = function(n, t) {
            var i = $(".comments-item-template").clone(),
                r;
            return t && (i.find(".comment-controls .rbx-comment-report-link").remove(), i.find(".comment-controls").append("<a class='rbx-comment-remove' data-comment-id='" + n.Id + "' title='Remove Comment'><span class='icon-remove'></span></a>")), r = Roblox && Roblox.Endpoints ? Roblox.Endpoints.getAbsoluteUrl("/users/" + n.AuthorId + "/profile") : "/users/" + n.AuthorId + "/profile", i.find(".comment-item").attr("data-comment-id", n.Id), i.find(".comment-user .Avatar a").attr("title", n.AuthorName).attr(r), i.find(".comment-body .text-name").text(n.AuthorName).attr("href", r), i.find(".comment-user .Avatar").addClass("roblox-avatar-image").attr("data-user-id", n.AuthorId).html(""), i.find(".comment-body span.xsmall").text(n.PostedDate), i.find(".comment-content").html(n.Text.replace(/\$/g, "&#36;")), i.find(), i = i.html(), i = i.replace("%CommentID", n.Id), i = i.replace("%PageURL", encodeURIComponent(window.location.href))
        },
        w = function() {
            i.attr("disabled", !0), $.post(ut, {
                assetId: n.attr("data-asset-id"),
                text: t.val()
            }, function(n) {
                var f, i;
                if (n && typeof n.Id != "undefined" && !n.error) {
                    f = {
                        Id: n.Id,
                        PostedDate: n.PostedDate,
                        Text: n.Text,
                        AuthorName: n.AuthorName,
                        AuthorId: n.AuthorId,
                        AuthorThumbnail: {
                            Url: ""
                        }
                    };
                    t.val("");
                    e.html("");
                    r(u, "", !0, !0);
                    i = $(".comments");
                    i.find(".empty").remove();
                    i.prepend(s(f, !1));
                    Roblox.require("Widgets.AvatarImage", function(n) {
                        n.populate()
                    });
                } else if (n && n.error) {
                    p(n.error);
                }

                // Always reload the page on any HTTP 200 success from the POST.
                window.location.reload();

            }).always(function() {
                i.removeAttr("disabled")
            })
        },
        p = function(n) {
            if (!n) {
                r(u, "", !0, !0);
                return;
            }
            n == "01" ? Roblox.Dialog.open({
                titleText: Roblox.Comments.Resources.emailVerifiedABTitle,
                bodyContent: Roblox.Comments.Resources.emailVerifiedABMessage,
                onAccept: function() {
                    var n = Roblox && Roblox.Endpoints ? Roblox.Endpoints.getAbsoluteUrl("/my/account?confirmemail=1") : "/my/account?confirmemail=1";
                    window.location.href = n
                },
                acceptText: Roblox.Comments.Resources.accept,
                declineText: Roblox.Comments.Resources.decline,
                allowHtmlContentInBody: !0
            }) : n && n === "Captcha" ? g() || k() : r(u, n, !1, !0)
        },
        y = function(t) {
            var r, i;
            t = t == undefined ? 0 : t, r = n.attr("data-asset-id"), i = $(".comments"), i.find(".empty,.more").remove(), i.append(it.clone());
            var u = Math.floor(Math.random() * 9001),
                f = 100,
                e = 100,
                o = "PNG";
            $.ajax({
                type: "GET",
                url: "/comments/get-json",
                data: {
                    assetId: r,
                    startindex: t,
                    thumbnailWidth: f,
                    thumbnailHeight: e,
                    thumbnailFormat: o,
                    cachebuster: u
                },
                contentType: "application/json; charset=utf-8",
                success: function(n) {
                    var r = n.Comments,
                        e = n.MaxRows,
                        u = $(".rbx-comments-see-more"),
                        f;
                    i.find(".loading-animated,.empty,.more").remove();
                    for (f in r) i.append(s(r[f], n.IsUserModerator));
                    !r || r.length < e ? u.addClass("hidden") : u.removeClass("hidden"), r && (r.length != 0 || t != 0) || i.append($('<div class="empty">' + Roblox.Comments.Resources.noCommentsFound + "</div>")), Roblox.require("Widgets.AvatarImage", function(n) {
                        n.populate()
                    })
                },
                error: function() {
                    $(".comments").find(".loading-animated,.empty,.more").remove().append($('<div class="empty">' + Roblox.Comments.Resources.sorrySomethingWentWrong + "</div>"))
                }
            })
        },
        a = function(n, t) {
            e.html(Math.max(0, Math.min(t, t - n.length)) + " " + Roblox.Comments.Resources.charactersRemaining)
        },
        l = function(n) {
            return new RegExp(Roblox.Comments.FilterRegex).test(n.replace(/(\r\n|\n|\r|<br\/>)/gm, "")) ? !0 : !1
        },
        r = function(n, t, r, u) {
            u = typeof u == "undefined" ? !1 : u, r = typeof r == "undefined" ? !0 : r, u && n.empty(), n.append(t + " "), r ? (n.hide(), i.removeAttr("disabled")) : (n.show(), e.html(""), i.attr("disabled", !0))
        },
        v = function(n, t, i) {
            if (i = typeof i == "undefined" ? "" : i, i !== "") {
                var r = n.split(i).length - 1;
                return r <= t
            }
            return n.length <= t
        },
        nt = function(n, t, i) {
            if (i = typeof i == "undefined" ? "" : i, i !== "") {
                var f = n.split(i),
                    e = f.length,
                    u = "",
                    r;
                if (e > t)
                    for (r = 0; r < e; ++r) u += f[r], r < t && (u += i);
                n = u
            } else n = n.substr(0, t);
            return n
        },
        et = function(n) {
            $.post(rt, {
                commentId: n
            }, function() {
                $(".comments [data-comment-id=" + n + "]").parents(".comment-item")
            })
        };
    return {
        initialize: tt,
        getComments: y,
        makeComment: w,
        deleteComment: et,
        gotFirstComments: b,
        isCommentBlockInViewport: ft,
        loadCommentsIfReady: c
    }
}();