using Newtonsoft.Json;
using PerplexUmbraco.Forms.Code;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using Umbraco.Forms.Data;
using Umbraco.Forms.Web.Trees;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using Umbraco.Web;
using umbraco.BusinessLogic.Actions;
using umbraco;

namespace PerplexUmbraco.Forms.Controllers
{
    /// <summary>
    /// This tree is not actually used (or even exists), but is only here to allow us to route requests to the App_Plugins/PerplexUmbracoForms folder,
    /// using the URL syntax #/forms/perplexForms/...
    /// This appears to only be possible when a matching PluginController and Tree have been defined with the right name.
    /// </summary>
    [PluginController("PerplexUmbracoForms")]
    [Tree("forms", "perplexForms", "", "icon-folder", "icon-folder-open", false, 0)]
    public class PerplexDummyController : FormTreeController { }

    /// <summary>
    /// These decorations purposely are the default Umbraco Forms definitions.
    /// Without these, the default form behavior does not work anymore, which we obviously still need.
    /// This is also why we need to define the empty controller above with our own names.
    /// </summary>
    [PluginController("UmbracoForms")]
    [Tree("forms", "form", "Forms", "icon-folder", "icon-folder-open", true, 0)]
    public class PerplexFormTreeController : FormTreeController
    {
        // We load our custom menu actions from our own folder
        private const string VIEWS_ROOT = "/App_Plugins/PerplexUmbracoForms/views/";

        public PerplexFormTreeController() { }

        protected override Umbraco.Web.Models.Trees.TreeNodeCollection GetTreeNodes(string id, System.Net.Http.Formatting.FormDataCollection queryStrings)
        {
            // If this is a form, use Umbraco's default behavior
            var folder = PerplexFolder.Get(id);
            if (folder == null)
            {
                return base.GetTreeNodes(id, queryStrings);
            }

            // This is a folder

            // We require all forms, and apply filtering based on folders later
            var baseTreeNodes = base.GetTreeNodes("-1", queryStrings);

            // Sanity check; make sure there are no orphan forms around
            // (forms not contained within any folder). If so, move them to the root folder
            var orphans = baseTreeNodes.Where(n => PerplexFolder.Get(f => f.Forms.Any(formId => formId == n.Id.ToString())) == null).ToList();
            if(orphans.Count > 0)
            {
                foreach (var orphan in orphans)
                {
                    PerplexFolder.GetRootFolder().Forms.Add(orphan.Id.ToString());
                }

                PerplexFolder.SaveAll();
            }

            // Hide all Forms that are not contained within this folder
            // If this folder itself is disabled (due to the user not having access),
            // we also hide all its forms
            baseTreeNodes.RemoveAll(n => 
                !folder.Forms.Contains(n.Id.ToString()) ||
                (folder.Disabled && folder.Forms.Contains(n.Id.ToString()))
            );

            // Sort the forms of this folder in the order as defined by the folder
            baseTreeNodes.Sort((x, y) =>
            {
                int idxX, idxY;

                idxX = folder.Forms.FindIndex(0, s => s == x.Id.ToString());
                idxY = folder.Forms.FindIndex(0, s => s == y.Id.ToString());

                return idxX.CompareTo(idxY);
            });

            // Add any subfolders of this node
            // We loop through the list in reverse as we add every folder at the start of the list (before forms)
            foreach (var subFolder in folder.Folders.Reverse<PerplexFolder>())
            {
                // If this subfolder is disabled, and it is not on a path towards
                // a folder that is NOT disabled, it should not be listed at all.
                // When multiple start nodes are defined, it is possible for a disabled
                // folder to be displayed in the tree, when one of its descendant folders is enabled.
                if (subFolder.Disabled)
                {
                    var startFolders = PerplexFolder.GetStartFoldersForCurrentUser();

                    bool isOnPathTowardsStartFolder = startFolders.Any(sf => sf.Path.Any(fid => fid == subFolder.Id));
                    if (!isOnPathTowardsStartFolder)
                    {
                        continue;
                    }
                }

                var treeNode = CreateTreeNode(subFolder.Id, id, queryStrings, subFolder.Name);

                // Clicking this folder will show the folder overview
                // By default all nodes go to /forms/form/edit/<GUID>, but this
                // is only valid for forms. We direct to our custom folder view
                treeNode.RoutePath = "forms/perplexForms/folder/" + treeNode.Id;
                if (subFolder.Disabled)
                {
                    treeNode.CssClasses.Add("disabled");
                }

                // Folder has children if it has either forms or folders.
                // If it is disabled, this is only true when it has subfolders 
                // since we do not show its forms.
                treeNode.HasChildren = (subFolder.Disabled && subFolder.Folders.Any()) || (!subFolder.Disabled && (subFolder.Forms.Any() || subFolder.Folders.Any()));

                // Folders are added at the top of the list, before forms
                baseTreeNodes.Insert(0, treeNode);
            }

            return baseTreeNodes;
        }

        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            // Get the start folders of this user. 
            var startFolders = PerplexFolder.GetStartFoldersForCurrentUser();

