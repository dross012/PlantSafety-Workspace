// ============================================================================
// File: ComplianceV1Controller.cs
// Project: PlantSafety.Api
// Module: Compliance.Compliance
// Created: 2026-08-02
// Description: V1 controller exposing the Application-layer (self-named) Compliance.Compliance
//              module's Commands and Queries over HTTP. Covers the full regulatory-governance
//              evaluation lifecycle: Create -> Activate -> Evaluate -> Complete -> Close ->
//              Archive. Mirrors PlantSafety.Api.Compliance.Permits.Controllers.V1.PermitsV1Controller.
//
// Route note: this controller's route ("api/v{version}/compliance/compliance") does not collide
// with the pre-existing read-only category controllers under
// PlantSafety.Api/Compliance/Compliance/<Category>/Controllers (Obligations/
// RegulatoryRequirements/Tracking) -- those are routed via their own nested segments, confirmed
// by grep before finalizing this route. The controller class itself is named
// "ComplianceV1Controller" (matching the aggregate's own simple name), but every Application-layer
// type it references is imported via its fully-qualified namespace
// (PlantSafety.Application.Compliance.Compliance.*), never a bare "Compliance" type reference.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields are populated via the shared RequestEnvelopeFactory. ActorId always comes
//   from the authenticated caller (ICurrentUserService), never from client-supplied input.
// - All mutating actions use SafetyTierPolicies.Write; reads use SafetyTierPolicies.Read.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Compliance.Compliance.Contracts.Requests;
using PlantSafety.Application.Compliance.Compliance.Commands;
using PlantSafety.Application.Compliance.Compliance.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Compliance.Compliance.Controllers.V1;

/// <summary>V1 controller for the (self-named) Compliance.Compliance module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/compliance")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Compliance.Compliance")]
public sealed class ComplianceV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ComplianceV1Controller"/>.</summary>
    public ComplianceV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Submits a new Compliance evaluation record.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateComplianceResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateComplianceResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateComplianceRuleRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateComplianceCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            RegulationReference: request.RegulationReference,
            RuleDescription: request.RuleDescription,
            ComplianceScore: request.ComplianceScore,
            NonConformanceCount: request.NonConformanceCount,
            ResponsiblePartyId: request.ResponsiblePartyId,
            RequesterUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Activates a Compliance evaluation record (Created -&gt; Active).</summary>
    [HttpPost("{complianceId:guid}/activate")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ComplianceStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ComplianceStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateAsync(
        Guid complianceId,
        [FromBody] ActivateComplianceRuleRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ActivateComplianceCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ComplianceId: complianceId,
            ActivatedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Re-evaluates an active Compliance record, updating its score and non-conformance count (status unchanged).</summary>
    [HttpPost("{complianceId:guid}/evaluate")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ComplianceStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ComplianceStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EvaluateAsync(
        Guid complianceId,
        [FromBody] EvaluateComplianceRuleRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new EvaluateComplianceCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ComplianceId: complianceId,
            EvaluatedByUserId: env.ActorId,
            ComplianceScore: request.ComplianceScore,
            NonConformanceCount: request.NonConformanceCount)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Marks a Compliance evaluation record completed (Active -&gt; Completed).</summary>
    [HttpPost("{complianceId:guid}/complete")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ComplianceStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ComplianceStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CompleteAsync(
        Guid complianceId,
        [FromBody] CompleteComplianceRuleRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteComplianceCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ComplianceId: complianceId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Formally closes a completed Compliance record (Completed -&gt; Closed).</summary>
    [HttpPost("{complianceId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ComplianceStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ComplianceStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid complianceId,
        [FromBody] CloseComplianceRuleRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseComplianceCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ComplianceId: complianceId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed Compliance record for retention (Closed -&gt; Archived).</summary>
    [HttpPost("{complianceId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ComplianceStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ComplianceStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid complianceId,
        [FromBody] ArchiveComplianceRuleRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveComplianceCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ComplianceId: complianceId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single Compliance read-model record by identity, including its current ETag.</summary>
    [HttpGet("{complianceId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Compliance.Compliance.Models.ComplianceItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid complianceId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetComplianceByIdQuery(env.TenantId, complianceId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
