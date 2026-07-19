// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Contractor
//  Layer:          Domain
//  File:           Contractor.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  contractor-management record shape grounded in
//                  project_plantsafety_domains.md Module 6 (Contractor: "Contractor
//                  onboarding, qualification, and compliance scoring").
//                  Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Contractor bounded context. Tracks a single
//                  contractor organization/engagement: its company, qualification
//                  status, readiness score, insurance expiry, and primary contact.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Contractor.Aggregates;

/// <summary>Aggregate root for the Contractor bounded context.</summary>
public sealed partial class Contractor
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Contractor aggregate.</summary>
    public ContractorId ContractorId { get; private set; }

    /// <summary>Tenant that owns this contractor record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this contractor engagement is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ContractorStatus Status { get; private set; }

    // ---- Contractor-specific business properties ------------------------------

    /// <summary>Contractor company/organization name.</summary>
    public string ContractorCompany { get; private set; } = string.Empty;

    /// <summary>Individual contractor/worker name, if this record represents an individual.</summary>
    public string ContractorName { get; private set; } = string.Empty;

    /// <summary>Qualification status: Pending, Qualified, Expired, Disqualified.</summary>
    public string QualificationStatus { get; private set; } = "Pending";

    /// <summary>Computed contractor readiness/compliance score (0-100).</summary>
    public decimal ReadinessScore { get; private set; }

    /// <summary>Identity of the primary contact for this contractor engagement.</summary>
    public UserId PrimaryContactId { get; private set; }

    /// <summary>UTC timestamp the contractor's insurance coverage expires.</summary>
    public UtcTimestamp? InsuranceExpiresAtUtc { get; private set; }

    /// <summary>UTC timestamp the contractor completed onboarding.</summary>
    public UtcTimestamp? OnboardedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IContractorDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IContractorDomainEvent> DomainEvents
        => new ReadOnlyCollection<IContractorDomainEvent>(_domainEvents);

    // For rehydration only.
    private Contractor() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Contractor aggregate and raises <see cref="ContractorCreatedEvent"/>.</summary>
    public static Contractor Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string contractorCompany = "",
        string contractorName = "",
        string qualificationStatus = "Pending",
        decimal readinessScore = 0m,
        UserId primaryContactId = default,
        UtcTimestamp? insuranceExpiresAtUtc = null,
        UtcTimestamp? onboardedAtUtc = null)
    {
        var agg = new Contractor
        {
            ContractorId            = ContractorId.New(),
            TenantId                = tenantId,
            SiteId                  = siteId,
            Status                  = ContractorStatus.Created,
            ContractorCompany       = (contractorCompany ?? string.Empty).Trim(),
            ContractorName          = (contractorName ?? string.Empty).Trim(),
            QualificationStatus     = (qualificationStatus ?? "Pending").Trim(),
            ReadinessScore          = readinessScore,
            PrimaryContactId        = primaryContactId,
            InsuranceExpiresAtUtc   = insuranceExpiresAtUtc,
            OnboardedAtUtc          = onboardedAtUtc,
            CreatedAtUtc            = createdAtUtc,
            CreatedBy               = createdBy,
        };

        agg.AddDomainEvent(new ContractorCreatedEvent(
            EventId:       new ContractorEventId(Uuid7.New()),
            ContractorId:      agg.ContractorId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IContractorDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ContractorStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Contractor {ContractorId}.");
    }
}
