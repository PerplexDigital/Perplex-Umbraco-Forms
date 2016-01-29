using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Core.IO;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Attributes;
using Umbraco.Forms.Core.Extensions;
using Umbraco.Forms.Data;

namespace PerplexUmbraco.Forms.FieldTypes
{
    public class PerplexImageUpload : Umbraco.Forms.Core.FieldType
    {
        const string _defaultFileTypes = "jpg,jpeg,png,gif,tif";

        #region Settings
        [Setting("Allowed file types", description = "If nothing is checked, all in the list are allowed",
        view = "perplexcheckboxlist", prevalues = _defaultFileTypes)]
        public string AllowedFileTypes { get; set; }

        [Setting("Allowed file types error message",
        description = "",
        view = "TextField")]
        public string AllowedFileTypesErrorMessage { get; set; }

        [Setting("Maximum file size (in MB)",
        description = "",
        view = "TextField")]
        public string MaximumFileSize { get; set; }

        [Setting("Maximum file size error message",
        description = "",
        view = "TextField")]
        public string MaximumFileSizeErrorMessage { get; set; }

        [Setting("Multi upload",
        description = "",
        view = "checkbox")]
        public bool MultiUpload { get; set; }

        [Umbraco.Forms.Core.Attributes.Setting("Additional attributes",
        view = "TextField")]
        public string AdditionalAttributes { get; set; }

        #endregion

        public PerplexImageUpload()
        {
            //Provider
            this.Id = new Guid("11fff56b-7e0e-4bfc-97ba-b5126158d33d");
            this.Name = "Perplex image upload";
            this.Description = "Renders an upload field";
            this.Icon = "icon-download-alt";
            this.DataType = FieldDataType.String;
            this.SortOrder = 10;
            this.RenderView = "file";
            this.Category = "Simple";
            //this.RenderView = "~/views/partials/forms/fieldtypes/FieldType.PerplexUploadField.cshtml";
        }

        public override IEnumerable<string> ValidateField(Form form, Field field, IEnumerable<object> postedValues, HttpContextBase context)
        {
            HttpFileCollectionBase files = context.Request.Files;
            if (files.Count > 0 && Enumerable.Contains<string>((IEnumerable<string>)files.AllKeys, field.Id.ToString()))
                postedValues = (IEnumerable<object>)new object[1]
                                                        {
                                                          (object) files[field.Id.ToString()].FileName
                                                        };


            if (postedValues.Any(x => !String.IsNullOrEmpty(x.ToString())) == true)
            {
                #region Perplex extra
                // Check the allowed file extensions
                var allowedFileTypesSetting = field.Settings.FirstOrDefault(x => x.Key == "AllowedFileTypes").Value;
                string[] allowedFileTypes = null;
                if (!String.IsNullOrEmpty(allowedFileTypesSetting))
                    allowedFileTypes = allowedFileTypesSetting.Split(',');
                else // use the default
                    allowedFileTypes = _defaultFileTypes.Split(',');

                if (allowedFileTypes.Any())
                {
                    foreach (var file in postedValues.Where(x => !String.IsNullOrEmpty(x.ToString())))
                    {
                        if (!allowedFileTypes.Contains(Path.GetExtension(file.ToString()).Replace(".", "").ToLower()))
                        {
                            // Reset the uploaded files
                            postedValues = null;
                            return (IEnumerable<string>)new string[1]
                          {
                            string.Format(StringExtensions.ParsePlaceHolders(field.Settings.First(x => x.Key == "AllowedFileTypesErrorMessage").Value ?? ""), (object) field.Caption)
                          };
                        }
                    }
                }

                // Check the file size
                var maxFileLength = field.Settings.FirstOrDefault(x => x.Key == "MaximumFileSize");
                int maxMegaBytes;
                if (!String.IsNullOrEmpty(maxFileLength.Value) && Int32.TryParse(maxFileLength.Value, out maxMegaBytes) && maxMegaBytes > 0)
                {
                    int maxBytes = maxMegaBytes * 1024 * 1024;
                    int index = 0;
                    foreach (string str in files.AllKeys)
                    {
                        HttpPostedFileBase file = files[index];
                        // ContentLength is in bytes
                        if (file.ContentLength > maxBytes)
                        {
                            // Reset the uploaded files
                            postedValues = null;
                            return (IEnumerable<string>)new string[1]
                          {
                            string.Format(StringExtensions.ParsePlaceHolders(field.Settings.First(x => x.Key == "MaximumFileSizeErrorMessage").Value ?? ""), (object) field.Caption)
                          };
                        }

                        index++;
                    }
                }
            }

            #endregion

            return base.ValidateField(form, field, postedValues, context);
        }

        public override IEnumerable<object> ProcessSubmittedValue(Field field, IEnumerable<object> postedValues, HttpContextBase context)
        {
            bool flag1 = false;
            bool flag2 = false;
            List<object> list = new List<object>();
            string path1 = context.Request[field.Id.ToString() + "_file"] ?? "";
            HttpFileCollectionBase files = context.Request.Files;
            if (files.Count > 0 && Enumerable.Contains<string>((IEnumerable<string>)files.AllKeys, field.Id.ToString()))
                flag1 = true;
            if (!string.IsNullOrEmpty(path1))
                flag2 = true;
            if (flag1)
            {
                if (flag2)
                    File.Delete(IOHelper.MapPath(path1));
                int index = 0;
                foreach (string str in files.AllKeys)
                {
                    if (str == field.Id.ToString())
                    {
                        HttpPostedFileBase httpPostedFileBase = files[index];
                        if (httpPostedFileBase.ContentLength > 0)
                        {
                            string url = Enumerable.Last<string>((IEnumerable<string>)httpPostedFileBase.FileName.Split('\\'));
                            string path2 = Configuration.UploadTempPath + "/" + Guid.NewGuid().ToString();
                            string path3 = path2 + "/" + FileHelper.SafeUrl(url);
                            Directory.CreateDirectory(IOHelper.MapPath(path2));
                            httpPostedFileBase.SaveAs(IOHelper.MapPath(path3));
                            list.Add((object)path3);
                        }
                    }
                    ++index;
                }
            }
            list.Add((object)path1);
            return (IEnumerable<object>)list;
        }

        public override IEnumerable<object> ConvertToRecord(Field field, IEnumerable<object> postedValues, HttpContextBase context)
        {
            List<object> list = new List<object>();
            foreach (object obj in postedValues)
            {
                if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                {
                    string str1 = IOHelper.MapPath(obj.ToString());
                    string path = Configuration.UploadPath + obj.ToString().Substring(Configuration.UploadTempPath.Length);
                    if (File.Exists(str1))
                    {
                        string str2 = IOHelper.MapPath(path);
                        string directoryName = Path.GetDirectoryName(str2);
                        if (!Directory.Exists(directoryName))
                            Directory.CreateDirectory(directoryName);
                        File.Move(str1, str2);
                        list.Add((object)path);
                    }
                }
            }
            return (IEnumerable<object>)list;
        }
    }
}