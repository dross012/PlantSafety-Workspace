// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.DrillsAndExercises.Handlers
//  Layer:          Tests (Unit)
//  File:           GetDrillsAndExercisesByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetDrillsAndExercisesByIdQueryHandler -- Phase 4
//                  SafetyEvents.DrillsAndExercises full build-out. Covers: successful retrieval
//                  with ETag projection, not-found behavior with correct severity, and
//                  tenant-isolation (a DrillsAndExercises record belonging to a different tenant
//                  must surface as NotFound, never leaking cross-tenant existence). Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Handlers.GetPermitsByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.SafetyEvents.DrillsAndExercises.Handlers;
using PlantSafety.Application.SafetyEvents.DrillsAndExercises.Models;
using PlantSafety.Application.SafetyEvents.DrillsAndExercises.Queries;
using PlantSafety.Application.SafetyEvents.DrillsAndExercises.Repositories;

namespace PlantSafety.Tests.SafetyEvents.DrillsAndExercises.Handlers;

public sealed class GetDrillsAndExercisesByIdQueryHandlerTests : UnitTestBase
{
    private readonly IDrillsAndExercisesRepository _repo = Substitute.For<IDrillsAndExercisesRepository>();

    private GetDrillsAndExercisesByIdQueryHandler Sut() => new(_repo, NullLogger<GetDrillsAndExercisesByIdQueryHandler>());

    private static DrillsAndExercisesDetailDto DrillFor(Guid drillsAndExercisesId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        DrillsAndExercisesId = drillsAndExercisesId,
        TenantId = tenantId,
        Status = "Scheduled",
        DrillType = "FireDrill",
        ParticipantCount = 0,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenDrillExistsForTenant_ReturnsItemWithEtag()
    {
        var drillId = Guid.NewGuid();
        var drill = DrillFor(drillId, TestTenantId.Value);
        _repo.GetByIdAsync(drillId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(drill);

        var result = await Sut().Handle(new GetDrillsAndExercisesByIdQuery(TestTenantId.Value, drillId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.DrillsAndExercisesId.Should().Be(drillId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenDrillDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((DrillsAndExercisesDetailDto?)null);

        var result = await Sut().Handle(new GetDrillsAndExercisesByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.DrillsAndExercises.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing DrillsAndExercises record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenDrillBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_DrillsAndExercises_GetById filters by TenantId in
        // SQL, so a lookup scoped to TestTenantId for a drill that actually belongs to another
        // tenant returns no row at all -- the repository is not stubbed for that
        // (drillId, TestTenantId) combination and NSubstitute's default (null) applies, exactly
        // as the real proc would.
        var drillId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(drillId, otherTenantId, Arg.Any<CancellationToken>()).Returns(DrillFor(drillId, otherTenantId));

        var result = await Sut().Handle(new GetDrillsAndExercisesByIdQuery(TestTenantId.Value, drillId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.DrillsAndExercises.NotFound");
    }
}
