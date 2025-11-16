"use strict";

robloxAppService.factory("cacheService", [function() {
        return {
            createPaginationCache:function() {
                var n= {}

                ; return {
                    getPage:function(t, i, r) {
                        return n[t]?n[t].slice((i-1)*r, i*r):[]
                    }

                    , getLength:function(t) {
                        return n[t]?n[t].length:0
                    }

                    , append:function(t, i) {
                        n[t]||(n[t]=[]), n[t]=n[t].concat(i)
                    }

                    , clear:function(t) {
                        n[t]=[]
                    }
                }
            }

            , buildKey:function(n) {
                var i="", t; for(t in n)n.hasOwnProperty(t)&&(i+="&" +t+"=" +n[t]); return i
            }
        }
    }

    ]);