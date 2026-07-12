// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetOperatorsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.CrossDomain.Operators.Queries;
using PlantSafety.Application.CrossDomain.Operators.Repositories;

namespace PlantSafety.Application.CrossDomain.Operators.Handlers;

/// <summary>Handles <see cref="GetOperatorsQuery"/>.</summary>
public sealed class GetOperatorsQueryHandler : IRequestHandler<GetOperatorsQuery, Result<OperatorsQueryResult>>
{
    private readonly IOperatorsRepository _repository;
    private readonly ILogger<GetOperatorsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetOperatorsQueryHandler(IOperatorsRepository repository, ILogger<GetOperatorsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<OperatorsQueryResult>> Handle(GetOperatorsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetOperatorsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<OperatorsQueryResult>.Success(new OperatorsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Operators for Tenant {TenantId}", request.TenantId);

            return Result<OperatorsQueryResult>.Failure(new ApplicationError(
                "CrossDomain.Operators.RetrievalFailed",
                "Unable to retrieve operators data at this time.",
                Guid.NewGuid()));
        }
    }
}
