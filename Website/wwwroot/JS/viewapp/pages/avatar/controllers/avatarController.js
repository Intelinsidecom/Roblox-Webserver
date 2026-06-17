// ~/viewapp/pages/avatar/controllers/avatarController.js
"use strict";
avatar.controller("avatarController", ["$scope", "$log", "$timeout", "$q", "$rootScope", "avatarService", "robloxModalService", "avatarConstants", "avatarUrlConstants", "$anchorScroll", "googleAnalyticsEventsService", function(n, t, i, r, u, f, e, o, s, h, c) {
    function v() {
        var i = f.getAvatar();
        return i.then(function(i) {
            t.debug("Retrieved avatar details"), a = i, tt(a.assets), nt(), n.$broadcast(o.events.avatarDetailsLoaded, a)
        }, function() {}), i
    }

    function ni() {
        var i = f.getRules();
        return i.then(function(i) {
            t.debug("Retrieved avatar rules"), k = i, n.$broadcast(o.events.avatarRulesLoaded, k);
            var r = {
                id: 32,
                name: "Package",
                maxCount: 0
            };
            k.wearableAssetTypes.push(r), angular.forEach(k.wearableAssetTypes, function(n) {
                pt[n.id] = n, wt[n.name] = n
            })
        }, function() {}), i
    }

    function kt(n) {
        return pt[n]
    }

    function rt() {
        return n.tabWithOpenMenu && n.tabWithOpenMenu.active
    }

    function ui(n) {
        var i = "download" + (+new Date).toString(),
            t = $("<iframe />", {
                id: i
            }).appendTo("body");
        t.attr("src", n), t.hide()
    }

    function dt(t, r) {
        if (typeof r == "undefined" && (r = o.outfits.downloadOutfitImageMaxRetries), r <= 0) {
            n.systemFeedback.error(o.outfits.unableToDownloadThumbnail);
            return
        }
        f.getOutfitThumbnailForDownload(t.id).then(function(n) {
            if (n && n.Final) {
                var u = Roblox.Endpoints.generateAbsoluteUrl("/outfits/download") + "?userOutfitId=" + t.id;
                ui(u)
            } else r -= 1, i(function() {
                dt(t, r)
            }, o.outfits.downloadOutfitImageTimeout)
        }, function() {
            n.systemFeedback.error(o.outfits.unableToDownloadThumbnail)
        })
    }

    function fi() {
        var i = "outfit-name-modal",
            r = {
                title: o.outfits.createOutfitTitle,
                description: o.outfits.createOutfitDescription,
                buttonName: o.outfits.createOutfitButton
            };
        e.open(i, "", r).then(function() {
            t.debug("User clicked create outfit");
            var i = e.getScope(),
                r = i.outfitName;
            v().then(function() {
                var t = a.bodyColors,
                    i = y(a.assets);
                f.createOutfit(r, t, i).then(function() {
                    n.$broadcast(o.events.outfitsChanged, null), n.systemFeedback.success(o.outfits.successfulCreate)
                }, function(t) {
                    var i = t && t.errors && t.errors[0] && t.errors[0].code === 1;
                    i ? n.systemFeedback.error(o.outfits.maxNumberOfOutfits) : n.systemFeedback.error(o.outfits.errorCreatingOutfit)
                })
            }, function() {})
        }, function() {})
    }

    function ei(i) {
        var r = "outfit-update-modal",
            u = {};
        e.open(r, "", u).then(function() {
            t.debug("User clicked update outfit");
            var r = e.getScope();
            v().then(function() {
                var r = a.bodyColors,
                    u = y(a.assets),
                    e = {
                        bodyColors: r,
                        assetIds: u
                    };
                f.patchOutfit(i.id, e).then(function() {
                    t.debug("Updated outfit"), n.systemFeedback.success(o.outfits.successfulUpdate), n.addItemThumbnailAndLink(i)
                }, function(t) {
                    var i = t && t.errors && t.errors[0] && t.errors[0].code === 1;
                    i ? n.systemFeedback.error(o.outfits.updateFailedOutfitDeleted) : n.systemFeedback.error(o.outfits.errorUpdatingOutfit)
                })
            }, function() {})
        }, function() {})
    }

    function hi(i) {
        var r = "outfit-delete-modal",
            u = {};
        e.open(r, "", u).then(function() {
            t.debug("User clicked delete outfit"), f.deleteOutfit(i.id).then(function() {
                n.$broadcast(o.events.outfitDeleted, i), n.systemFeedback.success(o.outfits.successfulDelete)
            }, function() {
                n.systemFeedback.error(o.outfits.errorDeletingOutfit)
            })
        }, function() {
            t.debug("There was an error.")
        })
    }

    function ci(i) {
        var r = "outfit-name-modal",
            u = {
                title: o.outfits.renameOutfitTitle,
                description: o.outfits.renameOutfitDescription,
                buttonName: o.outfits.renameOutfitButton
            };
        e.open(r, "", u).then(function() {
            t.debug("User clicked rename outfit");
            var u = e.getScope(),
                r = u.outfitName,
                s = {
                    name: r
                };
            f.patchOutfit(i.id, s).then(function() {
                i.name = r, n.systemFeedback.success(o.outfits.successfulRename)
            }, function() {
                n.systemFeedback.error(o.outfits.errorRenamingOutfit)
            })
        }, function() {})
    }

    function b(n) {
        switch (n) {
            case "Hat":
            case "Hair Accessory":
            case "Face Accessory":
            case "Neck Accessory":
            case "Shoulder Accessory":
            case "Front Accessory":
            case "Back Accessory":
            case "Waist Accessory":
                return !0;
            default:
                return !1
        }
    }

    function li(n) {
        switch (n) {
            case "Climb Animation":
            case "Fall Animation":
            case "Idle Animation":
            case "Jump Animation":
            case "Run Animation":
            case "Swim Animation":
            case "Walk Animation":
            case "Pose Animation":
                return !0;
            default:
                return !1
        }
    }

    function bt() {
        var t = n.selectedMenu;
        n.showAdvancedAccessoriesLink = t && t.assetType && b(t.assetType)
    }

    function vi() {
        var n = "advanced-accessories-double-check-modal",
            t = {};
        return e.open(n, "", t)
    }

    function wi(i, r, u) {
        var f = vi();
        t.debug(f), f.then(function() {
            t.debug("Confirmed double check modal");
            var f = [];
            angular.forEach(r, function(n) {
                n.id !== "" && f.push(n.id)
            }), angular.forEach(i, function(n) {
                f.push(n.id)
            }), ht(f, !0).then(function(i) {
                if (i && i.invalidAssetIds && i.invalidAssetIds.length > 0) {
                    var f = i.invalidAssetIds;
                    t.debug(f), angular.forEach(r, function(n) {
                        t.debug(n);
                        var i = n.id === "" || f.indexOf(parseInt(n.id)) === -1;
                        n.valid = i
                    }), c.fireEvent(o.googleAnalytics.category, o.googleAnalytics.advancedAccessoriesAction, o.googleAnalytics.saveFailedLabel)
                } else n.systemFeedback.success(o.assets.savedAdvancedAccessories), t.debug("Successfully saved advanced assets"), u(), c.fireEvent(o.googleAnalytics.category, o.googleAnalytics.advancedAccessoriesAction, o.googleAnalytics.saveLabel)
            }, function() {
                n.systemFeedback.error(o.assets.errorUpdatingItems)
            })
        }, function() {
            t.debug("Cancelled double check modal")
        })
    }

    function yi(t) {
        var r = [],
            u = 3,
            i = 0;
        for (angular.forEach(t, function(t) {
                if (!(i >= u) && t.assetType.name === "Hat") {
                    var f = {};
                    f.type = "Asset", angular.extend(f, t), n.addItemThumbnailAndLink(f), i++, r.push(f)
                }
            }); i < u;) i++, r.push({
            empty: !0
        });
        n.hatSlots = r
    }

    function y(n) {
        var t = [];
        return angular.forEach(n, function(n) {
            t.push(n.id)
        }), t
    }

    function gt(n) {
        var i = y(n),
            t = {};
        return angular.forEach(i, function(n) {
            t[n] = !0
        }), t
    }

    function pi(n) {
        var t = kt(n);
        return t ? t.maxNumber : 1
    }

    function tt(n) {
        l = n, g = gt(n)
    }

    function nt() {
        var t = y(l);
        n.$broadcast(o.events.wornAssetsChanged, t), yi(l)
    }

    function ct(n) {
        var t = o.outfits.countNumbersInEnglish;
        return n > 5 ? n.toString() : t[n]
    }

    function ai(t) {
        f.wearOutfit(t.id).then(function(t) {
            var i, r, u;
            n.refreshThumbnail(), v(), i = t && t.invalidAssetIds && t.invalidAssetIds.length > 0, i ? (r = t.invalidAssetIds.length, u = "You no longer own " + ct(r) + " of the items in this outfit.", n.systemFeedback.error(u)) : n.systemFeedback.success(o.outfits.successfulWear)
        }, function() {
            n.systemFeedback.error(o.outfits.errorWearingOutfit)
        })
    }

    function ht(i, r) {
        var u = f.setWearingAssets(i);
        return u.then(function() {
            t.debug("Success with set-wearing-assets"), w ? w = !1 : n.refreshThumbnail(), r && v()
        }, function() {
            t.debug("Error with set-wearing-assets")
        }), u
    }

    function ot(t) {
        var r = l;
        tt(t), nt();
        var u = y(l),
            f = !1,
            i = ht(u, f);
        return i.then(function() {}, function() {
            n.systemFeedback.error(o.assets.errorUpdatingItems), tt(r), nt()
        }), i
    }

    function et(n) {
        return li(n.assetType.name) ? !0 : !1
    }

    function si(t) {
        f.wearAsset(t.id).then(function(t) {
            var i, r, u;
            n.refreshThumbnail(), v(), i = t && t.invalidAssetIds && t.invalidAssetIds.length > 0, i && (r = t.invalidAssetIds.length, u = "You no longer own " + ct(r) + " of the items in this outfit.", n.systemFeedback.error(u))
        }, function() {
            n.systemFeedback.error(o.packages.errorWearingPackage)
        })
    }

    function oi(n) {
        function e(n) {
            return !u && b(n.name) ? "Accessory" : n.name
        }

        function f(n) {
            var s = n.assetType,
                h = !u && b(s.name) ? o.assets.maxAccessories : pi(s.id),
                f = e(n.assetType);
            i[f] = typeof i[f] == "undefined" ? 0 : i[f], i[f] < h ? (i[f] += 1, r.push(n)) : t.debug("Removed asset " + n.name + " because it exceeded wearing limits")
        }
        var r = [],
            i = {},
            u = b(n.assetType.name);
        return f(n), angular.forEach(l, function(n) {
            f(n)
        }), et(n) && (w = !0), ot(r)
    }

    function at(n) {
        var t = [];
        return angular.forEach(l, function(i) {
            i.id !== n.id && t.push(i)
        }), et(n) && (w = !0), ot(t)
    }

    function ri(n) {
        return n.torsoColorId === n.leftLegColorId && n.leftLegColorId === n.rightLegColorId
    }

    function ii(n) {
        for (var i, t = 0; t < l.length; t++)
            if (i = l[t], i.assetType.name === n) return !0;
        return !1
    }

    function ti(n) {
        if (n === null || a === null || !ri(n)) return !1;
        var t = ii("Pants");
        return t ? !1 : !0
    }

    function lt() {
        d !== null && (i.cancel(d), d = null)
    }

    function ut() {
        if (n.avatarDataModel !== null && n.avatarDataModel.enableDefaultClothingMessage) {
            if (vt && !n.avatarDataModel.showDefaultClothingMessageOnPageLoad) {
                vt = !1;
                return
            }
            var t = ti(it),
                r = st !== t;
            st = t, t ? r && (n.defaultClothingOverlayVisible = !0, lt(), d = i(function() {
                n.defaultClothingOverlayVisible = !1
            }, o.defaultClothing.displayTimeInMilliseconds)) : (n.defaultClothingOverlayVisible = !1, lt())
        }
    }
    var l, g, w, ft, p, yt;
    t.debug("avatarController starting"), n.pageLoaded = !1, n.avatarDataModel = null, n.metaDataDeferred = r.defer(), n.metaDataDeferred.promise.then(function(t) {
        n.avatarDataModel = t, n.$broadcast(o.events.metaDataLoaded, t)
    }), n.systemFeedback = {
        error: function(n) {
            Roblox.BootstrapWidgets.ToggleSystemMessage($(".alert-warning"), 100, 2e3, n)
        },
        loading: function(n) {
            Roblox.BootstrapWidgets.ToggleSystemMessage($(".alert-loading"), 100, 2e3, n)
        },
        success: function(n) {
            Roblox.BootstrapWidgets.ToggleSystemMessage($(".alert-success"), 100, 2e3, n)
        }
    }, n.redrawFloodchecked = !1, n.refreshThumbnail = function() {
        Roblox.ThumbnailView.reloadThumbnail()
    }, n.redrawThumbnail = function() {
        f.redrawThumbnail().then(function() {
            n.refreshThumbnail()
        }, function(r) {
            var u = r && r.errors && r.errors[0] && r.errors[0].code === 1;
            u ? (n.systemFeedback.error(o.thumbnail.redrawFloodchecked), t.debug("Disabled redraw button"), n.redrawFloodchecked = !0, i(function() {
                t.debug("Re-enabled redraw"), n.redrawFloodchecked = !1
            }, o.thumbnail.waitForThumbnailRegenerationInSeconds * 1e3)) : n.systemFeedback.error(o.thumbnail.redrawThumbnailFailed)
        })
    }, n.scaleEnabled = !1;
    var k = null,
        a = null,
        pt = {},
        wt = {};
    n.getAssetTypeName = function(n) {
        var t = kt(n);
        return t && t.name ? t.name : null
    }, n.getAssetTypeByName = function(n) {
        return wt[n]
    }, n.loadAvatarPage = function() {
        var i = ni(),
            u = v();
        r.all([i, u]).then(function() {
            t.debug("Retrieved avatar rules and details"), n.pageLoaded = !0
        }, function() {
            n.systemFeedback.error("Unable to load avatar page")
        })
    }, n.tabs = o.tabs, n.selectedTab = null, n.selectedMenu = null, n.tabWithOpenMenu = null, n.isMenuOpen = !1, n.mouseLeftTabMenu = function() {
        rt() || (n.tabWithOpenMenu = null, n.isMenuOpen = !1)
    }, n.onTabBlur = function() {
        n.tabWithOpenMenu = null, n.isMenuOpen = !1
    }, n.onTabClick = function(t) {
        if (t.noMenus) n.onMenuClick(t, null);
        else {
            if (rt()) {
                if (n.tabWithOpenMenu === t) {
                    n.tabWithOpenMenu = null, n.isMenuOpen = !1, t.active = !1;
                    return
                }
                n.tabWithOpenMenu.active = !1, n.tabWithOpenMenu = t, n.isMenuOpen = !0, t.active = !0;
                return
            }
            n.isMenuOpen = !0, n.tabWithOpenMenu = t, t.active = !0
        }
    }, n.onTabHover = function(t) {
        rt() || (n.tabWithOpenMenu = t, n.isMenuOpen = !0)
    }, n.scrollToTop = function() {
        var n = angular.element("#wrap").hasClass("pinned");
        n && h("tab-content-top")
    }, n.onMenuClick = function(t, i) {
        n.isMenuOpen = !1, n.tabWithOpenMenu = null, n.selectedTab = t, n.selectedMenu = i, n.scrollToTop(), bt(), n.$broadcast(o.events.menuClicked, {
            tab: t,
            menu: i
        }), c.viewVirtual(s.www.avatar + "/" + t.name + (i == null ? "" : "/" + i.name))
    }, n.openOutfitMenu = function(n) {
        n.active = !0
    }, n.closeOutfitMenu = function(n) {
        n.active && (n.active = !1)
    }, n.onItemMenuButtonClicked = function(n, t, i) {
        t.active = !1;
        switch (i.name) {
            case "Delete":
                hi(t);
                break;
            case "Update":
                ei(t);
                break;
            case "Rename":
                ci(t);
                break;
            case "DownloadImage":
                dt(t)
        }
    }, n.outfitMenuOptions = o.outfits.menuOptions, n.createOutfitClicked = function() {
        fi()
    }, n.showAdvancedAccessoriesLink = !0, n.openAdvancedAccessories = function() {
        function a(n) {
            return f.exec(n) !== null
        }

        function v(n) {
            var t = u.exec(n);
            return t !== null && t[1] ? t[1] : null
        }
        var h = "advanced-accessories-modal",
            n = [],
            r = [],
            i, u, f, s;
        for (angular.forEach(l, function(t) {
                b(t.assetType.name) ? n.push({
                    id: t.id,
                    valid: !0
                }) : r.push(t)
            }), i = 0; i < 10; i++) typeof n[i] == "undefined" && (n[i] = {
            id: "",
            valid: !0
        });
        u = /catalog\/(\d+)/i, f = /^\d+$/i, s = {
            advancedAccessorySlots: n,
            onChange: function(n) {
                var t = v(n.id);
                t !== null && (n.id = t), n.valid = n.id === "" || a(n.id)
            },
            onSaveClick: function(i) {
                t.debug("submitFunc", i), wi(r, n, i)
            }
        }, e.open(h, "", s).then(function() {
            t.debug("User clicked save assets in advanced view")
        }, function() {
            t.debug("User cancelled out of assets advanced view"), c.fireEvent(o.googleAnalytics.category, o.googleAnalytics.advancedAccessoriesAction, o.googleAnalytics.closeLabel)
        }), c.fireEvent(o.googleAnalytics.category, o.googleAnalytics.advancedAccessoriesAction, o.googleAnalytics.openLabel)
    }, n.onItemClicked = function(n, t) {
        if (t.preventDefault(), n.type === "Outfit") ai(n);
        else if (n.type === "Asset") {
            if (n.assetType.name === "Package") {
                si(n);
                return
            }
            n.selected ? (n.selected = !1, at(n)) : (n.selected = !0, oi(n))
        }
    }, n.addItemThumbnailAndLink = function(t) {
        t.thumbnail = {
            Final: !1,
            Url: n.avatarDataModel.loadingThumbnailUrl,
            UsePlaceholder: !0
        };
        switch (t.type) {
            case "Asset":
                t.thumbnail.RetryUrl = s.www.assetThumbnail + t.id, t.link = Roblox.Endpoints.getCatalogItemUrl(t.id, t.name);
                break;
            case "Outfit":
                t.thumbnail.RetryUrl = s.www.outfitThumbnail + t.id
        }
    }, n.hatSlots = [], n.onHatSlotClicked = function(n) {
        at(n)
    }, l = [], g = [], n.updateItemSelected = function(n) {
        n.selected = n.type === "Asset" && g[n.id] === !0
    }, w = !1, n.defaultClothingOverlayVisible = !1, n.defaultClothingMessage = o.defaultClothing.wearClothing;
    var st = null,
        d = null,
        it = null,
        vt = !0;
    n.$on(o.events.avatarDetailsLoaded, function(n, t) {
        it = t.bodyColors, ut()
    }), u.$on(o.events.bodyColorsChanged, function(n, t) {
        it = t, ut()
    }), ft = !0, n.$on(o.events.wornAssetsChanged, function() {
        if (ft) {
            ft = !1;
            return
        }
        ut()
    }), p = n.tabs[0], yt = p.menus ? p.menus[0] : p.rows[0].menus[0];
    n.onMenuClick(p, yt);
    n.loadAvatarPage(), bt(), n.pageFocused = function(i) {
        t.debug("Page didn't have focus for ", i), i > o.page.idleRefreshTimeInSeconds && (v(), n.refreshThumbnail())
    }
}]);