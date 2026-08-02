// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.Handlers
//  Layer:          Tests (Unit)
//  File:           GetDigitalTwinByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetDigitalTwinByIdQueryHandler -- Phase 4 DigitalTwin full
//                  build-out. Covers: successful retrieval with ETag projection, not-found
//                  behavior with correct severity, and tenant-isolation. Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Handlers.GetAuditingByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.DigitalTwin.Handlers;
using PlantSafety.Application.DigitalTwin.Models;
using PlantSafety.Application.DigitalTwin.Queries;
using PlantSafety.Application.DigitalTwin.Repositories;

namespace PlantSafety.Tests.DigitalTwin.Handlers;

public sealed class GetDigitalTwinByIdQueryHandlerTests : UnitTestBase
{
    private readonly IDigitalTwinRepository _repo = Substitute.For<IDigitalTwinRepository>();

    private GetDigitalTwinByIdQueryHandler Sut() => new(_repo, NullLogger<GetDigitalTwinByIdQueryHandler>());

    private static DigitalTwinItemDto DigitalTwinFor(Guid digitalTwinId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        DigitalTwinId = digitalTwinId,
        TenantId = tenantId,
        Status = "Created",
        TwinName = "Line 3 Conveyor Twin",
        ModelType = "PhysicsBasedSimulation",
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenDigitalTwinExistsForTenant_ReturnsItemWithEtag()
    {
        var digitalTwinId = Guid.NewGuid();
        var digitalTwin = DigitalTwinFor(digitalTwinId, TestTenantId.Value);
        _repo.GetByIdAsync(digitalTwinId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(digitalTwin);

        var result = await Sut().Handle(new GetDigitalTwinByIdQuery(TestTenantId.Value, digitalTwinId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.DigitalTwinId.Should().Be(digitalTwinId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenDigitalTwinDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((DigitalTwinItemDto?)null);

        var result = await Sut().Handle(new GetDigitalTwinByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing DigitalTwin record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenDigitalTwinBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_DigitalTwinAggregate_GetById filters by TenantId in
        // SQL, so a lookup scoped to TestTenantId for a record that actually belongs to another
        // tenant returns no row at all -- the repository is not stubbed for that
        // (DigitalTwinId, TestTenantId) combination and NSubstitute's default (null) applies,
        // exactly as the real proc would.
        var digitalTwinId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(digitalTwinId, otherTenantId, Arg.Any<CancellationToken>()).Returns(DigitalTwinFor(digitalTwinId, otherTenantId));

        var result = await Sut().Handle(new GetDigitalTwinByIdQuery(TestTenantId.Value, digitalTwinId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.NotFound");
    }
}
