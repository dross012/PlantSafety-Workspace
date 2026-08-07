// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Domain
//  File:           PackagingSpecification.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the PHMSA Packaging Compliance bounded context. Tracks a
//                  single DOT packaging specification (49 CFR Part 178): the spec code,
//                  performance tests, capacity, closure instructions, reuse restrictions, and
//                  an optional association to a HazmatMaterial. Mirrors the Compliance.Permits
//                  aggregate's hand-rolled lifecycle pattern.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.Events;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.Exceptions;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.Rules;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Packaging.Aggregates;

/// <summary>Aggregate root for the PHMSA Packaging Compliance bounded context.</summary>
public sealed class PackagingSpecification
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this PackagingSpecification aggregate.</summary>
    public PackagingSpecificationId Id { get; private set; }

    /// <summary>
    /// Tenant that owns this specification record. Added additively (2026-08-07, Application/
    /// Infrastructure/API build-out pass) so the Infrastructure layer can satisfy the
    /// tenant-scoped, NOT NULL TenantId column on [Compliance].[PHMSA_PackagingSpecification] --
    /// mirrors every other tenant-scoped aggregate root in this codebase (e.g.
    /// PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.TenantId and the identical
    /// precedent already applied to PlantSafety.Domain.Compliance.PHMSA.Placarding.Aggregates.
    /// HazmatContainer.TenantId in this same build-out pass).
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PackagingSpecificationStatus Status { get; private set; }

    // ---- DOT packaging fields (49 CFR Part 178) --------------------------------

    /// <summary>DOT packaging specification code, e.g. "1A1", "3A", "4G", "31HA1".</summary>
    public DOTSpec DOTSpec { get; private set; }

    /// <summary>Performance tests this packaging has passed (e.g. "Drop Test", "Hydrostatic Pressure Test").</summary>
    public IReadOnlyList<string> PerformanceTests { get; private set; } = [];

    /// <summary>Maximum rated capacity.</summary>
    public decimal MaxCapacity { get; private set; }

    /// <summary>Unit of measure for <see cref="MaxCapacity"/>, e.g. "L", "kg".</summary>
    public string CapacityUnit { get; private set; } = string.Empty;

    /// <summary>Closure instructions required for this packaging to remain compliant.</summary>
    public string ClosureInstructions { get; private set; } = string.Empty;

    /// <summary>Restrictions on reuse of this packaging, if any (e.g. single-trip only).</summary>
    public string? ReuseRestrictions { get; private set; }

    /// <summary>HazmatMaterial this packaging spec was authorized for, if applicable.</summary>
    public HazmatMaterialId? AssociatedMaterialId { get; private set; }

    // ---- Authorship / audit ----------------------------------------------------

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    // ---- Lifecycle attributes ---------------------------------------------------

    /// <summary>Identity of the PackagingSpecification that superseded this one, if any.</summary>
    public PackagingSpecificationId? SupersededBySpecificationId { get; private set; }

    /// <summary>UTC instant this specification was superseded.</summary>
    public UtcTimestamp? SupersededAtUtc { get; private set; }

    /// <summary>UTC instant this specification was retired.</summary>
    public UtcTimestamp? RetiredAtUtc { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPackagingSpecificationDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPackagingSpecificationDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPackagingSpecificationDomainEvent>(_domainEvents);

    // For rehydration only.
    private PackagingSpecification() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>
    /// Authorizes a new PackagingSpecification and raises <see cref="PackagingSpecificationAuthorizedEvent"/>.
    /// </summary>
    public static PackagingSpecification Create(
        DOTSpec dotSpec,
        IReadOnlyList<string> performanceTests,
        decimal maxCapacity,
        string capacityUnit,
        string closureInstructions,
        string? reuseRestrictions,
        HazmatMaterialId? associatedMaterialId,
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId = null)
    {
        if (maxCapacity <= 0)
            throw new ArgumentException("MaxCapacity must be positive.", nameof(maxCapacity));
        if (string.IsNullOrWhiteSpace(capacityUnit))
            throw new ArgumentException("CapacityUnit must not be empty.", nameof(capacityUnit));

        var agg = new PackagingSpecification
        {
            Id                    = PackagingSpecificationId.New(),
            TenantId              = tenantId,
            SiteId                = siteId,
            Status                = PackagingSpecificationStatus.Authorized,
            DOTSpec               = dotSpec,
            PerformanceTests      = performanceTests ?? [],
            MaxCapacity           = maxCapacity,
            CapacityUnit          = capacityUnit.Trim(),
            ClosureInstructions   = (closureInstructions ?? string.Empty).Trim(),
            ReuseRestrictions     = string.IsNullOrWhiteSpace(reuseRestrictions) ? null : reuseRestrictions.Trim(),
            AssociatedMaterialId  = associatedMaterialId,
            CreatedBy             = createdBy,
            CreatedAtUtc          = createdAtUtc,
        };

        agg.AddDomainEvent(new PackagingSpecificationAuthorizedEvent(
            PackagingSpecificationId: agg.Id,
            DOTSpec:                  dotSpec,
            AuthorizedAtUtc:          createdAtUtc,
            Actor:                    actor,
            CorrelationId:            correlationId,
            CausationId:              causationId));

        return agg;
    }

    // ---- Lifecycle: Supersede / Retire ---------------------------------------

    /// <summary>
    /// Marks this specification superseded by a newer authorization (Authorized -&gt; Superseded).
    /// Valid only from <see cref="PackagingSpecificationStatus.Authorized"/>.
    /// </summary>
    public void Supersede(
        PackagingSpecificationId supersededBySpecificationId,
        UtcTimestamp supersededAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusGuard(correlationId, causationId, PackagingSpecificationLifecycleRules.CanSupersede, "Superseded");

        SupersededBySpecificationId = supersededBySpecificationId;
        SupersededAtUtc = supersededAtUtc;
        Status = PackagingSpecificationStatus.Superseded;

        AddDomainEvent(new PackagingSpecificationSupersededEvent(
            PackagingSpecificationId:    Id,
            SupersededBySpecificationId: supersededBySpecificationId,
            SupersededAtUtc:             supersededAtUtc,
            Actor:                       actor,
            CorrelationId:               correlationId,
            CausationId:                 causationId));
    }

    /// <summary>Retires this specification record (any non-Retired status -&gt; Retired).</summary>
    public void Retire(
        UtcTimestamp retiredAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusGuard(correlationId, causationId, PackagingSpecificationLifecycleRules.CanRetire, "Retired");

        RetiredAtUtc = retiredAtUtc;
        Status = PackagingSpecificationStatus.Retired;

        AddDomainEvent(new PackagingSpecificationRetiredEvent(
            PackagingSpecificationId: Id,
            RetiredAtUtc:             retiredAtUtc,
            Actor:                    actor,
            CorrelationId:            correlationId,
            CausationId:              causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPackagingSpecificationDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusGuard(CorrelationId correlationId, CausationId causationId, Func<PackagingSpecificationStatus, bool> guard, string targetStatus)
    {
        if (guard(Status)) return;

        throw new PackagingSpecificationInvalidStatusTransitionException(
            Id, Status.ToString(), targetStatus, correlationId, causationId);
    }
}
