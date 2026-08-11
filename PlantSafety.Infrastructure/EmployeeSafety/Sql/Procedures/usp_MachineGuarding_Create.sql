-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.MachineGuarding
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_MachineGuarding_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [EmployeeSafety].[MachineGuarding], backing
--                  IMachineGuardingRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_MachineGuarding_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @MachineGuardingId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @GuardTitle NVARCHAR(400),
    @GuardType NVARCHAR(100),
    @EquipmentTag NVARCHAR(400),
    @Location NVARCHAR(400),
    @HasInterlock BIT,
    @InspectionIntervalDays INT,
    @LastInspectedByUserId UNIQUEIDENTIFIER = NULL,
    @LastInspectedAtUtc DATETIME2(3) = NULL,
    @NextInspectionDueAtUtc DATETIME2(3) = NULL,
    @OpenDeficiencyCount INT,
    @GuardConditionScore DECIMAL(9,2) = NULL,
    @BypassDetected BIT,
    @LastFindingNotes NVARCHAR(2000) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [EmployeeSafety].[MachineGuarding]
            ([MachineGuardingId], [TenantId], [SiteId], [Status], [GuardTitle], [GuardType], [EquipmentTag], [Location], [HasInterlock], [InspectionIntervalDays], [LastInspectedByUserId], [LastInspectedAtUtc], [NextInspectionDueAtUtc], [OpenDeficiencyCount], [GuardConditionScore], [BypassDetected], [LastFindingNotes], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@MachineGuardingId, @TenantId, @SiteId, @Status, @GuardTitle, @GuardType, @EquipmentTag, @Location, @HasInterlock, @InspectionIntervalDays, @LastInspectedByUserId, @LastInspectedAtUtc, @NextInspectionDueAtUtc, @OpenDeficiencyCount, @GuardConditionScore, @BypassDetected, @LastFindingNotes, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
        -- Canonical audit trail (Handbook §2.11): the write is recorded after it commits, in its
        -- own TRY/CATCH so an audit failure can never fail the operation it is describing.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'MachineGuarding', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'MachineGuardingId=', @MachineGuardingId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'MachineGuardingId=', @MachineGuardingId, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

    THROW;
END CATCH;
GO
