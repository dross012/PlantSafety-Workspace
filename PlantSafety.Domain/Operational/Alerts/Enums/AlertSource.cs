// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertSource.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  Description:    Smart Enum representing the canonical sources that may
//                  generate alerts. Used for routing, reporting, and
//                  integration boundaries.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Enums
{
    public sealed class AlertSource
    {
        public string Code { get; }
        public string DisplayName { get; }

        private AlertSource(string code, string displayName)
        {
            Code = code;
            DisplayName = displayName;
        }

        // --------------------------------------------------------------------
        // Canonical Instances
        // --------------------------------------------------------------------

        public static readonly AlertSource WorkerModule =
            new("WorkerModule", "Worker Management");

        public static readonly AlertSource MachineModule =
            new("MachineModule", "Machine Management");

        public static readonly AlertSource ProcessModule =
            new("ProcessModule", "Process Management");

        public static readonly AlertSource PredictiveMaintenance =
            new("PredictiveMaintenance", "Predictive Maintenance");

        public static readonly AlertSource IncidentModule =
            new("IncidentModule", "Incident Management");

        public static readonly AlertSource SafetyPrograms =
            new("SafetyPrograms", "Safety Programs");

        // --------------------------------------------------------------------
        // Registry
        // --------------------------------------------------------------------

        public static readonly IReadOnlyList<AlertSource> All =
        [
            WorkerModule,
            MachineModule,
            ProcessModule,
            PredictiveMaintenance,
            IncidentModule,
            SafetyPrograms
        ];

        public static AlertSource Default => WorkerModule;

        // --------------------------------------------------------------------
        // Parsing
        // --------------------------------------------------------------------

        public static bool TryParse(string? code, out AlertSource result)
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

        public static AlertSource From(string code)
        {
            if (!TryParse(code, out var result))
                throw new ArgumentException($"Invalid AlertSource: '{code}'", nameof(code));

            return result;
        }

        public bool IsValid() => All.Contains(this);

        public override string ToString() => Code;
    }
}
