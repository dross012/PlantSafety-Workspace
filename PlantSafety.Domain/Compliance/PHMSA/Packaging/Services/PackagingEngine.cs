// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Domain
//  File:           PackagingEngine.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless, deterministic domain service that determines and validates
//                  DOT-authorized packaging for a given hazard class/packing group combination.
//
//                  IMPORTANT: The rules table below is an ILLUSTRATIVE, deterministic ruleset
//                  covering a handful of common hazard class/packing group combinations. It is
//                  NOT a substitute for the full 49 CFR Part 173/178 packaging authorization
//                  tables. It is intentionally structured as a simple (HazardClass, PackingGroup)
//                  keyed lookup so a real data source can replace the dictionary later without
//                  changing this class's public API. When no match is found, this engine returns
//                  Success=false with an explanatory message -- it never guesses or fabricates an
//                  authorization.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects;

namespace PlantSafety.Domain.Compliance.PHMSA.Packaging.Services;

/// <summary>Input to <see cref="PackagingEngine.DeterminePackaging"/>.</summary>
public sealed record PackagingDeterminationInput(
    HazardClass HazardClass,
    PackingGroup PackingGroup,
    decimal Quantity,
    string QuantityUnit,
    string ContainerTypeRequested);

/// <summary>Result of <see cref="PackagingEngine.DeterminePackaging"/>.</summary>
public sealed record PackagingDeterminationResult(
    bool Success,
    IReadOnlyList<DOTSpec> AuthorizedPackaging,
    string ClosureInstructions,
    IReadOnlyList<string> Messages)
{
    /// <summary>Convenience factory for an unsuccessful determination with a single explanatory message.</summary>
    public static PackagingDeterminationResult Failed(string message) => new(
        Success: false,
        AuthorizedPackaging: [],
        ClosureInstructions: string.Empty,
        Messages: [message]);
}

/// <summary>Result of <see cref="PackagingEngine.ValidatePackaging"/>.</summary>
public sealed record PackagingValidationResult(
    bool IsAuthorized,
    string Message);

/// <summary>
/// Pure, stateless, deterministic packaging determination/validation engine. Not a substitute
/// for the full 49 CFR Part 173/178 authorization tables -- see file header for details.
/// </summary>
public sealed class PackagingEngine : IDomainService
{
    private sealed record RuleEntry(IReadOnlyList<string> AuthorizedSpecs, string ClosureInstructions);

    // ---- Illustrative deterministic rules table, keyed by (hazard class, packing group). ----
    // Structured to be trivially replaceable by a real data source (repository/external feed).
    private static readonly IReadOnlyDictionary<(string HazardClassKey, PackingGroup PackingGroup), RuleEntry> RulesTable =
        new Dictionary<(string, PackingGroup), RuleEntry>
        {
            [("3", PackingGroup.II)] = new RuleEntry(
                AuthorizedSpecs: ["1A1", "1A2", "3A1", "3H1"],
                ClosureInstructions: "Close all bungs/closures to manufacturer torque spec; verify gasket seating before transport."),

            [("3", PackingGroup.III)] = new RuleEntry(
                AuthorizedSpecs: ["1A1", "1A2", "3A1", "3H1", "4G"],
                ClosureInstructions: "Close all bungs/closures to manufacturer torque spec."),

            [("8", PackingGroup.II)] = new RuleEntry(
                AuthorizedSpecs: ["1A2", "3A1", "6HA1"],
                ClosureInstructions: "Use acid-resistant gaskets; verify closure integrity and secondary containment before transport."),

            [("8", PackingGroup.III)] = new RuleEntry(
                AuthorizedSpecs: ["1A2", "3A1", "6HA1", "4G"],
                ClosureInstructions: "Use acid-resistant gaskets; verify closure integrity before transport."),

            [("2.3", PackingGroup.NotApplicable)] = new RuleEntry(
                AuthorizedSpecs: ["3AA", "3T"],
                ClosureInstructions: "Use DOT-approved pressure vessel valve/cap assembly; verify leak-tightness per 49 CFR 173.301 before transport."),

            [("9", PackingGroup.III)] = new RuleEntry(
                AuthorizedSpecs: ["1A2", "4G", "11G"],
                ClosureInstructions: "Close per manufacturer instructions; no special closure torque required for Class 9."),
        };

    /// <summary>
    /// Determines DOT-authorized packaging for the given hazard class/packing group. Returns
    /// <c>Success=false</c> with an explanatory message when no rule matches -- this method
    /// never guesses or fabricates an authorization for an unrecognized combination.
    /// </summary>
    public PackagingDeterminationResult DeterminePackaging(PackagingDeterminationInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.Quantity <= 0)
            return PackagingDeterminationResult.Failed("Quantity must be positive.");

        var key = (input.HazardClass.ToString(), input.PackingGroup);
        if (!RulesTable.TryGetValue(key, out var rule))
        {
            return PackagingDeterminationResult.Failed(
                $"No packaging determination rule found for hazard class '{input.HazardClass}', packing group " +
                $"'{input.PackingGroup}'. This is not a substitute for the full 49 CFR Part 173/178 packaging " +
                "authorization tables -- consult a licensed hazmat packaging reference for an authoritative answer.");
        }

        return new PackagingDeterminationResult(
            Success: true,
            AuthorizedPackaging: rule.AuthorizedSpecs.Select(spec => new DOTSpec(spec)).ToList(),
            ClosureInstructions: rule.ClosureInstructions,
            Messages: [$"Determined {rule.AuthorizedSpecs.Count} authorized packaging spec(s) for hazard class " +
                       $"'{input.HazardClass}', packing group '{input.PackingGroup}'."]);
    }

    /// <summary>
    /// Validates whether <paramref name="proposedSpec"/> is DOT-authorized for the given hazard
    /// class/packing group combination.
    /// </summary>
    public PackagingValidationResult ValidatePackaging(DOTSpec proposedSpec, HazardClass hazardClass, PackingGroup packingGroup)
    {
        var key = (hazardClass.ToString(), packingGroup);
        if (!RulesTable.TryGetValue(key, out var rule))
        {
            return new PackagingValidationResult(
                IsAuthorized: false,
                Message: $"No packaging authorization rule found for hazard class '{hazardClass}', packing group '{packingGroup}'; cannot validate '{proposedSpec}'.");
        }

        var isAuthorized = rule.AuthorizedSpecs.Any(spec => string.Equals(spec, proposedSpec.Value, StringComparison.OrdinalIgnoreCase));

        return new PackagingValidationResult(
            IsAuthorized: isAuthorized,
            Message: isAuthorized
                ? $"'{proposedSpec}' is authorized for hazard class '{hazardClass}', packing group '{packingGroup}'."
                : $"'{proposedSpec}' is NOT authorized for hazard class '{hazardClass}', packing group '{packingGroup}'. " +
                  $"Authorized specs: {string.Join(", ", rule.AuthorizedSpecs)}.");
    }
}
