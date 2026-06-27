// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertSeverity.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  Description:    Smart Enum representing canonical severity levels for
//                  alerts. Used for routing, escalation, SLA enforcement,
//                  dashboards, and reporting.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Enums
{
    public sealed class AlertSeverity
    {
        public int Level { get; }
        public string Code { get; }
        public string DisplayName { get; }

        private AlertSeverity(int level, string code, string displayName)
        {
            Level = level;
            Code = code;
            DisplayName = displayName;
        }

        // --------------------------------------------------------------------
        // Canonical Instances
        // --------------------------------------------------------------------

        public static readonly AlertSeverity Low =
            new(1, "Low", "Low Severity");

        public static readonly AlertSeverity Medium =
            new(2, "Medium", "Medium Severity");

        public static readonly AlertSeverity High =
            new(3, "High", "High Severity");

        public static readonly AlertSeverity Critical =
            new(4, "Critical", "Critical Severity");

        public static readonly AlertSeverity Emergency =
            new(5, "Emergency", "Emergency Severity");

        // --------------------------------------------------------------------
        // Registry
        // --------------------------------------------------------------------

        public static readonly IReadOnlyList<AlertSeverity> All =
        [
            Low, Medium, High, Critical, Emergency
        ];

        public static AlertSeverity Default => Medium;

        // --------------------------------------------------------------------
        // Parsing
        // --------------------------------------------------------------------

        public static bool TryParse(string? code, out AlertSeverity result)
        {
            result = Default;

            if (string.IsNullOrWhiteSpace(code))
                return false;

            var match = All.FirstOrDefault(x =>
                x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

            if (match is null)
                return false;

            result = match;
            return true;
        }

        public static AlertSeverity From(string code)
        {
            if (!TryParse(code, out var result))
                throw new ArgumentException($"Invalid AlertSeverity: '{code}'", nameof(code));

            return result;
        }

        public bool IsValid() => All.Contains(this);

        public override string ToString() => Code;
    }
}
