// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.EmergencyManagement.Handlers
//  Layer:          Tests (Unit)
//  File:           GetEmergencyManagementByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetEmergencyManagementByIdQueryHandler -- Phase 4
//                  SafetyEvents.EmergencyManagement full build-out. Covers: successful retrieval
//                  with ETag projection, not-found behavior with correct severity, and
//                  tenant-isolation (an EmergencyManagement record belonging to a different
//                  tenant must surface as NotFound, never leaking cross-tenant existence).
//                  Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Handlers.GetPermitsByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.SafetyEvents.EmergencyManagement.Handlers;
using PlantSafety.Application.SafetyEvents.EmergencyManagement.Models;
using PlantSafety.Application.SafetyEvents.EmergencyManagement.Queries;
using PlantSafety.Application.SafetyEvents.EmergencyManagement.Repositories;

namespace PlantSafety.Tests.SafetyEvents.EmergencyManagement.Handlers;

public sealed class GetEmergencyManagementByIdQueryHandlerTests : UnitTestBase
{
    private readonly IEmergencyManagementRepository _repo = Substitute.For<IEmergencyManagementRepository>();

    private GetEmergencyManagementByIdQueryHandler Sut() => new(_repo, NullLogger<GetEmergencyManagementByIdQueryHandler>());

    private static EmergencyManagementItemDto EmergencyFor(Guid emergencyId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        EmergencyId = emergencyId,
        TenantId = tenantId,
        Status = "Created",
        EmergencyType = "Fire",
        Severity = "Major",
        ReportedAtUtc = FixedUtcNow,
        ReportedBy = TestUserId.Value,
        Location = "Building A",
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenEmergencyExistsForTenant_ReturnsItemWithEtag()
    {
        var emergencyId = Guid.NewGuid();
        var emergency = EmergencyFor(emergencyId, TestTenantId.Value);
        _repo.GetByIdAsync(emergencyId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(emergency);

        var result = await Sut().Handle(new GetEmergencyManagementByIdQuery(TestTenantId.Value, emergencyId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.EmergencyId.Should().Be(emergencyId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenEmergencyDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((EmergencyManagementItemDto?)null);

        var result = await Sut().Handle(new GetEmergencyManagementByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.EmergencyManagement.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing EmergencyManagement record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenEmergencyBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_EmergencyManagementItems_GetById filters by TenantId in
        // SQL, so a lookup scoped to TestTenantId for a record that actually belongs to another
        // tenant returns no row at all -- the repository is not stubbed for that
        // (emergencyId, TestTenantId) combination and NSubstitute's default (null) applies, exactly
        // as the real proc would.
        var emergencyId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(emergencyId, otherTenantId, Arg.Any<CancellationToken>()).Returns(EmergencyFor(emergencyId, otherTenantId));

        var result = await Sut().Handle(new GetEmergencyManagementByIdQuery(TestTenantId.Value, emergencyId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.EmergencyManagement.NotFound");
    }
}
