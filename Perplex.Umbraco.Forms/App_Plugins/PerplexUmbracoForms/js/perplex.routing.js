angular.module("umbraco")
.run(function ($rootScope, $route, $location, treeService) {
    var originalRouteController = null;

    $rootScope.$on('$routeChangeStart', function (event, next, current) {
        if (
            next == null ||
            next.$$route == null ||
            next.$$route.controller == null ||
            next.params.section !== 'users' ||
            next.params.tree !== 'formsecurity' ||
            next.params.method !== 'edit' ||
            next.params.id == null
        ) {
            // Do we need to restore the original controller?
            // Only do this for requests to the URL matching that controller (/:section/:tree/:method/:id)
            if (originalRouteController != null && next.params.section && next.params.tree && next.params.method && next.params.id != null) {
                next.$$route.controller = originalRouteController;
            }

            return;
        }

        // Ugliest hack in history
        // There might be a different way to change the template for the /user/formsecurity/edit URL
        // without replacing the entire route controller, but I could not find one after extensive experimentation.
        // The problem is, Umbraco defined the route /:section/:tree/:method/:id which matches
        // the URL we want to manipulate already (obviously). So while it is possible to add a new route to the routeProvider
        // with some workarounds, the existing one will be tested first to the URL and will then be used.
        // We therefore have to adjust the route that matches the request (next.$$route) directly.
        // The /:section/:tree/:method/:id route specifically uses $scope.templateUrl to define the template,
        // so this seems to be the only real way to change the template of all URLs that match the route.
        // Since manipulating this controller will affect all future requests to
        // /:section/:tree/:method/:id, we have to save the original controller and restore it
        // on future route changes. It's a very ugly way of doing things, but it does work so let's not get too worked up about it.
        if (originalRouteController == null) {
            originalRouteController = next.$$route.controller;
        }

        // Redirect to our edit.html
        next.$$route.controller = function ($scope) {
            $scope.templateUrl = '/App_Plugins/PerplexUmbracoForms/backoffice/FormSecurity/edit.html';
        };
    });
});
