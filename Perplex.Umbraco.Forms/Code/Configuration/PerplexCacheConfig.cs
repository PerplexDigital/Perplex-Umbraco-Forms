using System.Xml.Serialization;

namespace PerplexUmbraco.Forms.Code.Configuration
{
    [XmlType("PerplexCacheConfig")]
    public class PerplexCacheConfig
    {
        [XmlElement("CacheDurationInMinutes")]
        public int CacheDurationInMinutes { get; set; }

        [XmlElement("EnableCache")]
        public bool EnableCache { get; set; }
    }
}
