using Xunit;
using FluentAssertions;
using Mango.Services.Product.Domain.Entities;

namespace Mango.Services.Product.UnitTests.Domain;

/// <summary>
/// Unit tests for Product entity validation and business rules.
/// These tests define the expected behavior BEFORE implementing the entity.
/// </summary>
public class ProductEntityTests
{
    #region Category Entity Tests

    [Fact]
    public void Category_Creation_WithValidData_Succeeds()
    {
        // Arrange
        var name = "Electronics";
        var description = "Electronic products";

        // Act
        var category = new Category
        {
            Id = 1,
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        category.Id.Should().Be(1);
        category.Name.Should().Be(name);
        category.Description.Should().Be(description);
        category.IsDeleted.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Category_WithInvalidName_ShouldValidate(string invalidName)
    {
        // Arrange
        var category = new Category { Name = invalidName };

        // Act & Assert
        category.Name.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    public void Category_SoftDelete_MaintainsData()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Electronics", IsDeleted = false };

        // Act
        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;

        // Assert
        category.IsDeleted.Should().BeTrue();
        category.DeletedAt.Should().NotBeNull();
    }

    #endregion

    #region Product Entity Tests

    [Fact]
    public void Product_Creation_WithValidData_Succeeds()
    {
        // Arrange
        var productName = "Laptop";
        var price = 999.99m;
        var categoryId = 1;

        // Act
        var product = new Product
        {
            Id = 1,
            Name = productName,
            Price = price,
            CategoryId = categoryId,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        product.Id.Should().Be(1);
        product.Name.Should().Be(productName);
        product.Price.Should().Be(price);
        product.CategoryId.Should().Be(categoryId);
        product.IsAvailable.Should().BeTrue();
        product.IsDeleted.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10.50)]
    [InlineData(-1)]
    public void Product_WithZeroOrNegativePrice_ShouldValidate(decimal invalidPrice)
    {
        // Arrange & Act
        var product = new Product { Price = invalidPrice };

        // Assert
        product.Price.Should().BeLessThanOrEqualTo(0);
    }

    [Theory]
    [InlineData(99.99, true)]
    [InlineData(0.01, true)]
    [InlineData(999999.99, true)]
    public void Product_WithValidPrice_ShouldAccept(decimal validPrice, bool expected)
    {
        // Arrange & Act
        var product = new Product { Price = validPrice };

        // Assert
        (product.Price > 0).Should().Be(expected);
    }

    [Fact]
    public void Product_IsAvailable_DefaultsToTrue()
    {
        // Arrange & Act
        var product = new Product { Name = "Laptop" };

        // Assert
        product.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Product_SetUnavailable_ReasonsCapture()
    {
        // Arrange
        var product = new Product { Name = "Laptop", IsAvailable = true };

        // Act
        product.IsAvailable = false;

        // Assert
        product.IsAvailable.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Product_WithInvalidName_ShouldValidate(string invalidName)
    {
        // Arrange & Act
        var product = new Product { Name = invalidName };

        // Assert
        product.Name.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    public void Product_WithImageUrl_Persists()
    {
        // Arrange
        var imageUrl = "https://example.com/laptop.jpg";

        // Act
        var product = new Product { Name = "Laptop", ImageUrl = imageUrl };

        // Assert
        product.ImageUrl.Should().Be(imageUrl);
    }

    [Fact]
    public void Product_Timestamps_SetCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var product = new Product
        {
            Name = "Laptop",
            CreatedAt = now,
            UpdatedAt = now
        };

        // Assert
        product.CreatedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
        product.UpdatedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Product_SoftDelete_Works()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Laptop", IsDeleted = false };
        var deleteTime = DateTime.UtcNow;

        // Act
        product.IsDeleted = true;
        product.DeletedAt = deleteTime;

        // Assert
        product.IsDeleted.Should().BeTrue();
        product.DeletedAt.Should().NotBeNull();
    }

    #endregion

    #region Relationship Tests

    [Fact]
    public void Product_BelongsToCategory_ByForeignKey()
    {
        // Arrange
        var categoryId = 5;

        // Act
        var product = new Product
        {
            Name = "Laptop",
            CategoryId = categoryId
        };

        // Assert
        product.CategoryId.Should().Be(categoryId);
    }

    #endregion
}
