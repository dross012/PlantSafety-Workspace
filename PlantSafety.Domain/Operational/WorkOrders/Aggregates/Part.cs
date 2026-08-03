// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Domain
//  File:           Part.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Part (maintenance parts inventory) bounded context.
//                  Deliberately simple per scoping (docs/Phase5/ui-gaps-scoping.md "Maintenance"
//                  section: "read-mostly, maybe simple stock tracking") -- tracks on-hand quantity,
//                  reorder threshold, and storage location, with a single stock-adjustment
//                  operation. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.WorkOrders.Events;
using PlantSafety.Domain.Operational.WorkOrders.Exceptions;
using PlantSafety.Domain.Operational.WorkOrders.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.WorkOrders.Aggregates;

/// <summary>Aggregate root for the Part (maintenance parts inventory) bounded context.</summary>
public sealed class Part
{
    /// <summary>Strong-typed identity of this Part aggregate.</summary>
    public PartId PartId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Human-readable part number.</summary>
    public string PartNumber { get; private set; } = string.Empty;

    /// <summary>Part display name.</summary>
    public string PartName { get; private set; } = string.Empty;

    /// <summary>Quantity currently on hand.</summary>
    public int QuantityOnHand { get; private set; }

    /// <summary>Reorder threshold -- QuantityOnHand at or below this value needs reordering.</summary>
    public int ReorderThreshold { get; private set; }

    /// <summary>Storage location.</summary>
    public string Location { get; private set; } = string.Empty;

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user/system that created this record.</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>UTC instant of the most recent update to this record.</summary>
    public UtcTimestamp UpdatedAtUtc { get; private set; }

    private readonly List<IPartDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPartDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPartDomainEvent>(_domainEvents);

    // For rehydration only.
    private Part() { }

    /// <summary>Creates a new Part inventory record.</summary>
    public static Part Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        Guid tenantId,
        Guid? siteId,
        string partNumber,
        string partName,
        int initialQuantityOnHand,
        int reorderThreshold,
        string location)
    {
        if (string.IsNullOrWhiteSpace(partNumber))
            throw new ArgumentException("PartNumber must be supplied.", nameof(partNumber));
        if (string.IsNullOrWhiteSpace(partName))
            throw new ArgumentException("PartName must be supplied.", nameof(partName));
        if (initialQuantityOnHand < 0)
            throw new ArgumentOutOfRangeException(nameof(initialQuantityOnHand), "Initial quantity cannot be negative.");

        return new Part
        {
            PartId = PartId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            PartNumber = partNumber.Trim(),
            PartName = partName.Trim(),
            QuantityOnHand = initialQuantityOnHand,
            ReorderThreshold = reorderThreshold,
            Location = (location ?? string.Empty).Trim(),
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
            UpdatedAtUtc = createdAtUtc,
        };
    }

    /// <summary>
    /// Adjusts QuantityOnHand by <paramref name="deltaQuantity"/> (positive = restock, negative =
    /// consumption). Throws <see cref="PartInsufficientStockException"/> if the result would go
    /// negative.
    /// </summary>
    public void AdjustStock(
        int deltaQuantity,
        string? reason,
        UtcTimestamp adjustedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        var newQuantity = QuantityOnHand + deltaQuantity;
        if (newQuantity < 0)
            throw new PartInsufficientStockException(PartId, QuantityOnHand, deltaQuantity, correlationId, causationId);

        QuantityOnHand = newQuantity;
        UpdatedAtUtc = adjustedAtUtc;

        AddDomainEvent(new PartStockAdjustedEvent(
            EventId: Guid.NewGuid(),
            PartId: PartId,
            DeltaQuantity: deltaQuantity,
            NewQuantityOnHand: newQuantity,
            Reason: string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            AdjustedAtUtc: adjustedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    private void AddDomainEvent(IPartDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
