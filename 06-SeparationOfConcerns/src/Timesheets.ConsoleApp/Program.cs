using Timesheets;

var path = args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, "timesheet-sample.csv");

if (!File.Exists(path))
{
    Console.Error.WriteLine($"File not found: {path}");
    return 1;
}

try
{
    Console.Write(TimesheetReportPipeline.Run(File.ReadAllText(path)));
    return 0;
}
catch (FormatException exception)
{
    Console.Error.WriteLine($"Not a timesheet export: {exception.Message}");
    return 2;
}
