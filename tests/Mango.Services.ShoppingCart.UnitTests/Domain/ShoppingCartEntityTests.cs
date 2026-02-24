using Xunit;
using FluentAssertions;
using CartEntity = Mango.Services.ShoppingCart.Domain.Entities.ShoppingCart;

namespace Mango.Services.ShoppingCart.UnitTests.Domain;

/// <summary>
/// Unit tests for ShoppingCart domain entity business logic.
/// </summary>
public class ShoppingCartEntityTests
{
    [Fact]
    public void CreateCart_WithUserId_ShouldSucceed()
    {
        // Arrange & Act
        var cart = new CartEntity
        {
            UserId = "user123"
        };

        // Assert
        cart.UserId.Should().Be("user123");
        cart.Items.Should().BeEmpty();
        cart.IsEmpty.Should().BeTrue();
        cart.GetSubtotal().Should().Be(0);
    }

    [Fact]
    public void AddItem_WithValidProduct_ShouldAddToCart()
    {
        // Arrange
        var cart = new CartEntity { UserId = "user123" };

        // Act
        cart.AddItem(productId: 1, productName: "Laptop", productPrice: 999.99m, productImageUrl: null, quantity: 1);

        // Assert
        cart.Items.Should().HaveCount(1);
        cart.Items[0].ProductId.Should().Be(1);
        cart.Items[0].ProductName.Should().Be("Laptop");
        cart.Items[0].Quantity.Should().Be(1);
        cart.GetSubtotal().Should().Be(999.99m);
        cart.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void AddItem_WithSameProductTwice_ShouldIncreaseQuantity()
    {
        // Arrange
        var cart = new CartEntity { UserId = "user123" };
        cart.AddItem(1, "Laptop", 999.99m, null, 1);

        // Act
        cart.AddItem(1, "Laptop", 999.99m, null, 2);

        // Assert
        cart.Items.Should().HaveCount(1);
        cart.Items[0].Quantity.Should().Be(3);
        cart.GetSubtotal().Should().Be(2999.97m); // 999.99 * 3
    }

    [Fact]
    public void AddItem_WithMultipleDifferentProducts_ShouldAddAllItems()
    {
        // Arrange
        var cart = new CartEntity { UserId = "user123" };

        // Act
        cart.AddItem(1, "Laptop", 999.99m, null, 1);
        cart.AddItem(2, "Mouse", 29.99m, null, 2);
        cart.AddItem(3, "Keyboard", 79.99m, null, 1);

        // Assert
        cart.Items.Should().HaveCount(3);
        cart.GetSubtotal().Should().Be(1139.96m); // 999.99 + (29.99 * 2) + 79.99
        cart.GetItemCount().Should().Be(4); // 1 + 2 + 1
    }

    [Fact]
    public void AddItem_WithZeroQuantity_ShouldThrowException()
    {
        // Arrange
        var cart = new CartEntity { UserId = "user123" };

        // Act & Assert
        cart.Invoking(c => c.AddItem(1, "Laptop", 999.99m, null, 0))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemoveItem_WithExistingProduct_ShouldRemoveFromCart()
    {
        // Arrange
        var cart = new CartEntity { UserId = "user123" };
        cart.AddItem(1, "Laptop", 999.99m, null, 1);
        cart.AddItem(2, "Mouse", 29.99m, null, 1);

        // Act
        cart.RemoveItem(1);

        // Assert
        cart.Items.Should().HaveCount(1);
        cart.Items[0].ProductId.Should().Be(2);
        cart.GetSubtotal().Should().Be(29.99m);
    }

    [Fact]
    public void RemoveItem_WithNonExistingProduct_ShouldDoNothing()
    {
        // Arrange
        var cart = new CartEntity { UserId = "user123" };
        cart.AddItem(1, "Laptop", 999.99m, null, 1);

        // Act
        cart.RemoveItem(999);

        // Assert
        cart.Items.Should().HaveCount(1);
    }

    [Fact]
    public void UpdateItemQuantity_WithValidQuantity_ShouldUpdate()
    {
        // Arrange
        var cart = new CartEntity { UserId = "user123" };
        cart.AddItem(1, "Laptop", 999.99m, null, 1);

        // Act
        cart.UpdateItemQuantity(1, 3);

        // Assert
        cart.Items[0].Quantity.Should().Be(3);
        cart.GetSubtotal().Should().Be(2999.97m);
    }

    [Fact]
    public void UpdateItemQuantity_WithZeroQuantity_ShouldRemoveItem()
    {
        // Arrange
        var cart = new CartEntity { UserId = "user123" };
        cart.AddItem(1, "Laptop", 999.99m, null, 1);

        // Act
        cart.UpdateItemQuantity(1, 0);

        // Assert
        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public void Clear_ShouldRemoveAllItemsAndCoupon()
    {
        // Arrange
        var cart = new CartEntity { UserId = "user123" };
        cart.AddItem(1, "Laptop", 999.99m, null, 1);
        cart.ApplyCoupon("SAVE10", 99.99m);

        // Act
        cart.Clear();

        // Assert
        cart.Items.Should().BeEmpty();
        cart.CouponCode.Should().BeNull();
        cart.DiscountAmount.Should().Be(0);
        cart.GetSubtotal().Should().Be(0);
        cart.GetTotal().Should().Be(0);
    }

    [Fact]
    public void ApplyCoupon_WithValidDiscount_ShouldApplyDiscount()
    {
        // Arrange
        var cart = new CartEntity { UserId = "user123" };
        cart.AddItem(1, "Laptop", 100m, null, 1);

        // Act
        cart.ApplyCoupon("SAVE10", 10m);

        // Assert
        cart.CouponCode.Should().Be("SAVE10");
        cart.DiscountAmount.Should().Be(10m);
        cart.GetTotal().Should().Be(90m);
    }

    [Fact]
    public void ApplyCoupon_WithDiscountGreaterThanSubtotal_ShouldCapDiscount()
    {
        // Arrange
        var cart = new CartEntity { UserId = "user123" };
        cart.AddItem(1, "Laptop", 100m, null, 1);

        // Act
        cart.ApplyCoupon("HUGE", 200m);

        // Assert
        cart.DiscountAmount.Should().Be(100m); // Capped at subtotal
        cart.GetTotal().Should().Be(0);
    }

    [Fact]
    public void ApplyCoupon_WithNegativeDiscount_ShouldThrowException()
    {
        // Arrange
        var cart = new CartEntity { UserId = "user123" };

        // Act & Assert
        cart.Invoking(c => c.ApplyCoupon("INVALID", -10m))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemoveCoupon_ShouldClearCouponAndDiscount()
    {
        // Arrange
        var cart = new CartEntity { UserId = "user123" };
        cart.AddItem(1, "Laptop", 100m, null, 1);
        cart.ApplyCoupon("SAVE10", 10m);

        // Act
        cart.RemoveCoupon();

        // Assert
        cart.CouponCode.Should().BeNull();
        cart.DiscountAmount.Should().Be(0);
        cart.GetTotal().Should().Be(100m);
    }

    [Fact]
    public void IsValidForCheckout_WithValidCart_ShouldReturnTrue()
    {
        // Arrange
        var cart = new CartEntity { UserId = "user123" };
        cart.AddItem(1, "Laptop", 999.99m, null, 1);

        // Act & Assert
        cart.IsValidForCheckout().Should().BeTrue();
    }

    [Fact]
    public void IsValidForCheckout_WithEmptyCart_ShouldReturnFalse()
    {
        // Arrange
        var cart = new CartEntity { UserId = "user123" };

        // Act & Assert
        cart.IsValidForCheckout().Should().BeFalse();
    }

    [Fact]
    public void GetItemCount_ShouldReturnTotalQuantity()
    {
        // Arrange
        var cart = new CartEntity { UserId = "user123" };
        cart.AddItem(1, "Laptop", 999.99m, null, 2);
        cart.AddItem(2, "Mouse", 29.99m, null, 3);

        // Act
        var count = cart.GetItemCount();

        // Assert
        count.Should().Be(5); // 2 + 3
    }

    [Fact]
    public void CartItem_LineTotal_ShouldBeCalculated()
    {
        // Arrange
        var cart = new CartEntity { UserId = "user123" };
        cart.AddItem(1, "Laptop", 999.99m, null, 3);

        // Act & Assert
        cart.Items[0].LineTotal.Should().Be(2999.97m);
    }
}
