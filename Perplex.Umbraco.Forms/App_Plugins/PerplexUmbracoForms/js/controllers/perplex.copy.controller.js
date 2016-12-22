angular.module("umbraco").controller("Perplex.Form.CopyController",
	function ($scope, perplexFormResource, navigationService) {
	    $scope.copy = function (id) {

	        perplexFormResource.copyByGuid(id).then(function(response) {
	            // The response contains the parent folder of the form
	            var folder = response.data;

	            // We want to expand to the last form of the folder, 
	            // which is the form we just added
	            var formId = folder.forms[folder.forms.length - 1];

	            var path = folder.path.concat([formId]);

	            // Refresh the folder
	            navigationService.syncTree({ tree: "form", path: path, forceReload: true });

	            // Hide menu
	            navigationService.hideNavigation();
	        });

	    };
	    $scope.cancelCopy = function () {
	        navigationService.hideNavigation();
	    };
	});