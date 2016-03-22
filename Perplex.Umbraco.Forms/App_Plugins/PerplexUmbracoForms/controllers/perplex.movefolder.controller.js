angular.module("umbraco").controller("Perplex.Form.MoveFolderController",
	function ($scope, $http, perplexFormResource, navigationService, treeService) {	  
	    $scope.folders = [];

	    // Load all folders, all nested inside the root
	    perplexFormResource.getRootFolder().then(function (response) {
	        if (response.data != null) {
	            var rootFolder = response.data;

	            // Add all folders to the flat list,
	            // except the folder we are going to move                
	            addFolder(rootFolder);
	        }
	    }, function (error) {});

	    function addFolder(folder) {
	        // Do not add the folder itself, we cannot be moving a folder to itself.
	        // Moving to the current parent is silly as well, but removing that would mess up the tree's looks            
	        if (folder.id == $scope.currentNode.id) {
	            return;
	        }

	        folder.depth = folder.path.length - 1;
	        $scope.folders.push(folder);
	        // Add child nodes	        
	        for (var i = 0; i < folder.folders.length; i++) {
	            addFolder(folder.folders[i]);
	        }
	    }

	    $scope.moveFolder = function (id, folderId) {
	        perplexFormResource.moveFolder(id, folderId).then(function (response) {
	            // response.data contains the moved folder (after it was moved)
	            var movedFolder = response.data;
	            
	            // Remove the folder from the tree
	            treeService.removeNode($scope.currentNode);

	            // Activate the form in its new location
	            navigationService.syncTree({ tree: "form", path: movedFolder.path, forceReload: false, activate: true });

	            // Hide menu
	            navigationService.hideNavigation();
	        });

	    };

	    $scope.selectFolder = function (folder) {
	        $scope.folderId = folder.id;
	    };

	    $scope.cancelMoveFolder = function () {
	        navigationService.hideNavigation();
	    };
	});