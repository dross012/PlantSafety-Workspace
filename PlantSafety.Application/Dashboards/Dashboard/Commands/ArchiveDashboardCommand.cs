// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to archive a Dashboard.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Dashboards.Dashboard.Commands;

/// <summary>Archives a Dashboard, removing it from navigation.</summary>
/// <param name="CorrelationId">Correlation lineage identifier for this operation group.</param>
/// <param name="CausationId">Identifier of the immediate parent command/event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="DashboardId">Identifier of the dashboard to archive.</param>
/// <param name="ArchivedByUserId">Identity of the user archiving the dashboard.</param>
public sealed record ArchiveDashboardCommand(
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    Guid DashboardId,
    Guid ArchivedByUserId) : ICommand<Result<ArchiveDashboardResult>>
{
    /// <summary>Gets the unique identifier of this command instance.</summary>
    public Guid CommandId { get; init; } = Guid.NewGuid();
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";
    /// <summary>Placeholder: no tenant field exists on this command; defaults to <see cref="Guid.Empty"/> until tenant scoping is added.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;
    /// <summary>Maps to <see cref="ArchivedByUserId"/>.</summary>
    public Guid ActorId => ArchivedByUserId;
}

/// <summary>Result of <see cref="ArchiveDashboardCommand"/>.</summary>
public sealed record ArchiveDashboardResult(Guid DashboardId, string Status);
