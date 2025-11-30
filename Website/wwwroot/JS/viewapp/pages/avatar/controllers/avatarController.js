"use strict";

avatar.controller("avatarController", ["$scope", "$log", "$timeout", "$q", "$rootScope", "avatarService", "robloxModalService", "avatarConstants", "avatarUrlConstants", "$anchorScroll", "googleAnalyticsEventsService", "$location", function(n, t, i, r, u, f, e, o, s, h, c, lo) {
        function y() {
            var i=f.getAvatar(); return i.then(function(i) {
                    t.debug("Retrieved avatar details"), a=i, tt(a.assets), nt(), n.$broadcast(o.events.avatarDetailsLoaded, a)
                }

                , function() {}), i
        }

        function ri() {
            var i=f.getRules(); return i.then(function(i) {
                    t.debug("Retrieved avatar rules"), v=i, n.$broadcast(o.events.avatarRulesLoaded, v); var r= {
                        id:32, name:"Package", maxCount:0
                    }

                    ; v.wearableAssetTypes.push(r), angular.forEach(v.wearableAssetTypes, function(n) {
                            wt[n.id]=n, bt[n.name]=n
                        })
                }

                , function() {}), i
        }

        function gt(n) {
            return wt[n]
        }

        function it() {
            return n.tabWithOpenMenu&&n.tabWithOpenMenu.active
        }

        function oi(n) {
            var i="download" +(+new Date).toString(), t=$("<iframe />", {
                id:i
            }).appendTo("body"); t.attr("src", n), t.hide()
    }

    function ni(t, r) {
        if(typeof r=="undefined" &&(r=o.outfits.downloadOutfitImageMaxRetries), r<=0) {
            n.systemFeedback.error(o.outfits.unableToDownloadThumbnail); return
        }

        f.getOutfitThumbnailForDownload(t.id).then(function(n) {
                if(n&&n.Final) {
                    var u=Roblox.Endpoints.generateAbsoluteUrl("/outfits/download")+"?userOutfitId=" +t.id; oi(u)
                }

                else r-=1, i(function() {
                        ni(t, r)
                    }

                    , o.outfits.downloadOutfitImageTimeout)
            }

            , function() {
                n.systemFeedback.error(o.outfits.unableToDownloadThumbnail)
            })
    }

    function ti(n) {
        var i= !1, t; if(n&&n.errors)for(t=0; t<n.errors.length; t++)n.errors[t].code===o.outfits.outfitErrorCodes.invalidOutfitName&&(i= !0); return i
    }

    function si() {
        var i="outfit-name-modal", r= {
            title:o.outfits.createOutfitTitle, description:o.outfits.createOutfitDescription, buttonName:o.outfits.createOutfitButton, nameInputPlaceholder:o.outfits.nameInputPlaceholder
        }

        ; e.open(i, "", r).then(function() {
                t.debug("User clicked create outfit"); var i=e.getScope(), r=i.outfitName; y().then(function() {
                        var t=a.bodyColors, i=p(a.assets); f.createOutfit(r, t, i).then(function() {
                                n.$broadcast(o.events.outfitsChanged, null), n.systemFeedback.success(o.outfits.successfulCreate)
                            }

                            , function(t) {
                                var i, r; n.$broadcast(o.events.outfitsChanged, null), i=t&&t.errors&&t.errors[0]&&t.errors[0].code===o.outfits.outfitErrorCodes.maxOutfits, i?n.systemFeedback.error(o.outfits.maxNumberOfOutfits):(r=ti(t), r?n.systemFeedback.error(o.outfits.invalidOutfitName):n.systemFeedback.error(o.outfits.errorCreatingOutfit))
                            })
                    }

                    , function() {})
            }

            , function() {})
    }

    function hi(i) {
        var r="outfit-update-modal", u= {}

        ; e.open(r, "", u).then(function() {
                t.debug("User clicked update outfit"); var r=e.getScope(); y().then(function() {
                        var r=a.bodyColors, u=p(a.assets), e= {
                            bodyColors:r, assetIds:u
                        }

                        ; f.patchOutfit(i.id, e).then(function() {
                                t.debug("Updated outfit"), n.systemFeedback.success(o.outfits.successfulUpdate), n.addItemThumbnailAndLink(i)
                            }

                            , function(t) {
                                var i=t&&t.errors&&t.errors[0]&&t.errors[0].code===1; i?n.systemFeedback.error(o.outfits.updateFailedOutfitDeleted):n.systemFeedback.error(o.outfits.errorUpdatingOutfit)
                            })
                    }

                    , function() {})
            }

            , function() {})
    }

    function ci(i) {
        var r="outfit-delete-modal", u= {}

        ; e.open(r, "", u).then(function() {
                t.debug("User clicked delete outfit"), f.deleteOutfit(i.id).then(function() {
                        n.$broadcast(o.events.outfitDeleted, i), n.systemFeedback.success(o.outfits.successfulDelete)
                    }

                    , function() {
                        n.systemFeedback.error(o.outfits.errorDeletingOutfit)
                    })
            }

            , function() {
                t.debug("There was an error.")
            })
    }

    function vi(i) {
        var r="outfit-name-modal", u= {
            title:o.outfits.renameOutfitTitle, description:o.outfits.renameOutfitDescription, buttonName:o.outfits.renameOutfitButton, nameInputPlaceholder:o.outfits.nameInputPlaceholder
        }

        ; e.open(r, "", u).then(function() {
                t.debug("User clicked rename outfit"); var u=e.getScope(), r=u.outfitName, s= {
                    name:r
                }

                ; f.patchOutfit(i.id, s).then(function() {
                        i.name=r, n.systemFeedback.success(o.outfits.successfulRename)
                    }

                    , function(t) {
                        var i=ti(t); i?n.systemFeedback.error(o.outfits.invalidOutfitName):n.systemFeedback.error(o.outfits.errorRenamingOutfit)
                    })
            }

            , function() {})
    }

    function k(n) {
        switch(n) {
            case"Hat":case"Hair Accessory":case"Face Accessory":case"Neck Accessory":case"Shoulder Accessory":case"Front Accessory":case"Back Accessory":case"Waist Accessory":return !0; default:return !1
        }
    }

    function dt(n) {
        switch(n.assetType.name) {
            case"Climb Animation":case"Fall Animation":case"Jump Animation":case"Run Animation":case"Swim Animation":case"Walk Animation":return !1; default:return !0
        }
    }

    function kt() {
        var t=n.selectedMenu; n.showAdvancedAccessoriesLink=t&&t.assetType&&k(t.assetType)
    }

    function pi() {
        var n="advanced-accessories-double-check-modal", t= {}

        ; return e.open(n, "", t)
    }

    function ki(i, r, u) {
        var f=pi(); t.debug(f), f.then(function() {
                t.debug("Confirmed double check modal"); var f=[]; angular.forEach(r, function(n) {
                        n.id !=="" &&f.push(n.id)

                    }), angular.forEach(i, function(n) {
                        f.push(n.id)

                    }), ct(f, !0).then(function(i) {
                        if(i&&i.invalidAssetIds&&i.invalidAssetIds.length>0) {
                            var f=i.invalidAssetIds; t.debug(f), angular.forEach(r, function(n) {
                                    t.debug(n); var i=n.id==="" ||f.indexOf(parseInt(n.id))===-1; n.valid=i
                                }), c.fireEvent(o.googleAnalytics.category, o.googleAnalytics.advancedAccessoriesAction, o.googleAnalytics.saveFailedLabel)
                        }

                        else n.systemFeedback.success(o.assets.savedAdvancedAccessories), t.debug("Successfully saved advanced assets"), u(), c.fireEvent(o.googleAnalytics.category, o.googleAnalytics.advancedAccessoriesAction, o.googleAnalytics.saveLabel)
                    }

                    , function() {
                        n.systemFeedback.error(o.assets.errorUpdatingItems)
                    })
            }

            , function() {
                t.debug("Cancelled double check modal")
            })
    }

    function wi(t) {
        var r=[], u=3, i=0; for(angular.forEach(t, function(t) {
                    if( !(i>=u)&&t.assetType.name==="Hat") {
                        var f= {}

                        ; f.type="Asset", angular.extend(f, t), n.addItemThumbnailAndLink(f), i++, r.push(f)
                    }

                }); i<u; )i++, r.push({
            empty: !0
        }); n.hatSlots=r
}

function p(n) {
    var t=[]; return angular.forEach(n, function(n) {
            t.push(n.id)
        }), t
}

function ii(n) {
    var i=p(n), t= {}

    ; return angular.forEach(i, function(n) {
            t[n]= !0
        }), t
}

function bi(n) {
    var t=gt(n); return t?t.maxNumber:1
}

function tt(n) {
    l=n, g=ii(n)
}

function nt() {
    var t=p(l); n.$broadcast(o.events.wornAssetsChanged, t), wi(l)
}

function at(n) {
    var t=o.outfits.countNumbersInEnglish; return n>5?n.toString():t[n]
}

function yi(t) {
    f.wearOutfit(t.id).then(function(t) {
            var i, r, u; n.refreshThumbnail(), y(), i=t&&t.invalidAssetIds&&t.invalidAssetIds.length>0, i?(r=t.invalidAssetIds.length, u="You no longer own " +at(r)+" of the items in this outfit.", n.systemFeedback.error(u)):n.systemFeedback.success(o.outfits.successfulWear)
        }

        , function() {
            n.systemFeedback.error(o.outfits.errorWearingOutfit)
        })
}

function ct(i, r) {
    var u=f.setWearingAssets(i); return u.then(function() {
            t.debug("Success with set-wearing-assets"), b?n.refreshThumbnail():b= !0, r&&y()
        }

        , function() {
            t.debug("Error with set-wearing-assets")
        }), u
}

function st(t) {
    var r=l; tt(t), nt(); var u=p(l), f= !1, i=ct(u, f); return i.then(function() {}

        , function() {
            n.systemFeedback.error(o.assets.errorUpdatingItems), tt(r), nt()
        }), i
}

function ai(t) {
    f.wearAsset(t.id).then(function(t) {
            var i, r, u; n.refreshThumbnail(), y(), i=t&&t.invalidAssetIds&&t.invalidAssetIds.length>0, i&&(r=t.invalidAssetIds.length, u="You no longer own " +at(r)+" of the items in this outfit.", n.systemFeedback.error(u))
        }

        , function() {
            n.systemFeedback.error(o.packages.errorWearingPackage)
        })
}

function li(n) {
    function e(n) {
        return !u&&k(n.name)?"Accessory":n.name
    }

    function f(n) {
        var s=n.assetType, h= !u&&k(s.name)?o.assets.maxAccessories:bi(s.id), f=e(n.assetType); i[f]=typeof i[f]=="undefined" ?0:i[f], i[f]<h?(i[f]+=1, r.push(n)):t.debug("Removed asset "+n.name+" because it exceeded wearing limits")
    }

    var r=[], i= {}

    , u=k(n.assetType.name); var existing=l; if(n.assetType&&n.assetType.name==="T-Shirt") {
        existing=[];
        angular.forEach(l, function(t) {
            if(!t.assetType||t.assetType.name!=="T-Shirt"||t.id===n.id) existing.push(t)
        })
    }

    f(n);
    angular.forEach(existing, function(t) {
            f(t)
        });
    b=dt(n); return st(r)
}

function ot(n) {
    var t=[]; return angular.forEach(l, function(i) {
            i.id !==n.id&&t.push(i)
        }), b=dt(n), st(t);
}

function lt(n) {
    for(var r="#000000", u, i=0; i<v.bodyColorsPalette.length; i++)
        u=v.bodyColorsPalette[i], u.brickColorId===n&&(r=u.hexColor);
    var hex=r||"#000000";
    if(hex.charAt(0)==="#") hex=hex.slice(1);
    hex=hex.trim();
    if(hex.length!==6) throw new Error("Unable to parse bodyColor with ID="+n+" and retrieved hex="+r);
    var rStr=hex.substring(0,2), gStr=hex.substring(2,4), bStr=hex.substring(4,6);
    var R=parseInt(rStr,16), G=parseInt(gStr,16), B=parseInt(bStr,16);
    if(isNaN(R)||isNaN(G)||isNaN(B)) throw new Error("Unable to parse bodyColor with ID="+n+" and retrieved hex="+r);
    return { R:R, G:G, B:B };
}

function yt(n, t) {
    var i=lt(n), r=lt(t), u=Roblox.AvatarBodyColorDifference.DeltaE(i, r); return u<v.minimumDeltaEBodyColorDifference
}

function ei(t) {
    var i=t.torsoColorId===t.leftLegColorId&&t.leftLegColorId===t.rightLegColorId; return n.avatarDataModel.defaultClothingForSimilarColorsEnabled?i||yt(t.torsoColorId, t.rightLegColorId)||yt(t.torsoColorId, t.leftLegColorId):i
}

function fi(n) {
    for(var i, t=0; t<l.length; t++)if(i=l[t], i.assetType.name===n)return !0; return !1
}

function ui(n) {
    if(n===null||a===null|| !ei(n))return !1; var t=fi("Pants"); return t? !1: !0
}

function vt() {
    d !==null&&(i.cancel(d), d=null)
}

function rt() {
    if(n.avatarDataModel !==null&&n.avatarDataModel.enableDefaultClothingMessage) {
        if(ht&& !n.avatarDataModel.showDefaultClothingMessageOnPageLoad) {
            ht= !1; return
        }

        var t=ui(ft), r=et !==t; et=t, t?r&&(n.defaultClothingOverlayVisible= !0, vt(), d=i(function() {
                    n.defaultClothingOverlayVisible= !1
                }

                , o.defaultClothing.displayTimeInMilliseconds)):(n.defaultClothingOverlayVisible= !1, vt())
    }
}

var l, g, b, ut, w, pt; t.debug("avatarController starting"), n.pageLoaded= !1, n.avatarDataModel=Roblox.AvatarData, n.systemFeedback= {
    error:function(n) {
        Roblox.BootstrapWidgets.ToggleSystemMessage($(".alert-warning"), 100, 2e3, n)
    }

    , loading:function(n) {
        Roblox.BootstrapWidgets.ToggleSystemMessage($(".alert-loading"), 100, 2e3, n)
    }

    , success:function(n) {
        Roblox.BootstrapWidgets.ToggleSystemMessage($(".alert-success"), 100, 2e3, n)
    }
}

, n.redrawFloodchecked= !1, n.refreshThumbnail=function() {
	// For any avatar update (assets, body colors, outfits), emulate the same
	// behavior as the manual Refetch button: first clear the existing image,
	// then show a spinner, then reload the thumbnail markup.
	try {
		var nodes = document.querySelectorAll('.thumbnail-holder .thumbnail-span img, .thumbnail-holder .thumbnail-span canvas');
		for (var i = 0; i < nodes.length; i++) {
			var node = nodes[i];
			if (node && node.parentNode) {
				node.parentNode.removeChild(node);
			}
		}
	} catch(e) {}

	try {
		if (typeof showAvatarSpinner === "function")
			showAvatarSpinner();
		else if (window.Roblox && Roblox.ThumbnailSpinner) {
			var r = $(".avatar-thumbnail .thumbnail-span");
			r.length && Roblox.ThumbnailSpinner.show(r);
		}
	} catch(e) {}
	Roblox.ThumbnailView.reloadThumbnail()
}

, n.redrawThumbnail=function() {
    try {
        if (window.Roblox && Roblox.ThumbnailSpinner) {
            var r = $(".avatar-thumbnail .thumbnail-span");
            r.length && Roblox.ThumbnailSpinner.show(r);
        }
    } catch(e) {}

    f.redrawThumbnail().then(function() {
            n.refreshThumbnail()
        }

        , function(r) {
            var u=r&&r.errors&&r.errors[0]&&r.errors[0].code===1; u?(n.systemFeedback.error(o.thumbnail.redrawFloodchecked), t.debug("Disabled redraw button"), n.redrawFloodchecked= !0, i(function() {
                        t.debug("Re-enabled redraw"), n.redrawFloodchecked= !1
                    }

                    , o.thumbnail.waitForThumbnailRegenerationInSeconds*1e3)):n.systemFeedback.error(o.thumbnail.redrawThumbnailFailed)
        })
}

, n.scaleEnabled= !1; var v=null, a=null, wt= {}

, bt= {}

; n.getAssetTypeName=function(n) {
    var t=gt(n); return t&&t.name?t.name:null
}

, n.getAssetTypeByName=function(n) {
    return bt[n]
}

, n.loadAvatarPage=function() {
    var i=ri(), u=y(); r.all([i, u]).then(function() {
            t.debug("Retrieved avatar rules and details"), n.pageLoaded= !0
        }

        , function() {
            n.systemFeedback.error("Unable to load avatar page")
        })
}

, n.tabs=o.tabs, n.selectedTab=null, n.selectedMenu=null, n.tabWithOpenMenu=null, n.isMenuOpen= !1, n.mouseLeftTabMenu=function() {
    it()||(n.tabWithOpenMenu=null, n.isMenuOpen= !1)
}

, n.onTabBlur=function() {
    n.tabWithOpenMenu=null, n.isMenuOpen= !1
}

, n.onTabClick=function(t) {
    if(t.noMenus)n.onMenuClick(t, null); else {
        if(it()) {
            if(n.tabWithOpenMenu===t) {
                n.tabWithOpenMenu=null, n.isMenuOpen= !1, t.active= !1; return
            }

            n.tabWithOpenMenu.active= !1, n.tabWithOpenMenu=t, n.isMenuOpen= !0, t.active= !0; return
        }

        n.isMenuOpen= !0, n.tabWithOpenMenu=t, t.active= !0
    }
}

, n.onTabHover=function(t) {
    it()||(n.tabWithOpenMenu=t, n.isMenuOpen= !0)
}

, n.scrollToTop=function() {
    var pinned=angular.element("#wrap").hasClass("pinned");
    if(!pinned) return;
    try {
        if(document.getElementById("tab-content-top")) {
            if(window && typeof window.scrollTo === "function") {
                window.scrollTo(0, 0);
            }
            return;
        }
    } catch(e) {
        // no-op
    }
}

, n.onMenuClick=function(t, i) {
    n.isMenuOpen= !1, n.tabWithOpenMenu=null, n.selectedTab=t, n.selectedMenu=i, n.scrollToTop(), kt(), n.$broadcast(o.events.menuClicked, {
        tab:t, menu:i
    }), c.viewVirtual(s.www.avatar+"/" +t.name+(i==null?"":"/" +i.name))
}

, n.openOutfitMenu=function(n) {
    n.active= !0
}

, n.closeOutfitMenu=function(n) {
    n.active&&(n.active= !1)
}

, n.onItemMenuButtonClicked=function(n, t, i) {
    t.active= !1; switch(i.name) {
        case"Delete":ci(t); break; case"Update":hi(t); break; case"Rename":vi(t); break; case"DownloadImage":ni(t)
    }
}

, n.outfitMenuOptions=o.outfits.menuOptions, n.createOutfitClicked=function() {
    si()
}

, n.showAdvancedAccessoriesLink= !0, n.openAdvancedAccessories=function() {
    function a(n) {
        return f.exec(n) !==null
    }

    function v(n) {
        var t=u.exec(n); return t !==null&&t[1]?t[1]:null
    }

    var h="advanced-accessories-modal", n=[], r=[], i, u, f, s; for(angular.forEach(l, function(t) {
                k(t.assetType.name)?n.push({
                    id:t.id, valid: !0
                }):r.push(t)

        }), i=0; i<10; i++)typeof n[i]=="undefined" &&(n[i]= {
        id:"", valid: !0

    }); u=/catalog\/(\d+)/i, f=/^\d+$/i, s= {
    advancedAccessorySlots:n, onChange:function(n) {
        var t=v(n.id); t !==null&&(n.id=t), n.valid=n.id==="" ||a(n.id)
    }

    , onSaveClick:function(i) {
        t.debug("submitFunc", i), ki(r, n, i)
    }
}

, e.open(h, "", s).then(function() {
        t.debug("User clicked save assets in advanced view")
    }

    , function() {
        t.debug("User cancelled out of assets advanced view"), c.fireEvent(o.googleAnalytics.category, o.googleAnalytics.advancedAccessoriesAction, o.googleAnalytics.closeLabel)
    }), c.fireEvent(o.googleAnalytics.category, o.googleAnalytics.advancedAccessoriesAction, o.googleAnalytics.openLabel)
}

, n.onItemClicked=function(n, t) {
    if(t.preventDefault(), n.type==="Outfit")yi(n); else if(n.type==="Asset") {
        if(n.assetType.name==="Package") {
            ai(n); return
        }

        n.selected?(n.selected= !1, ot(n)):(n.selected= !0, li(n))
    }
}

, n.addItemThumbnailAndLink=function(n) {
    n.thumbnail= {
        Final: !1, Url:""
    }

    ; switch(n.type) {
        case"Asset":n.thumbnail.RetryUrl=s.www.assetThumbnail+n.id, n.link=Roblox.Endpoints.getCatalogItemUrl(n.id,n.name); break; case"Outfit":n.thumbnail.RetryUrl=s.www.outfitThumbnail+n.id
    }
}

, n.hatSlots=[], n.onHatSlotClicked=function(n) {
    ot(n)
}

, l=[], g=[], n.updateItemSelected=function(n) {
    n.selected=n.type==="Asset" &&g[n.id]=== !0
}

, b= !0, n.defaultClothingOverlayVisible= !1, n.defaultClothingMessage=o.defaultClothing.wearClothing; var et=null, d=null, ft=null, ht= !0; n.$on(o.events.avatarDetailsLoaded, function(n, t) {
        ft=t.bodyColors, rt()

    }), u.$on(o.events.bodyColorsChanged, function(n, t) {
        ft=t, rt()

    }), ut= !0, n.$on(o.events.wornAssetsChanged, function() {
        if(ut) {
            ut= !1; return
        }

        rt()

    }), w=n.tabs[0], pt=w.menus?w.menus[0]:w.rows[0].menus[0]; n.onMenuClick(w, pt); n.loadAvatarPage(), kt(), n.pageFocused=function(i) {
    t.debug("Page didn't have focus for ", i), i>o.page.idleRefreshTimeInSeconds&&(y(), n.refreshThumbnail())
}
}

]);