angular.module("umbraco").controller("Perplex.Form.MoveFormController",
	function ($scope, $http, perplexFormResource, navigationService, treeService) {
	    $scope.folders = [];

	    // Load all folders, all nested inside the root
	    perplexFormResource.getRootFolder().then(function (response) {
	        if (response.data != null) {
	            var rootFolder = response.data;

	            // Add all folders to the flat list
	            addFolder(rootFolder);

	            // Since we added start folders, we have to tweak this list a bit more
	            var enabledFolders = _.filter($scope.folders, { disabled: false });	            

	            // Find common ancestor
	            var folderPaths = _.pluck(enabledFolders, 'path');

	            var commonAncestors = _.intersection.apply(this, folderPaths);

                // Grab the last one (deepest in tree)
	            var commonAncestor = commonAncestors[commonAncestors.length - 1];

	            // Since the Start Nodes were introduced, respect them
	            // Filter out all disabled folders, but keep the common ancestor (which is disabled itself usually),
                // and any node between the common ancestor and a node which is not disabled
	            $scope.folders = _.filter($scope.folders, function (folder) {
	                return !folder.disabled || folder.id === commonAncestor ||
                        (folder.path.indexOf(commonAncestor) > -1 && _.any(enabledFolders, function (f) { return f.path.indexOf(folder.id) > -1; }));
	            });

	            // Fix depth
	            var minDepthFolder = _.min($scope.folders, 'depth');
	            var minDepth = minDepthFolder.depth;

	            $scope.folders = _.map($scope.folders, function (folder) {
	                folder.depth = folder.depth - minDepth;
	                return folder;
	            });	            
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
	            
	            var formPath = newFolder.relativePath.concat([formId]);

	            // Remove the form from the tree
	            treeService.removeNode($scope.currentNode);

	            // Activate the form in its new location
	            navigationService.syncTree({ tree: "form", path: formPath, forceReload: false, activate: true });

	            // Hide menu
	            navigationService.hideNavigation();
	        });
	    };

	    $scope.selectFolder = function (folder) {            
            // Can't select disabled folders, sorry
	        if (folder.disabled) return;

	        $scope.folderId = folder.id;
	    };

	    $scope.cancelMoveForm = function () {
	        navigationService.hideNavigation();
	    };
	});