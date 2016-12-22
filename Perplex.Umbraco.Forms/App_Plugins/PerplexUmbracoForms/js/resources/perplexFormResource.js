function perplexFormResource($http) {
    var apiRoot = "backoffice/api/PerplexUmbracoForm/";

    return {
        copyByGuid: function (id) {
            return $http.post(apiRoot + "CopyByGuid?guid=" + id);
        },

		createFolder: function(parentId, name) {
			return $http.post(apiRoot + "CreateFolder?parentId=" + parentId + "&name=" + name);
		},

		moveForm: function (formId, folderId) {
				return $http.post(apiRoot + "MoveForm?formId=" + formId + "&folderId=" + folderId);
		},

		moveFolder: function (id, folderId) {
				return $http.post(apiRoot + "MoveFolder?id=" + id + "&folderId=" + folderId);
		},

		update: function(folder) {
				return $http.post(apiRoot + "Update", folder);
		},

		getFolder: function(folderId) {
				return $http.get(apiRoot + "GetFolder?folderId=" + folderId);
		},

		getRootFolder: function () {
				return $http.get(apiRoot + "GetRootFolder");
		},

		deleteFolder: function (folderId, deleteForms) {
				return $http.post(apiRoot + "DeleteFolder?folderId=" + folderId + "&deleteForms=" + deleteForms);
		},

		setFormStartNodes: function (userId, folderIds) {
				return $http.post(apiRoot + "SetFormStartNodes?userId=" + userId + "&folderIds=" + folderIds.join(','));
		},

		getFormStartNodes: function (userId) {
				return $http.get(apiRoot + "GetFormStartNodes?userId=" + userId);
		},

		getFormFolder: function (formId) {
				return $http.get(apiRoot + "GetFormFolder?formId=" + formId);
		},

		getFormsRootNode: function () {
				return $http.get(apiRoot + "GetFormsRootNode")
		},

		getConfiguration: function () {
		    return $http.get(apiRoot + "GetConfiguration");
		},

		getHideFieldTypes: function () {
		    return $http.get(apiRoot + "GetHideFieldTypes");
		},

		getFileUploadAllowedExtensions: function () {
		    return $http.get(apiRoot + "GetFileUploadAllowedExtensions");
		},

		getImageUploadAllowedExtensions: function () {
		    return $http.get(apiRoot + "GetImageUploadAllowedExtensions");
		},
    };
}

angular.module('umbraco.resources').factory('perplexFormResource', perplexFormResource);