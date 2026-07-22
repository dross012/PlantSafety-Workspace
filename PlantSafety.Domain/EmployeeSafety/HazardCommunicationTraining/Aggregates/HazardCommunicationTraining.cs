// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.HazardCommunicationTraining
//  Layer:          Domain
//  File:           HazardCommunicationTraining.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single Hazard Communication (HazCom) training cycle. Ensures
//                  employees understand workplace hazards via GHS labeling, accessible Safety Data
//                  Sheets (SDS), and training on chemical handling and emergency response. Models the
//                  real lifecycle: scheduling, delivery, comprehension verification, and retraining.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Aggregates;

/// <summary>Aggregate root for a single HazCom training cycle.</summary>
public sealed partial class HazardCommunicationTraining
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this HazardCommunicationTraining aggregate.</summary>
    public HazardCommunicationTrainingId HazardCommunicationTrainingId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public HazardCommunicationTrainingStatus Status { get; private set; }

    // ---- Program identity -----------------------------------------------------

    /// <summary>Human-readable training program title.</summary>
    public string ProgramTitle { get; private set; } = string.Empty;

    /// <summary>GHS hazard class this training addresses.</summary>
    public GhsHazardClass HazardClass { get; private set; }

    /// <summary>Reference to the associated Safety Data Sheet(s), if chemical-related.</summary>
    public string SdsReference { get; private set; } = string.Empty;

    /// <summary>Identity of the trainer.</summary>
    public UserId TrainedByUserId { get; private set; }

    /// <summary>UTC timestamp training was delivered, if recorded.</summary>
    public UtcTimestamp? TrainingDeliveredAtUtc { get; private set; }

    /// <summary>Number of employees who attended the training.</summary>
    public int EmployeesTrainedCount { get; private set; }

    /// <summary>Average comprehension score (0-100) from post-training assessment.</summary>
    public decimal? ComprehensionScore { get; private set; }

    /// <summary>Minimum passing comprehension score.</summary>
    public decimal PassingScoreThreshold { get; private set; } = 80m;

    /// <summary>UTC timestamp the next training cycle is due.</summary>
    public UtcTimestamp? NextTrainingDueAtUtc { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IHazardCommunicationTrainingDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IHazardCommunicationTrainingDomainEvent> DomainEvents
        => new ReadOnlyCollection<IHazardCommunicationTrainingDomainEvent>(_domainEvents);

    // For rehydration only.
    private HazardCommunicationTraining() { }

    // ---- Factory: Create -----------------------------------------------------------

    /// <summary>Schedules a new HazCom training cycle and raises <see cref="HazardCommunicationTrainingScheduledEvent"/>.</summary>
    public static HazardCommunicationTraining Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string programTitle = "",
        GhsHazardClass hazardClass = GhsHazardClass.Chemical,
        string sdsReference = "",
        UserId trainedByUserId = default,
        decimal passingScoreThreshold = 80m)
    {
        var agg = new HazardCommunicationTraining
        {
            HazardCommunicationTrainingId = HazardCommunicationTrainingId.New(),
            TenantId               = tenantId,
            SiteId                 = siteId,
            Status                 = HazardCommunicationTrainingStatus.Scheduled,
            ProgramTitle           = (programTitle ?? string.Empty).Trim(),
            HazardClass            = hazardClass,
            SdsReference           = (sdsReference ?? string.Empty).Trim(),
            TrainedByUserId        = trainedByUserId,
            PassingScoreThreshold  = passingScoreThreshold,
            CreatedAtUtc           = createdAtUtc,
            CreatedBy              = createdBy,
        };

        agg.AddDomainEvent(new HazardCommunicationTrainingScheduledEvent(
            HazardCommunicationTrainingEventId.New(), agg.HazardCommunicationTrainingId, hazardClass,
            createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: DeliverTraining -----------------------------------------------

    /// <summary>Records delivery of the training session.</summary>
    public void DeliverTraining(int employeesTrainedCount, UtcTimestamp deliveredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(HazardCommunicationTrainingStatus.Scheduled, HazardCommunicationTrainingStatus.RetrainingRequired);

        if (employeesTrainedCount < 0)
            throw new ArgumentOutOfRangeException(nameof(employeesTrainedCount), "Employees trained count cannot be negative.");

        EmployeesTrainedCount  = employeesTrainedCount;
        TrainingDeliveredAtUtc = deliveredAtUtc;
        Status = HazardCommunicationTrainingStatus.Delivered;

        AddDomainEvent(new HazardCommunicationTrainingDeliveredEvent(
            HazardCommunicationTrainingEventId.New(), HazardCommunicationTrainingId, employeesTrainedCount,
            deliveredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: VerifyComprehension ---------------------------------------------

    /// <summary>Records the outcome of the post-training comprehension assessment.</summary>
    public void VerifyComprehension(decimal comprehensionScore, UtcTimestamp verifiedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(HazardCommunicationTrainingStatus.Delivered);

        if (comprehensionScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(comprehensionScore), "Comprehension score must be between 0 and 100.");

        ComprehensionScore = comprehensionScore;

        if (comprehensionScore >= PassingScoreThreshold)
        {
            Status = HazardCommunicationTrainingStatus.ComprehensionVerified;
            AddDomainEvent(new HazardCommunicationTrainingComprehensionVerifiedEvent(
                HazardCommunicationTrainingEventId.New(), HazardCommunicationTrainingId, comprehensionScore,
                verifiedAtUtc, actor, correlationId, causationId));
        }
        else
        {
            Status = HazardCommunicationTrainingStatus.RetrainingRequired;
            AddDomainEvent(new HazardCommunicationTrainingRetrainingRequiredEvent(
                HazardCommunicationTrainingEventId.New(), HazardCommunicationTrainingId, comprehensionScore,
                verifiedAtUtc, actor, correlationId, causationId));
        }
    }

    // ---- Behavior: ScheduleNextCycle -----------------------------------------------

    /// <summary>Schedules the next mandatory training cycle.</summary>
    public void ScheduleNextCycle(UtcTimestamp dueAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(HazardCommunicationTrainingStatus.ComprehensionVerified);

        NextTrainingDueAtUtc = dueAtUtc;

        AddDomainEvent(new HazardCommunicationTrainingNextCycleScheduledEvent(
            HazardCommunicationTrainingEventId.New(), HazardCommunicationTrainingId, dueAtUtc,
            actor, correlationId, causationId));
    }

    // ---- Behavior: Close -----------------------------------------------------------

    /// <summary>Closes this training cycle record.</summary>
    public void Close(UtcTimestamp closedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(HazardCommunicationTrainingStatus.ComprehensionVerified);

        Status = HazardCommunicationTrainingStatus.Closed;

        AddDomainEvent(new HazardCommunicationTrainingClosedEvent(
            HazardCommunicationTrainingEventId.New(), HazardCommunicationTrainingId, closedAtUtc,
            actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IHazardCommunicationTrainingDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params HazardCommunicationTrainingStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for HazardCommunicationTraining {HazardCommunicationTrainingId}.");
    }
}
