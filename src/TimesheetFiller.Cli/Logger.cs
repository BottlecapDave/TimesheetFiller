using System;
using System.Threading.Tasks;

namespace TimesheetFiller.Cli;

public class Logger : Core.ILogger
{
    public Task LogAsync(string msg)
    {
        Console.WriteLine(msg);
        return Task.FromResult(false);
    }
}
