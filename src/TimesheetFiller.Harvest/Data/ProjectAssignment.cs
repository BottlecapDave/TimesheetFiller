using System;
using System.Collections.Generic;
using System.Text;

namespace TimesheetFiller.Harvest.Data
{
    public class ProjectAssignment
    {
        public Client client { get; set; }

        public Project project { get; set; }

        public IEnumerable<TaskAssignment> task_assignments { get; set; }
    }
}
