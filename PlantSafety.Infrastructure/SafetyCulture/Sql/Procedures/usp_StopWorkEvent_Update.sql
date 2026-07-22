-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyCulture.StopWorkEvent
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_StopWorkEvent_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [SafetyCulture].[StopWorkEvent], backing
--                  IStopWorkEventRepository.UpdateAsync (Investigate/Resolve lifecycle
--                  transitions). Standard error envelope: TRY/CATCH with transaction
--                  rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyCulture].[usp_StopWorkEvent_Update]
    @TenantId               UNIQUEIDENTIFIER,
    @CorrelationId          UNIQUEIDENTIFIER,
    @ActorId                UNIQUEIDENTIFIER,
    @NowUtc                 DATETIME2(3),
    @StopWorkEventId        UNIQUEIDENTIFIER,
    @Status                 NVARCHAR(20),
    @ResolutionSummary      NVARCHAR(2000) = NULL,
    @WasCelebrated          BIT,
    @InvestigationFindings  NVARCHAR(2000) = NULL,
    @InvestigatedAtUtc      DATETIME2(3) = NULL,
    @ResolvedAtUtc          DATETIME2(3) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [SafetyCulture].[StopWorkEvent]
        SET [Status]                = @Status,
            [ResolutionSummary]     = @ResolutionSummary,
            [WasCelebrated]         = @WasCelebrated,
            [InvestigationFindings] = @InvestigationFindings,
            [InvestigatedAtUtc]     = @InvestigatedAtUtc,
            [ResolvedAtUtc]         = @ResolvedAtUtc,
            [LastModifiedAtUtc]     = @NowUtc
        WHERE [StopWorkEventId] = @StopWorkEventId
          AND [TenantId]        = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
