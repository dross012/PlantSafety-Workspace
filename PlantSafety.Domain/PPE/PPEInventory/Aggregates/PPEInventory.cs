// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEInventory
//  Layer:          Domain
//  File:           PPEInventory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the PPEInventory bounded context (module: PPE).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEInventory.Aggregates;

/// <summary>Aggregate root for the PPEInventory bounded context.</summary>
public sealed partial class PPEInventory
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this PPEInventory aggregate.</summary>
    public PPEInventoryId PPEInventoryId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PPEInventoryStatus Status { get; private set; }

    // ---- PPEInventory-specific business properties --------------------------------

    /// <summary>PpeItemCode business attribute.</summary>
    public string PpeItemCode { get; private set; }

    /// <summary>StockQuantity business attribute.</summary>
    public int StockQuantity { get; private set; }

    /// <summary>IssuedQuantity business attribute.</summary>
    public int IssuedQuantity { get; private set; }

    /// <summary>ExpiresAtUtc business attribute.</summary>
    public UtcTimestamp? ExpiresAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPPEInventoryDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPPEInventoryDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPPEInventoryDomainEvent>(_domainEvents);

    // For rehydration only.
    private PPEInventory() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new PPEInventory aggregate and raises <see cref="PPEInventoryCreatedEvent"/>.</summary>
    public static PPEInventory Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string ppeItemCode = "",
        int stockQuantity = 0,
        int issuedQuantity = 0,
        UtcTimestamp? expiresAtUtc = null)
    {
        var agg = new PPEInventory
        {
            PPEInventoryId     = PPEInventoryId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = PPEInventoryStatus.InStock,
            PpeItemCode = (ppeItemCode ?? string.Empty).Trim(),
            StockQuantity = stockQuantity,
            IssuedQuantity = issuedQuantity,
            ExpiresAtUtc = expiresAtUtc,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new PPEInventoryCreatedEvent(
            EventId:       new PPEInventoryEventId(Uuid7.New()),
            PPEInventoryId:      agg.PPEInventoryId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    /// <summary>Reorder threshold: available balance at or below this value triggers <see cref="PPEInventoryStatus.LowStock"/> ("Automated reorder alerts").</summary>
    public int ReorderThreshold { get; private set; } = 0;

    /// <summary>Available balance = StockQuantity - IssuedQuantity.</summary>
    public int AvailableQuantity => StockQuantity - IssuedQuantity;

    // ---- Lifecycle: Inventory Tracking ---------------------------------------

    /// <summary>Receives new stock into the item's on-hand balance. Valid from any non-terminal status.</summary>
    public void ReceiveStock(int quantity, UtcTimestamp receivedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PPEInventoryStatus.InStock, PPEInventoryStatus.LowStock);
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Quantity received must be positive.");

        StockQuantity += quantity;
        if (Status == PPEInventoryStatus.LowStock && AvailableQuantity > ReorderThreshold)
            Status = PPEInventoryStatus.InStock;

        AddDomainEvent(new PPEInventoryStockReceivedEvent(
            EventId: new PPEInventoryEventId(Uuid7.New()),
            PPEInventoryId: PPEInventoryId,
            QuantityReceived: quantity,
            NewStockQuantity: StockQuantity,
            ReceivedAtUtc: receivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Issues units out of on-hand stock ("Issuance and return logs"). Automatically transitions
    /// to <see cref="PPEInventoryStatus.LowStock"/> when the remaining available balance falls to
    /// or below <paramref name="reorderThreshold"/> ("Automated reorder alerts").
    /// </summary>
    public void IssueUnits(int quantity, int reorderThreshold, UtcTimestamp issuedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PPEInventoryStatus.InStock, PPEInventoryStatus.LowStock);
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Quantity issued must be positive.");
        if (quantity > AvailableQuantity)
            throw new InvalidOperationException(
                $"Cannot issue {quantity} units of PPEInventory {PPEInventoryId}; only {AvailableQuantity} are available.");

        ReorderThreshold = reorderThreshold;
        IssuedQuantity += quantity;

        var lowStockTriggered = AvailableQuantity <= reorderThreshold;
        Status = lowStockTriggered ? PPEInventoryStatus.LowStock : PPEInventoryStatus.InStock;

        AddDomainEvent(new PPEInventoryUnitsIssuedEvent(
            EventId: new PPEInventoryEventId(Uuid7.New()),
            PPEInventoryId: PPEInventoryId,
            QuantityIssued: quantity,
            RemainingAvailable: AvailableQuantity,
            LowStockTriggered: lowStockTriggered,
            IssuedAtUtc: issuedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Replacement & Retirement ---------------------------------

    /// <summary>Flags the item for replacement due to inspection failure, damage, or approaching expiration.</summary>
    public void FlagForReplacement(string reason, UtcTimestamp flaggedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PPEInventoryStatus.InStock, PPEInventoryStatus.LowStock);
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A replacement reason must be supplied.", nameof(reason));

        Status = PPEInventoryStatus.PendingReplacement;

        AddDomainEvent(new PPEInventoryFlaggedForReplacementEvent(
            EventId: new PPEInventoryEventId(Uuid7.New()),
            PPEInventoryId: PPEInventoryId,
            Reason: reason.Trim(),
            FlaggedAtUtc: flaggedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Retires the item from active service. Valid only from <see cref="PPEInventoryStatus.PendingReplacement"/>.</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PPEInventoryStatus.PendingReplacement);

        Status = PPEInventoryStatus.Retired;

        AddDomainEvent(new PPEInventoryRetiredEvent(
            EventId: new PPEInventoryEventId(Uuid7.New()),
            PPEInventoryId: PPEInventoryId,
            RetiredAtUtc: retiredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPPEInventoryDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PPEInventoryStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for PPEInventory {PPEInventoryId}.");
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, params PPEInventoryStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new PPEInventoryInvalidStatusTransitionException(
            PPEInventoryId, Status.ToString(), string.Join("|", allowed.Select(a => a.ToString())),
            correlationId, causationId);
    }
}