-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.Ppe
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Ppe_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [EmployeeSafety].[Ppe], backing
--                  IPpeRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_Ppe_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
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
        INSERT INTO [EmployeeSafety].[Ppe]
            ([PpeId], [TenantId], [SiteId], [Status], [ItemDescription], [Category], [AssignedZone], [IssuedToUserId], [IssuedAtUtc], [RequiresFitTest], [FitTestedAtUtc], [ReplacementCycleDays], [NextReplacementDueAtUtc], [LastInspectedAtUtc], [ConditionScore], [ReplacementReason], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@PpeId, @TenantId, @SiteId, @Status, @ItemDescription, @Category, @AssignedZone, @IssuedToUserId, @IssuedAtUtc, @RequiresFitTest, @FitTestedAtUtc, @ReplacementCycleDays, @NextReplacementDueAtUtc, @LastInspectedAtUtc, @ConditionScore, @ReplacementReason, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
