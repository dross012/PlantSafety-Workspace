// ============================================================================
// File: CertificationV1Controller.cs
// Project: PlantSafety.Api
// Module: Compliance.Certification
// Created: 2026-08-02
// Description: V1 controller exposing the Application-layer Compliance.Certification module's
//              Commands and Queries over HTTP. Covers the full personnel-certification lifecycle:
//              Create -> Verify -> Renew/Expire -> Close -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Permits.Controllers.V1.PermitsV1Controller.
//
// Route note: this controller's route ("api/v{version}/compliance/certification") does not
// collide with the pre-existing read-only category controllers under
// PlantSafety.Api/Compliance/Certification/<Category>/Controllers (Contractor/DOT/EPA/
// IndustrySpecific/ISO14001/ISO45001/OSHACertifications/Training) -- those are routed via their
// own nested segments, confirmed by grep before finalizing this route.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields are populated via the shared RequestEnvelopeFactory. ActorId always comes
//   from the authenticated caller (ICurrentUserService), never from client-supplied input.
// - All mutating actions use SafetyTierPolicies.Write; reads use SafetyTierPolicies.Read.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Compliance.Certification.Contracts.Requests;
using PlantSafety.Application.Compliance.Certification.Commands;
using PlantSafety.Application.Compliance.Certification.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Compliance.Certification.Controllers.V1;

/// <summary>V1 controller for the Compliance.Certification module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/certification")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Compliance.Certification")]
public sealed class CertificationV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="CertificationV1Controller"/>.</summary>
    public CertificationV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Submits a new Certification record.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateCertificationResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateCertificationResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateCertificationRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateCertificationCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            HolderWorkerId: request.HolderWorkerId,
            CertificationName: request.CertificationName,
            CertificationBody: request.CertificationBody,
            CertificationNumber: request.CertificationNumber,
            IssuedAtUtc: request.IssuedAtUtc,
            ExpiresAtUtc: request.ExpiresAtUtc,
            RenewalRequired: request.RenewalRequired,
            RequesterUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Verifies and activates a Certification record (Created -&gt; Active).</summary>
    [HttpPost("{certificationId:guid}/verify")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CertificationStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CertificationStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> VerifyAsync(
        Guid certificationId,
        [FromBody] VerifyCertificationRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new VerifyCertificationCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            CertificationId: certificationId,
            VerifiedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Renews an active Certification record, extending its expiry (status unchanged).</summary>
    [HttpPost("{certificationId:guid}/renew")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CertificationStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CertificationStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RenewAsync(
        Guid certificationId,
        [FromBody] RenewCertificationRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RenewCertificationCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            CertificationId: certificationId,
            RenewedByUserId: env.ActorId,
            NewExpiresAtUtc: request.NewExpiresAtUtc)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Expires an active Certification record that passed its validity window (Active -&gt; Completed).</summary>
    [HttpPost("{certificationId:guid}/expire")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CertificationStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CertificationStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExpireAsync(
        Guid certificationId,
        [FromBody] ExpireCertificationRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ExpireCertificationCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            CertificationId: certificationId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Formally closes a completed/expired Certification record (Completed -&gt; Closed).</summary>
    [HttpPost("{certificationId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CertificationStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CertificationStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid certificationId,
        [FromBody] CloseCertificationRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseCertificationCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            CertificationId: certificationId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed Certification record for compliance retention (Closed -&gt; Archived).</summary>
    [HttpPost("{certificationId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CertificationStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CertificationStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid certificationId,
        [FromBody] ArchiveCertificationRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveCertificationCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            CertificationId: certificationId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single Certification read-model record by identity, including its current ETag.</summary>
    [HttpGet("{certificationId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Compliance.Certification.Models.CertificationItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid certificationId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetCertificationByIdQuery(env.TenantId, certificationId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
