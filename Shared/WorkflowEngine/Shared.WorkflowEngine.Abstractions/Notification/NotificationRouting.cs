// ============================================================================
//  Project:        Shared.WorkflowEngine
//  Module:         Abstractions.Notification
//  File:           NotificationRouting.cs
//  Created:        2026-08-08
//  Description:    Phase C.7: a deterministic notification routing rule --
//                  which employee role should be notified and over which
//                  channel. Uses Shared.EmployeeDomain's EmployeeRole so
//                  routing stays consistent with the real role vocabulary.
// ============================================================================
using Shared.EmployeeDomain.Abstractions.Enums;

namespace Shared.WorkflowEngine.Abstractions.Notification;

/// <summary>How a notification is delivered.</summary>
public enum NotificationChannel
{
    /// <summary>Delivered via email.</summary>
    Email,

    /// <summary>Delivered via SMS text message.</summary>
    Sms,

    /// <summary>Delivered via an urgent paging/on-call system.</summary>
    Page,
}

/// <summary>A deterministic notification routing rule produced by a workflow evaluation.</summary>
public sealed record NotificationRouting(
    EmployeeRole RecipientRole,
    NotificationChannel Channel);
