using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PerplexUmbraco.Forms.Code.Configuration
{
    [XmlType("Extension")]
    public class ExtensionConfig
    {
        [XmlText]
        public string Extension { get; set; }
    }
}
