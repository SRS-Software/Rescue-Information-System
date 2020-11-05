#region

using System.Xml.Serialization;

#endregion

namespace RIS.Core.FireboardApi
{
    [XmlRoot(ElementName = "fireboardOperation")]
    public class FireboardOperation
    {
        [XmlElement(ElementName = "uniqueId")] public string UniqueId { get; set; }

        [XmlElement(ElementName = "basicData")]
        public BasicData BasicData { get; set; }

        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
    }

    [XmlRoot(ElementName = "geo_location")]
    public class Geo_location
    {
        [XmlElement(ElementName = "latitude")] public double Latitude { get; set; }

        [XmlElement(ElementName = "longitude")]
        public double Longitude { get; set; }
    }

    [XmlRoot(ElementName = "timestampStarted")]
    public class TimestampStarted
    {
        [XmlElement(ElementName = "long")] public string Long { get; set; }
    }

    [XmlRoot(ElementName = "basicData")]
    public class BasicData
    {
        [XmlElement(ElementName = "externalNumber")]
        public string ExternalNumber { get; set; }

        [XmlElement(ElementName = "keyword")] public string Keyword { get; set; }

        [XmlElement(ElementName = "announcement")]
        public string Announcement { get; set; }

        [XmlElement(ElementName = "location")] public string Location { get; set; }

        [XmlElement(ElementName = "geo_location")]
        public Geo_location Geo_location { get; set; }

        [XmlElement(ElementName = "timestampStarted")]
        public TimestampStarted TimestampStarted { get; set; }

        [XmlElement(ElementName = "situation")]
        public string Situation { get; set; }
    }
}

///// <remarks/>
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
//[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
//public class FireboardOperation
//{
//    public FireboardOperation()
//    {
//        versionField = "1.0";
//        uniqueIdField = Guid.NewGuid().ToString("N");
//    }
//    public FireboardOperation(OperationBasicData basicData) 
//    {
//        versionField = "1.0";
//        uniqueIdField = Guid.NewGuid().ToString("N");
//        basicDataField = basicData;
//    }

//    private string versionField;
//    private string uniqueIdField;
//    private OperationBasicData basicDataField;

//    public string version
//    {
//        get
//        {
//            return this.versionField;
//        }
//    }
//    public string uniqueId
//    {
//        get
//        {
//            return this.uniqueIdField;
//        }
//    }        
//    public OperationBasicData basicData
//    {
//        get
//        {
//            return this.basicDataField;
//        }
//        set
//        {
//            this.basicDataField = value;
//        }
//    }

//}

///// <remarks/>
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
//public class OperationBasicData
//{

//    private string externalNumberField;

//    private string keywordField;

//    private string announcementField;

//    private string locationField;

//    private OperationBasicDataGeolocation geo_locationField;

//    private OperationBasicDataTimestampStarted timestampStartedField;

//    private string situationField;

//    /// <remarks/>
//    public string externalNumber
//    {
//        get
//        {
//            return this.externalNumberField;
//        }
//        set
//        {
//            this.externalNumberField = value;
//        }
//    }

//    /// <remarks/>
//    public string keyword
//    {
//        get
//        {
//            return this.keywordField;
//        }
//        set
//        {
//            this.keywordField = value;
//        }
//    }

//    /// <remarks/>
//    public string announcement
//    {
//        get
//        {
//            return this.announcementField;
//        }
//        set
//        {
//            this.announcementField = value;
//        }
//    }

//    /// <remarks/>
//    public string location
//    {
//        get
//        {
//            return this.locationField;
//        }
//        set
//        {
//            this.locationField = value;
//        }
//    }

//    /// <remarks/>
//    public OperationBasicDataGeolocation geo_location
//    {
//        get
//        {
//            return this.geo_locationField;
//        }
//        set
//        {
//            this.geo_locationField = value;
//        }
//    }

//    /// <remarks/>
//    public OperationBasicDataTimestampStarted timestampStarted
//    {
//        get
//        {
//            return this.timestampStartedField;
//        }
//        set
//        {
//            this.timestampStartedField = value;
//        }
//    }

//    /// <remarks/>
//    public string situation
//    {
//        get
//        {
//            return this.situationField;
//        }
//        set
//        {
//            this.situationField = value;
//        }
//    }
//}

///// <remarks/>
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
//public class OperationBasicDataGeolocation
//{

//    private object latitudeField;

//    private object longitudeField;

//    /// <remarks/>
//    public object latitude
//    {
//        get
//        {
//            return this.latitudeField;
//        }
//        set
//        {
//            this.latitudeField = value;
//        }
//    }

//    /// <remarks/>
//    public object longitude
//    {
//        get
//        {
//            return this.longitudeField;
//        }
//        set
//        {
//            this.longitudeField = value;
//        }
//    }
//}

///// <remarks/>
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
//public class OperationBasicDataTimestampStarted
//{

//    private ulong longField;

//    /// <remarks/>
//    public ulong @long
//    {
//        get
//        {
//            return this.longField;
//        }
//        set
//        {
//            this.longField = value;
//        }
//    }
//}