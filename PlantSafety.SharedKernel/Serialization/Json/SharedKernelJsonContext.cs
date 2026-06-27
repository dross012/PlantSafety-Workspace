/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-25
    FILE:               SharedKernelJsonContext.cs
    DESCRIPTION:        Source-generated JSON serialization context for SharedKernel primitives. Ensures deterministic,
                        AOT-safe serialization for StrongIds, ValueTypes, and core primitives.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System.Text.Json.Serialization;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.SharedKernel.Primitives.StrongIds;
using PlantSafety.SharedKernel.Primitives.Errors;

namespace PlantSafety.SharedKernel.Serialization.Json;

[JsonSerializable(typeof(UtcTimestamp))]
[JsonSerializable(typeof(NonEmptyString))]
[JsonSerializable(typeof(PositiveInt))]
[JsonSerializable(typeof(GuidStrongId))]
[JsonSerializable(typeof(EventMetadata))]
[JsonSerializable(typeof(object))]
public partial class SharedKernelJsonContext : JsonSerializerContext
{
}
