-- ============================================================================
--  Module:         CommunitySafety.CommunityNotification
--  File:           usp_CommunityNotification_GetItems.sql
--  Created:        2026-08-04
--  Description:    Read-only stored procedure backing
--                  ICommunityNotificationRepository.GetCommunityNotificationItemsAsync
--                  (Application read-model for ListNotificationsByPeriod query / Notification
--                  History & Reach dashboard). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityNotification_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [CommunityNotificationId],
        [TenantId],
        [SiteId],
        [Channel],
        [RecipientContactId],
        [Subject],
        [RelatedConcernId],
        [RelatedIncidentId],
        [Status] AS [NotificationStatus],
        [SentAtUtc],
        [AcknowledgedAtUtc],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [CommunitySafety].[CommunityNotification]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ISNULL([IsDeleted], 0) = 0
      AND SentAtUtc >= @FromUtc
      AND SentAtUtc <= @ToUtc
    ORDER BY SentAtUtc DESC;
END
GO
