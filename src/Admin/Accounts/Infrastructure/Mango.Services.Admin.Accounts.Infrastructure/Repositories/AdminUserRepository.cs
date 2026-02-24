using Microsoft.EntityFrameworkCore;
using Mango.Services.Admin.Accounts.Application.Interfaces;
using Mango.Services.Admin.Accounts.Domain.Entities;
using Mango.Services.Admin.Accounts.Infrastructure.Data;

namespace Mango.Services.Admin.Accounts.Infrastructure.Repositories;

/// <summary>
/// Repository for admin user data access.
/// </summary>
public class AdminUserRepository : IAdminUserRepository
{
    private readonly AdminAccountsDbContext _context;

    public AdminUserRepository(AdminAccountsDbContext context)
    {
        _context = context;
    }

    public async Task<AdminUser?> GetByIdAsync(int id)
    {
        return await _context.AdminUsers
            .Include(a => a.ApiKey)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<AdminUser?> GetByEmailAsync(string email)
    {
        return await _context.AdminUsers
            .Include(a => a.ApiKey)
            .FirstOrDefaultAsync(a => a.Email == email);
    }

    public async Task<(List<AdminUser>, int TotalCount)> GetAllAsync(int pageNumber, int pageSize)
    {
        var query = _context.AdminUsers.AsQueryable();
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(a => a.ApiKey)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<AdminUser>> GetActiveAsync()
    {
        return await _context.AdminUsers
            .Where(a => a.IsActive)
            .OrderBy(a => a.FullName)
            .Include(a => a.ApiKey)
            .ToListAsync();
    }

    public async Task AddAsync(AdminUser adminUser)
    {
        await _context.AdminUsers.AddAsync(adminUser);
    }

    public async Task UpdateAsync(AdminUser adminUser)
    {
        _context.AdminUsers.Update(adminUser);
        await Task.CompletedTask;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var adminUser = await GetByIdAsync(id);
        if (adminUser == null) return false;

        _context.AdminUsers.Remove(adminUser);
        return true;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
