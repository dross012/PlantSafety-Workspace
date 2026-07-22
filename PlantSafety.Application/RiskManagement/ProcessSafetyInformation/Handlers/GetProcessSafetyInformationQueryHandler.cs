// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetProcessSafetyInformationQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.RiskManagement.ProcessSafetyInformation.Queries;
using PlantSafety.Application.RiskManagement.ProcessSafetyInformation.Repositories;

namespace PlantSafety.Application.RiskManagement.ProcessSafetyInformation.Handlers;

/// <summary>Handles <see cref="GetProcessSafetyInformationQuery"/>.</summary>
public sealed class GetProcessSafetyInformationQueryHandler : IRequestHandler<GetProcessSafetyInformationQuery, Result<ProcessSafetyInformationQueryResult>>
{
    private readonly IProcessSafetyInformationRepository _repository;
    private readonly ILogger<GetProcessSafetyInformationQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetProcessSafetyInformationQueryHandler(IProcessSafetyInformationRepository repository, ILogger<GetProcessSafetyInformationQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ProcessSafetyInformationQueryResult>> Handle(GetProcessSafetyInformationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetProcessSafetyInformationAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ProcessSafetyInformationQueryResult>.Success(new ProcessSafetyInformationQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ProcessSafetyInformation for Tenant {TenantId}", request.TenantId);

            return Result<ProcessSafetyInformationQueryResult>.Failure(new ApplicationError(
                "RiskManagement.ProcessSafetyInformation.RetrievalFailed",
                "Unable to retrieve ProcessSafetyInformation data at this time.",
                Guid.NewGuid()));
        }
    }
}