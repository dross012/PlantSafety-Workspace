// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Scheduling.EmergencyDrillPlanning.Handlers
//  Layer:          Tests (Unit)
//  File:           GetEmergencyDrillPlanningByIdQueryHandlerTests.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetEmergencyDrillPlanningByIdQueryHandler -- Phase 4
//                  Scheduling.EmergencyDrillPlanning full build-out. Covers: successful retrieval
//                  with ETag projection, not-found behavior with correct severity, and
//                  tenant-isolation (a record belonging to a different tenant must surface as
//                  NotFound, never leaking cross-tenant existence). Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Handlers.GetPermitsByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Handlers;
using PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Models;
using PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Queries;
using PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Repositories;

namespace PlantSafety.Tests.Scheduling.EmergencyDrillPlanning.Handlers;

public sealed class GetEmergencyDrillPlanningByIdQueryHandlerTests : UnitTestBase
{
    private readonly IEmergencyDrillPlanningRepository _repo = Substitute.For<IEmergencyDrillPlanningRepository>();

    private GetEmergencyDrillPlanningByIdQueryHandler Sut() => new(_repo, NullLogger<GetEmergencyDrillPlanningByIdQueryHandler>());

    private static EmergencyDrillPlanningItemDto DrillFor(Guid emergencyDrillPlanningId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        EmergencyDrillPlanningId = emergencyDrillPlanningId,
        TenantId = tenantId,
        Status = "Created",
        DrillType = "FireEvacuation",
        ShiftCode = "Day",
        ParticipationCount = 0,
        CreatedAtUtc = FixedUtcNow,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenEmergencyDrillPlanningExistsForTenant_ReturnsItemWithEtag()
    {
        var emergencyDrillPlanningId = Guid.NewGuid();
        var drill = DrillFor(emergencyDrillPlanningId, TestTenantId.Value);
        _repo.GetByIdAsync(emergencyDrillPlanningId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(drill);

        var result = await Sut().Handle(new GetEmergencyDrillPlanningByIdQuery(TestTenantId.Value, emergencyDrillPlanningId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.EmergencyDrillPlanningId.Should().Be(emergencyDrillPlanningId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenEmergencyDrillPlanningDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((EmergencyDrillPlanningItemDto?)null);

        var result = await Sut().Handle(new GetEmergencyDrillPlanningByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Scheduling.EmergencyDrillPlanning.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing EmergencyDrillPlanning record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenEmergencyDrillPlanningBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_EmergencyDrillPlanning_GetById filters by TenantId in
        // SQL, so a lookup scoped to TestTenantId for a record that actually belongs to another
        // tenant returns no row at all -- the repository is not stubbed for that
        // (EmergencyDrillPlanningId, TestTenantId) combination and NSubstitute's default (null)
        // applies, exactly as the real proc would.
        var emergencyDrillPlanningId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(emergencyDrillPlanningId, otherTenantId, Arg.Any<CancellationToken>()).Returns(DrillFor(emergencyDrillPlanningId, otherTenantId));

        var result = await Sut().Handle(new GetEmergencyDrillPlanningByIdQuery(TestTenantId.Value, emergencyDrillPlanningId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Scheduling.EmergencyDrillPlanning.NotFound");
    }
}
