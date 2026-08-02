// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Application
//  File:           CreateAiSafetyIntelligenceCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to register a new AiSafetyIntelligence record -- an AI-generated
//                  safety insight/prediction. Carries the full command envelope per Handbook Rule
//                  2.1. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AiSafetyIntelligence.Commands;

/// <summary>Registers a new AiSafetyIntelligence record.</summary>
public sealed record CreateAiSafetyIntelligenceCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string InsightType,
    string Description,
    decimal ConfidenceScore,
    string SourceModelName,
    Guid RequesterUserId) : ICommand<Result<CreateAiSafetyIntelligenceResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateAiSafetyIntelligenceResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateAiSafetyIntelligenceCommand"/> from raw, untrusted transport values.</summary>
    public static CreateAiSafetyIntelligenceCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string insightType,
        string description,
        decimal confidenceScore,
        string sourceModelName,
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
            InsightType: (insightType ?? "AnomalyDetection").Trim(),
            Description: (description ?? string.Empty).Trim(),
            ConfidenceScore: confidenceScore,
            SourceModelName: (sourceModelName ?? string.Empty).Trim(),
            RequesterUserId: requesterUserId);
}
