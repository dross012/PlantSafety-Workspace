// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to create a new SafetyPolicy record within the SafetyPolicies module. Carries the full command envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyPolicies.Commands;

/// <summary>
/// Creates a new SafetyPolicy record under the SafetyPolicies bounded context.
/// </summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="SafetyPolicyTitle">Human-readable title for the new safetypolicy record.</param>
/// <param name="Category">PolicyCategory code (see SafetyPolicies.Enums.PolicyCategory) governing this record.</param>
/// <param name="OwnerId">Identity of the accountable owner.</param>
/// <param name="EffectiveAtUtc">Optional UTC timestamp the record becomes effective.</param>
/// <param name="ReviewRequired">Whether formal review/acknowledgment is required.</param>
public sealed record CreateSafetyPolicyCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string SafetyPolicyTitle,
    string Category,
    Guid OwnerId,
    DateTime? EffectiveAtUtc,
    bool ReviewRequired) : ICommand<Result<CreateSafetyPolicyResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.
    /// Placeholder: no dedicated actor/created-by field exists on this command yet; defaults to <see cref="Guid.Empty"/>.</summary>
    public Guid ActorId { get; init; } = Guid.Empty;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateSafetyPolicyResult>>.CausationId => CausationId;

    /// <summary>
    /// Builds a <see cref="CreateSafetyPolicyCommand"/> from raw, untrusted transport values, normalizing
    /// strings and generating envelope identifiers where the caller did not supply them.
    /// </summary>
    public static CreateSafetyPolicyCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string safetyPolicyTitle,
        string category,
        Guid ownerId,
        DateTime? effectiveAtUtc,
        bool reviewRequired,
        Guid? correlationId = null,
        Guid? causationId = null,
        string apiVersionToken = "v1")
        => new(
            CommandId: Guid.NewGuid(),
            CorrelationId: correlationId ?? Guid.NewGuid(),
            CausationId: causationId ?? Guid.NewGuid(),
            RequestedAtUtc: DateTime.UtcNow,
            ApiVersionToken: apiVersionToken,
            TenantId: tenantId,
            SiteId: siteId,
            SafetyPolicyTitle: (safetyPolicyTitle ?? string.Empty).Trim(),
            Category: (category ?? string.Empty).Trim(),
            OwnerId: ownerId,
            EffectiveAtUtc: effectiveAtUtc,
            ReviewRequired: reviewRequired);
}