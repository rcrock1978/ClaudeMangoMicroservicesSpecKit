using MediatR;
using Mango.Services.Admin.Accounts.Application.DTOs;
using Mango.Services.Admin.Accounts.Application.Interfaces;

namespace Mango.Services.Admin.Accounts.Application.MediatR.Queries;

/// <summary>
/// Query to get all admin users with pagination.
/// </summary>
public class GetAllAdminUsersQuery : BaseQuery<PaginatedResponse<AdminUserDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Handler for GetAllAdminUsersQuery.
/// </summary>
public class GetAllAdminUsersQueryHandler : IRequestHandler<GetAllAdminUsersQuery, ResponseDto<PaginatedResponse<AdminUserDto>>>
{
    private readonly IAdminUserRepository _userRepository;

    public GetAllAdminUsersQueryHandler(IAdminUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<ResponseDto<PaginatedResponse<AdminUserDto>>> Handle(GetAllAdminUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate pagination parameters
            var pageNumber = Math.Max(1, request.PageNumber);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);

            // Get paginated data (includes total count)
            var (adminUsers, totalCount) = await _userRepository.GetAllAsync(pageNumber, pageSize);

            var userDtos = adminUsers.Select(u => new AdminUserDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Role = u.Role,
                IsActive = u.IsActive,
                LastLoginAt = u.LastLoginAt,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            }).ToList();

            var result = new PaginatedResponse<AdminUserDto>
            {
                Items = userDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return new ResponseDto<PaginatedResponse<AdminUserDto>>
            {
                IsSuccess = true,
                Message = "Admin users retrieved successfully",
                Result = result
            };
        }
        catch (Exception ex)
        {
            return new ResponseDto<PaginatedResponse<AdminUserDto>>
            {
                IsSuccess = false,
                Message = $"Error retrieving admin users: {ex.Message}"
            };
        }
    }
}
