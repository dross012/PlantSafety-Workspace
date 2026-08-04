-- ============================================================================
--  Module:         CommunitySafety.CommunityNotification
--  File:           usp_CommunityNotification_GetById.sql
--  Created:        2026-08-04
--  Description:    Returns a single CommunityNotification row by identifier, backing
--                  ICommunityNotificationRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityNotification_GetById]
(
    @CommunityNotificationId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [CommunityNotificationId], [TenantId], [SiteId], [Status], [Channel], [RecipientContactId], [Subject], [MessageBody], [RelatedConcernId], [RelatedIncidentId], [SentAtUtc], [AcknowledgedAtUtc], [FailureReason], [CreatedAtUtc], [CreatedBy]
    FROM [CommunitySafety].[CommunityNotification]
    WHERE [CommunityNotificationId] = @CommunityNotificationId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
