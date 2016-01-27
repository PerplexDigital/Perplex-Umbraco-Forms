using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;
using System.Net.Http.Formatting;
using Umbraco.Core;

namespace PerplexUmbraco.Forms
{
    public class MvcApplication : IApplicationEventHandler
    {
        public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            // Add the menu items to the form plugin
            TreeControllerBase.MenuRendering += TreeControllerBase_MenuRendering;
        }

        void TreeControllerBase_MenuRendering(TreeControllerBase sender, MenuRenderingEventArgs e)
        {
            // Even nog afvangen dat dit alleen op het formulier zelf gebeurt
            if (sender.TreeAlias == "form")
            {
                // If we can parse the GUID, it must be a form
                // Other types contain a guid with _workflow and _entries which do not parse as a guid
                Guid result;
                if (Guid.TryParse(e.QueryStrings.Get("id"), out result))
                {
                    var menuItem = new MenuItem("copyForm", "Copy");
                    menuItem.Icon = "icon icon-documents";
                    e.Menu.Items.Add(menuItem);

                }
            }
        }

        public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }

        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }
    }
}
