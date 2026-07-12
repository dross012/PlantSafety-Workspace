// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetLineBreakQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Permits.LineBreak.Queries;
using PlantSafety.Application.Compliance.Permits.LineBreak.Repositories;

namespace PlantSafety.Application.Compliance.Permits.LineBreak.Handlers;

/// <summary>Handles <see cref="GetLineBreakQuery"/>.</summary>
public sealed class GetLineBreakQueryHandler : IRequestHandler<GetLineBreakQuery, Result<LineBreakQueryResult>>
{
    private readonly ILineBreakRepository _repository;
    private readonly ILogger<GetLineBreakQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetLineBreakQueryHandler(ILineBreakRepository repository, ILogger<GetLineBreakQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LineBreakQueryResult>> Handle(GetLineBreakQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetLineBreakAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<LineBreakQueryResult>.Success(new LineBreakQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve LineBreak for Tenant {TenantId}", request.TenantId);

            return Result<LineBreakQueryResult>.Failure(new ApplicationError(
                "Compliance.Permits.LineBreak.RetrievalFailed",
                "Unable to retrieve line break permit data at this time.",
                Guid.NewGuid()));
        }
    }
}
