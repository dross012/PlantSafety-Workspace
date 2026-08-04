// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListUnauthorizedControlChangesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Models;
using PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Queries;
using PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Handlers;

/// <summary>Handles <see cref="ListUnauthorizedControlChangesQuery"/>.</summary>
public sealed class ListUnauthorizedControlChangesQueryHandler : IRequestHandler<ListUnauthorizedControlChangesQuery, Result<IReadOnlyList<ControlChangeEventDto>>>
{
    private readonly IControlChangeEventReadRepository _repository;
    private readonly ILogger<ListUnauthorizedControlChangesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListUnauthorizedControlChangesQueryHandler(IControlChangeEventReadRepository repository, ILogger<ListUnauthorizedControlChangesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<ControlChangeEventDto>>> Handle(ListUnauthorizedControlChangesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListUnauthorizedAsync(request.TenantId, cancellationToken);
            return Result<IReadOnlyList<ControlChangeEventDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list unauthorized ControlChangeEvents for Tenant {TenantId}", request.TenantId);

            return Result<IReadOnlyList<ControlChangeEventDto>>.Failure(new ApplicationError(
                "CyberPhysicalSecurity.ControlChangeEvent.ListFailed",
                "Unable to list unauthorized control-change events at this time.",
                Guid.NewGuid()));
        }
    }
}
