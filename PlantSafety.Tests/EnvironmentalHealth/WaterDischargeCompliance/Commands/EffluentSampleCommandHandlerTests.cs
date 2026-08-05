// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.EnvironmentalHealth.WaterDischargeCompliance.Commands
//  Layer:          Tests (Unit)
//  File:           EffluentSampleCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command handler tests for the EffluentSample aggregate: Record, Void.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Enums;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Interfaces;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.ValueObjects.Ids;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Aggregates.EffluentSample;

namespace PlantSafety.Tests.EnvironmentalHealth.WaterDischargeCompliance.Commands;

public sealed class EffluentSampleCommandHandlerTests : UnitTestBase
{
    private readonly IEffluentSampleRepository _repo = Substitute.For<IEffluentSampleRepository>();

    private static Agg RecordedSample() => Agg.Create(
        createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
        causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value,
        dischargePointId: Guid.NewGuid(), parameter: SampleParameterType.PH, measuredValue: 7.2m,
        unitOfMeasure: "pH", sampledAtUtc: FixedTimestamp, collectedByUserId: TestUserId,
        labSourceSystem: "LIMS-Prod", chainOfCustodyReference: "COC-2026-0001");

    [Fact]
    public async Task Record_WithValidCommand_PersistsNewSampleAsRecorded()
    {
        var sut = new RecordEffluentSampleCommandHandler(_repo, NullLogger<RecordEffluentSampleCommandHandler>());
        var command = new RecordEffluentSampleCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, Guid.NewGuid(),
            Guid.NewGuid(), "PH", 7.4m, "pH", FixedUtcNow, TestUserId.Value, "LIMS-Prod", "COC-2026-0002",
            null, null, TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(EffluentSampleStatus.Recorded));
        await _repo.Received(1).AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Record_AsCorrection_PersistsWithCorrectionOfSampleIdSet()
    {
        var originalId = Guid.NewGuid();
        var sut = new RecordEffluentSampleCommandHandler(_repo, NullLogger<RecordEffluentSampleCommandHandler>());
        Agg? captured = null;
        await _repo.AddAsync(Arg.Do<Agg>(a => captured = a), Arg.Any<CancellationToken>());
        var command = new RecordEffluentSampleCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, Guid.NewGuid(),
            Guid.NewGuid(), "PH", 7.4m, "pH", FixedUtcNow, TestUserId.Value, "LIMS-Prod", "COC-2026-0003",
            originalId, "Recalibration.", TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.CorrectionOfSampleId.Should().Be(originalId);
    }

    [Fact]
    public async Task Void_WhenRecorded_TransitionsToVoidedAndPersists()
    {
        var sample = RecordedSample();
        _repo.GetByIdAsync(Arg.Any<EffluentSampleId>(), Arg.Any<CancellationToken>()).Returns(sample);
        var sut = new VoidEffluentSampleCommandHandler(_repo, NullLogger<VoidEffluentSampleCommandHandler>());
        var command = new VoidEffluentSampleCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            sample.EffluentSampleId.Value, "Chain of custody broken.", TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(EffluentSampleStatus.Voided));
        await _repo.Received(1).UpdateAsync(sample, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Void_WhenSampleDoesNotExist_ReturnsNotFoundFailure()
    {
        _repo.GetByIdAsync(Arg.Any<EffluentSampleId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);
        var sut = new VoidEffluentSampleCommandHandler(_repo, NullLogger<VoidEffluentSampleCommandHandler>());
        var command = new VoidEffluentSampleCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), "reason", TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Contain("NotFound");
    }
}