            // If none are set, all folders are allowed so we just use default behavior
            // Likewise if the common ancestors of all allowed folders is the root.
            PerplexFolder commonAncestor = PerplexFolder.GetCommonAncestor(startFolders);            

            if (!startFolders.Any() || commonAncestor == PerplexFolder.GetRootFolder())
            {
                return base.CreateRootNode(queryStrings);
            }

            // At this point the root folder for this user is different from the regular root,
            // so let's create it.
            var rootNode = CreateTreeNode(commonAncestor.Id, commonAncestor.ParentId, queryStrings, commonAncestor.Name);

            // Clicking this folder will show the folder overview
            // By default all nodes go to /forms/form/edit/<GUID>, but this
            // is only valid for forms. We direct to our custom folder view
            // It is possible this root node is disabled for the current user,
            // if this is the common ancestor of this user's start nodes but not
            // a start node itself. In that case it should also show as disabled in
            // the UI, and we hide its URL.
            rootNode.RoutePath = "forms/perplexForms/folder/" + commonAncestor.Id;    
            if(commonAncestor.Disabled)
            {
                rootNode.CssClasses.Add("disabled");
            }   

            // Folder has children if it has either forms or folders
            rootNode.HasChildren = commonAncestor.Forms.Any() || commonAncestor.Folders.Any();

            return rootNode;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            // Default behavior
            var menu = base.GetMenuForNode(id, queryStrings);

            // Remove Umbraco Copy
            menu.Items.RemoveAll(m => m.Alias == "copy");

            // Clear any stored folderId
            var sessionId = UmbracoContext.Security.GetSessionId();
            HttpContext.Current.Cache.Remove(sessionId + "_folderId");

            // If we can parse the GUID, it must be a form (or a folder)
            // Other types contain a guid with _workflow and _entries which do not parse as a guid
            Guid result;
            if (Guid.TryParse(queryStrings.Get("id"), out result))
            {
                // Is this a Folder?
                var folder = PerplexFolder.Get(result.ToString());

                if (folder != null)
                {
                    // If the folder is disabled, we will remove all entries from the menu,
                    // except for "Reload" (it's harmless)                    
                    if (folder.Disabled)
                    {
                        menu.Items.RemoveAll(m => m.Alias != ActionRefresh.Instance.Alias);
                        return menu;
                    }                   

                    // Create Form (default Umbraco view, hence alias)
                    AddMenuItem(menu, "Create Form", alias:  "create", icon: "icon icon-add");

                    // Create Folder
                    AddMenuItem(menu, "Create Folder", view: "createFolder", icon: "icon icon-folder");

                    // Move Folder
                    AddMenuItem(menu, "Move Folder", view: "moveFolder", icon: "icon icon-enter");

                    // Remove existing Delete (works only on a Form)
                    menu.Items.RemoveAll(m => m.Alias == "delete");

                    // Delete Folder
                    AddMenuItem(menu, "Delete Folder", view: "deleteFolder", icon: "icon icon-delete");

                    // Sort Folder
                    AddMenuItem(menu, "Sort", view: "sort", icon: "icon icon-navigation-vertical");

                    // Reload
                    menu.Items.Add<RefreshNode, ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);

                    // We store this folder's ID in our session in case a new form is created
                    HttpContext.Current.Cache[sessionId + "_folderId"] = folder.Id;
                }
                // This is a form
                else
                {
                    // Copy Form
                    AddMenuItem(menu, "Copy", view: "copyForm", icon: "icon icon-documents");

                    // Move Form
                    AddMenuItem(menu, "Move", view: "moveForm", icon: "icon icon-enter");
                }
            }
            else
            {
                // This is the root folder
                var root = PerplexFolder.GetRootFolder();

                // If the root folder is disabled, remove all menu actions except Reload
                if(root.Disabled)
                {
                    menu.Items.RemoveAll(m => m.Alias != ActionRefresh.Instance.Alias);
                    return menu;
                }

                // Add Create Folder button to root
                AddMenuItem(menu, "Create Folder", view: "createFolder", icon: "icon icon-folder");

                // Sort Folder
                AddMenuItem(menu, "Sort", view: "sort", icon: "icon icon-navigation-vertical");

                // We store this folder's ID in our session in case a new form is created
                HttpContext.Current.Cache[sessionId + "_folderId"] = "-1";
            }

            return menu;
        }

        private void AddMenuItem(MenuItemCollection menu, string name, string alias = null, string icon = null, string view = null)
        {
            var menuItem = new MenuItem { Name = name };

            if (alias != null)
            {
                menuItem.Alias = alias;
            }

            if (icon != null)
            {
                menuItem.Icon = icon;
            }

            if (view != null)
            {
                menuItem.LaunchDialogView(VIEWS_ROOT + view + ".html", name);
            }

            menu.Items.Add(menuItem);
        }
    }
}