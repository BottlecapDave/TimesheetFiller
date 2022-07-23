using System;
using System.IO;
using System.Threading;
using TimesheetFiller.GoogleCalendar;
using TimesheetFiller.Harvest;
using CommandLine;
using TimesheetFiller.Cli;
using YamlDotNet.Serialization;
using Google.Apis.Auth.OAuth2;
using YamlDotNet.Serialization.NamingConventions;

Parser.Default.ParseArguments<Options>(args)
    .WithNotParsed<Options>(errors => {
        System.Console.WriteLine("Error");
    })
    .WithParsedAsync<Options>(async options => {
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
    }).Wait();