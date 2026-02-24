using Mango.Services.Admin.Accounts.Domain.Entities;

namespace Mango.Services.Admin.Accounts.Application.Interfaces;

/// <summary>
/// Repository interface for admin user data access.
/// </summary>
public interface IAdminUserRepository
{
    Task<AdminUser?> GetByIdAsync(int id);
    Task<AdminUser?> GetByEmailAsync(string email);
    Task<(List<AdminUser>, int TotalCount)> GetAllAsync(int pageNumber, int pageSize);
    Task<List<AdminUser>> GetActiveAsync();
    Task AddAsync(AdminUser adminUser);
    Task UpdateAsync(AdminUser adminUser);
    Task<bool> DeleteAsync(int id);
    Task SaveChangesAsync();
}
