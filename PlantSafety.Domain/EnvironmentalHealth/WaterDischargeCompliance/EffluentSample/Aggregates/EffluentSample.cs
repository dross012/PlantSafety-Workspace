// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.EffluentSample
//  Layer:          Domain
//  File:           EffluentSample.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single effluent sample collected at a DischargePoint.
//                  Data-governance: chain-of-custody is immutable/append-only -- corrections never
//                  mutate the original record, they create a new sample referencing it via
//                  CorrectionOfSampleId while the original transitions to Corrected. Every sample
//                  is tagged with its originating lab source system. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Aggregates;

/// <summary>Aggregate root for a single effluent sample collected at a discharge point.</summary>
public sealed partial class EffluentSample
{
    /// <summary>Strong-typed identity of this EffluentSample aggregate.</summary>
    public EffluentSampleId EffluentSampleId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public EffluentSampleStatus Status { get; private set; }

    /// <summary>Identity of the DischargePoint this sample was collected from.</summary>
    public Guid DischargePointId { get; private set; }

    /// <summary>Parameter measured by this sample.</summary>
    public SampleParameterType Parameter { get; private set; }

    /// <summary>Measured value.</summary>
    public decimal MeasuredValue { get; private set; }

    /// <summary>Unit of measure for <see cref="MeasuredValue"/>.</summary>
    public string UnitOfMeasure { get; private set; } = string.Empty;

    /// <summary>UTC instant the sample was physically collected.</summary>
    public UtcTimestamp SampledAtUtc { get; private set; }

    /// <summary>Identity of the employee who collected the sample.</summary>
    public UserId CollectedByUserId { get; private set; }

    /// <summary>Source system that produced this sample result (lab-instrument import tagging;
    /// data-governance requirement -- every sample is tagged with its originating lab source system).</summary>
    public string LabSourceSystem { get; private set; } = string.Empty;

    /// <summary>Immutable chain-of-custody reference for this sample.</summary>
    public string ChainOfCustodyReference { get; private set; } = string.Empty;

    /// <summary>Identity of the original sample this record corrects, if this is a correction record.</summary>
    public Guid? CorrectionOfSampleId { get; private set; }

    /// <summary>Reason for correction, if this is a correction record.</summary>
    public string? CorrectionReason { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    private readonly List<IEffluentSampleDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IEffluentSampleDomainEvent> DomainEvents
        => new ReadOnlyCollection<IEffluentSampleDomainEvent>(_domainEvents);

    private EffluentSample() { }

    /// <summary>Records a new effluent sample (or an append-only correction of a prior sample) and
    /// raises <see cref="EffluentSampleRecordedEvent"/>.</summary>
    public static EffluentSample Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid dischargePointId = default,
        SampleParameterType parameter = SampleParameterType.PH,
        decimal measuredValue = 0m,
        string unitOfMeasure = "",
        UtcTimestamp sampledAtUtc = default,
        UserId collectedByUserId = default,
        string labSourceSystem = "",
        string chainOfCustodyReference = "",
        Guid? correctionOfSampleId = null,
        string? correctionReason = null)
    {
        var agg = new EffluentSample
        {
            EffluentSampleId = EffluentSampleId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = EffluentSampleStatus.Recorded,
            DischargePointId = dischargePointId,
            Parameter = parameter,
            MeasuredValue = measuredValue,
            UnitOfMeasure = (unitOfMeasure ?? string.Empty).Trim(),
            SampledAtUtc = sampledAtUtc,
            CollectedByUserId = collectedByUserId,
            LabSourceSystem = (labSourceSystem ?? string.Empty).Trim(),
            ChainOfCustodyReference = (chainOfCustodyReference ?? string.Empty).Trim(),
            CorrectionOfSampleId = correctionOfSampleId,
            CorrectionReason = correctionReason,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new EffluentSampleRecordedEvent(
            EffluentSampleEventId.New(), agg.EffluentSampleId, dischargePointId, parameter, measuredValue,
            agg.UnitOfMeasure, agg.LabSourceSystem, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    /// <summary>Marks this sample as superseded by an append-only correction record. The original
    /// row is never mutated other than this status flag -- the corrected value lives on a brand-new
    /// EffluentSample aggregate created via <see cref="Create"/> with CorrectionOfSampleId set.</summary>
    public void MarkCorrected(Guid correctingSampleId, string reason, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!EffluentSampleLifecycleRules.CanMarkCorrected(Status))
            throw new EffluentSampleInvalidStatusTransitionException(EffluentSampleId, Status.ToString(), EffluentSampleStatus.Corrected.ToString(), correlationId, causationId);

        Status = EffluentSampleStatus.Corrected;

        AddDomainEvent(new EffluentSampleCorrectedEvent(
            EffluentSampleEventId.New(), EffluentSampleId, correctingSampleId, reason, occurredAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Voids this sample (e.g. broken chain of custody). The record is retained for audit.</summary>
    public void Void(string reason, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!EffluentSampleLifecycleRules.CanVoid(Status))
            throw new EffluentSampleInvalidStatusTransitionException(EffluentSampleId, Status.ToString(), EffluentSampleStatus.Voided.ToString(), correlationId, causationId);

        Status = EffluentSampleStatus.Voided;

        AddDomainEvent(new EffluentSampleVoidedEvent(
            EffluentSampleEventId.New(), EffluentSampleId, reason, occurredAtUtc, actor, correlationId, causationId));
    }

    private void AddDomainEvent(IEffluentSampleDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}