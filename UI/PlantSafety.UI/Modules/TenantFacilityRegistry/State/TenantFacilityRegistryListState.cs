// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.TenantFacilityRegistry.State
//  File:           TenantFacilityRegistryListState.cs
//  Created:        2026-08-09
//  Description:    State container for the Tenant Facility Registry list page.
// ============================================================================
using PlantSafety.UI.Modules.TenantFacilityRegistry.Services;

namespace PlantSafety.UI.Modules.TenantFacilityRegistry.State;

/// <summary>State container for the Tenant Facility Registry list page.</summary>
public sealed class TenantFacilityRegistryListState(TenantFacilityRegistryUiService uiService) : IAsyncDisposable
{
    /// <summary>True while loading/writing.</summary>
    public bool IsLoading { get; private set; }

    /// <summary>True if the last operation failed.</summary>
    public bool HasError { get; private set; }

    /// <summary>The last operation's error message, or null.</summary>
    public string? Error { get; private set; }

    /// <summary>The current tenant/facility bindings.</summary>
    public IReadOnlyList<TenantFacilityMapDto> Bindings { get; private set; } = [];

    /// <summary>Loads every binding.</summary>
    public async Task LoadAsync()
    {
        IsLoading = true;
        HasError = false;
        Error = null;
        try
        {
            Bindings = await uiService.GetAllAsync();
        }
        catch (Exception ex)
        {
            HasError = true;
            Error = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Reloads every binding.</summary>
    public Task RefreshAsync() => LoadAsync();

    /// <summary>Registers a facility to a tenant, then reloads.</summary>
    public async Task RegisterAsync(string tenantId, Guid facilityId, bool isPrimary)
    {
        IsLoading = true;
        HasError = false;
        Error = null;
        try
        {
            await uiService.RegisterAsync(tenantId, facilityId, isPrimary);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            HasError = true;
            Error = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
