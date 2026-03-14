using System;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SchedulingService.Data;

internal static class AgentLogger
{
    private const string LogPath = "debug-4847dc.log";

    private static void WriteLine(string jsonLine)
    {
        try
        {
            File.AppendAllText(LogPath, jsonLine + Environment.NewLine);
        }
        catch
        {
            // Swallow all logging errors
        }
    }

    private static string BuildEntry(string hypothesisId, string message, string dataJson)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        // dataJson must be a valid JSON object literal (e.g. "{}" or "{\"key\":\"value\"}")
        return
            $"{{\"sessionId\":\"4847dc\",\"runId\":\"initial\",\"hypothesisId\":\"{hypothesisId}\",\"location\":\"Program.cs\",\"message\":\"{message}\",\"data\":{dataJson},\"timestamp\":{timestamp}}}";
    }

    public static void Log(string hypothesisId, string message, string dataJson = "{}")
    {
        // #region agent log
        WriteLine(BuildEntry(hypothesisId, message, dataJson));
        // #endregion
    }
}

internal class Program
{
    private static void Main(string[] args)
    {
        try
        {
            AgentLogger.Log("H1", "Starting DB connectivity check");

            var connStr = Environment.GetEnvironmentVariable("SCHEDULING_DB_CS");
            if (!string.IsNullOrWhiteSpace(connStr))
            {
                try
                {
                    var builder = new SqlConnectionStringBuilder(connStr);
                    Console.WriteLine($"Using server: {builder.DataSource}, database: {builder.InitialCatalog}");
                }
                catch { /* ignore parse error */ }
            }

            using var context = new SchedulingDbContext();

            AgentLogger.Log("H2", "Created SchedulingDbContext instance");

            var canConnect = context.Database.CanConnect();

            AgentLogger.Log("H3", "Result from Database.CanConnect()", $"{{\"canConnect\":{canConnect.ToString().ToLowerInvariant()}}}");

            if (canConnect)
            {
                Console.WriteLine("Database connection successful.");
            }
            else
            {
                Console.WriteLine("Database connection failed.");
                // CanConnect() returns false without throwing; open connection to get the actual error
                try
                {
                    context.Database.GetDbConnection().Open();
                }
                catch (Exception openEx)
                {
                    Console.WriteLine($"Reason: {openEx.Message}");
                    AgentLogger.Log("H5", "OpenConnection exception", $"{{\"message\":\"{openEx.Message.Replace("\"", "\\\"")}\"}}");
                }
            }
        }
        catch (Exception ex)
        {
            AgentLogger.Log(
                "H4",
                "Exception during DB connectivity check",
                $"{{\"exceptionType\":\"{ex.GetType().FullName}\",\"message\":\"{ex.Message}\"}}"
            );

            Console.WriteLine($"Error while trying to connect to the database: {ex.Message}");
        }

        Console.WriteLine("Press any key to exit...");
        if (!Console.IsInputRedirected)
        {
            Console.ReadKey();
        }
    }
}