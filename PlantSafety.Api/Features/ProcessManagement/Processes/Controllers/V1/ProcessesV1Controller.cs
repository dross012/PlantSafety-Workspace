// ============================================================================
// File: ProcessesV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: V1 controller exposing ProcessManagement/Processes Commands and Queries
//              from the Application-layer Features/ CQRS tree over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Routes follow /api/v1/features/process-management/processes/...
// - Command/Query envelope fields are populated via RequestEnvelopeFactory,
//   which reads IOperationContext and ICurrentUserService for the current request.
// - Handlers in this tree return plain values (Guid / DTO / PaginatedResult<T>),
//   not Result<T>; unhandled failures propagate to the global exception middleware.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.ProcessManagement.Processes.Contracts.Requests;
using PlantSafety.Api.Features.ProcessManagement.Processes.Contracts.Responses;
using PlantSafety.Application.Features.ProcessManagement.Commands.Processes.CreateProcess;
using PlantSafety.Application.Features.ProcessManagement.Models.Processes;
using PlantSafety.Application.Features.ProcessManagement.Queries.Processes.GetProcess;
using PlantSafety.Application.Features.ProcessManagement.Queries.Processes.ListProcesses;

namespace PlantSafety.Api.Features.ProcessManagement.Processes.Controllers.V1;

/// <summary>V1 controller for ProcessManagement/Processes command and query endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features/process-management/processes")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("ProcessManagement")]
public sealed class ProcessesV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ProcessesV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds Command/Query envelope fields from the current request context.</param>
    public ProcessesV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new Process Definition in Draft status.</summary>
    /// <param name="request">Process fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the new process identifier.</returns>
    /// <response code="200">Process created successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateProcessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateProcessRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateProcessCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            SiteId = request.SiteId,
            ProcessName = request.ProcessName,
            Category = request.Category,
            OwnerId = request.OwnerId,
        };

        var processId = await _sender.Send(command, cancellationToken);

        var response = new CreateProcessResponse(processId);

        return Ok(ApiResponse<CreateProcessResponse>.Ok(response, env.CorrelationId.ToString()));
    }

    /// <summary>Returns a single Process Definition by identifier.</summary>
    /// <param name="processId">Process identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the process, or 404 when not found.</returns>
    /// <response code="200">Process returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Process not found.</response>
    [HttpGet("{processId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ProcessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(
        Guid processId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetProcessByIdQuery
        {
            TraceId = env.TraceId,
            SpanId = env.SpanId,
            ApiVersionToken = env.ApiVersionToken,
            RequestingUserId = env.RequestingUserId,
            RequestingUserRole = env.RequestingUserRole,
            TenantId = env.TenantId,
            CorrelationId = env.CorrelationId,
            CachePreference = env.CachePreference,
            SuppressUnauthorized = env.SuppressUnauthorized,
            StrictMode = env.StrictMode,
            ProcessId = processId,
        };

        var process = await _sender.Send(query, cancellationToken);

        if (process is null)
            return NotFound(ApiResponse<ProcessResponse>.Fail("Process not found.", env.CorrelationId.ToString()));

        return Ok(ApiResponse<ProcessResponse>.Ok(ToProcessResponse(process), env.CorrelationId.ToString()));
    }

    /// <summary>Returns a paginated list of Process Definitions for the tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the paginated process list.</returns>
    /// <response code="200">Processes returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ProcessListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync(
        [FromQuery] ListProcessesRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new ListProcessesQuery
        {
            TraceId = env.TraceId,
            SpanId = env.SpanId,
            ApiVersionToken = env.ApiVersionToken,
            RequestingUserId = env.RequestingUserId,
            RequestingUserRole = env.RequestingUserRole,
            TenantId = env.TenantId,
            CorrelationId = env.CorrelationId,
            CachePreference = env.CachePreference,
            SuppressUnauthorized = env.SuppressUnauthorized,
            StrictMode = env.StrictMode,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            OrderBy = request.OrderBy,
            SortDirection = request.SortDirection,
            SiteId = request.SiteId,
            Category = request.Category,
            Status = request.Status,
        };

        var result = await _sender.Send(query, cancellationToken);

        var response = new ProcessListResponse(
            result.Items.Select(ToProcessResponse).ToList(),
            result.TotalCount,
            result.PageNumber,
            result.PageSize,
            result.TotalPages,
            result.HasNextPage,
            result.HasPreviousPage);

        return Ok(ApiResponse<ProcessListResponse>.Ok(response, env.CorrelationId.ToString()));
    }

    private static ProcessResponse ToProcessResponse(ProcessDefinitionDto dto) => new(
        dto.ProcessId,
        dto.TenantId,
        dto.SiteId,
        dto.ProcessName,
        dto.Category,
        dto.Status,
        dto.Version,
        dto.OwnerId,
        dto.LastModifiedAtUtc,
        dto.ETag);
}
