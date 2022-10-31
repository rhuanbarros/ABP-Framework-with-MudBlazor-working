using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using MaterialeShop.ListaItems;
using Volo.Abp.Content;
using MaterialeShop.Shared;

namespace MaterialeShop.Controllers.ListaItems
{
    [RemoteService]
    [Area("app")]
    [ControllerName("ListaItem")]
    [Route("api/app/lista-items")]

    public class ListaItemController : AbpController, IListaItemsAppService
    {
        private readonly IListaItemsAppService _listaItemsAppService;

        public ListaItemController(IListaItemsAppService listaItemsAppService)
        {
            _listaItemsAppService = listaItemsAppService;
        }

        [HttpGet]
        public virtual Task<PagedResultDto<ListaItemDto>> GetListAsync(GetListaItemsInput input)
        {
            return _listaItemsAppService.GetListAsync(input);
        }

        [HttpGet]
        [Route("{id}")]
        public virtual Task<ListaItemDto> GetAsync(Guid id)
        {
            return _listaItemsAppService.GetAsync(id);
        }

        [HttpPost]
        public virtual Task<ListaItemDto> CreateAsync(ListaItemCreateDto input)
        {
            return _listaItemsAppService.CreateAsync(input);
        }

        [HttpPut]
        [Route("{id}")]
        public virtual Task<ListaItemDto> UpdateAsync(Guid id, ListaItemUpdateDto input)
        {
            return _listaItemsAppService.UpdateAsync(id, input);
        }

        [HttpDelete]
        [Route("{id}")]
        public virtual Task DeleteAsync(Guid id)
        {
            return _listaItemsAppService.DeleteAsync(id);
        }

        [HttpGet]
        [Route("as-excel-file")]
        public virtual Task<IRemoteStreamContent> GetListAsExcelFileAsync(ListaItemExcelDownloadDto input)
        {
            return _listaItemsAppService.GetListAsExcelFileAsync(input);
        }

        [HttpGet]
        [Route("download-token")]
        public Task<DownloadTokenResultDto> GetDownloadTokenAsync()
        {
            return _listaItemsAppService.GetDownloadTokenAsync();
        }
    }
}