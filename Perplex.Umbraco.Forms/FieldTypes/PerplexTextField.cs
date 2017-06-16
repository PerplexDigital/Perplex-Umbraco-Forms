using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Extensions;
using Umbraco.Forms.Data;
using static PerplexUmbraco.Forms.Code.Constants;

namespace PerplexUmbraco.Forms.FieldTypes
{
    public class PerplexTextField : Umbraco.Forms.Core.FieldType
    {
        #region Settings
        [Umbraco.Forms.Core.Attributes.Setting("Placeholder",
        view = "TextField")]
        public string Placeholder { get; set; }

        [Umbraco.Forms.Core.Attributes.Setting("Maximum length",
        view = "TextField")]
        public string MaximumLength { get; set; }

        [Umbraco.Forms.Core.Attributes.Setting("Type",
        view = "dropdownlist", prevalues = "email,tel,text,number")]
        public string FieldType { get; set; }

        [Umbraco.Forms.Core.Attributes.Setting("Additional attributes",
        view = "TextField")]
        public string AdditionalAttributes { get; set; }

        [Umbraco.Forms.Core.Attributes.Setting("DefaultValue", description = "Enter a default value")]
        public string DefaultValue { get; set; }

        #endregion

        public PerplexTextField()
        {
            Id = new Guid("9ead6835-57db-418b-ae2b-528f8db375a0");
            Name = "Perplex Text field";
            FieldTypeViewName = $"FieldType.{ nameof(PerplexTextField) }.cshtml";
            Description = "Renders a html input fieldKey";
            Icon = "icon-autofill";
            DataType = FieldDataType.String;
            Category = "Simple";
            SortOrder = 10;
        }

        public override bool SupportsRegex => true;
    }
}