namespace TimesheetFiller.Harvest.Data
{
    public class Project
    {
        public int id { get; set; }

        public string name { get; set; }
    }

    public class ProjectTask
    {
        public int id { get; set; }

        public string name { get; set; }
    }

    public class TaskAssignment
    {
        public ProjectTask task { get; set; }
    }
}