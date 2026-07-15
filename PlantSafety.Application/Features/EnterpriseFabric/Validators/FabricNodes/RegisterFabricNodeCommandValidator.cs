// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: FluentValidation validator for RegisterFabricNodeCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.EnterpriseFabric.Commands.FabricNodes.RegisterNode;
using PlantSafety.Application.Validation.Extensions;
using PlantSafety.Application.Validation.Rules;

namespace PlantSafety.Application.Features.EnterpriseFabric.Validators.FabricNodes;

/// <summary>Validates RegisterFabricNodeCommand structural fields.</summary>
public sealed class RegisterFabricNodeCommandValidator : AbstractValidator<RegisterFabricNodeCommand>
{
    private static readonly string[] ValidNodeTypes = ["Connector", "Gateway", "Broker", "Adapter"];

    /// <summary>Defines validation rules.</summary>
    public RegisterFabricNodeCommandValidator()
    {
        RuleFor(x => x.CommandId).IsValidId("CommandId");
        RuleFor(x => x.CorrelationId).IsValidId("CorrelationId");
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();

        RuleFor(x => x.NodeName).NotEmpty().MaximumLength(CommonValidationRules.MaxTitleLength);
        RuleFor(x => x.NodeType).NotEmpty().Must(v => ValidNodeTypes.Contains(v))
            .WithMessage("NodeType must be one of: Connector, Gateway, Broker, Adapter.");
        RuleFor(x => x.Region).NotEmpty().MaximumLength(50);
    }
}
