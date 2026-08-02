// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Application
//  File:           CreateProceduresCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to submit a new Procedures (SOP) document. Carries the full command
//                  envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc, ApiVersionToken)
//                  per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Procedures.Commands;

/// <summary>Submits a new Procedures document.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="ProcedureTitle">Human-readable procedure title (e.g., "Confined Space Entry SOP").</param>
/// <param name="ProcedureNumber">Procedure document number/reference.</param>
/// <param name="Category">Procedure category (e.g., LOTO, HotWork, ConfinedSpace, EmergencyResponse).</param>
/// <param name="RequesterUserId">Identity of the user submitting/owning the procedure.</param>
public sealed record CreateProceduresCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string ProcedureTitle,
    string ProcedureNumber,
    string Category,
    Guid RequesterUserId) : ICommand<Result<CreateProceduresResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateProceduresResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateProceduresCommand"/> from raw, untrusted transport values.</summary>
    public static CreateProceduresCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string procedureTitle,
        string procedureNumber,
        string category,
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
            ProcedureTitle: (procedureTitle ?? string.Empty).Trim(),
            ProcedureNumber: (procedureNumber ?? string.Empty).Trim(),
            Category: (category ?? string.Empty).Trim(),
            RequesterUserId: requesterUserId);
}
