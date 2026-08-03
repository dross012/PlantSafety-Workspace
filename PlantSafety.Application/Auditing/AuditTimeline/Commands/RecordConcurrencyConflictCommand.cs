// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Command to durably capture an optimistic concurrency conflict rather than
//   silently dropping it as a transient exception (Handbook Phase 5 §6.1). Infrastructure
//   repositories that catch a ConcurrencyConflictException on an aggregate UpdateAsync path
//   should also dispatch this command (or call IConcurrencyConflictRepository directly) so the
//   conflict appears in AuditTimeline reconstruction.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.AuditTimeline.Commands;

/// <summary>
/// Captures a single optimistic concurrency conflict for audit purposes.
/// </summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="EntityType">Type discriminator of the entity the conflicting write targeted.</param>
/// <param name="EntityId">Identity of the entity the conflicting write targeted.</param>
/// <param name="ExpectedETag">ETag/concurrency token the caller supplied.</param>
/// <param name="ActualETag">ETag/concurrency token actually present in storage at conflict time, if known.</param>
/// <param name="AttemptedOperation">Operation that was attempted (e.g. "UpdateAsync").</param>
/// <param name="ActorId">Identity of the actor who attempted the write.</param>
public sealed record RecordConcurrencyConflictCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    string EntityType,
    Guid EntityId,
    string ExpectedETag,
    string? ActualETag,
    string AttemptedOperation,
    Guid ActorId) : ICommand<Result<Guid>>
{
    Guid? ICommand<Result<Guid>>.CausationId => CausationId;

    /// <summary>Builds a command from raw values, generating envelope identifiers where not supplied.</summary>
    public static RecordConcurrencyConflictCommand Create(
        Guid tenantId, string entityType, Guid entityId, string expectedETag, string? actualETag,
        string attemptedOperation, Guid actorId, Guid? correlationId = null, Guid? causationId = null, string apiVersionToken = "v1")
        => new(
            Guid.NewGuid(), correlationId ?? Guid.NewGuid(), causationId ?? Guid.NewGuid(),
            DateTime.UtcNow, apiVersionToken, tenantId, (entityType ?? string.Empty).Trim(), entityId,
            (expectedETag ?? string.Empty).Trim(), actualETag, (attemptedOperation ?? string.Empty).Trim(), actorId);
}
