using System;
using System.Linq;
using System.Web;

using PerplexUmbraco.Forms.Code.Configuration;

using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Forms.Core;
using Umbraco.Forms.Data.Storage;
using Umbraco.Web;

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
            LogHelper.Info<UmbracoEvents>("ReCaptcha FormValidate Event added to Umbraco Forms.");
        }

        void FormStorage_Created(object sender, FormEventArgs e)
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

            ClearFormsCache(folderId.ToString());
        }

        void FormStorage_Deleted(object sender, FormEventArgs e)
        {
            // If this Form was stored in a Folder, remove it.
            var form = e.Form;
            var folder = PerplexFolder.Get(f => f.Forms.Any(fid => fid == form.Id.ToString()));
            if (folder != null)
            {
                folder.Forms.Remove(form.Id.ToString());
                PerplexFolder.SaveAll();

                ClearFormsCache(folder.Id);
            }
        }

        void ClearFormsCache(string folderId)
        {
            var cacheConfig = PerplexUmbracoFormsConfig.Get.PerplexCacheConfig;

            if (cacheConfig.EnableCache)
            {
                var cacheKey = $"PerplexFormTreeController_GetTreeNodes_id:{folderId}";
                var rtCache = ApplicationContext.Current.ApplicationCache.RuntimeCache;

                if (rtCache.GetCacheItemsByKeySearch(cacheKey).Any())
                    rtCache.ClearCacheByKeySearch(cacheKey);
            }
        }
    }
}
