using MediatR;
using Mango.Services.Admin.Accounts.Application.DTOs;
using Mango.Services.Admin.Accounts.Application.Interfaces;

namespace Mango.Services.Admin.Accounts.Application.MediatR.Queries;

/// <summary>
/// Query to get an admin user by ID.
/// </summary>
public class GetAdminUserByIdQuery : BaseQuery<AdminUserDto>
{
    public int Id { get; set; }

    public GetAdminUserByIdQuery(int id)
    {
        Id = id;
    }
}

/// <summary>
/// Handler for GetAdminUserByIdQuery.
/// </summary>
public class GetAdminUserByIdQueryHandler : IRequestHandler<GetAdminUserByIdQuery, ResponseDto<AdminUserDto>>
{
    private readonly IAdminUserRepository _userRepository;

    public GetAdminUserByIdQueryHandler(IAdminUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<ResponseDto<AdminUserDto>> Handle(GetAdminUserByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
        {
            return new ResponseDto<AdminUserDto>
            {
                IsSuccess = false,
                Message = "Invalid admin user ID"
            };
        }

        try
        {
            var adminUser = await _userRepository.GetByIdAsync(request.Id);
            if (adminUser == null)
            {
                return new ResponseDto<AdminUserDto>
                {
                    IsSuccess = false,
                    Message = "Admin user not found"
                };
            }

            return new ResponseDto<AdminUserDto>
            {
                IsSuccess = true,
                Message = "Admin user retrieved successfully",
                Result = new AdminUserDto
                {
                    Id = adminUser.Id,
                    Email = adminUser.Email,
                    FullName = adminUser.FullName,
                    Role = adminUser.Role,
                    IsActive = adminUser.IsActive,
                    LastLoginAt = adminUser.LastLoginAt,
                    CreatedAt = adminUser.CreatedAt,
                    UpdatedAt = adminUser.UpdatedAt
                }
            };
        }
        catch (Exception ex)
        {
            return new ResponseDto<AdminUserDto>
            {
                IsSuccess = false,
                Message = $"Error retrieving admin user: {ex.Message}"
            };
        }
    }
}
