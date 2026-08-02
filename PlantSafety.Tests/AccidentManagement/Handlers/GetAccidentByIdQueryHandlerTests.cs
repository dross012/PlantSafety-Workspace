// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AccidentManagement.Handlers
//  Layer:          Tests (Unit)
//  File:           GetAccidentByIdQueryHandlerTests.cs
//  Created:        2026-07-29
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetAccidentByIdQueryHandler -- closes the Phase 4
//                  AccidentManagement read-model gap. Covers: successful retrieval with ETag
//                  projection, not-found behavior, and tenant-isolation (an Accident belonging to
//                  a different tenant must surface as NotFound). Mirrors
//                  PlantSafety.Tests.RiskManagement.Handlers.GetRiskByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.AccidentManagement.Handlers;
using PlantSafety.Application.AccidentManagement.Models;
using PlantSafety.Application.AccidentManagement.Queries;
using PlantSafety.Application.AccidentManagement.Repositories;
using PlantSafety.Application.Common.Errors;

namespace PlantSafety.Tests.AccidentManagement.Handlers;

public sealed class GetAccidentByIdQueryHandlerTests : UnitTestBase
{
    private readonly IAccidentRepository _repo = Substitute.For<IAccidentRepository>();

    private GetAccidentByIdQueryHandler Sut() => new(_repo, NullLogger<GetAccidentByIdQueryHandler>());

    private static AccidentItemDto AccidentFor(Guid accidentId, Guid tenantId, string? eTag = "0x0000000000004321") => new()
    {
        AccidentId = accidentId,
        TenantId = tenantId,
        Status = "Reported",
        AccidentTitle = "Forklift collision near loading dock",
        Location = "Warehouse B",
        Severity = "Moderate",
        InjuredPersonCount = 1,
        OccurredAtUtc = FixedUtcNow,
        Category = "Reported",
        SiteSecuredAndTriaged = false,
        RegulatoryFilingRequired = false,
        OwnerId = TestUserId.Value,
        ReviewRequired = true,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenAccidentExistsForTenant_ReturnsItemWithEtag()
    {
        var accidentId = Guid.NewGuid();
        var accident = AccidentFor(accidentId, TestTenantId.Value);
        _repo.GetByIdAsync(accidentId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(accident);

        var result = await Sut().Handle(new GetAccidentByIdQuery(TestTenantId.Value, accidentId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccidentId.Should().Be(accidentId);
        result.Value.ETag.Should().Be("0x0000000000004321");
    }

    [Fact]
    public async Task Handle_WhenAccidentDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AccidentItemDto?)null);

        var result = await Sut().Handle(new GetAccidentByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AccidentManagement.Accident.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing Accident must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenAccidentBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_Accident_GetById filters by TenantId in SQL, so a
        // lookup scoped to TestTenantId for an Accident that actually belongs to another tenant
        // returns no row -- the repository is not stubbed for that (AccidentId, TestTenantId)
        // combination and NSubstitute's default (null) applies, exactly as the real proc would.
        var accidentId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(accidentId, otherTenantId, Arg.Any<CancellationToken>()).Returns(AccidentFor(accidentId, otherTenantId));

        var result = await Sut().Handle(new GetAccidentByIdQuery(TestTenantId.Value, accidentId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AccidentManagement.Accident.NotFound");
    }
}
