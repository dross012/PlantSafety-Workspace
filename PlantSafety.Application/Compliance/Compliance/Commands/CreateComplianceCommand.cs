// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Application
//  File:           CreateComplianceCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to submit a new (self-named) Compliance evaluation record under the
//                  Compliance.Compliance bounded context. Carries the full command envelope per
//                  Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand. This
//                  record itself is named "CreateComplianceCommand" (a suffixed usage, not a bare
//                  reference to the aggregate type), so no alias/qualification is required here --
//                  see CreateComplianceCommandHandler for where the aggregate type itself is
//                  referenced and therefore fully qualified.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Compliance.Commands;

/// <summary>Submits a new Compliance evaluation record.</summary>
public sealed record CreateComplianceCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string RegulationReference,
    string RuleDescription,
    decimal ComplianceScore,
    int NonConformanceCount,
    Guid ResponsiblePartyId,
    Guid RequesterUserId) : ICommand<Result<CreateComplianceResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateComplianceResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateComplianceCommand"/> from raw, untrusted transport values.</summary>
    public static CreateComplianceCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string regulationReference,
        string ruleDescription,
        decimal complianceScore,
        int nonConformanceCount,
        Guid responsiblePartyId,
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
            RegulationReference: (regulationReference ?? string.Empty).Trim(),
            RuleDescription: (ruleDescription ?? string.Empty).Trim(),
            ComplianceScore: complianceScore,
            NonConformanceCount: nonConformanceCount,
            ResponsiblePartyId: responsiblePartyId,
            RequesterUserId: requesterUserId);
}
