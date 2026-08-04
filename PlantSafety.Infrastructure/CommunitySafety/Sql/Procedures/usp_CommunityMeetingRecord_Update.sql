-- ============================================================================
--  Module:         CommunitySafety.CommunityMeetingRecord
--  File:           usp_CommunityMeetingRecord_Update.sql
--  Created:        2026-08-04
--  Description:    Update procedure for [CommunitySafety].[CommunityMeetingRecord] lifecycle
--                  fields, backing ICommunityMeetingRecordRepository.UpdateAsync. Additive-only
--                  evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityMeetingRecord_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @CommunityMeetingRecordId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50)
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [CommunitySafety].[CommunityMeetingRecord]
        SET
            [Status] = @Status,
            [ModifiedAtUtc] = @NowUtc,
            [ModifiedBy] = @ActorId,
            [CorrelationId] = @CorrelationId
        WHERE [CommunityMeetingRecordId] = @CommunityMeetingRecordId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
