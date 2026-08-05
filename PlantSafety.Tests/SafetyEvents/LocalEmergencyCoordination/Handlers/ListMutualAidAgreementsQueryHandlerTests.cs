// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Handlers
//  Layer:          Tests (Unit)
//  File:           ListMutualAidAgreementsQueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ListMutualAidAgreementsQueryHandler. Isolated, deterministic,
//                  <50ms per test.
// ============================================================================
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Handlers;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Queries;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Repositories;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Handlers;

public sealed class ListMutualAidAgreementsQueryHandlerTests : UnitTestBase
{
    private readonly ILocalEmergencyCoordinationRepository _repo = Substitute.For<ILocalEmergencyCoordinationRepository>();

    private ListMutualAidAgreementsQueryHandler Sut() => new(_repo, NullLogger<ListMutualAidAgreementsQueryHandler>());

    private MutualAidAgreementItemDto Agreement() => new()
    {
        MutualAidAgreementId = Guid.NewGuid(),
        LocalEmergencyCoordinationId = Guid.NewGuid(),
        TenantId = TestTenantId.Value,
        AgencyName = "Metro Fire Dept",
        Discipline = "Fire",
        DocumentVersion = 1,
        EffectiveFromUtc = FixedUtcNow,
        RenewalDueAtUtc = FixedUtcNow.AddYears(1),
        RenewalStatus = "Current",
        EstablishedAtUtc = FixedUtcNow,
        EstablishedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_ReturnsAgreementsScopedToTenantAndSite()
    {
        var siteId = Guid.NewGuid();
        var agreements = new List<MutualAidAgreementItemDto> { Agreement() };
        _repo.ListMutualAidAgreementsAsync(TestTenantId.Value, siteId, Arg.Any<CancellationToken>()).Returns(agreements);

        var result = await Sut().Handle(new ListMutualAidAgreementsQuery(TestTenantId.Value, siteId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEquivalentTo(agreements);
        result.Value.SiteId.Should().Be(siteId);
    }

    [Fact]
    public async Task Handle_WithNullSiteId_ReturnsTenantWideResults()
    {
        _repo.ListMutualAidAgreementsAsync(TestTenantId.Value, null, Arg.Any<CancellationToken>()).Returns(new List<MutualAidAgreementItemDto>());

        var result = await Sut().Handle(new ListMutualAidAgreementsQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.ListMutualAidAgreementsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<MutualAidAgreementItemDto>>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new ListMutualAidAgreementsQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.LocalEmergencyCoordination.ListMutualAidAgreementsFailed");
    }
}
