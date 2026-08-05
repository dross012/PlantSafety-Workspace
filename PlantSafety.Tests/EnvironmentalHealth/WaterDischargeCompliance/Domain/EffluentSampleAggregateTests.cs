// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.EnvironmentalHealth.WaterDischargeCompliance.Domain
//  Layer:          Tests (Unit)
//  File:           EffluentSampleAggregateTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the EffluentSample aggregate root, including the
//                  append-only chain-of-custody correction invariant.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Enums;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Exceptions;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Aggregates.EffluentSample;

namespace PlantSafety.Tests.EnvironmentalHealth.WaterDischargeCompliance.Domain;

public sealed class EffluentSampleAggregateTests : UnitTestBase
{
    private static Agg RecordedSample() => Agg.Create(
        createdBy: TestUserId,
        createdAtUtc: FixedTimestamp,
        correlationId: TestCorrelationId,
        causationId: TestCausationId,
        actor: TestActor,
        tenantId: TestTenantId.Value,
        siteId: Guid.NewGuid(),
        dischargePointId: Guid.NewGuid(),
        parameter: SampleParameterType.PH,
        measuredValue: 7.2m,
        unitOfMeasure: "pH",
        sampledAtUtc: FixedTimestamp,
        collectedByUserId: TestUserId,
        labSourceSystem: "LIMS-Prod",
        chainOfCustodyReference: "COC-2026-0001");

    [Fact]
    public void Create_WithValidData_RecordsSampleAndRaisesEvent()
    {
        var sample = RecordedSample();

        sample.Status.Should().Be(EffluentSampleStatus.Recorded);
        sample.MeasuredValue.Should().Be(7.2m);
        sample.CorrectionOfSampleId.Should().BeNull();
        sample.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void MarkCorrected_WhenRecorded_TransitionsToCorrected()
    {
        var sample = RecordedSample();
        var correctingId = Guid.NewGuid();

        sample.MarkCorrected(correctingId, "Instrument recalibration invalidated original reading.", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        sample.Status.Should().Be(EffluentSampleStatus.Corrected);
    }

    [Fact]
    public void MarkCorrected_WhenAlreadyCorrected_ThrowsInvalidStatusTransition()
    {
        var sample = RecordedSample();
        sample.MarkCorrected(Guid.NewGuid(), "reason", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => sample.MarkCorrected(Guid.NewGuid(), "reason again", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<EffluentSampleInvalidStatusTransitionException>();
    }

    [Fact]
    public void Void_WhenRecorded_TransitionsToVoided()
    {
        var sample = RecordedSample();

        sample.Void("Chain of custody broken.", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        sample.Status.Should().Be(EffluentSampleStatus.Voided);
    }

    [Fact]
    public void Void_WhenAlreadyCorrected_ThrowsInvalidStatusTransition()
    {
        var sample = RecordedSample();
        sample.MarkCorrected(Guid.NewGuid(), "reason", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => sample.Void("reason", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<EffluentSampleInvalidStatusTransitionException>();
    }

    [Fact]
    public void Create_AsCorrectionRecord_LinksToOriginalSampleWithoutMutatingIt()
    {
        var original = RecordedSample();

        var correction = Agg.Create(
            createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
            causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value,
            dischargePointId: original.DischargePointId, parameter: SampleParameterType.PH,
            measuredValue: 7.4m, unitOfMeasure: "pH", sampledAtUtc: FixedTimestamp,
            collectedByUserId: TestUserId, labSourceSystem: "LIMS-Prod",
            chainOfCustodyReference: "COC-2026-0002",
            correctionOfSampleId: original.EffluentSampleId.Value,
            correctionReason: "Instrument recalibration.");

        correction.CorrectionOfSampleId.Should().Be(original.EffluentSampleId.Value);
        correction.CorrectionReason.Should().Be("Instrument recalibration.");
        // The original is a wholly separate in-memory instance -- append-only, never mutated by
        // constructing the correction record.
        original.Status.Should().Be(EffluentSampleStatus.Recorded);
    }
}
