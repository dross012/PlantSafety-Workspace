// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.SafetyCaseValidation
//  Layer:          Domain
//  File:           SafetyCaseValidation.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single safety-case validation record supporting ANSI/RIA
//                  R15.08 (mobile robot safety) and ISO 3691-4 (AGV) evidence trails. Models
//                  submission, validation, rejection, and expiry, driving the recurring safety-case
//                  revalidation cadence. Safety-case documents are version-controlled; prior
//                  versions are retained for audit rather than overwritten. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Aggregates;

/// <summary>Aggregate root for a single safety-case validation record.</summary>
public sealed partial class SafetyCaseValidation
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this SafetyCaseValidation aggregate.</summary>
    public SafetyCaseValidationId SafetyCaseValidationId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public SafetyCaseValidationStatus Status { get; private set; }

    // ---- Safety case ---------------------------------------------------------

    /// <summary>Identity of the autonomous vehicle asset this safety case applies to.</summary>
    public Guid AutonomousVehicleAssetId { get; private set; }

    /// <summary>Standard the safety case is validated against.</summary>
    public SafetyStandardReference StandardReference { get; private set; }

    /// <summary>Version identifier of the safety-case document.</summary>
    public string DocumentVersion { get; private set; } = string.Empty;

    /// <summary>Cadence, in days, at which this safety case must be revalidated.</summary>
    public int RevalidationCadenceDays { get; private set; }

    /// <summary>Identity of the user who validated the case, if validated.</summary>
    public UserId? ValidatedByUserId { get; private set; }

    /// <summary>UTC timestamp the case was validated, if validated.</summary>
    public UtcTimestamp? ValidatedAtUtc { get; private set; }

    /// <summary>UTC timestamp the validation expires, if validated.</summary>
    public UtcTimestamp? ExpiresAtUtc { get; private set; }

    /// <summary>Reason recorded when the case is rejected, if any.</summary>
    public string? RejectionReason { get; private set; }

    // ---- Audit -------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -------------------------------------------------------

    private readonly List<ISafetyCaseValidationDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ISafetyCaseValidationDomainEvent> DomainEvents
        => new ReadOnlyCollection<ISafetyCaseValidationDomainEvent>(_domainEvents);

    // For rehydration only.
    private SafetyCaseValidation() { }

    // ---- Factory: Create (Submit) -----------------------------------------------

    /// <summary>Submits a new safety case for validation and raises <see cref="SafetyCaseSubmittedEvent"/>.</summary>
    public static SafetyCaseValidation Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid autonomousVehicleAssetId = default,
        SafetyStandardReference standardReference = SafetyStandardReference.AnsiRia1508,
        string documentVersion = "",
        int revalidationCadenceDays = 365)
    {
        var agg = new SafetyCaseValidation
        {
            SafetyCaseValidationId = SafetyCaseValidationId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = SafetyCaseValidationStatus.PendingReview,
            AutonomousVehicleAssetId = autonomousVehicleAssetId,
            StandardReference = standardReference,
            DocumentVersion = (documentVersion ?? string.Empty).Trim(),
            RevalidationCadenceDays = revalidationCadenceDays,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new SafetyCaseSubmittedEvent(
            SafetyCaseValidationEventId.New(), agg.SafetyCaseValidationId, autonomousVehicleAssetId, standardReference, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: Validate -----------------------------------------------------

    /// <summary>Validates the safety case, computing its expiry from the revalidation cadence.</summary>
    public void Validate(UserId validatedByUserId, UtcTimestamp validatedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(SafetyCaseValidationStatus.PendingReview);

        ValidatedByUserId = validatedByUserId;
        ValidatedAtUtc = validatedAtUtc;
        ExpiresAtUtc = UtcTimestamp.From(validatedAtUtc.Value.AddDays(RevalidationCadenceDays));
        Status = SafetyCaseValidationStatus.Validated;

        AddDomainEvent(new SafetyCaseValidatedEvent(
            SafetyCaseValidationEventId.New(), SafetyCaseValidationId, ExpiresAtUtc.Value, validatedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Reject -------------------------------------------------------

    /// <summary>Rejects the safety case.</summary>
    public void Reject(string reason, UtcTimestamp rejectedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(SafetyCaseValidationStatus.PendingReview);

        RejectionReason = (reason ?? string.Empty).Trim();
        Status = SafetyCaseValidationStatus.Rejected;

        AddDomainEvent(new SafetyCaseRejectedEvent(
            SafetyCaseValidationEventId.New(), SafetyCaseValidationId, RejectionReason, rejectedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Expire -------------------------------------------------------

    /// <summary>Expires a previously validated safety case.</summary>
    public void Expire(UtcTimestamp expiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(SafetyCaseValidationStatus.Validated);

        Status = SafetyCaseValidationStatus.Expired;

        AddDomainEvent(new SafetyCaseExpiredEvent(
            SafetyCaseValidationEventId.New(), SafetyCaseValidationId, expiredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ISafetyCaseValidationDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params SafetyCaseValidationStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for SafetyCaseValidation {SafetyCaseValidationId}.");
    }
}
