// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Analytics.Culture.Queries
//  Layer:          Tests (Unit)
//  File:           GetCultureQueryTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetCultureQuery record validity.
// ============================================================================
using PlantSafety.Application.Analytics.Culture.Queries;

namespace PlantSafety.Tests.Analytics.Culture.Queries;

public sealed class GetCultureQueryTests : UnitTestBase
{
    [Fact]
    public void Ctor_ShouldPreserveTenantId()
    {
        var qry = new GetCultureQuery(new Guid("00000010-0000-0000-0000-000000000010"), TestTenantId);
        qry.TenantId.Should().Be(TestTenantId);
    }

    [Fact]
    public void Ctor_ShouldPreserveId()
    {
        var id = new Guid("00000010-0000-0000-0000-000000000010");
        var qry = new GetCultureQuery(id, TestTenantId);
        qry.Id.Should().Be(id);
    }
}
