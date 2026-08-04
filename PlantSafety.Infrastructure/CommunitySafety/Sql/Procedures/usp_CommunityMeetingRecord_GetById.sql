-- ============================================================================
--  Module:         CommunitySafety.CommunityMeetingRecord
--  File:           usp_CommunityMeetingRecord_GetById.sql
--  Created:        2026-08-04
--  Description:    Returns a single CommunityMeetingRecord row by identifier, backing
--                  ICommunityMeetingRecordRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityMeetingRecord_GetById]
(
    @CommunityMeetingRecordId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [CommunityMeetingRecordId], [TenantId], [SiteId], [Status], [MeetingTitle], [MeetingDateUtc], [Location], [AttendeeCount], [TopicsSummary], [ActionItems], [RecordedByUserId], [CreatedAtUtc], [CreatedBy]
    FROM [CommunitySafety].[CommunityMeetingRecord]
    WHERE [CommunityMeetingRecordId] = @CommunityMeetingRecordId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
