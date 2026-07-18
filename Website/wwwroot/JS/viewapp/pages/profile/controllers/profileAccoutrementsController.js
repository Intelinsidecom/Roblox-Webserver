// ~/viewapp/pages/profile/controllers/profileAccoutrementsController.js
"use strict";
profile.controller("profileAccoutrementsController", ["$scope", "$log", function(n) {
    n.profileAccoutrementsLayout = {}, n.getAccoutrementsPage = function(t) {
        angular.element(".profile-accoutrements-slider ul").css("display", "none"), angular.element(".profile-accoutrements-slider ul:nth-child(" + (t + 1) + ")").css("display", "block"), n.profileAccoutrementsLayout.currentPageNumber = t
    }
}]);