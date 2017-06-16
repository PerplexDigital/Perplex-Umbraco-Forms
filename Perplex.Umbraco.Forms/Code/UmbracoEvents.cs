using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Forms.Mvc;
using Umbraco.Forms.Web.Controllers;
using PerplexUmbraco.Forms.Code.Configuration;
using Umbraco.Web;
using Umbraco.Forms.Data.Storage;
using System;
using static PerplexUmbraco.Forms.Code.FieldTypeHelpers;
using PerplexUmbraco.Forms.Code.Recaptcha;
using Umbraco.Forms.Core;
using static PerplexUmbraco.Forms.Code.Constants;

namespace PerplexUmbraco.Forms.Code
{
    public class UmbracoEvents : ApplicationEventHandler
    {
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            // Make sure the configuration is created if it is not there yet
            PerplexUmbracoFormsConfig.CreateIfNotExists();

            var treeService = ApplicationContext.Current.Services.ApplicationTreeService;
            if(treeService != null)
            {
                // Hide default Umbraco Forms folder, we use our own to display folders
                var umbFormTree = treeService.GetByAlias("form");
                if (umbFormTree != null && umbFormTree.Initialize)
                {
                    umbFormTree.Initialize = false;
                    treeService.SaveTree(umbFormTree);
                }

                // Add our own tree if it's not there yet
                var pplxFormTree = treeService.GetByAlias("perplexForms");
                if (pplxFormTree == null)
                {
                    treeService.MakeNew(true, 1, "forms", "perplexForms", "Forms", "icon-folder", "icon-folder-open", "PerplexUmbraco.Forms.Controllers.PerplexFormTreeController, Perplex.Umbraco.Forms");
                }
            }

            FormStorage.Created += FormStorage_Created;
            FormStorage.Deleted += FormStorage_Deleted;

            // Create perplexUmbracoUser for storage of Forms start nodes
            // if it does not exist already. There seem to be some issues with SqlServer CE,
            // it does not support some statements in this query.
            // Those will be fixed later, for now we continue
            try { Sql.ExecuteSql(Constants.SQL_CREATE_PERPLEX_USER_TABLE_IF_NOT_EXISTS); }
            catch (Exception) { }

            // ReCapatcha events
            UmbracoFormsController.FormValidate += RecaptchaValidate;
            LogHelper.Info<UmbracoEvents>("ReCaptcha FormValidate Event added to Umbraco Forms.");
        }

        void FormStorage_Created(object sender, Umbraco.Forms.Core.FormEventArgs e)
        {
            var form = e.Form;

            // Was this form created in a folder?
            var sessionId = UmbracoContext.Current.Security.GetSessionId();
            var folderId = HttpContext.Current.Cache[sessionId + "_folderId"];
            if (folderId == null) return;

            var folder = PerplexFolder.Get(folderId.ToString());
            if (folder == null) return;

            folder.Forms.Add(form.Id.ToString());
            PerplexFolder.SaveAll();
        }

        void FormStorage_Deleted(object sender, Umbraco.Forms.Core.FormEventArgs e)
        {
            // If this Form was stored in a Folder, remove it.
            var form = e.Form;
            var folder = PerplexFolder.Get(f => f.Forms.Any(fid => fid == form.Id.ToString()));
            if (folder != null)
            {
                folder.Forms.Remove(form.Id.ToString());
                PerplexFolder.SaveAll();
            }
        }

        private void RecaptchaValidate(object sender, FormValidationEventArgs e)
        {
            LogHelper.Info<UmbracoEvents>("FormValidate with ReCaptcha Running...");

            Field reCaptchaField = e.Form.AllFields.FirstOrDefault(f => f.FieldType.Id == new Guid(EnmFieldTypeId.PerplexRecaptcha.Description()));
            if (reCaptchaField == null) return;

            var httpContext = HttpContext.Current;
            var secretKey = Umbraco.Forms.Core.Configuration.GetSetting("RecaptchaPrivateKey");

            if (string.IsNullOrEmpty(secretKey))
            {
                LogHelper.Warn<UmbracoEvents>("ERROR: ReCaptcha v.2 is missing the Secret Key - Please update the '/app_plugins/umbracoforms/umbracoforms.config' to include 'key=\"RecaptchaPrivateKey\"'");
                return;
            }

            var reCaptchaResponse = httpContext.Request["g-recaptcha-response"];

            var url =
                $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={reCaptchaResponse}";

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

            if (isSuccess) return;

            // DK | 2016-11-04
            // Original code below reads actual error codes from Recaptcha.
            // We should look into this in the future if we may want to display
            // specific errors. For now, we use the configured error message
            // from the field itself or our settings configuration file.
            // Commented out for the time being.

            /*
            var compiledErrors = ",";
            foreach (var code in errorCodes)
            {
                //TODO: Use Dictionary Keys to return error message text
                compiledErrors += ", " + code;
            }
            compiledErrors = compiledErrors.Replace(",,", "");
            var errorMsg = $"Recaptcha Verification Failed: {compiledErrors}";
            */

            // Add errors to Form Model
            var controller = sender as Controller;

            if (controller == null) return;

            // Get configured error message, either from the specific form or otherwise
            // the globally configured one
            string errorMsg = GetSettingValue(
                reCaptchaField,
                nameof(PerplexRecaptcha.ErrorMessage),
                PerplexUmbracoFormsConfig.Get.PerplexRecaptchaConfig.ErrorMessage
            );

            controller.ModelState.AddModelError(reCaptchaField.Id.ToString(), errorMsg);
        }
    }
}