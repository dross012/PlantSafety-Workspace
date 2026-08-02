// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Application
//  File:           CreateCertificationCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to submit a new Certification record under the Compliance.Certification
//                  bounded context. Carries the full command envelope per Handbook Rule 2.1.
//                  Mirrors PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Certification.Commands;

/// <summary>Submits a new Certification record.</summary>
public sealed record CreateCertificationCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    Guid HolderWorkerId,
    string CertificationName,
    string CertificationBody,
    string CertificationNumber,
    DateTime? IssuedAtUtc,
    DateTime? ExpiresAtUtc,
    bool RenewalRequired,
    Guid RequesterUserId) : ICommand<Result<CreateCertificationResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateCertificationResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateCertificationCommand"/> from raw, untrusted transport values.</summary>
    public static CreateCertificationCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        Guid holderWorkerId,
        string certificationName,
        string certificationBody,
        string certificationNumber,
        DateTime? issuedAtUtc,
        DateTime? expiresAtUtc,
        bool renewalRequired,
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
            HolderWorkerId: holderWorkerId,
            CertificationName: (certificationName ?? string.Empty).Trim(),
            CertificationBody: (certificationBody ?? string.Empty).Trim(),
            CertificationNumber: (certificationNumber ?? string.Empty).Trim(),
            IssuedAtUtc: issuedAtUtc,
            ExpiresAtUtc: expiresAtUtc,
            RenewalRequired: renewalRequired,
            RequesterUserId: requesterUserId);
}
