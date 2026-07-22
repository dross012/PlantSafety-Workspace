// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to create a new (draft) Dashboard.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Dashboards.Dashboard.Commands;

/// <summary>Creates a new draft Dashboard.</summary>
/// <param name="CorrelationId">Correlation lineage identifier for this operation group.</param>
/// <param name="CausationId">Identifier of the immediate parent command/event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope.</param>
/// <param name="Name">Human-readable dashboard name.</param>
/// <param name="Description">Narrative description.</param>
/// <param name="CategoryCode">Catalog category code (one of the 19 named program dashboards).</param>
/// <param name="DefaultFilters">Default filters applied when the dashboard is opened (plant, line, department, shift).</param>
/// <param name="CreatedByUserId">Identity of the user creating this dashboard.</param>
public sealed record CreateDashboardCommand(
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    Guid TenantId,
    Guid? SiteId,
    string Name,
    string Description,
    string CategoryCode,
    IReadOnlyDictionary<string, string>? DefaultFilters,
    Guid CreatedByUserId) : ICommand<Result<CreateDashboardResult>>
{
    /// <summary>Gets the unique identifier of this command instance.</summary>
    public Guid CommandId { get; init; } = Guid.NewGuid();
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";
    /// <summary>Maps to <see cref="CreatedByUserId"/>.</summary>
    public Guid ActorId => CreatedByUserId;
}

/// <summary>Result of <see cref="CreateDashboardCommand"/>.</summary>
public sealed record CreateDashboardResult(Guid DashboardId, string Status);
