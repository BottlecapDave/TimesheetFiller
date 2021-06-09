using System.Collections;
using System.Collections.Generic;

namespace TimesheetFiller.GoogleCalendar
{
    public class GoogleCalendarConfig
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public IEnumerable<string> CalendarIds { get; set; }
    }
}
