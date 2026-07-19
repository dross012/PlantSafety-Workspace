// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.RiskManagement
//  Layer:          Domain
//  File:           RiskManagement.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  hazard/risk-assessment record shape grounded in
//                  project_plantsafety_domains.md Module 15 (Risk Management: "Hazard
//                  identification, risk assessment, control management", flagged as a
//                  Critical Module). Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the RiskManagement bounded context. Tracks a
//                  single hazard/risk assessment: its description, category,
//                  likelihood/severity ratings, computed risk score, and control
//                  measures.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.RiskManagement.Aggregates;

/// <summary>Aggregate root for the RiskManagement bounded context.</summary>
public sealed partial class RiskManagement
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this RiskManagement aggregate.</summary>
    public RiskManagementId RiskManagementId { get; private set; }

    /// <summary>Tenant that owns this risk record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this risk assessment is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public RiskManagementStatus Status { get; private set; }

    // ---- Risk-specific business properties -------------------------------------

    /// <summary>Description of the identified hazard.</summary>
    public string HazardDescription { get; private set; } = string.Empty;

    /// <summary>Risk category (e.g., Mechanical, Chemical, Electrical, Ergonomic, Fire).</summary>
    public string RiskCategory { get; private set; } = string.Empty;

    /// <summary>Likelihood rating on a 1-5 scale.</summary>
    public int LikelihoodRating { get; private set; } = 1;

    /// <summary>Severity rating on a 1-5 scale.</summary>
    public int SeverityRating { get; private set; } = 1;

    /// <summary>Computed risk score (LikelihoodRating * SeverityRating).</summary>
    public int RiskScore => LikelihoodRating * SeverityRating;

    /// <summary>Control measures identified to mitigate this risk.</summary>
    public string ControlMeasures { get; private set; } = string.Empty;

    /// <summary>Identity of the party accountable for risk control verification.</summary>
    public UserId ResponsiblePartyId { get; private set; }

    /// <summary>UTC timestamp this risk assessment is next due for review.</summary>
    public UtcTimestamp? ReviewDueAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IRiskManagementDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IRiskManagementDomainEvent> DomainEvents
        => new ReadOnlyCollection<IRiskManagementDomainEvent>(_domainEvents);

    // For rehydration only.
    private RiskManagement() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new RiskManagement aggregate and raises <see cref="RiskManagementCreatedEvent"/>.</summary>
    public static RiskManagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string hazardDescription = "",
        string riskCategory = "",
        int likelihoodRating = 1,
        int severityRating = 1,
        string controlMeasures = "",
        UserId responsiblePartyId = default,
        UtcTimestamp? reviewDueAtUtc = null)
    {
        var agg = new RiskManagement
        {
            RiskManagementId    = RiskManagementId.New(),
            TenantId            = tenantId,
            SiteId              = siteId,
            Status              = RiskManagementStatus.Created,
            HazardDescription   = (hazardDescription ?? string.Empty).Trim(),
            RiskCategory        = (riskCategory ?? string.Empty).Trim(),
            LikelihoodRating    = Math.Clamp(likelihoodRating, 1, 5),
            SeverityRating      = Math.Clamp(severityRating, 1, 5),
            ControlMeasures     = (controlMeasures ?? string.Empty).Trim(),
            ResponsiblePartyId  = responsiblePartyId,
            ReviewDueAtUtc      = reviewDueAtUtc,
            CreatedAtUtc        = createdAtUtc,
            CreatedBy           = createdBy,
        };

        agg.AddDomainEvent(new RiskManagementCreatedEvent(
            EventId:       new RiskManagementEventId(Uuid7.New()),
            RiskManagementId:      agg.RiskManagementId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IRiskManagementDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params RiskManagementStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for RiskManagement {RiskManagementId}.");
    }
}
