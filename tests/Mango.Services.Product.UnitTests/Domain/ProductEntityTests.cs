using Xunit;
using FluentAssertions;
using ProductEntity = Mango.Services.Product.Domain.Entities.Product;
using CategoryEntity = Mango.Services.Product.Domain.Entities.Category;

namespace Mango.Services.Product.UnitTests.Domain;

/// <summary>
/// Unit tests for Product entity validation and business rules.
/// TDD approach: tests define expected behavior before implementation.
/// </summary>
public class ProductEntityTests
{
    [Fact]
    public void Product_Creation_WithValidPrice_Succeeds()
    {
        // Act
        var product = new ProductEntity
        {
            Name = "Laptop",
            Price = 999.99m,
            CategoryId = 1,
            IsAvailable = true
        };

        // Assert
        product.Name.Should().Be("Laptop");
        product.Price.Should().Be(999.99m);
        product.CategoryId.Should().Be(1);
        product.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Category_Creation_WithValidData_Succeeds()
    {
        // Act
        var category = new CategoryEntity
        {
            Name = "Electronics",
            Description = "Electronic products"
        };

        // Assert
        category.Name.Should().Be("Electronics");
        category.Description.Should().Be("Electronic products");
    }

    [Fact]
    public void Product_CanCalculateTotal_WithQuantity()
    {
        // Arrange
        var product = new ProductEntity { Price = 100m };

        // Act
        var total = product.CalculateTotal(5);

        // Assert
        total.Should().Be(500m);
    }

    [Fact]
    public void Product_CanBePurchased_WhenAvailableAndNotDeleted()
    {
        // Arrange
        var product = new ProductEntity
        {
            Price = 100m,
            IsAvailable = true,
            IsDeleted = false
        };

        // Act
        var canBePurchased = product.CanBePurchased();

        // Assert
        canBePurchased.Should().BeTrue();
    }

    [Fact]
    public void Product_CannotBePurchased_WhenUnavailable()
    {
        // Arrange
        var product = new ProductEntity
        {
            Price = 100m,
            IsAvailable = false,
            IsDeleted = false
        };

        // Act
        var canBePurchased = product.CanBePurchased();

        // Assert
        canBePurchased.Should().BeFalse();
    }
}
