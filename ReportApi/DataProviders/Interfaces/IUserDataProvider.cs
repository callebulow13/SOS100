using ReportApi.DTOs.External;

namespace ReportApi.DataProviders.Interfaces;

public interface IUserDataProvider
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
}