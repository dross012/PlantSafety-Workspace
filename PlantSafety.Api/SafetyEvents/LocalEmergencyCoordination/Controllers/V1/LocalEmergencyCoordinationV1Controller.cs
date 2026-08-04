// ============================================================================
// File: LocalEmergencyCoordinationV1Controller.cs
// Project: PlantSafety.Api
// Module: SafetyEvents.LocalEmergencyCoordination
// Created: 2026-08-04
// Description: V1 MVC controller exposing GET/POST endpoints for the LocalEmergencyCoordination
//              module -- structured coordination workflows for external local emergency
//              responders (fire, police, EMS, mutual-aid), sibling to the EmergencyManagement and
//              CrisisManagement V1 controllers. Routes follow
//              /api/v1/safety-events/local-emergency-coordination/...
// ============================================================================
//
// Rationale:
// - Routes all reads/writes through MediatR ISender to the Application layer. No domain logic or
//   data access here.
//
// Future Extension Seams:
// - A future PublicSafetyCommunication module can subscribe to
//   ILocalEmergencyCoordinationDomainEvent via the outbox without any change to this controller.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Common;
using PlantSafety.Api.SafetyEvents.LocalEmergencyCoordination.Contracts.Requests;
using PlantSafety.Api.SafetyEvents.LocalEmergencyCoordination.Contracts.Responses;
using PlantSafety.Api.SafetyEvents.LocalEmergencyCoordination.Authorization;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Queries;

namespace PlantSafety.Api.SafetyEvents.LocalEmergencyCoordination.Controllers.V1;

