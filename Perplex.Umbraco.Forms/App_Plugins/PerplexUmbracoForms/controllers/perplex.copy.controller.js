angular.module("umbraco").controller("Perplex.Form.CopyController",
	function ($scope, perplexFormResource, navigationService, treeService) {
	    $scope.copy = function (id) {
	        
	        perplexFormResource.copyByGuid(id).then(function () {
	            // Reload the tree by passing the parent path
	            navigationService.syncTree({ tree: "form", path: [String($scope.currentNode.parent().path)], forceReload: true });
                // Hide the tree
	            navigationService.hideNavigation();
	        });

	    };
	    $scope.cancelCopy = function () {
	        navigationService.hideNavigation();
	    };
	});

function perplexFormResource($http) {
    //the factory object returned
    var apiRoot = "backoffice/api/PerplexUmbracoForm/";

    return {
        copyByGuid: function (id) {
            return $http.post(apiRoot + "CopyByGuid?guid=" + id);
        },
    };
}

angular.module('umbraco.resources').factory('perplexFormResource', perplexFormResource);