// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetRegulatoryComplianceDocumentationQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.AccidentManagement.RegulatoryComplianceDocumentation.Queries;
using PlantSafety.Application.AccidentManagement.RegulatoryComplianceDocumentation.Repositories;

namespace PlantSafety.Application.AccidentManagement.RegulatoryComplianceDocumentation.Handlers;

/// <summary>Handles <see cref="GetRegulatoryComplianceDocumentationQuery"/>.</summary>
public sealed class GetRegulatoryComplianceDocumentationQueryHandler : IRequestHandler<GetRegulatoryComplianceDocumentationQuery, Result<RegulatoryComplianceDocumentationQueryResult>>
{
    private readonly IRegulatoryComplianceDocumentationRepository _repository;
    private readonly ILogger<GetRegulatoryComplianceDocumentationQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRegulatoryComplianceDocumentationQueryHandler(IRegulatoryComplianceDocumentationRepository repository, ILogger<GetRegulatoryComplianceDocumentationQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RegulatoryComplianceDocumentationQueryResult>> Handle(GetRegulatoryComplianceDocumentationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRegulatoryComplianceDocumentationAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<RegulatoryComplianceDocumentationQueryResult>.Success(new RegulatoryComplianceDocumentationQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve RegulatoryComplianceDocumentation for Tenant {TenantId}", request.TenantId);

            return Result<RegulatoryComplianceDocumentationQueryResult>.Failure(new ApplicationError(
                "AccidentManagement.RegulatoryComplianceDocumentation.RetrievalFailed",
                "Unable to retrieve RegulatoryComplianceDocumentation data at this time.",
                Guid.NewGuid()));
        }
    }
}