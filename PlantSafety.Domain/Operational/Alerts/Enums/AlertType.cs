// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertType.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  Description:    Smart Enum representing the canonical Alert Types used
//                  across the Alerts Module, Routing Module, Reporting, and
//                  SLA enforcement. Fully compliant with the Enterprise
//                  Architectural Rule Handbook and Alerts Playbook.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Enums
{
    public sealed class AlertType
    {
        public string Code { get; }
        public string DisplayName { get; }
        public string Category { get; }

        private AlertType(string code, string displayName, string category)
        {
            Code = code;
            DisplayName = displayName;
            Category = category;
        }

        // --------------------------------------------------------------------
        // Canonical Smart Enum Instances
        // --------------------------------------------------------------------

        public static readonly AlertType WorkerSafety =
            new("WorkerSafety", "Worker Safety", "Safety");

        public static readonly AlertType MachineFailure =
            new("MachineFailure", "Machine Failure", "Operations");

        public static readonly AlertType ProcessDeviation =
            new("ProcessDeviation", "Process Deviation", "Operations");

        public static readonly AlertType Environmental =
            new("Environmental", "Environmental Hazard", "Compliance");

        public static readonly AlertType PredictiveMaintenance =
            new("PredictiveMaintenance", "Predictive Maintenance", "Maintenance");

        public static readonly AlertType IncidentLinked =
            new("IncidentLinked", "Linked to Incident", "Safety");

        // --------------------------------------------------------------------
        // Registry
        // --------------------------------------------------------------------

        public static readonly IReadOnlyList<AlertType> All =
        [
            WorkerSafety,
            MachineFailure,
            ProcessDeviation,
            Environmental,
            PredictiveMaintenance,
            IncidentLinked
        ];

        public static AlertType Default => WorkerSafety;

        // --------------------------------------------------------------------
        // Parsing
        // --------------------------------------------------------------------

        public static bool TryParse(string? code, out AlertType result)
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

        public static AlertType From(string code)
        {
            if (!TryParse(code, out var result))
                throw new ArgumentException($"Invalid AlertType: '{code}'", nameof(code));

            return result;
        }

        public bool IsValid() => All.Contains(this);

        public override string ToString() => Code;
    }
}
