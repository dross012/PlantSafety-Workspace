// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Handlers
//  Layer:          Tests (Unit)
//  File:           GetJointResponsePlanByIncidentQueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetJointResponsePlanByIncidentQueryHandler. Isolated,
//                  deterministic, <50ms per test.
// ============================================================================
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Handlers;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Queries;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Repositories;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Handlers;

public sealed class GetJointResponsePlanByIncidentQueryHandlerTests : UnitTestBase
{
    private readonly ILocalEmergencyCoordinationRepository _repo = Substitute.For<ILocalEmergencyCoordinationRepository>();

    private GetJointResponsePlanByIncidentQueryHandler Sut() => new(_repo, NullLogger<GetJointResponsePlanByIncidentQueryHandler>());

    [Fact]
    public async Task Handle_ReturnsPlansForIncident()
    {
        var incidentId = Guid.NewGuid();
        var plans = new List<JointResponsePlanItemDto>
        {
            new()
            {
                JointResponsePlanId = Guid.NewGuid(),
                LocalEmergencyCoordinationId = Guid.NewGuid(),
                TenantId = TestTenantId.Value,
                IncidentId = incidentId,
                PlanName = "Joint Wildfire Response Plan",
                IsActive = true,
                ActivatedAtUtc = FixedUtcNow,
                ActivatedBy = TestUserId.Value,
            },
        };
        _repo.GetJointResponsePlansByIncidentAsync(TestTenantId.Value, incidentId, Arg.Any<CancellationToken>()).Returns(plans);

        var result = await Sut().Handle(new GetJointResponsePlanByIncidentQuery(TestTenantId.Value, incidentId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEquivalentTo(plans);
        result.Value.IncidentId.Should().Be(incidentId);
    }

    [Fact]
    public async Task Handle_WhenNoPlansForIncident_ReturnsEmptyList()
    {
        var incidentId = Guid.NewGuid();
        _repo.GetJointResponsePlansByIncidentAsync(TestTenantId.Value, incidentId, Arg.Any<CancellationToken>()).Returns(new List<JointResponsePlanItemDto>());

        var result = await Sut().Handle(new GetJointResponsePlanByIncidentQuery(TestTenantId.Value, incidentId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.GetJointResponsePlansByIncidentAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<JointResponsePlanItemDto>>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new GetJointResponsePlanByIncidentQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.LocalEmergencyCoordination.GetJointResponsePlanByIncidentFailed");
    }
}
