// ============================================================================
// File: BudgetsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: V1 controller exposing Financials/Budgets Commands and Queries
//              from the Application-layer Features/ CQRS tree over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Routes follow /api/v1/features/financials/budgets/...
// - Command/Query envelope fields are populated via RequestEnvelopeFactory,
//   which reads IOperationContext and ICurrentUserService for the current request.
// - Handlers in this tree return plain values (Guid / DTO / PaginatedResult<T>),
//   not Result<T>; unhandled failures propagate to the global exception middleware.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.Financials.Budgets.Contracts.Requests;
using PlantSafety.Api.Features.Financials.Budgets.Contracts.Responses;
using PlantSafety.Application.Features.Financials.Commands.Budgets.CreateBudget;
using PlantSafety.Application.Features.Financials.Models.Budgets;
using PlantSafety.Application.Features.Financials.Queries.Budgets.GetBudget;
using PlantSafety.Application.Features.Financials.Queries.Budgets.ListBudgets;

namespace PlantSafety.Api.Features.Financials.Budgets.Controllers.V1;

/// <summary>V1 controller for Financials/Budgets command and query endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features/financials/budgets")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Budgets")]
public sealed class BudgetsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="BudgetsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds Command/Query envelope fields from the current request context.</param>
    public BudgetsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new Safety Budget in Draft status.</summary>
    /// <param name="request">Budget fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the new budget identifier.</returns>
    /// <response code="200">Budget created successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateBudgetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateBudgetRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateBudgetCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            SiteId = request.SiteId,
            FiscalYear = request.FiscalYear,
            Category = request.Category,
            AllocatedAmount = request.AllocatedAmount,
            CurrencyCode = request.CurrencyCode,
        };

        var budgetId = await _sender.Send(command, cancellationToken);

        var response = new CreateBudgetResponse(budgetId);

        return Ok(ApiResponse<CreateBudgetResponse>.Ok(response, env.CorrelationId.ToString()));
    }

    /// <summary>Returns a single Safety Budget by identifier.</summary>
    /// <param name="budgetId">Budget identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the budget, or 404 when not found.</returns>
    /// <response code="200">Budget returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Budget not found.</response>
    [HttpGet("{budgetId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<BudgetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(
        Guid budgetId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetBudgetByIdQuery
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
            BudgetId = budgetId,
        };

        var budget = await _sender.Send(query, cancellationToken);

        if (budget is null)
            return NotFound(ApiResponse<BudgetResponse>.Fail("Budget not found.", env.CorrelationId.ToString()));

        return Ok(ApiResponse<BudgetResponse>.Ok(ToBudgetResponse(budget), env.CorrelationId.ToString()));
    }

    /// <summary>Returns a paginated list of Safety Budgets for the tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the paginated budget list.</returns>
    /// <response code="200">Budgets returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<BudgetListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync(
        [FromQuery] ListBudgetsRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new ListBudgetsQuery
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
            FiscalYear = request.FiscalYear,
            SiteId = request.SiteId,
            Category = request.Category,
            Status = request.Status,
        };

        var result = await _sender.Send(query, cancellationToken);

        var response = new BudgetListResponse(
            result.Items.Select(ToBudgetResponse).ToList(),
            result.TotalCount,
            result.PageNumber,
            result.PageSize,
            result.TotalPages,
            result.HasNextPage,
            result.HasPreviousPage);

        return Ok(ApiResponse<BudgetListResponse>.Ok(response, env.CorrelationId.ToString()));
    }

    private static BudgetResponse ToBudgetResponse(SafetyBudgetDto dto) => new(
        dto.BudgetId,
        dto.TenantId,
        dto.SiteId,
        dto.FiscalYear,
        dto.Category,
        dto.AllocatedAmount,
        dto.CurrencyCode,
        dto.ActualSpend,
        dto.RemainingBalance,
        dto.Status,
        dto.LastModifiedAtUtc,
        dto.ETag);
}
