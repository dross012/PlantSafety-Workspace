// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Handlers
//  Layer:          Tests (Unit)
//  File:           GetLocalEmergencyCoordinationByIdQueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetLocalEmergencyCoordinationByIdQueryHandler. Covers: successful
//                  retrieval with ETag projection, not-found behavior with correct severity, and
//                  tenant-isolation. Isolated, deterministic, <50ms per test.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Handlers;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Queries;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Repositories;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Handlers;

public sealed class GetLocalEmergencyCoordinationByIdQueryHandlerTests : UnitTestBase
{
    private readonly ILocalEmergencyCoordinationRepository _repo = Substitute.For<ILocalEmergencyCoordinationRepository>();

    private GetLocalEmergencyCoordinationByIdQueryHandler Sut() => new(_repo, NullLogger<GetLocalEmergencyCoordinationByIdQueryHandler>());

    private static LocalEmergencyCoordinationItemDto CaseFor(Guid id, Guid tenantId, string? eTag = "0x0000000000001111") => new()
    {
        LocalEmergencyCoordinationId = id,
        TenantId = tenantId,
        CaseName = "Wildfire Mutual Aid Case",
        Status = "Created",
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenCaseExistsForTenant_ReturnsItemWithEtag()
    {
        var caseId = Guid.NewGuid();
        _repo.GetByIdAsync(caseId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(CaseFor(caseId, TestTenantId.Value));

        var result = await Sut().Handle(new GetLocalEmergencyCoordinationByIdQuery(TestTenantId.Value, caseId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.LocalEmergencyCoordinationId.Should().Be(caseId);
        result.Value.ETag.Should().Be("0x0000000000001111");
    }

    [Fact]
    public async Task Handle_WhenCaseDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((LocalEmergencyCoordinationItemDto?)null);

        var result = await Sut().Handle(new GetLocalEmergencyCoordinationByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.LocalEmergencyCoordination.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
    }

    [Fact]
    public async Task Handle_WhenCaseBelongsToDifferentTenant_ReturnsNotFound()
    {
        var caseId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(caseId, otherTenantId, Arg.Any<CancellationToken>()).Returns(CaseFor(caseId, otherTenantId));

        var result = await Sut().Handle(new GetLocalEmergencyCoordinationByIdQuery(TestTenantId.Value, caseId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.LocalEmergencyCoordination.NotFound");
    }
}
