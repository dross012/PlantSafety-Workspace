-- ============================================================================
--  Module:         CommunitySafety.CommunityConcern
--  File:           usp_CommunityConcern_Update.sql
--  Created:        2026-08-04
--  Description:    Update procedure for [CommunitySafety].[CommunityConcern] lifecycle fields,
--                  backing ICommunityConcernRepository.UpdateAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityConcern_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @CommunityConcernId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @Severity NVARCHAR(30),
    @EscalationCount INT,
    @EscalationReason NVARCHAR(1000) = NULL,
    @EscalatedAtUtc DATETIME2(3) = NULL,
    @ResolutionNotes NVARCHAR(4000) = NULL,
    @ResolvedAtUtc DATETIME2(3) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [CommunitySafety].[CommunityConcern]
        SET
            [Status] = @Status,
            [Severity] = @Severity,
            [EscalationCount] = @EscalationCount,
            [EscalationReason] = @EscalationReason,
            [EscalatedAtUtc] = @EscalatedAtUtc,
            [ResolutionNotes] = @ResolutionNotes,
            [ResolvedAtUtc] = @ResolvedAtUtc,
            [ModifiedAtUtc] = @NowUtc,
            [ModifiedBy] = @ActorId,
            [CorrelationId] = @CorrelationId
        WHERE [CommunityConcernId] = @CommunityConcernId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
