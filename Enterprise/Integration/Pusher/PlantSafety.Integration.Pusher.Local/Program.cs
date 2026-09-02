// ============================================================================
// PlantSafety.Integration.Pusher.Local -- Program.cs
// Console entry point. Intended to run as a scheduled task (mirrors EmergencyMgt's own
// tools/*.ps1 "no CI runner, run manually or via Windows Task Scheduler" precedent -- see
// EmergencyMgt docs/deviations.md DEV-020). Trusted-connection (Windows auth) to both named
// instances, matching every other tool in either repo. No secrets in source or config.
// ============================================================================
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using PlantSafety.Integration.Pusher.Local;

var emergencyMgtConn = args.Length > 0 ? args[0] : @"Server=.\EMERGENCYMGT;Database=EmergencyManagementDB;Trusted_Connection=True;TrustServerCertificate=True;";
var plantSafetyConn = args.Length > 1 ? args[1] : @"Server=.\PLANTSAFETY;Database=PlantSafety.CoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
var logger = loggerFactory.CreateLogger<LocalEmergencyMgtPusher>();

var pusher = new LocalEmergencyMgtPusher(emergencyMgtConn, plantSafetyConn, logger);
var result = await pusher.PushAsync(CancellationToken.None);

Console.WriteLine();
Console.WriteLine($"=== EmergencyMgt -> PlantSafety Pusher: {result.Status} ===");
Console.WriteLine($"Succeeded: {result.SucceededCount}  Failed: {result.FailedCount}  Elapsed: {(result.CompletedUtc - result.StartedUtc).TotalMilliseconds:N0}ms");
foreach (var (feed, detail) in result.FeedDetails)
{
    Console.WriteLine($"  [{feed}] {detail}");
}

// Record a Health signal into PlantSafety's EmergencyGovernanceLog reflecting this run's own
// outcome, so /integration/emergency/health and future runs can see when the pusher last ran
// and whether it succeeded -- a real, live health signal, not a placeholder.
await using (var target = new SqlConnection(plantSafetyConn))
{
    await target.OpenAsync();
    var payload = System.Text.Json.JsonSerializer.Serialize(new
    {
        status = result.Status.ToString(),
        succeededCount = result.SucceededCount,
        failedCount = result.FailedCount,
        startedUtc = result.StartedUtc,
        completedUtc = result.CompletedUtc,
    });
    await target.ExecuteAsync(new CommandDefinition(
        "[EmergencyMgtIntegration].[usp_EmergencyGovernanceLog_Record]",
        new { SignalType = "Health", PayloadJson = payload },
        commandType: System.Data.CommandType.StoredProcedure));
}

Environment.Exit(result.Status == PlantSafety.Integration.Pusher.Abstractions.IntegrationPushStatus.Failed ? 1 : 0);
