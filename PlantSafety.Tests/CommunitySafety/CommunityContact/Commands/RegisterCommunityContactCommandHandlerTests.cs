// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CommunitySafety.CommunityContact.Commands
//  Layer:          Tests (Unit)
//  File:           RegisterCommunityContactCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RegisterCommunityContactCommandHandler. Covers successful
//                  registration/persistence, invalid ContactType rejection, and repository failure
//                  handling. Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.CommunitySafety.Commands;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityContact.Aggregates.CommunityContact;
using PlantSafety.Domain.CommunitySafety.CommunityContact.Interfaces;

namespace PlantSafety.Tests.CommunitySafety.CommunityContact.Commands;

public sealed class RegisterCommunityContactCommandHandlerTests : UnitTestBase
{
    private readonly ICommunityContactRepository _repo = Substitute.For<ICommunityContactRepository>();

    private RegisterCommunityContactCommandHandler Sut() => new(_repo, NullLogger<RegisterCommunityContactCommandHandler>());

    private static RegisterCommunityContactCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: null,
        DisplayName: "Fire Chief - Station 4",
        ContactType: "EmergencyServices",
        CommunityArea: "North Ridge",
        Email: "chief@example.org",
        Organization: "City Fire Dept",
        Phone: "555-0100",
        OptedInForNotifications: true,
        PerformedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_PersistsContactAndReturnsSuccess()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Active");
        await _repo.Received(1).AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnrecognizedContactType_ReturnsFailureWithoutPersisting()
    {
        var command = ValidCommand() with { ContactType = "NotARealType" };

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CommunitySafety.CommunityContact.InvalidContactType");
        await _repo.DidNotReceive().AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>()).Returns<Task>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CommunitySafety.CommunityContact.RegisterFailed");
    }
}
