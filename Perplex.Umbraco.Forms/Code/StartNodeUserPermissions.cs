using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models.Membership;
using Umbraco.Web;

namespace PerplexUmbraco.Forms.Code
{
    public static class StartNodeUserPermissions
    {
        /// <summary>
        /// Applies Form Start Node permissions to collection of users
        /// </summary>
        /// <param name="users">Collection of users to iterate over and apply permissions to</param>
        /// <param name="folder">Perplex folder to use for assigning access</param>
        /// <remarks>
        /// In order for this to assign the user permissions to the Perplex Folder Start Node, then the 
        /// user must have a matching Start Content Node name that matches the Perplex Folder name. 
        /// Ex. User has been assigned a Start Content Node of "Testing"; a Perplex folder is created
        /// with the same name "Testing" so the user will be automatically assigned this folder as a 
        /// Start Form Node within Perplex
        /// </remarks>
        public static void ApplyFolderPermissionsToUsers(IEnumerable<IUser> users, PerplexFolder folder)
        {
            // iterate over each user
            foreach (var user in users)
            {
                // get the user's start content nodes
                List<dynamic> userStartNodes = GetStartContentNodesByUser(user);

                // iterate over the user's start content nodes
                foreach (var node in userStartNodes)
                {
                    // check to see if the name of the perplex folder matches the name of the user's content start node
                    if (folder.Name.Trim().ToLower() == node.Name.ToString().Trim().ToLower())
                    {
                        // add the folder to the list of user's Form Start Nodes
                        // delete any association between a user and this folder being deleted
                        try { Sql.ExecuteSql("INSERT INTO [perplexUmbracoUser] (userId, formsStartNode) VALUES (@userId, @formsStartNode)", parameters: new { userId = user.Id, formsStartNode = folder.Id }); }
                        catch (Exception) { }
                    }
                }
            }           
        }

        /// <summary>
        /// Deletes the assocation of the form being deleted with any users in the perplexUmbracoUser table
        /// </summary>
        /// <param name="folderId">GUID of the folder being deleted</param>
        public static void DeleteUserAssociationWithFolder(string folderId)
        {
            try { Sql.ExecuteSql("DELETE FROM [perplexUmbracoUser] WHERE formsStartNode = @formsStartNode", parameters: new { formsStartNode = folderId }); }
            catch (Exception) { }
        }

        /// <summary>
        /// Gets all the content start nodes associated with a specific IUser
        /// </summary>
        /// <param name="user">IUser account to retrieve content start nodes</param>
        /// <returns></returns>
        public static List<dynamic> GetStartContentNodesByUser(IUser user)
        {
            Umbraco.Web.UmbracoHelper helper = new UmbracoHelper(UmbracoContext.Current);
            List<dynamic> contentNodes = new List<dynamic>();

            foreach (int id in user.StartContentIds)
            {
                var node = helper.ContentQuery.Content(id);

                // only add this node to the collection if it's of type www_section
                if (node.DocumentTypeAlias.ToLower() == "www_section")
                {
                    // add the content node to the collection
                    contentNodes.Add(node);
                }
            }

            return contentNodes;
        }
    }
}
