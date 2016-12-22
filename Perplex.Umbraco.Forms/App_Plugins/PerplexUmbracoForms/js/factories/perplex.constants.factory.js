angular.module('umbraco')
.factory('perplexConstants', [
    function () {
        var f = {};

        f.fieldTypeIds = {
            PerplexFileUpload: '3e170f26-1fcb-4f60-b5d2-1aa2723528fd',
            PerplexImageUpload: '11fff56b-7e0e-4bfc-97ba-b5126158d33d',
            PerplexTextarea: '8c38cb28-8018-4545-b939-d1166a96b916',
            PerplexTextField: '9ead6835-57db-418b-ae2b-528f8db375a0',
            PerplexRecaptcha: '9c804aa5-d7d6-42d8-b492-2e06101987ad'
        };

        return f;
    }
]);