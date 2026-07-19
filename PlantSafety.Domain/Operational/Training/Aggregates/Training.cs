// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Training
//  Layer:          Domain
//  File:           Training.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  training-assignment record shape grounded in
//                  project_plantsafety_domains.md Module 18 (Training: "Training
//                  catalog, assignments, completions, and compliance").
//                  Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Training bounded context. Tracks a single
//                  training assignment: the course, worker, assignment/due/completion
//                  dates, score, and compliance category.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Training.Aggregates;

/// <summary>Aggregate root for the Training bounded context.</summary>
public sealed partial class Training
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Training aggregate.</summary>
    public TrainingId TrainingId { get; private set; }

    /// <summary>Tenant that owns this training record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this training assignment is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public TrainingStatus Status { get; private set; }

    // ---- Training-specific business properties ----------------------------------

    /// <summary>Human-readable course name.</summary>
    public string CourseName { get; private set; } = string.Empty;

    /// <summary>Course catalog code/reference.</summary>
    public string CourseCode { get; private set; } = string.Empty;

    /// <summary>Worker this training is assigned to.</summary>
    public UserId WorkerId { get; private set; }

    /// <summary>Compliance category (e.g., Mandatory, Refresher, Elective).</summary>
    public string ComplianceCategory { get; private set; } = "Mandatory";

    /// <summary>UTC timestamp the training was assigned.</summary>
    public UtcTimestamp? AssignedAtUtc { get; private set; }

    /// <summary>UTC timestamp the training is due for completion.</summary>
    public UtcTimestamp? DueAtUtc { get; private set; }

    /// <summary>UTC timestamp the training was completed.</summary>
    public UtcTimestamp? CompletedAtUtc { get; private set; }

    /// <summary>Assessment score achieved, if the course includes an assessment (0-100).</summary>
    public decimal? Score { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ITrainingDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ITrainingDomainEvent> DomainEvents
        => new ReadOnlyCollection<ITrainingDomainEvent>(_domainEvents);

    // For rehydration only.
    private Training() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Training aggregate and raises <see cref="TrainingCreatedEvent"/>.</summary>
    public static Training Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string courseName = "",
        string courseCode = "",
        UserId workerId = default,
        string complianceCategory = "Mandatory",
        UtcTimestamp? assignedAtUtc = null,
        UtcTimestamp? dueAtUtc = null)
    {
        var agg = new Training
        {
            TrainingId           = TrainingId.New(),
            TenantId             = tenantId,
            SiteId               = siteId,
            Status               = TrainingStatus.Created,
            CourseName           = (courseName ?? string.Empty).Trim(),
            CourseCode           = (courseCode ?? string.Empty).Trim(),
            WorkerId             = workerId,
            ComplianceCategory   = (complianceCategory ?? "Mandatory").Trim(),
            AssignedAtUtc        = assignedAtUtc ?? createdAtUtc,
            DueAtUtc             = dueAtUtc,
            CreatedAtUtc         = createdAtUtc,
            CreatedBy            = createdBy,
        };

        agg.AddDomainEvent(new TrainingCreatedEvent(
            EventId:       new TrainingEventId(Uuid7.New()),
            TrainingId:      agg.TrainingId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ITrainingDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params TrainingStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Training {TrainingId}.");
    }
}
