using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DDay.iCal;
using DDay.iCal.Serialization;
using DDay.iCal.Serialization.iCalendar;

namespace CalDavSynchronizer.DDayICalWorkaround
{

  public class CalDavSynchronizerSerializerFactory : ISerializerFactory
  {
    private readonly ISerializerFactory _decorated;

    public CalDavSynchronizerSerializerFactory(ISerializerFactory decorated)
    {
      _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
    }

    public ISerializer Build(Type objectType, ISerializationContext ctx)
    {

      if (objectType != null)
      {
        ISerializer serializer = null;

        if (typeof(IAttendee).IsAssignableFrom(objectType))
          serializer = new CustomAttendeeSerializer();
     
        if (serializer != null)
        {
          serializer.SerializationContext = ctx;
          return serializer;
        }
      }

      return _decorated.Build(objectType, ctx);
    }
  }
}
