// Upgrades/BuildersClubProductsGrid.js
$(function() {
    $("a.product-button").click(function() {
        var t = $("#UserDataInfo").attr("data-auth"),
            rank = $(this).data("rank");
        var rankMap = { "BC": "bc", "TBC": "tbc", "OBC": "obc" };
        var membershipType = rankMap[rank] || "bc";
        window.location.href = t == "false" ? "/NewLogin?ReturnUrl=" + encodeURIComponent(location.pathname + location.search) : "/upgrades/membership?MembershipType=" + membershipType;
    })
});