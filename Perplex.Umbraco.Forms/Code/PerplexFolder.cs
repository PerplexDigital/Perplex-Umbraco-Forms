using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Forms.Data;
using Umbraco.Web;

namespace PerplexUmbraco.Forms.Code
{
    public class PerplexFolder
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "parentId")]
        public string ParentId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "forms")]
        public IList<string> Forms { get; set; } = new List<string>();

        [JsonProperty(PropertyName = "folders")]
        public IList<PerplexFolder> Folders { get; set; } = new List<PerplexFolder>();

        /// <summary>
        /// The path of this folder in the tree.
        /// A list of IDs starting at the root to this folder (inclusive)
        /// </summary>
        /// <returns></returns>
        [JsonProperty(PropertyName = "path")]
        public List<string> Path
        {
            get
            {
                // A deserialize triggers this property too,
                // but when we are *in* the deserialization process
                // this cannot succeed since rootFolder isn't set yet
                // This can be fixed with another ContractResolver (to just skip these dynamic getter-only properties)
                // which will be implemented later.
                if (rootFolder == null) return new List<string>();

                var parent = GetParent();
                // No parent? Cool, we're done
                if(parent == null) return new List<string>() { Id };

                // Add our ID to the parent's path
                var path = parent.Path;
                path.Add(Id);
                return path;
            }
        }

        /// <summary>
        /// The path, but relative to the Forms Root Node.
        /// When no Start Nodes have been set for the current user, this
        /// will always be the same as Path. If Start Nodes have been set,
        /// it might be different, depending on which Start Nodes are set.
        /// This is always a subset of Path.
        /// </summary>
        [JsonProperty(PropertyName = "relativePath")]
        public List<string> RelativePath
        {
            get
            {
                // A deserialize triggers this property too,
                // but when we are *in* the deserialization process
                // this cannot succeed since rootFolder isn't set yet
                // This can be fixed with another ContractResolver (to just skip these dynamic getter-only properties)
                // which might be implemented later.
                if (rootFolder == null) return new List<string>();

                List<PerplexFolder> startFolders = GetStartFoldersForCurrentUser();

                if(!startFolders.Any())
                {
                    return Path;
                }

                var commonAncestor = GetCommonAncestor(startFolders);

                return Path.SkipWhile(fId => fId != commonAncestor.Id).ToList();
            }
        }

        /// <summary>
        /// If the current user does not have access to this folder,
        /// it will be disabled.
        /// This property should not actually be serialized to disk
        /// since it is different per user, but setting [JsonIgnore] will also
        /// cause the API controllers to not return this property anymore (which makes sense),
        /// but we do use it for the UI so we will just serialize it anyway.
        /// </summary>
        [JsonProperty(PropertyName = "disabled")]
        public bool Disabled
        {
            get
            {
                // Determine this folder's startfolders
                var startFolders = GetStartFoldersForCurrentUser();

                // If no start folders are defined, everything is accessible
                if (!startFolders.Any())
                    return false;

                // Otherwise, this folder is accessible only if
                // it is a startfolder or a subfolder of a startfolder
                // First check if this folder is a start folder itself
                if (startFolders.Any(f => f == this))
                    return false;

                // Then check if this folder is a descendant of the startfolder.
                // Path contains all ancestors of this folder, so we use that instead of
                // looking at all descendant folders of the start folders, which is way more inefficient
                return !Path.Any(folderId => startFolders.Any(f => f.Id == folderId));
            }
        }

        private static PerplexFolder rootFolder;
        /// <summary>
        /// Returns the root folder.
        /// Any other folders are nested inside the root folder.
        /// </summary>
        public static PerplexFolder GetRootFolder()
        {
            if (rootFolder == null)
            {
                var jsonFile = GetFilePath();

                if (File.Exists(jsonFile))
                {
                    try
                    {
                        string json = File.ReadAllText(jsonFile);
                        if (!string.IsNullOrEmpty(json))
                        {
                            PerplexFolder folder = null;
                            try { folder = JsonConvert.DeserializeObject<PerplexFolder>(json); }
                            catch (JsonSerializationException) { }

                            // Only set as rootfolder if deserialization was succesful
                            if (folder != null)
                            {
                                rootFolder = folder;
                            }
                        }
                    }
                    catch (Exception) { } // TODO: Handle errors?
                }

                // Something went wrong retrieving the rootFolder
                // Create a new instance and write it to disk.
                // We also create a new root folder when the Id is missing.
                // This can occur when the JSON was simply "{}" which will deserialize properly 
                // but will yield a completely empty PerplexFolder with no data whatsoever, whereas
                // an Id is required to function properly.
                if (rootFolder == null || string.IsNullOrEmpty(rootFolder.Id))
                {
                    rootFolder = CreateNewRoot();
                    SaveAll();
                }
            }

            return rootFolder;
        }

        /// <summary>
        /// Creates a new, empty root folder.
        /// This can be used when there has not been any data found
        /// </summary>
        /// <returns></returns>
        public static PerplexFolder CreateNewRoot()
        {
            var treeService = ApplicationContext.Current.Services.ApplicationTreeService;
            var tree = treeService.GetByAlias("perplexForms");
            if (tree == null)
            {
                throw new Exception("Perplex Forms Tree not found");
            }

            // A new, empty root folder
            return new PerplexFolder
            {
                Id = "-1",
                ParentId = null,
                Name = tree.Title,
                Folders = new List<PerplexFolder>(),
                Forms = new List<string>()
            };
        }

        /// <summary>
        /// Returns all folders as a flat list
        /// </summary>
        /// <returns></returns>
        public static List<PerplexFolder> GetAll()
        {
            // Start with the root folder
            var rootFolder = GetRootFolder();

            // As a list
            var all = new List<PerplexFolder>() { rootFolder };

            // And add all descendant folders to the list
            all.AddRange(rootFolder.GetDescendantFolders());

            return all;
        }

        /// <summary>
        /// Adds a new folder
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="parentId"></param>
        public static void Add(PerplexFolder folder, string parentId)
        {
            var parentFolder = Get(parentId);
            if (parentFolder == null)
            {
                throw new ArgumentException("Folder with parentId " + parentId + " not found.", "parentId");
            }

            parentFolder.AddFolder(folder);
        }

        public void AddFolder(PerplexFolder folder)
        {
            folder.ParentId = Id;
            Folders.Add(folder);
            SaveAll();
        }

        public void Move(PerplexFolder newParent)
        {
            // Remove from current parent
            PerplexFolder currentParent = GetParent();
            // Should not be possible, but just in case (root folder can never be moved)
            if (currentParent == null) return;

            // Trying to "move" to the same parent? Nope.
            if (currentParent.Id == newParent.Id) return;

            // Remove from current parent
            currentParent.Folders.Remove(this);

            // Move to new parent
            ParentId = newParent.Id;
            newParent.Folders.Add(this);

            SaveAll();
        }

        /// <summary>
        /// Updates this folder with all properties of the given folder, except for id.
        /// </summary>
        /// <param name="folder"></param>
        public void Update(PerplexFolder folder)
        {
            // Disabled folders cannot be updated
            if (Disabled) return;

            ParentId = folder.ParentId;
            Name = folder.Name;
            Forms = folder.Forms;
            Folders = folder.Folders;

            SaveAll();
        }

        /// <summary>
        /// Returns a folder based on its ID
        /// </summary>
        /// <param name="folderId"></param>
        /// <returns></returns>
        public static PerplexFolder Get(string folderId)
        {
            return GetAll().FirstOrDefault(f => f.Id == folderId);
        }

        /// <summary>
        /// Returns the first folder that matches the given predicate, or null
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static PerplexFolder Get(Func<PerplexFolder, bool> predicate)
        {
            return GetAll().FirstOrDefault(predicate);
        }

        public PerplexFolder GetParent()
        {
            return Get(ParentId);
        }

        public static string GetFilePath()
        {
            return HostingEnvironment.MapPath(Constants.FOLDERS_DATA_FILE_PATH);
        }

        /// <summary>
        /// Removes this folder and all subfolders
        /// </summary>
        public void Remove()
        {
            // First remove all subfolders
            RemoveSubFolders();

            // Then remove ourself from our parent's folders
            var parent = GetParent();
            if(parent != null)
            {
                parent.Folders.Remove(this);
            }
        }

        public void RemoveSubFolders()
        {
            // Use a copy of our folders as the original might be modified in the loop (which will throw an exception)
            foreach (var subfolder in new List<PerplexFolder>(Folders))
            {
                subfolder.Remove();
            }

            Folders.Clear();
        }

        /// <summary>
        /// Returns a list of all descendant folders of this folder
        /// </summary>
        /// <returns></returns>
        public List<PerplexFolder> GetDescendantFolders()
        {
            // Start with my own subfolders (copied!)
            var descendants = new List<PerplexFolder>(Folders);

            // Add my children's children, recursively
            foreach (var subFolder in Folders)
            {
                descendants.AddRange(subFolder.GetDescendantFolders());
            }

            return descendants;
        }

        /// <summary>
        /// Saves the configuration of all folders to disk
        /// </summary>
        public static void SaveAll()
        {
            // It's theoretically possible to trigger this method multiple times before writing of a previous call is finished,
            // triggering an I/O exception when the file is still locked for writing.
            // We ignore that error.
            try {
                var filePath = GetFilePath();
                var directory = System.IO.Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(filePath, JsonConvert.SerializeObject(GetRootFolder(), Formatting.Indented)); }
            catch (IOException) { }
        }

        /// <summary>
        /// Returns the first common ancestor of the list of folders
        /// </summary>
        /// <param name="folders"></param>
        /// <returns></returns>
        public static PerplexFolder GetCommonAncestor(List<PerplexFolder> folders)
        {
            if (!folders.Any())
                return null;

            // The common ancestor is found by simply taking the intersection of all folder paths
            // If the resulting list contains more than 1 folder, we take the last one (= deepest in the tree).
            var folderPaths = folders.Select(folder => folder.Path);

            // This list will always be at least of length 1, as all folders share at least the root folder.
            var commonAncestors = folderPaths.Aggregate((intersected, list) => intersected.Intersect(list).ToList());

            return PerplexFolder.Get(commonAncestors.Last());
        }

        /// <summary>
        /// In the User section we can specify the start folders for a given user.
        /// Those folders and subfolders are accessible, others are not.
        /// This returns the list of folders set as start folders for the current backoffice user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<PerplexFolder> GetStartFoldersForCurrentUser()
        {
            var user = UmbracoContext.Current.Security.CurrentUser;
            return GetStartFoldersForUser(user);
        }

        /// <summary>
        /// Returns the start folders for the given user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static List<PerplexFolder> GetStartFoldersForUser(IUser user)
        {
            if (user == null)
                return new List<PerplexFolder>();

            // We cache the result by user id,
            // to prevent unnecessary SQL queries
            string cacheKey = "_Perplex_StartFolders_" + user.Id;

            List<PerplexFolder> folders = HttpContext.Current.Cache[cacheKey] as List<PerplexFolder>;

            if (folders == null)
            {
                try
                {
                    folders = new List<PerplexFolder>();

                    var records = Sql.CreateSqlDataEnumerator(
                        "SELECT formsStartNode FROM [perplexUmbracoUser] WHERE userId = @userId",
                        System.Data.CommandType.Text,
                        new { userId = user.Id });

                    foreach(IDataRecord record in records)
                    {
                        string folderId = record["formsStartNode"].ToString();
                        PerplexFolder folder = PerplexFolder.Get(folderId);
                        if (folder != null)
                        {
                            folders.Add(folder);
                        }
                    }
                }
                catch (Exception) { folders = new List<PerplexFolder>(); }

                HttpContext.Current.Cache[cacheKey] = folders;
            }

            return folders;
        }
    }
}
