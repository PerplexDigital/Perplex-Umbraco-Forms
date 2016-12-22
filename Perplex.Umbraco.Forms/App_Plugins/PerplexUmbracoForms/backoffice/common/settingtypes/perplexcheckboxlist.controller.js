angular.module("umbraco")
.controller("SettingTypes.PerplexcheckboxlistController",
function ($scope, $routeParams, $q, pickerResource, perplexFormResource, perplexConstants) {
	var self = this;

	$scope.selectedValues = [];

    // Load saved values
	if (typeof $scope.setting.value === 'string') {
	    $scope.selectedValues = $scope.setting.value.split(',');
	}

    // Possible checkbox values
	self.values = [];

    // The values are configurable and depend upon the FieldType
    var field = $scope.model && $scope.model.field;
    if (field) {
        var fieldTypeId = field.fieldTypeId.toLowerCase();

        // Values for the checkboxlist should be provided in a function returning a promise
        // such as an $http.get call.
        var promiseFn = null;

        switch(fieldTypeId) {
            case perplexConstants.fieldTypeIds.PerplexFileUpload:
                promiseFn = perplexFormResource.getFileUploadAllowedExtensions;

                break;

            case perplexConstants.fieldTypeIds.PerplexImageUpload:
                promiseFn = perplexFormResource.getImageUploadAllowedExtensions;

                break;

            default: break;
        }

        if (typeof promiseFn === 'function') {
            promiseFn().then(function (response) {
                self.values = response.data;

                // Make sure the $selectedValues does not contain any
                // values that are not currently configured anymore
                $scope.selectedValues = _.filter($scope.selectedValues, function (selectedValue) {
                    return _.contains(self.values, selectedValue);
                });

                // And update the setting value itself
                setValue();
            });
        }
    }

    $scope.updateCheckboxValue = function (value) {
	    if (removeFromArray($scope.selectedValues, value) === false) {
	        $scope.selectedValues.push(value);
	    }

	    setValue();
	};

	function removeFromArray(array, object) {
	    var index = array.indexOf(object);

	    if (index > -1) {
	        array.splice(index, 1);
	        return true;
	    }

	    return false;
	}

	// Sets the value as a comma separated list of values
	function setValue() {
	    $scope.setting.value = $scope.selectedValues.join(',');
	}
});