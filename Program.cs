using SchedulingService.Data;

var ctx = new SchedulingDbContext();
try
{
    var canConnect = ctx.Database.CanConnect();
    Console.WriteLine(canConnect ? "Connected to database successfully." : "Could not connect to database.");
}
catch (Exception ex)
{
    Console.WriteLine($"Database connection failed: {ex.Message}");
}
