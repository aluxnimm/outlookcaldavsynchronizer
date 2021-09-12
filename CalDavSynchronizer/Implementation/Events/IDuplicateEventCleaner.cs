using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CalDavSynchronizer.Implementation.Events
{
    public interface IDuplicateEventCleaner
    {
        Task NotifySynchronizationFinished();
        void AnnounceAppointment(AppointmentSlim appointment);
        void AnnounceAppointmentDeleted(AppointmentId id);
        Task<IEnumerable<AppointmentId>> DeleteAnnouncedEventsIfDuplicates(Predicate<AppointmentId> canBeDeleted);
    }
}