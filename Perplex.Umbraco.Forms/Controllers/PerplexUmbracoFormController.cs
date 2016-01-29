using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
    public class PerplexUmbracoFormController : Umbraco.Web.WebApi.UmbracoAuthorizedApiController
    {
        [System.Web.Http.HttpPost]
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
                    foreach(var workflowId in form.WorkflowIds){
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
                        foreach(var workflow in workflows){
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

                        return Request.CreateResponse(HttpStatusCode.OK);
                    }


                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
            }
        }
    }
}
