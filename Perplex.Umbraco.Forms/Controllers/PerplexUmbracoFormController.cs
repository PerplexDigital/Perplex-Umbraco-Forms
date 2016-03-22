using Newtonsoft.Json;
using PerplexUmbraco.Forms.Code;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Hosting;
using System.Web.Http;
using Umbraco.Core.IO;
using Umbraco.Forms.Core.Providers;
using Umbraco.Forms.Data.Storage;
using Umbraco.Forms.Mvc.Models.Backoffice;
using Umbraco.Forms.Web.Models.Backoffice;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace PerplexUmbraco.Forms.Controllers
{
    public class PerplexUmbracoFormController : UmbracoAuthorizedApiController
    {
        [HttpPost]
        public HttpResponseMessage CopyByGuid(Guid guid)
        {
            using (FormStorage formStorage = new FormStorage())
            {
                using (WorkflowStorage workflowStorage = new WorkflowStorage())
                {
                    Umbraco.Forms.Core.Form form = formStorage.GetForm(guid);

                    if (form == null)
                        return Request.CreateResponse(HttpStatusCode.NotFound);

                    // Get the corresponding workflows
                    List<Umbraco.Forms.Core.Workflow> workflows = new List<Umbraco.Forms.Core.Workflow>();
                    foreach (var workflowId in form.WorkflowIds)
                    {
                        workflows.Add(workflowStorage.GetWorkflow(workflowId));
                    }

                    // Clone the form, manual copy because the clone function implemented by Umbraco doesn't work (not serializable)
                    var newForm = new Umbraco.Forms.Core.Form();
                    newForm.Pages = form.Pages.ToList();
                    newForm.DataSource = form.DataSource;
                    newForm.DisableDefaultStylesheet = form.DisableDefaultStylesheet;
                    newForm.FieldIndicationType = form.FieldIndicationType;
                    newForm.GoToPageOnSubmit = form.GoToPageOnSubmit;
                    newForm.HideFieldValidation = form.HideFieldValidation;
                    newForm.Indicator = form.Indicator;
                    newForm.InvalidErrorMessage = form.InvalidErrorMessage;
                    newForm.ManualApproval = form.ManualApproval;
                    newForm.MessageOnSubmit = form.MessageOnSubmit;
                    newForm.Name = form.Name + " - copy - " + DateTime.Now.ToString("dd-MM-yyyy HH:mm");
                    newForm.NextLabel = form.NextLabel;
                    newForm.PrevLabel = form.PrevLabel;
                    newForm.RequiredErrorMessage = form.RequiredErrorMessage;
                    newForm.ShowValidationSummary = form.ShowValidationSummary;
                    newForm.StoreRecordsLocally = form.StoreRecordsLocally;
                    newForm.SubmitLabel = form.SubmitLabel;
                    newForm.SupportedDependencies = form.SupportedDependencies;
                    newForm.UseClientDependency = form.UseClientDependency;
                    newForm.WorkflowIds = new List<Guid>();
                    newForm.XPathOnSubmit = form.XPathOnSubmit;
                    newForm.CssClass = form.CssClass;

                    var submittedForm = formStorage.InsertForm(newForm);
                    if (submittedForm != null)
                    {
                        // Clear the default workflowId
                        submittedForm.WorkflowIds = new List<Guid>();
                        // Save
                        formStorage.UpdateForm(submittedForm);

                        // Create copies of the workflows
                        foreach (var workflow in workflows)
                        {
                            var newWorkflow = new Umbraco.Forms.Core.Workflow();
                            newWorkflow.Active = workflow.Active;
                            newWorkflow.ExecutesOn = workflow.ExecutesOn;
                            newWorkflow.Form = submittedForm.Id;
                            newWorkflow.Name = workflow.Name;
                            // Copy so we have no reference! - http://stackoverflow.com/a/8859151/2992405
                            newWorkflow.Settings = new Dictionary<string, string>(workflow.Settings);
                            newWorkflow.SortOrder = workflow.SortOrder;
                            newWorkflow.Type = workflow.Type;
                            newWorkflow.WorkflowTypeId = workflow.WorkflowTypeId;

                            // Save the new workflow
                            workflowStorage.InsertWorkflow(submittedForm, newWorkflow);
                        }

                        // Put the form in the same folder as the original
                        var folder = PerplexFolder.Get(f => f.forms.Any(formId => formId == guid.ToString()));
                        if (folder != null)
                        {
                            folder.forms.Add(newForm.Id.ToString());
                            PerplexFolder.SaveAll();

                            // Return the folder so we can expand the tree again
                            return Request.CreateResponse(HttpStatusCode.OK, folder);
                        }

                        return Request.CreateResponse(HttpStatusCode.OK);
                    }


                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
            }
        }

        [HttpPost]
        public HttpResponseMessage CreateFolder(string parentId, string name)
        {
            if (PerplexFolder.Get(parentId) == null)
                return new HttpResponseMessage(HttpStatusCode.NotFound) { ReasonPhrase = "Parent folder with id " + parentId + " not found" };

            // Add folder
            var folder = new PerplexFolder
            {
                id = Guid.NewGuid().ToString(),
                parentId = parentId,
                // Do not allow a name as null, transform that to empty string instead (UI related, would display as "null" otherwise).
                // If users want an empty folder name, so be it, so we are not going to prevent them from inputting an empty name.
                name = name == null ? "" : name,
                forms = new List<string>(),
                folders = new List<PerplexFolder>()
            };

            PerplexFolder.Add(folder, parentId);

            return Request.CreateResponse(HttpStatusCode.OK, folder);
        }

        [HttpPost]
        public HttpResponseMessage Update(PerplexFolder folder)
        {
            // Update the folder
            var folderToUpdate = PerplexFolder.Get(folder.id);
            if(folderToUpdate == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound) { ReasonPhrase = "Folder with id " + folder.id + " not found" };
            }

            folderToUpdate.Update(folder);

            // Return the updated folder
            return Request.CreateResponse(HttpStatusCode.OK, folderToUpdate);
        }

        [HttpPost]
        public HttpResponseMessage MoveForm(string formId, string folderId)
        {
            var newFolder = PerplexFolder.Get(folderId);

            if (newFolder == null) return new HttpResponseMessage(HttpStatusCode.NotFound) { ReasonPhrase = "Folder with id " + folderId + " not found." };

            // If form was contained in another folder (should be the case),
            // remove it from there
            var oldFolder = PerplexFolder.Get(f => f.forms.Any(ff => ff == formId));
            if (oldFolder != null)
            {
                oldFolder.forms.Remove(formId);
            }

            // Add form to new folder
            newFolder.forms.Add(formId);

            // Save to disk
            PerplexFolder.SaveAll();

            // Respond with the new parent folder, so the UI can move to that new location
            return Request.CreateResponse(HttpStatusCode.OK, newFolder);
        }

        /// <summary>
        /// Moves a folder to a new location (i.e., changes parent folder)
        /// </summary>
        /// <param name="id">Id of the folder to move</param>
        /// <param name="folderId">Id of the destination folder</param>
        /// <returns>Object with oldFolder and newFolder properties.</returns>
        [HttpPost]
        public HttpResponseMessage MoveFolder(string id, string folderId)
        {
            // The folder being moved
            var folder = PerplexFolder.Get(id);

            if (folder == null) return new HttpResponseMessage(HttpStatusCode.NotFound) { ReasonPhrase = "Folder to be moved with id " + id + " not found." };

            // Destination folder
            var newFolder = PerplexFolder.Get(folderId);

            // Destination folder not found
            if (newFolder == null) return new HttpResponseMessage(HttpStatusCode.NotFound) { ReasonPhrase = "Destination folder with id " + folderId + " not found." };

            folder.Move(newFolder);

            // Respond with the moved folder, so the UI can move to that new location in the tree
            return Request.CreateResponse(HttpStatusCode.OK, folder);
        }

        [HttpGet]
        public PerplexFolder GetFolder(string folderId)
        {
            return PerplexFolder.Get(folderId);
        }

        /// <summary>
        /// Returns the root folder
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public PerplexFolder GetRootFolder()
        {
            return PerplexFolder.GetRootFolder();
        }

        [HttpPost]
        public HttpResponseMessage DeleteFolder(string folderId, bool deleteForms)
        {
            var folder = PerplexFolder.Get(folderId);
            if (folder == null) return new HttpResponseMessage(HttpStatusCode.NotFound) { ReasonPhrase = "Folder with id " + folderId + " not found." };

            // Delete forms if requested, of this folder and all descendant folders
            if (deleteForms)
            {
                using (FormStorage formStorage = new FormStorage())
                {
                    var formIds = new List<string>(folder.forms);
                    formIds.AddRange(folder.GetDescendantFolders().SelectMany(f => f.forms));

                    // Forms of this folder
                    foreach (string formId in formIds)
                    {
                        Guid guid;
                        if (!Guid.TryParse(formId, out guid)) continue;

                        var form = formStorage.GetForm(guid);
                        if(form != null)
                        {
                            formStorage.DeleteForm(form);
                        }
                    }
                }
            }

            PerplexFolder parentFolder = folder.GetParent();

            // Remove this folder
            folder.Remove();

            // Save to disk
            PerplexFolder.SaveAll();

            // Respond with the parent folder, so we can refresh it
            return Request.CreateResponse(HttpStatusCode.OK, parentFolder);
        }
    }
}
