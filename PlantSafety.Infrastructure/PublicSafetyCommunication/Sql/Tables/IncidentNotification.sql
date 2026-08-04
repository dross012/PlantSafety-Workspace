-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PublicSafetyCommunication
--  Layer:          Infrastructure -- SQL Scripts
--  File:           IncidentNotification.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the IncidentNotification aggregate root
--                  (IIncidentNotificationRepository). Tenant-scoped. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IncidentNotification' AND schema_id = SCHEMA_ID('PublicSafetyCommunication'))
BEGIN
    CREATE TABLE [PublicSafetyCommunication].[IncidentNotification]
    (
        IncidentNotificationId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_IncidentNotification PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        IncidentId UNIQUEIDENTIFIER NULL,
        PublicSafetyAgencyId UNIQUEIDENTIFIER NOT NULL,
        ChannelCode NVARCHAR(64) NOT NULL,
        Message NVARCHAR(MAX) NOT NULL,
        DispatchedAtUtc DATETIME2(3) NULL,
        DispatchedBy UNIQUEIDENTIFIER NULL,
        AcknowledgedAtUtc DATETIME2(3) NULL,
        AcknowledgedByContactName NVARCHAR(400) NULL,
        ClosedAtUtc DATETIME2(3) NULL,
        FailureReason NVARCHAR(400) NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_IncidentNotification_IsDeleted DEFAULT (0),
        RowVersion ROWVERSION NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_IncidentNotification_TenantId_Site ON [PublicSafetyCommunication].[IncidentNotification] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_IncidentNotification_IncidentId ON [PublicSafetyCommunication].[IncidentNotification] (IncidentId);
    CREATE NONCLUSTERED INDEX IX_IncidentNotification_Status ON [PublicSafetyCommunication].[IncidentNotification] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_IncidentNotification_PublicSafetyAgencyId ON [PublicSafetyCommunication].[IncidentNotification] (PublicSafetyAgencyId);
END
GO
