// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Small extension helper that unwraps a
//   ApiResponseEnvelope&lt;ReadModelEnvelopeDto&lt;TItem&gt;&gt; into its inner
//   ReadModelPayloadDto&lt;TItem&gt; payload, or throws a sanitized
//   InvalidOperationException describing the failure. Used by
//   CertificationDetailPage's deep-link fallback lookup, where a single
//   expression chain is clearer than the list page's multi-branch handling.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Linq;

using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.Certifications.Models;

namespace PlantSafety.UI.Modules.Certifications.Mappings;

/// <summary>Extension helpers for unwrapping certification read-model envelopes.</summary>
public static class ReadModelEnvelopeExtensions
{
    /// <summary>
    /// Returns the inner payload when the call succeeded end-to-end (HTTP success and
    /// <c>Success: true</c> body), otherwise throws a sanitized <see cref="InvalidOperationException"/>.
    /// </summary>
    public static ReadModelPayloadDto<TItem> RequireData<TItem>(
        this ApiResponseEnvelope<ReadModelEnvelopeDto<TItem>> envelope)
    {
        if (envelope.IsSuccess && envelope.Data is { Success: true, Data: not null })
            return envelope.Data.Data;

        if (envelope.IsSuccess)
        {
            var message = envelope.Data?.Errors is { Count: > 0 } errs
                ? string.Join("; ", errs)
                : "The certification read-model returned no data.";
            throw new InvalidOperationException(message);
        }

        if (envelope.IsProblem)
            throw new InvalidOperationException(envelope.Problem!.Detail);

        throw new InvalidOperationException($"Failed to load certification: {envelope.FaultType}");
    }
}
