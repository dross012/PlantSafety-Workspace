// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Registers Workflow module Application-layer services. MediatR handlers and
//   FluentValidation validators are auto-registered via the top-level assembly scan in
//   ApplicationServiceRegistration; this extension registers module-scoped services that are not
//   picked up by that scan (the stateless WorkflowRuntimeService domain service).
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;
using PlantSafety.Domain.Workflow.Factories;
using PlantSafety.Domain.Workflow.Interfaces;
using PlantSafety.Domain.Workflow.Services;

namespace PlantSafety.Application.Workflow.DependencyInjection;

/// <summary>Extension methods to register Workflow Application-layer services.</summary>
public static class WorkflowApplicationServiceRegistration
{
    /// <summary>
    /// Registers Workflow-specific Application services: the WorkflowRuntimeService domain service
    /// and the Domain-layer aggregate factories. MediatR handlers and validators are auto-registered
    /// via assembly scan.
    /// </summary>
    public static IServiceCollection AddWorkflowApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<WorkflowRuntimeService>();
        services.AddScoped<IWorkflowDefinitionFactory, WorkflowDefinitionFactory>();
        services.AddScoped<IWorkflowInstanceFactory, WorkflowInstanceFactory>();

        return services;
    }
}