/// <summary>V1 controller for LocalEmergencyCoordination read and write endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/safety-events/local-emergency-coordination")]
[Authorize(Policy = LocalEmergencyCoordinationAuthorizationPolicies.Read)]
[Tags("LocalEmergencyCoordination")]
public sealed class LocalEmergencyCoordinationV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="LocalEmergencyCoordinationV1Controller"/>.</summary>
    public LocalEmergencyCoordinationV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Opens a new LocalEmergencyCoordination case.</summary>
    [HttpPost]
    [Authorize(Policy = LocalEmergencyCoordinationAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<OpenLocalEmergencyCoordinationCaseResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OpenLocalEmergencyCoordinationCaseResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> OpenCaseAsync([FromBody] OpenLocalEmergencyCoordinationCaseRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new OpenLocalEmergencyCoordinationCaseCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            IncidentId: request.IncidentId,
            CaseName: request.CaseName,
            OpenedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Establishes a mutual aid agreement with an external local emergency responder agency.</summary>
    [HttpPost("{localEmergencyCoordinationId:guid}/mutual-aid-agreements")]
    [Authorize(Policy = LocalEmergencyCoordinationAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EstablishMutualAidAgreementResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EstablishMutualAidAgreementAsync(
        Guid localEmergencyCoordinationId,
        [FromBody] EstablishMutualAidAgreementRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new EstablishMutualAidAgreementCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            LocalEmergencyCoordinationId: localEmergencyCoordinationId,
            AgencyName: request.AgencyName,
            Discipline: request.Discipline,
            DocumentVersion: request.DocumentVersion,
            EffectiveFromUtc: request.EffectiveFromUtc,
            RenewalDueAtUtc: request.RenewalDueAtUtc,
            EstablishedByUserId: env.ActorId)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Activates a joint response plan coordinated with external local emergency responders.</summary>
    [HttpPost("{localEmergencyCoordinationId:guid}/joint-response-plans")]
    [Authorize(Policy = LocalEmergencyCoordinationAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<JointResponsePlanStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateJointResponsePlanAsync(
        Guid localEmergencyCoordinationId,
        [FromBody] ActivateJointResponsePlanRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ActivateJointResponsePlanCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            LocalEmergencyCoordinationId: localEmergencyCoordinationId,
            IncidentId: request.IncidentId,
            PlanName: request.PlanName,
            ActivatedByUserId: env.ActorId)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Deactivates a previously activated joint response plan.</summary>
    [HttpPost("{localEmergencyCoordinationId:guid}/joint-response-plans/{jointResponsePlanId:guid}/deactivate")]
    [Authorize(Policy = LocalEmergencyCoordinationAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<JointResponsePlanStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeactivateJointResponsePlanAsync(
        Guid localEmergencyCoordinationId,
        Guid jointResponsePlanId,
        [FromBody] DeactivateJointResponsePlanRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new DeactivateJointResponsePlanCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            LocalEmergencyCoordinationId: localEmergencyCoordinationId,
            JointResponsePlanId: jointResponsePlanId,
            DeactivatedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Assigns an internal user as the live-incident liaison to a specific external responder agency.</summary>
    [HttpPost("{localEmergencyCoordinationId:guid}/liaisons")]
    [Authorize(Policy = LocalEmergencyCoordinationAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AssignExternalResponderLiaisonResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignExternalResponderLiaisonAsync(
        Guid localEmergencyCoordinationId,
        [FromBody] AssignExternalResponderLiaisonRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AssignExternalResponderLiaisonCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            LocalEmergencyCoordinationId: localEmergencyCoordinationId,
            IncidentId: request.IncidentId,
            AgencyName: request.AgencyName,
            LiaisonUserId: request.LiaisonUserId,
            AssignedByUserId: env.ActorId)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Logs a liaison communication exchange with an external responder agency during a live incident.</summary>
    [HttpPost("{localEmergencyCoordinationId:guid}/liaisons/{externalResponderLiaisonId:guid}/log")]
    [Authorize(Policy = LocalEmergencyCoordinationAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<LogLiaisonCommunicationResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> LogLiaisonCommunicationAsync(
        Guid localEmergencyCoordinationId,
        Guid externalResponderLiaisonId,
        [FromBody] LogLiaisonCommunicationRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new LogLiaisonCommunicationCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            LocalEmergencyCoordinationId: localEmergencyCoordinationId,
            ExternalResponderLiaisonId: externalResponderLiaisonId,
            Message: request.Message,
            LoggedByUserId: env.ActorId)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a post-incident debrief conducted jointly with external local emergency responder agencies.</summary>
    [HttpPost("{localEmergencyCoordinationId:guid}/debriefs")]
    [Authorize(Policy = LocalEmergencyCoordinationAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RecordJointDebriefResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordJointDebriefAsync(
        Guid localEmergencyCoordinationId,
        [FromBody] RecordJointDebriefRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RecordJointDebriefCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            LocalEmergencyCoordinationId: localEmergencyCoordinationId,
            IncidentId: request.IncidentId,
            AgenciesInvolved: request.AgenciesInvolved,
            Summary: request.Summary,
            RecordedByUserId: env.ActorId)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Closes a LocalEmergencyCoordination case.</summary>
    [HttpPost("{localEmergencyCoordinationId:guid}/close")]
    [Authorize(Policy = LocalEmergencyCoordinationAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<LocalEmergencyCoordinationStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid localEmergencyCoordinationId,
        [FromBody] CloseLocalEmergencyCoordinationCaseRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseLocalEmergencyCoordinationCaseCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            LocalEmergencyCoordinationId: localEmergencyCoordinationId,
            ClosedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed LocalEmergencyCoordination case for EPCRA/LEPC compliance retention.</summary>
    [HttpPost("{localEmergencyCoordinationId:guid}/archive")]
    [Authorize(Policy = LocalEmergencyCoordinationAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<LocalEmergencyCoordinationStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid localEmergencyCoordinationId,
        [FromBody] ArchiveLocalEmergencyCoordinationCaseRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveLocalEmergencyCoordinationCaseCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            LocalEmergencyCoordinationId: localEmergencyCoordinationId,
            ArchivedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single LocalEmergencyCoordination case by identity, including its current ETag.</summary>
    [HttpGet("{localEmergencyCoordinationId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models.LocalEmergencyCoordinationItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(Guid localEmergencyCoordinationId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetLocalEmergencyCoordinationByIdQuery(env.TenantId, localEmergencyCoordinationId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns mutual aid agreements for the resolved tenant -- backs the Mutual Aid Agreement Registry dashboard.</summary>
    [HttpGet("mutual-aid-agreements")]
    [ProducesResponseType(typeof(ApiResponse<ListMutualAidAgreementsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListMutualAidAgreementsAsync([FromQuery] ListMutualAidAgreementsRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var correlationId = env.CorrelationId.ToString();

        var query = new ListMutualAidAgreementsQuery(env.TenantId, request.SiteId);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<ListMutualAidAgreementsResponse>.Fail(result.Error.Message, correlationId));

        var response = new ListMutualAidAgreementsResponse(result.Value.TenantId, result.Value.SiteId, result.Value.GeneratedAt, result.Value.Items);

        return Ok(ApiResponse<ListMutualAidAgreementsResponse>.Ok(response, correlationId));
    }

    /// <summary>Returns joint response plans coordinated for the given incident -- backs the Active Joint Response Status dashboard.</summary>
    [HttpGet("incidents/{incidentId:guid}/joint-response-plans")]
    [ProducesResponseType(typeof(ApiResponse<GetJointResponsePlanByIncidentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetJointResponsePlanByIncidentAsync(Guid incidentId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var correlationId = env.CorrelationId.ToString();

        var query = new GetJointResponsePlanByIncidentQuery(env.TenantId, incidentId);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<GetJointResponsePlanByIncidentResponse>.Fail(result.Error.Message, correlationId));

        var response = new GetJointResponsePlanByIncidentResponse(result.Value.TenantId, result.Value.IncidentId, result.Value.GeneratedAt, result.Value.Items);

        return Ok(ApiResponse<GetJointResponsePlanByIncidentResponse>.Ok(response, correlationId));
    }

    /// <summary>Returns the liaison communication log for the given incident.</summary>
    [HttpGet("incidents/{incidentId:guid}/liaison-log")]
    [ProducesResponseType(typeof(ApiResponse<GetLiaisonLogByIncidentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLiaisonLogByIncidentAsync(Guid incidentId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var correlationId = env.CorrelationId.ToString();

        var query = new GetLiaisonLogByIncidentQuery(env.TenantId, incidentId);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<GetLiaisonLogByIncidentResponse>.Fail(result.Error.Message, correlationId));

        var response = new GetLiaisonLogByIncidentResponse(result.Value.TenantId, result.Value.IncidentId, result.Value.GeneratedAt, result.Value.Items);

        return Ok(ApiResponse<GetLiaisonLogByIncidentResponse>.Ok(response, correlationId));
    }

    /// <summary>Returns joint post-incident debrief records for the resolved tenant within a date range -- backs the Post-Incident Debrief Summary dashboard and the joint debrief completion rate KPI.</summary>
    [HttpGet("debriefs")]
    [ProducesResponseType(typeof(ApiResponse<ListJointDebriefsByPeriodResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListJointDebriefsByPeriodAsync([FromQuery] ListJointDebriefsByPeriodRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var correlationId = env.CorrelationId.ToString();

        var query = new ListJointDebriefsByPeriodQuery(env.TenantId, request.From, request.To ?? DateTime.UtcNow);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<ListJointDebriefsByPeriodResponse>.Fail(result.Error.Message, correlationId));

        var response = new ListJointDebriefsByPeriodResponse(result.Value.TenantId, result.Value.GeneratedAt, result.Value.Items);

        return Ok(ApiResponse<ListJointDebriefsByPeriodResponse>.Ok(response, correlationId));
    }
}
