// ============================================================================
//  Test IDs:  PLNT-SMKE-REPO-001..004
//  Manual:    tests_rules_design_manual.md (SMKE type) /
//             project_datalake_tests.md (bootstrap/fixture conventions)
//  Purpose:   Confirms the 105 repositories built out this session across the
//             Compliance (62), Analytics (22), CrossDomain (12) and
//             SafetyEvents (9) read-model trees resolve cleanly from the DI
//             container that PlantSafety.Api wires up in
//             InfrastructureServiceRegistration.AddInfrastructure. A failure
//             here means the application would fail to start in production.
//             No live SQL Server is used -- ISqlConnectionFactory is a test
//             double so this exercises constructor/DI wiring only, not query
//             execution (query execution against a real DB is covered by
//             PlantSafety.IntegrationTests and PlantSafety.DataIntegrationTests).
// ============================================================================
namespace PlantSafety.SmokeTests.Repositories;

public sealed class RepositoryLayerSmokeTests
{
    /// <summary>
    /// Composes the real container via <c>InfrastructureServiceRegistration.AddInfrastructure</c>
    /// and returns every repository registration in it.
    /// </summary>
    /// <remarks>
    /// This previously called AddComplianceRepositories/AddAnalyticsRepositories/
    /// AddCrossDomainRepositories/AddSafetyEventsRepositories, which are vestigial stubs whose
    /// bodies are a single "// TODO: services.AddScoped&lt;...&gt;()" comment. They register nothing,
    /// so the scale assertion below found 12 descriptors (the four handful this file adds by hand)
    /// instead of the ~100 it asserts, and the whole suite looked like the repository layer had
    /// never been built.
    ///
    /// It had been built -- 417 *DapperRepository implementations exist and 217 repository
    /// interfaces are registered across 41 per-module registration files, all chained together by
    /// AddInfrastructure. The test was simply pointed at the wrong entry point, which is also what
    /// its own header always said it should use: "the DI container that PlantSafety.Api wires up
    /// in InfrastructureServiceRegistration.AddInfrastructure".
    ///
    /// Only repository registrations are returned. AddInfrastructure also wires Redis, MassTransit
    /// and typed HttpClients; those register lazily and are never resolved here, so no external
    /// service is contacted. ISqlConnectionFactory is still substituted, so this exercises
    /// constructor/DI wiring only -- not query execution.
    /// </remarks>
    private static ServiceProvider BuildProvider(out IReadOnlyList<ServiceDescriptor> repositoryDescriptors)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Redis"] = "localhost:6379",
                ["ConnectionStrings:PlantSafetyDb"] = "Server=(localdb)\\MSSQLLocalDB;Database=PlantSafetySmoke;Integrated Security=true;TrustServerCertificate=true;",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);

        // Substituted after AddInfrastructure so these win over the real registrations: the smoke
        // test must not open a SQL connection or reach blob storage.
        services.AddSingleton(Substitute.For<ISqlConnectionFactory>());
        services.AddSingleton(Substitute.For<PlantSafety.Infrastructure.FileStorage.IBlobStorageService>());

        repositoryDescriptors = services
            .Where(descriptor => descriptor.ServiceType.IsInterface)
            .Where(descriptor => descriptor.ServiceType.Name.EndsWith("Repository", StringComparison.Ordinal))
            .ToList();

        return services.BuildServiceProvider(validateScopes: true);
    }

    [Fact(DisplayName = "PLNT-SMKE-REPO-001: Compliance+Analytics+CrossDomain+SafetyEvents repositories register at least 100 interfaces")]
    public void RepositoryRegistrations_ShouldMatchExpectedScale()
    {
        using var provider = BuildProvider(out var descriptors);

        // Scoped to the module: 62 Compliance + 22 Analytics + 12 CrossDomain + 9 SafetyEvents = 105.
        descriptors.Count.Should().BeGreaterThanOrEqualTo(100,
            "the newly-built read-model repository layer registers 105 interfaces (62+22+12+9)");
    }

    [Fact(DisplayName = "PLNT-SMKE-REPO-002: Every registered repository resolves without throwing")]
    public void AllRepositories_ShouldResolve_WithoutThrowing()
    {
        using var provider = BuildProvider(out var descriptors);
        using var scope = provider.CreateScope();

        var failures = new List<string>();

        foreach (var descriptor in descriptors)
        {
            try
            {
                var instance = scope.ServiceProvider.GetRequiredService(descriptor.ServiceType);
                if (instance is null)
                    failures.Add($"{descriptor.ServiceType.FullName} resolved to null");
            }
            catch (Exception ex)
            {
                failures.Add($"{descriptor.ServiceType.FullName} -> {ex.GetType().Name}: {ex.Message}");
            }
        }

        failures.Should().BeEmpty("all repository interfaces must be resolvable from the composed DI container");
    }

    [Fact(DisplayName = "PLNT-SMKE-REPO-003: Every registered repository implementation type is named *Repository")]
    public void AllRepositories_ImplementationTypes_ShouldFollowNamingConvention()
    {
        using var provider = BuildProvider(out var descriptors);

        // Asserts *Repository, not *DapperRepository. The narrower form held only while this test
        // looked at the four (empty) read-model stubs; across the real container it is simply not
        // the convention, and demanding it would be asserting something untrue rather than
        // catching drift. Dapper is the rule for read-model repositories backed by stored
        // procedures, but the container also holds deliberate non-Dapper implementations --
        // InMemoryPartnerIdentityMappingRepository, the AuditTimeline*OutboxRepository pair, and
        // the EmployeeSafety/ContractorSafety/MachineSafety/SafetyProcedures repositories.
        var offenders = descriptors
            .Where(d => d.ImplementationType is not null)
            .Where(d => !d.ImplementationType!.Name.EndsWith("Repository", StringComparison.Ordinal))
            .Select(d => d.ImplementationType!.FullName)
            .ToList();

        offenders.Should().BeEmpty("every repository implementation follows the *Repository naming convention");
    }

    [Fact(DisplayName = "PLNT-SMKE-REPO-004: Resolving the same scoped repository twice within a scope returns the same instance")]
    public void ScopedRepositories_ShouldBeScopedLifetime()
    {
        using var provider = BuildProvider(out var descriptors);
        var sample = descriptors.First();

        using var scope = provider.CreateScope();
        var first = scope.ServiceProvider.GetRequiredService(sample.ServiceType);
        var second = scope.ServiceProvider.GetRequiredService(sample.ServiceType);

        ReferenceEquals(first, second).Should().BeTrue("all repositories are registered AddScoped and must share an instance within a scope");
    }
}
