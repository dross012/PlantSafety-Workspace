-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.Ppe
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Ppe_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [EmployeeSafety].[Ppe], backing
--                  IPpeRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_Ppe_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @PpeId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @ItemDescription NVARCHAR(2000),
    @Category NVARCHAR(100),
    @AssignedZone NVARCHAR(400),
    @IssuedToUserId UNIQUEIDENTIFIER,
    @IssuedAtUtc DATETIME2(3),
    @RequiresFitTest BIT,
    @FitTestedAtUtc DATETIME2(3) = NULL,
    @ReplacementCycleDays INT,
    @NextReplacementDueAtUtc DATETIME2(3) = NULL,
    @LastInspectedAtUtc DATETIME2(3) = NULL,
    @ConditionScore DECIMAL(9,2) = NULL,
    @ReplacementReason NVARCHAR(2000) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [EmployeeSafety].[Ppe]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [ItemDescription] = @ItemDescription,
        [Category] = @Category,
        [AssignedZone] = @AssignedZone,
        [IssuedToUserId] = @IssuedToUserId,
        [IssuedAtUtc] = @IssuedAtUtc,
        [RequiresFitTest] = @RequiresFitTest,
        [FitTestedAtUtc] = @FitTestedAtUtc,
        [ReplacementCycleDays] = @ReplacementCycleDays,
        [NextReplacementDueAtUtc] = @NextReplacementDueAtUtc,
        [LastInspectedAtUtc] = @LastInspectedAtUtc,
        [ConditionScore] = @ConditionScore,
        [ReplacementReason] = @ReplacementReason,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [PpeId] = @PpeId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
