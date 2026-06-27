// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           BlobStorageOptions.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strongly typed Azure Blob Storage configuration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Configuration;

/// <summary>Strongly typed Azure Blob Storage options.</summary>
public sealed class BlobStorageOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "BlobStorage";

    /// <summary>Azure Storage account connection string.</summary>
    public string ConnectionString { get; init; } = string.Empty;

    /// <summary>Default container name for evidence/document uploads.</summary>
    public string DefaultContainerName { get; init; } = "plantsafety-documents";
}
