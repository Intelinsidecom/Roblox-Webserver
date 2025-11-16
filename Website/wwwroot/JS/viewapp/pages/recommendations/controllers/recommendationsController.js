"use strict";

recommendations.controller("recommendationsController", ["$scope", "recommendationsService", "$log", "$document", function(n, t, i, r) {
        var u=0; n.initialize=function() {
            n.recommendationsLayout=n.recommendationsLayout?n.recommendationsLayout: {
                numberOfItemsToDisplay:0, assetId:0
            }

            , n.recommendationsData=n.recommendationsData?n.recommendationsData: {
                items:[], assetTypeName:"items"
            }
        }

        , n.initializeWithModelValues=function(t, i, r) {
            n.recommendationsLayout.numberOfItemsToDisplay=r, n.recommendationsLayout.assetId=t, n.getItems(i)
        }

        ; $(document).on("Roblox.Recommendations.Init", function(t, i, r, u) {
                n.initializeWithModelValues(i, r, u)

            }); $(document).on("Roblox.Recommendations.GetItems", function(t, i, r) {
                n.getItems(i, r)

            }); $(document).on("Roblox.Recommendations.Clear", function() {
                n.recommendationsData.items=[], u++

            }); n.getItems=function(r, f) {
            if(n.recommendationsData.items=[], t.canShowRecommendationsForAssetType(r)) {
                f&&(n.recommendationsData.assetTypeName=f); var o=Roblox.InventoryData.inventoryDomain, e=Roblox.InventoryData.useInventorySite, s=++u; t.beginUpdateRecommendedItems(n.recommendationsLayout.assetId, r, n.recommendationsLayout.numberOfItemsToDisplay, o, e).then(function(t) {
                        if(s !==u) {
                            i.debug("Request came back but was not latest"); return
                        }

                        t&&t.data?e?(angular.forEach(t.data, function(n) {
                                    n.HasPrice=n.Item&&n.Item.Price

                                }), n.recommendationsData.items=t.data):(angular.forEach(t.data.Items, function(n) {
                                    n.HasPrice=n.Item&&n.Item.Price
                                }), n.recommendationsData.items=t.data.Items):i.debug(" ------ beginUpdateRecommendedItems no data received -------")
                    }

                    , function() {
                        i.debug(" ------ beginUpdateRecommendedItems error -------")
                    })
            }
        }

        , n.initialize(), r.triggerHandler("Roblox.Recommendations.FinishedInit")
    }

    ]);