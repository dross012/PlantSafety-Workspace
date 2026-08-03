// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Command to run the Compliance Rules Engine (Safety, Regulatory, and/or
//   InternalPolicy pipelines) against a target entity, given caller-gathered facts. Carries the
//   full command envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc,
//   ApiVersionToken) per Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Compliance.RulesEngine.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.RulesEngine.Commands;

/// <summary>
/// Runs the Compliance Rules Engine against a target entity.
/// </summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="TriggeredBy">Identity of the accountable user who triggered the evaluation.</param>
/// <param name="TargetEntityType">Type discriminator of the entity being evaluated (e.g. "Risk", "Permit", "AuditFinding").</param>
/// <param name="TargetEntityId">Identity of the entity being evaluated.</param>
/// <param name="Categories">Optional subset of rule categories to run ("Safety"/"Regulatory"/"InternalPolicy"). Null runs all three.</param>
/// <param name="Facts">Caller-gathered, already-resolved data points the rule pipeline needs (see ComplianceRuleContext).</param>
public sealed record EvaluateComplianceCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    Guid TriggeredBy,
    string TargetEntityType,
    Guid TargetEntityId,
    IReadOnlyCollection<string>? Categories,
    IReadOnlyDictionary<string, string?> Facts) : ICommand<Result<ComplianceEvaluationDto>>
{
    /// <inheritdoc/>
    public Guid ActorId => TriggeredBy;
    Guid? ICommand<Result<ComplianceEvaluationDto>>.CausationId => CausationId;

    /// <summary>
    /// Builds an <see cref="EvaluateComplianceCommand"/> from raw, untrusted transport values,
    /// normalizing strings and generating envelope identifiers where the caller did not supply them.
    /// </summary>
    public static EvaluateComplianceCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        Guid triggeredBy,
        string targetEntityType,
        Guid targetEntityId,
        IReadOnlyCollection<string>? categories,
        IReadOnlyDictionary<string, string?> facts,
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
            TriggeredBy: triggeredBy,
            TargetEntityType: (targetEntityType ?? string.Empty).Trim(),
            TargetEntityId: targetEntityId,
            Categories: categories,
            Facts: facts ?? new Dictionary<string, string?>());
}
