// ============================================================================
//  Test IDs:  PLNT-PEN-SQLI-001..003
//  Manual:    tests_rules_design_manual.md (PEN type); OWASP Top 10 (A03
//             Injection); project_plantsafety_patterns.md (Dapper +
//             stored-procs-only persistence rule).
//  Purpose:   Confirms StoredProcedureBuilder (the sole route by which
//             Infrastructure repositories construct SQL parameters) always
//             produces PARAMETERIZED Dapper DynamicParameters -- never raw
//             string concatenation -- so classic SQL-injection payloads are
//             inert regardless of what a caller supplies as a value.
// ============================================================================
namespace PlantSafety.SecurityTests.Persistence;

public sealed class SqlInjectionResistanceTests
{
    private const string ClassicPayload = "'; DROP TABLE Alerts; --";
    private const string UnionPayload = "' UNION SELECT TenantId, Password FROM Users --";
    private const string BooleanBypassPayload = "' OR '1'='1";

    [Theory(DisplayName = "PLNT-PEN-SQLI-001: AddString stores injection payloads as inert parameter VALUES, never concatenated SQL")]
    [InlineData(ClassicPayload)]
    [InlineData(UnionPayload)]
    [InlineData(BooleanBypassPayload)]
    public void AddString_WithInjectionPayload_ShouldBeStoredAsParameterValue(string payload)
    {
        var parameters = new StoredProcedureBuilder()
            .AddString("@Description", payload)
            .Build();

        // DynamicParameters exposes only named parameter slots -- there is no SQL text to
        // inspect at all, which is itself the proof there is no string-concatenation surface.
        parameters.ParameterNames.Should().ContainSingle().Which.Should().Be("Description");
        ((object?)parameters.Get<string>("@Description")).Should().Be(payload,
            "the payload must round-trip unchanged as a bound parameter value, never interpreted as SQL");
    }

    [Fact(DisplayName = "PLNT-PEN-SQLI-002: WithCanonicalHeaders binds TenantId/CorrelationId/ActorId strictly as typed GUID parameters")]
    public void WithCanonicalHeaders_ShouldBindGuidParametersWithStrictTyping()
    {
        var tenantId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, now)
            .Build();

        parameters.Get<Guid>("@TenantId").Should().Be(tenantId);
        parameters.Get<Guid>("@CorrelationId").Should().Be(correlationId);
        parameters.Get<Guid>("@ActorId").Should().Be(actorId);
        // A GUID-typed parameter cannot carry a string SQL-injection payload -- Dapper/SqlClient
        // will reject non-GUID input at the ADO.NET boundary before any command executes.
    }

    [Fact(DisplayName = "PLNT-PEN-SQLI-003: A malicious tenant identifier string cannot be smuggled through a GUID parameter")]
    public void WithCanonicalHeaders_ShouldNotAcceptStringPayloadAsTenantId()
    {
        // WithCanonicalHeaders' signature only accepts System.Guid for the tenant/correlation/actor
        // parameters -- this is a compile-time guarantee, not a runtime check. Attempting to pass
        // an injection string simply does not compile, which is the strongest possible defense.
        // (See project_plantsafety_patterns.md: canonical query object + StrongId pattern.)
        typeof(StoredProcedureBuilder)
            .GetMethod(nameof(StoredProcedureBuilder.WithCanonicalHeaders))!
            .GetParameters()
            .Where(p => p.Name is "tenantId" or "correlationId" or "actorId")
            .Should().OnlyContain(p => p.ParameterType == typeof(Guid));
    }
}
