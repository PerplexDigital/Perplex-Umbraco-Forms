angular.module("umbraco").controller("SettingTypes.PerplexcheckboxlistController",
	function ($scope, $routeParams, pickerResource) {
	    $scope.selectedValues = [];

	    $scope.init = function () {
            if($scope.setting.value != "")
	            $scope.selectedValues = $scope.setting.value.split(',');
	    };

	    $scope.updateCheckboxValue = function (value) {
	        if (removeFromArray($scope.selectedValues, value) === false) {
	            $scope.selectedValues.push(value);
	        }

	        $scope.setting.value = $scope.selectedValues.join(',');
	    };

	    removeFromArray = function (array, object) {
	        var index = array.indexOf(object);

	        if (index > -1) {
	            array.splice(index, 1);
	            return true;
	        } else
	            return false;
	    }
	});