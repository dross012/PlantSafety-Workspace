-- ============================================================================
--  Module:         CommunitySafety.CommunityContact
--  File:           usp_CommunityContact_Update.sql
--  Created:        2026-08-04
--  Description:    Update procedure for [CommunitySafety].[CommunityContact] lifecycle fields,
--                  backing ICommunityContactRepository.UpdateAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityContact_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @CommunityContactId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @OptedInForNotifications BIT,
    @ArchivedAtUtc DATETIME2(3) = NULL,
    @ArchivalReason NVARCHAR(1000) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [CommunitySafety].[CommunityContact]
        SET
            [Status] = @Status,
            [OptedInForNotifications] = @OptedInForNotifications,
            [ArchivedAtUtc] = @ArchivedAtUtc,
            [ArchivalReason] = @ArchivalReason,
            [ModifiedAtUtc] = @NowUtc,
            [ModifiedBy] = @ActorId,
            [CorrelationId] = @CorrelationId
        WHERE [CommunityContactId] = @CommunityContactId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
