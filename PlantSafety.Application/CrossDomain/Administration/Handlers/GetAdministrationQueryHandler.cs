// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetAdministrationQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.CrossDomain.Administration.Queries;
using PlantSafety.Application.CrossDomain.Administration.Repositories;

namespace PlantSafety.Application.CrossDomain.Administration.Handlers;

/// <summary>Handles <see cref="GetAdministrationQuery"/>.</summary>
public sealed class GetAdministrationQueryHandler : IRequestHandler<GetAdministrationQuery, Result<AdministrationQueryResult>>
{
    private readonly IAdministrationRepository _repository;
    private readonly ILogger<GetAdministrationQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAdministrationQueryHandler(IAdministrationRepository repository, ILogger<GetAdministrationQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AdministrationQueryResult>> Handle(GetAdministrationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRecordsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<AdministrationQueryResult>.Success(new AdministrationQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Administration for Tenant {TenantId}", request.TenantId);

            return Result<AdministrationQueryResult>.Failure(new ApplicationError(
                "CrossDomain.Administration.RetrievalFailed",
                "Unable to retrieve administration data at this time.",
                Guid.NewGuid()));
        }
    }
}
