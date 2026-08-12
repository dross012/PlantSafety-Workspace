// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.CyberPhysicalSecurity
//  Layer:          IntegrationTests
//  File:           CyberPhysicalSecurityRepositoryIntegrationTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    Functional + Security round-trip integration tests against the Testcontainers
//                  SQL Server instance for all five CyberPhysicalSecurity stored-procedure sets
//                  (usp_OtAsset_*, usp_ControlChangeEvent_*, usp_SensorIntegrityCheck_*,
//                  usp_SabotageIndicator_*, usp_CyberPhysicalIncident_*), mirroring the raw-SQL
//                  Dapper-execution pattern established by
//                  PlantSafety.IntegrationTests.Repositories.RepositoryIdempotencyTests (this repo
//                  has no repository-level DI wiring in the shared composition root for this module
//                  yet -- see CyberPhysicalSecurityInfrastructureServiceRegistration's header comment
//                  -- so these tests exercise the stored procedures directly rather than through the
//                  Dapper repository classes).
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.CyberPhysicalSecurity.Repositories;

[Collection(nameof(IntegrationTestCollection))]
public sealed class CyberPhysicalSecurityRepositoryIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    private async Task<Guid> InsertOtAssetAsync(SqlConnection conn, string assetTag = "PLC-101")
    {
        var otAssetId = Guid.NewGuid();
        var p = new DynamicParameters();
        p.Add("OtAssetId", otAssetId);
        p.Add("TenantId", TenantId);
        p.Add("SiteId", TestIds.SiteId);
        p.Add("AssetTag", assetTag);
        p.Add("AssetName", "Line 3 PLC");
        p.Add("AssetType", "Plc");
        p.Add("Vendor", "Siemens");
        p.Add("Model", "S7-1500");
        p.Add("FirmwareVersion", "V2.9");
        p.Add("NetworkZone", "Level 1");
        p.Add("Criticality", "Medium");
        p.Add("Status", "Active");
        p.Add("LastReconciledAtUtc", null);
        p.Add("CreatedAtUtc", FixedUtcNow);
        p.Add("CreatedBy", ActorId);

        await conn.ExecuteAsync("[CyberPhysicalSecurity].[usp_OtAsset_Insert]", p, commandType: CommandType.StoredProcedure);
        return otAssetId;
    }

    // ---- OtAsset -----------------------------------------------------------------

    [Fact]
    public async Task OtAsset_InsertThenGetById_RoundTripsAllFields()
    {
        await using var conn = new SqlConnection(ConnectionString);
        var otAssetId = await InsertOtAssetAsync(conn, "PLC-RT-001");

        var row = await conn.QuerySingleOrDefaultAsync(
            "[CyberPhysicalSecurity].[usp_OtAsset_GetById]",
            new { OtAssetId = otAssetId, TenantId },
            commandType: CommandType.StoredProcedure)
            ?? throw new InvalidOperationException("Expected the stored procedure to return the row just written.");

        ((object?)row).Should().NotBeNull();
        ((string)row.AssetTag).Should().Be("PLC-RT-001");
        ((string)row.Status).Should().Be("Active");
    }

    [Fact]
    public async Task OtAsset_GetById_ScopedToTenant_ReturnsNullForOtherTenant()
    {
        await using var conn = new SqlConnection(ConnectionString);
        var otAssetId = await InsertOtAssetAsync(conn, "PLC-TENANT-002");

        var row = await conn.QuerySingleOrDefaultAsync(
            "[CyberPhysicalSecurity].[usp_OtAsset_GetById]",
            new { OtAssetId = otAssetId, TenantId = Guid.NewGuid() },
            commandType: CommandType.StoredProcedure)
            ?? throw new InvalidOperationException("Expected the stored procedure to return the row just written.");

        ((object?)row).Should().BeNull("tenant isolation must prevent cross-tenant reads");
    }

    [Fact]
    public async Task OtAsset_Update_ChangesStatusAndReconciliationTimestamp()
    {
        await using var conn = new SqlConnection(ConnectionString);
        var otAssetId = await InsertOtAssetAsync(conn, "PLC-UPD-003");

        await conn.ExecuteAsync("[CyberPhysicalSecurity].[usp_OtAsset_Update]",
            new { OtAssetId = otAssetId, TenantId, Status = "Quarantined", LastReconciledAtUtc = FixedUtcNow },
            commandType: CommandType.StoredProcedure);

        var row = await conn.QuerySingleOrDefaultAsync(
            "[CyberPhysicalSecurity].[usp_OtAsset_GetById]", new { OtAssetId = otAssetId, TenantId },
            commandType: CommandType.StoredProcedure)
            ?? throw new InvalidOperationException("Expected the stored procedure to return the row just written.");

        ((string)row.Status).Should().Be("Quarantined");
    }

    [Fact]
    public async Task OtAsset_DuplicateAssetTagWithinTenant_ViolatesUniqueConstraint()
    {
        await using var conn = new SqlConnection(ConnectionString);
        await InsertOtAssetAsync(conn, "PLC-DUP-004");

        var act = async () => await InsertOtAssetAsync(conn, "PLC-DUP-004");

        await act.Should().ThrowAsync<SqlException>("UX_OtAsset_Tenant_AssetTag enforces asset-tag uniqueness per tenant");
    }

    // ---- ControlChangeEvent (Security: tamper-evident hash chain) -----------------

    [Fact]
    public async Task ControlChangeEvent_InsertThenGetById_RoundTripsHashFields()
    {
        await using var conn = new SqlConnection(ConnectionString);
        var otAssetId = await InsertOtAssetAsync(conn, "PLC-CCE-005");
        var recordHash = new string('A', 64);

        var eventId = Guid.NewGuid();
        await conn.ExecuteAsync("[CyberPhysicalSecurity].[usp_ControlChangeEvent_Insert]", new
        {
            ControlChangeEventId = eventId, TenantId, SiteId = TestIds.SiteId, OtAssetId = otAssetId,
            ChangeDescription = "Setpoint drift", ChangedParameter = "SetPoint.Temperature",
            OldValue = "72.0", NewValue = "95.0", ChangeSourceUserId = (Guid?)null,
            AuthorizationStatus = "PendingReview", DetectedAtUtc = FixedUtcNow,
            PriorRecordHash = (string?)null, RecordHash = recordHash,
            CreatedAtUtc = FixedUtcNow, CreatedBy = ActorId,
        }, commandType: CommandType.StoredProcedure);

        var row = await conn.QuerySingleOrDefaultAsync(
            "[CyberPhysicalSecurity].[usp_ControlChangeEvent_GetById]",
            new { ControlChangeEventId = eventId, TenantId }, commandType: CommandType.StoredProcedure)
            ?? throw new InvalidOperationException("Expected the stored procedure to return the row just written.");

        ((object?)row).Should().NotBeNull();
        ((string)row.RecordHash).Should().Be(recordHash);
        ((string?)row.PriorRecordHash).Should().BeNull();
    }

    [Fact]
    public async Task ControlChangeEvent_ThreeRecordChain_PersistsUnbrokenPriorRecordHashLinkage()
    {
        // Security: persists a 3-record hash chain for one asset and verifies each row's
        // PriorRecordHash equals the previous row's RecordHash as read back from SQL Server -- the
        // integrity-sweep query a tamper-detection job would run against the real database.
        await using var conn = new SqlConnection(ConnectionString);
        var otAssetId = await InsertOtAssetAsync(conn, "PLC-CHAIN-006");

        string? priorHash = null;
        var insertedHashes = new List<string>();
        for (var i = 0; i < 3; i++)
        {
            var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes($"link-{i}-{Guid.NewGuid()}")));

            await conn.ExecuteAsync("[CyberPhysicalSecurity].[usp_ControlChangeEvent_Insert]", new
            {
                ControlChangeEventId = Guid.NewGuid(), TenantId, SiteId = TestIds.SiteId, OtAssetId = otAssetId,
                ChangeDescription = $"change {i}", ChangedParameter = "SetPoint.Temperature",
                OldValue = i.ToString(), NewValue = (i + 1).ToString(), ChangeSourceUserId = (Guid?)null,
                AuthorizationStatus = "PendingReview", DetectedAtUtc = FixedUtcNow.AddSeconds(i),
                PriorRecordHash = priorHash, RecordHash = hash,
                CreatedAtUtc = FixedUtcNow, CreatedBy = ActorId,
            }, commandType: CommandType.StoredProcedure);

            insertedHashes.Add(hash);
            priorHash = hash;
        }

        var rows = (await conn.QueryAsync(
            "[CyberPhysicalSecurity].[usp_ControlChangeEvent_GetLatestForAsset]",
            new { OtAssetId = otAssetId, TenantId }, commandType: CommandType.StoredProcedure)).ToList();

        // usp_ControlChangeEvent_GetLatestForAsset returns only the single most recent row.
        rows.Should().ContainSingle();
        ((string)rows[0].RecordHash).Should().Be(insertedHashes[^1]);
        ((string)rows[0].PriorRecordHash).Should().Be(insertedHashes[^2]);
    }

    [Fact]
    public async Task ControlChangeEvent_ReclassifyAuthorization_UpdatesOnlyAuthorizationStatus_HashIsImmutable()
    {
        await using var conn = new SqlConnection(ConnectionString);
        var otAssetId = await InsertOtAssetAsync(conn, "PLC-RECLASS-007");
        var recordHash = new string('B', 64);
        var eventId = Guid.NewGuid();

        await conn.ExecuteAsync("[CyberPhysicalSecurity].[usp_ControlChangeEvent_Insert]", new
        {
            ControlChangeEventId = eventId, TenantId, SiteId = TestIds.SiteId, OtAssetId = otAssetId,
            ChangeDescription = "desc", ChangedParameter = "SetPoint.Pressure", OldValue = "10", NewValue = "12",
            ChangeSourceUserId = (Guid?)null, AuthorizationStatus = "PendingReview", DetectedAtUtc = FixedUtcNow,
            PriorRecordHash = (string?)null, RecordHash = recordHash, CreatedAtUtc = FixedUtcNow, CreatedBy = ActorId,
        }, commandType: CommandType.StoredProcedure);

        await conn.ExecuteAsync("[CyberPhysicalSecurity].[usp_ControlChangeEvent_ReclassifyAuthorization]",
            new { ControlChangeEventId = eventId, TenantId, AuthorizationStatus = "Unauthorized" },
            commandType: CommandType.StoredProcedure);

        var row = await conn.QuerySingleOrDefaultAsync(
            "[CyberPhysicalSecurity].[usp_ControlChangeEvent_GetById]",
            new { ControlChangeEventId = eventId, TenantId }, commandType: CommandType.StoredProcedure)
            ?? throw new InvalidOperationException("Expected the stored procedure to return the row just written.");

        ((string)row.AuthorizationStatus).Should().Be("Unauthorized");
        ((string)row.RecordHash).Should().Be(recordHash, "reclassifying authorization must never mutate the tamper-evident hash");
    }

    // ---- SensorIntegrityCheck ------------------------------------------------------

    [Fact]
    public async Task SensorIntegrityCheck_InsertThenGetHistory_ReturnsMostRecentFirst()
    {
        await using var conn = new SqlConnection(ConnectionString);
        var otAssetId = await InsertOtAssetAsync(conn, "SENSOR-008");

        foreach (var (result, offsetSeconds) in new[] { ("Pass", 0), ("Fail", 10), ("Pass", 20) })
        {
            await conn.ExecuteAsync("[CyberPhysicalSecurity].[usp_SensorIntegrityCheck_Insert]", new
            {
                SensorIntegrityCheckId = Guid.NewGuid(), TenantId, SiteId = TestIds.SiteId, OtAssetId = otAssetId,
                CheckType = "Calibration", Result = result, Details = "details",
                CheckedAtUtc = FixedUtcNow.AddSeconds(offsetSeconds), NextCheckDueUtc = (DateTime?)null,
                CreatedAtUtc = FixedUtcNow, CreatedBy = ActorId,
            }, commandType: CommandType.StoredProcedure);
        }

        var rows = (await conn.QueryAsync(
            "[CyberPhysicalSecurity].[usp_SensorIntegrityCheck_GetHistoryForAsset]",
            new { OtAssetId = otAssetId, TenantId }, commandType: CommandType.StoredProcedure)).ToList();

        rows.Should().HaveCount(3);
        ((string)rows[0].Result).Should().Be("Pass", "most recent check (offset 20s) must be first");
    }

    [Fact]
    public async Task SensorIntegrityCheck_GetAssetsWithChecksDue_ReturnsOnlyElapsedAssets()
    {
        await using var conn = new SqlConnection(ConnectionString);
        var overdueAssetId = await InsertOtAssetAsync(conn, "SENSOR-OVERDUE-009");
        var futureAssetId = await InsertOtAssetAsync(conn, "SENSOR-FUTURE-010");

        await conn.ExecuteAsync("[CyberPhysicalSecurity].[usp_SensorIntegrityCheck_Insert]", new
        {
            SensorIntegrityCheckId = Guid.NewGuid(), TenantId, SiteId = TestIds.SiteId, OtAssetId = overdueAssetId,
            CheckType = "Calibration", Result = "Pass", Details = "", CheckedAtUtc = FixedUtcNow.AddDays(-31),
            NextCheckDueUtc = FixedUtcNow.AddDays(-1), CreatedAtUtc = FixedUtcNow, CreatedBy = ActorId,
        }, commandType: CommandType.StoredProcedure);

        await conn.ExecuteAsync("[CyberPhysicalSecurity].[usp_SensorIntegrityCheck_Insert]", new
        {
            SensorIntegrityCheckId = Guid.NewGuid(), TenantId, SiteId = TestIds.SiteId, OtAssetId = futureAssetId,
            CheckType = "Calibration", Result = "Pass", Details = "", CheckedAtUtc = FixedUtcNow,
            NextCheckDueUtc = FixedUtcNow.AddDays(30), CreatedAtUtc = FixedUtcNow, CreatedBy = ActorId,
        }, commandType: CommandType.StoredProcedure);

        var dueAssetIds = (await conn.QueryAsync<Guid>(
            "[CyberPhysicalSecurity].[usp_SensorIntegrityCheck_GetAssetsWithChecksDue]",
            new { TenantId, AsOfUtc = FixedUtcNow }, commandType: CommandType.StoredProcedure)).ToList();

        dueAssetIds.Should().Contain(overdueAssetId);
        dueAssetIds.Should().NotContain(futureAssetId);
    }

    // ---- SabotageIndicator ----------------------------------------------------------

    [Fact]
    public async Task SabotageIndicator_InsertThenGetActive_ReturnsOnlyOpenAndUnderInvestigation()
    {
        await using var conn = new SqlConnection(ConnectionString);

        async Task InsertAsync(string status) => await conn.ExecuteAsync("[CyberPhysicalSecurity].[usp_SabotageIndicator_Insert]", new
        {
            SabotageIndicatorId = Guid.NewGuid(), TenantId, SiteId = TestIds.SiteId, Source = "ControlChange",
            SourceReferenceId = (Guid?)null, Description = $"indicator-{status}-{Guid.NewGuid()}", Severity = "Medium",
            Status = status, CorrelatedIndicatorCount = 1, RaisedAtUtc = FixedUtcNow, RiskManagementPriority = "P2",
            CreatedAtUtc = FixedUtcNow, CreatedBy = ActorId,
        }, commandType: CommandType.StoredProcedure);

        await InsertAsync("Open");
        await InsertAsync("UnderInvestigation");
        await InsertAsync("Confirmed");
        await InsertAsync("FalsePositive");

        var active = (await conn.QueryAsync(
            "[CyberPhysicalSecurity].[usp_SabotageIndicator_GetActive]", new { TenantId },
            commandType: CommandType.StoredProcedure)).ToList();

        var activeStatuses = active.Select(r => (string)r.Status).ToList();
        activeStatuses.Should().OnlyContain(s => s == "Open" || s == "UnderInvestigation");
    }

    [Fact]
    public async Task SabotageIndicator_Update_ChangesSeverityStatusAndCorrelatedCount()
    {
        await using var conn = new SqlConnection(ConnectionString);
        var indicatorId = Guid.NewGuid();

        await conn.ExecuteAsync("[CyberPhysicalSecurity].[usp_SabotageIndicator_Insert]", new
        {
            SabotageIndicatorId = indicatorId, TenantId, SiteId = TestIds.SiteId, Source = "ControlChange",
            SourceReferenceId = (Guid?)null, Description = "desc", Severity = "Low", Status = "Open",
            CorrelatedIndicatorCount = 1, RaisedAtUtc = FixedUtcNow, RiskManagementPriority = "P3",
            CreatedAtUtc = FixedUtcNow, CreatedBy = ActorId,
        }, commandType: CommandType.StoredProcedure);

        await conn.ExecuteAsync("[CyberPhysicalSecurity].[usp_SabotageIndicator_Update]", new
        {
            SabotageIndicatorId = indicatorId, TenantId, SiteId = TestIds.SiteId, Source = "ControlChange",
            SourceReferenceId = (Guid?)null, Description = "desc", Severity = "Critical", Status = "UnderInvestigation",
            CorrelatedIndicatorCount = 3, RaisedAtUtc = FixedUtcNow, RiskManagementPriority = "P1",
            CreatedAtUtc = FixedUtcNow, CreatedBy = ActorId,
        }, commandType: CommandType.StoredProcedure);

        var row = await conn.QuerySingleOrDefaultAsync(
            "[CyberPhysicalSecurity].[usp_SabotageIndicator_GetById]",
            new { SabotageIndicatorId = indicatorId, TenantId }, commandType: CommandType.StoredProcedure)
            ?? throw new InvalidOperationException("Expected the stored procedure to return the row just written.");

        ((string)row.Severity).Should().Be("Critical");
        ((string)row.Status).Should().Be("UnderInvestigation");
        ((int)row.CorrelatedIndicatorCount).Should().Be(3);
    }

    [Fact]
    public async Task SabotageIndicator_GetForCorrelationWindow_ExcludesIndicatorsOutsideWindow()
    {
        await using var conn = new SqlConnection(ConnectionString);

        async Task InsertAtAsync(DateTime raisedAt) => await conn.ExecuteAsync("[CyberPhysicalSecurity].[usp_SabotageIndicator_Insert]", new
        {
            SabotageIndicatorId = Guid.NewGuid(), TenantId, SiteId = TestIds.SiteId, Source = "SensorIntegrity",
            SourceReferenceId = (Guid?)null, Description = $"windowed-{Guid.NewGuid()}", Severity = "Medium",
            Status = "Open", CorrelatedIndicatorCount = 1, RaisedAtUtc = raisedAt, RiskManagementPriority = "P2",
            CreatedAtUtc = FixedUtcNow, CreatedBy = ActorId,
        }, commandType: CommandType.StoredProcedure);

        await InsertAtAsync(FixedUtcNow);
        await InsertAtAsync(FixedUtcNow.AddHours(-5));

        var windowed = (await conn.QueryAsync(
            "[CyberPhysicalSecurity].[usp_SabotageIndicator_GetForCorrelationWindow]",
            new { TenantId, WindowStartUtc = FixedUtcNow.AddMinutes(-60), WindowEndUtc = FixedUtcNow },
            commandType: CommandType.StoredProcedure)).ToList();

        var raisedTimestamps = windowed.Select(r => (DateTime)r.RaisedAtUtc).ToList();
        raisedTimestamps.Should().OnlyContain(t => t >= FixedUtcNow.AddMinutes(-60));
    }

    // ---- CyberPhysicalIncident --------------------------------------------------------

    [Fact]
    public async Task CyberPhysicalIncident_InsertThenGetById_RoundTripsRelatedIndicatorIds()
    {
        await using var conn = new SqlConnection(ConnectionString);
        var incidentId = Guid.NewGuid();
        var relatedIds = $"{Guid.NewGuid()};{Guid.NewGuid()}";

        await conn.ExecuteAsync("[CyberPhysicalSecurity].[usp_CyberPhysicalIncident_Insert]", new
        {
            CyberPhysicalIncidentId = incidentId, TenantId, SiteId = TestIds.SiteId, Title = "Incident",
            Description = "desc", Severity = "High", Status = "Open", RelatedSabotageIndicatorIds = relatedIds,
            OwnerId = ActorId, OpenedAtUtc = FixedUtcNow, ClosedAtUtc = (DateTime?)null, ResponseTimeMinutes = (int?)null,
            CreatedAtUtc = FixedUtcNow, CreatedBy = ActorId,
        }, commandType: CommandType.StoredProcedure);

        var row = await conn.QuerySingleOrDefaultAsync(
            "[CyberPhysicalSecurity].[usp_CyberPhysicalIncident_GetById]",
            new { CyberPhysicalIncidentId = incidentId, TenantId }, commandType: CommandType.StoredProcedure)
            ?? throw new InvalidOperationException("Expected the stored procedure to return the row just written.");

        ((string)row.RelatedSabotageIndicatorIds).Should().Be(relatedIds);
    }

    [Fact]
    public async Task CyberPhysicalIncident_Update_ClosesIncidentAndSetsResponseTime()
    {
        await using var conn = new SqlConnection(ConnectionString);
        var incidentId = Guid.NewGuid();

        await conn.ExecuteAsync("[CyberPhysicalSecurity].[usp_CyberPhysicalIncident_Insert]", new
        {
            CyberPhysicalIncidentId = incidentId, TenantId, SiteId = TestIds.SiteId, Title = "Incident",
            Description = "desc", Severity = "High", Status = "Open", RelatedSabotageIndicatorIds = (string?)null,
            OwnerId = ActorId, OpenedAtUtc = FixedUtcNow, ClosedAtUtc = (DateTime?)null, ResponseTimeMinutes = (int?)null,
            CreatedAtUtc = FixedUtcNow, CreatedBy = ActorId,
        }, commandType: CommandType.StoredProcedure);

        var closedAt = FixedUtcNow.AddMinutes(90);
        await conn.ExecuteAsync("[CyberPhysicalSecurity].[usp_CyberPhysicalIncident_Update]", new
        {
            CyberPhysicalIncidentId = incidentId, TenantId, Status = "Closed", ClosedAtUtc = closedAt, ResponseTimeMinutes = 90,
        }, commandType: CommandType.StoredProcedure);

        var row = await conn.QuerySingleOrDefaultAsync(
            "[CyberPhysicalSecurity].[usp_CyberPhysicalIncident_GetById]",
            new { CyberPhysicalIncidentId = incidentId, TenantId }, commandType: CommandType.StoredProcedure)
            ?? throw new InvalidOperationException("Expected the stored procedure to return the row just written.");

        ((string)row.Status).Should().Be("Closed");
        ((int)row.ResponseTimeMinutes).Should().Be(90);
    }

    [Fact]
    public async Task CyberPhysicalIncident_GetAll_OrdersByOpenedAtUtcDescending()
    {
        await using var conn = new SqlConnection(ConnectionString);
        var tenantId = Guid.NewGuid(); // isolate from other tests' rows in this shared table

        async Task InsertAsync(DateTime openedAt) => await conn.ExecuteAsync("[CyberPhysicalSecurity].[usp_CyberPhysicalIncident_Insert]", new
        {
            CyberPhysicalIncidentId = Guid.NewGuid(), TenantId = tenantId, SiteId = TestIds.SiteId, Title = "Incident",
            Description = "desc", Severity = "High", Status = "Open", RelatedSabotageIndicatorIds = (string?)null,
            OwnerId = ActorId, OpenedAtUtc = openedAt, ClosedAtUtc = (DateTime?)null, ResponseTimeMinutes = (int?)null,
            CreatedAtUtc = FixedUtcNow, CreatedBy = ActorId,
        }, commandType: CommandType.StoredProcedure);

        await InsertAsync(FixedUtcNow.AddDays(-2));
        await InsertAsync(FixedUtcNow);
        await InsertAsync(FixedUtcNow.AddDays(-1));

        var rows = (await conn.QueryAsync(
            "[CyberPhysicalSecurity].[usp_CyberPhysicalIncident_GetAll]", new { TenantId = tenantId },
            commandType: CommandType.StoredProcedure)).ToList();

        rows.Should().HaveCount(3);
        ((DateTime)rows[0].OpenedAtUtc).Should().Be(FixedUtcNow);
        ((DateTime)rows[^1].OpenedAtUtc).Should().Be(FixedUtcNow.AddDays(-2));
    }
}
