angular.module('umbraco')
.config(function($provide) {
    $provide.decorator('formResource', function ($delegate, $q, perplexFormResource, perplexConstants) {
        var getAllFieldTypesWithSettingsFn = $delegate.getAllFieldTypesWithSettings;

        var isArray = function(obj) {
            return Object.prototype.toString.call(obj) === '[object Array]';
        }

        $delegate.getAllFieldTypesWithSettings = function () {
            var def = $q.defer();

            getAllFieldTypesWithSettingsFn().then(function (originalResponse) {
                var originalFieldTypes = originalResponse.data;

                // We will reroute all our fields to App_Plugins\PerplexUmbracoForms instead of App_Plugins\UmbracoForms
                // This will happen in the loop a bit further down (so we do not have to iterate the Array twice)
                var perplexFieldTypeIds = _.values(perplexConstants.fieldTypeIds);

                // Filter out excluded fields
                perplexFormResource.getHideFieldTypes().then(function (response) {
                    // response is an Array of FieldTypeConfig objects (with a `Guid` field among others)
                    if (!isArray(response.data)) {
                        // If our call messes up just return the original response
                        def.reject(originalResponse);
                        return;
                    }

                    var hideFieldTypes = response.data;
                    var fieldTypes = [];

                    // Add all fields that should not be hidden
                    for (var i = 0; i < originalFieldTypes.length; i++) {
                        var originalFieldType = originalFieldTypes[i];

                        // Reroute to PerplexUmbracoForms if it's our field type
                        if (perplexFieldTypeIds.indexOf(originalFieldType.id.toLowerCase()) > -1) {
                            originalFieldType.view = originalFieldType.view.replace('UmbracoForms', 'PerplexUmbracoForms');

                            // The same goes for a custom SettingType
                            for (var s = 0; s < originalFieldType.settings.length; s++) {
                                var setting = originalFieldType.settings[s];

                                // Not very robust, but gets the job done
                                if (/\/SettingTypes\/Perplex.*\.html$/i.test(setting.view)) {
                                    setting.view = setting.view.replace('UmbracoForms', 'PerplexUmbracoForms');
                                }
                            }
                        }

                        var hide = false;
                        for (var j = 0; j < hideFieldTypes.length; j++) {
                            var hideFieldType = hideFieldTypes[j];

                            if (originalFieldType.id.toLowerCase() === hideFieldType.Guid.toLowerCase()) {
                                hide = hideFieldType.Hide;
                                break;
                            }
                        }

                        if (!hide) {
                            fieldTypes.push(originalFieldType);
                        }
                    };

                    // Replace original response data with new data
                    originalResponse.data = fieldTypes;

                    def.resolve(originalResponse);
                }, function (error) {
                    // If our call messes up just return the original response
                    def.resolve(originalResponse);
                });
            }, function (originalError) {
                def.reject(originalError);
            });

            return def.promise;
        }

        return $delegate;
    });
});
