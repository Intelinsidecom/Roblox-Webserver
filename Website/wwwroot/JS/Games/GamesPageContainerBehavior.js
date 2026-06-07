// Games/GamesPageContainerBehavior.js
"use strict";
if (typeof Roblox === 'undefined') {
    var Roblox = {};
}
Roblox.GamesPageContainerBehavior = {
    init: function() {
        this.bindEvents();
        this.setupFilters();
        this.initializeGames();
    },
    
    bindEvents: function() {
        $(document).on('change', '.games-page-filter', this.handleFilterChange.bind(this));
        $(document).on('click', '.games-filter-changer', this.handleFilterClick.bind(this));
    },
    
    setupFilters: function() {
        $('#SortFilter, #TimeFilter, #GenreFilter').each(function() {
            $(this).find('li[data-value]').on('click', function() {
                var value = $(this).data('value');
                var label = $(this).find('a').text();
                $(this).closest('.games-page-filter').find('.rbx-selection-label').text(label).attr('data-value', value);
            });
        });
    },
    
    initializeGames: function() {
        if (typeof Roblox.GamesPage !== 'undefined') {
            // Check if dependencies are loaded before calling init
            if (typeof Roblox.GamesPage.Settings !== 'undefined' && typeof Roblox.GamesPage.State !== 'undefined') {
                Roblox.GamesPage.init();
            } else {
                console.log('GamesPageContainerBehavior: Dependencies not loaded, skipping init');
            }
        }
    },
    
    handleFilterChange: function(e) {
        var filterType = $(e.target).closest('.games-page-filter').attr('id');
        var filterValue = $(e.target).data('value');
        this.updateURL(filterType, filterValue);
        this.reloadGames();
    },
    
    handleFilterClick: function(e) {
        e.preventDefault();
        var container = $(e.target).closest('.games-list-container');
        var sortFilter = container.data('sortfilter');
        this.showGamesList(sortFilter);
    },
    
    updateURL: function(filterType, filterValue) {
        var url = new URL(window.location);
        url.searchParams.set(filterType, filterValue);
        window.history.pushState({}, '', url);
    },
    
    reloadGames: function() {
        if (typeof Roblox.GamesPage !== 'undefined' && Roblox.GamesPage.loadGames) {
            Roblox.GamesPage.loadGames();
        }
    },
    
    showGamesList: function(sortFilter) {
        $('.games-list-container').addClass('hidden');
        $('#GamesListContainer' + sortFilter).removeClass('hidden');
    }
};

$(function() {
    if (typeof Roblox.GamesPageContainerBehavior !== 'undefined' && Roblox.GamesPageContainerBehavior.init) {
        Roblox.GamesPageContainerBehavior.init();
    }
});
