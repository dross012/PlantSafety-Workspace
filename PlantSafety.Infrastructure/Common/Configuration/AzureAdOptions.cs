// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           AzureAdOptions.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strongly typed Azure Active Directory / Entra ID configuration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Configuration;

/// <summary>Strongly typed Azure AD / Entra ID options for token validation.</summary>
public sealed class AzureAdOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "AzureAd";

    /// <summary>Azure AD tenant ID.</summary>
    public string TenantId { get; init; } = string.Empty;

    /// <summary>Application (client) ID registered in Azure AD.</summary>
    public string ClientId { get; init; } = string.Empty;

    /// <summary>Expected token audience (API application ID URI).</summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>Authority URL: https://login.microsoftonline.com/{TenantId}</summary>
    public string Authority => $"https://login.microsoftonline.com/{TenantId}";
}
