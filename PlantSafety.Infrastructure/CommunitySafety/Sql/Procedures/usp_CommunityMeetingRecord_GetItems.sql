-- ============================================================================
--  Module:         CommunitySafety.CommunityMeetingRecord
--  File:           usp_CommunityMeetingRecord_GetItems.sql
--  Created:        2026-08-04
--  Description:    Read-only stored procedure backing
--                  ICommunityMeetingRecordRepository.GetCommunityMeetingRecordItemsAsync
--                  (Application read-model). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityMeetingRecord_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [CommunityMeetingRecordId],
        [TenantId],
        [SiteId],
        [MeetingTitle],
        [MeetingDateUtc],
        [Location],
        [AttendeeCount],
        [TopicsSummary],
        [ActionItems],
        [Status] AS [RecordStatus],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [CommunitySafety].[CommunityMeetingRecord]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ISNULL([IsDeleted], 0) = 0
      AND MeetingDateUtc >= @FromUtc
      AND MeetingDateUtc <= @ToUtc
    ORDER BY MeetingDateUtc DESC;
END
GO
