using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.DataGrid;
using Volo.Abp.BlazoriseUI.Components;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Components.Web.Theming.PageToolbars;
using MaterialeShop.Listas;
using MaterialeShop.Permissions;
using MaterialeShop.Shared;
using MudBlazor;
using Microsoft.Extensions.Logging;

namespace MaterialeShop.Blazor.Pages.Cliente;

public partial class ListasPage
{
    protected List<Volo.Abp.BlazoriseUI.BreadcrumbItem> BreadcrumbItems = new List<Volo.Abp.BlazoriseUI.BreadcrumbItem>();
    protected PageToolbar Toolbar { get; } = new PageToolbar();
    // private IReadOnlyList<ListaWithNavigationPropertiesDto> ListaList { get; set; }
    private MudDataGrid<ListaWithNavigationPropertiesDto> _listaList;
    private int PageSize { get; } = LimitedResultRequestDto.DefaultMaxResultCount;
    private int CurrentPage { get; set; } = 1;
    private string CurrentSorting { get; set; }
    private int TotalCount { get; set; }
    private bool CanCreateLista { get; set; }
    private bool CanEditLista { get; set; }
    private bool CanDeleteLista { get; set; }
    private ListaCreateDto NewLista { get; set; }
    private Validations NewListaValidations { get; set; }
    private ListaUpdateDto EditingLista { get; set; }
    private Validations EditingListaValidations { get; set; }
    private Guid EditingListaId { get; set; }
    private Modal CreateListaModal { get; set; }
    private Modal EditListaModal { get; set; }
    private GetListasInput Filter { get; set; }
    private DataGridEntityActionsColumn<ListaWithNavigationPropertiesDto> EntityActionsColumn { get; set; }
    protected string SelectedCreateTab = "lista-create-tab";
    protected string SelectedEditTab = "lista-edit-tab";
    private IReadOnlyList<LookupDto<Guid?>> EnderecosNullable { get; set; } = new List<LookupDto<Guid?>>();
    private readonly ILogger<ListasPage> _logger;

    public ListasPage(ILogger<ListasPage> logger)
    {
        NewLista = new ListaCreateDto();
        EditingLista = new ListaUpdateDto();
        Filter = new GetListasInput
        {
            MaxResultCount = PageSize,
            SkipCount = (CurrentPage - 1) * PageSize,
            Sorting = CurrentSorting
        };
        this._logger = logger;
    }

    protected override async Task OnInitializedAsync()
    {
        await SetToolbarItemsAsync();
        await SetBreadcrumbItemsAsync();
        await SetPermissionsAsync();
        await GetNullableEnderecoLookupAsync();


    }

    private bool onParametersSet = false;
    

    protected override void OnParametersSet()
    {
        onParametersSet = true;
        _logger.LogWarning("----onParametersSet true");
    }

    protected virtual ValueTask SetBreadcrumbItemsAsync()
    {
        BreadcrumbItems.Add(new Volo.Abp.BlazoriseUI.BreadcrumbItem(L["Menu:Listas"]));
        return ValueTask.CompletedTask;
    }

    protected virtual ValueTask SetToolbarItemsAsync()
    {
        Toolbar.AddButton(L["ExportToExcel"], async () => { await DownloadAsExcelAsync(); }, IconName.Download);

        Toolbar.AddButton(L["NewLista"], async () =>
        {
            await OpenCreateListaModalAsync();
        }, 
        IconName.Add, 
        requiredPolicyName: null //MaterialeShopPermissions.Listas.Create
        );

        return ValueTask.CompletedTask;
    }

    private async Task SetPermissionsAsync()
    {
        CanCreateLista = await AuthorizationService
            .IsGrantedAsync(MaterialeShopPermissions.Listas.Create);
        CanEditLista = await AuthorizationService
                        .IsGrantedAsync(MaterialeShopPermissions.Listas.Edit);
        CanDeleteLista = await AuthorizationService
                        .IsGrantedAsync(MaterialeShopPermissions.Listas.Delete);
    }

