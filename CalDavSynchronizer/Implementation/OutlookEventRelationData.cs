using System;
using System.Xml.Serialization;
using CalDavSynchronizer.Generic.EntityRelationManagement;

namespace CalDavSynchronizer.Implementation
{
  public class OutlookEventRelationData : IEntityRelationData<string, DateTime, Uri, string>
  {
    public string AtypeId { get; set; }
    public DateTime AtypeVersion { get; set; }

    [XmlIgnore]
    public Uri BtypeId { get; set; }

    [XmlElement ("BtypeId")]
    public string SerializableBtypeId
    {
      get { return BtypeId.ToString(); }
      set { BtypeId = new Uri (value, UriKind.Relative); }
    }

    public string BtypeVersion { get; set; }
  }
}