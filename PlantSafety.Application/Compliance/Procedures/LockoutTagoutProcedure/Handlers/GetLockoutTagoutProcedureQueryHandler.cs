// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetLockoutTagoutProcedureQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Procedures.LockoutTagoutProcedure.Queries;
using PlantSafety.Application.Compliance.Procedures.LockoutTagoutProcedure.Repositories;

namespace PlantSafety.Application.Compliance.Procedures.LockoutTagoutProcedure.Handlers;

/// <summary>Handles <see cref="GetLockoutTagoutProcedureQuery"/>.</summary>
public sealed class GetLockoutTagoutProcedureQueryHandler : IRequestHandler<GetLockoutTagoutProcedureQuery, Result<LockoutTagoutProcedureQueryResult>>
{
    private readonly ILockoutTagoutProcedureRepository _repository;
    private readonly ILogger<GetLockoutTagoutProcedureQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetLockoutTagoutProcedureQueryHandler(ILockoutTagoutProcedureRepository repository, ILogger<GetLockoutTagoutProcedureQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LockoutTagoutProcedureQueryResult>> Handle(GetLockoutTagoutProcedureQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetProceduresAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<LockoutTagoutProcedureQueryResult>.Success(new LockoutTagoutProcedureQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve LockoutTagoutProcedure for Tenant {TenantId}", request.TenantId);

            return Result<LockoutTagoutProcedureQueryResult>.Failure(new ApplicationError(
                "Compliance.Procedures.LockoutTagoutProcedure.RetrievalFailed",
                "Unable to retrieve LockoutTagoutProcedure data at this time.",
                Guid.NewGuid()));
        }
    }
}
