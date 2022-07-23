# Timesheet Filler

## Prepare

To prepare your environment, we utilise [asdf](https://github.com/asdf-vm/asdf). Follow their installation guide and then run

```
# You may need additional requirements as per https://github.com/asdf-vm/asdf-nodejs
asdf plugin add nodejs https://github.com/asdf-vm/asdf-nodejs.git

asdf plugin-add dotnet-core https://github.com/emersonsoares/asdf-dotnet-core.git
```

## Building

To build, run the following:

```bash
cd src && dotnet pack
```

## Installing

To install a local version, run the following after building:

```bash
dotnet tool install --global --add-source ./src/output TimesheetFiller.Cli
```

## Running

To run, run the following:

```bash
timesheet-filler -i config.yaml
```