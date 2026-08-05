// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Commands
//  Layer:          Tests (Unit)
//  File:           RecordDisposalCertificateCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Handler tests for RecordDisposalCertificateCommandHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HazardousWasteManagement.Commands;
using PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Interfaces;
using Agg = PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Aggregates.DisposalCertificate;

namespace PlantSafety.Tests.HazardousWasteManagement.Commands;

public sealed class RecordDisposalCertificateCommandHandlerTests : UnitTestBase
{
    private readonly IDisposalCertificateRepository _repo = Substitute.For<IDisposalCertificateRepository>();

    private RecordDisposalCertificateCommandHandler Sut() => new(_repo, NullLogger<RecordDisposalCertificateCommandHandler>());

    private static RecordDisposalCertificateCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: null,
        ManifestId: Guid.NewGuid(),
        TsdfFacilityId: Guid.NewGuid(),
        DisposalMethod: "Incineration",
        DisposalDateUtc: FixedUtcNow,
        PerformedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_PersistsAggregateAndReturnsSuccess()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Received");
        await _repo.Received(1).AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnknownDisposalMethod_DefaultsToLandfill()
    {
        var command = ValidCommand() with { DisposalMethod = "NotARealMethod" };

        await Sut().Handle(command, CancellationToken.None);

        await _repo.Received(1).AddAsync(
            Arg.Is<Agg>(a => a.DisposalMethod == PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Enums.DisposalMethod.Landfill),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureWithoutPropagating()
    {
        _repo.AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("SQL failure"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HazardousWasteManagement.DisposalCertificate.RecordFailed");
    }
}
