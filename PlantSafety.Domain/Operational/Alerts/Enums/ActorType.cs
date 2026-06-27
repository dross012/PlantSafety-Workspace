// ============================================================================
//  File:           ActorType.cs
//  Created:        2026-05-26
//  Description:    Smart Enum representing canonical actor types.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Enums;

public sealed class ActorType
{
    public string Code { get; }
    public string DisplayName { get; }

    private ActorType(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    // Canonical Instances
    public static readonly ActorType User = new("User", "Human User");
    public static readonly ActorType System = new("System", "System Actor");
    public static readonly ActorType Workflow = new("Workflow", "Workflow Engine");
    public static readonly ActorType Sensor = new("Sensor", "Hardware Sensor");
    public static readonly ActorType External = new("External", "External System");

    public static readonly IReadOnlyList<ActorType> All =
    [
        User, System, Workflow, Sensor, External
    ];

    public static ActorType From(string code)
        => All.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
           ?? throw new ArgumentException($"Invalid ActorType: {code}");

    public void EnsureValidCombination(
        UserId? userId,
        SystemCode? systemCode,
        DeviceId? deviceId)
    {
        switch (Code)
        {
            case "User":
                if (userId is null)
                    throw new InvalidOperationException("User actor requires UserId.");
                break;

            case "System":
            case "Workflow":
            case "External":
                if (systemCode is null)
                    throw new InvalidOperationException($"{Code} actor requires SystemCode.");
                break;

            case "Sensor":
                if (deviceId is null)
                    throw new InvalidOperationException("Sensor actor requires DeviceId.");
                break;
        }
    }

    public override string ToString() => Code;
}
