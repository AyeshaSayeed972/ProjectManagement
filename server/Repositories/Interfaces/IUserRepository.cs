using ProjectManagement.Entities;

namespace ProjectManagement.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
}
