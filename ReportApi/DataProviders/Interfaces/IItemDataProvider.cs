using ReportApi.DTOs.External;

namespace ReportApi.DataProviders.Interfaces;

public interface IItemDataProvider
{
    Task<List<ItemDto>> GetAllItemsAsync();
    Task<ItemDto?> GetItemByIdAsync(int id);
}