angular.module("umbraco").controller("Perplex.Form.SortController",
	function ($scope, $routeParams, $http, formResource, perplexFormResource, navigationService, notificationsService) {
	    $scope.folder = null;
	    $scope.forms = [];

	    // If currentNode is undefined, use routeParams
        // This is the case in the folder-view (folder.html, when a folder is clicked, it uses the same controller)
	    var nodeId;
	    if ($scope.currentNode) {
	        nodeId = $scope.currentNode.id;
	    } else {
	        nodeId = $routeParams.id;
	    }

	    // Load all forms (required to display their names, we do not store form names)	    
	    formResource.getOverView()
        .then(function (response) {
            $scope.forms = response.data;

            // Load this folder
            perplexFormResource.getFolder(nodeId).then(function (response) {
                $scope.folder = response.data;

                // Goto folder in tree 	    
                navigationService.syncTree({ tree: "form", path: $scope.folder.path, forceReload: false, activate: true });
            }, function (error) { });
        });

	    $scope.sort = function () {
            // We call it sort, but we actually update every aspect of this folder :)
	        perplexFormResource.update($scope.folder).then(function (response) {
	            // Reload folder
	            navigationService.syncTree({ tree: "form", path: $scope.folder.path, forceReload: true }).then(function (syncArgs) {
	                navigationService.reloadNode(syncArgs.node);

	                // Hide the tree
	                navigationService.hideNavigation();

                    // Notify user
	                notificationsService.showNotification({ type: 0, header: "Sorting successful" });
	            });
	        });
	    };

	    $scope.getFormName = function (formId) {
	        var form = getForm(formId);
	        if (form == null) return "";

	        return form.name;
	    };

	    $scope.getFormId = function (formId) {
	        var form = getForm(formId);
	        if (form == null) return "";

	        return form.id;
	    }

	    function getForm(formId) {
	        return _.find($scope.forms, { id: formId });
	    }

	    $scope.cancelSort = function () {
	        navigationService.hideNavigation();
	    };
	});