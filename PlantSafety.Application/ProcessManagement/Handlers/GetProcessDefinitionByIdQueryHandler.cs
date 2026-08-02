// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Application
//  File:           GetProcessDefinitionByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetProcessDefinitionByIdQuery. Reads from the read-side
//                  repository. Returns a NotFound failure both when the row does not exist and
//                  when it belongs to a different tenant, so tenant membership is never revealed
//                  to an unauthorized caller. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Handlers.GetAuditingByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ProcessManagement.Models;
using PlantSafety.Application.ProcessManagement.Queries;
using PlantSafety.Application.ProcessManagement.Repositories;

namespace PlantSafety.Application.ProcessManagement.Handlers;

/// <summary>Handles <see cref="GetProcessDefinitionByIdQuery"/>.</summary>
public sealed class GetProcessDefinitionByIdQueryHandler : IRequestHandler<GetProcessDefinitionByIdQuery, Result<ProcessDefinitionItemDto>>
{
    private readonly IProcessDefinitionRepository _repository;
    private readonly ILogger<GetProcessDefinitionByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetProcessDefinitionByIdQueryHandler(IProcessDefinitionRepository repository, ILogger<GetProcessDefinitionByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ProcessDefinitionItemDto>> Handle(GetProcessDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.ProcessDefinitionId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("ProcessDefinition {ProcessDefinitionId} not found for Tenant {TenantId}", request.ProcessDefinitionId, request.TenantId);

                return Result<ProcessDefinitionItemDto>.Failure(new ApplicationError(
                    "ProcessManagement.ProcessDefinition.NotFound",
                    "The requested process definition could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<ProcessDefinitionItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ProcessDefinition {ProcessDefinitionId}", request.ProcessDefinitionId);

            return Result<ProcessDefinitionItemDto>.Failure(new ApplicationError(
                "ProcessManagement.ProcessDefinition.RetrievalFailed",
                "Unable to retrieve process definition data at this time.",
                Guid.NewGuid()));
        }
    }
}
