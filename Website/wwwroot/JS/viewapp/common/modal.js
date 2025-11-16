angular.module("ui.bootstrap").config(["$provide", function(n) {
        n.decorator("modalBackdropDirective", ["$delegate", function(n) {
                return n[0].templateUrl=Roblox.uiBootstrap.modalBackdropTemplateLink, n
            }

            ]), n.decorator("modalWindowDirective", ["$delegate", function(n) {
                return n[0].templateUrl=Roblox.uiBootstrap.modalWindowTemplateLink, n
            }

            ])
    }

    ]).factory("robloxModalService", ["$modal", "$log", function(n, t) {
        var i; return {
            open:function(t, i, r) {
                angular.element("body").addClass("modal-open-noscroll"); var u=n.open({
                    templateUrl:t?t:"myModalContent.html", controller:"ModalInstanceCtrl", resolve: {
                        params:function() {
                            return r
                        }
                    }
                }); return u.result
        }

        , getScope:function() {
            return t.debug(i), i
        }

        , setScope:function(n) {
            i=n
        }
    }
}

]).controller("ModalInstanceCtrl", ["$scope", "$modalInstance", "$log", "robloxModalService", "params", function(n, t, i, r, u) {
        u&&(n.params=u), n.close=function() {
            i.debug("ModalInstanceCtrl close at: " +new Date), t.dismiss("close")
        }

        , n.submit=function() {
            i.debug("Submiting form"), angular.element("body").removeClass("modal-open-noscroll"), r.setScope(n), t.close("close")
        }
    }

    ]);