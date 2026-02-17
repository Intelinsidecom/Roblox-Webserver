// GameItemCard.js
var Roblox = Roblox || {};
$(function() {
    var n = $("[data-voting-processed=false]");
    n.each(function(n, t) {
        var i = $(t),
            r = i.find(".vote-container"),
            u = parseInt(r.attr("data-upvotes")),
            f = parseInt(r.attr("data-downvotes"));
        Roblox.Voting.UpdateVoteBar(u, f, i), i.attr("data-voting-processed", !0)
    })
});