"use strict";

avatar.constant("avatarUrlConstants", {
    www: {
        assetThumbnail:"/asset-thumbnail/json?width=150&height=150&format=png&assetId=", outfitThumbnail:"/outfit-thumbnail/json?width=150&height=150&format=png&userOutfitId=", inventoryJson:"/users/inventory/list-json", outfitThumbnailJson:"/outfit-thumbnail/json", catalog:"/catalog", avatar:"/my/avatar"
    }

    , avatarApi: {
        getOutfitDetailsUrl:"/v1/outfits/{id}/details", createOutfitUrl:"/v1/outfits/create", deleteOutfitUrl:"/v1/outfits/{id}/delete", updateOutfitUrl:"/v1/outfits/{id}/update", wearOutfitUrl:"/v1/outfits/{id}/wear", getOutfitsUrl:"/v1/users/{userId}/outfits", patchOutfitUrl:"/v1/outfits/{id}", setBodyColorsUrl:"/v1/avatar/set-body-colors", setScalesUrl:"/v1/avatar/set-scales", setAvatarTypeUrl:"/v1/avatar/set-player-avatar-type", getAvatarUrl:"/v1/avatar", getAvatarRulesUrl:"/v1/avatar-rules", getRecentItemsUrl:"/v1/recent-items/{type}/list", wearAssetUrl:"/v1/avatar/assets/{id}/wear", removeAssetUrl:"/v1/avatar/assets/{id}/remove", setWearingAssetsUrl:"/v1/avatar/set-wearing-assets", redrawThumbnailUrl:"/v1/avatar/redraw-thumbnail"
    }
});