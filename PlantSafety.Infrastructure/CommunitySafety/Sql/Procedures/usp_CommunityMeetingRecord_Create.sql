-- ============================================================================
--  Module:         CommunitySafety.CommunityMeetingRecord
--  File:           usp_CommunityMeetingRecord_Create.sql
--  Created:        2026-08-04
--  Description:    Insert procedure for [CommunitySafety].[CommunityMeetingRecord], backing
--                  ICommunityMeetingRecordRepository.AddAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityMeetingRecord_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @CommunityMeetingRecordId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @MeetingTitle NVARCHAR(300),
    @MeetingDateUtc DATETIME2(3),
    @Location NVARCHAR(300),
    @AttendeeCount INT,
    @TopicsSummary NVARCHAR(4000),
    @ActionItems NVARCHAR(4000) = NULL,
    @RecordedByUserId UNIQUEIDENTIFIER,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [CommunitySafety].[CommunityMeetingRecord]
            ([CommunityMeetingRecordId], [TenantId], [SiteId], [Status], [MeetingTitle], [MeetingDateUtc], [Location], [AttendeeCount], [TopicsSummary], [ActionItems], [RecordedByUserId], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@CommunityMeetingRecordId, @TenantId, @SiteId, @Status, @MeetingTitle, @MeetingDateUtc, @Location, @AttendeeCount, @TopicsSummary, @ActionItems, @RecordedByUserId, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
