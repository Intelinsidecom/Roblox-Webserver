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
                var r=[], u=h.getPagingParameter("assetTypeId"), f=n.getAssetTypeName(u);
                console.log("[AvatarItems] loadSuccess invoked. assetTypeId:", u, "name:", f);
                console.log("[AvatarItems] Raw response items length:", Array.isArray(i)?i.length:"?", "payload:", i);

                angular.forEach(i, function(i) {
                        var e= {};

                        try {
                            console.log("[AvatarItems] Processing asset", i && i.Item ? i.Item.AssetId : "unknown", i && i.Item ? i.Item.Name : "?");

                            if(!i || !i.Item) {
                                console.warn("[AvatarItems] Skipping item with missing Item payload", i);
                                return
                            }

                            e.name=i.Item.Name;
                            e.id=i.Item.AssetId;
                            e.type="Asset";

                            // Some helper methods may not exist depending on parent scope; guard them.
                            if(typeof n.addItemThumbnailAndLink==="function") {
                                n.addItemThumbnailAndLink(e)
                            } else {
                                console.warn("[AvatarItems] addItemThumbnailAndLink is not a function on scope", n)
                            }

                            if(n.$parent && typeof n.$parent.updateItemSelected==="function") {
                                n.$parent.updateItemSelected(e)
                            } else {
                                console.warn("[AvatarItems] $parent.updateItemSelected is not available; selection state may be stale")
                            }

                            e.expired=i.UserItem&&i.UserItem.IsRentalExpired;

                            // Keep full server payload for templates that still reference it
                            e.Item = i.Item;
                            e.Thumbnail = i.Thumbnail;

                            // Fallback URL when no CDN thumbnail yet
                            e.thumbnailUrl = (i.Thumbnail && i.Thumbnail.Url) ? i.Thumbnail.Url : (e.thumbnail && e.thumbnail.RetryUrl);

                            e.assetType= {
                                id:u,
                                name:f
                            };

                            if(e.expired) {
                                t.debug(e.name+" is expired!");
                                return
                            }

                            if(c[e.id]) {
                                console.log("[AvatarItems] Duplicate skipped", e.id);
                                t.debug(e.name+" is a duplicate");
                                return
                            }

                            c[e.id]= !0;
                            r.push(e);
                            console.log("[AvatarItems] Added item", e.id)
                        } catch(ex) {
                            console.error("[AvatarItems] Error while processing inventory item", i, ex)
                        }
                    });

                [].push.apply(n.items, r);
                console.log("[AvatarItems] Items added this page:", r.length, "Total items now:", n.items.length);

                // Ensure Angular updates bindings even if outside digest
                n.$applyAsync && n.$applyAsync();

                h.hasNextPage()?n.$emit("manualInfiniteScrollCheck"):(t.debug("Doesn't have next page, loading recommendations"), w())
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