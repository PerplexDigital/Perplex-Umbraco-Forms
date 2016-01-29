using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Attributes;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Data.Storage;

namespace PerplexUmbraco.Forms.Workflows
{
    public class DeleteOnSubmitWorkflow : Umbraco.Forms.Core.WorkflowType
    {
        public DeleteOnSubmitWorkflow()
        {
            this.Name = "Delete on submit";
            this.Id = new Guid("65541c70-5e23-4cd9-a855-17c5f6a33631");
            this.Description = "Deletes the form entry and files on submit, this is irreversible";
        }


        [Setting("Do not delete files on submit", description = "Default files are deleted on submit (when using this workflow). By checking this checkbox the file remains on disk.", view = "Checkbox")]
        public string DoNotDeleteFilesOnSubmit { get; set; }

        Guid[] listOfUploadTypes = {
                                    new Guid("11fff56b-7e0e-4bfc-97ba-b5126158d33d") /* Perplex image upload */,
                                    new Guid("3e170f26-1fcb-4f60-b5d2-1aa2723528fd") /* Perplex file upload */,
                                    new Guid("84A17CF8-B711-46a6-9840-0E4A072AD000") /* Umbraco Forms file upload */,
                                    };

        public override WorkflowExecutionStatus Execute(Record record, RecordEventArgs e)
        {
            if (this.DoNotDeleteFilesOnSubmit != true.ToString())
            {
                List<string> files = new List<string>();

                // Retrieve the file locations
                foreach (var field in e.Record.RecordFields.Where(x => listOfUploadTypes.Contains(x.Value.Field.FieldTypeId)))
                    files.AddRange(field.Value.Values.Select(x => HostingEnvironment.MapPath(x.ToString())));

                // We will fail trying to delete the files if they are send as attachments in the mail (file in use exception)
                // Start a background task that will delete the files after x amount of seconds
                Task.Run(() => DeleteFilesWithDelay(files));
            }

            // Clear the entry data and replace with a "-"
            foreach (var field in e.Record.RecordFields)
                field.Value.Values = new List<object>() { "-" };

            //The re-generate the record data
            e.Record.GenerateRecordDataAsJson();

            // Set the state as deleted, this throws an exception if set to deleted, disabled for now
            //record.State = Umbraco.Forms.Core.Enums.FormState.Deleted;

            // Set the workflow status as completed
            return WorkflowExecutionStatus.Completed;   
        }

        async Task DeleteFilesWithDelay(List<string> files)
        {
            // Wait 10 seconds
            await Task.Delay(10000);

            // Loop through each file
            foreach (var file in files)
            {
                // Try to delete the files
                try
                {
                    // Delete the folder name
                    var dir = new FileInfo(file).Directory.FullName;
#if DEBUG
                    Debug.WriteLine("Trying to delete directory " + dir);
#endif

                    // This might throw an exception because the file is in use
                    // This could happen as the mail function doens't seem to dispose the attachments properly?
                    // Keep trying untill we succeed
                    Directory.Delete(dir, true);

#if DEBUG
                    Debug.WriteLine("Deleted directory " + dir);
#endif

                }
                catch(DirectoryNotFoundException ex)
                {
                    // Ignore the directory not found exception, dir might be already deleted...
                }
                catch (IOException ex)
                {
#if DEBUG
                    Debug.WriteLine(ex.Message);
#endif
                    // Keep trying untill you succeed
                    await Task.Run(() => DeleteFilesWithDelay(new List<string> { file }));
                }
            }
        }

        public override List<Exception> ValidateSettings()
        {
            List<Exception> list = new List<Exception>();

            // No possible exceptions yet
            //if (string.IsNullOrEmpty(this.Email))
            //    list.Add(new Exception("'Email' setting has not been set"));
            //if (string.IsNullOrEmpty(this.Message))
            //    list.Add(new Exception("'Message' setting has not been set'"));
            return list;
        }
    }
}
