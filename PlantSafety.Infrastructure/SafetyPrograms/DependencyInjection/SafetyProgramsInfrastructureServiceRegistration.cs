// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPrograms.DependencyInjection
//  Layer:          Infrastructure
//  File:           SafetyProgramsInfrastructureServiceRegistration.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI registration entry point for the SafetyPrograms
//                  Infrastructure layer. Mirrors SafetyCultureInfrastructureServiceRegistration:
//                  call <c>services.AddSafetyProgramsInfrastructure()</c> from the host startup
//                  alongside <c>services.AddInfrastructure(configuration)</c>. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Interfaces;
using PlantSafety.Infrastructure.SafetyPrograms.SafetyProgram.Persistence;

namespace PlantSafety.Infrastructure.SafetyPrograms.DependencyInjection;

/// <summary>Per-module DI entry point for the SafetyPrograms Infrastructure layer.</summary>
public static class SafetyProgramsInfrastructureServiceRegistration
{
    /// <summary>Registers all SafetyPrograms Infrastructure repositories into the DI container.</summary>
    public static IServiceCollection AddSafetyProgramsInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ISafetyProgramRepository, SafetyProgramDapperRepository>();

        return services;
    }
}
