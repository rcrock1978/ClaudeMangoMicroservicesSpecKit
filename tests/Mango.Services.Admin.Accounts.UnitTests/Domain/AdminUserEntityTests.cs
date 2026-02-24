using FluentAssertions;
using Mango.Services.Admin.Accounts.Domain.Entities;

namespace Mango.Services.Admin.Accounts.UnitTests.Domain;

public class AdminUserEntityTests
{
    [Fact]
    public void IsValid_WithValidData_ReturnsTrue()
    {
        // Arrange
        var adminUser = new AdminUser
        {
            Id = 1,
            Email = "admin@test.com",
            FullName = "Test Admin",
            Role = AdminRole.SUPER_ADMIN,
            IsActive = true
        };

        // Act
        var result = adminUser.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WhenInactive_ReturnsFalse()
    {
        // Arrange
        var adminUser = new AdminUser
        {
            Id = 1,
            Email = "admin@test.com",
            FullName = "Test Admin",
            Role = AdminRole.SUPER_ADMIN,
            IsActive = false
        };

        // Act
        var result = adminUser.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyEmail_ReturnsFalse()
    {
        // Arrange
        var adminUser = new AdminUser
        {
            Id = 1,
            Email = "",
            FullName = "Test Admin",
            Role = AdminRole.SUPER_ADMIN,
            IsActive = true
        };

        // Act
        var result = adminUser.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithShortFullName_ReturnsFalse()
    {
        // Arrange
        var adminUser = new AdminUser
        {
            Id = 1,
            Email = "admin@test.com",
            FullName = "A",
            Role = AdminRole.SUPER_ADMIN,
            IsActive = true
        };

        // Act
        var result = adminUser.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanAccess_SuperAdminAccessingAnyRole_ReturnsTrue()
    {
        // Arrange
        var adminUser = new AdminUser
        {
            Id = 1,
            Email = "admin@test.com",
            FullName = "Super Admin",
            Role = AdminRole.SUPER_ADMIN,
            IsActive = true
        };

        // Act & Assert
        adminUser.CanAccess(AdminRole.SUPER_ADMIN).Should().BeTrue();
        adminUser.CanAccess(AdminRole.ADMIN).Should().BeTrue();
        adminUser.CanAccess(AdminRole.MODERATOR).Should().BeTrue();
    }

    [Fact]
    public void CanAccess_AdminAccessingAdminRole_ReturnsTrue()
    {
        // Arrange
        var adminUser = new AdminUser
        {
            Id = 1,
            Email = "admin@test.com",
            FullName = "Admin User",
            Role = AdminRole.ADMIN,
            IsActive = true
        };

        // Act & Assert
        adminUser.CanAccess(AdminRole.ADMIN).Should().BeTrue();
        adminUser.CanAccess(AdminRole.MODERATOR).Should().BeTrue();
        adminUser.CanAccess(AdminRole.SUPER_ADMIN).Should().BeFalse();
    }

    [Fact]
    public void CanAccess_ModeratorAccessingModeratorRoleOnly_ReturnsTrue()
    {
        // Arrange
        var adminUser = new AdminUser
        {
            Id = 1,
            Email = "admin@test.com",
            FullName = "Moderator User",
            Role = AdminRole.MODERATOR,
            IsActive = true
        };

        // Act & Assert
        adminUser.CanAccess(AdminRole.MODERATOR).Should().BeTrue();
        adminUser.CanAccess(AdminRole.ADMIN).Should().BeFalse();
        adminUser.CanAccess(AdminRole.SUPER_ADMIN).Should().BeFalse();
    }

    [Fact]
    public void CanAccess_InactiveUser_ReturnsFalse()
    {
        // Arrange
        var adminUser = new AdminUser
        {
            Id = 1,
            Email = "admin@test.com",
            FullName = "Inactive Admin",
            Role = AdminRole.SUPER_ADMIN,
            IsActive = false
        };

        // Act
        var result = adminUser.CanAccess(AdminRole.MODERATOR);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RecordLogin_SetsLastLoginAtToCurrentTime()
    {
        // Arrange
        var adminUser = new AdminUser
        {
            Id = 1,
            Email = "admin@test.com",
            FullName = "Test Admin",
            Role = AdminRole.ADMIN,
            IsActive = true
        };
        var beforeTime = DateTime.UtcNow;

        // Act
        adminUser.RecordLogin();
        var afterTime = DateTime.UtcNow;

        // Assert
        adminUser.LastLoginAt.Should().NotBeNull();
        adminUser.LastLoginAt.Should().BeOnOrAfter(beforeTime);
        adminUser.LastLoginAt.Should().BeOnOrBefore(afterTime);
    }
}
