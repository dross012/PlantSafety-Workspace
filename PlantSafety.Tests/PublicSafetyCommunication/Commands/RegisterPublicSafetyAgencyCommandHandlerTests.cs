// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.Commands
//  Layer:          Tests (Unit)
//  File:           RegisterPublicSafetyAgencyCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler tests for RegisterPublicSafetyAgencyCommandHandler. Isolated,
//                  deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.PublicSafetyCommunication.Commands;
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Interfaces;
using AgencyAggregate = PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Aggregates.PublicSafetyAgency;

namespace PlantSafety.Tests.PublicSafetyCommunication.Commands;

public sealed class RegisterPublicSafetyAgencyCommandHandlerTests : UnitTestBase
{
    private readonly IPublicSafetyAgencyRepository _repo = Substitute.For<IPublicSafetyAgencyRepository>();

    private RegisterPublicSafetyAgencyCommandHandler Sut() => new(_repo, NullLogger<RegisterPublicSafetyAgencyCommandHandler>());

    private static RegisterPublicSafetyAgencyCommand ValidCommand(string agencyType = "Fire") => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: null,
        AgencyName: "County Fire Department",
        AgencyType: agencyType,
        JurisdictionDescription: "County district 4",
        PrimaryPhoneNumber: "555-0100",
        PrimaryEmail: "dispatch@countyfire.example")
    { ActorId = TestUserId.Value };

    [Fact]
    public async Task Handle_WithValidCommand_PersistsAggregateAndReturnsSuccess()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Active");
        await _repo.Received(1).AddAsync(Arg.Is<AgencyAggregate>(a => a.AgencyName == "County Fire Department"), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("Fire")]
    [InlineData("Police")]
    [InlineData("EmergencyMedicalServices")]
    [InlineData("RegulatoryHotline")]
    public async Task Handle_WithEachRecognisedAgencyType_Succeeds(string agencyType)
    {
        var result = await Sut().Handle(ValidCommand(agencyType), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithUnrecognisedAgencyType_ReturnsFailureWithoutPersisting()
    {
        var result = await Sut().Handle(ValidCommand("NotARealAgencyType"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PublicSafetyCommunication.PublicSafetyAgency.InvalidAgencyType");
        await _repo.DidNotReceive().AddAsync(Arg.Any<AgencyAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailureWithoutLeakingInternals()
    {
        _repo.AddAsync(Arg.Any<AgencyAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("SQL connection pool exhausted (internal detail)."));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PublicSafetyCommunication.PublicSafetyAgency.RegisterFailed");
        result.Error.Message.Should().NotContain("SQL connection pool");
    }
}
