// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Analytics.Culture
//  Layer:          IntegrationTests
//  File:           CultureWritePathIntegrationTests.cs
//  Created:        2026-07-27
//  SchemaVersion:  2026.1
//  Description:    Exercises CreateCultureCommand -- the only real write action in the Analytics
//                  module (every other Analytics stored procedure under
//                  PlantSafety.Infrastructure/Analytics/Sql/Procedures is a read-only "_List"
//                  query) -- through the real MediatR pipeline, the real CultureRepository, and
//                  the real [Analytics].[Culture] table + usp_Culture_Insert/GetById/GetAll
//                  stored procedures applied to the Testcontainers SQL Server instance (see
//                  Setup\TestDatabaseFactory.ApplyAnalyticsSchemaAsync). Neither the Culture table
//                  nor its stored procedures existed anywhere in the codebase before this pass,
//                  even though CultureRepository referenced them by name -- CreateCultureCommand
//                  could never have succeeded against a real database until now.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.Analytics.Culture.Commands;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.IntegrationTests.Analytics.Culture;

/// <summary>
/// Exercises <c>CreateCultureCommand</c> through the real MediatR pipeline, the real
/// <c>CultureRepository</c>, and the real <c>[Analytics].[Culture]</c> table + stored procedures.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class CultureWritePathIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateCulture_ShouldPersistToRealCultureTable()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var command = new CreateCultureCommand(
                TenantId: SharedKernel.Identifiers.TenantId.FromGuid(TenantId),
                UserId: SharedKernel.Identifiers.UserId.FromGuid(ActorId),
                CorrelationId: SharedKernel.Identifiers.CorrelationId.FromGuid(CorrelationId),
                CausationId: CausationId.New(),
                Actor: PerformedByActor.User(SharedKernel.Identifiers.UserId.FromGuid(ActorId)));

            await sender.Send(command, CancellationToken.None);

            await using var conn = new SqlConnection(ConnectionString);
            var rows = (await conn.QueryAsync(
                @"SELECT CultureId, TenantId, Status, SurveyPeriod, CreatedBy
                  FROM [Analytics].[Culture]
                  WHERE TenantId = @TenantId",
                new { TenantId })).ToList();

            rows.Should().NotBeEmpty("CreateCultureCommand must persist a real row via usp_Culture_Insert");
            var row = rows.Single();
            ((string)row.Status).Should().Be("Created");
            ((Guid)row.TenantId).Should().Be(TenantId);
            ((Guid)row.CreatedBy).Should().Be(ActorId);
        }
        finally
        {
            scope.Dispose();
        }
    }
}
