// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           InfrastructureOptions.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Composite options wrapper for all Infrastructure configuration sections.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Configuration;

/// <summary>Composite options wrapper that aggregates all Infrastructure configuration.</summary>
public sealed class InfrastructureOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "Infrastructure";

    /// <summary>SQL Server database configuration.</summary>
    public DatabaseOptions Database { get; init; } = new();

    /// <summary>Azure Service Bus messaging configuration.</summary>
    public MessagingOptions Messaging { get; init; } = new();

    /// <summary>Azure Blob Storage configuration.</summary>
    public BlobStorageOptions BlobStorage { get; init; } = new();

    /// <summary>Azure AD / Entra ID configuration.</summary>
    public AzureAdOptions AzureAd { get; init; } = new();

    /// <summary>Caching configuration.</summary>
    public CacheOptions Cache { get; init; } = new();
}
