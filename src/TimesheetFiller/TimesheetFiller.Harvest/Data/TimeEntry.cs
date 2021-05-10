using System;
using System.Collections.Generic;
using System.Text;

namespace TimesheetFiller.Harvest.Data
{
    public class TimeEntry
    {
        public string ClientName { get; set; }

        public int ProjectId { get; set; }

        public string ProjectName { get; set; }
        
        public int TaskId { get; set; }

        public string TaskName { get; set; }

        public DateTime SpentDate { get; set; }
        
        public double Hours { get; set; }
        
        public string Notes { get; set; }
    }
}
