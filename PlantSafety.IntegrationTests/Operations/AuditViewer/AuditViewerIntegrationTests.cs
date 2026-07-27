// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Operations.AuditViewer
//  Layer:          IntegrationTests
//  File:           AuditViewerIntegrationTests.cs
//  Created:        2026-07-27
//  SchemaVersion:  2026.1
//  Description:    Cross-module integration tests verifying that PlantSafety's
//                  Operations.AuditViewer read path (SearchAuditLogQuery ->
//                  SearchAuditLogQueryHandler) round-trips end-to-end through the
//                  extracted Shared.AuditLogging module -- real DI wiring
//                  (services.AddSharedAuditLogging), the real AuditLogDapperRepository,
//                  and a real dbo.AuditLog table + dbo.usp_SearchAuditLog stored
//                  procedure applied to the Testcontainers SQL Server instance (see
//                  Setup\TestDatabaseFactory.ApplySharedAuditLoggingSchemaAsync).
//
//                  This does not go through HTTP because
//                  OperationsAuditViewerV1Controller requires authentication and this
//                  test host does not configure a test authentication handler (see
//                  Financials\Api\FinancialsApiTests.cs, which only asserts 401 for
//                  the equivalent case) -- resolving MediatR's ISender from the DI
//                  container mirrors the established EndToEnd pattern used by
//                  Financials\EndToEnd\FinancialsEndToEndTests.cs, exercising the same
//                  Application -> Shared.AuditLogging.Infrastructure -> SQL pipeline
//                  the controller itself calls into.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.Operations.AuditViewer.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

using Shared.AuditLogging.Abstractions.Models;

namespace PlantSafety.IntegrationTests.Operations.AuditViewer;

/// <summary>
/// Verifies that PlantSafety's Operations.AuditViewer query pipeline correctly
/// round-trips <see cref="AuditLogSearchCriteria"/> and <see cref="AuditLogSearchResult"/>
/// through the shared Shared.AuditLogging module's real repository and stored procedure.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class AuditViewerIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    private const string ModuleName = "AuditViewerIntegrationTests";

    [Fact]
    public async Task SearchAuditLogQuery_ShouldRoundTripThroughSharedAuditLogging()
    {
        // Arrange — seed rows directly into dbo.AuditLog, the exact table
        // AuditLogDapperRepository (Shared.AuditLogging.Infrastructure) reads via
        // dbo.usp_SearchAuditLog. AuditLog is append-only/write-once by hundreds of stored
        // procedures in production, so seeding via direct INSERT here (rather than via any
        // PlantSafety write path) matches how the table is actually populated live.
        var changedBy = $"svc_{Guid.NewGuid():N}";
        var changeTime = FixedUtcNow;

        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            @"INSERT INTO dbo.AuditLog
                (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module, BeforeSnapshot, AfterSnapshot)
              VALUES
                (@TableName, @SourceDatabase, @Operation, @ChangedBy, @ChangeTime, @ViewerAccessed, @ErrorContext, @Severity, @Module, NULL, NULL)",
            new
            {
                TableName = "AiGovernance",
                SourceDatabase = "PlantSafety.CoreDB",
                Operation = "INSERT",
                ChangedBy = changedBy,
                ChangeTime = changeTime,
                ViewerAccessed = false,
                ErrorContext = (string?)null,
                Severity = 0,
                Module = ModuleName
            });

        var criteria = new AuditLogSearchCriteria(Module: ModuleName, ChangedBy: changedBy, PageNumber: 1, PageSize: 50);

        // Act — resolve ISender from the real, fully-wired DI container (the same
        // container OperationsAuditViewerV1Controller resolves from) and send the query
        // exactly as the controller does.
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        AuditLogSearchResult result;
        try
        {
            var response = await sender.Send(new SearchAuditLogQuery(criteria), CancellationToken.None);
            response.IsSuccess.Should().BeTrue("SearchAuditLogQueryHandler should succeed against a healthy AuditDB");
            result = response.Value;
        }
        finally
        {
            scope.Dispose();
        }

        // Assert — the query pipeline returned the shared repository's own result type,
        // correctly round-tripped and filtered by the criteria we passed in.
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1, "exactly one AuditLog row matches the seeded Module/ChangedBy filter");
        result.Items.Should().ContainSingle();

        var entry = result.Items.Single();
        entry.TableName.Should().Be("AiGovernance");
        entry.SourceDatabase.Should().Be("PlantSafety.CoreDB");
        entry.Operation.Should().Be("INSERT");
        entry.ChangedBy.Should().Be(changedBy);
        entry.Module.Should().Be(ModuleName);
        entry.ChangeTime.Should().BeCloseTo(changeTime, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task SearchAuditLogQuery_WithNoMatches_ShouldReturnEmptyResultNotFailure()
    {
        var criteria = new AuditLogSearchCriteria(
            Module: $"{ModuleName}.NoSuchModule.{Guid.NewGuid():N}", PageNumber: 1, PageSize: 50);

        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var response = await sender.Send(new SearchAuditLogQuery(criteria), CancellationToken.None);

            response.IsSuccess.Should().BeTrue();
            response.Value.Items.Should().BeEmpty();
            response.Value.TotalCount.Should().Be(0);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task AuditLogTable_And_SearchProcedure_ShouldExist()
    {
        // SQL-layer contract check, mirroring Financials\Sql\FinancialsSqlTests.cs --
        // guards against the Testcontainers schema setup silently drifting from the
        // canonical Shared\AuditLogging\sql scripts.
        await using var conn = new SqlConnection(ConnectionString);

        var tableExists = await conn.ExecuteScalarAsync<int>(
            @"SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES
              WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'AuditLog'");
        tableExists.Should().Be(1, "dbo.AuditLog must exist before running AuditViewer integration tests");

        var procedureExists = await conn.ExecuteScalarAsync<int>(
            @"SELECT COUNT(1) FROM sys.procedures WHERE name = 'usp_SearchAuditLog'");
        procedureExists.Should().Be(1, "dbo.usp_SearchAuditLog must exist before running AuditViewer integration tests");
    }
}
