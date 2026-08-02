// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Application
//  File:           CreatePolicyCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to submit a new Policy document. Carries the full command envelope
//                  (CorrelationId, CausationId, CommandId, RequestedAtUtc, ApiVersionToken) per
//                  Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Policy.Commands;

/// <summary>Submits a new Policy document.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="PolicyTitle">Human-readable policy title.</param>
/// <param name="Category">Policy category (e.g., PPE, HotWork, EmergencyResponse, GeneralSafety).</param>
/// <param name="AcknowledgmentRequired">Whether personnel acknowledgment is required for this policy.</param>
/// <param name="RequesterUserId">Identity of the user submitting/owning the policy.</param>
public sealed record CreatePolicyCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string PolicyTitle,
    string Category,
    bool AcknowledgmentRequired,
    Guid RequesterUserId) : ICommand<Result<CreatePolicyResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreatePolicyResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreatePolicyCommand"/> from raw, untrusted transport values.</summary>
    public static CreatePolicyCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string policyTitle,
        string category,
        bool acknowledgmentRequired,
        Guid requesterUserId,
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
            PolicyTitle: (policyTitle ?? string.Empty).Trim(),
            Category: (category ?? string.Empty).Trim(),
            AcknowledgmentRequired: acknowledgmentRequired,
            RequesterUserId: requesterUserId);
}
