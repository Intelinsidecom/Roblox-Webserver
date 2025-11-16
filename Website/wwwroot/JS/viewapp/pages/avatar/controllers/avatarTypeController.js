"use strict";

// Minimal controller to support R6/R15 toggle and avoid ng:areq errors.
// Keeps behavior conservative: only calls avatarService if available.
avatar.controller("avatarTypeController", ["$scope", "$log", "avatarService", function($scope, $log, avatarService) {
    $scope.pageLoaded = true;
    // Default to R6 unless the view binds a different value
    if (!$scope.avatarType) {
        $scope.avatarType = "R6";
    }

    $scope.updateAvatarType = function() {
        try {
            if (avatarService && typeof avatarService.setAvatarType === "function") {
                // Pass through the bound value; service decides how to handle it
                var result = avatarService.setAvatarType($scope.avatarType);
                if (result && typeof result.then === "function") {
                    result.then(function() {
                        if (typeof $scope.refreshThumbnail === "function") {
                            $scope.refreshThumbnail();
                        }
                    });
                }
            }
        } catch (e) {
            if ($log && typeof $log.debug === "function") {
                $log.debug(e);
            }
        }
    };
}]);