// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Domain
//  File:           TenantEvents.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §2.1/§2.3 Tenant Creation & Lifecycle)
//  SchemaVersion:  2026.1
//  Description:    Concrete domain events raised by the Tenant aggregate across its provisioning
//                  and lifecycle transitions. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.Tenancy.Tenant.Events;

/// <summary>Raised when a new Tenant is registered and enters the automated provisioning pipeline.</summary>
public sealed record TenantProvisionedEvent(
    Guid EventId,
    TenantId TenantId,
    string Name,
    string Region,
    string PlanCode,
    UtcTimestamp ProvisionedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITenantDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TenantProvisioned";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ProvisionedAtUtc;
}

/// <summary>Raised when a Tenant's provisioning pipeline completes and the tenant becomes Active.</summary>
public sealed record TenantActivatedEvent(
    Guid EventId,
    TenantId TenantId,
    UtcTimestamp ActivatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITenantDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TenantActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ActivatedAtUtc;
}

/// <summary>Raised when a Tenant is suspended.</summary>
public sealed record TenantSuspendedEvent(
    Guid EventId,
    TenantId TenantId,
    string Reason,
    UtcTimestamp SuspendedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITenantDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TenantSuspended";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => SuspendedAtUtc;
}

/// <summary>Raised when a suspended Tenant is reactivated.</summary>
public sealed record TenantReactivatedEvent(
    Guid EventId,
    TenantId TenantId,
    UtcTimestamp ReactivatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITenantDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TenantReactivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ReactivatedAtUtc;
}

/// <summary>Raised when a Tenant's data is archived (Phase 7 §2.3 lifecycle management).</summary>
public sealed record TenantArchivedEvent(
    Guid EventId,
    TenantId TenantId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITenantDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TenantArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}

/// <summary>Raised when a Tenant is soft-deleted.</summary>
public sealed record TenantDeletedEvent(
    Guid EventId,
    TenantId TenantId,
    string Reason,
    UtcTimestamp DeletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITenantDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TenantDeleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DeletedAtUtc;
}

/// <summary>Raised when a Tenant's TenantConfiguration (modules/feature-flag overrides/compliance rules) is updated.</summary>
public sealed record TenantConfigurationUpdatedEvent(
    Guid EventId,
    TenantId TenantId,
    UtcTimestamp UpdatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITenantDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TenantConfigurationUpdated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => UpdatedAtUtc;
}
