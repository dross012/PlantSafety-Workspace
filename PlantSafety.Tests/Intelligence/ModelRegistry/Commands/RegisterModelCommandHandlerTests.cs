// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Intelligence.ModelRegistry.Commands
//  Layer:          Tests (Unit)
//  File:           RegisterModelCommandHandlerTests.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RegisterModelCommandHandler: the new Intelligence.ModelRegistry
//                  submodule build-out. Covers successful registration into Draft status, invalid
//                  SemVer/argument mapping to a Validation ApplicationError, and unhandled failure
//                  mapping. Mirrors
//                  PlantSafety.Tests.Intelligence.DataLake.Commands.StartProcessingDataLakeActivityCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Intelligence.ModelRegistry.Commands;
using PlantSafety.Domain.Intelligence.ModelRegistry.Interfaces;

namespace PlantSafety.Tests.Intelligence.ModelRegistry.Commands;

public sealed class RegisterModelCommandHandlerTests : UnitTestBase
{
    private readonly IModelRegistryEntryRepository _repo = Substitute.For<IModelRegistryEntryRepository>();

    private RegisterModelCommandHandler Sut() => new(_repo, NullLogger<RegisterModelCommandHandler>());

    private static RegisterModelCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        RequestedAtUtc: FixedUtcNow,
        ApiVersionToken: "v1",
        TenantId: TestTenantId.Value,
        SiteId: null,
        ModelName: "FailurePredictor",
        ModelVersion: "1.0.0",
        DatasetHash: "sha256:abc123",
        TrainingAccuracyScore: 0.85,
        RegisteredByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_RegistersEntryInDraftStatus()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Draft");
        await _repo.Received(1).AddAsync(Arg.Any<PlantSafety.Domain.Intelligence.ModelRegistry.Aggregates.ModelRegistryEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidModelVersion_ReturnsValidationError()
    {
        var command = ValidCommand() with { ModelVersion = "not-a-semver" };

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.ModelRegistry.RegisterInvalid");
        result.Error.Severity.Should().Be(ErrorSeverity.Validation);
        await _repo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WithEmptyModelName_ReturnsValidationError()
    {
        var command = ValidCommand() with { ModelName = "   " };

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.ModelRegistry.RegisterInvalid");
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.AddAsync(Arg.Any<PlantSafety.Domain.Intelligence.ModelRegistry.Aggregates.ModelRegistryEntry>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("boom"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.ModelRegistry.RegisterFailed");
    }
}
