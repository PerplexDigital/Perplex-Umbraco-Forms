using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Web;
using System.Linq;

using Newtonsoft.Json.Linq;

using PerplexUmbraco.Forms.Code.Configuration;

using Umbraco.Core.Logging;
using Umbraco.Forms.Core;

using static PerplexUmbraco.Forms.Code.Constants;


namespace PerplexUmbraco.Forms.Code.Recaptcha
{
    public class PerplexRecaptcha : FieldType
    {
        public PerplexRecaptcha()
        {
            Id = new Guid(EnmFieldTypeId.PerplexRecaptcha.Description());
            Name = "Perplex Recaptcha";
            FieldTypeViewName = $"FieldType.{ nameof(PerplexRecaptcha) }.cshtml";
            Description = "New and improved Google Recaptcha";
            Icon = "icon-eye";
            DataType = FieldDataType.String;
            SortOrder = 21;
        }

        [Umbraco.Forms.Core.Attributes.Setting("Error message",
            description = "The error message to display when the user does not pass the Recaptcha check",
            view = "TextField")]
        public string ErrorMessage { get; set; }

        public override IEnumerable<string> ValidateField(Form form, Field field, IEnumerable<object> postedValues, HttpContextBase context)
        {
            var secretKey = Umbraco.Forms.Core.Configuration.GetSetting("RecaptchaPrivateKey");
            var recaptchaPrivateKey = ConfigurationManager.AppSettings["RecaptchaSecretKey"] ?? secretKey;

            // Get configured error message, either from this field or the XML configuration file.
            // The ErrorMessage property is empty here, for some reason.
            string fieldError = FieldTypeHelpers.GetSettingValue(field, nameof(ErrorMessage));
            string errorMsg = fieldError?.Length > 0 ? fieldError: PerplexUmbracoFormsConfig.Get.PerplexRecaptchaConfig?.ErrorMessage;            

            if (string.IsNullOrEmpty(recaptchaPrivateKey))
            {
                // just return the error message
                LogHelper.Warn<UmbracoEvents>("ERROR: ReCaptcha v.2 is missing the Secret Key - Please update the '/app_plugins/umbracoforms/umbracoforms.config' to include 'key=\"RecaptchaPrivateKey\"'");
                return new[] { errorMsg };
            }

            var reCaptchaResponse = context.Request["g-recaptcha-response"];
            var url = $"https://www.google.com/recaptcha/api/siteverify?secret={recaptchaPrivateKey}&response={reCaptchaResponse}";

            var isSuccess = false;
            var errorCodes = new List<string>();

            using (var client = new WebClient())
            {
                var response = client.DownloadString(url);

                var responseParsed = JObject.Parse(response);

                //Get Success Status
                JToken sucessToken;
                var sucessFound = responseParsed.TryGetValue("success", out sucessToken);
                if (sucessFound)
                {
                    isSuccess = sucessToken.Value<bool>();
                }

                //Get Error codes
                JToken errorsToken;
                var errorsFound = responseParsed.TryGetValue("error-codes", out errorsToken);
                if (errorsFound)
                {
                    var errorsChildren = errorsToken.Children();
                    errorCodes.AddRange(errorsChildren.Select(child => child.Value<string>()));
                }
                else
                {
                    errorCodes.Add("unknown-error");
                }
            }

            if (isSuccess)
            {
                return Enumerable.Empty<string>();
            }

            return new[] { errorMsg };
        }
    }
}