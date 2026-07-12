// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetCAPATriggersQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Auditing.CAPATriggers.Queries;
using PlantSafety.Application.Compliance.Auditing.CAPATriggers.Repositories;

namespace PlantSafety.Application.Compliance.Auditing.CAPATriggers.Handlers;

/// <summary>Handles <see cref="GetCAPATriggersQuery"/>.</summary>
public sealed class GetCAPATriggersQueryHandler : IRequestHandler<GetCAPATriggersQuery, Result<CAPATriggersQueryResult>>
{
    private readonly ICAPATriggersRepository _repository;
    private readonly ILogger<GetCAPATriggersQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCAPATriggersQueryHandler(ICAPATriggersRepository repository, ILogger<GetCAPATriggersQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CAPATriggersQueryResult>> Handle(GetCAPATriggersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetTriggersAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<CAPATriggersQueryResult>.Success(new CAPATriggersQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve CAPATriggers for Tenant {TenantId}", request.TenantId);

            return Result<CAPATriggersQueryResult>.Failure(new ApplicationError(
                "Compliance.Auditing.CAPATriggers.RetrievalFailed",
                "Unable to retrieve CAPA triggers at this time.",
                Guid.NewGuid()));
        }
    }
}
