"use strict";

avatar.controller("avatarItemsController", ["$scope", "$log", "$timeout", "$q", "avatarService", "cursorPaginationService", "avatarConstants", "avatarUrlConstants", "$document", function(n, t, i, r, u, f, e, o, s) {
        function a() {
            var t=Roblox.AvatarData.numberOfRecommendations, n=h.getPagingParameter(n); s.triggerHandler("Roblox.Recommendations.Init", [null, n, t])
        }

        function w() {
            var t=h.getPagingParameter("assetTypeId"), i=n.getAssetTypeName(t); n.absoluteCatalogUrl=Roblox.Endpoints.generateAbsoluteUrl(l[t]||o.www.catalog), s.triggerHandler("Roblox.Recommendations.GetItems", [t, i])
        }

        function b() {
            s.triggerHandler("Roblox.Recommendations.Clear")
        }

        function v() {
            n.items=[], c= {}

            , n.loading= !0, b(), h.loadFirstPage().then(function() {
                    n.loading= !1
                }

                , function() {})
        }

        function y() {
            var t=n.selectedMenu, i=t.fullLabel?t.fullLabel:t.label; n.emptyMessage=e.assets.emptyMessage+i
        }

        function p() {
            return n.selectedTab.tabType==="Assets"
        }

        var c, l, h; t.debug("avatarItemsController"), n.loading= !1, n.items=[], n.emptyMessage="", c= {}

        , n.absoluteCatalogUrl=Roblox.Endpoints.generateAbsoluteUrl(o.www.catalog), l=n.avatarDataModel.assetTypeToCatalogUrlMap, h=f.createPager({
            limitName:"itemsPerPage", sortOrder:f.sortOrder.Desc, pageSize:30, loadPageSize:50, getDataListFromResponse:function(n) {
                return n.Data?n.Data.Items:[]
            }

            , getNextPageCursorFromResponse:function(n) {
                return n.Data?n.Data.nextPageCursor:null
            }

            , getErrorsFromResponse:function(n) {
                return n.isValid?[]:[ {
                    code:0, message:n.Data||n.error||""
                }

                ]
            }

            , getCacheKeyParameters:function(n) {
                return {
                    assetTypeId:n.assetTypeId
                }
            }

            , getRequestUrl:function() {
                return o.www.inventoryJson
            }

            , beforeLoad:function() {}

            , loadSuccess:function(i) {
                var r=[], u=h.getPagingParameter("assetTypeId"), f=n.getAssetTypeName(u); angular.forEach(i, function(i) {
                        var e= {}

                        ; if(e.name=i.Item.Name, e.id=i.Item.AssetId, e.type="Asset", n.addItemThumbnailAndLink(e), n.$parent.updateItemSelected(e), e.expired=i.UserItem&&i.UserItem.IsRentalExpired, e.thumbnail=i.Thumbnail, e.assetType= {
                                id:u, name:f
                            }

                            , e.expired) {
                            t.debug(e.name+" is expired!"); return
                        }

                        c[e.id]?t.debug(e.name+" is a duplicate"):(c[e.id]= !0, r.push(e))
                    }), [].push.apply(n.items, r), h.hasNextPage()?n.$emit("manualInfiniteScrollCheck"):(t.debug("Doesn't have next page, loading recommendations"), w())
            }

            , loadError:function() {
                n.systemFeedback.error(e.assets.couldNotLoadList)
            }

        }), n.getNextPage=function() {
        if(n.selectedTab.tabType==="Assets" &&h.canLoadNextPage()) {
            var t=h.loadNextPage(); n.loading= !0, t.then(function() {
                    n.loading= !1
                }

                , function() {})
        }
    }

    , n.$on(e.events.wornAssetsChanged, function() {
            angular.forEach(n.items, function(t) {
                    n.$parent.updateItemSelected(t)
                })

        }), n.$on(e.events.menuClicked, function(t, i) {
            var u=i.menu, r; p()&&u.assetType&&(r=n.getAssetTypeByName(u.assetType), r&&(h.setPagingParameter("assetTypeId", r.id), y(), v()))

        }), h.setPagingParameter("userId", n.avatarDataModel.userId), h.setPagingParameter("assetTypeId", e.assets.hatAssetTypeId), y(), a(); $(document).on("Roblox.Recommendations.FinishedInit", function() {
            a()
        }); p()&&v()
}

]);