// Trade/InventoryItem.js
typeof Roblox == "undefined" && (Roblox = {}), Roblox.InventoryItem = function(n) {
    function f(n) {
        typeof n != "undefined" && (t.find(".InventoryItemName").text(n.Name), t.find(".InventoryItemLink").attr("href", n.ItemLink), t.find(".ItemImg").attr("src", n.ImageLink), t.find(".InventoryItemAveragePrice").text(i(n.AveragePrice)), t.find(".InventoryItemOriginalPrice").text(i(n.OriginalPrice)), t.find(".InventoryItemSerial").text(n.SerialNumber), t.find(".SerialNumberTotal").text(n.SerialNumberTotal), t.find(".BuildersClubOverlay").attr("src", n.MembershipLevel))
    }

    function i(n) {
        var t = Number(n);
        return !isNaN(t) && t > 1e6 ? Math.round(n / 1e6) + "M" : n
    }
    var t = n,
        r = "LargeInventoryItem",
        u = "SmallInventoryItem";
    return {
        display: f,
        largeClassName: r,
        smallClassName: u
    }
};