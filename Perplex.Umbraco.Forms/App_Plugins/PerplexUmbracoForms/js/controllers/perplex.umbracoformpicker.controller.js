angular.module("umbraco").controller("Perplex.UmbracoFormPickerController",
function ($scope, $http, formResource, perplexFormResource) {
    $scope.loading = true;

    $scope.folders = [];

    // Load all forms
    formResource.getOverView()
    .then(function (response) {
        $scope.forms = response.data;

        // When the forms are loaded, load all folders
        perplexFormResource.getRootFolder().then(function (response) {
            if (response.data != null) {
                var rootFolder = response.data;

                // Make sure to order the forms of the root folder 
                // in the order defined by the root folder.
                // By default, $scope.forms is ordered in whatever order 
                // the formResource returns them.
                var orderedForms = rootFolder.forms;

                $scope.forms.sort(function (x, y) { return orderedForms.indexOf(x.id) > orderedForms.indexOf(y.id) ? 1 : -1; });

                // Do not actually show the root folder,
                // it will always be expanded anyway, so start
                // at the root's children
                _.each(rootFolder.folders, function (folder) {
                    $scope.folders.push(folder);
                    initFolder(folder);
                });
            }

            $scope.loading = false;
        }, function (error) {
            // TODO: Handle error
            $scope.loading = false;
        });

    }, function (err) {
        $scope.error = "An error has occured while loading!";
        $scope.loading = false;
    });

    // Initialize a folder and its subfolders
    function initFolder(folder) {
        // If this folder contains the form that is currently selected,
        // expand the folder tree to this folder
        if (_.contains(folder.forms, $scope.model.value)) {
            // Expand all parent folders and the folder itself
            _.each(folder.path, function (folderId) {
                var ff = findFolder(folderId);
                if (ff != null) {
                    ff.expanded = true;
                }
            });
        }

        // Load forms from $scope.forms into this object
        var forms = [];

        for (var i = 0; i < folder.forms.length; i++) {
            var formId = folder.forms[i];

            var form = _.find($scope.forms, { id: formId });

            // Remove form from $scope.forms
            $scope.forms = _.filter($scope.forms, function (f) {
                return f.id !== formId;
            });

            if (form != null) {
                forms.push(form);
            }
        }

        if (forms.length > 0) {
            folder.forms = forms;
        }

        // Init subfolders
        _.each(folder.folders, function (subFolder) {
            initFolder(subFolder);
        })
    }

    // Recusively find a folder inside a list of folders
    function findFolder(id, parent) {
        // No parent given, search through $scope.folders
        if (parent == null) {
            var folder = _.find($scope.folders, { id: id });
            if (folder != null) {
                return folder;
            }

            // Search sub folders
            for (var i = 0; i < $scope.folders.length; i++) {
                folder = findFolder(id, $scope.folders[i]);
                if (folder != null) {
                    return folder;
                }
            }
        } else {
            if (parent.id == id) {
                return parent;
            }

            // Search sub folders
            for (var i = 0; i < parent.folders.length; i++) {
                folder = findFolder(id, parent.folders[i]);
                if (folder != null) {
                    return folder;
                }
            }
        }
    }

    $scope.toggleFolder = function (folder) {
        // Set expanded state
        folder.expanded = !folder.expanded;
    }

    // Returns whether or not to show, does not actually show/hide
    $scope.showFolder = function (folder) {
        // Always show root folders
        if (folder.parentId === '-1') {
            return true;
        }

        var parent = findFolder(folder.parentId);
        if (parent == null) {
            return false;
        }

        return parent.expanded && $scope.showFolder(parent);
    }

    $scope.selectForm = function (form) {
        $scope.model.value = form.id;
    }

    $scope.clear = function () {
        $scope.model.value = null;
    }
});