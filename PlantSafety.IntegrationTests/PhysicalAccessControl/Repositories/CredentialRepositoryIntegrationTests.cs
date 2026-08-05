// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.PhysicalAccessControl
//  Layer:          IntegrationTests
//  File:           CredentialRepositoryIntegrationTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Repository/DB round-trip integration tests for the Credential aggregate --
//                  exercises the real, DI-resolved ICredentialRepository (CredentialDapperRepository)
//                  against the real [PhysicalAccessControl].[Credentials] table and
//                  usp_Credentials_Create/Update/GetById/GetStatusItem stored procedures. The
//                  read-model assertions specifically prove the SQL-layer badge-number masking
//                  (usp_Credentials_GetStatusItem: ('****' + RIGHT(BadgeNumber, 4))) applied to
//                  this module's PII surface, per Security test-category coverage.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.Domain.PhysicalAccessControl.Credentials.Enums;
using PlantSafety.Domain.PhysicalAccessControl.Credentials.Interfaces;
using PlantSafety.Application.PhysicalAccessControl.Credentials.Repositories;
using PlantSafety.IntegrationTests.Common.TestFixtures;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using CredentialAggregate = PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates.Credential;

namespace PlantSafety.IntegrationTests.PhysicalAccessControl.Repositories;

[Collection(nameof(IntegrationTestCollection))]
public sealed class CredentialRepositoryIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_ShouldRoundTripThroughRealStoredProcedures()
    {
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICredentialRepository>();

        var createdBy = UserId.FromGuid(Guid.NewGuid());
        var holderUserId = Guid.NewGuid();
        var aggregate = CredentialAggregate.Create(
            createdBy, UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()),
            PerformedByActor.User(createdBy),
            tenantId: TenantId,
            badgeNumber: "BADGE-IT-778899",
            holderUserId: holderUserId,
            holderType: CredentialHolderType.Contractor,
            accessLevel: "RestrictedZoneA");

        await repository.AddAsync(aggregate, CancellationToken.None);

        var fetched = await repository.GetByIdAsync(aggregate.CredentialId, CancellationToken.None);

        fetched.Should().NotBeNull();
        fetched!.BadgeNumber.Should().Be("BADGE-IT-778899");
        fetched.HolderUserId.Should().Be(holderUserId);
        fetched.HolderType.Should().Be(CredentialHolderType.Contractor);
        fetched.Status.Should().Be(CredentialStatus.Issued);
    }

    [Fact]
    public async Task UpdateAsync_AfterRevoke_ShouldPersistRevocationReason()
    {
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICredentialRepository>();

        var createdBy = UserId.FromGuid(Guid.NewGuid());
        var actor = PerformedByActor.User(createdBy);
        var aggregate = CredentialAggregate.Create(
            createdBy, UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()), actor,
            tenantId: TenantId, badgeNumber: "BADGE-IT-990011", holderUserId: Guid.NewGuid());

        await repository.AddAsync(aggregate, CancellationToken.None);

        aggregate.Revoke("Badge reported stolen", UtcTimestamp.Now(), actor, new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()));
        await repository.UpdateAsync(aggregate, CancellationToken.None);

        var afterRevoke = await repository.GetByIdAsync(aggregate.CredentialId, CancellationToken.None);

        afterRevoke!.Status.Should().Be(CredentialStatus.Revoked);
        afterRevoke.RevocationReason.Should().Be("Badge reported stolen");
    }

    [Fact]
    public async Task ReadModel_GetStatusAsync_ShouldReturnMaskedBadgeNumber()
    {
        using var scope = Factory.Services.CreateScope();
        var writeRepository = scope.ServiceProvider.GetRequiredService<ICredentialRepository>();
        var readRepository = scope.ServiceProvider.GetRequiredService<ICredentialReadRepository>();

        var createdBy = UserId.FromGuid(Guid.NewGuid());
        var aggregate = CredentialAggregate.Create(
            createdBy, UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()),
            PerformedByActor.User(createdBy),
            tenantId: TenantId, badgeNumber: "BADGE-IT-654321", holderUserId: Guid.NewGuid());

        await writeRepository.AddAsync(aggregate, CancellationToken.None);

        var statusDto = await readRepository.GetStatusAsync(TenantId, aggregate.CredentialId.Value, CancellationToken.None);

        statusDto.Should().NotBeNull();
        statusDto!.BadgeNumberMasked.Should().Be("****4321",
            "the real usp_Credentials_GetStatusItem stored procedure masks all but the last 4 digits at the SQL layer");
        statusDto.BadgeNumberMasked.Should().NotContain("BADGE-IT-654321",
            "the raw badge number must never appear anywhere in the projection returned to callers");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICredentialRepository>();

        var fetched = await repository.GetByIdAsync(
            PlantSafety.Domain.PhysicalAccessControl.Credentials.ValueObjects.Ids.CredentialId.New(), CancellationToken.None);

        fetched.Should().BeNull();
    }
}
