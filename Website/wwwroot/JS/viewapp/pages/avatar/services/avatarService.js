"use strict";

avatar.factory("avatarService", ["$log", "$q", "httpService", "avatarUrlConstants", function(n, t, i, r) {
        function f(n, t) {
            return e+Roblox.Endpoints.generateAbsoluteUrl(n,t,!0)
        }

        var e=Roblox.AvatarData.avatarDomain, h= {
            getOutfitDetailsUrl:"/v1/outfits/{id}/details", createOutfitUrl:"/v1/outfits/create", deleteOutfitUrl:"/v1/outfits/{id}/delete", patchOutfitUrl:"/v1/outfits/{id}", wearOutfitUrl:"/v1/outfits/{id}/wear", getOutfitsUrl:"/v1/users/{userId}/outfits", setBodyColorsUrl:"/v1/avatar/set-body-colors", setScalesUrl:"/v1/avatar/set-scales", setAvatarTypeUrl:"/v1/avatar/set-player-avatar-type", getAvatarUrl:"/v1/avatar", getAvatarRulesUrl:"/v1/avatar-rules", getRecentItemsUrl:"/v1/recent-items/{type}/list", wearAssetUrl:"/v1/avatar/assets/{id}/wear", removeAssetUrl:"/v1/avatar/assets/{id}/remove", setWearingAssetsUrl:"/v1/avatar/set-wearing-assets", redrawThumbnailUrl:"/v1/avatar/redraw-thumbnail"
        }

        , u=r.avatarApi, o=r.www; return {
            getOutfitThumbnailForDownload:function(n) {
                var t=o.outfitThumbnailJson, r= {
                    url:t
                }

                , u= {
                    width:"352", height:"352", format:"png", userOutfitId:n
                }

                ; return i.httpGet(r, u)
            }

            , getOutfitDetails:function(n) {
                var t=f(u.getOutfitDetailsUrl, {
                    id:n

                }), r= {
                url:t
            }

            , e= {}

            ; return i.httpGet(r, e)
        }

        , createOutfit:function(n, t, r) {
            var e=f(u.createOutfitUrl), o= {
                url:e, withCredentials: !0
            }

            , s= {
                name:n, bodyColors:t, assetIds:r
            }

            ; return i.httpPost(o, s)
        }

        , patchOutfit:function(t, r) {
            n.debug("Patch outfit"); var e=f(u.patchOutfitUrl, {
                id:t

            }), o= {
            url:e, withCredentials: !0
        }

        ; return i.httpPatch(o, r)
    }

    , deleteOutfit:function(n) {
        var t=f(u.deleteOutfitUrl, {
            id:n

        }), r= {
        url:t, withCredentials: !0
    }

    , e= {}

    ; return i.httpPost(r, e)
}

, getOutfits:function(n, t, r) {
    var e=f(u.getOutfitsUrl, {
        userId:n

    }), o= {
    url:e, retryable: !0
}

, s= {
    itemsPerPage:r, page:t
}

; return i.httpGet(o, s)
}

, setBodyColors:function(n) {
    var t=f(u.setBodyColorsUrl), r= {
        url:t, withCredentials: !0
    }

    ; return i.httpPost(r, n)
}

, setWearingAssets:function(n) {
    var t=f(u.setWearingAssetsUrl), r= {
        url:t, withCredentials: !0
    }

    , e= {
        assetIds:n
    }

    ; return i.httpPost(r, e)
}

, wearAsset:function(n) {
    var t=f(u.wearAssetUrl, {
        id:n

    }), r= {
    url:t, withCredentials: !0
}

, e= {}

; return i.httpPost(r, e)
}

, removeAsset:function(n) {
    var t=f(u.removeAssetUrl, {
        id:n

    }), r= {
    url:t, withCredentials: !0
}

, e= {}

; return i.httpPost(r, e)
}

, wearOutfit:function(n) {
    var t=f(u.wearOutfitUrl, {
        id:n

    }), r= {
    url:t, withCredentials: !0
}

, e= {}

; return i.httpPost(r, e)
}

, getAvatar:function() {
    var n=f(u.getAvatarUrl), t= {
        url:n, withCredentials: !0, retryable: !0
    }

    , r= {}

    ; return i.httpGet(t, r)
}

, getRules:function() {
    var n=f(u.getAvatarRulesUrl), t= {
        url:n
    }

    , r= {}

    ; return i.httpGet(t, r)
}

, setScales:function(n) {
    var t=f(u.setScalesUrl), r= {
        url:t, withCredentials: !0
    }

    ; return i.httpPost(r, n)
}

, setAvatarType:function(n) {
    var t=f(u.setAvatarTypeUrl), r= {
        url:t, withCredentials: !0
    }

    , e= {
        playerAvatarType:n
    }

    ; return i.httpPost(r, e)
}

, getRecentItems:function(n) {
    var t=f(u.getRecentItemsUrl, {
        type:n

    }), r= {
    url:t, withCredentials: !0, retryable: !0
}

, e= {}

; return i.httpGet(r, e)
}

, redrawThumbnail:function() {
    var n=f(u.redrawThumbnailUrl), t= {
        url:n, withCredentials: !0, retryable: !1
    }

    , r= {}

    ; return i.httpPost(t, r)
}
}
}

]);