/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-28
    FILE:               GuardCollection.cs
    DESCRIPTION:        Canonical guard helpers for collection invariants. Enforces non-null, non-empty, and element-level
                        constraints. MUST remain SIEM-safe and deterministic.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System.Collections;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Exceptions;

namespace PlantSafety.SharedKernel.Primitives.Guards;

/// <summary>
/// Guard helpers for collection invariants.
/// </summary>
public static class GuardCollection
{
    public static void NotNullOrEmpty(IEnumerable? collection, string name)
    {
        if (collection is null)
            throw new DomainException(CommonErrors.NullValue);

        var enumerator = collection.GetEnumerator();
        if (!enumerator.MoveNext())
            throw new DomainException(
                ErrorFactory.Create("validation.emptycollection", ErrorType.Validation,
                    $"{name} cannot be empty."));
    }
}
