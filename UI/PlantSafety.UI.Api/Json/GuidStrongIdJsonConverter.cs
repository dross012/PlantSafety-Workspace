// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Json
//  File:           GuidStrongIdJsonConverter.cs
//  Created:        2026-08-09
//  Description:    Generic System.Text.Json converter for the "wraps a
//                  non-empty Guid via a private constructor + New()/From()"
//                  strong-id pattern used throughout Shared.*/Enterprise.*
//                  (FacilityId, EmployeeId, MachineId, BuildingId, AreaId,
//                  ZoneId, EnterpriseRuleId, EnterpriseWorkflowId,
//                  EnterpriseNodeId, ...). These types have no public
//                  constructor, so System.Text.Json cannot bind them without
//                  an explicit converter -- this is that converter,
//                  parameterized once per type instead of hand-written N
//                  times. Serializes/deserializes as a plain GUID string.
// ============================================================================
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlantSafety.UI.Api.Json;

/// <summary>Serializes/deserializes a "wraps a non-empty Guid" strong-id struct as a plain GUID string.</summary>
public sealed class GuidStrongIdJsonConverter<T> : JsonConverter<T>
{
    private readonly Func<Guid, T> _from;
    private readonly Func<T, Guid> _toGuid;

    /// <summary>Creates a converter for a strong-id type using its own <c>From(Guid)</c> factory and <c>Value</c> accessor.</summary>
    public GuidStrongIdJsonConverter(Func<Guid, T> from, Func<T, Guid> toGuid)
    {
        _from = from ?? throw new ArgumentNullException(nameof(from));
        _toGuid = toGuid ?? throw new ArgumentNullException(nameof(toGuid));
    }

    /// <inheritdoc />
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        _from(reader.GetGuid());

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
        writer.WriteStringValue(_toGuid(value));
}
