// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Domain
//  File:           Tenant.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §1/§2 SaaS Multi-Tenant Architecture)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the platform's first-class tenant registry. Prior to this
//                  file, tenants existed only implicitly as a raw Guid TenantId scattered across
//                  ~150+ tenant-scoped tables/aggregates -- no first-class Tenant entity existed
//                  anywhere in the codebase (confirmed by repo-wide search). This aggregate is the
//                  foundation other Phase 7 work (Billing, Admin Portal) builds on: it owns the
//                  tenant's identity, region, plan reference, lifecycle status, and configuration.
//                  It does NOT own or replace the platform's existing shared-DB + Row-Level-
//                  Security tenant isolation strategy (Security.fn_TenantAccessPredicate,
//                  TenantSessionContextExtensions) -- that mechanism keys off the same raw Guid
//                  TenantId this aggregate's own <see cref="TenantId"/> wraps, and continues to
//                  apply to every other tenant-scoped aggregate/table unchanged (see
//                  docs/Phase7/tenant-isolation-model.md). Additive-only evolution.
// ============================================================================
using System.Collections.ObjectModel;
using PlantSafety.SharedKernel.Identifiers;
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Tenancy.Tenant.Entities;
using PlantSafety.Domain.Tenancy.Tenant.Enums;
using PlantSafety.Domain.Tenancy.Tenant.Events;
using PlantSafety.Domain.Tenancy.Tenant.Exceptions;
using PlantSafety.Domain.Tenancy.Tenant.Rules;
using PlantSafety.Domain.Tenancy.Tenant.ValueObjects;

namespace PlantSafety.Domain.Tenancy.Tenant.Aggregates;

/// <summary>Aggregate root for a single registered platform tenant.</summary>
public sealed class Tenant
{
    // ---- Identity --------------------------------------------------------------

    /// <summary>Strong-typed identity of this tenant. <see cref="TenantId.Value"/> is the same Guid every existing tenant-scoped table/aggregate's TenantId column carries.</summary>
    public TenantId TenantId { get; private set; }

    /// <summary>Tenant's registered display name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Data-residency/deployment region this tenant is pinned to (e.g. <c>"us-east"</c>, <c>"eu-west"</c>). Immutable once provisioned -- region migration is a distinct, explicit operation (Global-Scale Multi-Region architecture), not a plain configuration update.</summary>
    public string Region { get; private set; } = string.Empty;

    /// <summary>Current lifecycle status.</summary>
    public TenantStatus Status { get; private set; } = TenantStatus.Default;

    /// <summary>Subscription plan reference. See <see cref="SubscriptionPlanCode"/> for the scope note on the full Billing module.</summary>
    public SubscriptionPlanCode PlanCode { get; private set; } = SubscriptionPlanCode.Default;

    /// <summary>The tenant's own configuration (enabled modules, feature-flag overrides, custom compliance rules).</summary>
    public TenantConfiguration Configuration { get; private set; } = TenantConfiguration.CreateDefault();

    /// <summary>UTC instant this tenant was registered/provisioning began.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>UTC instant provisioning completed and the tenant became Active, if any.</summary>
    public UtcTimestamp? ActivatedAtUtc { get; private set; }

    /// <summary>UTC instant the tenant was suspended, if currently or previously Suspended.</summary>
    public UtcTimestamp? SuspendedAtUtc { get; private set; }

    /// <summary>Reason recorded for the most recent suspension, if any.</summary>
    public string? SuspensionReason { get; private set; }

    /// <summary>UTC instant the tenant's data was archived, if Archived.</summary>
    public UtcTimestamp? ArchivedAtUtc { get; private set; }

    /// <summary>UTC instant the tenant was soft-deleted, if Deleted.</summary>
    public UtcTimestamp? DeletedAtUtc { get; private set; }

    /// <summary>Reason recorded for soft deletion, if Deleted.</summary>
    public string? DeletionReason { get; private set; }

    // ---- Domain Events -----------------------------------------------------------

    private readonly List<ITenantDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ITenantDomainEvent> DomainEvents
        => new ReadOnlyCollection<ITenantDomainEvent>(_domainEvents);

    // For rehydration only.
    private Tenant() { }

    /// <summary>
    /// Registers a new Tenant in the Provisioning status. The Application-layer
    /// ProvisionTenantCommandHandler drives the remainder of the automated provisioning pipeline
    /// (RLS/session-context init, default reference-data seed, admin user creation, role
    /// assignment, API key generation) and calls <see cref="Activate"/> once every step succeeds.
    /// </summary>
    public static Tenant Provision(
        string name, string region, SubscriptionPlanCode planCode,
        UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant.Name must not be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(region))
            throw new ArgumentException("Tenant.Region must not be empty.", nameof(region));
        ArgumentNullException.ThrowIfNull(planCode);

