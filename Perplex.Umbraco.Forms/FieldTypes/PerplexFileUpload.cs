using PerplexUmbraco.Forms.Code.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Attributes;
using static PerplexUmbraco.Forms.Code.Constants;

namespace PerplexUmbraco.Forms.FieldTypes
{
    public class PerplexFileUpload : PerplexBaseFileFieldType
    {
        protected override PerplexBaseFileConfig Config => PerplexUmbracoFormsConfig.Get.PerplexFileUpload;

        #region Settings
        [Setting("Multi upload", description = "If checked, allows the user to upload multiple files", view = "checkbox")]
        public bool MultiUpload { get; set; }
        #endregion

        public PerplexFileUpload()
        {
            Id = new Guid("3e170f26-1fcb-4f60-b5d2-1aa2723528fd");
            Name = "Perplex file upload";
            FieldTypeViewName = $"FieldType.{ nameof(PerplexFileUpload) }.cshtml";
            Description = "Renders an upload field";
            Icon = "icon-download-alt";
            DataType = FieldDataType.String;
            SortOrder = 10;
            Category = "Simple";            
        }
    }
}