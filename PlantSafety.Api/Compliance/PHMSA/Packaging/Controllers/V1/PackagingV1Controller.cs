// ============================================================================
// File: PackagingV1Controller.cs
// Project: PlantSafety.Api
// Module: Compliance.PHMSA.Packaging
// Created: 2026-08-07
// Description: V1 controller exposing the Application-layer Compliance.PHMSA.Packaging module's
//              Commands and Queries over HTTP. Covers the packaging authorization lifecycle:
//              Authorize -> Supersede/Retire, plus stateless Validate/Suggest engine queries.
//              Mirrors PlantSafety.Api.Compliance.Permits.Controllers.V1.PermitsV1Controller.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields (CommandId/CorrelationId/TenantId/ActorId etc.) are populated via the shared
//   RequestEnvelopeFactory rather than hand-rolled per action. ActorId always comes from the
//   authenticated caller (ICurrentUserService), never from client-supplied input.
// - All mutating actions (including Validate, which carries the envelope for consistency even
//   though it has no persistence side-effect) use SafetyTierPolicies.Write; reads use
//   SafetyTierPolicies.Read.
// - GET "suggest" is registered before the "{packagingSpecificationId:guid}" route; the ":guid"
//   route constraint on the latter prevents any ambiguity regardless of declaration order.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Compliance.PHMSA.Packaging.Contracts.Requests;
using PlantSafety.Application.Compliance.PHMSA.Packaging.Commands;
using PlantSafety.Application.Compliance.PHMSA.Packaging.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Compliance.PHMSA.Packaging.Controllers.V1;

/// <summary>V1 controller for the Compliance.PHMSA.Packaging module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/phmsa/packaging")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Compliance.PHMSA.Packaging")]
public sealed class PackagingV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="PackagingV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public PackagingV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Authorizes a new DOT packaging specification.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AuthorizePackagingResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthorizePackagingResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AuthorizeAsync(
        [FromBody] AuthorizePackagingRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AuthorizePackagingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            DOTSpec: request.DOTSpec,
            PerformanceTests: request.PerformanceTests,
            MaxCapacity: request.MaxCapacity,
            CapacityUnit: request.CapacityUnit,
            ClosureInstructions: request.ClosureInstructions,
            ReuseRestrictions: request.ReuseRestrictions,
            AssociatedMaterialId: request.AssociatedMaterialId,
            AuthorizedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Marks a PackagingSpecification superseded by a newer authorization.</summary>
    [HttpPost("{packagingSpecificationId:guid}/supersede")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PackagingStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PackagingStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SupersedeAsync(
        Guid packagingSpecificationId,
        [FromBody] SupersedePackagingRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new SupersedePackagingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PackagingSpecificationId: packagingSpecificationId,
            SupersededBySpecificationId: request.SupersededBySpecificationId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires a PackagingSpecification record.</summary>
    [HttpPost("{packagingSpecificationId:guid}/retire")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PackagingStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PackagingStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RetireAsync(
        Guid packagingSpecificationId,
        [FromBody] RetirePackagingRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RetirePackagingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PackagingSpecificationId: packagingSpecificationId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Validates whether a proposed DOT packaging spec is authorized for a hazard class/packing group.</summary>
    [HttpPost("validate")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ValidatePackagingResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidatePackagingResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ValidateAsync(
        [FromBody] ValidatePackagingRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ValidatePackagingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ProposedDOTSpec: request.ProposedDOTSpec,
            HazardClass: request.HazardClass,
            HazardClassDivision: request.HazardClassDivision,
            PackingGroup: request.PackingGroup,
            RequestedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single PackagingSpecification read-model record by identity, including its current ETag.</summary>
    [HttpGet("{packagingSpecificationId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Compliance.PHMSA.Packaging.Models.PackagingSpecificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid packagingSpecificationId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPackagingSpecQuery(env.TenantId, packagingSpecificationId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Determines DOT-authorized packaging for a hazard class/packing group/quantity combination.</summary>
    [HttpGet("suggest")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<SuggestPackagingResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SuggestPackagingResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SuggestAsync(
        [FromQuery] SuggestPackagingRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new SuggestPackagingQuery(
            TenantId: env.TenantId,
            HazardClass: request.HazardClass,
            HazardClassDivision: request.HazardClassDivision,
            PackingGroup: request.PackingGroup,
            Quantity: request.Quantity,
            QuantityUnit: request.QuantityUnit,
            ContainerTypeRequested: request.ContainerTypeRequested);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns PackagingSpecification read-model data for a tenant/site, optionally filtered by DOT spec code.</summary>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ListPackagingSpecsResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ListPackagingSpecsResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync(
        [FromQuery] PackagingListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new ListPackagingSpecsQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            DOTSpec: request.DOTSpec);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
