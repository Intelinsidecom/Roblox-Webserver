// VotingPanel.js
var Roblox = Roblox || {};
Roblox.Voting = function() {
    var LoadVotingService = function(container, placeId) {
            var url = "/games/votingservice/" + placeId;
            $.ajax({
                url: url,
                success: function(data) {
                    container.replaceWith(data);
                }
            });
        },
        Initialize = function() {
            $(".users-vote .upvote").unbind().click(function() {
                Vote($(this), true)
            }), $(".users-vote .downvote").unbind().click(function() {
                Vote($(this), false)
            });
            var upVotes = parseInt($(".voting-panel").data("total-up-votes")),
                downVotes = parseInt($(".voting-panel").data("total-down-votes"));
            UpdateVoteBar(upVotes, downVotes)
        },
        Vote = function(element, voteType) {
            var isAuthenticated = $(".voting-panel").data("user-authenticated");
            if (!isAuthenticated) {
                ShowModal("GuestUser");
                return
            }
            var targetId = $(".voting-panel").data("target-id"),
                url = "/voting/vote?assetId=" + targetId + "&vote=",
                voteUrl = $(".voting-panel").data("vote-url");
            voteUrl && (url = voteUrl), element.hasClass("selected") || element.find("i").hasClass("selected") || element.find(".icon-like, .icon-dislike").hasClass("selected") ? SubmitVote(url, null) : SubmitVote(url, voteType)
        },
        SubmitVote = function(url, voteType) {
            $(".voting-panel .loading").show(), $.ajax({
                type: "POST",
                url: url + voteType,
                success: OnVoteSuccess,
                error: OnVoteError
            })
        },
        OnVoteSuccess = function(data) {
            $(".voting-panel .loading").hide();
            
            if (data.Success || data.success) {
                var targetId = $(".voting-panel").data("target-id");
                var url = "/games/votingservice/" + targetId;
                
                $.ajax({
                    url: url,
                    success: function(htmlData) {
                        $("#voting-section").replaceWith(htmlData);
                        Roblox.Voting.Initialize();
                    },
                    error: function() {
                        console.log("Failed to refresh voting panel");
                    }
                });
            } else {
                console.log("Vote failed:", data);
                ShowModal(data.ModalType || data.modalType);
            }
        },
        OnVoteError = function() {
            $(".voting-panel .loading").hide()
        },
        UpdateVoteBar = function(upVotes, downVotes, votingSection) {
            var element = votingSection || $("#voting-section"),
                percentage, voteContainer, voteBackground;
            isNaN(upVotes) || isNaN(downVotes) || (percentage = upVotes === 0 ? 0 : downVotes === 0 ? 100 : Math.floor(upVotes / (upVotes + downVotes) * 100), percentage > 100 && (percentage = 100), voteContainer = element.find(".vote-container"), voteBackground = voteContainer.find(".vote-background"), voteContainer.find(".vote-percentage").css("width", percentage + "%"), downVotes > 0 ? voteBackground.addClass("has-votes") : voteBackground.removeClass("has-votes"))
        },
        SetVotes = function(upVotes, downVotes) {
            upVotes = Roblox.NumberFormatting.abbreviatedFormat(upVotes), downVotes = Roblox.NumberFormatting.abbreviatedFormat(downVotes), $(".voting-panel .total-upvotes-text").text(upVotes), $(".voting-panel .total-downvotes-text").text(downVotes), $(".voting-panel #vote-up-text").text(upVotes), $(".voting-panel #vote-down-text").text(downVotes), UpdateVoteBar(upVotes, downVotes)
        },
        GetModalContent = function(modalType) {
            var modalTypes = {
                EmailIsVerified: {
                    titleText: Roblox.Voting.Resources.emailVerifiedTitle,
                    bodyContent: Roblox.Voting.Resources.emailVerifiedMessage,
                    onAccept: function() {
                        window.location.href = Roblox && Roblox.Endpoints ? Roblox.Endpoints.getAbsoluteUrl("/my/account?confirmemail=1") : "/my/account?confirmemail=1"
                    },
                    acceptColor: Roblox.Dialog.green,
                    acceptText: Roblox.Voting.Resources.accept,
                    declineText: Roblox.Voting.Resources.decline,
                    allowHtmlContentInBody: true
                },
                PlayGame: {
                    titleText: Roblox.Voting.Resources.playGameTitle,
                    bodyContent: Roblox.Voting.Resources.playGameMessage,
                    showAccept: false,
                    declineText: Roblox.Voting.Resources.ok
                },
                UseModel: {
                    titleText: Roblox.Voting.Resources.useModelTitle,
                    bodyContent: Roblox.Voting.Resources.useModelMessage,
                    showAccept: false,
                    declineText: Roblox.Voting.Resources.ok
                },
                InstallPlugin: {
                    titleText: Roblox.Voting.Resources.installPluginTitle,
                    bodyContent: Roblox.Voting.Resources.installPluginMessage,
                    showAccept: false,
                    declineText: Roblox.Voting.Resources.ok
                },
                BuyGamePass: {
                    titleText: Roblox.Voting.Resources.buyGamePassTitle,
                    bodyContent: Roblox.Voting.Resources.buyGamePassMessage,
                    showAccept: false,
                    declineText: Roblox.Voting.Resources.ok
                },
                FloodCheckThresholdMet: {
                    titleText: Roblox.Voting.Resources.floodCheckThresholdMetTitle,
                    bodyContent: Roblox.Voting.Resources.floodCheckThresholdMetMessage,
                    showAccept: false,
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
                    allowHtmlContentInBody: true
                },
                Error: {
                    titleText: Roblox.Voting.Resources.unknownProblemTitle,
                    bodyContent: Roblox.Voting.Resources.unknownProblemMessage,
                    showAccept: false,
                    declineText: Roblox.Voting.Resources.ok
                },
                AssetNotVoteable: {
                    titleText: Roblox.Voting.Resources.assetNotVoteableTitle,
                    bodyContent: Roblox.Voting.Resources.assetNotVoteableMessage,
                    showAccept: false,
                    declineText: Roblox.Voting.Resources.ok
                }
            };
            return modalTypes[modalType] || modalTypes.Error
        },
        ShowModal = function(modalType) {
            modalType && Roblox.Dialog.open(GetModalContent(modalType))
        };
    return {
        LoadVotingService: LoadVotingService,
        Initialize: Initialize,
        Vote: SubmitVote,
        SetVotes: SetVotes,
        UpdateVoteBar: UpdateVoteBar
    }
}();