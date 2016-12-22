using PerplexUmbraco.Forms.Code.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

using Umbraco.Forms.Core;

namespace PerplexUmbraco.Forms.FieldTypes
{
    public class PerplexImageUpload : PerplexBaseFileFieldType
    {
        protected override PerplexBaseFileConfig Config => PerplexUmbracoFormsConfig.Get.PerplexImageUpload;

        public PerplexImageUpload()
        {
            Id = new Guid("11fff56b-7e0e-4bfc-97ba-b5126158d33d");
            Name = "Perplex image upload";
            Description = "Renders an upload field";
            Icon = "icon-download-alt";
            DataType = FieldDataType.String;
            SortOrder = 10;
            Category = "Simple";
            FieldTypeViewName = $"FieldType.{ nameof(PerplexImageUpload) }.cshtml";
        }
    }
}