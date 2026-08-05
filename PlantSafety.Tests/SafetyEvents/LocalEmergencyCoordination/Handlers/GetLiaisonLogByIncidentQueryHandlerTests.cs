// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Handlers
//  Layer:          Tests (Unit)
//  File:           GetLiaisonLogByIncidentQueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetLiaisonLogByIncidentQueryHandler. Isolated, deterministic,
//                  <50ms per test.
// ============================================================================
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Handlers;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Queries;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Repositories;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Handlers;

public sealed class GetLiaisonLogByIncidentQueryHandlerTests : UnitTestBase
{
    private readonly ILocalEmergencyCoordinationRepository _repo = Substitute.For<ILocalEmergencyCoordinationRepository>();

    private GetLiaisonLogByIncidentQueryHandler Sut() => new(_repo, NullLogger<GetLiaisonLogByIncidentQueryHandler>());

    [Fact]
    public async Task Handle_ReturnsLogEntriesForIncident()
    {
        var incidentId = Guid.NewGuid();
        var entries = new List<LiaisonLogEntryItemDto>
        {
            new()
            {
                LiaisonLogEntryId = Guid.NewGuid(),
                ExternalResponderLiaisonId = Guid.NewGuid(),
                LocalEmergencyCoordinationId = Guid.NewGuid(),
                IncidentId = incidentId,
                AgencyName = "County EMS",
                Message = "Requesting additional ambulances.",
                LoggedAtUtc = FixedUtcNow,
                LoggedBy = TestUserId.Value,
            },
        };
        _repo.GetLiaisonLogByIncidentAsync(TestTenantId.Value, incidentId, Arg.Any<CancellationToken>()).Returns(entries);

        var result = await Sut().Handle(new GetLiaisonLogByIncidentQuery(TestTenantId.Value, incidentId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEquivalentTo(entries);
    }

    [Fact]
    public async Task Handle_WhenNoEntriesForIncident_ReturnsEmptyList()
    {
        var incidentId = Guid.NewGuid();
        _repo.GetLiaisonLogByIncidentAsync(TestTenantId.Value, incidentId, Arg.Any<CancellationToken>()).Returns(new List<LiaisonLogEntryItemDto>());

        var result = await Sut().Handle(new GetLiaisonLogByIncidentQuery(TestTenantId.Value, incidentId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.GetLiaisonLogByIncidentAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<LiaisonLogEntryItemDto>>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new GetLiaisonLogByIncidentQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.LocalEmergencyCoordination.GetLiaisonLogByIncidentFailed");
    }
}
