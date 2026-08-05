// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Functional
//  Layer:          Tests (Functional)
//  File:           LocalEmergencyCoordinationFunctionalTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Functional tests exercising command handlers in sequence against a shared
//                  in-memory repository fake, verifying that state changes made by one command are
//                  visible to the next -- the closest in-process equivalent to a real multi-request
//                  workflow without standing up the full API/DB stack. Isolated, deterministic,
//                  no I/O, no network.
// ============================================================================
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;
using PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.TestSupport;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Functional;

public sealed class LocalEmergencyCoordinationFunctionalTests : UnitTestBase
{
    [Fact]
    public async Task OpenThenEstablishMutualAid_PersistsAgreementAgainstTheSameCase()
    {
        var repo = new InMemoryLocalEmergencyCoordinationRepository();

        var openResult = await new OpenLocalEmergencyCoordinationCaseCommandHandler(repo, NullLogger<OpenLocalEmergencyCoordinationCaseCommandHandler>())
            .Handle(new OpenLocalEmergencyCoordinationCaseCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow, "v1",
                TestTenantId.Value, null, null, "Wildfire Mutual Aid Case", TestUserId.Value), CancellationToken.None);

        openResult.IsSuccess.Should().BeTrue();

        var establishResult = await new EstablishMutualAidAgreementCommandHandler(repo, NullLogger<EstablishMutualAidAgreementCommandHandler>())
            .Handle(new EstablishMutualAidAgreementCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                openResult.Value.LocalEmergencyCoordinationId, "Metro Fire Dept", "Fire", 1,
                FixedUtcNow, FixedUtcNow.AddYears(1), TestUserId.Value), CancellationToken.None);

        establishResult.IsSuccess.Should().BeTrue();
        establishResult.Value.LocalEmergencyCoordinationId.Should().Be(openResult.Value.LocalEmergencyCoordinationId);
        repo.WriteCount.Should().Be(2, "one AddAsync for Open, one UpdateAsync for EstablishMutualAidAgreement");
    }

    [Fact]
    public async Task OpenThenCloseThenArchive_TransitionsThroughFullLifecycle()
    {
        var repo = new InMemoryLocalEmergencyCoordinationRepository();

        var openResult = await new OpenLocalEmergencyCoordinationCaseCommandHandler(repo, NullLogger<OpenLocalEmergencyCoordinationCaseCommandHandler>())
            .Handle(new OpenLocalEmergencyCoordinationCaseCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow, "v1",
                TestTenantId.Value, null, null, "Case to Close", TestUserId.Value), CancellationToken.None);

        var closeResult = await new CloseLocalEmergencyCoordinationCaseCommandHandler(repo, NullLogger<CloseLocalEmergencyCoordinationCaseCommandHandler>())
            .Handle(new CloseLocalEmergencyCoordinationCaseCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                openResult.Value.LocalEmergencyCoordinationId, TestUserId.Value), CancellationToken.None);

        closeResult.IsSuccess.Should().BeTrue();
        closeResult.Value.Status.Should().Be("Closed");

        var archiveResult = await new ArchiveLocalEmergencyCoordinationCaseCommandHandler(repo, NullLogger<ArchiveLocalEmergencyCoordinationCaseCommandHandler>())
            .Handle(new ArchiveLocalEmergencyCoordinationCaseCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                openResult.Value.LocalEmergencyCoordinationId, TestUserId.Value), CancellationToken.None);

        archiveResult.IsSuccess.Should().BeTrue();
        archiveResult.Value.Status.Should().Be("Archived");
    }
}
