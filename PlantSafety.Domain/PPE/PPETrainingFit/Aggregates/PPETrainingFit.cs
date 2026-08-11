// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPETrainingFit
//  Layer:          Domain
//  File:           PPETrainingFit.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the PPETrainingFit bounded context (module: PPE).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPETrainingFit.Aggregates;

/// <summary>Aggregate root for the PPETrainingFit bounded context.</summary>
public sealed partial class PPETrainingFit
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this PPETrainingFit aggregate.</summary>
    public PPETrainingFitId PPETrainingFitId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PPETrainingFitStatus Status { get; private set; }

    // ---- PPETrainingFit-specific business properties --------------------------------

    /// <summary>WorkerId business attribute.</summary>
    public Guid WorkerId { get; private set; }

    /// <summary>TrainingType business attribute.</summary>
    public string TrainingType { get; private set; }

    /// <summary>FitTestExpiresAtUtc business attribute.</summary>
    public UtcTimestamp? FitTestExpiresAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPPETrainingFitDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPPETrainingFitDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPPETrainingFitDomainEvent>(_domainEvents);

    // For rehydration only.
    private PPETrainingFit() 
    {
        // Assigned by the data mapper immediately after construction; see the public
        // constructor for the invariant these properties actually hold.
        TrainingType = null!;
    }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new PPETrainingFit aggregate and raises <see cref="PPETrainingFitCreatedEvent"/>.</summary>
    public static PPETrainingFit Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid workerId = default,
        string trainingType = "",
        UtcTimestamp? fitTestExpiresAtUtc = null)
    {
        var agg = new PPETrainingFit
        {
            PPETrainingFitId     = PPETrainingFitId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = PPETrainingFitStatus.Created,
            WorkerId = workerId,
            TrainingType = (trainingType ?? string.Empty).Trim(),
            FitTestExpiresAtUtc = fitTestExpiresAtUtc,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new PPETrainingFitCreatedEvent(
            EventId:       new PPETrainingFitEventId(Uuid7.New()),
            PPETrainingFitId:      agg.PPETrainingFitId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPPETrainingFitDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PPETrainingFitStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for PPETrainingFit {PPETrainingFitId}.");
    }
}