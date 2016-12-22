angular.module('umbraco')
.directive('perplexFormsStartNodes', function() {
    return {
        restrict: 'E',
        templateUrl: '/App_Plugins/PerplexUmbracoForms/views/formsStartNodes.html',
        replace: true
    };
});