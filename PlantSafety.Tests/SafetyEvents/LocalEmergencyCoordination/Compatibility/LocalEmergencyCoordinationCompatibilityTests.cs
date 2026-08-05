// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Compatibility
//  Layer:          Tests (Compatibility / Backward Compatibility)
//  File:           LocalEmergencyCoordinationCompatibilityTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Compatibility and backward-compatibility tests: default ApiVersionToken/ETag
//                  values so older callers that omit these optional envelope fields keep working,
//                  additive-only enum ordinal stability (LocalEmergencyCoordinationStatus /
//                  MutualAidAgreementRenewalStatus), and command-envelope shape stability across the
//                  nine commands. Isolated, deterministic, no I/O, no network.
// ============================================================================
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Enums;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Compatibility;

public sealed class LocalEmergencyCoordinationCompatibilityTests : UnitTestBase
{
    [Fact]
    public void EstablishMutualAidAgreementCommand_WhenApiVersionTokenOmitted_DefaultsToV1()
    {
        var command = new EstablishMutualAidAgreementCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Agency", "Fire", 1, FixedUtcNow, FixedUtcNow.AddYears(1), Guid.NewGuid());

        command.ApiVersionToken.Should().Be("v1", "older callers that never send an explicit version token must still route to v1 semantics");
    }

    [Fact]
    public void EstablishMutualAidAgreementCommand_WhenEtagOmitted_DefaultsToNull_AllowingUnconditionalFirstWrite()
    {
        var command = new EstablishMutualAidAgreementCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Agency", "Fire", 1, FixedUtcNow, FixedUtcNow.AddYears(1), Guid.NewGuid());

        command.ETag.Should().BeNull("callers that never read-before-write (e.g. legacy batch importers) must not be forced to supply an ETag");
    }

    [Theory]
    [InlineData(LocalEmergencyCoordinationStatus.Created, 0)]
    [InlineData(LocalEmergencyCoordinationStatus.Active, 1)]
    [InlineData(LocalEmergencyCoordinationStatus.Completed, 2)]
    [InlineData(LocalEmergencyCoordinationStatus.Closed, 3)]
    [InlineData(LocalEmergencyCoordinationStatus.Archived, 4)]
    public void LocalEmergencyCoordinationStatus_OrdinalsAreStable_ForPersistedAndSerializedData(LocalEmergencyCoordinationStatus status, int expectedOrdinal)
        => ((int)status).Should().Be(expectedOrdinal, "existing persisted rows and any wire-serialized enum values must not silently shift meaning across deployments");

    [Theory]
    [InlineData(MutualAidAgreementRenewalStatus.Current, 0)]
    [InlineData(MutualAidAgreementRenewalStatus.PendingRenewal, 1)]
    [InlineData(MutualAidAgreementRenewalStatus.Expired, 2)]
    public void MutualAidAgreementRenewalStatus_OrdinalsAreStable(MutualAidAgreementRenewalStatus status, int expectedOrdinal)
        => ((int)status).Should().Be(expectedOrdinal);

    [Fact]
    public void CloseLocalEmergencyCoordinationCaseCommand_AcceptsNullEtag_ForCallersThatNeverRead()
    {
        var command = new CloseLocalEmergencyCoordinationCaseCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        command.ETag.Should().BeNull();
    }
}
