// Games/GamesListBehavior.js
"use strict";
if (typeof Roblox === 'undefined') {
    var Roblox = {};
}
Roblox.GamesListBehavior = {
    RefreshAdsInGamesPageEnabled: true,
    isUserEligibleForMultirowFirstSort: false,
    
    init: function() {
        this.bindEvents();
        this.setupCarousels();
        this.initializeVoting();
    },
    
    bindEvents: function() {
        $(document).on('click', '.game-card', this.handleGameCardClick.bind(this));
        $(document).on('mouseenter', '.game-card', this.handleGameCardHover.bind(this));
        $(document).on('mouseleave', '.game-card', this.handleGameCardLeave.bind(this));
    },
    
    setupCarousels: function() {
        $('.horizontally-scrollable').each(function() {
            var container = $(this);
            var prevBtn = container.closest('.games-list').find('.scroller.prev');
            var nextBtn = container.closest('.games-list').find('.scroller.next');
            var list = container.find('.hlist');
            
            prevBtn.on('click', function() {
                var scrollAmount = 200;
                container.animate({ scrollLeft: container.scrollLeft() - scrollAmount }, 300);
                updateCarouselButtons();
            });
            
            nextBtn.on('click', function() {
                var scrollAmount = 200;
                container.animate({ scrollLeft: container.scrollLeft() + scrollAmount }, 300);
                updateCarouselButtons();
            });
            
            function updateCarouselButtons() {
                var scrollLeft = container.scrollLeft();
                var maxScroll = container[0].scrollWidth - container.width();
                
                prevBtn.toggleClass('disabled', scrollLeft <= 0);
                nextBtn.toggleClass('disabled', scrollLeft >= maxScroll);
            }
            
            updateCarouselButtons();
        });
    },
    
    initializeVoting: function() {
        if (typeof Roblox !== 'undefined' && Roblox.Voting) {
            this.updateVotes();
        }
    },
    
    updateVotes: function() {
        // Update vote bars on game cards
        $('[data-voting-processed=false]').each(function() {
            var card = $(this);
            var voteContainer = card.find('.vote-container');
            var upvotes = parseInt(voteContainer.attr('data-upvotes')) || 0;
            var downvotes = parseInt(voteContainer.attr('data-downvotes')) || 0;
            
            if (typeof Roblox !== 'undefined' && Roblox.Voting && Roblox.Voting.UpdateVoteBar) {
                Roblox.Voting.UpdateVoteBar(upvotes, downvotes, card);
            }
            
            card.attr('data-voting-processed', 'true');
        });
    },
    
    handleGameCardClick: function(e) {
        var card = $(e.target).closest('.game-card');
        var placeId = card.data('place-id');
        
        if (placeId) {
            window.location.href = '/games/' + placeId;
        }
    },
    
    handleGameCardHover: function(e) {
        var card = $(e.target).closest('.game-card');
        card.addClass('hover');
        card.find('.vote-container .vote-percentage').addClass('hover');
    },
    
    handleGameCardLeave: function(e) {
        var card = $(e.target).closest('.game-card');
        card.removeClass('hover');
        card.find('.vote-container .vote-percentage').removeClass('hover');
    },
    
    refreshAds: function() {
        if (this.RefreshAdsInGamesPageEnabled && typeof Roblox !== 'undefined' && Roblox.AdsHelper) {
            Roblox.AdsHelper.refreshAds();
        }
    }
};

$(function() {
    if (typeof Roblox.GamesListBehavior !== 'undefined' && Roblox.GamesListBehavior.init) {
        Roblox.GamesListBehavior.init();
    }
});
