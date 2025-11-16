angular.module("ui.bootstrap").config(["$provide", function(n) {
        n.decorator("tabsetDirective", ["$delegate", function(n) {
                return n[0].templateUrl=Roblox.uiBootstrap.tabsetTemplateLink, n
            }

            ]), n.decorator("tabDirective", ["$delegate", function(n) {
                return n[0].templateUrl=Roblox.uiBootstrap.tabTemplateLink, n[0].scope.subtitle="@", n
            }

            ]), n.decorator("pagerDirective", ["$delegate", function(n) {
                return n[0].templateUrl=Roblox.uiBootstrap.pagerTemplateLink, n[0].scope.currentPage="=", n[0].scope.numPages="=", n
            }

            ]), n.decorator("paginationDirective", ["$delegate", function(n) {
                return n[0].templateUrl=Roblox.uiBootstrap.paginationTemplateLink, n[0].scope.currentPage="=", n[0].scope.numPages="=", n[0].scope.maxSize="=", n
            }

            ]), n.decorator("accordionDirective", ["$delegate", function(n) {
                return n[0].templateUrl=Roblox.uiBootstrap.accordionTemplateLink, n
            }

            ]), n.decorator("accordionGroupDirective", ["$delegate", function(n) {
                return n[0].templateUrl=Roblox.uiBootstrap.accordionGroupTemplateLink, n
            }

            ]), n.decorator("tooltipPopupDirective", ["$delegate", function(n) {
                return n[0].templateUrl=Roblox.uiBootstrap.tooltipPopupTemplateLink, n
            }

            ]), n.decorator("tooltipHtmlUnsafeDirective", ["$delegate", function(n) {
                return n[0].templateUrl=Roblox.uiBootstrap.tooltipHtmlUnsafePopupTemplateLink, n
            }

            ]), n.decorator("popoverPopupDirective", ["$delegate", function(n) {
                return n[0].templateUrl=Roblox.uiBootstrap.popoverPopupTemplateLink, n
            }

            ]), n.decorator("modalBackdropDirective", ["$delegate", function(n) {
                return n[0].templateUrl=Roblox.uiBootstrap.modalBackdropTemplateLink, n
            }

            ]), n.decorator("modalWindowDirective", ["$delegate", function(n) {
                return n[0].templateUrl=Roblox.uiBootstrap.modalWindowTemplateLink, n
            }

            ])
    }

    ]).controller("DropdownDemoCtrl", ["$scope", "$log", "$attrs", "$parse", function(n) {
        var u=["Places", "Clothes", "Gear"]; n.items=u, n.selection= {
            keyword:""
        }

        , n.select=function(t) {
            n.selectedItem=t, n.status.isopen= !1
        }
    }

    ]).controller("PaginationDemoCtrl", ["$scope", "$log", function(n) {
        n.totalItems=420, n.currentPage=1, n.maxSize=7
    }

    ]).controller("PopoverDemoCtrl", ["$scope", "$log", function(n) {
        n.content=null, n.popoverClick=function(t) {
            n.content=t
        }

        , angular.element(document.body).bind("click", function(n) {
                var r=document.querySelectorAll("*[popover]"), i, f; if(r.length>0)for(i=0; i<r.length; i++) {
                    var e=r[i], t=angular.element(e), u= !0; t.next().hasClass("popover")&&(f=angular.element(t.next()[0].querySelector(".popover-content")), u=f.has(n.target).length===0? !0: !1), !t.is(n.target)&&t.has(n.target).length===0&&u&&(t.scope().tt_isOpen= !1, t.scope().$digest())
                }
            })
    }

    ]).directive("popoverList", ["$log", function() {
        return {
            restrict:"EA", transclude: !0, replace: !0, scope: !0, templateUrl:Roblox.uiBootstrap.popoverListTemplateLink, link:function(n) {
                var t= {
                    0:[ {
                        label:"Setting", link:"/#!/setting"
                    }

                    , {
                    label:"Privacy", link:"/#!/privacy"
                }

                ], 1:[ {
                    label:"Configure", link:"/#!/configure"
                }

                , {
                label:"Advertise", link:"/#!/advertise"
            }

            , {
            label:"Create Badge", link:"/#!/createbadge"
        }

        ]
    }

    ; n.list=t[n.content]
}
}
}

]).controller("ModalDemoCtrl", ["$scope", "$modal", "$log", function(n, t, i) {
        n.open=function() {
            angular.element("body").addClass("modal-open-noscroll"); var u=t.open({
                templateUrl:"my-modal-content.html", controller:"ModalInstanceCtrl"

            }); u.result.then(function(t) {
                n.selected=t
            }

            , function() {
                angular.element("body").removeClass("modal-open-noscroll"), i.debug("Modal dismissed at: " +new Date)
            })
    }
}

]).controller("ModalInstanceCtrl", ["$scope", "$modalInstance", "$log", function(n, t, i) {
        n.close=function() {
            i.debug("ModalInstanceCtrl close at: " +new Date), t.dismiss("close")
        }
    }

    ]);