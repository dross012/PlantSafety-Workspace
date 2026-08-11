// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Analytics.Culture
//  Layer:          IntegrationTests
//  File:           CultureWritePathIntegrationTests.cs
//  Created:        2026-07-27
//  UPDATED:        2026-07-27 -- expanded from a single Create-then-raw-SQL-read test into the
//                  full Create -> GetById -> GetAll -> audit + concurrency lifecycle bar applied
//                  to IncidentManagement (IncidentLifecycleIntegrationTests.cs). Exercises
//                  ICultureRepository directly (resolved from the real DI container) rather than
//                  through a MediatR query, since no Get/List query handler exists for Culture --
//                  ICultureRepository.AddAsync is the only method CreateCultureCommandHandler
//                  consumes; GetByIdAsync/GetAllAsync exist on the repository but were never
//                  wired to an Application-layer query. Exercising them directly still proves the
//                  real stored procedures work end-to-end; it does not fabricate query
//                  infrastructure that doesn't exist.
//  SchemaVersion:  2026.2
//  Description:    Exercises CreateCultureCommand -- the only real write action in the Analytics
//                  module (every other Analytics stored procedure under
//                  PlantSafety.Infrastructure/Analytics/Sql/Procedures is a read-only "_List"
//                  query) -- through the real MediatR pipeline, then reads it back through the
//                  real CultureRepository.GetByIdAsync/GetAllAsync, against the real
//                  [Analytics].[Culture] table + usp_Culture_Insert/GetById/GetAll stored
//                  procedures applied to the Testcontainers SQL Server instance (see
//                  Setup\TestDatabaseFactory.ApplyAnalyticsSchemaAsync). Asserts the canonical
//                  AuditLog write and the RowVersion-derived ETag are both genuinely populated,
//                  not merely present in the procedure text.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;
using Microsoft.Extensions.DependencyInjection;

using PlantSafety.Application.Analytics.Culture.Commands;
using PlantSafety.Application.Common.Interfaces.Persistence;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;
using PlantSafety.Infrastructure.Analytics.Culture.Persistence;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.IntegrationTests.Analytics.Culture;

/// <summary>
/// Exercises <c>CreateCultureCommand</c> through the real MediatR pipeline, then
/// <c>ICultureRepository.GetByIdAsync</c>/<c>GetAllAsync</c> directly, against the real
/// <c>[Analytics].[Culture]</c> table + stored procedures.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class CultureWritePathIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CultureLifecycle_CreateGetByIdGetAll_ShouldRoundTripThroughRealStoredProceduresWithAuditAndConcurrency()
    {
        var senderScope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = senderScope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            // ---- Create ---------------------------------------------------------
            var command = new CreateCultureCommand(
                TenantId: SharedKernel.Identifiers.TenantId.FromGuid(TenantId),
                UserId: SharedKernel.Identifiers.UserId.FromGuid(ActorId),
                CorrelationId: SharedKernel.Identifiers.CorrelationId.FromGuid(CorrelationId),
                CausationId: CausationId.New(),
                Actor: PerformedByActor.User(SharedKernel.Identifiers.UserId.FromGuid(ActorId)));

            await sender.Send(command, CancellationToken.None);

            // ---- GetById/GetAll (real CultureRepository, real usp_Culture_GetById/GetAll) ----
            // GetByIdAsync/GetAllAsync are not part of ICultureRepository (Application-layer;
            // AddAsync only, per docs/architecture/step-a-iculturerepository-implementation-plan.md)
            // -- they're concrete-only methods on CultureRepository. Only ICultureRepository is
            // registered in DI, so resolve that and cast to the concrete type rather than
            // registering CultureRepository separately (which would risk a second, divergent
            // registration -- exactly the "no ghosts, no duplicates" DI hygiene this pass exists
            // to enforce).
            var (repoScope, cultureRepositoryInterface) = Factory.Services.GetScopedService<ICultureRepository>();
            var repository = (CultureRepository)cultureRepositoryInterface;
            Guid cultureId;
            try
            {
                var all = await repository.GetAllAsync(TenantId, CancellationToken.None);
                all.Should().NotBeEmpty("CreateCultureCommand must have persisted a row via usp_Culture_Insert");
                var created = all.Single(c => c.TenantId == TenantId);
                cultureId = created.CultureId;

                created.Status.Should().Be("Created");
                created.CreatedBy.Should().Be(ActorId);
                // SurveyPeriod/CultureIndexScore/EngagementScore/ParticipationRate/TopRiskTheme
                // all default to empty/zero here -- CreateCultureCommand and Culture.Create's
                // optional parameters (Culture.cs) don't currently supply them; asserting a
                // non-empty value would test a capability CreateCultureCommand doesn't have.
                created.SurveyPeriod.Should().Be(string.Empty);

                // ---- Concurrency: usp_Culture_GetAll must project a real, well-formed
                // ---- ETag derived from the row's native RowVersion column, not an empty
                // ---- placeholder -- proves the ROWVERSION column and its CAST(...AS BINARY(8))
                // ---- projection (docs/architecture/step-b-rowversion-conversion-fix-plan.md)
                // ---- both actually work for Culture, not just for Incident.
                created.ETag.Should().NotBeNullOrWhiteSpace("RowVersion must populate on insert");
                created.ETag.Should().StartWith("0x", "CONVERT(..., 1) always produces a 0x-prefixed hex string");

                var byId = await repository.GetByIdAsync(cultureId, TenantId, CancellationToken.None);
                byId.Should().NotBeNull("usp_Culture_GetById must find the row usp_Culture_Insert just created");
                byId!.CultureId.Should().Be(cultureId);
                byId.ETag.Should().Be(created.ETag, "GetById and GetAll must derive the identical ETag from the same RowVersion value");
            }
            finally
            {
                repoScope.Dispose();
            }

            // ---- Audit: the canonical AuditLog write inside usp_Culture_Insert ---
            await using var conn = new SqlConnection(ConnectionString);
            var auditCount = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM dbo.AuditLog WHERE TableName = @TableName AND Operation = @Operation AND ErrorContext LIKE @Pattern",
                new { TableName = "Culture", Operation = "INSERT", Pattern = $"%CultureId={cultureId}%" });

            auditCount.Should().BeGreaterThan(0,
                "usp_Culture_Insert's canonical AuditLog insert must have recorded this Create, matching every other module's TRY/CATCH + AuditLog + ErrorLog pattern");
        }
        finally
        {
            senderScope.Dispose();
        }
    }
}
