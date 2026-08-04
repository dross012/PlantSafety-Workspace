// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           GenerateEsgDisclosureReportCommand.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command to generate a new (version-1) draft ESG disclosure report from a source
//                  AdvancedMetricsLedger. Backs the quarterly/annual scheduled report-generation
//                  hook. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Commands;

/// <summary>Generates a new draft ESG disclosure report from a source AdvancedMetricsLedger.</summary>
public sealed record GenerateEsgDisclosureReportCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    Guid SourceLedgerId,
    string Frameworks,
    string MethodologyVersion,
    Guid RequesterUserId) : ICommand<Result<GenerateEsgDisclosureReportResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<GenerateEsgDisclosureReportResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="GenerateEsgDisclosureReportCommand"/> from raw, untrusted transport values.</summary>
    public static GenerateEsgDisclosureReportCommand FromDto(
        Guid tenantId, Guid? siteId, Guid sourceLedgerId, string frameworks, string methodologyVersion,
        Guid requesterUserId, Guid? correlationId = null, Guid? causationId = null, string apiVersionToken = "v1")
        => new(
            CommandId: Guid.NewGuid(),
            CorrelationId: correlationId ?? Guid.NewGuid(),
            CausationId: causationId ?? Guid.NewGuid(),
            RequestedAtUtc: DateTime.UtcNow,
            ApiVersionToken: apiVersionToken,
            TenantId: tenantId,
            SiteId: siteId,
            SourceLedgerId: sourceLedgerId,
            Frameworks: frameworks,
            MethodologyVersion: (methodologyVersion ?? string.Empty).Trim(),
            RequesterUserId: requesterUserId);
}

/// <summary>Result of a successful <see cref="GenerateEsgDisclosureReportCommand"/> execution.</summary>
public sealed record GenerateEsgDisclosureReportResult(
    Guid EsgDisclosureReportId,
    int Version,
    string Status);
