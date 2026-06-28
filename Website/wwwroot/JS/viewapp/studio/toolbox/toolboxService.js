// ~/viewapp/studio/toolbox/toolboxService.js
"use strict";
clientToolbox.factory("ClientToolboxService", ["httpService", "$q", "$log", function(n, t, i) {
    var r = {
        RESOURCES: {
            FreeMeshes: {
                id: null,
                name: "Meshes",
                category: "FreeMeshes",
                searchable: !0,
                hasChildren: !1,
                showSuggestedSearches: !0,
                sortable: !0
            },
            FreeModels: {
                id: null,
                name: "Models",
                category: "FreeModels",
                searchable: !0,
                hasChildren: !1,
                showSuggestedSearches: !0,
                sortable: !0
            },
            FreeDecals: {
                id: null,
                name: "Decals",
                category: "FreeDecals",
                searchable: !0,
                hasChildren: !1,
                showSuggestedSearches: !0,
                sortable: !0
            },
            FreeAudio: {
                id: null,
                name: "Audio",
                category: "FreeAudio",
                searchable: !0,
                hasChildren: !1,
                showSuggestedSearches: !0,
                sortable: !0
            },
            RecentMeshes: {
                id: null,
                name: "Recent Meshes",
                category: "RecentMeshes",
                searchable: !1,
                hasChildren: !1,
                showSuggestedSearches: !1,
                sortable: !1
            },
            RecentModels: {
                id: null,
                name: "Recent Models",
                category: "RecentModels",
                searchable: !1,
                hasChildren: !1,
                showSuggestedSearches: !1,
                sortable: !1
            },
            RecentDecals: {
                id: null,
                name: "Recent Decals",
                category: "RecentDecals",
                searchable: !1,
                hasChildren: !1,
                showSuggestedSearches: !1,
                sortable: !1
            },
            RecentAudio: {
                id: null,
                name: "Recent Audio",
                category: "RecentAudio",
                searchable: !1,
                hasChildren: !1,
                showSuggestedSearches: !1,
                sortable: !1
            },
            MyMeshes: {
                id: null,
                name: "My Meshes",
                category: "MyMeshes",
                searchable: !0,
                hasChildren: !1,
                showSuggestedSearches: !1,
                sortable: !1
            },
            MyModels: {
                id: null,
                name: "My Models",
                category: "MyModels",
                searchable: !0,
                hasChildren: !1,
                showSuggestedSearches: !1,
                sortable: !1
            },
            MyDecals: {
                id: null,
                name: "My Decals",
                category: "MyDecals",
                searchable: !0,
                hasChildren: !1,
                showSuggestedSearches: !1,
                sortable: !1
            },
            MyAudio: {
                id: null,
                name: "My Audio",
                category: "MyAudio",
                searchable: !0,
                hasChildren: !1,
                showSuggestedSearches: !1,
                sortable: !1
            },
            MySets: {
                id: null,
                name: "My Sets",
                category: "MySets",
                searchable: !1,
                hasChildren: !0,
                showSuggestedSearches: !1,
                sortable: !1
            },
            MySubscribedSets: {
                id: null,
                name: "My Subscribed Sets",
                category: "MySubscribedSets",
                searchable: !1,
                hasChildren: !0,
                showSuggestedSearches: !1,
                sortable: !1
            },
            RobloxSets: {
                id: null,
                name: "ROBLOX Sets",
                category: "RobloxSets",
                searchable: !1,
                hasChildren: !0,
                showSuggestedSearches: !1,
                sortable: !1
            },
            GroupMeshes: {
                id: null,
                name: "Group Meshes",
                category: "GroupMeshes",
                searchable: !1,
                hasChildren: !0,
                showSuggestedSearches: !1,
                sortable: !1
            },
            GroupModels: {
                id: null,
                name: "Group Models",
                category: "GroupModels",
                searchable: !1,
                hasChildren: !0,
                showSuggestedSearches: !1,
                sortable: !1
            },
            GroupDecals: {
                id: null,
                name: "Group Decals",
                category: "GroupDecals",
                searchable: !1,
                hasChildren: !0,
                showSuggestedSearches: !1,
                sortable: !1
            },
            GroupAudio: {
                id: null,
                name: "Group Audio",
                category: "GroupAudio",
                searchable: !1,
                hasChildren: !0,
                showSuggestedSearches: !1,
                sortable: !1
            }
        },
        ROBLOXID: 1,
        SORTTYPES: ["Relevance", "Most Taken", "Favorites", "Updated", "Ratings"],
        DEFAULTSORDER: "Relevance",
        INITIALPAGENUM: 1,
        NUMOFITEMSPERPAGE: 30,
        RETRYTIMER: 1500,
        SEARCHFIELD: "searchset",
        SELECTFIELD: "selectedset",
        SELECTEDVALUE: "getmymodels",
        HTTPCALLFAILS: "Errors from http call: ",
        initializeSet: "FreeModels",
        disableUIUpdate: !1,
        updateAssetsComplete: null,
        getAssetsError: !1,
        currentAssets: {},
        allSets: {},
        page: {
            totalPage: 1,
            currentPage: 1,
            nextPage: 2,
            previousPage: 1,
            totalAssetsNum: 0
        },
        params: {
            category: "",
            setId: "",
            keyword: "",
            sortBy: "Relevance",
            pageNum: 1,
            creatorId: "",
            creatorType: ""
        },
        creatorName: "",
        searchText: "",
        showReferralLinks: Roblox.EnableNewToolboxSearch,
        showAudio: Roblox.AreAudioShown,
        updateAllSets: function(n, t) {
            if (t.length > 0) {
                r.allSets[n] = r.RESOURCES[n], r.allSets[n].children = [];
                for (var i = 0; i < t.length; i++) r.allSets[n].children.push({
                    id: t[i].Id,
                    category: r.RESOURCES[n].category,
                    name: t[i].Name,
                    searchable: r.RESOURCES[n].searchable
                })
            }
        },
        getSetUrl: function(n, t) {
            var i = {};
            switch (n) {
                case r.RESOURCES.GroupModels.category:
                case r.RESOURCES.GroupMeshes.category:
                case r.RESOURCES.GroupDecals.category:
                case r.RESOURCES.GroupAudio.category:
                    i = {
                        url: Roblox.ClientToolboxLinks.GetGroupsCanManageGamesInLink,
                        params: {}
                    };
                    break;
                case r.RESOURCES.MySets.category:
                    i = {
                        url: Roblox.ClientToolboxLinks.GetSetsByCreatorLink,
                        params: {
                            userid: t
                        }
                    };
                    break;
                case r.RESOURCES.RobloxSets.category:
                    i = {
                        url: Roblox.ClientToolboxLinks.GetRobloxSetsLink,
                        params: {}
                    };
                    break;
                case r.RESOURCES.MySubscribedSets.category:
                    i = {
                        url: Roblox.ClientToolboxLinks.GetSubscribedSetsLink,
                        params: {
                            userid: t
                        }
                    }
            }
            return i
        },
        getSets: function(t, u) {
            var f = r.getSetUrl(t, u),
                e = {
                    url: f.url
                };
            return n.httpGet(e, f.params).then(function(n) {
                return r.updateAllSets(t, n), n
            }, function() {
                return i.debug(r.HTTPCALLFAILS + status), {
                    status: !1
                }
            })
        },
        getAllSets: function() {
            return r.allSets[r.RESOURCES.FreeModels.category] = r.RESOURCES.FreeModels, r.allSets[r.RESOURCES.FreeDecals.category] = r.RESOURCES.FreeDecals, Roblox.AreMeshesEnabled === !0 && (r.allSets[r.RESOURCES.FreeMeshes.category] = r.RESOURCES.FreeMeshes), Roblox.AreAudioShown && (r.allSets[r.RESOURCES.FreeAudio.category] = r.RESOURCES.FreeAudio), r.getSets(r.RESOURCES.RobloxSets.category, r.ROBLOXID).then(function() {
                if (Roblox.ClientToolboxModel.IsUserAuthenticated) {
                    r.allSets[r.RESOURCES.RecentModels.category] = r.RESOURCES.RecentModels, r.allSets[r.RESOURCES.RecentDecals.category] = r.RESOURCES.RecentDecals, r.allSets[r.RESOURCES.MyModels.category] = r.RESOURCES.MyModels, r.allSets[r.RESOURCES.MyDecals.category] = r.RESOURCES.MyDecals, Roblox.AreMeshesEnabled === !0 && (r.allSets[r.RESOURCES.RecentMeshes.category] = r.RESOURCES.RecentMeshes, r.allSets[r.RESOURCES.MyMeshes.category] = r.RESOURCES.MyMeshes), Roblox.AreAudioShown === !0 && (r.allSets[r.RESOURCES.RecentAudio.category] = r.RESOURCES.RecentAudio, r.allSets[r.RESOURCES.MyAudio.category] = r.RESOURCES.MyAudio);
                    var n = [];
                    return Roblox.ClientToolboxModel.ShowGroupCategories && (n.push(r.getSets(r.RESOURCES.GroupModels.category, Roblox.ClientToolboxModel.UserId)), n.push(r.getSets(r.RESOURCES.GroupDecals.category, Roblox.ClientToolboxModel.UserId)), Roblox.AreMeshesEnabled === !0 && n.push(r.getSets(r.RESOURCES.GroupMeshes.category, Roblox.ClientToolboxModel.UserId)), Roblox.AreAudioShown && n.push(r.getSets(r.RESOURCES.GroupAudio.category, Roblox.ClientToolboxModel.UserId))), n.push(r.getSets(r.RESOURCES.MySets.category, Roblox.ClientToolboxModel.UserId)), n.push(r.getSets(r.RESOURCES.MySubscribedSets.category, Roblox.ClientToolboxModel.UserId)), t.all(n)
                }
            })
        },
        updatePage: function(n) {
            var t = r.getAssetsError ? 0 : Math.ceil(n.TotalResults / r.NUMOFITEMSPERPAGE);
            r.page = {
                totalPage: t,
                totalAssetsNum: r.getAssetsError ? 0 : n.TotalResults,
                currentPage: r.params.pageNum,
                nextPage: t > r.params.pageNum ? r.params.pageNum + 1 : t,
                previousPage: t > 1 ? r.params.pageNum - 1 : t
            }
        },
        updateParams: function(n, t, i, u, f, e, o) {
            r.params = {
                category: n ? n : r.params.category,
                setId: t,
                keyword: i ? i : "",
                sortBy: u ? u : r.DEFAULTSORDER,
                pageNum: f ? f : r.INITIALPAGENUM,
                creatorId: e ? e : "",
                creatorType: o ? o : ""
            }
        },
        updateCurrentAssets: function(n, t) {
            r.updateAssetsComplete = !0, r.getAssetsError = t, r.currentAssets = r.params.pageNum > 1 ? t ? null : r.currentAssets.concat(n.Results) : t ? null : n.Results, r.updatePage(n)
        },
        getAssetUrl: function(n, t, i, u, f, e, o) {
            var s = {},
                h, c;
            switch (n) {
                case r.RESOURCES.GroupDecals.category:
                case r.RESOURCES.GroupModels.category:
                case r.RESOURCES.GroupMeshes.category:
                case r.RESOURCES.GroupAudio.category:
                    s = {
                        url: Roblox.ClientToolboxLinks.GetAssetsLink,
                        params: {
                            sort: u ? u.replace(/ /g, "") : r.DEFAULTSORDER,
                            category: n,
                            keyword: i ? i : "",
                            page: f ? f : r.INITIALPAGENUM,
                            num: r.NUMOFITEMSPERPAGE,
                            creatorId: e,
                            creatorType: o,
                            groupId: t
                        }
                    };
                    break;
                case r.RESOURCES.MySets.category:
                case r.RESOURCES.RobloxSets.category:
                    h = Roblox.ClientToolboxLinks.GetSetItemsLink.replace("0", t), s = {
                        url: h,
                        params: {
                            page: f ? f : r.INITIALPAGENUM,
                            num: r.NUMOFITEMSPERPAGE
                        }
                    };
                    break;
                case r.RESOURCES.MySubscribedSets.category:
                    c = Roblox.ClientToolboxLinks.GetSetItemsLink.replace("0", t), s = {
                        url: c,
                        params: {
                            page: f ? f : r.INITIALPAGENUM,
                            num: r.NUMOFITEMSPERPAGE
                        }
                    };
                    break;
                case r.RESOURCES.MyModels.category:
                case r.RESOURCES.MyDecals.category:
                case r.RESOURCES.MyMeshes.category:
                case r.RESOURCES.MyAudio.category:
                    e = Roblox.ClientToolboxModel.UserId;
                default:
                    s = {
                        url: Roblox.ClientToolboxLinks.GetAssetsLink,
                        params: {
                            sort: u ? u.replace(/ /g, "") : r.DEFAULTSORDER,
                            category: n,
                            keyword: i ? i : "",
                            page: f ? f : r.INITIALPAGENUM,
                            num: r.NUMOFITEMSPERPAGE,
                            creatorType: o,
                            creatorId: e
                        }
                    }
            }
            return s
        },
        updateAssets: function(t, u, f, e, o, s, h) {
            var c = r.getAssetUrl(t, u, f, e, o, s, h),
                l = {
                    url: c.url
                };
            r.updateParams(t, u, f, e, o, s, h), r.updateAssetsComplete = !1, Roblox.AreAudioShown && window.external && window.external.StopAllSounds && window.external.StopAllSounds(), Roblox.EnableNewToolboxSearch && (r.showReferralLinks = r.searchText == ""), n.httpGet(l, c.params).then(function(n) {
                r.updateCurrentAssets(n, !1)
            }, function(n, t) {
                i.debug(r.HTTPCALLFAILS + t), r.updateCurrentAssets(null, !0)
            })
        },
        updateRecentlyInsertedAsset: function(t) {
            var u = {
                    url: Roblox.ClientToolboxLinks.InsertAssetLink
                },
                f = {
                    assetId: t
                };
            return n.httpPost(u, f).then(function(n) {
                return n
            }, function(n, t) {
                return i.debug(r.HTTPCALLFAILS + t), {
                    status: !1
                }
            })
        },
        updateVoting: function(t, u) {
            var f = typeof u == "undefined" || u == null ? Roblox.ClientToolboxLinks.UnvoteStudioLink : Roblox.ClientToolboxLinks.VoteStudioLink,
                e = angular.element(document.querySelector("input[name='__RequestVerificationToken']")).attr("value"),
                o = {
                    url: f
                },
                s = {
                    assetId: t,
                    __RequestVerificationToken: e,
                    vote: u
                };
            return n.httpPost(o, s).then(function(n) {
                return n
            }, function(n, t) {
                return i.debug(r.HTTPCALLFAILS + t), {
                    status: !1
                }
            })
        }
    };
    return r
}]);