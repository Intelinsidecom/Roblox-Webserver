"use strict";

recommendations.factory("recommendationsService", ["httpService", "assetTypes", "$log", function(n, t) {
        return {
            canShowRecommendationsForAssetType:function(n) {
                var i=t.ids; return n>0&&n !==i.gamePasses&&n !==i.badges
            }

            , beginUpdateRecommendedItems:function(t, i, r, u, f) {
                var s=u+"/v1/recommendations/" +i, o= {
                    numItems:r
                }

                , e; return t>0&&(o.contextAssetId=t), e= {
                    url:s
                }

                , f||(e.url="/assets/recommended-json", e.noCache= !0, o.assetTypeId=i), n.httpGet(e, o)
            }
        }
    }

    ]);