// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetSafetyCultureBehavioralQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyPolicies.SafetyCultureBehavioral.Queries;
using PlantSafety.Application.SafetyPolicies.SafetyCultureBehavioral.Repositories;

namespace PlantSafety.Application.SafetyPolicies.SafetyCultureBehavioral.Handlers;

/// <summary>Handles <see cref="GetSafetyCultureBehavioralQuery"/>.</summary>
public sealed class GetSafetyCultureBehavioralQueryHandler : IRequestHandler<GetSafetyCultureBehavioralQuery, Result<SafetyCultureBehavioralQueryResult>>
{
    private readonly ISafetyCultureBehavioralRepository _repository;
    private readonly ILogger<GetSafetyCultureBehavioralQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSafetyCultureBehavioralQueryHandler(ISafetyCultureBehavioralRepository repository, ILogger<GetSafetyCultureBehavioralQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SafetyCultureBehavioralQueryResult>> Handle(GetSafetyCultureBehavioralQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetSafetyCultureBehavioralAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<SafetyCultureBehavioralQueryResult>.Success(new SafetyCultureBehavioralQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve SafetyCultureBehavioral for Tenant {TenantId}", request.TenantId);

            return Result<SafetyCultureBehavioralQueryResult>.Failure(new ApplicationError(
                "SafetyPolicies.SafetyCultureBehavioral.RetrievalFailed",
                "Unable to retrieve SafetyCultureBehavioral data at this time.",
                Guid.NewGuid()));
        }
    }
}