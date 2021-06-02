using CommandLine;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TimesheetFiller.GoogleCalendar;
using TimesheetFiller.Harvest;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TimesheetFiller.Cli
{
    class Logger : Core.ILogger
    {
        public Task LogAsync(string msg)
        {
            Console.WriteLine(msg);
            return Task.FromResult(false);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o => new Program().Run(o).Wait())
                .WithNotParsed<Options>(errors => {
                    System.Console.WriteLine("Error");
                });
        }

        private async Task Run(Options options)
        {
            using (var reader = new StreamReader(options.Input))
            {
                var content = await reader.ReadToEndAsync();

                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                //yml contains a string containing your YAML
                var config = deserializer.Deserialize<Config>(content);

                var from = DateTime.Now.Date.AddDays((options.DaysToOffset ?? 0) * -1).Date;
                var to = DateTime.Now.Date.AddDays(1).AddSeconds(-1);

                UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets()
                    {
                        ClientId = config.Google.ClientId,
                        ClientSecret = config.Google.ClientSecret
                    },
                    new[] { "https://www.googleapis.com/auth/calendar.events.readonly", "https://www.googleapis.com/auth/calendar.readonly" },
                    "user",
                    CancellationToken.None
                );

                var logger = new Logger();
                var calendarService = new GoogleCalendarService(credential, config.Google, logger);
                var timesheetService = new HarvestTimesheetService(config.Harvest, logger);

                var events = await calendarService.GetEventsAsync(from, to);
                await timesheetService.AddTimesheetEntries(from, to, events, options.IsDryRun);
            }
        }

    }
}
