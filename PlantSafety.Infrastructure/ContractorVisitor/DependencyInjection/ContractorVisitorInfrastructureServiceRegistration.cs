// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Infrastructure
//  File:           ContractorVisitorInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the ContractorVisitor Infrastructure-layer
//                  repository implementations. Registers both same-simple-name
//                  IContractorVisitorRepository interfaces (Domain write-side ->
//                  ContractorVisitorAggregateDapperRepository, Application read-side ->
//                  ContractorVisitorReadDapperRepository) using fully-qualified names to avoid an
//                  ambiguous-reference compile error. Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.ContractorVisitor.Persistence;

namespace PlantSafety.Infrastructure.ContractorVisitor.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the ContractorVisitor module.</summary>
public static class ContractorVisitorInfrastructureServiceRegistration
{
    /// <summary>Adds ContractorVisitor Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddContractorVisitorModuleInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.ContractorVisitor.Repositories.IContractorVisitorRepository, ContractorVisitorAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.ContractorVisitor.Repositories.IContractorVisitorRepository, ContractorVisitorReadDapperRepository>();

        return services;
    }
}
