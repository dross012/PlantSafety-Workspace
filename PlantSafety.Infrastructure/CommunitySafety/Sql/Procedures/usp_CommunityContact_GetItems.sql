-- ============================================================================
--  Module:         CommunitySafety.CommunityContact
--  File:           usp_CommunityContact_GetItems.sql
--  Created:        2026-08-04
--  Description:    Read-only stored procedure backing
--                  ICommunityContactRepository.GetCommunityContactItemsAsync (Application
--                  read-model). PII-minimal projection per the data-governance mandate.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityContact_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [CommunityContactId],
        [TenantId],
        [SiteId],
        [DisplayName],
        [ContactType],
        [Organization],
        [CommunityArea],
        [OptedInForNotifications],
        [Status],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [CommunitySafety].[CommunityContact]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ISNULL([IsDeleted], 0) = 0
      AND CreatedAtUtc >= @FromUtc
      AND CreatedAtUtc <= @ToUtc
    ORDER BY CreatedAtUtc DESC;
END
GO
