// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.Observation.Handlers
//  Layer:          Tests (Unit)
//  File:           GetObservationByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetObservationByIdQueryHandler -- Phase 4
//                  SafetyEvents.Observation full build-out. Covers: successful retrieval with
//                  ETag projection, not-found behavior with correct severity, and
//                  tenant-isolation (an Observation record belonging to a different tenant must
//                  surface as NotFound, never leaking cross-tenant existence). Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Handlers.GetPermitsByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.SafetyEvents.Observation.Handlers;
using PlantSafety.Application.SafetyEvents.Observation.Models;
using PlantSafety.Application.SafetyEvents.Observation.Queries;
using PlantSafety.Application.SafetyEvents.Observation.Repositories;

namespace PlantSafety.Tests.SafetyEvents.Observation.Handlers;

public sealed class GetObservationByIdQueryHandlerTests : UnitTestBase
{
    private readonly IObservationRepository _repo = Substitute.For<IObservationRepository>();

    private GetObservationByIdQueryHandler Sut() => new(_repo, NullLogger<GetObservationByIdQueryHandler>());

    private static ObservationItemDto ObservationFor(Guid observationId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        ObservationId = observationId,
        TenantId = tenantId,
        Status = "Created",
        ObservationType = "AtRiskBehavior",
        Category = "Slip/Trip",
        ObservedAtUtc = FixedUtcNow,
        ObservedBy = TestUserId.Value,
        Location = "Warehouse B",
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenObservationExistsForTenant_ReturnsItemWithEtag()
    {
        var observationId = Guid.NewGuid();
        var observation = ObservationFor(observationId, TestTenantId.Value);
        _repo.GetByIdAsync(observationId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(observation);

        var result = await Sut().Handle(new GetObservationByIdQuery(TestTenantId.Value, observationId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ObservationId.Should().Be(observationId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenObservationDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ObservationItemDto?)null);

        var result = await Sut().Handle(new GetObservationByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.Observation.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing Observation record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenObservationBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_ObservationItems_GetById filters by TenantId in SQL,
        // so a lookup scoped to TestTenantId for a record that actually belongs to another tenant
        // returns no row at all -- the repository is not stubbed for that
        // (observationId, TestTenantId) combination and NSubstitute's default (null) applies,
        // exactly as the real proc would.
        var observationId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(observationId, otherTenantId, Arg.Any<CancellationToken>()).Returns(ObservationFor(observationId, otherTenantId));

        var result = await Sut().Handle(new GetObservationByIdQuery(TestTenantId.Value, observationId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.Observation.NotFound");
    }
}
