// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetPermitTemplateQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PermitToWork.PermitTemplate.Queries;
using PlantSafety.Application.PermitToWork.PermitTemplate.Repositories;

namespace PlantSafety.Application.PermitToWork.PermitTemplate.Handlers;

/// <summary>Handles <see cref="GetPermitTemplateQuery"/>.</summary>
public sealed class GetPermitTemplateQueryHandler : IRequestHandler<GetPermitTemplateQuery, Result<PermitTemplateQueryResult>>
{
    private readonly IPermitTemplateRepository _repository;
    private readonly ILogger<GetPermitTemplateQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPermitTemplateQueryHandler(IPermitTemplateRepository repository, ILogger<GetPermitTemplateQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PermitTemplateQueryResult>> Handle(GetPermitTemplateQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPermitTemplateAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PermitTemplateQueryResult>.Success(new PermitTemplateQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PermitTemplate for Tenant {TenantId}", request.TenantId);

            return Result<PermitTemplateQueryResult>.Failure(new ApplicationError(
                "PermitToWork.PermitTemplate.RetrievalFailed",
                "Unable to retrieve PermitTemplate data at this time.",
                Guid.NewGuid()));
        }
    }
}