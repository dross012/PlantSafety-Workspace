// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Domain
//  File:           ProcessDefinition.cs
//  Created:        2026-07-15
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the ProcessManagement bounded context. Represents a
//                  documented operational/manufacturing process definition (its safety-critical
//                  parameters, revision, owner, and lifecycle status) tracked per tenant/site.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ProcessManagement.Aggregates;

/// <summary>
/// Aggregate root for the ProcessManagement bounded context. Tracks a single manufacturing or
/// operational process definition, its safety-critical category, workflow status, and owner.
/// </summary>
public sealed class ProcessDefinition
{
    /// <summary>Unique identity of this ProcessDefinition aggregate.</summary>
    public ProcessDefinitionId ProcessDefinitionId { get; private set; }

    /// <summary>Tenant that owns this process definition.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this process is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Human-readable process name (e.g., "Confined Space Entry — Tank Farm B").</summary>
    public string ProcessName { get; private set; } = string.Empty;

    /// <summary>Safety-critical process category (e.g., HazardousMaterials, ConfinedSpace, LOTO, HotWork).</summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>Lifecycle status: Draft, Active, UnderReview, Retired.</summary>
    public string Status { get; private set; } = "Draft";

    /// <summary>Monotonically increasing revision number, starting at 1.</summary>
    public int Version { get; private set; } = 1;

    /// <summary>Identity of the process owner (accountable role holder).</summary>
    public UserId OwnerId { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // Domain events
    private readonly List<IProcessDefinitionDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IProcessDefinitionDomainEvent> DomainEvents
        => new ReadOnlyCollection<IProcessDefinitionDomainEvent>(_domainEvents);

    private ProcessDefinition() { }

    /// <summary>Creates a new ProcessDefinition aggregate in Draft status and raises <see cref="ProcessDefinitionCreatedEvent"/>.</summary>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="siteId">Optional owning site.</param>
    /// <param name="processName">Human-readable process name. Must not be null or whitespace.</param>
    /// <param name="category">Safety-critical process category. Must not be null or whitespace.</param>
    /// <param name="ownerId">Accountable process owner.</param>
    /// <param name="createdBy">CRUD authorship identity.</param>
    /// <param name="createdAtUtc">Creation timestamp (UTC).</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="actor">Domain-action actor identity.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="processName"/> or <paramref name="category"/> is null or whitespace.</exception>
    public static ProcessDefinition Create(
        Guid tenantId, Guid? siteId, string processName, string category, UserId ownerId,
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor)
    {
        if (string.IsNullOrWhiteSpace(processName))
            throw new ArgumentException("ProcessName must not be null or whitespace.", nameof(processName));
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category must not be null or whitespace.", nameof(category));

        var agg = new ProcessDefinition
        {
            ProcessDefinitionId = ProcessDefinitionId.New(),
            TenantId            = tenantId,
            SiteId              = siteId,
            ProcessName         = processName.Trim(),
            Category            = category.Trim(),
            Status              = "Draft",
            Version             = 1,
            OwnerId             = ownerId,
            CreatedAtUtc        = createdAtUtc,
            CreatedBy           = createdBy,
        };

        agg._domainEvents.Add(new ProcessDefinitionCreatedEvent(
            EventId:             Uuid7.New(),
            ProcessDefinitionId: agg.ProcessDefinitionId.Value,
            TenantId:            tenantId,
            ProcessName:         agg.ProcessName,
            Category:            agg.Category,
            CreatedAtUtc:        createdAtUtc,
            Actor:               actor,
            CorrelationId:       correlationId,
            CausationId:         causationId));

        return agg;
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
