// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CyberPhysicalSecurity.Handlers
//  Layer:          Tests (Unit)
//  File:           CyberPhysicalSecurityQueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the module's five read-side query handlers (one per entity):
//                  GetOtAssetByIdQueryHandler, ListUnauthorizedControlChangesQueryHandler,
//                  GetSensorIntegrityHistoryQueryHandler, ListActiveSabotageIndicatorsQueryHandler,
//                  GetCyberPhysicalIncidentByIdQueryHandler. Grouped together to keep test volume
//                  proportionate to this module's small, uniform query surface.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Handlers;
using PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Models;
using PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Queries;
using PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Repositories;
using PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Handlers;
using PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Models;
using PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Queries;
using PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Repositories;
using PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Handlers;
using PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Models;
using PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Queries;
using PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Repositories;
using PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Handlers;
using PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Models;
using PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Queries;
using PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Repositories;
using PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Handlers;
using PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Models;
using PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Queries;
using PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Repositories;

namespace PlantSafety.Tests.CyberPhysicalSecurity.Handlers;

public sealed class GetOtAssetByIdQueryHandlerTests : UnitTestBase
{
    private readonly IOtAssetReadRepository _repo = Substitute.For<IOtAssetReadRepository>();
    private GetOtAssetByIdQueryHandler Sut() => new(_repo, NullLogger<GetOtAssetByIdQueryHandler>());

    private static OtAssetDto AssetFor(Guid id, Guid tenantId) => new(
        id, tenantId, Guid.NewGuid(), "PLC-101", "Line 3 PLC", "Plc", "Siemens", "S7-1500", "V2.9",
        "Level 1", "Medium", "Active", null, FixedUtcNow);

    [Fact]
    public async Task Handle_WhenAssetExistsForTenant_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(id, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(AssetFor(id, TestTenantId.Value));

        var result = await Sut().Handle(new GetOtAssetByIdQuery(TestTenantId.Value, id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.OtAssetId.Should().Be(id);
    }

    [Fact]
    public async Task Handle_WhenAssetDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((OtAssetDto?)null);

        var result = await Sut().Handle(new GetOtAssetByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CyberPhysicalSecurity.OtAsset.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureInsteadOfPropagating()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<OtAssetDto?>>(_ => throw new InvalidOperationException("simulated DB failure"));

        var result = await Sut().Handle(new GetOtAssetByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CyberPhysicalSecurity.OtAsset.RetrievalFailed");
    }
}

public sealed class ListUnauthorizedControlChangesQueryHandlerTests : UnitTestBase
{
    private readonly IControlChangeEventReadRepository _repo = Substitute.For<IControlChangeEventReadRepository>();
    private ListUnauthorizedControlChangesQueryHandler Sut() => new(_repo, NullLogger<ListUnauthorizedControlChangesQueryHandler>());

    [Fact]
    public async Task Handle_ReturnsUnauthorizedRecordsFromRepository()
    {
        var dto = new ControlChangeEventDto(Guid.NewGuid(), TestTenantId.Value, Guid.NewGuid(), Guid.NewGuid(),
            "desc", "SetPoint.Pressure", "10", "12", null, "Unauthorized", FixedUtcNow, new string('A', 64));
        _repo.ListUnauthorizedAsync(TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(new[] { dto });

        var result = await Sut().Handle(new ListUnauthorizedControlChangesQuery(TestTenantId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle().Which.AuthorizationStatus.Should().Be("Unauthorized");
    }

    [Fact]
    public async Task Handle_WithNoUnauthorizedRecords_ReturnsEmptyList()
    {
        _repo.ListUnauthorizedAsync(TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(Array.Empty<ControlChangeEventDto>());

        var result = await Sut().Handle(new ListUnauthorizedControlChangesQuery(TestTenantId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}

public sealed class GetSensorIntegrityHistoryQueryHandlerTests : UnitTestBase
{
    private readonly ISensorIntegrityCheckReadRepository _repo = Substitute.For<ISensorIntegrityCheckReadRepository>();
    private GetSensorIntegrityHistoryQueryHandler Sut() => new(_repo, NullLogger<GetSensorIntegrityHistoryQueryHandler>());

    [Fact]
    public async Task Handle_ReturnsHistoryOrderedFromRepository()
    {
        var otAssetId = Guid.NewGuid();
        var dto = new SensorIntegrityCheckDto(Guid.NewGuid(), TestTenantId.Value, Guid.NewGuid(), otAssetId,
            "Calibration", "Pass", "OK", FixedUtcNow, null);
        _repo.GetHistoryForAssetAsync(otAssetId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(new[] { dto });

        var result = await Sut().Handle(new GetSensorIntegrityHistoryQuery(TestTenantId.Value, otAssetId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle().Which.OtAssetId.Should().Be(otAssetId);
    }
}

public sealed class ListActiveSabotageIndicatorsQueryHandlerTests : UnitTestBase
{
    private readonly ISabotageIndicatorReadRepository _repo = Substitute.For<ISabotageIndicatorReadRepository>();
    private ListActiveSabotageIndicatorsQueryHandler Sut() => new(_repo, NullLogger<ListActiveSabotageIndicatorsQueryHandler>());

    [Fact]
    public async Task Handle_ReturnsActiveIndicatorsFromRepository()
    {
        var dto = new SabotageIndicatorDto(Guid.NewGuid(), TestTenantId.Value, Guid.NewGuid(), "ControlChange", null,
            "desc", "High", "Open", 1, FixedUtcNow, "P1");
        _repo.ListActiveAsync(TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(new[] { dto });

        var result = await Sut().Handle(new ListActiveSabotageIndicatorsQuery(TestTenantId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle().Which.Status.Should().Be("Open");
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureInsteadOfPropagating()
    {
        _repo.ListActiveAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<SabotageIndicatorDto>>>(_ => throw new InvalidOperationException("simulated DB failure"));

        var result = await Sut().Handle(new ListActiveSabotageIndicatorsQuery(TestTenantId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CyberPhysicalSecurity.SabotageIndicator.ListFailed");
    }
}

public sealed class GetCyberPhysicalIncidentByIdQueryHandlerTests : UnitTestBase
{
    private readonly ICyberPhysicalIncidentReadRepository _repo = Substitute.For<ICyberPhysicalIncidentReadRepository>();
    private GetCyberPhysicalIncidentByIdQueryHandler Sut() => new(_repo, NullLogger<GetCyberPhysicalIncidentByIdQueryHandler>());

    private static CyberPhysicalIncidentDto IncidentFor(Guid id, Guid tenantId) => new(
        id, tenantId, Guid.NewGuid(), "Incident", "desc", "High", "Open", [], TestUserId.Value, FixedUtcNow, null, null);

    [Fact]
    public async Task Handle_WhenIncidentExistsForTenant_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(id, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(IncidentFor(id, TestTenantId.Value));

        var result = await Sut().Handle(new GetCyberPhysicalIncidentByIdQuery(TestTenantId.Value, id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CyberPhysicalIncidentId.Should().Be(id);
    }

    [Fact]
    public async Task Handle_WhenIncidentBelongsToDifferentTenant_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(id, otherTenantId, Arg.Any<CancellationToken>()).Returns(IncidentFor(id, otherTenantId));

        var result = await Sut().Handle(new GetCyberPhysicalIncidentByIdQuery(TestTenantId.Value, id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CyberPhysicalSecurity.CyberPhysicalIncident.NotFound");
    }
}
