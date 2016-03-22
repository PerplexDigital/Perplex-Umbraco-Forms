angular.module("umbraco").controller("Perplex.Form.MoveFormController",
	function ($scope, $http, perplexFormResource, navigationService, treeService) {
	    $scope.folders = [];

	    // Load all folders, all nested inside the root
	    perplexFormResource.getRootFolder().then(function (response) {
	        if (response.data != null) {
	            var rootFolder = response.data;

	            // Add all folders to the flat list
	            addFolder(rootFolder);
	        }
	    }, function (error) { });

	    function addFolder(folder) {
	        folder.depth = folder.path.length - 1;
	        $scope.folders.push(folder);
	        // Add child nodes	        
	        for (var i = 0; i < folder.folders.length; i++) {
	            addFolder(folder.folders[i]);
	        }
	    }

	    $scope.moveForm = function (formId, folderId) {
	        perplexFormResource.moveForm(formId, folderId).then(function (response) {
	            // response.data contains the new folder so we can sync the tree
	            var newFolder = response.data;
	            
	            var formPath = newFolder.path.concat([formId]);

	            // Remove the form from the tree
	            treeService.removeNode($scope.currentNode);

	            // Activate the form in its new location
	            navigationService.syncTree({ tree: "form", path: formPath, forceReload: false, activate: true });

	            // Hide menu
	            navigationService.hideNavigation();
	        });
	    };

	    $scope.selectFolder = function (folder) {
	        $scope.folderId = folder.id;
	    };

	    $scope.cancelMoveForm = function () {
	        navigationService.hideNavigation();
	    };
	});