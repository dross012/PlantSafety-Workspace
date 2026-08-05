// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Smoke
//  Layer:          Tests (Smoke)
//  File:           LocalEmergencyCoordinationSmokeTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Fast, minimal build-verification checks: the aggregate can be constructed, its
//                  nine commands and five queries all resolve to well-formed types, and the basic
//                  create -> close -> archive path works. Intended to catch total breakage
//                  (missing DI registrations, broken constructors) quickly, not to be exhaustive.
//                  Isolated, deterministic, <50ms per test.
// ============================================================================
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Queries;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Enums;
using PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.TestSupport;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Smoke;

public sealed class LocalEmergencyCoordinationSmokeTests : UnitTestBase
{
    [Fact]
    public void Aggregate_CanBeCreated_WithCreatedStatus()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        agg.Status.Should().Be(LocalEmergencyCoordinationStatus.Created);
        agg.LocalEmergencyCoordinationId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Aggregate_QuickCreateCloseArchive_Succeeds()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        agg.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(LocalEmergencyCoordinationStatus.Archived);
    }

    [Theory]
    [InlineData(typeof(OpenLocalEmergencyCoordinationCaseCommand))]
    [InlineData(typeof(EstablishMutualAidAgreementCommand))]
    [InlineData(typeof(ActivateJointResponsePlanCommand))]
    [InlineData(typeof(DeactivateJointResponsePlanCommand))]
    [InlineData(typeof(AssignExternalResponderLiaisonCommand))]
    [InlineData(typeof(LogLiaisonCommunicationCommand))]
    [InlineData(typeof(RecordJointDebriefCommand))]
    [InlineData(typeof(CloseLocalEmergencyCoordinationCaseCommand))]
    [InlineData(typeof(ArchiveLocalEmergencyCoordinationCaseCommand))]
    public void AllNineCommands_ExistAsSealedRecords_ImplementingIBaseRequest(Type commandType)
    {
        commandType.IsSealed.Should().BeTrue();
        typeof(MediatR.IBaseRequest).IsAssignableFrom(commandType).Should().BeTrue();
    }

    [Theory]
    [InlineData(typeof(GetLocalEmergencyCoordinationByIdQuery))]
    [InlineData(typeof(ListMutualAidAgreementsQuery))]
    [InlineData(typeof(GetJointResponsePlanByIncidentQuery))]
    [InlineData(typeof(GetLiaisonLogByIncidentQuery))]
    [InlineData(typeof(ListJointDebriefsByPeriodQuery))]
    public void AllFiveQueries_ExistAsSealedRecords(Type queryType)
        => queryType.IsSealed.Should().BeTrue();
}
