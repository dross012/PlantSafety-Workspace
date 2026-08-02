// ============================================================================
// File: DataLakeActivityV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code refactoring-agent)
// Created: 2026-08-02
// Description: V1 MVC controller exposing the Phase 4 full build-out's Create/lifecycle/by-id
//              command and query endpoints for the newly built DataLakeActivity aggregate.
//              Distinct from the pre-existing
//              PlantSafety.Api.Intelligence.DataLake.Controllers.V1.DataLakeV1Controller (the
//              untouched date-range read-model controller) -- different controller class name,
//              different route segment ("activities" vs the read controller's own route), no path
//              collision.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/intelligence/data-lake/activities/...
//
// Rationale:
// - Routes all reads/writes through MediatR ISender to the Application layer. No domain logic or
//   data access here.
//
// ============================================================================

using PlantSafety.Api.Intelligence.DataLake.Contracts.Requests;
using PlantSafety.Api.Intelligence.DataLake.Authorization;
using PlantSafety.Application.Intelligence.DataLake.Commands;
using PlantSafety.Application.Intelligence.DataLake.Queries;

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;

namespace PlantSafety.Api.Intelligence.DataLake.Controllers.V1;

/// <summary>
/// V1 controller for the Phase 4 full build-out's Create/lifecycle/by-id command and query
/// endpoints for the DataLakeActivity aggregate.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/intelligence/data-lake/activities")]
[Authorize(Policy = DataLakeAuthorizationPolicies.Read)]
[Tags("DataLakeActivity")]
public sealed class DataLakeActivityV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="DataLakeActivityV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public DataLakeActivityV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new Intelligence.DataLake activity record.</summary>
    [HttpPost]
    [Authorize(Policy = DataLakeAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DataLakeActivityStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DataLakeActivityStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateDataLakeActivityRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateDataLakeActivityCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            DatasetName: request.DatasetName,
            Zone: request.Zone,
            RecordCount: request.RecordCount,
            ProcessedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Begins processing of an ingested data lake activity (Ingested -&gt; Processing).</summary>
    [HttpPost("{dataLakeActivityId:guid}/start-processing")]
    [Authorize(Policy = DataLakeAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DataLakeActivityStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DataLakeActivityStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StartProcessingAsync(
        Guid dataLakeActivityId,
        [FromBody] StartProcessingDataLakeActivityRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new StartProcessingDataLakeActivityCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            DataLakeActivityId: dataLakeActivityId,
            StartedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Marks a data lake activity's data as curated (Processing -&gt; Curated).</summary>
    [HttpPost("{dataLakeActivityId:guid}/curate")]
    [Authorize(Policy = DataLakeAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DataLakeActivityStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DataLakeActivityStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CurateAsync(
        Guid dataLakeActivityId,
        [FromBody] CurateDataLakeActivityRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CurateDataLakeActivityCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            DataLakeActivityId: dataLakeActivityId,
            PerformedByUserId: env.ActorId,
            FinalRecordCount: request?.FinalRecordCount)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Marks a data lake activity as failed (Processing -&gt; Failed).</summary>
    [HttpPost("{dataLakeActivityId:guid}/fail")]
    [Authorize(Policy = DataLakeAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DataLakeActivityStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DataLakeActivityStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> FailAsync(
        Guid dataLakeActivityId,
        [FromBody] FailDataLakeActivityRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new FailDataLakeActivityCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            DataLakeActivityId: dataLakeActivityId,
            PerformedByUserId: env.ActorId,
            FailureReason: request.FailureReason)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a curated or failed data lake activity for compliance retention (Curated/Failed -&gt; Archived).</summary>
    [HttpPost("{dataLakeActivityId:guid}/archive")]
    [Authorize(Policy = DataLakeAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DataLakeActivityStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DataLakeActivityStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid dataLakeActivityId,
        [FromBody] ArchiveDataLakeActivityRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveDataLakeActivityCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            DataLakeActivityId: dataLakeActivityId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single DataLakeActivity read-model record by identity, including its current ETag.</summary>
    [HttpGet("{dataLakeActivityId:guid}")]
    [Authorize(Policy = DataLakeAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Intelligence.DataLake.Models.DataLakeActivityDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid dataLakeActivityId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetDataLakeActivityByIdQuery(env.TenantId, dataLakeActivityId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
