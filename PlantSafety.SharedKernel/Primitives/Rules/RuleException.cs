/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-28
    FILE:               RuleException.cs
    DESCRIPTION:        Canonical exception for rule evaluation failures that are *not* expected business failures.
                        Represents unexpected or catastrophic rule engine errors.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Exceptions;

namespace PlantSafety.SharedKernel.Primitives.Rules;

/// <summary>
/// Represents an unexpected or catastrophic rule evaluation failure.
/// </summary>
public sealed class RuleException : BaseException
{
    public RuleException(string message, Exception? inner = null)
        : base(
            ErrorFactory.Create("rules.exception", ErrorType.Failure, message),
            ExceptionSeverity.Critical,
            inner)
    {
    }
}
