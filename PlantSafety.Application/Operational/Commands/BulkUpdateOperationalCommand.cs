// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Command to apply a single operation uniformly across multiple Operational resources.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.Commands;

/// <summary>
/// Applies <paramref name="Operation"/> (e.g. "Complete", "Close") to every resource in
/// <paramref name="ResourceIds"/>. Uses partial-success semantics: individual item failures do not
/// fail the whole command, they are reported in <see cref="BulkUpdateOperationalResult.Results"/>.
/// </summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="ResourceIds">Identifiers of the Operational resources to operate on. Maximum 500 per request.</param>
/// <param name="Operation">The operation to apply (e.g. "Complete", "Close").</param>
/// <param name="Parameters">Optional operation-specific key-value data (e.g. ClosureSignedBy for Close).</param>
public sealed record BulkUpdateOperationalCommand(
    Guid TenantId,
    Guid? SiteId,
    IReadOnlyList<Guid> ResourceIds,
    string Operation,
    Dictionary<string, string>? Parameters) : IRequest<Result<BulkUpdateOperationalResult>>;
