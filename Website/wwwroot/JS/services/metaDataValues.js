// services/metaDataValues.js
"use strict";
var Roblox = Roblox || {};
Roblox.MetaDataValues = function() {
    function t() {
        return n && n.dataset && n.dataset.internalPageName
    }
    var n = document.querySelector('meta[name="page-meta"]');
    return {
        getPageName: t
    }
}();