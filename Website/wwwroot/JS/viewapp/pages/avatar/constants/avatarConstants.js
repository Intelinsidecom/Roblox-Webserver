"use strict";

avatar.constant("avatarConstants", {
    events: {
        avatarDetailsLoaded:"Roblox.Avatar.AvatarDetailsLoaded", avatarRulesLoaded:"Roblox.Avatar.AvatarRulesLoaded", menuClicked:"Roblox.Avatar.MenuClicked", outfitsChanged:"Roblox.Avatar.OutfitsChanged", outfitDeleted:"Roblox.Avatar.OutfitDeleted", wornAssetsChanged:"Roblox.Avatar.WornAssetsChanged", metaDataLoaded:"Roblox.Avatar.MetaDataLoaded", bodyColorsChanged:"Roblox.Avatar.BodyColorsChanged"
    }

    , tabs:[ {
        label:"Recent", name:"Recent", menus:[ {
            name:"All", label:"All"
        }

        , {
        name:"Accessories", label:"Accessories"
    }

    , {
    name:"Clothing", label:"Clothing"
}

, {
name:"BodyParts", label:"Body Parts"
}

, {
name:"AvatarAnimations", label:"Animations"
}

, {
name:"Outfits", label:"Outfits"
}

]
}

, {
label:"Clothing", name:"Clothing", tabType:"Assets", menuType:"Nested", rows:[ {
    title:"Accessories", menus:[ {
        name:"Hat", label:"Hat", fullLabel:"Hat Accessories", assetType:"Hat"
    }

    , {
    name:"Hair", label:"Hair", fullLabel:"Hair Accessories", assetType:"Hair Accessory"
}

, {
name:"Face", label:"Face", fullLabel:"Face Accessories", assetType:"Face Accessory"
}

, {
name:"Neck", label:"Neck", fullLabel:"Neck Accessories", assetType:"Neck Accessory"
}

, {
name:"Shoulders", label:"Shoulders", fullLabel:"Shoulder Accessories", assetType:"Shoulder Accessory"
}

, {
name:"Front", label:"Front", fullLabel:"Front Accessories", assetType:"Front Accessory"
}

, {
name:"Back", label:"Back", fullLabel:"Back Accessories", assetType:"Back Accessory"
}

, {
name:"Waist", label:"Waist", fullLabel:"Waist Accessories", assetType:"Waist Accessory"
}

]
}

, {
title:"Clothes", menus:[ {
    name:"Shirts", label:"Shirts", assetType:"Shirt"
}

, {
name:"Pants", label:"Pants", assetType:"Pants"
}

, {
name:"T-Shirts", label:"T-Shirts", assetType:"T-Shirt"
}

]
}

, {
title:"Gear", menus:[ {
    name:"Gear", label:"Gear", assetType:"Gear"
}

]
}

]
}

, {
label:"Body", name:"Body", tabType:"Assets", menus:[ {
    name:"BodyColors", label:"Skin Tone"
}

, {
name:"Package", label:"Packages", assetType:"Package"
}

, {
name:"Face", label:"Face", assetType:"Face"
}

, {
name:"Head", label:"Head", assetType:"Head"
}

, {
name:"Torso", label:"Torso", fullLabel:"Torsos", assetType:"Torso"
}

, {
name:"LeftArms", label:"Left Arms", assetType:"Left Arm"
}

, {
name:"RightArms", label:"Right Arms", assetType:"Right Arm"
}

, {
name:"LeftLegs", label:"Left Legs", assetType:"Left Leg"
}

, {
name:"RightLegs", label:"Right Legs", assetType:"Right Leg"
}

]
}

, {
label:"Animations", name:"Animations", tabType:"Assets", menus:[ {
    name:"Walk", label:"Walk", fullLabel:"Walk Animations", assetType:"Walk Animation"
}

, {
name:"Run", label:"Run", fullLabel:"Run Animations", assetType:"Run Animation"
}

, {
name:"Fall", label:"Fall", fullLabel:"Fall Animations", assetType:"Fall Animation"
}

, {
name:"Jump", label:"Jump", fullLabel:"Jump Animations", assetType:"Jump Animation"
}

, {
name:"Swim", label:"Swim", fullLabel:"Swim Animations", assetType:"Swim Animation"
}

, {
name:"Climb", label:"Climb", fullLabel:"Climb Animations", assetType:"Climb Animation"
}

, {
name:"Idle", label:"Idle", fullLabel:"Idle Animations", assetType:"Idle Animation"
}

]
}

, {
label:"Outfits", name:"Outfits", tabType:"Outfits", menus:[], noMenus: !0
}

], thumbnail: {
    redrawFloodchecked:"You have redrawn your avatar too many times, please try again later", redrawThumbnailFailed:"Failed to redraw thumbnail", waitForThumbnailRegenerationInSeconds:30
}

, recent: {
    couldNotLoadList:"Failed to load recent items", emptyMessage:"You don't have any recent items"
}

, outfits: {
    renameOutfitTitle:"Rename Outfit", renameOutfitDescription:"Choose a new name for your outfit", renameOutfitButton:"Rename", createOutfitTitle:"Create New Outfit", createOutfitDescription:"An outfit will be created from your avatar's current appearance", createOutfitButton:"Create", successfulRename:"Renamed outfit", successfulDelete:"Deleted outfit", successfulUpdate:"Updated outfit", successfulCreate:"Created outfit", successfulWear:"Successfully wore outfit", emptyMessage:"You don't have any outfits. Try creating some!", maxNumberOfOutfits:"You have reached the maximum number of outfits", errorCreatingOutfit:"Unable to create outfit, try again later", invalidOutfitName:"Name must be appropriate and less than 200 characters", updateFailedOutfitDelete:"The outfit you tried to update no longer exists", errorUpdatingOutfit:"Outfit update failed, please try again later", errorDeletingOutfit:"Failed to delete outfit", errorRenamingOutfit:"Failed to rename outfit", errorWearingOutfit:"Failed to wear outfit", unableToDownloadThumbnail:"Failed to download outfit thumbnail", failedToLoadOutfits:"Failed to load outfits", downloadingImage:"Downloading image...", countNumbersInEnglish:["zero", "one", "two", "three", "four", "five"], nameInputPlaceholder:"Name your outfit", downloadOutfitImageTimeout:2e3, downloadOutfitImageMaxRetries:10, menuOptions:[ {
        label:"Update", name:"Update"
    }

    , {
    label:"Rename", name:"Rename"
}

, {
label:"Download Image", name:"DownloadImage"
}

, {
label:"Delete", name:"Delete"
}

, {
label:"Cancel", name:"Cancel"
}

], outfitErrorCodes: {
    maxOutfits:1, invalidBodyColors:3, invalidOutfitName:4, unwearableAsset:5, internalError:6
}

, renameOutfitsToCostumes:function(n) {
    var i, t; for(i in n.outfits)n.outfits.hasOwnProperty(i)&&(t=n.outfits[i], typeof t=="string" &&(t=t.replace("outfit", "costume").replace("Outfit", "Costume"), n.outfits[i]=t)); angular.forEach(n.tabs, function(n) {
            n.name==="Outfits" ?n.label="Costumes":n.name==="Recent" &&angular.forEach(n.menus, function(n) {
                    n.name==="Outfits" &&(n.label="Costumes")
                })
        })
}
}

, packages: {
    errorWearingPackage:"Failed to wear package"
}

, assets: {
    savedAdvancedAccessories:"Saved accessories", emptyMessage:"You don't have any ", couldNotLoadList:"Failed to load assets list", errorUpdatingItems:"Error while updating worn items", maxAccessories:10
}

, scales: {
    failedToUpdate:"Failed to update scales"
}

, avatarType: {
    failedToUpdate:"Failed to update avatar type", defaultOnPageLoad:"R15", avatarTypes:["R6", "R15"]
}

, bodyColors: {
    failedToUpdate:"Failed to update skin tone", palette:[ {
        brickColorId:364, hexColor:"#5A4C42"
    }

    , {
    brickColorId:217, hexColor:"#7C5C46"
}

, {
brickColorId:359, hexColor:"#AF9483"
}

, {
brickColorId:18, hexColor:"#CC8E69"
}

, {
brickColorId:125, hexColor:"#EAB892"
}

, {
brickColorId:361, hexColor:"#564236"
}

, {
brickColorId:192, hexColor:"#694028"
}

, {
brickColorId:351, hexColor:"#BC9B5D"
}

, {
brickColorId:352, hexColor:"#C7AC78"
}

, {
brickColorId:5, hexColor:"#D7C59A"
}

, {
brickColorId:153, hexColor:"#957977"
}

, {
brickColorId:1007, hexColor:"#A34B4B"
}

, {
brickColorId:101, hexColor:"#DA867A"
}

, {
brickColorId:1025, hexColor:"#FFC9C9"
}

, {
brickColorId:330, hexColor:"#FF98DC"
}

, {
brickColorId:135, hexColor:"#74869D"
}

, {
brickColorId:305, hexColor:"#527CAE"
}

, {
brickColorId:11, hexColor:"#80BBDC"
}

, {
brickColorId:1026, hexColor:"#B1A7FF"
}

, {
brickColorId:321, hexColor:"#A75E9B"
}

, {
brickColorId:107, hexColor:"#008F9C"
}

, {
brickColorId:310, hexColor:"#5B9A4C"
}

, {
brickColorId:317, hexColor:"#7C9C6B"
}

, {
brickColorId:29, hexColor:"#A1C48C"
}

, {
brickColorId:105, hexColor:"#E29B40"
}

, {
brickColorId:24, hexColor:"#F5CD30"
}

, {
brickColorId:334, hexColor:"#F8D96D"
}

, {
brickColorId:199, hexColor:"#635F62"
}

, {
brickColorId:1002, hexColor:"#CDCDCD"
}

, {
brickColorId:1001, hexColor:"#F8F8F8"
}

]
}

, page: {
    idleRefreshTimeInSeconds:10
}

, googleAnalytics: {
    category:"AvatarPage", advancedAccessoriesAction:"AdvancedAccessories", advancedBodyColorsAction:"AdvancedBodyColors", openLabel:"Open", closeLabel:"Close", saveLabel:"Save", saveFailedLabel:"SaveFailed"
}

, defaultClothing: {
    wearClothing:"Default clothing has been applied to your avatar - wear something from your clothing", displayTimeInMilliseconds:5e3
}
});