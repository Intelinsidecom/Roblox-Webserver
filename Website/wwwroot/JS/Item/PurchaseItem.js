// Item/PurchaseItem.js
"use strict";
var Roblox = Roblox || {};
Roblox.PurchaseItem = function() {
    var n = $("#item-container"),
        t = $(".PurchaseButton, .InstallButton").first(),
        i;
    t.length > 0 && (t.attr("data-expected-price", n.attr("data-expected-price")), t.attr("data-bc-requirement", n.attr("data-bc-requirement")), t.attr("data-product-id", n.attr("data-product-id")), t.attr("data-item-id", n.attr("data-item-id")), t.attr("data-item-name", n.attr("data-item-name")), t.attr("data-asset-type", n.attr("data-asset-type")), t.attr("data-expected-currency", n.attr("data-expected-currency")), t.attr("data-expected-seller-id", n.attr("data-expected-seller-id"))), i = $(".PurchaseButton"), i.length > 0 && (i.attr("data-seller-name", n.attr("data-seller-name")), i.attr("data-userasset-id", n.attr("data-lowest-private-sale-userasset-id")))
}();