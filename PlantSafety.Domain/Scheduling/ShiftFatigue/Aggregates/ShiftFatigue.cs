// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Domain
//  File:           ShiftFatigue.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the ShiftFatigue bounded context (module: Scheduling).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates;

/// <summary>Aggregate root for the ShiftFatigue bounded context.</summary>
public sealed partial class ShiftFatigue
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this ShiftFatigue aggregate.</summary>
    public ShiftFatigueId ShiftFatigueId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ShiftFatigueStatus Status { get; private set; }

    // ---- ShiftFatigue-specific business properties --------------------------------

    /// <summary>EmployeeId business attribute.</summary>
    public Guid EmployeeId { get; private set; }

    /// <summary>ShiftPattern business attribute.</summary>
    public string ShiftPattern { get; private set; }

    /// <summary>ConsecutiveNightShifts business attribute.</summary>
    public int ConsecutiveNightShifts { get; private set; }

    /// <summary>OvertimeHours business attribute.</summary>
    public decimal OvertimeHours { get; private set; }

    /// <summary>FatigueRiskScore business attribute.</summary>
    public decimal FatigueRiskScore { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IShiftFatigueDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IShiftFatigueDomainEvent> DomainEvents
        => new ReadOnlyCollection<IShiftFatigueDomainEvent>(_domainEvents);

    // For rehydration only.
    private ShiftFatigue() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new ShiftFatigue aggregate and raises <see cref="ShiftFatigueCreatedEvent"/>.</summary>
    public static ShiftFatigue Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid employeeId = default,
        string shiftPattern = "",
        int consecutiveNightShifts = 0,
        decimal overtimeHours = 0m,
        decimal fatigueRiskScore = 0m)
    {
        var agg = new ShiftFatigue
        {
            ShiftFatigueId     = ShiftFatigueId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = ShiftFatigueStatus.Created,
            EmployeeId = employeeId,
            ShiftPattern = (shiftPattern ?? string.Empty).Trim(),
            ConsecutiveNightShifts = consecutiveNightShifts,
            OvertimeHours = overtimeHours,
            FatigueRiskScore = fatigueRiskScore,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new ShiftFatigueCreatedEvent(
            EventId:       new ShiftFatigueEventId(Uuid7.New()),
            ShiftFatigueId:      agg.ShiftFatigueId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IShiftFatigueDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ShiftFatigueStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for ShiftFatigue {ShiftFatigueId}.");
    }
}