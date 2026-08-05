// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Security
//  Layer:          Tests (Security)
//  File:           LocalEmergencyCoordinationSecurityTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Security tests: tenant isolation must hold across every query handler and every
//                  mutating command handler -- a record belonging to a different tenant must never
//                  be readable or mutable via another tenant's caller context, and must surface as
//                  NotFound (never a distinguishable "forbidden" signal that would leak existence).
//                  Isolated, deterministic, no I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Handlers;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Queries;
using AppRepositories = PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Repositories;
using DomainInterfaces = PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Interfaces;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.ValueObjects.Ids;
using PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.TestSupport;
using LocalEmergencyCoordinationAggregate = PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Security;

public sealed class LocalEmergencyCoordinationSecurityTests : UnitTestBase
{
    [Fact]
    public async Task GetById_WhenCaseBelongsToAnotherTenant_ReturnsNotFound_NeverForbidden()
    {
        var repo = Substitute.For<AppRepositories.ILocalEmergencyCoordinationRepository>();
        var caseId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        repo.GetByIdAsync(caseId, otherTenantId, Arg.Any<CancellationToken>()).Returns(new LocalEmergencyCoordinationItemDto
        {
            LocalEmergencyCoordinationId = caseId,
            TenantId = otherTenantId,
            CaseName = "Other Tenant's Case",
            Status = "Active",
            CreatedAtUtc = FixedUtcNow,
            CreatedBy = Guid.NewGuid(),
        });

        var handler = new GetLocalEmergencyCoordinationByIdQueryHandler(repo, NullLogger<GetLocalEmergencyCoordinationByIdQueryHandler>());
        var result = await handler.Handle(new GetLocalEmergencyCoordinationByIdQuery(TestTenantId.Value, caseId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "cross-tenant access must never surface as Forbidden -- that would confirm the record's existence to an unauthorized caller");
    }

    [Fact]
    public async Task MutualAidAgreements_ListedForOneTenant_NeverIncludeAnotherTenantsAgreements()
    {
        var repo = Substitute.For<AppRepositories.ILocalEmergencyCoordinationRepository>();
        var tenantAgreements = new List<MutualAidAgreementItemDto>
        {
            new()
            {
                MutualAidAgreementId = Guid.NewGuid(),
                LocalEmergencyCoordinationId = Guid.NewGuid(),
                TenantId = TestTenantId.Value,
                AgencyName = "Tenant-Scoped Agency",
                Discipline = "Fire",
                EffectiveFromUtc = FixedUtcNow,
                RenewalDueAtUtc = FixedUtcNow.AddYears(1),
                RenewalStatus = "Current",
                EstablishedAtUtc = FixedUtcNow,
                EstablishedBy = TestUserId.Value,
            },
        };
        repo.ListMutualAidAgreementsAsync(TestTenantId.Value, null, Arg.Any<CancellationToken>()).Returns(tenantAgreements);

        var handler = new ListMutualAidAgreementsQueryHandler(repo, NullLogger<ListMutualAidAgreementsQueryHandler>());
        var result = await handler.Handle(new ListMutualAidAgreementsQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().OnlyContain(a => a.TenantId == TestTenantId.Value);
    }

    [Fact]
    public async Task EstablishMutualAidAgreement_WhenCaseBelongsToAnotherTenant_ReturnsNotFound_NeverMutatesAggregate()
    {
        // The domain write-side repository's GetByIdAsync is itself the tenant boundary: it is
        // stubbed for (id, otherTenantId) only, so a caller resolved to TestTenantId gets null,
        // exactly as the real tenant-scoped stored procedure would behave.
        var repo = Substitute.For<DomainInterfaces.ILocalEmergencyCoordinationRepository>();
        var otherTenantId = Guid.NewGuid();
        var agg = LocalEmergencyCoordinationTestSupport.NewCase(tenantId: otherTenantId);
        repo.GetByIdAsync(agg.LocalEmergencyCoordinationId, otherTenantId, Arg.Any<CancellationToken>()).Returns(agg);

        var handler = new EstablishMutualAidAgreementCommandHandler(repo, NullLogger<EstablishMutualAidAgreementCommandHandler>());
        var command = new EstablishMutualAidAgreementCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            agg.LocalEmergencyCoordinationId.Value, "Agency", "Fire", 1, FixedUtcNow, FixedUtcNow.AddYears(1), TestUserId.Value);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
        agg.MutualAidAgreements.Should().BeEmpty("a cross-tenant command must never be allowed to mutate another tenant's aggregate");
        await repo.DidNotReceive().UpdateAsync(Arg.Any<LocalEmergencyCoordinationAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }
}
