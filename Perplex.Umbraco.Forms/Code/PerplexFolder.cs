using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using Umbraco.Core;

namespace PerplexUmbraco.Forms.Code
{
    public class PerplexFolder
    {
        private const string FOLDER_JSON_PATH = "~/App_Plugins/PerplexUmbracoForms/data/folders.json";

        public string id { get; set; }
        public string parentId { get; set; }
        public string name { get; set; }
        public List<string> forms { get; set; }
        public List<PerplexFolder> folders { get; set; }

        /// <summary>
        /// The path of this folder in the tree.
        /// A list of IDs starting at the root to this folder (inclusive)
        /// </summary>
        /// <returns></returns>
        public List<string> path
        {
            get
            {
                var parent = GetParent();
                // No parent? Cool, we're done
                if(parent == null) return new List<string>() { id };

                // Add our ID to the parent's path
                var path = parent.path;
                path.Add(id);
                return path;
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
                var treeService = ApplicationContext.Current.Services.ApplicationTreeService;
                var tree = treeService.GetByAlias("perplexForms");
                if(tree == null)
                {
                    throw new Exception("Perplex Forms Tree not found");
                }

                // In case of errors we will use this new empty instance
                rootFolder = new PerplexFolder
                {
                    id = "-1",
                    parentId = null,
                    name = tree.Title,
                    folders = new List<PerplexFolder>(),
                    forms = new List<string>()
                };

                var jsonFile = GetJsonFilePath();

                // Should we write rootFolder to disk, in case the JSON is corrupt or the file does not exist
                bool save = true;

                if (File.Exists(jsonFile))
                {
                    try
                    {
                        string json = File.ReadAllText(jsonFile);
                        if(!string.IsNullOrEmpty(json))
                        {
                            PerplexFolder folder = null;
                            try { folder = JsonConvert.DeserializeObject<PerplexFolder>(json); }
                            catch (JsonSerializationException) { }

                            // Only set as rootfolder if deserialization was succesful
                            if (folder != null)
                            {
                                rootFolder = folder;

                                // All is good, no need to overwrite the file with itself
                                save = false;
                            }
                        }
                    }
                    catch (Exception) { } // TODO: Handle errors?
                }

                if(save) SaveAll();
            }

            return rootFolder;
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
            folder.parentId = id;
            folders.Add(folder);
            SaveAll();
        }

        public void Move(PerplexFolder newParent)
        {
            // Remove from current parent
            PerplexFolder currentParent = GetParent();
            // Should not be possible, but just in case (root folder can never be moved)
            if (currentParent == null) return;

            // Trying to "move" to the same parent? Nope.
            if (currentParent.id == newParent.id) return;

            // Remove from current parent
            currentParent.folders.Remove(this);

            // Move to new parent
            parentId = newParent.id;
            newParent.folders.Add(this);

            SaveAll();
        }

        /// <summary>
        /// Updates this folder with all properties of the given folder, except for id.
        /// </summary>
        /// <param name="folder"></param>
        public void Update(PerplexFolder folder)
        {
            parentId = folder.parentId;
            name = folder.name;
            forms = folder.forms;
            folders = folder.folders;

            SaveAll();
        }

        /// <summary>
        /// Returns a folder based on its ID
        /// </summary>
        /// <param name="folderId"></param>
        /// <returns></returns>
        public static PerplexFolder Get(string folderId)
        {
            return GetAll().FirstOrDefault(f => f.id == folderId);
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
            return Get(parentId);
        }

        public static string GetJsonFilePath()
        {
            return HostingEnvironment.MapPath(FOLDER_JSON_PATH);
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
                parent.folders.Remove(this);
            }
        }

        public void RemoveSubFolders()
        {
            // Use a copy of our folders as the original might be modified in the loop (which will throw an exception)
            foreach (var subfolder in new List<PerplexFolder>(folders))
            {
                subfolder.Remove();
            }

            folders.Clear();
        }

        /// <summary>
        /// Returns a list of all descendant folders of this folder
        /// </summary>
        /// <returns></returns>
        public List<PerplexFolder> GetDescendantFolders()
        {
            // Start with my own subfolders (copied!)
            var descendants = new List<PerplexFolder>(folders);

            // Add my children's children, recursively
            foreach (var subFolder in folders)
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
            try { File.WriteAllText(GetJsonFilePath(), JsonConvert.SerializeObject(GetRootFolder(), Formatting.Indented)); }
            catch (IOException) { }
        }
    }
}
