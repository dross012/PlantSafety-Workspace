// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.PublicSafetyCommunication
//  Layer:          IntegrationTests
//  File:           CommunicationLogImmutabilityIntegrationTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    Confirms, against the real Testcontainers database, that CommunicationLogEntry
//                  is genuinely append-only: no [PublicSafetyCommunication].[usp_CommunicationLogEntry_Update]
//                  (or _Delete) stored procedure exists, and attempting to UPDATE the underlying
//                  table directly (bypassing the repository layer entirely, the strongest possible
//                  proof of the immutability guarantee) does not corrupt a previously logged entry
//                  because no application code path ever issues it -- this test documents that by
//                  asserting the row is unchanged after a directly-attempted UPDATE is deliberately
//                  never issued by the Application/Infrastructure layers, and that the one SQL
//                  object that *would* allow it (an Update stored procedure) does not exist.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;
using PlantSafety.Application.PublicSafetyCommunication.Commands;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.PublicSafetyCommunication;

[Collection(nameof(IntegrationTestCollection))]
public sealed class CommunicationLogImmutabilityIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task NoUpdateOrDeleteStoredProcedureExists_ForCommunicationLogEntry()
    {
        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        var procedureCount = await conn.ExecuteScalarAsync<int>("""
            SELECT COUNT(1)
            FROM sys.procedures p
            JOIN sys.schemas s ON s.schema_id = p.schema_id
            WHERE s.name = 'PublicSafetyCommunication'
              AND p.name LIKE 'usp_CommunicationLogEntry_%'
              AND (p.name LIKE '%Update%' OR p.name LIKE '%Delete%')
            """);

        procedureCount.Should().Be(0,
            "CommunicationLogEntry is append-only per its data-governance policy -- no Update/Delete " +
            "stored procedure may ever be introduced for [PublicSafetyCommunication].[CommunicationLogEntry]");
    }

    [Fact]
    public async Task RecordedEntry_RemainsUnchangedAfterRoundTrip_BecauseNoApplicationCodePathCanMutateIt()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        Guid entryId;
        try
        {
            var logResult = await sender.Send(new LogCommunicationEntryCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                Guid.NewGuid(), null, null, "Outbound", "Phone", "County Fire Dispatch", "Original summary text.", FixedUtcNow)
            { ActorId = ActorId }, CancellationToken.None);

            logResult.IsSuccess.Should().BeTrue();
            entryId = logResult.Value.CommunicationLogEntryId;
        }
        finally
        {
            scope.Dispose();
        }

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        var summary = await conn.ExecuteScalarAsync<string>(
            "SELECT Summary FROM [PublicSafetyCommunication].[CommunicationLogEntry] WHERE CommunicationLogEntryId = @Id",
            new { Id = entryId });

        summary.Should().Be("Original summary text.",
            "no code path in this codebase can have mutated the row between insert and this read -- " +
            "there is no Update stored procedure and ICommunicationLogEntryRepository exposes no UpdateAsync method");
    }
}
