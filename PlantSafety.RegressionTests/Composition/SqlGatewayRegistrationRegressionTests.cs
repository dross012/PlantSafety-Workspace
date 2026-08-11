// ============================================================================
//  Test IDs:  PLNT-REG-DI-001..004
//  Purpose:   Pins AddSqlGateway's registration completeness.
//
//             AddSqlGateway registered two of SqlServerGateway's five
//             constructor dependencies. IClock, ILogEventWriter and
//             ICorrelationContext were not registered -- ICorrelationContext
//             was registered nowhere in the entire repository -- so any host
//             that called AddSqlGateway and nothing else compiled cleanly,
//             started cleanly, and failed at the first resolve with
//             "Unable to resolve service for type ... while attempting to
//             activate SqlServerGateway".
//
//             The gap survived because no test ever built a container from
//             AddSqlGateway alone; the suites that touched the gateway all
//             registered the missing services themselves as part of a larger
//             composition, which is precisely what masked it. These tests
//             call AddSqlGateway the way a host does and nothing more.
// ============================================================================
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Shared.Deterministic.Abstractions.Clock;
using Shared.SqlGateway.Abstractions.Gateway;
using Shared.SqlGateway.Infrastructure.DependencyInjection;
using Shared.SqlGateway.Infrastructure.Options;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;

namespace PlantSafety.RegressionTests.Composition;

public sealed class SqlGatewayRegistrationRegressionTests
{
    private const string SectionKey = "Shared:SqlGateway:ConnectionString";
    private const string FakeConnectionString = "Server=(local);Database=RegressionOnly;Integrated Security=true;";

    private static ServiceProvider BuildProvider(string? connectionString = FakeConnectionString)
    {
        var settings = new Dictionary<string, string?>();

        if (connectionString is not null)
        {
            settings[SectionKey] = connectionString;
        }

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        var services = new ServiceCollection();

        // AddLogging only. It is the one thing every host has before it registers anything of its
        // own, so including it keeps this an honest "AddSqlGateway and nothing else" test rather
        // than quietly supplying the very services whose absence is the defect.
        services.AddLogging();
        services.AddSqlGateway(configuration);

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    [Fact(DisplayName = "PLNT-REG-DI-001: AddSqlGateway alone can resolve ISqlGateway")]
    public void AddSqlGateway_WithNoOtherRegistrations_ShouldResolveTheGateway()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        // Scoped resolve, not root: ICorrelationContext is per-request, and resolving a scoped
        // dependency from the root provider is exactly the mistake ValidateScopes exists to catch.
        var act = () => scope.ServiceProvider.GetRequiredService<ISqlGateway>();

        act.Should().NotThrow(
            "a host that calls AddSqlGateway must get a usable gateway -- registering only some " +
            "of SqlServerGateway's dependencies defers the failure to the first query at runtime");
    }

    [Theory(DisplayName = "PLNT-REG-DI-002: Each of SqlServerGateway's dependencies is registered by AddSqlGateway")]
    [InlineData(typeof(IClock))]
    [InlineData(typeof(ILogEventWriter))]
    [InlineData(typeof(ICorrelationContext))]
    [InlineData(typeof(IOptions<SqlGatewayOptions>))]
    public void AddSqlGateway_ShouldRegisterEveryGatewayDependency(Type dependency)
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        // Enumerated individually rather than relying on PLNT-REG-DI-001 alone so that a future
        // failure names WHICH dependency went missing instead of only that activation failed.
        scope.ServiceProvider.GetService(dependency).Should().NotBeNull(
            "{0} is a constructor dependency of SqlServerGateway and must come from AddSqlGateway",
            dependency.Name);
    }

    [Fact(DisplayName = "PLNT-REG-DI-003: A host-supplied dependency still wins over the built-in default")]
    public void AddSqlGateway_ShouldNotOverrideAHostSuppliedDependency()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [SectionKey] = FakeConnectionString })
            .Build();

        var hostClock = new FixedClock();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IClock>(hostClock);
        services.AddSqlGateway(configuration);

        using var provider = services.BuildServiceProvider();

        // The defaults are added with TryAdd for this reason: tests substitute a deterministic
        // clock, and a host that has already chosen an implementation must keep it. If these ever
        // became plain Add calls, the last registration would win and silently displace it.
        provider.GetRequiredService<IClock>().Should().BeSameAs(hostClock,
            "AddSqlGateway's defaults are TryAdd, so an implementation the host already registered must survive");
    }

    [Fact(DisplayName = "PLNT-REG-DI-004: A missing connection string fails at startup, not at first query")]
    public void AddSqlGateway_WithNoConnectionString_ShouldFailFast()
    {
        using var provider = BuildProvider(connectionString: null);

        var act = () => provider.GetRequiredService<IOptions<SqlGatewayOptions>>().Value;

        // Before this was enforced, a missing connection string was accepted at registration and
        // surfaced much later as a SqlException on the first real query -- in whichever request
        // happened to touch the database first, far from the misconfiguration that caused it.
        act.Should().Throw<OptionsValidationException>(
            "a misconfigured connection string must surface as a startup failure, not as a " +
            "database error inside an unrelated request");
    }

    private sealed class FixedClock : IClock
    {
        public DateTime UtcNow => new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
