angular.module('umbraco')
.config(function($provide) {   
    $provide.decorator('navigationService', function($delegate, $location, $routeParams, $q, perplexFormResource) {
        var syncTreeFn = $delegate.syncTree;

        $delegate.syncTree = function() {
            var original = (function(args) { return function() { return syncTreeFn.apply($delegate, args) }; })(arguments);

            // If we are not inside the Forms section viewing a Form, use the default behavior.
            // Same if somehow no arguments are passed to this function, we need at least 1
            if(arguments.length === 0 || $location.path().indexOf('/forms/form/edit') !== 0)
                return original();

            // We want to manipulate the arguments to include the form's parent folders
            var newArgs = Array.prototype.slice.apply(arguments);

            // Arguments should have a path property on its first argument,
            // which should be an Array and have only 1 element. If that's the case,
            // it's likely a call from Umbraco's Form Edit controller, which does not include
            // the folder structure yet as part of its path.
            var arg = newArgs[0];

            // We need a valid path with exactly 1 entry, otherwise fall back to default again
            if(!arg.path || arg.path.constructor.name !== 'Array' || arg.path.length !== 1)
                return original();

            // If so, we must update the arguments to include the form's parent folders
            var deferred = $q.defer();

            perplexFormResource.getFormFolder($routeParams.id).then(function (response) {
                var containingFolder = response.data;

                // No containing folder? Err ok. Should never happen but just in case.
                if (containingFolder == null || containingFolder ===  'null') {
                    deferred.reject();
                    return;
                }
                
                // Add folder path to the arguments for syncTree
                Array.prototype.unshift.apply(arg.path, containingFolder.relativePath);

                // We are done, resolve the promise
                deferred.resolve(syncTreeFn.apply($delegate, newArgs));                
            });

            return deferred.promise;
        }

        return $delegate;
    });
});
