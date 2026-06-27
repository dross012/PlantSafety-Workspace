// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertStatus.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  Description:    Smart Enum representing the canonical lifecycle states for
//                  alerts. Fully aligned with the Alerts Module Playbook
//                  lifecycle (Created → Acknowledged → InProgress → Escalated
//                  → Resolved → Closed → Archived).
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Enums
{
    public sealed class AlertStatus
    {
        public int Order { get; }
        public string Code { get; }
        public string DisplayName { get; }

        private AlertStatus(int order, string code, string displayName)
        {
            Order = order;
            Code = code;
            DisplayName = displayName;
        }

        // --------------------------------------------------------------------
        // Canonical Instances
        // --------------------------------------------------------------------

        public static readonly AlertStatus Created =
            new(1, "Created", "Created");

        public static readonly AlertStatus Acknowledged =
            new(2, "Acknowledged", "Acknowledged");

        public static readonly AlertStatus InProgress =
            new(3, "InProgress", "In Progress");

        public static readonly AlertStatus Escalated =
            new(4, "Escalated", "Escalated");

        public static readonly AlertStatus Resolved =
            new(5, "Resolved", "Resolved");

        public static readonly AlertStatus Closed =
            new(6, "Closed", "Closed");

        public static readonly AlertStatus Archived =
            new(7, "Archived", "Archived");

        // --------------------------------------------------------------------
        // Registry
        // --------------------------------------------------------------------

        public static readonly IReadOnlyList<AlertStatus> All =
        [
            Created,
            Acknowledged,
            InProgress,
            Escalated,
            Resolved,
            Closed,
            Archived
        ];

        public static AlertStatus Default => Created;

        // --------------------------------------------------------------------
        // Parsing
        // --------------------------------------------------------------------

        public static bool TryParse(string? code, out AlertStatus result)
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

        public static AlertStatus From(string code)
        {
            if (!TryParse(code, out var result))
                throw new ArgumentException($"Invalid AlertStatus: '{code}'", nameof(code));

            return result;
        }

        public bool IsValid() => All.Contains(this);

        public override string ToString() => Code;
    }
}
