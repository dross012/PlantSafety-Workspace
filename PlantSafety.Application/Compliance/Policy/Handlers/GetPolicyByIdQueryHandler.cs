// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Application
//  File:           GetPolicyByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetPolicyByIdQuery. Reads from the read-side repository (optimised
//                  stored proc; no domain-aggregate reconstruction). Returns a NotFound failure both
//                  when the row does not exist and when it belongs to a different tenant, so tenant
//                  membership is never revealed to an unauthorized caller. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Policy.Models;
using PlantSafety.Application.Compliance.Policy.Queries;
using PlantSafety.Application.Compliance.Policy.Repositories;

namespace PlantSafety.Application.Compliance.Policy.Handlers;

/// <summary>Handles <see cref="GetPolicyByIdQuery"/>.</summary>
public sealed class GetPolicyByIdQueryHandler : IRequestHandler<GetPolicyByIdQuery, Result<PolicyItemDto>>
{
    private readonly IPolicyRepository _repository;
    private readonly ILogger<GetPolicyByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPolicyByIdQueryHandler(IPolicyRepository repository, ILogger<GetPolicyByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PolicyItemDto>> Handle(GetPolicyByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.PolicyId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("Policy {PolicyId} not found for Tenant {TenantId}", request.PolicyId, request.TenantId);

                return Result<PolicyItemDto>.Failure(new ApplicationError(
                    "Compliance.Policy.NotFound",
                    "The requested policy could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<PolicyItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Policy {PolicyId}", request.PolicyId);

            return Result<PolicyItemDto>.Failure(new ApplicationError(
                "Compliance.Policy.RetrievalFailed",
                "Unable to retrieve policy data at this time.",
                Guid.NewGuid()));
        }
    }
}
