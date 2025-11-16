"use strict";

avatar.controller("bodyColorsController", ["$scope", "$log", "$timeout", "$q", "avatarService", "robloxModalService", "avatarConstants", "googleAnalyticsEventsService", "$rootScope", function(n, t, i, r, u, f, e, o, s) {
        function v(t, i) {
            var r=i.brickColorId, u=n.myBodyColors; switch(t.name) {
                case"all":u.headColorId=r, u.torsoColorId=r, u.leftArmColorId=r, u.rightArmColorId=r, u.leftLegColorId=r, u.rightLegColorId=r; break; case"headColorId":u.headColorId=r; break; case"torsoColorId":u.torsoColorId=r; break; case"leftArmColorId":u.leftArmColorId=r; break; case"rightArmColorId":u.rightArmColorId=r; break; case"leftLegColorId":u.leftLegColorId=r; break; case"rightLegColorId":u.rightLegColorId=r
            }

            l()
        }

        function l() {
            n.currentColorId=y(n.myBodyColors)?n.myBodyColors.headColorId:null
        }

        function y(n) {
            var t=n.headColorId; return n.headColorId===t&&n.torsoColorId===t&&n.rightArmColorId===t&&n.leftArmColorId===t&&n.rightLegColorId===t&&n.leftLegColorId===t
        }

        function p(t, i) {
            return i.name==="all" ?n.currentColorId===t:n.myBodyColors[i.name]===t
        }

        function w() {
            // Apply body color changes, then simply reload the avatar thumbnail.
            // No explicit redrawThumbnail API call is needed; we only want to
            // refresh the /thumbnail/user-avatar endpoint the page already uses.
            return u.setBodyColors(n.myBodyColors).then(function() {
                    s.$broadcast(e.events.bodyColorsChanged, n.myBodyColors);
                    n.refreshThumbnail();
                }

                , function() {
                    n.systemFeedback.error(e.bodyColors.failedToUpdate)
                })
        }

        function a() {
            var n=e.googleAnalytics.closeLabel; h&&(n=e.googleAnalytics.saveLabel), o.fireEvent(e.googleAnalytics.category, e.googleAnalytics.advancedBodyColorsAction, n)
        }

        n.myBodyColors= {
            headColorId:0, torsoColorId:0, rightArmColorId:0, leftArmColorId:0, rightLegColorId:0, leftLegColorId:0
        }

        , n.currentColorId=null, n.bodyParts=[ {
            label:"All", name:"all"
        }

        , {
        label:"Head", name:"headColorId"
    }

    , {
    label:"Torso", name:"torsoColorId"
}

, {
label:"Left Arm", name:"leftArmColorId"
}

, {
label:"Right Arm", name:"rightArmColorId"
}

, {
label:"Left Leg", name:"leftLegColorId"
}

, {
label:"Right Leg", name:"rightLegColorId"
}

]; var c=n.bodyParts[0], h= !1; 
        n.selectedBodyPart = c;
        n.colorsPalette = (e && e.bodyColors && e.bodyColors.palette) ? angular.copy(e.bodyColors.palette) : [];
        n.advancedPalette = angular.copy(n.colorsPalette);
        n.isColorSelected=function(t) {
            return t.brickColorId===n.currentColorId;
        }
        n.onColorDotClicked=function(i) {
            // Basic skin tone grid: apply immediately
            var t=n.selectedBodyPart||c; v(t, i); w()
        }

        // Advanced panel: update local state only; save occurs on Done
        n.onAdvancedColorDotClicked=function(i) {
            var t=n.selectedBodyPart||c; v(t, i); h=!0
        }

        n.openAdvancedBodyColors=function() {
            t.debug("advanced body colors"), h= !1; var r="advanced-body-colors-modal", i= {
                colorsPalette:n.advancedPalette,
                bodyParts:n.bodyParts,
                selectedBodyPart:c,
                setSelectedBodyPart:function(bodyPart) {
                    n.selectedBodyPart = bodyPart;
                    i.selectedBodyPart = bodyPart;
                },
                onColorDotClicked:n.onAdvancedColorDotClicked,
                isColorSelected:function(n) {
                    return p(n.brickColorId, i.selectedBodyPart)
                },
                isSaving:!1,
                onApply:function(submit) {
                    if (i.isSaving) {
                        return;
                    }
                    i.isSaving = !0;
                    h=!0; w().then(function() {
                        i.isSaving = !1;
                        submit && submit();
                    }, function() {
                        i.isSaving = !1;
                    });
                }
            }
    ; f.open(r, "", i).then(function() {
            a()
        }

        , function() {
            a()
        }), o.fireEvent(e.googleAnalytics.category, e.googleAnalytics.advancedBodyColorsAction, e.googleAnalytics.openLabel)
}

, n.$on(e.events.avatarRulesLoaded, function(t, i) {
        n.colorsPalette=i.basicBodyColorsPalette, n.advancedPalette=i.bodyColorsPalette

    }), n.$on(e.events.avatarDetailsLoaded, function(t, i) {
        n.myBodyColors=i.bodyColors, l()
    })
}

]);