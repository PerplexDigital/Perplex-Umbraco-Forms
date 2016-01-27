using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Extensions;
using Umbraco.Forms.Data;

namespace PerplexUmbraco.Forms.Types
{
    public class PerplexTextarea : Umbraco.Forms.Core.FieldType
    {
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
            //Provider
            this.Id = new Guid("8c38cb28-8018-4545-b939-d1166a96b916");
            this.Name = "Perplex Textarea";
            this.Description = "Renders a html text area";
            this.Icon = "icon-autofill";
            this.DataType = FieldDataType.LongString;
            this.Category = "Simple";
            this.SortOrder = 20;
        }

        public override bool SupportsRegex
        {
            get
            {
                return true;
            }
        }
    }
}