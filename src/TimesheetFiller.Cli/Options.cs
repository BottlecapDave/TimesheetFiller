using CommandLine;

namespace TimesheetFiller.Cli;

public class Options
{
    [Option('i', "input", Required = true, HelpText = "The input path of the config")]
    public string Input { get; set; }

    [Option("offset", HelpText = "The number of days to offset from today. For example, if you set this to 1, then it will run for yesterday and today.")]
    public int? DaysToOffset { get; set; }

    [Option("dry-run", HelpText = "Determines if this is is run in dry run")]
    public bool IsDryRun { get; set; }
}
