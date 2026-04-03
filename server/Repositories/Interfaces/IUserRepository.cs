using ProjectManagement.Entities;
using ProjectManagement.Enums;

namespace ProjectManagement.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
    Task<(IEnumerable<User> Items, int TotalCount)> GetFilteredPagedAsync(int pageNumber, int pageSize, UserRole? role);
}
