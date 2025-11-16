"use strict";

avatar.controller("recentItemsController", ["$scope", "$log", "$timeout", "$q", "avatarService", "avatarConstants", function(n, t, i, r, u, f) {
        function h() {
            return n.selectedTab.name==="Recent"
        }

        function s() {
            !h()||e||n.loading||(n.loading= !0, n.items=[], u.getRecentItems(o).then(function(t) {
                        var i, r; for(n.items=[], i=0; i<t.data.length; i++)r=t.data[i], n.addItemThumbnailAndLink(r), n.updateItemSelected(r); n.items=t.data, n.loading= !1, e= !0
                    }

                    , function() {
                        n.systemFeedback.error(f.recent.couldNotLoadList)
                    }))
        }

        n.loading= !1, n.items=[], n.emptyMessage=f.recent.emptyMessage; var o="All", e= !1; n.$on(f.events.wornAssetsChanged, function() {
                angular.forEach(n.items, function(t) {
                        n.$parent.updateItemSelected(t)
                    })

            }), n.$on(f.events.outfitDeleted, function(t, i) {
                angular.forEach(n.items, function(t, r) {
                        t.id===i.id&&n.items.splice(r, 1)
                    })

            }), n.$on(f.events.menuClicked, function(n, t) {
                var i=t.tab, r=t.menu; i.name==="Recent" &&(o=r.name, e= !1, s())
            }), s()
    }

    ]);