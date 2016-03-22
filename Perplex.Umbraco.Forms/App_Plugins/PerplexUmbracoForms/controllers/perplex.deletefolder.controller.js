angular.module("umbraco").controller("Perplex.Form.DeleteFolderController",
	function ($scope, perplexFormResource, navigationService, treeService) {
	    $scope.deleteFolder = function (folderId) {
            // Make sure deleteForms is a boolean, and not (for example) undefined;
	        var deleteForms = !!$scope.deleteForms;

	        perplexFormResource.deleteFolder(folderId, deleteForms).then(function (response) {
	            // Remove the deleted folder from the tree
	            treeService.removeNode($scope.currentNode);

	            // Hide menu
	            navigationService.hideNavigation();
	        });
	    }

	    $scope.cancelDeleteFolder = function () {
	        navigationService.hideNavigation();
	    };
	});