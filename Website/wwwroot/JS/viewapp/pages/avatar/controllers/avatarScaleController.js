"use strict";

avatar.controller("avatarScaleController", ["$scope", "$log", "$timeout", "$q", "avatarService", "avatarConstants", function(n, t, i, r, u, f) {
        function o(n, t) {
            t.min=n.min*100, t.max=n.max*100
        }

        function s(t) {
            n.scales.height.value=t.scales.height*100, n.scales.width.value=t.scales.width*100, n.scaleHeadEnabled&&(n.scales.head.value=t.scales.head*100)
        }

        n.scales= {
            height: {
                label:"Height", min:0, max:100, value:100, increment:5
            }

            , width: {
                label:"Width", min:0, max:100, value:100, increment:5
            }
        }

        , n.scaleHeadEnabled= !1, n.updateScales=function() {
            t.debug("updateScales"); var i= {
                height:n.scales.height.value/100, width:n.scales.width.value/100
            }

            ; n.scaleHeadEnabled&&(i.head=n.scales.head.value/100), u.setScales(i).then(function() {
                    n.refreshThumbnail()
                }

                , function() {
                    n.systemFeedback.error(f.scales.failedToUpdate)
                })
        }

        ; var h= !1, e=null; n.scales.height.increment=n.avatarDataModel.scaleHeightIncrement*100, n.scales.width.increment=n.avatarDataModel.scaleWidthIncrement*100, n.scales.head= {
            label:"Head", min:0, max:100, value:100, increment:5
        }

        , n.scales.head.increment=n.avatarDataModel.scaleHeadIncrement*100, n.scaleHeadEnabled= !0, n.$on(f.events.avatarRulesLoaded, function(i, r) {
                t.debug("Avatar rules loaded for scale controller"), o(r.scales.height, n.scales.height), o(r.scales.width, n.scales.width), n.scaleHeadEnabled&&o(r.scales.head, n.scales.head), h= !0, e !=null&&(s(e), e=null)

            }), n.$on(f.events.avatarDetailsLoaded, function(n, i) {
                t.debug("Avatar details loaded for scale controller"), h?s(i):e=i
            })
    }

    ]);