// ============================================================================
// EmergencyMgtIntegration.Tests -- real, runnable xUnit facts against the LIVE PlantSafety.CoreDB
// (and, for one cross-check, the live EmergencyMgt EmergencyManagementDB) named instances on this
// machine. NOT a Testcontainers harness -- no such harness exists for PlantSafety.CoreDB anywhere
// in this repo (see ci/test-template.yml's header). These tests are real and genuinely runnable
// today because both named instances are real, live, already-deployed local SQL Server instances,
// not because a container-based fixture was built.
// ============================================================================
using Dapper;
using Microsoft.Data.SqlClient;
using Xunit;

namespace EmergencyMgtIntegration.Tests;

public sealed class EmergencyMgtIntegrationTests
{
    private const string PlantSafetyConnString = @"Server=.\PLANTSAFETY;Database=PlantSafety.CoreDB;Trusted_Connection=True;TrustServerCertificate=True;";
    private const string EmergencyMgtConnString = @"Server=.\EMERGENCYMGT;Database=EmergencyManagementDB;Trusted_Connection=True;TrustServerCertificate=True;";

    [Fact]
    public async Task ValidateEmergencyMgtIntegration_AllChecksPass()
    {
        await using var conn = new SqlConnection(PlantSafetyConnString);
        var rows = (await conn.QueryAsync<(string CheckName, bool Passed, string? Detail)>(
            "EmergencyMgtIntegration.usp_ValidateEmergencyMgtIntegration")).ToList();

        Assert.NotEmpty(rows);
        var failed = rows.Where(r => !r.Passed).ToList();
        Assert.True(failed.Count == 0, "Failing checks: " + string.Join(", ", failed.Select(f => $"{f.CheckName} ({f.Detail})")));
    }

    [Fact]
    public async Task RoutingKeyRegistry_HasExactly20ActiveKeys()
    {
        await using var conn = new SqlConnection(PlantSafetyConnString);
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM EmergencyMgtIntegration.RoutingKeyRegistry WHERE IsActive = 1");
        Assert.Equal(20, count);
    }

    [Fact]
    public async Task EmergencySummaryInbound_UpsertThenRead_RoundTripsCorrectly()
    {
        await using var conn = new SqlConnection(PlantSafetyConnString);
        var eventId = Guid.NewGuid();
        var startedUtc = DateTime.UtcNow;

        await conn.ExecuteAsync(new CommandDefinition(
            "EmergencyMgtIntegration.usp_EmergencySummaryInbound_Upsert",
            new
            {
                SourceEmergencyEventId = eventId,
                EmergencyTypeName = "IntegrationTest",
                Severity = "Low",
                StatusCode = "Declared",
                IsStatusTerminal = false,
                Location = "TestFacility",
                InitiatedBy = "xUnit",
                StartedUtc = startedUtc,
                EndedUtc = (DateTime?)null,
            },
            commandType: System.Data.CommandType.StoredProcedure));

        var row = await conn.QuerySingleAsync(
            "SELECT * FROM EmergencyMgtIntegration.vwEmergencySummary WHERE SourceEmergencyEventId = @eventId",
            new { eventId });

        Assert.Equal("IntegrationTest", (string)row.EmergencyTypeName);
        Assert.Equal("Declared", (string)row.StatusCode);
        Assert.False((bool)row.IsStatusTerminal);

        // Cleanup -- this schema has no delete proc by design (append/upsert-only feed), so
        // direct DML cleanup is used, matching the pattern already established in this session's
        // own smoke tests.
        await conn.ExecuteAsync("DELETE FROM EmergencyMgtIntegration.EmergencySummaryInbound WHERE SourceEmergencyEventId = @eventId", new { eventId });
    }

    [Fact]
    public async Task VersionManifest_ComputeAndRecord_ProducesAHashedRow()
    {
        await using var conn = new SqlConnection(PlantSafetyConnString);
        var label = $"xunit-{Guid.NewGuid():N}";

        var row = await conn.QuerySingleAsync(new CommandDefinition(
            "EmergencyMgtIntegration.usp_VersionManifest_ComputeAndRecord",
            new { SchemaVersionLabel = label },
            commandType: System.Data.CommandType.StoredProcedure));

        Assert.Equal(label, (string)row.SchemaVersionLabel);
        Assert.NotNull(row.ManifestHash);
    }

    [Fact]
    public async Task DriftCheck_RecordThenResolve_ReflectsInUnresolvedList()
    {
        await using var conn = new SqlConnection(PlantSafetyConnString);

        await conn.ExecuteAsync(new CommandDefinition(
            "EmergencyMgtIntegration.usp_DriftCheck_Record",
            new { ObjectName = "xUnitTestObject", DriftType = "Schema", Detail = "Synthetic drift for test." },
            commandType: System.Data.CommandType.StoredProcedure));

        var unresolved = (await conn.QueryAsync(new CommandDefinition(
            "EmergencyMgtIntegration.usp_DriftCheck_GetUnresolved", commandType: System.Data.CommandType.StoredProcedure))).ToList();

        var testRow = unresolved.SingleOrDefault(r => (string)r.ObjectName == "xUnitTestObject");
        Assert.NotNull(testRow);

        Guid driftCheckId = testRow!.DriftCheckId;
        await conn.ExecuteAsync(new CommandDefinition(
            "EmergencyMgtIntegration.usp_DriftCheck_Resolve",
            new { DriftCheckId = driftCheckId },
            commandType: System.Data.CommandType.StoredProcedure));

        var stillUnresolved = (await conn.QueryAsync(new CommandDefinition(
            "EmergencyMgtIntegration.usp_DriftCheck_GetUnresolved", commandType: System.Data.CommandType.StoredProcedure))).ToList();
        Assert.DoesNotContain(stillUnresolved, r => (string)r.ObjectName == "xUnitTestObject");
    }

    [Fact]
    public async Task IntegrationHealthCheck_ReturnsARow()
    {
        await using var conn = new SqlConnection(PlantSafetyConnString);
        var row = await conn.QuerySingleAsync(new CommandDefinition(
            "EmergencyMgtIntegration.usp_IntegrationHealth_Check", commandType: System.Data.CommandType.StoredProcedure));

        Assert.Equal(20, (int)row.ActiveRoutingKeyCount);
    }

    [Fact]
    public async Task EmergencyMgt_SourceInstance_IsReachableAndHasTheExpectedTables()
    {
        // Cross-check that the pusher's assumed source schema still matches reality -- catches
        // schema drift on the EmergencyMgt side before it silently breaks the pusher.
        await using var conn = new SqlConnection(EmergencyMgtConnString);
        var tableCount = await conn.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
              WHERE s.name = 'EmergencyManagement' AND t.name IN ('EmergencyEvent', 'EmergencyType', 'EmergencyCorrectiveAction', 'EmergencyReadinessScore')");
        Assert.Equal(4, tableCount);
    }
}
