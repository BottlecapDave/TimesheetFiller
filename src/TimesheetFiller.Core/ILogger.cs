using System.Threading.Tasks;

namespace TimesheetFiller.Core
{
    public interface ILogger
    {
        Task LogAsync(string msg);
    }
}
