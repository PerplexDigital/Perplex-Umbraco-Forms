using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PerplexUmbraco.Forms.Code.Configuration
{
    [XmlType("FieldType")]
    public class FieldTypeConfig
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("guid")]
        public Guid Guid { get; set; }

        [XmlAttribute("hide")]
        public bool Hide { get; set; } = true;
    }
}
