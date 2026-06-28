// ~/viewapp/studio/toolbox/toolboxDirective.js
"use strict";
clientToolbox.directive("robloxToolboxHeader", ["ClientToolboxService", "GAEService", function(n, t) {
    var i = function(t) {
        var r = n.allSets.FreeModels.category,
            f = null,
            u = t.split("?"),
            i;
        u.length > 1 && u[1] && (i = u[1].split("="), i[0] == n.SEARCHFIELD ? (r = n.allSets.FreeModels.category, f = decodeURIComponent(i[1])) : i[0] == n.SELECTFIELD && i[1] == n.SELECTEDVALUE && (r = n.allSets.MyModels.category, n.initializeSet = n.allSets.MyModels.category)), n.disableUIUpdate = !0, n.updateAssets(r, null, f)
    };
    return {
        restrict: "A",
        scope: !1,
        templateUrl: Roblox.ClientToolboxLinks.HeaderTemplateLink,
        replace: !0,
        controller: ["$location", function(t) {
            n.getAllSets().then(function() {
                i(t.absUrl())
            })
        }],
        link: function(i) {
            i.$watch(function() {
                return n.allSets
            }, function(n, t) {
                n != t && (i.options = n)
            }, !0), i.$watch("option", function(t, r) {
                n.searchText = "", t && t != r && !n.disableUIUpdate && (t.hasChildren && t.children[0] ? i.childOption = t.children[0] : (i.childOption && (i.childOption = null), n.updateAssets(t.category, t.id)))
            }), i.$watch("childOption", function(t, i) {
                t && t != i && !n.disableUIUpdate && n.updateAssets(t.category, t.id)
            }), i.search = function() {
                t.fireEvent(["ClientToolbox", n.params.category, i.searchText]), n.updateAssets(n.params.category, null, i.searchText, n.params.sortBy)
            }, i.$watch("searchText", function(t, i) {
                t != i && (n.searchText = t)
            }, !0), i.$watch(function() {
                return n.params
            }, function(n, t) {
                n && (i.searchText = n.keyword, !i.option && n.category ? i.option = i.options[n.category] : n.category != t.category && n.category != i.option.category && (i.option = i.options[n.category]))
            }, !0)
        }
    }
}]), clientToolbox.directive("robloxToolboxSubnav", ["ClientToolboxService", function(n) {
    return {
        restrict: "A",
        scope: !1,
        templateUrl: Roblox.ClientToolboxLinks.SubnavTemplateLink,
        replace: !0,
        link: function(t) {
            t.showReferralLinks = function() {
                return n.showReferralLinks && n.RESOURCES[n.params.category] && n.RESOURCES[n.params.category].searchable && n.RESOURCES[n.params.category].showSuggestedSearches
            }, t.sortTypes = n.SORTTYPES, t.sort = t.sortTypes[0], t.$watch("sort", function(t, i) {
                t && t != i && n.updateAssets(n.params.category, n.params.setId, n.searchText, t, null, n.params.creatorId)
            }, !0)
        }
    }
}]), clientToolbox.directive("robloxToolboxPagination", ["ClientToolboxService", "$log", function(n, t) {
    return {
        restrict: "A",
        scope: !1,
        templateUrl: Roblox.ClientToolboxLinks.PaginationTemplateLink,
        replace: !0,
        link: function(i) {
            i.pagination = function(i) {
                t.debug("next function is triggering !!");
                var r = i == "next" ? n.page.nextPage : n.page.previousPage;
                n.updateAssets(n.params.category, n.params.setId, n.params.keyword, n.params.sortBy, r, n.params.creatorId)
            }, i.$watch(function() {
                return n.page
            }, function(t, r) {
                t && t != r && (i.page = n.page)
            }, !0)
        }
    }
}]), clientToolbox.directive("robloxToolboxFooter", ["ClientToolboxService", function(n) {
    return {
        restrict: "A",
        scope: !1,
        templateUrl: Roblox.ClientToolboxLinks.FooterTemplateLink,
        replace: !0,
        link: function(t) {
            t.backgrounds = ["White", "Black", "None"], t.selectedBackground = "White", n.imageBackground = t.selectedBackground, t.isSelected = function(n) {
                return t.selectedBackground == n
            }, t.updateBg = function(i, r) {
                i && (t.selectedBackground = i), n.imageBackground = i || r || t.selectedBackground
            }
        }
    }
}]), clientToolbox.directive("robloxToolboxAssets", ["ClientToolboxService", "$log", function(n, t) {
    return {
        restrict: "A",
        scope: !1,
        templateUrl: Roblox.ClientToolboxLinks.AssetsTemplateLink,
        replace: !0,
        link: function(i) {
            i.showVoteContainer = !1, i.selectedIds = [], i.insertAsset = function() {
                var r = this.asset,
                    u, f;
                Roblox.ClientToolboxModel.IsUserAuthenticated && n.updateRecentlyInsertedAsset(r.Asset.Id).then(function(n) {
                    n && (r.Voting.CanVote = !0, i.selectedIds.push(r.Asset.Id))
                }, function(i, r) {
                    return t.debug(n.HTTPCALLFAILS + r), !1
                }), window.external && angular.isFunction(window.external.Insert) && angular.isFunction(window.external.GetStudioSessionID) && (window.external.Insert(Roblox.ClientToolboxModel.ContentUrl + "?id=" + r.Asset.Id, r.Asset.TypeId), Roblox.EventStream && (u = window.external.GetStudioSessionID(), f = {
                    assetId: r.Asset.Id,
                    searchText: i.searchText,
                    assetIndex: this.$index,
                    studioSid: u
                }, Roblox.EventStream.SendEventWithTarget("toolboxInsert", "click", f, Roblox.EventStream.TargetTypes.STUDIO)))
            }, i.handleVote = function(i) {
                var r = this.asset;
                this.asset.Voting.UserVote == i ? n.updateVoting(r.Asset.Id).then(function() {
                    r.Voting.UserVote = null, r.HasVoted = !1
                }, function(i, r) {
                    return t.debug(n.HTTPCALLFAILS + r), !1
                }) : n.updateVoting(r.Asset.Id, i).then(function() {
                    r.Voting.UserVote = i, r.Voting.HasVoted = !0
                }, function(i, r) {
                    return t.debug(n.HTTPCALLFAILS + r), !1
                })
            }, i.searchByCreator = function() {
                var t = Roblox.AssetType[this.asset.Asset.TypeId];
                n.disableUIUpdate = !0, n.creatorName = this.asset.Creator.Name, n.updateAssets(t, n.params.setId, null, null, 1, this.asset.Creator.Id, this.asset.Creator.Type)
            }, i.$watch(function() {
                return n.updateAssetsComplete
            }, function(r, u) {
                r != u && (i.loadingComplete = n.updateAssetsComplete, i.getAssetsError = n.getAssetsError, n.updateAssetsComplete && (n.disableUIUpdate = !1), n.updateAssetsComplete && !n.getAssetsError ? (t.debug("currentAssets is updated !"), i.isAssetsPageShown = n.page.totalAssetsNum > 0 ? !0 : !1, i.assets = n.currentAssets, i.creatorName = n.params.creatorId ? n.creatorName : null, i.page = n.page) : i.isAssetsPageShown = n.updateAssetsComplete ? !1 : n.params.pageNum > 1 ? !0 : !1)
            }, !0)
        }
    }
}]), clientToolbox.directive("robloxToolboxThumbnail", ["ClientToolboxService", "robloxImagesService", "$log", function(n, t, i) {
    return {
        restrict: "A",
        scope: {
            asset: "=",
            index: "@",
            insertAsset: "&"
        },
        replace: !0,
        templateUrl: Roblox.ClientToolboxLinks.ThumbnailTemplateLink,
        link: function(r, u) {
            r.imageUrl = r.asset.Thumbnail.Url, r.asset.Thumbnail.Final || t.getImageUrl(r.asset.Thumbnail, function(n) {
                r.imageUrl = n
            }, 0), r.isPlaying = !1, r.playSound = function() {
                var n = this.asset;
                r.isPlaying = !r.isPlaying, r.isPlaying && window.external.SoundPlaybackFinished.connect(function(t) {
                    t == n.Asset.Id && r.$apply(function() {
                        r.isPlaying = !1
                    })
                }), window.external.SetSoundPlaying(n.Asset.Id, r.isPlaying)
            }, u.bind("dragstart", function() {
                if (i.debug("drag start event "), Roblox.ClientToolboxModel.IsUserAuthenticated && n.updateRecentlyInsertedAsset(r.asset.Asset.Id).then(function(n) {
                        n && (r.asset.Voting.CanVote = !0, r.$root.selectedIds.push(parseInt(r.asset.Asset.Id)))
                    }, function(t, r) {
                        return i.debug(n.HTTPCALLFAILS + r), !1
                    }), window.external.StartDrag(Roblox.ClientToolboxModel.ContentUrl + "?id=" + r.asset.Asset.Id + "#assetTypeId=" + r.asset.Asset.TypeId), Roblox.EventStream) {
                    var u = window.external.GetStudioSessionID(),
                        f = {
                            assetId: r.asset.Asset.Id,
                            searchText: r.$root.searchText,
                            assetIndex: r.index,
                            studioSid: u
                        };
                    Roblox.EventStream.SendEventWithTarget("toolboxInsert", "drag", f, Roblox.EventStream.TargetTypes.STUDIO)
                }
            }), r.imageBackground = n.imageBackground, r.$watch(function() {
                return n.imageBackground
            }, function(n, t) {
                n != t && (r.imageBackground = n)
            }, !0), u.bind("dragover", function(n) {
                return n.preventDefault(), !1
            }), u.bind("drop", function(n) {
                n.preventDefault(), n.stopPropagation()
            })
        }
    }
}]);