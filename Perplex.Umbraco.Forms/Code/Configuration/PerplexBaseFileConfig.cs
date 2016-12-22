using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PerplexUmbraco.Forms.Code.Configuration
{
    public abstract class PerplexBaseFileConfig
    {
        public List<ExtensionConfig> AllowedExtensions { get; set; }

        public decimal? MaxFileSize
        {
            get
            {
                decimal maxFileSize;

                if (!decimal.TryParse(MaxFileSizeRaw, out maxFileSize))
                {
                    return null;
                };

                return maxFileSize;
            }
        }

        [XmlElement(ElementName = nameof(MaxFileSize))]
        public string MaxFileSizeRaw { get; set; }
    }
}
