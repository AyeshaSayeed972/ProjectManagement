namespace ProjectManagement.DAL;

public interface IUserRepository
{
    System.Threading.Tasks.Task<User?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<bool> ExistsAsync(int id);
    System.Threading.Tasks.Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
    System.Threading.Tasks.Task<(IEnumerable<User> Items, int TotalCount)> GetFilteredPagedAsync(int pageNumber, int pageSize, UserRole? role);
}
