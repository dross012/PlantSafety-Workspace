// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.Queries
//  Layer:          Tests (Unit)
//  File:           ListPublicSafetyAgenciesQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler tests for ListPublicSafetyAgenciesQueryHandler. Isolated,
//                  deterministic, <50ms per test. No I/O.
// ============================================================================
using PlantSafety.Application.PublicSafetyCommunication.AgencyDirectory.Handlers;
using PlantSafety.Application.PublicSafetyCommunication.AgencyDirectory.Models;
using PlantSafety.Application.PublicSafetyCommunication.AgencyDirectory.Queries;
using PlantSafety.Application.PublicSafetyCommunication.AgencyDirectory.Repositories;

namespace PlantSafety.Tests.PublicSafetyCommunication.Queries;

public sealed class ListPublicSafetyAgenciesQueryHandlerTests : UnitTestBase
{
    private readonly IPublicSafetyAgencyRepository _repo = Substitute.For<IPublicSafetyAgencyRepository>();

    private ListPublicSafetyAgenciesQueryHandler Sut() => new(_repo, NullLogger<ListPublicSafetyAgenciesQueryHandler>());

    private static PublicSafetyAgencyItemDto SampleItem(Guid tenantId) => new()
    {
        PublicSafetyAgencyId = Guid.NewGuid(),
        TenantId = tenantId,
        Status = "Active",
        AgencyName = "County Fire Department",
        AgencyType = "Fire",
        JurisdictionDescription = "County district 4",
        PrimaryPhoneNumber = "555-0100",
        VerificationStatus = "Unverified",
        CreatedAtUtc = FixedUtcNow,
    };

    [Fact]
    public async Task Handle_ReturnsItemsFromRepositoryScopedToRequest()
    {
        var items = new[] { SampleItem(TestTenantId.Value) };
        _repo.ListPublicSafetyAgenciesAsync(TestTenantId.Value, null, null, false, Arg.Any<CancellationToken>())
            .Returns(items);

        var query = new ListPublicSafetyAgenciesQuery(TestTenantId.Value, null, null, false);
        var result = await Sut().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be(TestTenantId.Value);
        result.Value.Items.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task Handle_WithNoMatchingRecords_ReturnsEmptyItemsNotFailure()
    {
        _repo.ListPublicSafetyAgenciesAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<PublicSafetyAgencyItemDto>());

        var query = new ListPublicSafetyAgenciesQuery(TestTenantId.Value, null, "Fire", true);
        var result = await Sut().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.ListPublicSafetyAgenciesAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<PublicSafetyAgencyItemDto>>>(_ => throw new InvalidOperationException("boom"));

        var query = new ListPublicSafetyAgenciesQuery(TestTenantId.Value, null, null, false);
        var result = await Sut().Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PublicSafetyCommunication.PublicSafetyAgency.ListFailed");
    }
}
