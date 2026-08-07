// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Application
//  File:           ClassifyMaterialCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Command to classify a candidate hazardous material under 49 CFR/PHMSA. Carries
//                  raw classification input (CAS number, waste profile code, SDS hazard statement,
//                  material name); the handler runs this through the Domain-layer
//                  ClassificationEngine and, on success, persists a new HazmatMaterial aggregate
//                  via HazmatMaterial.Create. Carries the full command envelope
//                  (CorrelationId/CausationId/CommandId/RequestedAtUtc/ApiVersionToken) per
//                  Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Classification.Commands;

/// <summary>Classifies a candidate hazardous material against the DOT/PHMSA classification engine.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="MaterialName">Human-readable candidate material name.</param>
/// <param name="CasNumber">CAS registry number for the candidate material, if known.</param>
/// <param name="WasteProfileCode">Waste profile code for the candidate material, if applicable.</param>
/// <param name="SdsHazardStatement">Safety Data Sheet hazard statement text, if available.</param>
/// <param name="ClassifiedByUserId">Identity of the user requesting the classification.</param>
public sealed record ClassifyMaterialCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string MaterialName,
    string? CasNumber,
    string? WasteProfileCode,
    string? SdsHazardStatement,
    Guid ClassifiedByUserId) : ICommand<Result<ClassifyMaterialResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => ClassifiedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<ClassifyMaterialResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="ClassifyMaterialCommand"/> from raw, untrusted transport values.</summary>
    public static ClassifyMaterialCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string materialName,
        string? casNumber,
        string? wasteProfileCode,
        string? sdsHazardStatement,
        Guid classifiedByUserId,
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
            MaterialName: (materialName ?? string.Empty).Trim(),
            CasNumber: string.IsNullOrWhiteSpace(casNumber) ? null : casNumber.Trim(),
            WasteProfileCode: string.IsNullOrWhiteSpace(wasteProfileCode) ? null : wasteProfileCode.Trim(),
            SdsHazardStatement: string.IsNullOrWhiteSpace(sdsHazardStatement) ? null : sdsHazardStatement.Trim(),
            ClassifiedByUserId: classifiedByUserId);
}
