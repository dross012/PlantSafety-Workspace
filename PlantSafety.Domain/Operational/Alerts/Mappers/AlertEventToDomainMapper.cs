// ============================================================================
//  File:           AlertEventToDomainMapper.cs
//  Created:        2026-05-26
//  Description:
//      Maps domain events into domain state mutations. Used by the
//      rehydration service to rebuild an Alert aggregate from events.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.Mappers;

public static class AlertEventToDomainMapper
{
    public static void Apply(Alert alert, IAlertDomainEvent evt)
    {
        switch (evt)
        {
            case AlertCreatedEvent e:
                alert.ApplyCreated(e);
                break;

            case AlertAcknowledgedEvent e:
                alert.ApplyAcknowledged(e);
                break;

            case AlertInProgressEvent e:
                alert.ApplyInProgress(e);
                break;

            case AlertEscalatedEvent e:
                alert.ApplyEscalated(e);
                break;

            case AlertResolvedEvent e:
                alert.ApplyResolved(e);
                break;

            case AlertClosedEvent e:
                alert.ApplyClosed(e);
                break;

            case AlertArchivedEvent e:
                alert.ApplyArchived(e);
                break;

            case AlertAssignedEvent e:
                alert.ApplyAssigned(e);
                break;

            case AlertMetadataAddedEvent e:
                alert.ApplyMetadataAdded(e);
                break;

            case AlertMetadataUpdatedEvent e:
                alert.ApplyMetadataUpdated(e);
                break;

            case AlertMetadataRemovedEvent e:
                alert.ApplyMetadataRemoved(e);
                break;

        }
    }
}
