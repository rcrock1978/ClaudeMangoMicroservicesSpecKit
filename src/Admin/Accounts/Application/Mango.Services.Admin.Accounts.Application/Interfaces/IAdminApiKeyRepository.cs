using Mango.Services.Admin.Accounts.Domain.Entities;

namespace Mango.Services.Admin.Accounts.Application.Interfaces;

/// <summary>
/// Repository interface for API key data access.
/// </summary>
public interface IAdminApiKeyRepository
{
    Task<AdminApiKey?> GetByIdAsync(int id);
    Task<AdminApiKey?> GetByKeyPrefixAsync(string keyPrefix);
    Task<AdminApiKey?> GetByAdminIdAsync(int adminId);
    Task AddAsync(AdminApiKey apiKey);
    Task UpdateAsync(AdminApiKey apiKey);
    Task<bool> DeleteAsync(int id);
    Task SaveChangesAsync();
}
