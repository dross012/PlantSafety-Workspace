// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Intelligence.FeatureEngineering.Commands
//  Layer:          Tests (Unit)
//  File:           CreateFeatureSetCommandHandlerTests.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for CreateFeatureSetCommandHandler: the new
//                  Intelligence.FeatureEngineering submodule build-out. Covers successful creation
//                  (default and explicit version), invalid-argument mapping to a Validation
//                  ApplicationError, and unhandled failure mapping. Mirrors
//                  PlantSafety.Tests.Intelligence.DataLake.Commands.StartProcessingDataLakeActivityCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Intelligence.FeatureEngineering.Commands;
using PlantSafety.Domain.Intelligence.FeatureEngineering.Interfaces;

namespace PlantSafety.Tests.Intelligence.FeatureEngineering.Commands;

public sealed class CreateFeatureSetCommandHandlerTests : UnitTestBase
{
    private readonly IFeatureSetRepository _repo = Substitute.For<IFeatureSetRepository>();

    private CreateFeatureSetCommandHandler Sut() => new(_repo, NullLogger<CreateFeatureSetCommandHandler>());

    private static CreateFeatureSetCommand ValidCommand(string? version = null) => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        RequestedAtUtc: FixedUtcNow,
        ApiVersionToken: "v1",
        TenantId: TestTenantId.Value,
        SiteId: null,
        Name: "IncidentRiskFeatures",
        Version: version,
        DataSourcePath: "masked://datalake/incident-risk",
        CreatedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_CreatesUnpublishedFeatureSet()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsPublished.Should().BeFalse();
        result.Value.FeatureCount.Should().Be(0);
        await _repo.Received(1).AddAsync(Arg.Any<PlantSafety.Domain.Intelligence.FeatureEngineering.Entities.FeatureSet>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNoVersionSupplied_DefaultsToInitialVersion()
    {
        PlantSafety.Domain.Intelligence.FeatureEngineering.Entities.FeatureSet? captured = null;
        await _repo.AddAsync(Arg.Do<PlantSafety.Domain.Intelligence.FeatureEngineering.Entities.FeatureSet>(fs => captured = fs), Arg.Any<CancellationToken>());

        await Sut().Handle(ValidCommand(version: null), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Version.Value.Should().Be("1.0");
    }

    [Fact]
    public async Task Handle_WithExplicitVersion_UsesSuppliedVersion()
    {
        PlantSafety.Domain.Intelligence.FeatureEngineering.Entities.FeatureSet? captured = null;
        await _repo.AddAsync(Arg.Do<PlantSafety.Domain.Intelligence.FeatureEngineering.Entities.FeatureSet>(fs => captured = fs), Arg.Any<CancellationToken>());

        await Sut().Handle(ValidCommand(version: "2.3"), CancellationToken.None);

        captured!.Version.Value.Should().Be("2.3");
    }

    [Fact]
    public async Task Handle_WithInvalidVersion_ReturnsValidationError()
    {
        var command = ValidCommand(version: "not-a-version");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.FeatureEngineering.CreateInvalid");
        result.Error.Severity.Should().Be(ErrorSeverity.Validation);
    }

    [Fact]
    public async Task Handle_WithEmptyName_ReturnsValidationError()
    {
        var command = ValidCommand() with { Name = "   " };

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.FeatureEngineering.CreateInvalid");
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.AddAsync(Arg.Any<PlantSafety.Domain.Intelligence.FeatureEngineering.Entities.FeatureSet>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("boom"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.FeatureEngineering.CreateFailed");
    }
}
