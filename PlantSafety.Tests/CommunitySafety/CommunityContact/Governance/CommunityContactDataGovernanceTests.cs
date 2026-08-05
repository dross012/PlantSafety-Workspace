// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CommunitySafety.CommunityContact.Governance
//  Layer:          Tests (Unit)
//  File:           CommunityContactDataGovernanceTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Data-governance regression tests for the CommunityContact aggregate and its
//                  read-model DTO. CommunityContact stores neighbor/community PII and is subject
//                  to the CommunitySafety PII-minimization mandate: no home address or date-of-
//                  birth field may ever be introduced, and access must remain gated behind the
//                  dedicated View/Manage permission constants (never the generic Read/Write
//                  policies alone). These tests fail loudly if a future change reintroduces a
//                  forbidden PII field or removes the access-restriction permission constants.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using System.Reflection;
using PlantSafety.Application.CommunitySafety;
using PlantSafety.Application.CommunitySafety.CommunityContact.Models;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityContact.Aggregates.CommunityContact;

namespace PlantSafety.Tests.CommunitySafety.CommunityContact.Governance;

public sealed class CommunityContactDataGovernanceTests : UnitTestBase
{
    private static readonly string[] ForbiddenPiiFieldNameFragments =
    [
        "address", "street", "postal", "zipcode", "zip", "dateofbirth", "dob", "birthdate", "ssn", "socialsecurity",
    ];

    [Fact]
    public void CommunityContactAggregate_DoesNotExposeForbiddenPiiFields()
    {
        var propertyNames = typeof(Agg).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name);

        foreach (var name in propertyNames)
        {
            var lowered = name.ToLowerInvariant();
            ForbiddenPiiFieldNameFragments.Should().NotContain(
                fragment => lowered.Contains(fragment),
                $"CommunityContact property '{name}' looks like it may violate the PII-minimization mandate (no home address or date of birth).");
        }
    }

    [Fact]
    public void CommunityContactItemDto_DoesNotExposeForbiddenPiiFields()
    {
        var propertyNames = typeof(CommunityContactItemDto).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name);

        foreach (var name in propertyNames)
        {
            var lowered = name.ToLowerInvariant();
            ForbiddenPiiFieldNameFragments.Should().NotContain(
                fragment => lowered.Contains(fragment),
                $"CommunityContactItemDto property '{name}' looks like it may violate the PII-minimization mandate (no home address or date of birth).");
        }
    }

    [Fact]
    public void CommunitySafetyModule_DefinesNarrowCommunityContactAccessPermissions()
    {
        CommunitySafetyModule.Permissions.ViewCommunityContact.Should().Be("CommunitySafety.CommunityContact.View");
        CommunitySafetyModule.Permissions.ManageCommunityContact.Should().Be("CommunitySafety.CommunityContact.Manage");
    }

    [Fact]
    public void CommunityContactAggregate_StoresOnlyASingleEmailAndPhoneChannel()
    {
        // Guards against a future change silently widening the contact-channel surface area
        // (e.g. adding a collection of phone numbers/addresses) without a corresponding
        // data-governance review.
        var emailProperties = typeof(Agg).GetProperties().Where(p => p.Name.Contains("Email", StringComparison.OrdinalIgnoreCase));
        var phoneProperties = typeof(Agg).GetProperties().Where(p => p.Name.Contains("Phone", StringComparison.OrdinalIgnoreCase));

        emailProperties.Should().ContainSingle();
        phoneProperties.Should().ContainSingle();
    }
}
