// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityContact
//  Layer:          Domain
//  File:           CommunityContact.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the CommunityContact bounded context (Community Contact
//                  Registry subdomain of CommunitySafety). Represents a single point of contact
//                  in the surrounding community used for outreach and notification (a resident,
//                  local official, emergency-services liaison, media contact, community group, or
//                  regulatory agency contact).
//
//                  Data governance: per the CommunitySafety module's PII minimization mandate,
//                  this aggregate deliberately stores only the minimal field set required for
//                  outreach (display name, contact type, organization, a single email and a
//                  single phone number, and a coarse-grained community area label). It does NOT
//                  store home addresses, dates of birth, or any other sensitive PII. Access to
//                  this aggregate's data MUST be restricted to roles holding
//                  CommunitySafety.CommunityContact.View (see CommunitySafetyModule.Permissions).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityContact.Enums;
using PlantSafety.Domain.CommunitySafety.CommunityContact.Events;
using PlantSafety.Domain.CommunitySafety.CommunityContact.Exceptions;
using PlantSafety.Domain.CommunitySafety.CommunityContact.Rules;
using PlantSafety.Domain.CommunitySafety.CommunityContact.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityContact.Aggregates;

/// <summary>Aggregate root for the CommunityContact bounded context.</summary>
public sealed partial class CommunityContact
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this CommunityContact aggregate.</summary>
    public CommunityContactId CommunityContactId { get; private set; }

    /// <summary>Tenant that owns this contact record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site the contact is primarily associated with, or null when tenant-wide.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public CommunityContactStatus Status { get; private set; }

    // ---- Contact-specific business properties (PII-minimal) ------------------

    /// <summary>Display name of the contact (person or role, e.g. "Fire Chief - Station 4").</summary>
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>Classification of the community relationship this contact represents.</summary>
    public CommunityContactType ContactType { get; private set; }

    /// <summary>Organization/affiliation, if any (e.g. "City Council", "Local News 9").</summary>
    public string? Organization { get; private set; }

    /// <summary>Coarse-grained community area/neighborhood label (never a full street address).</summary>
    public string CommunityArea { get; private set; } = string.Empty;

    /// <summary>Contact email address used for outreach/notification.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>Contact phone number used for outreach/notification, if provided.</summary>
    public string? Phone { get; private set; }

    /// <summary>Whether this contact has opted in to receive proactive community notifications.</summary>
    public bool OptedInForNotifications { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>UTC instant this record was archived, once archived.</summary>
    public UtcTimestamp? ArchivedAtUtc { get; private set; }

    /// <summary>Reason recorded for archival, once archived.</summary>
    public string? ArchivalReason { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<ICommunityContactDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ICommunityContactDomainEvent> DomainEvents
        => new ReadOnlyCollection<ICommunityContactDomainEvent>(_domainEvents);

    // For rehydration only.
    private CommunityContact() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>
    /// Registers a new CommunityContact aggregate and raises
    /// <see cref="CommunityContactRegisteredEvent"/>.
    /// </summary>
    /// <exception cref="ArgumentException">A required field is missing or empty.</exception>
    public static CommunityContact Register(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string displayName,
        CommunityContactType contactType,
        string communityArea,
        string email,
        string? organization = null,
        string? phone = null,
        bool optedInForNotifications = true)
    {
        var normalizedDisplayName = (displayName ?? string.Empty).Trim();
        var normalizedCommunityArea = (communityArea ?? string.Empty).Trim();
        var normalizedEmail = (email ?? string.Empty).Trim();

        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(normalizedDisplayName))
            throw new ArgumentException("DisplayName is required.", nameof(displayName));
        if (string.IsNullOrWhiteSpace(normalizedCommunityArea))
            throw new ArgumentException("CommunityArea is required.", nameof(communityArea));
        if (string.IsNullOrWhiteSpace(normalizedEmail))
            throw new ArgumentException("Email is required.", nameof(email));

        var agg = new CommunityContact
        {
            CommunityContactId = CommunityContactId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = CommunityContactStatus.Active,
            DisplayName = normalizedDisplayName,
            ContactType = contactType,
            Organization = string.IsNullOrWhiteSpace(organization) ? null : organization.Trim(),
            CommunityArea = normalizedCommunityArea,
            Email = normalizedEmail,
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            OptedInForNotifications = optedInForNotifications,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new CommunityContactRegisteredEvent(
            CommunityContactEventId.New(), agg.CommunityContactId, agg.ContactType,
            createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    /// <summary>Reconstructs a CommunityContact aggregate from previously persisted state, without raising domain events.</summary>
    public static CommunityContact Rehydrate(
        CommunityContactId communityContactId,
        Guid tenantId,
        Guid? siteId,
        CommunityContactStatus status,
        string displayName,
        CommunityContactType contactType,
        string? organization,
        string communityArea,
        string email,
        string? phone,
        bool optedInForNotifications,
        UtcTimestamp createdAtUtc,
        UserId createdBy,
        UtcTimestamp? archivedAtUtc,
        string? archivalReason)
        => new()
        {
            CommunityContactId = communityContactId,
            TenantId = tenantId,
            SiteId = siteId,
            Status = status,
            DisplayName = displayName,
            ContactType = contactType,
            Organization = organization,
            CommunityArea = communityArea,
            Email = email,
            Phone = phone,
            OptedInForNotifications = optedInForNotifications,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
            ArchivedAtUtc = archivedAtUtc,
            ArchivalReason = archivalReason,
        };

    // ---- Lifecycle: SetInactive / Reactivate ----------------------------------

    /// <summary>Marks this contact Inactive (e.g. contact requested no further outreach).</summary>
    /// <exception cref="CommunityContactInvalidStatusTransitionException">The transition is not permitted.</exception>
    public void SetInactive(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!CommunityContactLifecycleRules.CanTransition(Status, CommunityContactStatus.Inactive))
            throw new CommunityContactInvalidStatusTransitionException(
                CommunityContactId, Status.ToString(), CommunityContactStatus.Inactive.ToString(), correlationId, causationId);

        Status = CommunityContactStatus.Inactive;
        OptedInForNotifications = false;
    }

    /// <summary>Reactivates a previously Inactive contact.</summary>
    /// <exception cref="CommunityContactInvalidStatusTransitionException">The transition is not permitted.</exception>
    public void Reactivate(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!CommunityContactLifecycleRules.CanTransition(Status, CommunityContactStatus.Active))
            throw new CommunityContactInvalidStatusTransitionException(
                CommunityContactId, Status.ToString(), CommunityContactStatus.Active.ToString(), correlationId, causationId);

        Status = CommunityContactStatus.Active;
    }

    // ---- Lifecycle: Archive ---------------------------------------------------

    /// <summary>
    /// Archives (soft-deletes) the contact, e.g. to satisfy a public-records redaction request.
    /// Raises <see cref="CommunityContactArchivedEvent"/>.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="reason"/> is empty.</exception>
    /// <exception cref="CommunityContactInvalidStatusTransitionException">The contact is already Archived.</exception>
    public void Archive(string reason, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!CommunityContactLifecycleRules.CanTransition(Status, CommunityContactStatus.Archived))
            throw new CommunityContactInvalidStatusTransitionException(
                CommunityContactId, Status.ToString(), CommunityContactStatus.Archived.ToString(), correlationId, causationId);

        var normalizedReason = (reason ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedReason))
            throw new ArgumentException("Reason is required to archive a contact.", nameof(reason));

        Status = CommunityContactStatus.Archived;
        ArchivalReason = normalizedReason;
        ArchivedAtUtc = nowUtc;
        OptedInForNotifications = false;

        AddDomainEvent(new CommunityContactArchivedEvent(
            CommunityContactEventId.New(), CommunityContactId, normalizedReason, nowUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ICommunityContactDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
