using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DDay.iCal.Serialization;
using DDay.iCal.Serialization.iCalendar;
using log4net;

namespace CalDavSynchronizer.DDayICalWorkaround
{
    static class DDayICalCustomization
    {
        private static readonly ILog s_logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void InitializeNoThrow()
        {
            try
            {
                var originalFactory = SerializationContext.Default.GetService<SerializerFactory>();
                if (originalFactory != null)
                {
                    SerializationContext.Default.RemoveService(typeof(SerializerFactory));
                    SerializationContext.Default.SetService(new CalDavSynchronizerSerializerFactory(originalFactory));
                }
                else
                {
                    s_logger.Error($"'{nameof(SerializationContext.Default.GetService)}' for '{nameof(SerializerFactory)}' returned NULL!");
                }
            }
            catch (Exception x)
            {
                s_logger.Error($"Error while initializing '{nameof(DDayICalWorkaround.DDayICalCustomization)}'", x);
            }
        }
    }
}