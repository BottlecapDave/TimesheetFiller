using CommandLine;

namespace TimesheetFiller.Cli
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "The input path of the config")]
        public string Input { get; set; }

        [Option("week")]
        public bool IsThisWeek { get; set; }

        [Option("week")]
        public bool IsYesterday { get; set; }

        [Option("dry-run", HelpText = "Determines if this is is run in dry run")]
        public bool IsDryRun { get; set; }
    }
}
