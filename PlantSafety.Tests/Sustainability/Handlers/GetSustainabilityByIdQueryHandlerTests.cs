// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Sustainability.Handlers
//  Layer:          Tests (Unit)
//  File:           GetSustainabilityByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetSustainabilityByIdQueryHandler -- Phase 4 Sustainability
//                  full build-out. Covers: successful retrieval with ETag projection, not-found
//                  behavior with correct severity, and tenant-isolation. Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Handlers.GetAuditingByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Sustainability.Handlers;
using PlantSafety.Application.Sustainability.Models;
using PlantSafety.Application.Sustainability.Queries;
using PlantSafety.Application.Sustainability.Repositories;

namespace PlantSafety.Tests.Sustainability.Handlers;

public sealed class GetSustainabilityByIdQueryHandlerTests : UnitTestBase
{
    private readonly ISustainabilityRepository _repo = Substitute.For<ISustainabilityRepository>();

    private GetSustainabilityByIdQueryHandler Sut() => new(_repo, NullLogger<GetSustainabilityByIdQueryHandler>());

    private static SustainabilityItemDto SustainabilityFor(Guid sustainabilityId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        SustainabilityId = sustainabilityId,
        TenantId = tenantId,
        Status = "Created",
        InitiativeName = "Site-Wide Emissions Reduction",
        Category = "Emissions",
        TargetValue = 1000m,
        OwnerUserId = TestUserId.Value,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenSustainabilityExistsForTenant_ReturnsItemWithEtag()
    {
        var sustainabilityId = Guid.NewGuid();
        var sustainability = SustainabilityFor(sustainabilityId, TestTenantId.Value);
        _repo.GetByIdAsync(sustainabilityId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(sustainability);

        var result = await Sut().Handle(new GetSustainabilityByIdQuery(TestTenantId.Value, sustainabilityId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.SustainabilityId.Should().Be(sustainabilityId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenSustainabilityDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((SustainabilityItemDto?)null);

        var result = await Sut().Handle(new GetSustainabilityByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Sustainability.Sustainability.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing Sustainability record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenSustainabilityBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_SustainabilityAggregate_GetById filters by TenantId in
        // SQL, so a lookup scoped to TestTenantId for a record that actually belongs to another
        // tenant returns no row at all -- the repository is not stubbed for that
        // (SustainabilityId, TestTenantId) combination and NSubstitute's default (null) applies,
        // exactly as the real proc would.
        var sustainabilityId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(sustainabilityId, otherTenantId, Arg.Any<CancellationToken>()).Returns(SustainabilityFor(sustainabilityId, otherTenantId));

        var result = await Sut().Handle(new GetSustainabilityByIdQuery(TestTenantId.Value, sustainabilityId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Sustainability.Sustainability.NotFound");
    }
}
