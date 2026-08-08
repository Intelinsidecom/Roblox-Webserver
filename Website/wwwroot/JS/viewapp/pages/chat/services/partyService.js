// ~/viewapp/pages/chat/services/partyService.js
"use strict";
chat.factory("partyService", ["chatHybridService", "$http", "$q", "$document", "$log", function(n, t, i, r, u) {
    var e = function(n, r, f) {
            var e = i.defer();
            return t({
                method: "GET",
                url: n.url,
                params: r,
                withCredentials: f,
                retryable: n.retryable
            }).success(function(n) {
                e.resolve(n)
            }).error(function(t) {
                u.debug("Error: unable to send " + n.url + " request."), e.reject(t)
            }), e.promise
        },
        f = function(n, r, f) {
            var e = i.defer();
            return t({
                method: "POST",
                url: n.url,
                data: r,
                withCredentials: f,
                retryable: n.retryable
            }).success(function(n) {
                e.resolve(n)
            }).error(function(t) {
                u.debug("Error: unable to send " + n.url + " request."), e.reject(t)
            }), e.promise
        };
    return {
        apiSets: {
            getPlaceUrl: {
                url: "/thumbnail/place",
                retryable: !0
            }
        },
        setParams: function(n) {
            this.apiSets.partyCreateApi = {
                url: n + "/v1.0/party/create",
                retryable: !1
            }, this.apiSets.partyInviteApi = {
                url: n + "/v1.0/party/invite",
                retryable: !0
            }, this.apiSets.partyLeaveApi = {
                url: n + "/v1.0/party/leave",
                retryable: !0
            }, this.apiSets.partyJoinApi = {
                url: n + "/v1.0/party/join",
                retryable: !0
            }, this.apiSets.getInvitedPartiesApi = {
                url: n + "/v1.0/party/get-invites",
                retryable: !0
            }, this.apiSets.getCurrentPartyApi = {
                url: n + "/v1.0/party/get-current",
                retryable: !0
            }, this.apiSets.removeFromPartyApi = {
                url: n + "/v1.0/party/remove-from-party",
                retryable: !0
            }, this.apiSets.getPartiesForConversationsApi = {
                url: n + "/v1.0/party/get-parties-for-conversations",
                retryable: !0
            }
        },
        partyCreate: function(n, t) {
            var i = {
                invitedUserIds: t,
                conversationId: n
            };
            return f(this.apiSets.partyCreateApi, i, !0)
        },
        partyInvite: function(n, t) {
            var i = {
                invitedUserId: t,
                partyId: n
            };
            return f(this.apiSets.partyInviteApi, i, !0)
        },
        partyLeave: function(n) {
            var t = {
                partyId: n
            };
            return f(this.apiSets.partyLeaveApi, t, !0)
        },
        partyJoin: function(n) {
            var t = {
                partyId: n
            };
            return f(this.apiSets.partyJoinApi, t, !0)
        },
        getInvitedParties: function(n, t) {
            var i = {
                pageNumber: n,
                pageSize: t
            };
            return e(this.apiSets.getInvitedPartiesApi, i, !0)
        },
        getPartiesForConversations: function(n) {
            var t = {
                conversationIds: n
            };
            return e(this.apiSets.getPartiesForConversationsApi, t, !0)
        },
        getCurrentParty: function() {
            return e(this.apiSets.getCurrentPartyApi, null, !0)
        },
        removeFromParty: function(n, t) {
            var i = {
                partyId: n,
                userId: t
            };
            return f(this.apiSets.removeFromPartyApi, i, !0)
        },
        getPlace: function(n) {
            var t = {
                placeId: n
            };
            return e(this.apiSets.getPlaceUrl, t, !0)
        },
        joinGame: function(n) {
            var i = n.party,
                u, f;
            i.GameId && i.GamePlaceId && (r.triggerHandler("Roblox.Chat.PartyInGame", {
                placeId: i.GamePlaceId
            }), u = "PlaceLauncherStatusPanel", f = angular.element(document.querySelector("#" + u)), this.getPlace(i.GamePlaceId).then(function(t) {
                n.placeThumbnail = t
            }), Roblox.GamePlayEvents && Roblox.GamePlayEvents.SendGamePlayIntent("Party", i.GamePlaceId), f.data("is-game-launch-interface-enabled") === "True" ? Roblox.GameLauncher.joinGameWithParty(i.GamePlaceId, i.Id, i.GameId) : Roblox.Client.WaitForRoblox(function() {
                RobloxLaunch.RequestPlayWithParty(u, i.GamePlaceId, i.Id, i.GameId)
            }))
        }
    }
}]);