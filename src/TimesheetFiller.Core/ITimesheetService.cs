using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TimesheetFiller.Core
{
    public interface ITimesheetService
    {
        Task AddTimesheetEntries(DateTime from, DateTime to, IEnumerable<CalendarEvent> events, bool isDryRun);
    }
}
