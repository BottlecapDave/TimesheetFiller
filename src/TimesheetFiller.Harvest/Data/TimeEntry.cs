namespace TimesheetFiller.Harvest.Data
{
    public class TimeEntry
    {
        public Client client { get; set; }

        public Project project { get; set; }

        public ProjectTask task { get; set; }

        public decimal hours { get; set; }

        public string notes { get; set; }
    }
}