    // private async Task GetListasAsync()
    // {
    //     Filter.MaxResultCount = PageSize;
    //     Filter.SkipCount = (CurrentPage - 1) * PageSize;
    //     Filter.Sorting = CurrentSorting;

    //     var result = await ListasAppService.GetListAsync(Filter);
    //     _listaList = result.Items;
    //     TotalCount = (int)result.TotalCount;
    // }

    protected virtual async Task SearchAsync()
    {
        CurrentPage = 1;
        await _listaList.ReloadServerData();
        await InvokeAsync(StateHasChanged);
    }

    private async Task DownloadAsExcelAsync()
    {
        var token = (await ListasAppService.GetDownloadTokenAsync()).Token;
        var remoteService = await RemoteServiceConfigurationProvider.GetConfigurationOrDefaultAsync("Default");
        NavigationManager.NavigateTo($"{remoteService.BaseUrl.EnsureEndsWith('/')}api/app/listas/as-excel-file?DownloadToken={token}&FilterText={Filter.FilterText}", forceLoad: true);
    }

    private async Task<GridData<ListaWithNavigationPropertiesDto>> LoadServerData(GridState<ListaWithNavigationPropertiesDto> state)
    {
        Filter.MaxResultCount = PageSize;
        Filter.SkipCount = (CurrentPage - 1) * PageSize;
        Filter.Sorting = CurrentSorting;

        var result = await ListasAppService.GetListAsync(Filter);

        return new()
        {
            Items = result.Items,
            TotalItems = (int)result.TotalCount
        };
    }


    // private async Task OnDataGridReadAsync(DataGridReadDataEventArgs<ListaWithNavigationPropertiesDto> e)
    // {
    //     CurrentSorting = e.Columns
    //         .Where(c => c.SortDirection != SortDirection.Default)
    //         .Select(c => c.Field + (c.SortDirection == SortDirection.Descending ? " DESC" : ""))
    //         .JoinAsString(",");
    //     CurrentPage = e.Page;
    // await GetListasAsync();
    //     await InvokeAsync(StateHasChanged);
    // }

    private async Task OpenCreateListaModalAsync()
    {
        NewLista = new ListaCreateDto
        {
        };
        await NewListaValidations.ClearAll();
        await CreateListaModal.Show();
    }

    private async Task CloseCreateListaModalAsync()
    {
        NewLista = new ListaCreateDto
        {
        };
        await CreateListaModal.Hide();
    }

    private async Task OpenEditListaModalAsync(ListaWithNavigationPropertiesDto input)
    {
        var lista = await ListasAppService.GetWithNavigationPropertiesAsync(input.Lista.Id);

        EditingListaId = lista.Lista.Id;
        EditingLista = ObjectMapper.Map<ListaDto, ListaUpdateDto>(lista.Lista);
        await EditingListaValidations.ClearAll();
        await EditListaModal.Show();
    }

    private async Task DeleteListaAsync(ListaWithNavigationPropertiesDto input)
    {
        await ListasAppService.DeleteAsync(input.Lista.Id);
        await _listaList.ReloadServerData();
    }

    private async Task CreateListaAsync()
    {
        try
        {
            if (await NewListaValidations.ValidateAll() == false)
            {
                return;
            }

            await ListasAppService.CreateAsync(NewLista);
            await _listaList.ReloadServerData();
            await CloseCreateListaModalAsync();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    private async Task CloseEditListaModalAsync()
    {
        await EditListaModal.Hide();
    }

    private async Task UpdateListaAsync()
    {
        try
        {
            if (await EditingListaValidations.ValidateAll() == false)
            {
                return;
            }

            await ListasAppService.UpdateAsync(EditingListaId, EditingLista);
            await _listaList.ReloadServerData();
            await EditListaModal.Hide();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    private void OnSelectedCreateTabChanged(string name)
    {
        SelectedCreateTab = name;
    }

    private void OnSelectedEditTabChanged(string name)
    {
        SelectedEditTab = name;
    }


    private async Task GetNullableEnderecoLookupAsync(string newValue = null)
    {
        EnderecosNullable = (await ListasAppService.GetEnderecoLookupAsync(new LookupRequestDto { Filter = newValue })).Items;
    }


}
