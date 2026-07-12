// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetIndustrySpecificQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Certification.IndustrySpecific.Queries;
using PlantSafety.Application.Compliance.Certification.IndustrySpecific.Repositories;

namespace PlantSafety.Application.Compliance.Certification.IndustrySpecific.Handlers;

/// <summary>Handles <see cref="GetIndustrySpecificQuery"/>.</summary>
public sealed class GetIndustrySpecificQueryHandler : IRequestHandler<GetIndustrySpecificQuery, Result<IndustrySpecificQueryResult>>
{
    private readonly IIndustrySpecificRepository _repository;
    private readonly ILogger<GetIndustrySpecificQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetIndustrySpecificQueryHandler(IIndustrySpecificRepository repository, ILogger<GetIndustrySpecificQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IndustrySpecificQueryResult>> Handle(GetIndustrySpecificQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetIndustrySpecificAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<IndustrySpecificQueryResult>.Success(new IndustrySpecificQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve IndustrySpecific for Tenant {TenantId}", request.TenantId);

            return Result<IndustrySpecificQueryResult>.Failure(new ApplicationError(
                "Compliance.Certification.IndustrySpecific.RetrievalFailed",
                "Unable to retrieve industry-specific certification data at this time.",
                Guid.NewGuid()));
        }
    }
}
