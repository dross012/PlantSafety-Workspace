// ============================================================================
//  File:           AlertsBySourceModuleSpecification.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Specifications;

public sealed class AlertsBySourceModuleSpecification : AlertSpecification
{
    private readonly string _sourceModule;

    public AlertsBySourceModuleSpecification(string sourceModule)
        => _sourceModule = sourceModule.Trim();

    public override Expression<Func<Alert, bool>> ToExpression()
        => alert => alert.SourceModule == _sourceModule;
}
