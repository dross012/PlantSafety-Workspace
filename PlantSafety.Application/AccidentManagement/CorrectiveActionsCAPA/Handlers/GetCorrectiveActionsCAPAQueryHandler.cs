// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetCorrectiveActionsCAPAQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.AccidentManagement.CorrectiveActionsCAPA.Queries;
using PlantSafety.Application.AccidentManagement.CorrectiveActionsCAPA.Repositories;

namespace PlantSafety.Application.AccidentManagement.CorrectiveActionsCAPA.Handlers;

/// <summary>Handles <see cref="GetCorrectiveActionsCAPAQuery"/>.</summary>
public sealed class GetCorrectiveActionsCAPAQueryHandler : IRequestHandler<GetCorrectiveActionsCAPAQuery, Result<CorrectiveActionsCAPAQueryResult>>
{
    private readonly ICorrectiveActionsCAPARepository _repository;
    private readonly ILogger<GetCorrectiveActionsCAPAQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCorrectiveActionsCAPAQueryHandler(ICorrectiveActionsCAPARepository repository, ILogger<GetCorrectiveActionsCAPAQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CorrectiveActionsCAPAQueryResult>> Handle(GetCorrectiveActionsCAPAQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetCorrectiveActionsCAPAAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<CorrectiveActionsCAPAQueryResult>.Success(new CorrectiveActionsCAPAQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve CorrectiveActionsCAPA for Tenant {TenantId}", request.TenantId);

            return Result<CorrectiveActionsCAPAQueryResult>.Failure(new ApplicationError(
                "AccidentManagement.CorrectiveActionsCAPA.RetrievalFailed",
                "Unable to retrieve CorrectiveActionsCAPA data at this time.",
                Guid.NewGuid()));
        }
    }
}