using TimesheetFiller.GoogleCalendar;
using TimesheetFiller.Harvest;

namespace TimesheetFiller.Cli;

public class Config
{
    public GoogleCalendarConfig Google { get; set; }

    public HarvestConfig Harvest { get; set; }
}
