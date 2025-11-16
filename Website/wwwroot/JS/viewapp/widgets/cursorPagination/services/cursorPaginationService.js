"use strict";

cursorPagination.factory("cursorPaginationService", ["$q", "$rootScope", "httpService", "cacheService", function(n, t, i, r) {
        var f="limit", e="cursor", o="sortOrder", u=n.defer(); return u.reject([ {
                code:-1, message:"Busy"
            }

            ]), {
        getDataListFromResponseMethods: {
            defaultV1:function(n) {
                return n.data
            }
        }

        , getNextPageCursorFromResponseMethods: {
            defaultV1:function(n) {
                return n.nextPageCursor
            }
        }

        , getErrorsFromResponseMethods: {
            defaultV1:function(n) {
                return n.errors
            }
        }

        , getQueryParametersMethods: {
            defaultV1:function(n) {
                return n
            }
        }

        , sortOrder: {
            Asc:"Asc", Desc:"Desc"
        }

        , createPager:function(t) {
            function v() {
                return r.buildKey(t.getCacheKeyParameters(s))
            }

            function y(r) {
                var f=v(), e=l.getPage(f, r, t.pageSize), u=n.defer(), o, y; return(u.promise.then(t.loadSuccess, t.loadError), e.length===t.pageSize||typeof c[f] !="string")?(h=r, u.resolve(e), u.promise):(s[t.cursorName]=c[f], t.beforeLoad&&t.beforeLoad(r, s), a= !0, o= {
                        url:t.getRequestUrl(s)
                    }

                    , y=t.getQueryParameters(s), i.httpGet(o, y).then(function(n) {
                            h=r, c[f]=t.getNextPageCursorFromResponse(n), a= !1; var i=t.getDataListFromResponse(n); Array.isArray(i)?(l.append(f, i), e=l.getPage(f, r, t.pageSize), u.resolve(e)):u.reject([ {
                                    code:0, message:"data pulled from response not array"
                                }

                                ])
                        }

                        , function(n) {
                            a= !1, u.reject(t.getErrorsFromResponse(n|| {}))
                        }), u.promise)
            }

            var h=0, a= !1, s= {}

            , c= {}

            , l=r.createPaginationCache(); return t.limitName=t.limitName||f, t.cursorName=t.cursorName||e, t.sortOrderName=t.sortOrderName||o, t.sortOrder=t.sortOrder||this.sortOrder.Asc, s[t.cursorName]="", s[t.sortOrderName]=t.sortOrder, s[t.limitName]=t.loadPageSize, t.getDataListFromResponse=t.getDataListFromResponse||this.getDataListFromResponseMethods.defaultV1, t.getNextPageCursorFromResponse=t.getNextPageCursorFromResponse||this.getNextPageCursorFromResponseMethods.defaultV1, t.getErrorsFromResponse=t.getErrorsFromResponse||this.getErrorsFromResponseMethods.defaultV1, t.getQueryParameters=t.getQueryParameters||this.getQueryParametersMethods.defaultV1, t.loadSuccess=t.loadSuccess||function() {}

            , t.loadError=t.loadError||function() {}

            , {
            isBusy:function() {
                return a
            }

            , setPagingParameter:function(n, t) {
                t===undefined||t===null?delete s[n]:s[n]=t
            }

            , getPagingParameter:function(n) {
                return s[n]
            }

            , getCurrentPageNumber:function() {
                return h
            }

            , loadNextPage:function() {
                return this.canLoadNextPage()?y(h+1):u.promise
            }

            , loadPreviousPage:function() {
                return this.canLoadPreviousPage()?y(h-1):u.promise
            }

            , loadFirstPage:function() {
                if( !this.canLoadFirstPage())return u.promise; var n=v(); return l.clear(n), c[n]="", y(1)
            }

            , canLoadNextPage:function() {
                return this.isBusy()? !1:this.hasNextPage()
            }

            , hasNextPage:function() {
                var n=v(); return l.getLength(n)>h*t.pageSize? !0:typeof c[n]=="string"
            }

            , canLoadPreviousPage:function() {
                return !this.isBusy()&&h>1
            }

            , canLoadFirstPage:function() {
                return !this.isBusy()
            }
        }
    }
}
}

]);