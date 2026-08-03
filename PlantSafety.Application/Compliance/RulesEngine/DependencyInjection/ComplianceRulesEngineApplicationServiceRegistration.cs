// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Registers the Compliance Rules Engine's Application-layer services: every
//   IComplianceRule implementation (Safety, Regulatory, InternalPolicy), the ComplianceEvaluator
//   orchestrator, and the Domain-layer ComplianceEvaluationFactory. Self-contained extension
//   method; wired into the composition root by the host application, mirroring
//   AuditingApplicationServiceRegistration's pattern. MediatR handlers and FluentValidation
//   validators are auto-registered via assembly scan.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;
using PlantSafety.Domain.Compliance.RulesEngine.Factories;
using PlantSafety.Domain.Compliance.RulesEngine.Interfaces;
using PlantSafety.Application.Compliance.RulesEngine.Evaluator;
using PlantSafety.Application.Compliance.RulesEngine.Rules;
using PlantSafety.Application.Compliance.RulesEngine.Rules.InternalPolicy;
using PlantSafety.Application.Compliance.RulesEngine.Rules.Regulatory;
using PlantSafety.Application.Compliance.RulesEngine.Rules.Safety;

namespace PlantSafety.Application.Compliance.RulesEngine.DependencyInjection;

/// <summary>Extension methods to register Compliance Rules Engine Application-layer services.</summary>
public static class ComplianceRulesEngineApplicationServiceRegistration
{
    /// <summary>Registers every compliance rule, the ComplianceEvaluator, and the evaluation factory.</summary>
    public static IServiceCollection AddComplianceRulesEngineApplicationServices(this IServiceCollection services)
    {
        // Safety pipeline
        services.AddTransient<IComplianceRule, ControlMeasureJustificationComplianceRule>();
        services.AddTransient<IComplianceRule, PermitExpiryComplianceRule>();

        // Regulatory pipeline
        services.AddTransient<IComplianceRule, RegulatoryObligationOverdueComplianceRule>();
        services.AddTransient<IComplianceRule, CertificationExpiredComplianceRule>();

        // Internal policy pipeline
        services.AddTransient<IComplianceRule, ManagementOfChangeApprovalComplianceRule>();
        services.AddTransient<IComplianceRule, PolicyAcknowledgmentComplianceRule>();

        services.AddScoped<IComplianceEvaluationFactory, ComplianceEvaluationFactory>();
        services.AddScoped<ComplianceEvaluator>();

        return services;
    }
}
