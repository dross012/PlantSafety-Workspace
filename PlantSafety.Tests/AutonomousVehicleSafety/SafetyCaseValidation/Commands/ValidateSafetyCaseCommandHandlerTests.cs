// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.SafetyCaseValidation.Commands
//  Layer:          Tests (Unit)
//  File:           ValidateSafetyCaseCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ValidateSafetyCaseCommandHandler. Covers the create-then-validate
//                  workflow, the enum-parse fallback, expiry computation, and the failure path.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.AutonomousVehicleSafety.Commands;
using PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Interfaces;
using Agg = PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Aggregates.SafetyCaseValidation;

namespace PlantSafety.Tests.AutonomousVehicleSafety.SafetyCaseValidation.Commands;

public sealed class ValidateSafetyCaseCommandHandlerTests : UnitTestBase
{
    private readonly ISafetyCaseValidationRepository _repo = Substitute.For<ISafetyCaseValidationRepository>();

    private ValidateSafetyCaseCommandHandler Sut() => new(_repo, NullLogger<ValidateSafetyCaseCommandHandler>());

    private static ValidateSafetyCaseCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: null,
        AutonomousVehicleAssetId: Guid.NewGuid(),
        StandardReference: "AnsiRia1508",
        DocumentVersion: "v1.0",
        RevalidationCadenceDays: 180,
        ValidatedByUserId: Guid.NewGuid(),
        PerformedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPersistValidatedCase_WithComputedExpiry()
    {
        var command = ValidCommand();

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Validated");
        result.Value.ExpiresAtUtc.Should().Be(command.RequestedAtUtc.AddDays(180));
        await _repo.Received(1).AddAsync(Arg.Is<Agg>(a => a.Status.ToString() == "Validated"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnrecognizedStandardReference_ShouldDefaultToAnsiRia1508()
    {
        var command = ValidCommand() with { StandardReference = "NotARealStandard" };

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Is<Agg>(a => a.StandardReference.ToString() == "AnsiRia1508"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureWithValidateFailedErrorCode()
    {
        _repo.AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>()).Returns<Task>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AutonomousVehicleSafety.SafetyCaseValidation.ValidateFailed");
    }
}
