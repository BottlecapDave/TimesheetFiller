using System.Collections.Generic;

namespace TimesheetFiller.Harvest
{
    public class HarvestConfig
    {
        public string AccountId { get; set; }

        public string ApiToken { get; set; }

        public double DefaultHoursPerDay { get; set; }

        public string DefaultClientName { get; set; }

        public string DefaultProjectName { get; set; }

        public string DefaultTaskName { get; set; }

        public IEnumerable<string> DaysToIgnore { get; set; }

        public IEnumerable<HarvestCalendarTaskMap> Tasks { get; set; }
    }
}
