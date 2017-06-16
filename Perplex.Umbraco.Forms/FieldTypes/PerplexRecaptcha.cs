using System;
using System.IO;
using System.Web.Hosting;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Attributes;
using static PerplexUmbraco.Forms.Code.Constants;

namespace PerplexUmbraco.Forms.Code.Recaptcha
{
    public class PerplexRecaptcha : FieldType
    {
        public PerplexRecaptcha()
        {
            Id = new Guid(EnmFieldTypeId.PerplexRecaptcha.Description());
            Name = "Perplex Recaptcha";
            FieldTypeViewName = $"FieldType.{ nameof(PerplexRecaptcha) }.cshtml";
            Description = "New and improved Google Recaptcha";
            Icon = "icon-eye";
            DataType = FieldDataType.String;
            SortOrder = 21;
        }

        [Setting("Error message",
            description = "The error message to display when the user does not pass the Recaptcha check",
            view = "TextField")]
        public string ErrorMessage { get; set; }
    }
}