﻿using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimesheetFiller.Core;
using TimesheetFiller.Harvest.Data;

namespace TimesheetFiller.Harvest
{
    public class HarvestTimesheetService : ITimesheetService
    {
        private readonly HarvestConfig _config;
        private readonly ILogger _logger;
        private readonly RestClient _client;

        public HarvestTimesheetService(HarvestConfig config, ILogger logger)
        {
            _config = config;
            _logger = logger;
            
            _client = new RestClient("https://api.harvestapp.com");
            _client.AddDefaultHeader("Authorization", $"Bearer {config.ApiToken}");
            _client.AddDefaultHeader("Harvest-Account-Id", config.AccountId);
            _client.AddDefaultHeader("User-Agent", $"TimesheetFiller (someone@somewhere.com)");
        }

        public async Task AddTimesheetEntries(DateTime from, DateTime to, IEnumerable<CalendarEvent> events, bool isDryRun)
        {
            var projectAssignments = await GetProjectAssignmentsAsync();
            var entries = new List<CreateTimeEntry>();
            await DateHelpers.ForEachDay(from, to, async (DateTime dayStart, DateTime dayEnd) =>
            {
                // See if the day should be skipped
                if (_config.DaysToIgnore != null && 
                    String.IsNullOrEmpty(_config.DaysToIgnore.FirstOrDefault(x => String.Equals(x, dayStart.DayOfWeek.ToString(), StringComparison.InvariantCultureIgnoreCase))) == false)
                {
                    await _logger.LogAsync($"Skipping '{dayStart}'");
                    return;
                }

                var existingTimeEntries = await this.GetTimeEntriesAsync(dayStart, dayEnd);

                double totalHours = existingTimeEntries.Sum(x => (double)x.hours);

                await _logger.LogAsync($"Adding records for '{dayStart}'...");

                var todaysEvents = events.Where(e => e.Start >= dayStart && e.End <= dayEnd)
                    // Account for the same event in multiple calendars
                    .GroupBy(x => x.Title)
                    .Select(x => x.FirstOrDefault());

                foreach (var e in todaysEvents)
                {
                    await _logger.LogAsync($"Processsing event '{e.Title}'...");

                    var targets = _config.Tasks.Where(t => e.Title.ToLowerInvariant().Contains(t.CalendarSearchTerm.ToLowerInvariant())).ToList();
                    if (targets.Count == 0)
                    {
                        await _logger.LogAsync($"Failed to match any harvest tasks");
                    }
                    else if (targets.Count > 1)
                    {
                        await _logger.LogAsync($"Failed. Matched more than one harvest task.");
                    }
                    else
                    {
                        var target = targets.First();

                        var projectTaskIds = this.FindProjectIdAndTaskId(projectAssignments, target.ClientName, target.ProjectName, target.TaskName);

                        var existingRecord = existingTimeEntries.FirstOrDefault(x =>
                            x.notes == e.Title &&
                            x.project.id == projectTaskIds.Item1 &&
                            x.task.id == projectTaskIds.Item2
                        );

                        if (existingRecord != null)
                        {
                            await _logger.LogAsync($"Skipping event '{e.Title}' as hours have already been logged");
                        }
                        else
                        {
                            var hours = e.End.Subtract(e.Start).TotalMinutes / 60;
                            totalHours += hours;

                            entries.Add(new CreateTimeEntry()
                            {
                                ClientName = target.ClientName,
                                ProjectId = projectTaskIds.Item1,
                                ProjectName = target.ProjectName,
                                TaskId = projectTaskIds.Item2,
                                TaskName = target.TaskName,
                                SpentDate = e.Start,
                                Hours = hours,
                                Notes = e.Title
                            });
                        }
                    }
                }

                if (String.IsNullOrEmpty(_config.DefaultClientName) == false && 
                    String.IsNullOrEmpty(_config.DefaultProjectName) == false &&
                    String.IsNullOrEmpty(_config.DefaultTaskName) == false &&
                    totalHours < _config.DefaultHoursPerDay)
                {
                    var remainingHours = _config.DefaultHoursPerDay - totalHours;
                    var projectTaskIds = this.FindProjectIdAndTaskId(projectAssignments, _config.DefaultClientName, _config.DefaultProjectName, _config.DefaultTaskName);

                    entries.Add(new CreateTimeEntry()
                    {
                        ClientName = _config.DefaultClientName,
                        ProjectId = projectTaskIds.Item1,
                        ProjectName = _config.DefaultProjectName,
                        TaskId = projectTaskIds.Item2,
                        TaskName = _config.DefaultTaskName,
                        Hours = remainingHours,
                        SpentDate = dayStart.Add(new TimeSpan(9, 0, 0)),
                    });
                }
            });

            await _logger.LogAsync($"Submitting entries...");
            foreach (var entry in entries)
            {
                await this.CreateTimeEntryAsync(entry, isDryRun);
            }
        }

        private Tuple<int, int> FindProjectIdAndTaskId(IEnumerable<ProjectAssignment> projectAssignments, string targetClientName, string targetProjectName, string targetTaskName)
        {
            var project = projectAssignments.FirstOrDefault(x =>
                            String.Equals(x.client.name, targetClientName, StringComparison.InvariantCultureIgnoreCase) &&
                            String.Equals(x.project.name, targetProjectName, StringComparison.InvariantCultureIgnoreCase)
                        );
            if (project == null)
            {
                throw new Exception($"Failed to find project '{targetProjectName}' for client '{targetClientName}'");
            }

            var task = project.task_assignments.FirstOrDefault(x => String.Equals(x.task.name, targetTaskName, StringComparison.InvariantCultureIgnoreCase));
            if (task == null)
            {
                throw new Exception($"Failed to find task '{targetTaskName}'");
            }

            return new Tuple<int, int>(project.project.id, task.task.id);
        }

        private async Task CreateTimeEntryAsync(CreateTimeEntry entry, bool isDryRun)
        {
            await _logger.LogAsync($"{(isDryRun ? "DRY RUN - " : "")}Adding {entry.Hours} hour(s) against project '{entry.ProjectName}' and task '{entry.TaskName}' for client '{entry.ClientName}'");
            if (isDryRun)
            {
                return;
            }

            var request = new RestRequest("v2/time_entries");
            request.AddJsonBody(new
            {
                project_id = entry.ProjectId,
                task_id = entry.TaskId,
                spent_date = entry.SpentDate,
                hours = entry.Hours,
                notes = String.IsNullOrEmpty(entry.Notes) ? "" : entry.Notes
            });

            var response = await _client.ExecutePostAsync(request);
            if (response.IsSuccessful == false)
            {
                throw new SystemException("Failed to create time entry");
            }
        }

        public async Task<IEnumerable<TimeEntry>> GetTimeEntriesAsync(DateTime from, DateTime to)
        {
            var request = new RestRequest($"v2/time_entries?from={from.ToString("yyyy-MM-dd")}&to={to.ToString("yyyy-MM-dd")}");

            var response = await _client.ExecuteGetAsync<TimeEntryResult>(request);
            if (response.IsSuccessful == false)
            {
                throw new SystemException("Failed to retrieve time entries");
            }

            return response.Data.time_entries;
        }

        public async Task<IEnumerable<ProjectAssignment>> GetProjectAssignmentsAsync()
        {
            var request = new RestRequest("v2/users/me/project_assignments");

            var response = await _client.ExecuteGetAsync<ProjectAssignmentsResult>(request);
            if (response.IsSuccessful == false)
            {
                throw new SystemException("Failed to create time entry");
            }

            return response.Data.project_assignments;
        }
    }
}
