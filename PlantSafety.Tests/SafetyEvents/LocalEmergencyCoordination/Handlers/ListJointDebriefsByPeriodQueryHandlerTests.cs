// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Handlers
//  Layer:          Tests (Unit)
//  File:           ListJointDebriefsByPeriodQueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ListJointDebriefsByPeriodQueryHandler. Isolated, deterministic,
//                  <50ms per test.
// ============================================================================
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Handlers;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Queries;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Repositories;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Handlers;

public sealed class ListJointDebriefsByPeriodQueryHandlerTests : UnitTestBase
{
    private readonly ILocalEmergencyCoordinationRepository _repo = Substitute.For<ILocalEmergencyCoordinationRepository>();

    private ListJointDebriefsByPeriodQueryHandler Sut() => new(_repo, NullLogger<ListJointDebriefsByPeriodQueryHandler>());

    [Fact]
    public async Task Handle_ReturnsDebriefsWithinPeriod()
    {
        var from = FixedUtcNow.AddDays(-7);
        var to = FixedUtcNow;
        var debriefs = new List<JointDebriefItemDto>
        {
            new()
            {
                JointDebriefRecordId = Guid.NewGuid(),
                LocalEmergencyCoordinationId = Guid.NewGuid(),
                TenantId = TestTenantId.Value,
                IncidentId = Guid.NewGuid(),
                AgenciesInvolved = "Metro Fire, County EMS",
                Summary = "Well coordinated response.",
                RecordedAtUtc = FixedUtcNow.AddDays(-1),
                RecordedBy = TestUserId.Value,
            },
        };
        _repo.ListJointDebriefsByPeriodAsync(TestTenantId.Value, from, to, Arg.Any<CancellationToken>()).Returns(debriefs);

        var result = await Sut().Handle(new ListJointDebriefsByPeriodQuery(TestTenantId.Value, from, to), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEquivalentTo(debriefs);
    }

    [Fact]
    public async Task Handle_WhenNoDebriefsInPeriod_ReturnsEmptyList()
    {
        var from = FixedUtcNow.AddDays(-7);
        var to = FixedUtcNow;
        _repo.ListJointDebriefsByPeriodAsync(TestTenantId.Value, from, to, Arg.Any<CancellationToken>()).Returns(new List<JointDebriefItemDto>());

        var result = await Sut().Handle(new ListJointDebriefsByPeriodQuery(TestTenantId.Value, from, to), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.ListJointDebriefsByPeriodAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<JointDebriefItemDto>>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new ListJointDebriefsByPeriodQuery(TestTenantId.Value, FixedUtcNow.AddDays(-7), FixedUtcNow), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.LocalEmergencyCoordination.ListJointDebriefsByPeriodFailed");
    }
}
