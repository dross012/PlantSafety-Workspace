// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Handler for RegisterGovernedModelCommand.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Utilities;
using PlantSafety.Application.Features.AiGovernance.Commands.RegisterModel;

namespace PlantSafety.Application.Features.AiGovernance.Handlers;

/// <summary>Handles RegisterGovernedModelCommand. Returns the new ModelId.</summary>
public sealed class RegisterGovernedModelCommandHandler : ICommandHandler<RegisterGovernedModelCommand, Guid>
{
    private readonly IAiGovernanceRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<RegisterGovernedModelCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RegisterGovernedModelCommandHandler(
        IAiGovernanceRepository repository,
        IDateTimeProvider clock,
        ILogger<RegisterGovernedModelCommandHandler> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(RegisterGovernedModelCommand request, CancellationToken cancellationToken)
    {
        var modelId = CorrelationIdGenerator.NewId(_clock.UtcNowOffset);

        await _repository.CreateAsync(
            modelId: modelId,
            tenantId: request.TenantId,
            siteId: request.SiteId,
            modelName: request.ModelName,
            modelVersion: request.ModelVersion,
            riskTier: request.RiskTier,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Governed model {ModelId} ({ModelName} {ModelVersion}) registered for Tenant {TenantId} by Actor {ActorId}",
            modelId, request.ModelName, request.ModelVersion, request.TenantId, request.ActorId);

        return modelId;
    }
}
