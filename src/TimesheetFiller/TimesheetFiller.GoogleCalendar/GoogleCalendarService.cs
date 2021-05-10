using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Http;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TimesheetFiller.Core;

namespace TimesheetFiller.GoogleCalendar
{
    public class GoogleCalendarService : ICalendarService
    {
        private readonly CalendarService _service;
        private readonly ILogger _logger;
        private readonly GoogleCalendarConfig _config;

        public GoogleCalendarService(IConfigurableHttpClientInitializer initialiser, GoogleCalendarConfig config, ILogger logger)
        {
            _logger = logger;
            _config = config;
            _service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = initialiser,
                ApplicationName = "TimesheetFiller",
            });
        }

        public async Task<IEnumerable<CalendarEvent>> GetEventsAsync(DateTime from, DateTime to)
        {
            var allEvents = new List<CalendarEvent>();

            await _logger.LogAsync("Retrieving calendars...");
            var calendars = await _service.CalendarList.List().ExecuteAsync();
            
            foreach (var calendar in calendars.Items)
            {
                if (_config.CalendarIds != null && _config.CalendarIds.Contains(calendar.Id) == false)
                {
                    continue;
                }

                await _logger.LogAsync($"Retrieving events for calendar '{calendar.Summary}' ({calendar.Id})...");
                
                var request = _service.Events.List(calendar.Id);
                request.TimeMin = from;
                request.TimeMax = to;
                request.ShowDeleted = false;
                request.SingleEvents = true;

                var events = await request.ExecuteAsync();

                allEvents.AddRange(events.Items
                    .Where(e => (e.Status == "confirmed" || e.Status == "tentative") && e.Start.DateTime.HasValue && e.End.DateTime.HasValue)
                    .Select(e => new CalendarEvent()
                    {
                        Title = e.Summary,
                        Start = e.Start.DateTime.Value,
                        End = e.End.DateTime.Value,
                        IsTentative = e.Status == "tentative"
                    }));

                await _logger.LogAsync("Complete");
            }

            await _logger.LogAsync($"Complete. Discovered {allEvents.Count} event(s)");

            return allEvents;
        }
    }
}
