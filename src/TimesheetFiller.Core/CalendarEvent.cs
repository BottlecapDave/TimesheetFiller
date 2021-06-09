using System;

namespace TimesheetFiller.Core
{
    public class CalendarEvent
    {
        public string Title { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public bool IsTentative { get; set; } 
    }
}