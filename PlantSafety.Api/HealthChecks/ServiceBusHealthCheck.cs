// ============================================================================
// File: ServiceBusHealthCheck.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-12
// Last Updated: 2026-06-12
// Description: ASP.NET Core health check that verifies Azure Service Bus namespace
//              connectivity for the /health/ready readiness probe.
// ============================================================================
//
// Onboarding Notes:
// - Registered in Extensions/ServiceCollectionExtensions.cs and mapped to the
//   /health/ready readiness endpoint in Program.cs.
// - Uses the Azure.Messaging.ServiceBus client to verify that the namespace is reachable
//   and the management client can list entity properties.
// - Azure Service Bus is the backbone of all integration event publishing via MassTransit.
//   If the namespace is unreachable, the Outbox processor (in Infrastructure) cannot
//   publish integration events. See Architecture Specification §8.3 (Messaging).
// - Authentication uses the pod-level Managed Identity (Azure Workload Identity) via
//   DefaultAzureCredential — no connection strings or API keys in configuration.
//
// Rationale:
// - Service Bus unavailability means integration events cannot flow between bounded contexts.
//   The readiness probe prevents pods from accepting traffic they cannot reliably process.
//
// Future Extension Seams:
// - Add dead-letter queue depth monitoring to flag degraded (not just failed) bus connectivity.
// - Support custom topic/queue names for more targeted health checks.
//
// ============================================================================

using Azure.Identity;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PlantSafety.Api.HealthChecks;

/// <summary>
/// Health check that validates Azure Service Bus namespace connectivity by querying
/// the namespace properties. Uses Managed Identity authentication.
/// Used by the /health/ready readiness probe.
/// </summary>
public sealed class ServiceBusHealthCheck : IHealthCheck
{
    private const string NamespaceConfigKey = "AzureServiceBus:Namespace";
    private readonly IConfiguration _configuration;
    private readonly ILogger<ServiceBusHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ServiceBusHealthCheck"/>.
    /// </summary>
    /// <param name="configuration">Configuration provider backed by Azure Key Vault in production.</param>
    /// <param name="logger">Logger for recording health check failures.</param>
    public ServiceBusHealthCheck(IConfiguration configuration, ILogger<ServiceBusHealthCheck> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Attempts to retrieve namespace properties from Azure Service Bus to verify connectivity.
    /// Uses the pod-level Managed Identity for authentication.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// <see cref="HealthCheckResult.Healthy"/> if the namespace is reachable;
    /// <see cref="HealthCheckResult.Unhealthy"/> otherwise.
    /// </returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var @namespace = _configuration[NamespaceConfigKey];

            if (string.IsNullOrWhiteSpace(@namespace))
            {
                return HealthCheckResult.Unhealthy("Azure Service Bus namespace is not configured.");
            }

            var fullyQualifiedNamespace = @namespace.Contains('.')
                ? @namespace
                : $"{@namespace}.servicebus.windows.net";

            var client = new ServiceBusAdministrationClient(fullyQualifiedNamespace, new DefaultAzureCredential());
            await client.GetNamespacePropertiesAsync(cancellationToken);

            return HealthCheckResult.Healthy("Azure Service Bus namespace is reachable.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service Bus health check failed.");
            return HealthCheckResult.Unhealthy("Azure Service Bus connection failed.", ex);
        }
    }
}
