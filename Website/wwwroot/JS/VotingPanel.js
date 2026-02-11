// VotingPanel.js
var Roblox = Roblox || {};
Roblox.Voting = function() {
    var f = function(n, t) {
            var i = "/games/votingservice/" + t;
            $.ajax({
                url: i,
                success: function(t) {
                    n.replaceWith(t)
                }
            })
        },
        e = function() {
            $(".users-vote .upvote").unbind().click(function() {
                i($(this), !0)
            }), $(".users-vote .downvote").unbind().click(function() {
                i($(this), !1)
            });
            var t = parseInt($(".voting-panel").data("total-up-votes")),
                r = parseInt($(".voting-panel").data("total-down-votes"));
            n(t, r)
        },
        i = function(n, i) {
            var e = $(".voting-panel").data("user-authenticated");
            if (!e) {
                u("GuestUser");
                return
            }
            var o = $(".voting-panel").data("target-id"),
                r = "/voting/vote?assetId=" + o + "&vote=",
                f = $(".voting-panel").data("vote-url");
            f && (r = f), n.hasClass("selected") || n.find("i").hasClass("selected") || n.find(".icon-like, .icon-dislike").hasClass("selected") ? t(r, null) : t(r, i)
        },
        t = function(n, t) {
            $(".voting-panel .loading").show(), $.ajax({
                type: "POST",
                url: n + t,
                success: o,
                error: s
            })
        },
        o = function(t) {
            var o = $(".icon-like").length;
            if ($(".voting-panel .loading").hide(), t.Success) {
                r(t.Model.UpVotes, t.Model.DownVotes);
                var i = $(".voting-panel .upvote"),
                    f = $(".voting-panel .downvote"),
                    e = $(".users-vote");
                o && (i = $(".voting-panel .upvote .icon-like"), f = $(".voting-panel .downvote .icon-dislike")), t.Model.UserVote !== null ? e.hasClass("has-voted") || e.addClass("has-voted") : e.removeClass("has-voted"), i.hasClass("selected") && i.removeClass("selected"), f.hasClass("selected") && f.removeClass("selected"), t.Model.UserVote !== null && (t.Model.UserVote ? i.addClass("selected") : f.addClass("selected")), n(t.Model.UpVotes, t.Model.DownVotes)
            } else u(t.ModalType)
        },
        s = function() {
            $(".voting-panel .loading").hide()
        },
        n = function(n, t, i) {
            var e = i || $("#voting-section"),
                r, u, f;
            isNaN(n) || isNaN(t) || (r = n === 0 ? 0 : t === 0 ? 100 : Math.floor(n / (n + t) * 100), r > 100 && (r = 100), u = e.find(".vote-container"), f = u.find(".vote-background"), u.find(".vote-percentage").css("width", r + "%"), t > 0 ? f.addClass("has-votes") : f.removeClass("has-votes"))
        },
        r = function(t, i) {
            t = Roblox.NumberFormatting.abbreviatedFormat(t), i = Roblox.NumberFormatting.abbreviatedFormat(i), $(".voting-panel .total-upvotes-text").text(t), $(".voting-panel .total-downvotes-text").text(i), $(".voting-panel #vote-up-text").text(t), $(".voting-panel #vote-down-text").text(i), n(t, i)
        },
        h = function(n) {
            var t = {
                EmailIsVerified: {
                    titleText: Roblox.Voting.Resources.emailVerifiedTitle,
                    bodyContent: Roblox.Voting.Resources.emailVerifiedMessage,
                    onAccept: function() {
                        window.location.href = Roblox && Roblox.Endpoints ? Roblox.Endpoints.getAbsoluteUrl("/my/account?confirmemail=1") : "/my/account?confirmemail=1"
                    },
                    acceptColor: Roblox.Dialog.green,
                    acceptText: Roblox.Voting.Resources.accept,
                    declineText: Roblox.Voting.Resources.decline,
                    allowHtmlContentInBody: !0
                },
                PlayGame: {
                    titleText: Roblox.Voting.Resources.playGameTitle,
                    bodyContent: Roblox.Voting.Resources.playGameMessage,
                    showAccept: !1,
                    declineText: Roblox.Voting.Resources.ok
                },
                UseModel: {
                    titleText: Roblox.Voting.Resources.useModelTitle,
                    bodyContent: Roblox.Voting.Resources.useModelMessage,
                    showAccept: !1,
                    declineText: Roblox.Voting.Resources.ok
                },
                InstallPlugin: {
                    titleText: Roblox.Voting.Resources.installPluginTitle,
                    bodyContent: Roblox.Voting.Resources.installPluginMessage,
                    showAccept: !1,
                    declineText: Roblox.Voting.Resources.ok
                },
                BuyGamePass: {
                    titleText: Roblox.Voting.Resources.buyGamePassTitle,
                    bodyContent: Roblox.Voting.Resources.buyGamePassMessage,
                    showAccept: !1,
                    declineText: Roblox.Voting.Resources.ok
                },
                FloodCheckThresholdMet: {
                    titleText: Roblox.Voting.Resources.floodCheckThresholdMetTitle,
                    bodyContent: Roblox.Voting.Resources.floodCheckThresholdMetMessage,
                    showAccept: !1,
                    declineText: Roblox.Voting.Resources.ok
                },
                GuestUser: {
                    titleText: Roblox.Voting.Resources.guestUserTitle,
                    bodyContent: Roblox.Voting.Resources.guestUserMessage,
                    onAccept: function() {
                        window.location.href = Roblox && Roblox.Endpoints ? Roblox.Endpoints.getAbsoluteUrl(Roblox.Voting.Resources.returnUrl) : Roblox.Voting.Resources.returnUrl
                    },
                    acceptColor: Roblox.Dialog.green,
                    acceptText: Roblox.Voting.Resources.login,
                    declineText: Roblox.Voting.Resources.decline,
                    allowHtmlContentInBody: !0
                },
                Error: {
                    titleText: Roblox.Voting.Resources.unknownProblemTitle,
                    bodyContent: Roblox.Voting.Resources.unknownProblemMessage,
                    showAccept: !1,
                    declineText: Roblox.Voting.Resources.ok
                },
                AssetNotVoteable: {
                    titleText: Roblox.Voting.Resources.assetNotVoteableTitle,
                    bodyContent: Roblox.Voting.Resources.assetNotVoteableMessage,
                    showAccept: !1,
                    declineText: Roblox.Voting.Resources.ok
                }
            };
            return t[n] || t.Error
        },
        u = function(n) {
            n && Roblox.Dialog.open(h(n))
        };
    return {
        Vote: t,
        Initialize: e,
        SetVotes: r,
        UpdateVoteBar: n,
        LoadVotingService: f
    }
}();