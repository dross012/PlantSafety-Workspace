// ============================================================================
//  Project:        Enterprise.DataLake.Infrastructure
//  Module:         DependencyInjection
//  File:           DataLakeServiceCollectionExtensions.cs
//  Created:        2026-08-08
//  Description:    Registers Enterprise.DataLake's in-memory store, writer,
//                  and query engine with the DI container. IDataLakeStore
//                  is a singleton (it IS the process-local store);
//                  IDataLakeWriter/IDataLakeQueryEngine are scoped,
//                  matching Shared.FacilityDomain's established convention.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;

using Enterprise.DataLake.Infrastructure.Query;
using Enterprise.DataLake.Infrastructure.Storage;
using Enterprise.DataLake.Infrastructure.Writing;

namespace Enterprise.DataLake.Infrastructure.DependencyInjection;

/// <summary>Registers Enterprise.DataLake's services with the DI container.</summary>
public static class DataLakeServiceCollectionExtensions
{
    /// <summary>Registers <see cref="IDataLakeStore"/> (singleton), <see cref="IDataLakeWriter"/>, and <see cref="IDataLakeQueryEngine"/> (both scoped).</summary>
    public static IServiceCollection AddEnterpriseDataLake(this IServiceCollection services)
    {
        services.AddSingleton<IDataLakeStore, DataLakeStore>();
        services.AddScoped<IDataLakeWriter, DataLakeWriter>();
        services.AddScoped<IDataLakeQueryEngine, DataLakeQueryEngine>();

        return services;
    }
}
