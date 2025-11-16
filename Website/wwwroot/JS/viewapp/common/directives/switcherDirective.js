"use strict";

robloxHelpers.directive("switcher", ["$log", function() {
        return {
            restrict:"A", controller:["$scope", function(n) {
                n.switcher= {}

                , n.switcher.games= {
                    currPage:0, itemsCount:0
                }

                , n.switcher.groups= {
                    currPage:0, itemsCount:0
                }
            }

            ], scope: {
                currpage:"=", itemscount:"="
            }

            , link:function(n, t) {
                var r=null; n.currpage=0, t.find(".switcher-item")&&t.find(".switcher-item").length>0&&(n.itemscount=t.find(".switcher-item").length); t.find(".carousel-control").on("click", function(i) {
                        n.$apply(function() {
                                angular.element(i.currentTarget).attr("data-switch")=="next" ?n.currpage+1<=n.itemscount-1?n.currpage+=1:n.currpage=0:n.currpage-1>=0?n.currpage-=1:n.currpage=n.itemscount-1
                            }), r=t.find(".switcher-item[data-index=" +n.currpage+"] img"), r.attr("src")||r.attr("src", r.attr("data-src"))
                    })
            }
        }
    }

    ]);