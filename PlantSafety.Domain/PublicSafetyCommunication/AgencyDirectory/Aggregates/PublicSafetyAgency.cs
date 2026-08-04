// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.AgencyDirectory
//  Layer:          Domain
//  File:           PublicSafetyAgency.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for an external public-safety stakeholder agency (fire, police,
//                  EMS, regulatory hotline, public alert authority, hazmat response, poison
//                  control, emergency management). Owns the agency's named contacts
//                  (AgencyContact child entities) and models the real directory lifecycle:
//                  registration, contact management, scheduled verification/refresh, and
//                  deactivation. Data-governance hook: contact data is kept current via
//                  scheduled verification (see AgencyVerificationStatus / RecordVerification).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Aggregates;

/// <summary>Aggregate root for an external public-safety stakeholder agency.</summary>
public sealed partial class PublicSafetyAgency
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this PublicSafetyAgency aggregate.</summary>
    public PublicSafetyAgencyId PublicSafetyAgencyId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current operational status (Active/Inactive).</summary>
    public AgencyOperationalStatus Status { get; private set; }

    // ---- Agency identity -------------------------------------------------------

    /// <summary>Legal/display name of the agency.</summary>
    public string AgencyName { get; private set; } = string.Empty;

    /// <summary>Classification of this agency.</summary>
    public PublicSafetyAgencyType AgencyType { get; private set; }

    /// <summary>Description of the jurisdiction/coverage area this agency serves.</summary>
    public string JurisdictionDescription { get; private set; } = string.Empty;

    /// <summary>Primary phone number for general/dispatch contact.</summary>
    public string PrimaryPhoneNumber { get; private set; } = string.Empty;

    /// <summary>Primary email address, if any.</summary>
    public string? PrimaryEmail { get; private set; }

    // ---- Verification (data-governance hook) ------------------------------------

    /// <summary>Current verification/currency status of this agency's contact data.</summary>
    public AgencyVerificationStatus VerificationStatus { get; private set; }

    /// <summary>UTC timestamp of the most recent successful verification, if any.</summary>
    public UtcTimestamp? LastVerifiedAtUtc { get; private set; }

    /// <summary>UTC timestamp the next scheduled verification is due.</summary>
    public UtcTimestamp? NextVerificationDueAtUtc { get; private set; }

    // ---- Contacts --------------------------------------------------------------

    private readonly List<AgencyContact> _contacts = [];

    /// <summary>Named contacts/desks maintained for this agency.</summary>
    public IReadOnlyCollection<AgencyContact> Contacts => new ReadOnlyCollection<AgencyContact>(_contacts);

    // ---- Audit -------------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -------------------------------------------------------------

    private readonly List<IPublicSafetyAgencyDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPublicSafetyAgencyDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPublicSafetyAgencyDomainEvent>(_domainEvents);

    // For rehydration only.
    private PublicSafetyAgency() { }

    // ---- Factory: Create (Register) ------------------------------------------------

    /// <summary>Registers a new external public-safety agency and raises <see cref="PublicSafetyAgencyRegisteredEvent"/>.</summary>
    public static PublicSafetyAgency Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string agencyName = "",
        PublicSafetyAgencyType agencyType = PublicSafetyAgencyType.Fire,
        string jurisdictionDescription = "",
        string primaryPhoneNumber = "",
        string? primaryEmail = null)
    {
        var agg = new PublicSafetyAgency
        {
            PublicSafetyAgencyId    = PublicSafetyAgencyId.New(),
            TenantId                = tenantId,
            SiteId                  = siteId,
            Status                  = AgencyOperationalStatus.Active,
            AgencyName              = (agencyName ?? string.Empty).Trim(),
            AgencyType              = agencyType,
            JurisdictionDescription = (jurisdictionDescription ?? string.Empty).Trim(),
            PrimaryPhoneNumber      = (primaryPhoneNumber ?? string.Empty).Trim(),
            PrimaryEmail            = string.IsNullOrWhiteSpace(primaryEmail) ? null : primaryEmail.Trim(),
            VerificationStatus      = AgencyVerificationStatus.Unverified,
            CreatedAtUtc            = createdAtUtc,
            CreatedBy               = createdBy,
        };

        agg.AddDomainEvent(new PublicSafetyAgencyRegisteredEvent(
            PublicSafetyAgencyEventId.New(), agg.PublicSafetyAgencyId, agencyType, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: AddContact ----------------------------------------------------

    /// <summary>Adds a named contact/desk to this agency.</summary>
    public AgencyContact AddContact(
        AgencyContactRole role,
        string contactName,
        string phoneNumber,
        string? email,
        UtcTimestamp occurredAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        var contact = new AgencyContact(
            AgencyContactId.New(), PublicSafetyAgencyId, role, contactName, phoneNumber, email, occurredAtUtc);

        _contacts.Add(contact);

        AddDomainEvent(new AgencyContactAddedEvent(
            PublicSafetyAgencyEventId.New(), PublicSafetyAgencyId, contact.AgencyContactId, role, occurredAtUtc, actor, correlationId, causationId));

        return contact;
    }

    // ---- Behavior: RecordVerification (scheduling hook) --------------------------

    /// <summary>Records that this agency's contact data was confirmed current, and schedules the next verification.</summary>
    public void RecordVerification(UtcTimestamp verifiedAtUtc, UtcTimestamp nextVerificationDueAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != AgencyOperationalStatus.Active)
            throw new PublicSafetyAgencyInvalidStatusTransitionException(PublicSafetyAgencyId, Status.ToString(), "Verified", correlationId, causationId);

        VerificationStatus       = AgencyVerificationStatus.Verified;
        LastVerifiedAtUtc         = verifiedAtUtc;
        NextVerificationDueAtUtc  = nextVerificationDueAtUtc;

        AddDomainEvent(new AgencyVerificationRecordedEvent(
            PublicSafetyAgencyEventId.New(), PublicSafetyAgencyId, nextVerificationDueAtUtc, verifiedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: MarkVerificationOverdue (scheduling hook) ----------------------

    /// <summary>Marks scheduled re-verification as overdue (invoked by the periodic verification background job).</summary>
    public void MarkVerificationOverdue(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        VerificationStatus = AgencyVerificationStatus.VerificationOverdue;

        AddDomainEvent(new AgencyVerificationOverdueEvent(
            PublicSafetyAgencyEventId.New(), PublicSafetyAgencyId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Deactivate ------------------------------------------------------

    /// <summary>Deactivates the agency so it no longer receives notifications.</summary>
    public void Deactivate(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == AgencyOperationalStatus.Inactive)
            throw new PublicSafetyAgencyInvalidStatusTransitionException(PublicSafetyAgencyId, Status.ToString(), AgencyOperationalStatus.Inactive.ToString(), correlationId, causationId);

        Status = AgencyOperationalStatus.Inactive;

        AddDomainEvent(new AgencyDeactivatedEvent(
            PublicSafetyAgencyEventId.New(), PublicSafetyAgencyId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPublicSafetyAgencyDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
