using PerplexUmbraco.Forms.Code.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Attributes;
using Umbraco.Forms.Core.Providers.FieldTypes;
using static PerplexUmbraco.Forms.Code.FieldTypeHelpers;

namespace PerplexUmbraco.Forms.FieldTypes
{
    /// <summary>
    /// Shared class for any Field Types that upload files (i.e. currently PerplexFileUpload and PerplexImageUpload)
    /// </summary>
    public abstract class PerplexBaseFileFieldType : FileUpload
    {
        #region Settings
        public string AdditionalAttributes { get; set; }
        public string AllowedFileTypes { get; set; }
        public string AllowedFileTypesErrorMessage { get; set; }
        public string MaximumFileSize { get; set; }
        public string MaximumFileSizeErrorMessage { get; set; }
        #endregion

        protected virtual PerplexBaseFileConfig Config { get; }

        public override bool StoresData => true;

        public override IEnumerable<string> ValidateField(Form form, Field field, IEnumerable<object> postedValues, HttpContextBase context)
        {
            // Set posted values to the filenames of uploaded files
            // This is necessary for Mandatory upload fields to validate
            // That is, postedValues should contain _something_ other than empty strings.
            postedValues = context.Request.Files.GetMultiple(field.Id.ToString())
                .Select(f => f.FileName)
                .Where(filename => !string.IsNullOrEmpty(filename))
                .ToList();

            // Allowed File Types
            string allowedFileTypesSetting = GetSettingValue(field, nameof(AllowedFileTypes));
            IEnumerable<string> allowedFileTypes = !string.IsNullOrEmpty(allowedFileTypesSetting)
                ? allowedFileTypesSetting.Split(',')
                // Nothing selected == everything selected
                // So use all extensions from configuration
                : Config.AllowedExtensions.Select(ae => ae.Extension);

            string allowedFileTypesErrormessage = GetSettingValue(field, nameof(AllowedFileTypesErrorMessage));

            IEnumerable<string> allowedFileTypesErrors = ValidateAllowedFileTypes(
                field, context, allowedFileTypes, allowedFileTypesErrormessage);

            if (allowedFileTypesErrors.Count() > 0)
            {
                // Clear posted values
                postedValues = Enumerable.Empty<object>();

                return allowedFileTypesErrors;
            }

            // Maximum File Size
            decimal? maximumFileSize = GetSettingValue(field, nameof(MaximumFileSize), s => decimal.Parse(s), Config.MaxFileSize);
            string maximumFileSizeErrormessage = GetSettingValue(field, nameof(MaximumFileSizeErrorMessage));

            IEnumerable<string> maximumFileSizeErrors = ValidateMaxFileSize(
                field, context, maximumFileSize, maximumFileSizeErrormessage);

            if (maximumFileSizeErrors.Count() > 0)
            {
                // Clear posted values
                postedValues = Enumerable.Empty<object>();

                return maximumFileSizeErrors;
            }

            return base.ValidateField(form, field, postedValues, context);
        }

        public override Dictionary<string, Setting> Settings()
        {
            return new Dictionary<string, Setting>
            {
                 {
                    "AdditionalAttributes",
                    new Setting("Additional attributes")
                    {
                        view = "TextField"
                    }
                },

                {
                    "AllowedFileTypes",
                    new Setting("Allowed file types")
                    {
                        description = "If nothing is checked, all in the list are allowed",
                        view = "perplexcheckboxlist"
                    }
                },

                {
                    "AllowedFileTypesErrorMessage",
                    new Setting("Allowed file types error message")
                    {
                        view = "TextField"
                    }
                },

                {
                    "MaximumFileSize",
                    new Setting("Maximum file size (in MB)")
                    {
                        description = "The maximum file size for an uploaded file. When left empty, the value configured in PerplexUmbracoForms.config will be used",
                        view = "TextField"
                    }
                },

                {
                    "MaximumFileSizeErrorMessage",
                    new Setting("Maximum file size error message")
                    {
                        description = "The error to display when the maximum file size exceeded. When left empty, the value configured in PerplexUmbracoForms.config will be used",
                        view = "TextField"
                    }
                },
            };
        }
    }
}