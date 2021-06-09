using System.Collections.Generic;

namespace TimesheetFiller.Harvest.Data
{
    public class TimeEntryResult
    {
        public IEnumerable<TimeEntry> time_entries { get; set; }
    }
}
