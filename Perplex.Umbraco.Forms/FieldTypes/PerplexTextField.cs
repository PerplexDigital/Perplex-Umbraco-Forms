using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Extensions;
using Umbraco.Forms.Data;

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
        view = "dropdownlist", prevalues = "email,tel,text,numeric")]
        public string FieldType { get; set; }

        [Umbraco.Forms.Core.Attributes.Setting("Additional attributes",
        view = "TextField")]
        public string AdditionalAttributes { get; set; }

        [Umbraco.Forms.Core.Attributes.Setting("DefaultValue", description = "Enter a default value")]
        public string DefaultValue { get; set; }

        #endregion

        public PerplexTextField()
        {
            //Provider
            this.Id = new Guid("9ead6835-57db-418b-ae2b-528f8db375a0");
            this.Name = "Perplex Text field";
            this.Description = "Renders a html input fieldKey";
            this.Icon = "icon-autofill";
            this.DataType = FieldDataType.String;
            this.Category = "Simple";
            this.SortOrder = 10;
        }

        public override bool SupportsRegex
        {
            get
            {
                return true;
            }
        }

        //public override IEnumerable<object> ConvertToRecord(Field field, IEnumerable<object> postedValues, HttpContextBase context)
        //{
        //    List<object> list = new List<object>();

        //    list.Add(PerplexLib.Security.Encrypt(postedValues.FirstOrDefault().ToString(), "testkey41234"));

        //    return (IEnumerable<object>)list;
        //}

        //public override IEnumerable<object> ConvertFromRecord(Field field, IEnumerable<object> storedValues)
        //{
        //    List<object> list = new List<object>();

        //    list.Add(PerplexLib.Security.Decrypt(storedValues.FirstOrDefault().ToString(), "testkey41234"));

        //    return (IEnumerable<object>)list;
        //}

    }
}