// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.Credentials
//  Layer:          Domain
//  File:           Credential.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single physical-access badge/credential issued to an
//                  employee, contractor, or visitor. Credential/PII fields are access-
//                  restricted and audit-logged per data-governance policy; no biometric data
//                  is modeled without an explicit consent workflow.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates;

/// <summary>Aggregate root for a single physical-access badge/credential.</summary>
public sealed partial class Credential
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Credential aggregate.</summary>
    public CredentialId CredentialId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public CredentialStatus Status { get; private set; }

    // ---- Credential identity -----------------------------------------------------

    /// <summary>Physical badge number encoded on the credential.</summary>
    public string BadgeNumber { get; private set; } = string.Empty;

    /// <summary>Identity of the person this credential is issued to.</summary>
    public Guid HolderUserId { get; private set; }

    /// <summary>Classification of the holder.</summary>
    public CredentialHolderType HolderType { get; private set; }

    /// <summary>Access-level label granted by this credential (e.g. "General", "RestrictedZoneA").</summary>
    public string AccessLevel { get; private set; } = string.Empty;

    /// <summary>UTC timestamp the credential was issued.</summary>
    public UtcTimestamp IssuedAtUtc { get; private set; }

    /// <summary>UTC timestamp the credential expires, if any.</summary>
    public UtcTimestamp? ExpiresAtUtc { get; private set; }

    /// <summary>Reason recorded when the credential was revoked, if applicable.</summary>
    public string? RevocationReason { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ICredentialDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ICredentialDomainEvent> DomainEvents
        => new ReadOnlyCollection<ICredentialDomainEvent>(_domainEvents);

    // For rehydration only.
    private Credential() { }

    // ---- Factory: Create (Issue) -----------------------------------------------------------

    /// <summary>Issues a new badge/credential and raises <see cref="CredentialIssuedEvent"/>.</summary>
    public static Credential Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string badgeNumber = "",
        Guid holderUserId = default,
        CredentialHolderType holderType = CredentialHolderType.Employee,
        string accessLevel = "General",
        UtcTimestamp? expiresAtUtc = null)
    {
        var agg = new Credential
        {
            CredentialId  = CredentialId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = CredentialStatus.Issued,
            BadgeNumber   = (badgeNumber ?? string.Empty).Trim(),
            HolderUserId  = holderUserId,
            HolderType    = holderType,
            AccessLevel   = (accessLevel ?? string.Empty).Trim(),
            IssuedAtUtc   = createdAtUtc,
            ExpiresAtUtc  = expiresAtUtc,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new CredentialIssuedEvent(
            CredentialEventId.New(), agg.CredentialId, holderType, holderUserId, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: Suspend -----------------------------------------------

    /// <summary>Temporarily suspends the credential.</summary>
    public void Suspend(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(CredentialStatus.Issued);

        Status = CredentialStatus.Suspended;

        AddDomainEvent(new CredentialSuspendedEvent(
            CredentialEventId.New(), CredentialId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Revoke -----------------------------------------------------------

    /// <summary>Permanently revokes the credential.</summary>
    public void Revoke(string reason, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(CredentialStatus.Issued, CredentialStatus.Suspended);

        Status = CredentialStatus.Revoked;
        RevocationReason = (reason ?? string.Empty).Trim();

        AddDomainEvent(new CredentialRevokedEvent(
            CredentialEventId.New(), CredentialId, RevocationReason, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Expire -----------------------------------------------------------

    /// <summary>Marks the credential expired (invoked by the periodic credential-expiration sweep).</summary>
    public void Expire(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(CredentialStatus.Issued, CredentialStatus.Suspended);

        Status = CredentialStatus.Expired;

        AddDomainEvent(new CredentialExpiredEvent(
            CredentialEventId.New(), CredentialId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ICredentialDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params CredentialStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Credential {CredentialId}.");
    }
}
