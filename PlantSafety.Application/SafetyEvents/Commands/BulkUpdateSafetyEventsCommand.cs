// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Command to apply a single operation uniformly across multiple SafetyEvents resources.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.Commands;

/// <summary>
/// Applies <paramref name="Operation"/> (e.g. "Acknowledge", "Escalate", "Close") to every resource in
/// <paramref name="ResourceIds"/>. Uses partial-success semantics: individual item failures do not
/// fail the whole command, they are reported in <see cref="BulkUpdateSafetyEventsResult.Results"/>.
/// </summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="ResourceIds">Identifiers of the SafetyEvents resources to operate on. Maximum 500 per request.</param>
/// <param name="Operation">The operation to apply (e.g. "Acknowledge", "Escalate", "Close").</param>
/// <param name="Parameters">Optional operation-specific key-value data (e.g. ClosureReason for Close).</param>
public sealed record BulkUpdateSafetyEventsCommand(
    Guid TenantId,
    Guid? SiteId,
    IReadOnlyList<Guid> ResourceIds,
    string Operation,
    Dictionary<string, string>? Parameters) : IRequest<Result<BulkUpdateSafetyEventsResult>>;
