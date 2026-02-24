using Mango.Services.Product.Application.DTOs;
using Mango.Services.ShoppingCart.Application.DTOs;

namespace Mango.E2E.Tests.Helpers;

/// <summary>
/// Helper class to build test data for E2E tests.
/// Provides fluent API for creating test entities and API payloads.
/// </summary>
public class TestDataBuilder
{
    /// <summary>
    /// Create a sample product DTO for testing.
    /// </summary>
    public static ProductDto CreateSampleProduct(int id = 1, string name = "Test Laptop")
    {
        return new ProductDto
        {
            Id = id,
            Name = name,
            Description = "High-performance laptop for professionals",
            Price = 999.99m,
            ImageUrl = "https://example.com/product.jpg",
            CategoryId = 1,
            IsAvailable = true
        };
    }

    /// <summary>
    /// Create multiple sample products for pagination testing.
    /// </summary>
    public static List<ProductDto> CreateSampleProducts(int count = 5)
    {
        var products = new List<ProductDto>();
        for (int i = 1; i <= count; i++)
        {
            products.Add(new ProductDto
            {
                Id = i,
                Name = $"Product {i}",
                Description = $"Test product {i} description",
                Price = (100m * i),
                ImageUrl = $"https://example.com/product{i}.jpg",
                CategoryId = (i % 3) + 1,
                IsAvailable = true
            });
        }
        return products;
    }

    /// <summary>
    /// Create a sample cart item DTO.
    /// </summary>
    public static CartItemDto CreateSampleCartItem(
        int productId = 1,
        string productName = "Test Product",
        decimal price = 99.99m,
        int quantity = 1)
    {
        return new CartItemDto
        {
            ProductId = productId,
            ProductName = productName,
            Price = price,
            Quantity = quantity
        };
    }

    /// <summary>
    /// Create a sample cart DTO with items.
    /// </summary>
    public static CartDto CreateSampleCart(string userId = "user123", int itemCount = 2)
    {
        var items = new List<CartItemDto>();
        for (int i = 0; i < itemCount; i++)
        {
            items.Add(CreateSampleCartItem(
                productId: i + 1,
                productName: $"Product {i + 1}",
                price: 100m * (i + 1),
                quantity: 1
            ));
        }

        var cartDto = new CartDto
        {
            Id = userId,
            Items = items
        };

        return cartDto;
    }

    /// <summary>
    /// Create test user ID for cart and order operations.
    /// </summary>
    public static string CreateTestUserId()
    {
        return $"testuser_{Guid.NewGuid()}";
    }

    /// <summary>
    /// Create test coupon code.
    /// </summary>
    public static string CreateTestCouponCode()
    {
        return $"TEST{DateTime.UtcNow.Ticks % 10000}";
    }

    /// <summary>
    /// Create test order reference ID.
    /// </summary>
    public static string CreateTestOrderId()
    {
        return $"ORD-{Guid.NewGuid()}";
    }
}
