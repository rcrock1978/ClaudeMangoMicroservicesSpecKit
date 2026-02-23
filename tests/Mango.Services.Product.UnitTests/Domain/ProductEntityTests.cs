using Xunit;
using FluentAssertions;

namespace Mango.Services.Product.UnitTests.Domain;

/// <summary>
/// Unit tests for Product entity validation and business rules.
/// TDD approach: tests define expected behavior before implementation.
/// </summary>
public class ProductEntityTests
{
    private const string ProductNamespace = "Mango.Services.Product.Domain.Entities";

    [Fact]
    public void Product_Creation_WithValidPrice_Succeeds()
    {
        // Arrange - Create entity using fully qualified name
        var productType = Type.GetType($"{ProductNamespace}.Product");
        Assert.NotNull(productType);

        // Act
        dynamic product = Activator.CreateInstance(productType)!;
        product.Name = "Laptop";
        product.Price = 999.99m;
        product.CategoryId = 1;
        product.IsAvailable = true;

        // Assert
        Assert.Equal("Laptop", product.Name);
        Assert.Equal(999.99m, product.Price);
        Assert.True(product.IsAvailable);
    }

    [Fact]
    public void Category_Creation_WithValidData_Succeeds()
    {
        // Arrange
        var categoryType = Type.GetType($"{ProductNamespace}.Category");
        Assert.NotNull(categoryType);

        // Act
        dynamic category = Activator.CreateInstance(categoryType)!;
        category.Name = "Electronics";
        category.Description = "Electronic products";

        // Assert
        Assert.Equal("Electronics", category.Name);
        Assert.Equal("Electronic products", category.Description);
    }

    [Fact]
    public void Product_CanCalculateTotal_WithQuantity()
    {
        // Arrange
        var productType = Type.GetType($"{ProductNamespace}.Product");
        var product = Activator.CreateInstance(productType) as dynamic;

        product.Price = 100m;

        // Act
        var total = product.CalculateTotal(5);

        // Assert
        Assert.Equal(500m, total);
    }

    [Fact]
    public void Product_CanBePurchased_WhenAvailableAndNotDeleted()
    {
        // Arrange
        var productType = Type.GetType($"{ProductNamespace}.Product");
        var product = Activator.CreateInstance(productType) as dynamic;

        product.Price = 100m;
        product.IsAvailable = true;
        product.IsDeleted = false;

        // Act
        var canBePurchased = product.CanBePurchased();

        // Assert
        Assert.True(canBePurchased);
    }

    [Fact]
    public void Product_CannotBePurchased_WhenUnavailable()
    {
        // Arrange
        var productType = Type.GetType($"{ProductNamespace}.Product");
        var product = Activator.CreateInstance(productType) as dynamic;

        product.Price = 100m;
        product.IsAvailable = false;
        product.IsDeleted = false;

        // Act
        var canBePurchased = product.CanBePurchased();

        // Assert
        Assert.False(canBePurchased);
    }
}
