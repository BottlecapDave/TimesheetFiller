using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TimesheetFiller.Core
{
    public interface ICalendarService
    {
        Task<IEnumerable<CalendarEvent>> GetEventsAsync(DateTime from, DateTime to);
    }
}
