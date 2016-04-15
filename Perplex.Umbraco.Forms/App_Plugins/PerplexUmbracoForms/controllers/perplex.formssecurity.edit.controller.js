angular.module("umbraco").controller("Perplex.FormsSecurity.EditController",
	function ($scope, $controller, perplexFormResource, navigationService, treeService, formResource, $routeParams, notificationsService, $http, $compile) {
	    $controller('UmbracoForms.Editors.Security.EditController', { $scope: $scope });

	    var defaultSave = $scope.save;

	    $scope.loading = true;
	    $scope.folders = [];
	    $scope.userId = $routeParams.id;
	    $scope.startNodes = [];

		// Fetch Umbraco's default edit view.
	    $http.get('/App_Plugins/UmbracoForms/backoffice/FormSecurity/edit.html').success(function (template) {
            // Remove Umbraco's controller
            template = template.replace(/ng-controller\s*=\s*(["'])UmbracoForms.Editors.Security.EditController\1/, '');

            // Add Form Start Nodes directive before the first umb-control-group
            template = template.replace(/(.*?)(<umb-control-group .*?)/, function(full, one, two) {
                return one + '<perplex-forms-start-nodes></perplex-forms-start-nodes>' + two
            });

            $compile($("#perplex-forms-security-edit").html(template).contents())($scope);
		});

	    $scope.save = function () {
	        defaultSave();
	        saveStartNodes();
	    }

	    function saveStartNodes() {
	        perplexFormResource.setFormStartNodes($scope.userId, $scope.startNodes).then(function (response) {
	            notificationsService.showNotification({ type: 0, header: "Start Nodes in Forms saved" });
            });
	    }

	    // Load the start nodes for the selected user
	    perplexFormResource.getFormStartNodes($scope.userId).then(function (response) {
	        $scope.startNodes = _.pluck(response.data, 'id');

            // Load folders
            perplexFormResource.getRootFolder().then(function (response) {
                if (response.data != null) {
                    var rootFolder = response.data;

                    $scope.folders.push(rootFolder);
                    initFolder(rootFolder);
                }

								updateDisabledFolders();

                $scope.loading = false;
            }, function (error) {
                // TODO: Handle error
                $scope.loading = false;
            });
	    });

	    // Initialize a folder and its subfolders
	    function initFolder(folder) {
	        // If this folder is set as a startnode,
            // expand to this folder
	        var isStartFolder = _.contains($scope.startNodes, folder.id);
	        if (isStartFolder) {
	            folder.selected = true;

	            // Expand all parent folders and the folder itself
	            _.each(folder.path, function (folderId) {
	                var ff = findFolder(folderId);
	                if (ff != null) {
	                    ff.expanded = true;
	                }
	            });
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
	        // Always show root
	        if (folder.id === '-1') {
	            return true;
	        }

	        var parent = findFolder(folder.parentId);
	        if (parent == null) {
	            return false;
	        }

	        return parent.expanded && $scope.showFolder(parent);
	    }

	    $scope.selectFolder = function (folder) {
	        folder.selected = !folder.selected;

	        if (folder.selected) {
	            $scope.startNodes.push(folder.id);
	        } else {
	            var index = $scope.startNodes.indexOf(folder.id);
	            $scope.startNodes.splice(index, 1);
	        }

	        updateDisabledFolders();
	    }

	    // Sets the disabled state of all folders based on
	    // the currently selected startnodes
	    function updateDisabledFolders() {
	        _.each($scope.folders, updateDisabledFolder);
	    }

	    function updateDisabledFolder(folder) {
	        // No startnodes? Then everything is enabled
	        if ($scope.startNodes.length === 0) {
	            folder.disabled = false;
	        } else {
	            // Startnodes are not in folder's path, folder is disabled
	            if(!_.any($scope.startNodes, function(sn) {
                    return folder.path.indexOf(sn) > -1;
	            })) {
	                folder.disabled = true;
	            } else {
	                // Otherwise => enabled
	                folder.disabled = false;
	            }
	        }

            // And traverse children
	        _.each(folder.folders, updateDisabledFolder);
	    }

	    $scope.clear = function () {
	        _.each($scope.startNodes, function (folderId) {
	            var folder = findFolder(folderId);
	            if (folder != null) {
	                folder.selected = false;
	            }
	        });

	        $scope.startNodes = [];

	        updateDisabledFolders();
	    }
	});