        var tenant = new Tenant
        {
            TenantId = TenantId.New(),
            Name = name.Trim(),
            Region = region.Trim(),
            PlanCode = planCode,
            Status = TenantStatus.Provisioning,
            Configuration = TenantConfiguration.CreateDefault(),
            CreatedAtUtc = nowUtc,
        };

        tenant.AddDomainEvent(new TenantProvisionedEvent(
            Uuid7.New(), tenant.TenantId, tenant.Name, tenant.Region, tenant.PlanCode.Code,
            nowUtc, actor, correlationId, causationId));

        return tenant;
    }

    /// <summary>Completes provisioning, transitioning Provisioning -> Active.</summary>
    public void Activate(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!TenantLifecycleRules.CanActivate(Status))
            throw new TenantInvalidStateTransitionException(TenantId, Status.Code, "Activate", correlationId, causationId);

        Status = TenantStatus.Active;
        ActivatedAtUtc = nowUtc;

        AddDomainEvent(new TenantActivatedEvent(Uuid7.New(), TenantId, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Suspends an Active tenant (e.g. billing/compliance hold).</summary>
    public void Suspend(string reason, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!TenantLifecycleRules.CanSuspend(Status))
            throw new TenantInvalidStateTransitionException(TenantId, Status.Code, "Suspend", correlationId, causationId);
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Tenant.Suspend requires a non-empty reason.", nameof(reason));

        Status = TenantStatus.Suspended;
        SuspendedAtUtc = nowUtc;
        SuspensionReason = reason.Trim();

        AddDomainEvent(new TenantSuspendedEvent(Uuid7.New(), TenantId, SuspensionReason, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Reactivates a Suspended tenant back to Active.</summary>
    public void Reactivate(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!TenantLifecycleRules.CanReactivate(Status))
            throw new TenantInvalidStateTransitionException(TenantId, Status.Code, "Reactivate", correlationId, causationId);

        Status = TenantStatus.Active;
        SuspensionReason = null;

        AddDomainEvent(new TenantReactivatedEvent(Uuid7.New(), TenantId, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Marks the tenant's data as archived (Phase 7 §2.3). The actual archival data-movement is performed by the Application layer's reuse of Operations.Archival; this only records the outcome on the aggregate.</summary>
    public void Archive(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!TenantLifecycleRules.CanArchive(Status))
            throw new TenantInvalidStateTransitionException(TenantId, Status.Code, "Archive", correlationId, causationId);

        Status = TenantStatus.Archived;
        ArchivedAtUtc = nowUtc;

        AddDomainEvent(new TenantArchivedEvent(Uuid7.New(), TenantId, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Soft-deletes the tenant (terminal; never a hard DELETE per this codebase's canonical pattern).</summary>
    public void SoftDelete(string reason, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!TenantLifecycleRules.CanDelete(Status))
            throw new TenantInvalidStateTransitionException(TenantId, Status.Code, "Delete", correlationId, causationId);
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Tenant.SoftDelete requires a non-empty reason.", nameof(reason));

        Status = TenantStatus.Deleted;
        DeletedAtUtc = nowUtc;
        DeletionReason = reason.Trim();

        AddDomainEvent(new TenantDeletedEvent(Uuid7.New(), TenantId, DeletionReason, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Replaces this tenant's <see cref="Configuration"/>, raising <see cref="TenantConfigurationUpdatedEvent"/>.</summary>
    public void UpdateConfiguration(
        TenantConfiguration configuration, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!TenantLifecycleRules.CanUpdateConfiguration(Status))
            throw new TenantInvalidStateTransitionException(TenantId, Status.Code, "UpdateConfiguration", correlationId, causationId);
        ArgumentNullException.ThrowIfNull(configuration);

        Configuration = configuration;

        AddDomainEvent(new TenantConfigurationUpdatedEvent(Uuid7.New(), TenantId, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Rehydrates a Tenant aggregate from persisted state. Infrastructure-layer use only.</summary>
    public static Tenant Rehydrate(
        TenantId tenantId, string name, string region, TenantStatus status, SubscriptionPlanCode planCode,
        TenantConfiguration configuration, UtcTimestamp createdAtUtc, UtcTimestamp? activatedAtUtc,
        UtcTimestamp? suspendedAtUtc, string? suspensionReason, UtcTimestamp? archivedAtUtc,
        UtcTimestamp? deletedAtUtc, string? deletionReason)
    {
        return new Tenant
        {
            TenantId = tenantId,
            Name = name,
            Region = region,
            Status = status,
            PlanCode = planCode,
            Configuration = configuration,
            CreatedAtUtc = createdAtUtc,
            ActivatedAtUtc = activatedAtUtc,
            SuspendedAtUtc = suspendedAtUtc,
            SuspensionReason = suspensionReason,
            ArchivedAtUtc = archivedAtUtc,
            DeletedAtUtc = deletedAtUtc,
            DeletionReason = deletionReason,
        };
    }

    private void AddDomainEvent(ITenantDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
