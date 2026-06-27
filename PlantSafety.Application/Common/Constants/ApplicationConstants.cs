// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Application-wide constants for the Application layer.
//   No magic strings or numbers anywhere in the codebase — all are referenced here.
//   Additive-only: new constants may be added; existing constants must not change value.
// SchemaVersion: 1
// Additive-only evolution: new constants may be added; existing values must not change.

namespace PlantSafety.Application.Common.Constants;

/// <summary>
/// Platform-wide constants for the Application layer.
/// Every constant that would otherwise be a magic string or number MUST be declared here.
/// </summary>
public static class ApplicationConstants
{
    /// <summary>The current API version token injected into all commands and queries.</summary>
    public const string ApiVersionToken = "v1";

    /// <summary>The service name used in structured log envelopes and telemetry.</summary>
    public const string ServiceName = "PlantSafety.Application";

    /// <summary>The elapsed millisecond threshold above which a request is classified as slow (500ms).</summary>
    public const long DefaultSlowRequestThresholdMs = 500L;

    /// <summary>The maximum number of retry attempts for transient failures in background jobs.</summary>
    public const int DefaultMaxRetryAttempts = 3;

    /// <summary>The base delay in milliseconds for exponential backoff on first retry.</summary>
    public const int DefaultBaseRetryDelayMs = 250;

    /// <summary>The maximum number of items that may be processed in a single bulk command.</summary>
    public const int DefaultMaxBulkItemCount = 500;

    /// <summary>The maximum length of a sanitized string field in structured log envelopes.</summary>
    public const int MaxLogMessageLength = 2048;

    /// <summary>The cache key separator character for multi-segment cache keys.</summary>
    public const char CacheKeySeparator = ':';

    /// <summary>Standard cache prefix for tenant-scoped query results.</summary>
    public const string CachePrefixTenantQuery = "tq";

    /// <summary>Standard cache prefix for aggregate read models.</summary>
    public const string CachePrefixReadModel = "rm";
}
