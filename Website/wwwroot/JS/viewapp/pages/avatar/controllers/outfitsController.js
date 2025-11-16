"use strict";

avatar.controller("outfitsController", ["$scope", "$log", "$timeout", "$q", "avatarService", "cursorPaginationService", "avatarConstants", function(n, t, i, r, u, f, e) {
        n.loading= !1, n.items=[], n.emptyMessage=e.outfits.emptyMessage; var o= !0, s=1, h=50, c=function() {
            s=1, o= !0, n.loading= !1, n.items=[], n.getNextPage()
        }

        ; n.getNextPage=function() {
            n.selectedTab.tabType==="Outfits" && !n.loading&&o&&(n.loading= !0, u.getOutfits(n.avatarDataModel.userId, s, h).then(function(t) {
                        n.loading= !1, o=t.data.length===h, s+=1, angular.forEach(t.data, function(t) {
                                t.type="Outfit", n.addItemThumbnailAndLink(t)
                            }), n.items=n.items.concat(t.data)
                    }

                    , function() {
                        n.systemFeedback.error(e.outfits.failedToLoadOutfits)
                    }))
        }

        , n.$on(e.events.outfitsChanged, function() {
                c()

            }), n.$on(e.events.outfitDeleted, function(t, i) {
                angular.forEach(n.items, function(t, r) {
                        t.id===i.id&&n.items.splice(r, 1)
                    })

            }), n.$on(e.events.menuClicked, function(n, t) {
                var i=t.tab; i.name==="Outfits" &&c()
            })
    }

    ]);