using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Xml.Serialization;
using static PerplexUmbraco.Forms.Code.Constants;

namespace PerplexUmbraco.Forms.Code.Configuration
{
    /// <summary>
    /// Configuration settings read from a config XML file
    /// </summary>
    [XmlRoot("PerplexUmbracoFormsConfig")]
    public class PerplexUmbracoFormsConfig
    {
        /// <summary>
        /// Cached instance of configuration
        /// </summary>
        private static PerplexUmbracoFormsConfig Config;

        private PerplexUmbracoFormsConfig() { }

        public static PerplexUmbracoFormsConfig Get
        {
            get
            {
                if (Config == null)
                {
                    // If the file is not there => Create with defaults
                    CreateIfNotExists();

                    // Create from configuration file
                    string path = GetFilePath();

                    XmlSerializer serializer = new XmlSerializer(typeof(PerplexUmbracoFormsConfig));

                    // Read from file
                    using (var reader = new StreamReader(GetFilePath()))
                    {
                        Config = (PerplexUmbracoFormsConfig)serializer.Deserialize(reader);
                    }
                }

                return Config;
            }
        }

        public static void CreateIfNotExists()
        {
            // Create from configuration file
            string path = GetFilePath();

            // If it is not there yet, serialize our defaults to file
            if (!File.Exists(path))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PerplexUmbracoFormsConfig));

                using (StreamWriter file = new StreamWriter(path))
                {
                    serializer.Serialize(file, DefaultConfig);
                }
            }
        }

        public List<FieldTypeConfig> HideFieldTypes { get; set; }

        public PerplexFileUploadConfig PerplexFileUpload { get; set; }

        public PerplexImageUploadConfig PerplexImageUpload { get; set; }

        public PerplexRecaptchaConfig PerplexRecaptchConfig { get; set; }

        private static string GetFilePath()
        {
            return HostingEnvironment.MapPath(Constants.CONFIGURATION_FILE_PATH);
        }

        private static readonly PerplexUmbracoFormsConfig DefaultConfig = new PerplexUmbracoFormsConfig
        {
            HideFieldTypes = new List<FieldTypeConfig>
            {
                // Perplex Fields =>
                new FieldTypeConfig { Guid = new Guid(EnmFieldTypeId.PerplexFileUpload.Description()), Name = "Perplex File Upload", Hide = false },
                new FieldTypeConfig { Guid = new Guid(EnmFieldTypeId.PerplexImageUpload.Description()), Name = "Perplex Image Upload", Hide = false },
                new FieldTypeConfig { Guid = new Guid(EnmFieldTypeId.PerplexTextField.Description()), Name = "Perplex Text field", Hide = false },
                new FieldTypeConfig { Guid = new Guid(EnmFieldTypeId.PerplexTextarea.Description()), Name = "Perplex Textarea", Hide = false },
                new FieldTypeConfig { Guid = new Guid(EnmFieldTypeId.PerplexRecaptcha.Description()), Name = "Perplex Recaptcha", Hide = false },
                // <= Perplex Fields

                // Built-in Fields =>
                new FieldTypeConfig { Guid = new Guid(EnmFieldTypeId.Checkbox.Description()), Name = "Checkbox", Hide = false },
                new FieldTypeConfig { Guid = new Guid(EnmFieldTypeId.Date.Description()), Name = "Date", Hide = false },
                new FieldTypeConfig { Guid = new Guid(EnmFieldTypeId.Dropdown.Description()), Name = "Dropdown", Hide = false },
                new FieldTypeConfig { Guid = new Guid(EnmFieldTypeId.ShortAnswer.Description()), Name = "Short answer", Hide = true },
                new FieldTypeConfig { Guid = new Guid(EnmFieldTypeId.LongAnswer.Description()), Name = "Long answer", Hide = true },
                new FieldTypeConfig { Guid = new Guid(EnmFieldTypeId.FileUpload.Description()), Name = "File upload", Hide = true },
                new FieldTypeConfig { Guid = new Guid(EnmFieldTypeId.Password.Description()), Name = "Password", Hide = true },
                new FieldTypeConfig { Guid = new Guid(EnmFieldTypeId.MultipleChoice.Description()), Name = "Multiple choice", Hide = false },
                new FieldTypeConfig { Guid = new Guid(EnmFieldTypeId.SingleChoice.Description()), Name = "Single choice", Hide = false },
                new FieldTypeConfig { Guid = new Guid(EnmFieldTypeId.TitleAndDescription.Description()), Name = "Title and description", Hide = false },
                new FieldTypeConfig { Guid = new Guid(EnmFieldTypeId.Recaptcha.Description()), Name = "Recaptcha", Hide = true },
                new FieldTypeConfig { Guid = new Guid(EnmFieldTypeId.Hidden.Description()), Name = "Hidden", Hide = true },
                // <= Built-in Fields
            },

            PerplexFileUpload = new PerplexFileUploadConfig
            {
                AllowedExtensions = new List<ExtensionConfig>
                {
                    new ExtensionConfig { Extension = "doc" },
                    new ExtensionConfig { Extension = "docx" },
                    new ExtensionConfig { Extension = "pdf" },
                    new ExtensionConfig { Extension = "xls" },
                    new ExtensionConfig { Extension = "xlsx" },
                    new ExtensionConfig { Extension = "zip" },
                    new ExtensionConfig { Extension = "rar" },
                },

                MaxFileSizeRaw = ""
            },

            PerplexImageUpload = new PerplexImageUploadConfig
            {
                AllowedExtensions = new List<ExtensionConfig>
                {
                    new ExtensionConfig { Extension = "jpg" },
                    new ExtensionConfig { Extension = "jpeg" },
                    new ExtensionConfig { Extension = "png" },
                    new ExtensionConfig { Extension = "gif" },
                    new ExtensionConfig { Extension = "tif" },
                },

                MaxFileSizeRaw = ""
            },

            PerplexRecaptchConfig = new PerplexRecaptchaConfig
            {
                ErrorMessage = ""
            }
        };
    }
}
