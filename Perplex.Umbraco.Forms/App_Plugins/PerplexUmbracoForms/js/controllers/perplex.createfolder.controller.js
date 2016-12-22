angular.module("umbraco").controller("Perplex.Form.CreateFolderController",
	function ($scope, perplexFormResource, navigationService, notificationsService) {
	    $scope.createFolder = function (parentId, name) {
	        // Empty names are fine, but undefined or null will be sent as string so transform
            // to empty string instead
	        perplexFormResource.createFolder(parentId, name || '').then(function (response) {	            
	            // response.data contains the created folder object
	            var folder = response.data;                

                // Navigate to the created folder's path in the tree
	            navigationService.syncTree({ tree: "form", path: folder.relativePath, forceReload: true, activate: true });

                // Hide the tree
	            navigationService.hideNavigation();

	            notificationsService.showNotification({ type: 0, header: "Folder created" });
	        });
	    };

	    $scope.cancelCreateFolder = function () {
	        navigationService.hideNavigation();
	    };
	});