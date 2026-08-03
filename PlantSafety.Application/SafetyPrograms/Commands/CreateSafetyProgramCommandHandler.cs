// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for CreateSafetyProgramCommand. Pure orchestration: constructs the
//   SafetyProgram aggregate via its domain factory method and persists it through the domain
//   repository interface. No business rules live here.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Enums;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Interfaces;

using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyPrograms.Commands;

/// <summary>Handles <see cref="CreateSafetyProgramCommand"/>.</summary>
public sealed class CreateSafetyProgramCommandHandler : IRequestHandler<CreateSafetyProgramCommand, Result<CreateSafetyProgramResult>>
{
    private readonly ISafetyProgramRepository _repository;
    private readonly ILogger<CreateSafetyProgramCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateSafetyProgramCommandHandler(ISafetyProgramRepository repository, ILogger<CreateSafetyProgramCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateSafetyProgramResult>> Handle(CreateSafetyProgramCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<SafetyProgramCategory>(request.Category, ignoreCase: true, out var category))
            {
                return Result<CreateSafetyProgramResult>.Failure(new ApplicationError(
                    "SafetyPrograms.SafetyProgram.InvalidCategory",
                    $"Category '{request.Category}' is not a recognized SafetyProgram category.",
                    request.CorrelationId,
                    ErrorSeverity.Validation));
            }

            var ownerId = UserId.FromGuid(request.OwnerId);

            var aggregate = Domain.SafetyPrograms.SafetyProgram.Aggregates.SafetyProgram.Create(
                createdBy: ownerId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(ownerId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                name: request.Name,
                description: request.Description,
                category: category,
                startDate: request.StartDate,
                endDate: request.EndDate,
                ownerId: ownerId,
                goals: request.Goals);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateSafetyProgramResult>.Success(new CreateSafetyProgramResult(
                aggregate.SafetyProgramId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create SafetyProgram for Tenant {TenantId}", request.TenantId);

            return Result<CreateSafetyProgramResult>.Failure(new ApplicationError(
                "SafetyPrograms.SafetyProgram.CreateFailed",
                "Unable to create the SafetyProgram record at this time.",
                request.CorrelationId));
        }
    }
}
