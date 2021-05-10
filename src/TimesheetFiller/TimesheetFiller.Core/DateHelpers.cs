using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TimesheetFiller.Core
{
    public static class DateHelpers
    {
        public static async Task ForEachDay(DateTime from, DateTime to, Func<DateTime, DateTime, Task> callback)
        {
            var dayStart = from.Date;
            while (dayStart < to)
            {
                var dayEnd = dayStart.AddDays(1).AddSeconds(-1);

                await callback(dayStart, dayEnd);

                dayStart = dayStart.AddDays(1);
            }
        }
    }
}
