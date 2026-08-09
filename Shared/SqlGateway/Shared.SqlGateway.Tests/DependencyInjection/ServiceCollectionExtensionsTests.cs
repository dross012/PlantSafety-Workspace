// ============================================================================
//  Project:        Shared.SqlGateway.Tests
//  File:           ServiceCollectionExtensionsTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Shared.Deterministic.Abstractions.Clock;
using Shared.SqlGateway.Abstractions.Gateway;
using Shared.SqlGateway.Infrastructure.DependencyInjection;
using Shared.SqlGateway.Infrastructure.Options;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.SqlGateway.Tests.DependencyInjection;

public sealed class ServiceCollectionExtensionsTests
{
    private sealed class NoOpLogEventWriter : ILogEventWriter
    {
        public void Write(ILogEvent logEvent)
        {
        }
    }

    private sealed class FixedCorrelationContext : ICorrelationContext
    {
        public string CorrelationId => "test-correlation";
    }

    private static IServiceProvider BuildProvider(Dictionary<string, string?> configValues)
    {
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();
        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddSingleton<IClock>(new FixedClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        services.AddSingleton<ILogEventWriter, NoOpLogEventWriter>();
        services.AddSingleton<ICorrelationContext, FixedCorrelationContext>();
        services.AddSqlGateway(configuration);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddSqlGateway_NoConnectionStringConfigured_FailsAtFirstOptionsAccessNotAtRegistration()
    {
        IServiceProvider provider = BuildProvider(new Dictionary<string, string?>());

        Action act = () => { _ = provider.GetRequiredService<IOptions<SqlGatewayOptions>>().Value; };

        act.Should().Throw<OptionsValidationException>()
            .WithMessage("*ConnectionString is required*");
    }

    [Fact]
    public void AddSqlGateway_ConnectionStringConfigured_ResolvesIsqlGatewayWithoutThrowing()
    {
        IServiceProvider provider = BuildProvider(new Dictionary<string, string?>
        {
            ["Shared:SqlGateway:ConnectionString"] = "Server=localhost;Database=PlantSafety;",
        });

        Action act = () => provider.GetRequiredService<ISqlGateway>();

        act.Should().NotThrow();
    }
}
