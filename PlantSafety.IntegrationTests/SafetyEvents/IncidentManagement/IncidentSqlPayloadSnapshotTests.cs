// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.SafetyEvents.IncidentManagement
//  Layer:          IntegrationTests
//  File:           IncidentSqlPayloadSnapshotTests.cs
//  Created:        2026-07-27
//  SchemaVersion:  2026.1
//  Description:    Pins the exact persisted row shape written to [SafetyEvents].[Incident] by
//                  IncidentDapperRepository.CreateAsync/UpdateAsync (via usp_Incident_Create /
//                  usp_Incident_Update), reading every column back with a raw SqlConnection
//                  rather than through usp_IncidentRead_* -- those read procedures only project
//                  a subset of columns, so a regression in a column they don't select (e.g.
//                  LastModifiedByActorId, DeletedAtUtc, RowVersion advancing) would pass
//                  IncidentLifecycleIntegrationTests.cs without being caught. This test exists
//                  specifically to catch silent drift in the CreateAsync/UpdateAsync ->
//                  StoredProcedureBuilder parameter mapping (see IncidentDapperRepository.cs)
//                  if someone edits it later without noticing a field stopped round-tripping.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Create;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Update;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.SafetyEvents.IncidentManagement;

/// <summary>
/// Pins the exact <c>[SafetyEvents].[Incident]</c> row shape produced by
/// <c>IncidentDapperRepository.CreateAsync</c>/<c>UpdateAsync</c>, read back column-by-column
/// with a raw query rather than through the read-side stored procedures.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class IncidentSqlPayloadSnapshotTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateIncident_ShouldPersistExactRowShape()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var siteId = Guid.NewGuid();
            var reportedByWorkerId = Guid.NewGuid();
            var occurredAtUtc = DateTimeOffset.UtcNow.AddHours(-2);

            var createCommand = new CreateIncidentCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = null,
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                SiteId = siteId,
                Title = "Snapshot payload probe incident",
                Description = "Used only to pin the exact persisted SQL row shape.",
                IncidentOccurredAtUtc = occurredAtUtc,
                Severity = "Minor",
                Location = "Snapshot Test Zone",
                ReportedByWorkerId = reportedByWorkerId,
                AssignedInvestigatorId = null,
            };

            var incidentId = await sender.Send(createCommand, CancellationToken.None);

            var row = await ReadIncidentRowAsync(incidentId);

            row.Should().NotBeNull("CreateAsync must have inserted exactly one row");
            row!.IncidentId.Should().Be(incidentId);
            row.TenantId.Should().Be(TenantId);
            row.SiteId.Should().Be(siteId);
            row.Title.Should().Be(createCommand.Title);
            row.Description.Should().Be(createCommand.Description);
            row.IncidentOccurredAtUtc.Should().BeCloseTo(occurredAtUtc.UtcDateTime, TimeSpan.FromSeconds(1));
            row.ReportedAtUtc.Should().NotBe(default);
            row.Severity.Should().Be("Minor");
            row.Status.Should().Be("Reported", "usp_Incident_Create must default Status via DF_Incident_Status");
            row.Location.Should().Be(createCommand.Location);
            row.ReportedByWorkerId.Should().Be(reportedByWorkerId);
            row.AssignedInvestigatorId.Should().BeNull();
            row.ClosedAtUtc.Should().BeNull();
            row.DeletionReason.Should().BeNull();
            row.DeletedAtUtc.Should().BeNull();
            row.IsDeleted.Should().BeFalse();
            row.LastModifiedByActorId.Should().Be(ActorId,
                "the canonical @ActorId header must be written to LastModifiedByActorId on insert");
            row.LastModifiedAtUtc.Should().NotBe(default);
            row.CreatedAtUtc.Should().NotBe(default);
            row.CreatedBy.Should().Be(ActorId);
            row.RowVersion.Should().NotBeNullOrEmpty("ROWVERSION must be populated for ETag-based concurrency control");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task UpdateIncident_ShouldPersistExactPartialDiff_LeavingUntouchedColumnsUnchanged()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var siteId = Guid.NewGuid();
            var reportedByWorkerId = Guid.NewGuid();
            var investigatorId = Guid.NewGuid();

            var createCommand = new CreateIncidentCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = null,
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                SiteId = siteId,
                Title = "Pre-update snapshot probe incident",
                Description = "Original description, must survive a partial update untouched.",
                IncidentOccurredAtUtc = DateTimeOffset.UtcNow.AddHours(-3),
                Severity = "Minor",
                Location = "Original Location",
                ReportedByWorkerId = reportedByWorkerId,
                AssignedInvestigatorId = null,
            };

            var incidentId = await sender.Send(createCommand, CancellationToken.None);
            var rowAfterCreate = await ReadIncidentRowAsync(incidentId);
            rowAfterCreate.Should().NotBeNull();

            // Only Severity + AssignedInvestigatorId are supplied; Title/Description/Location are
            // null (the ISNULL/COALESCE partial-update contract) and must round-trip unchanged.
            var updateCommand = new UpdateIncidentCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = null,
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                ETag = rowAfterCreate!.RowVersion,
                IncidentId = incidentId,
                Title = null,
                Description = null,
                Severity = "Serious",
                Location = null,
                AssignedInvestigatorId = investigatorId,
            };

            await sender.Send(updateCommand, CancellationToken.None);
            var rowAfterUpdate = await ReadIncidentRowAsync(incidentId);

            rowAfterUpdate.Should().NotBeNull();
            rowAfterUpdate!.Severity.Should().Be("Serious", "Severity was supplied on the update");
            rowAfterUpdate.AssignedInvestigatorId.Should().Be(investigatorId, "AssignedInvestigatorId was supplied on the update");

            // Untouched-field contract: everything not supplied on Update must be byte-for-byte
            // identical to the post-create row -- this is the exact behavior the ISNULL/COALESCE
            // partial-update stored procedure pattern promises, and the invariant the aggregate
            // migration in item 3 was required to preserve rather than regress to a full overwrite.
            rowAfterUpdate.Title.Should().Be(rowAfterCreate.Title);
            rowAfterUpdate.Description.Should().Be(rowAfterCreate.Description);
            rowAfterUpdate.Location.Should().Be(rowAfterCreate.Location);
            rowAfterUpdate.SiteId.Should().Be(rowAfterCreate.SiteId);
            rowAfterUpdate.IncidentOccurredAtUtc.Should().Be(rowAfterCreate.IncidentOccurredAtUtc);
            rowAfterUpdate.ReportedAtUtc.Should().Be(rowAfterCreate.ReportedAtUtc);
            rowAfterUpdate.ReportedByWorkerId.Should().Be(rowAfterCreate.ReportedByWorkerId);
            rowAfterUpdate.Status.Should().Be(rowAfterCreate.Status);
            rowAfterUpdate.CreatedAtUtc.Should().Be(rowAfterCreate.CreatedAtUtc);
            rowAfterUpdate.CreatedBy.Should().Be(rowAfterCreate.CreatedBy);
            rowAfterUpdate.IsDeleted.Should().BeFalse();

            // Mutated-metadata contract: an UPDATE must always advance these two, regardless of
            // which business fields changed.
            rowAfterUpdate.LastModifiedByActorId.Should().Be(ActorId);
            rowAfterUpdate.LastModifiedAtUtc.Should().BeOnOrAfter(rowAfterCreate.LastModifiedAtUtc);
            rowAfterUpdate.RowVersion.Should().NotBe(rowAfterCreate.RowVersion,
                "ROWVERSION must advance on every successful write, which is what the ETag-guarded concurrency check depends on");
        }
        finally
        {
            scope.Dispose();
        }
    }

    private async Task<IncidentRow?> ReadIncidentRowAsync(Guid incidentId)
    {
        await using var conn = new SqlConnection(ConnectionString);
        return await conn.QuerySingleOrDefaultAsync<IncidentRow>(
            """
            SELECT
                IncidentId, TenantId, SiteId, Title, Description, IncidentOccurredAtUtc,
                ReportedAtUtc, Severity, Status, Location, ReportedByWorkerId,
                AssignedInvestigatorId, ClosedAtUtc, DeletionReason, DeletedAtUtc, IsDeleted,
                LastModifiedByActorId, LastModifiedAtUtc, CreatedAtUtc, CreatedBy,
                CONVERT(NVARCHAR(34), RowVersion, 1) AS RowVersion
            FROM [SafetyEvents].[Incident]
            WHERE IncidentId = @IncidentId
            """,
            new { IncidentId = incidentId });
    }

    /// <summary>Raw projection of every column in <c>[SafetyEvents].[Incident]</c> for exact-shape assertions.</summary>
    private sealed record IncidentRow
    {
        public Guid IncidentId { get; init; }
        public Guid TenantId { get; init; }
        public Guid SiteId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public DateTime IncidentOccurredAtUtc { get; init; }
        public DateTime ReportedAtUtc { get; init; }
        public string Severity { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;
        public Guid ReportedByWorkerId { get; init; }
        public Guid? AssignedInvestigatorId { get; init; }
        public DateTime? ClosedAtUtc { get; init; }
        public string? DeletionReason { get; init; }
        public DateTime? DeletedAtUtc { get; init; }
        public bool IsDeleted { get; init; }
        public Guid LastModifiedByActorId { get; init; }
        public DateTime LastModifiedAtUtc { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public Guid CreatedBy { get; init; }
        public string RowVersion { get; init; } = string.Empty;
    }
}
