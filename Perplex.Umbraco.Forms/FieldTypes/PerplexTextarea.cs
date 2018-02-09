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
    public class PerplexTextarea : Umbraco.Forms.Core.FieldType
    {
        // TODO: Also migrate these to an override of Settings(), there apparently are some issues with [Setting],
        // at least when using a checkbox view.
        #region Settings
        [Umbraco.Forms.Core.Attributes.Setting("Placeholder",
        view = "TextField")]
        public string Placeholder { get; set; }

        [Umbraco.Forms.Core.Attributes.Setting("Maximum length",
        view = "TextField")]
        public string MaximumLength { get; set; }

        [Umbraco.Forms.Core.Attributes.Setting("Additional attributes",
        view = "TextField")]
        public string AdditionalAttributes { get; set; }

        [Umbraco.Forms.Core.Attributes.Setting("DefaultValue", description = "Enter a default value")]
        public string DefaultValue { get; set; }

        #endregion

        public PerplexTextarea()
        {
            Id = new Guid("8c38cb28-8018-4545-b939-d1166a96b916");
            Name = "Perplex Textarea";
            FieldTypeViewName = $"FieldType.{ nameof(PerplexTextarea) }.cshtml";
            Description = "Renders a html text area";
            Icon = "icon-autofill";
            DataType = FieldDataType.LongString;
            Category = "Simple";
            SortOrder = 20;
        }

        public override bool SupportsRegex => true;
    }
}