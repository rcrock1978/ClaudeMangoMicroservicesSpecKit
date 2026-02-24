using Microsoft.EntityFrameworkCore;
using Mango.Services.Admin.Accounts.Application.Interfaces;
using Mango.Services.Admin.Accounts.Domain.Entities;
using Mango.Services.Admin.Accounts.Infrastructure.Data;

namespace Mango.Services.Admin.Accounts.Infrastructure.Repositories;

/// <summary>
/// Repository for API key data access.
/// </summary>
public class AdminApiKeyRepository : IAdminApiKeyRepository
{
    private readonly AdminAccountsDbContext _context;

    public AdminApiKeyRepository(AdminAccountsDbContext context)
    {
        _context = context;
    }

    public async Task<AdminApiKey?> GetByIdAsync(int id)
    {
        return await _context.AdminApiKeys
            .Include(a => a.AdminUser)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<AdminApiKey?> GetByKeyPrefixAsync(string keyPrefix)
    {
        return await _context.AdminApiKeys
            .Include(a => a.AdminUser)
            .FirstOrDefaultAsync(a => a.KeyPrefix == keyPrefix && !a.IsRevoked);
    }

    public async Task<AdminApiKey?> GetByAdminIdAsync(int adminId)
    {
        return await _context.AdminApiKeys
            .Include(a => a.AdminUser)
            .FirstOrDefaultAsync(a => a.AdminUserId == adminId && !a.IsRevoked);
    }

    public async Task AddAsync(AdminApiKey apiKey)
    {
        await _context.AdminApiKeys.AddAsync(apiKey);
    }

    public async Task UpdateAsync(AdminApiKey apiKey)
    {
        _context.AdminApiKeys.Update(apiKey);
        await Task.CompletedTask;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var apiKey = await GetByIdAsync(id);
        if (apiKey == null) return false;

        _context.AdminApiKeys.Remove(apiKey);
        return true;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
