# Timesheet Filler

Fills in timesheets stored in Harvest, using Google Calendar events to record specific tasks.

This has been built in .NET with the intention of being installed as a .NET CLI tool.

## Building

To build, run the following:

```bash
cd src && dotnet pack
```

## Installing

To install, run the following after building:

```bash
dotnet tool install --global --add-source ./src/output TimesheetFiller.Cli
```

## Running

To run, run the following:

```bash
timesheet-filler -i config.yaml
```

## Config

A config file is required, which is used to configure Google Calendar and Harvest and how events should be turned in Harvest tasks.

```yaml
google:
  clientId: {{SENSITIVE}}
  clientSecret: {{SENSITIVE}}
  calendarIds:
  - test@group.calendar.google.com
harvest:
  accountId: {{SENSITIVE}}
  apiToken: {{SENSITIVE}}
  # This represents how many hours you should be targeting each day. If all events for the day don't add up to this time, then the remainder will be logged against the default task.
  defaultHoursPerDay: 8
  defaultClientName: CompanyX # This should match the name of the target Client in Harvest
  defaultProjectName: SuperProject # This should match the name of the target Project in Harvest
  defaultTaskName: Development # This should match the name of the target Task in Harvest
  tasks:
  # The tasks that calendar events should be logged against when the summary partially matches the specified searh term (case insensitive)
  - calendarSearchTerm: Super Super Project
    clientName: CompanyY
    projectName: SuperSuperProject
    taskName: Meeting
```

You will need to set up the following accounts for access:

### Google Application

You will need to follow these [docs](https://www.roundthecode.com/dotnet/how-to-add-google-authentication-to-a-asp-net-core-application) and add the secrets to the configuration file. When it comes to scopes in the setup process, you'll need to add the following scopes:
- calendar.calendars.readonly
- calendar.events.readonly

When you run the tool, you will be prompted to sign into your target Google account.

### Harvest

You will need to create a [Personal Access Token](https://help.getharvest.com/api-v2/authentication-api/authentication/authentication/#personal-access-tokens) and add it to the configuration file.