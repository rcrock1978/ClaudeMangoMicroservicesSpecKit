using Xunit;
using FluentAssertions;
using OrderEntity = Mango.Services.Order.Domain.Entities.Order;
using OrderItemEntity = Mango.Services.Order.Domain.Entities.OrderItem;
using OrderStatus = Mango.Services.Order.Domain.Entities.OrderStatus;

namespace Mango.Services.Order.UnitTests.Domain;

/// <summary>
/// Unit tests for Order and OrderItem entity validation and business rules.
/// TDD approach: tests define expected behavior before implementation.
/// </summary>
public class OrderEntityTests
{
    // ===== Order Creation Tests =====

    [Fact]
    public void Order_Creation_WithValidData_Succeeds()
    {
        // Act
        var order = new OrderEntity
        {
            UserId = "user123",
            OrderNumber = "ORD-001",
            ShippingAddress = "123 Main St",
            PaymentMethod = "CreditCard"
        };

        // Assert
        order.UserId.Should().Be("user123");
        order.OrderNumber.Should().Be("ORD-001");
        order.ShippingAddress.Should().Be("123 Main St");
        order.PaymentMethod.Should().Be("CreditCard");
        order.OrderStatus.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void Order_Creation_DefaultStatus_IsPending()
    {
        // Act
        var order = new OrderEntity { UserId = "user123" };

        // Assert
        order.OrderStatus.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void Order_Creation_DefaultTotalAmount_IsZero()
    {
        // Act
        var order = new OrderEntity();

        // Assert
        order.TotalAmount.Should().Be(0);
    }

    // ===== OrderItem Tests =====

    [Fact]
    public void OrderItem_Creation_WithValidData_Succeeds()
    {
        // Act
        var item = new OrderItemEntity
        {
            ProductId = 1,
            ProductName = "Laptop",
            UnitPrice = 999.99m,
            Quantity = 1
        };

        // Assert
        item.ProductId.Should().Be(1);
        item.ProductName.Should().Be("Laptop");
        item.UnitPrice.Should().Be(999.99m);
        item.Quantity.Should().Be(1);
    }

    [Fact]
    public void OrderItem_CalculateTotal_ReturnsUnitPriceTimesQuantity()
    {
        // Arrange
        var item = new OrderItemEntity
        {
            UnitPrice = 100m,
            Quantity = 5
        };

        // Act
        var total = item.CalculateTotal();

        // Assert
        total.Should().Be(500m);
    }

    [Fact]
    public void OrderItem_CalculateFinalPrice_WithDiscount_ReturnsCorrectAmount()
    {
        // Arrange
        var item = new OrderItemEntity
        {
            UnitPrice = 100m,
            Quantity = 5,
            DiscountAmount = 50m
        };

        // Act
        var finalPrice = item.CalculateFinalPrice();

        // Assert
        finalPrice.Should().Be(450m);
    }

    [Fact]
    public void OrderItem_IsValid_WithValidData_ReturnsTrue()
    {
        // Arrange
        var item = new OrderItemEntity
        {
            ProductId = 1,
            ProductName = "Product",
            UnitPrice = 100m,
            Quantity = 1,
            DiscountAmount = 0
        };

        // Act
        var isValid = item.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void OrderItem_IsValid_WithoutProductId_ReturnsFalse()
    {
        // Arrange
        var item = new OrderItemEntity
        {
            ProductId = 0,
            ProductName = "Product",
            UnitPrice = 100m,
            Quantity = 1
        };

        // Act
        var isValid = item.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void OrderItem_IsValid_WithoutProductName_ReturnsFalse()
    {
        // Arrange
        var item = new OrderItemEntity
        {
            ProductId = 1,
            ProductName = "",
            UnitPrice = 100m,
            Quantity = 1
        };

        // Act
        var isValid = item.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    // ===== AddItem Tests =====

    [Fact]
    public void Order_AddItem_InPendingState_Succeeds()
    {
        // Arrange
        var order = new OrderEntity { UserId = "user123", OrderStatus = OrderStatus.Pending };

        // Act
        var result = order.AddItem(1, "Laptop", 999.99m, 1);

        // Assert
        result.Should().BeTrue();
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public void Order_AddItem_MultipleItems_Succeeds()
    {
        // Arrange
        var order = new OrderEntity { UserId = "user123" };

        // Act
        order.AddItem(1, "Laptop", 999.99m, 1);
        order.AddItem(2, "Mouse", 29.99m, 2);

        // Assert
        order.Items.Should().HaveCount(2);
    }

    [Fact]
    public void Order_AddItem_WithInvalidProductId_Fails()
    {
        // Arrange
        var order = new OrderEntity { UserId = "user123" };

        // Act
        var result = order.AddItem(0, "Laptop", 999.99m, 1);

        // Assert
        result.Should().BeFalse();
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void Order_AddItem_WithInvalidQuantity_Fails()
    {
        // Arrange
        var order = new OrderEntity { UserId = "user123" };

        // Act
        var result = order.AddItem(1, "Laptop", 999.99m, 0);

        // Assert
        result.Should().BeFalse();
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void Order_AddItem_WithInvalidPrice_Fails()
    {
        // Arrange
        var order = new OrderEntity { UserId = "user123" };

        // Act
        var result = order.AddItem(1, "Laptop", 0, 1);

        // Assert
        result.Should().BeFalse();
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void Order_AddItem_WithEmptyProductName_Fails()
    {
        // Arrange
        var order = new OrderEntity { UserId = "user123" };

        // Act
        var result = order.AddItem(1, "", 999.99m, 1);

        // Assert
        result.Should().BeFalse();
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void Order_AddItem_InProcessingState_Fails()
    {
        // Arrange
        var order = new OrderEntity { UserId = "user123", OrderStatus = OrderStatus.Processing };

        // Act
        var result = order.AddItem(1, "Laptop", 999.99m, 1);

        // Assert
        result.Should().BeFalse();
        order.Items.Should().BeEmpty();
    }

    // ===== RemoveItem Tests =====

    [Fact]
    public void Order_RemoveItem_InPendingState_Succeeds()
    {
        // Arrange
        var order = new OrderEntity { UserId = "user123" };
        order.AddItem(1, "Laptop", 999.99m, 1);
        var itemId = order.Items.First().Id;

        // Act
        var result = order.RemoveItem(itemId);

        // Assert
        result.Should().BeTrue();
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void Order_RemoveItem_InProcessingState_Fails()
    {
        // Arrange
        var order = new OrderEntity { UserId = "user123", OrderStatus = OrderStatus.Processing };
        var item = new OrderItemEntity { ProductId = 1, ProductName = "Laptop", UnitPrice = 999.99m, Quantity = 1, Id = 1 };
        order.Items.Add(item);

        // Act
        var result = order.RemoveItem(1);

        // Assert
        result.Should().BeFalse();
        order.Items.Should().HaveCount(1);
    }

    // ===== CalculateTotal Tests =====

    [Fact]
    public void Order_CalculateTotal_WithItems_ReturnsCorrectTotal()
    {
        // Arrange
        var order = new OrderEntity();
        order.AddItem(1, "Item1", 100m, 2);
        order.AddItem(2, "Item2", 50m, 1);

        // Act
        var total = order.CalculateTotal();

        // Assert
        // Subtotal: (100*2) + (50*1) = 250
        total.Should().Be(250m);
    }

    [Fact]
    public void Order_CalculateTotal_WithDiscountAndShipping_ReturnsCorrectTotal()
    {
        // Arrange
        var order = new OrderEntity
        {
            DiscountAmount = 50m,
            ShippingCost = 10m,
            Tax = 5m
        };
        order.AddItem(1, "Item", 100m, 1);

        // Act
        var total = order.CalculateTotal();

        // Assert
        // 100 - 50 + 5 + 10 = 65
        total.Should().Be(65m);
    }

    [Fact]
    public void Order_CalculateSubtotal_ReturnsOnlyItemsTotal()
    {
        // Arrange
        var order = new OrderEntity
        {
            DiscountAmount = 50m,
            ShippingCost = 10m,
            Tax = 5m
        };
        order.AddItem(1, "Item", 100m, 1);

        // Act
        var subtotal = order.CalculateSubtotal();

        // Assert
        subtotal.Should().Be(100m);
    }

    // ===== State Machine Tests =====

    [Fact]
    public void Order_ConfirmOrder_FromPending_Succeeds()
    {
        // Arrange
        var order = new OrderEntity
        {
            UserId = "user123",
            ShippingAddress = "123 Main St",
            PaymentMethod = "Card"
        };
        order.AddItem(1, "Item", 100m, 1);

        // Act
        var result = order.ConfirmOrder();

        // Assert
        result.Should().BeTrue();
        order.OrderStatus.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public void Order_ConfirmOrder_WithoutItems_Fails()
    {
        // Arrange
        var order = new OrderEntity
        {
            UserId = "user123",
            ShippingAddress = "123 Main St",
            PaymentMethod = "Card"
        };

        // Act
        var result = order.ConfirmOrder();

        // Assert
        result.Should().BeFalse();
        order.OrderStatus.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void Order_ConfirmOrder_WithoutShippingAddress_Fails()
    {
        // Arrange
        var order = new OrderEntity
        {
            UserId = "user123",
            PaymentMethod = "Card"
        };
        order.AddItem(1, "Item", 100m, 1);

        // Act
        var result = order.ConfirmOrder();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Order_ProcessPayment_FromProcessing_Succeeds()
    {
        // Arrange
        var order = new OrderEntity
        {
            UserId = "user123",
            ShippingAddress = "123 Main St",
            PaymentMethod = "Card",
            OrderStatus = OrderStatus.Processing
        };
        order.AddItem(1, "Item", 100m, 1);

        // Act
        var result = order.ProcessPayment("TXN-12345");

        // Assert
        result.Should().BeTrue();
        order.OrderStatus.Should().Be(OrderStatus.Shipped);
        order.PaymentTransactionId.Should().Be("TXN-12345");
    }

    [Fact]
    public void Order_ProcessPayment_FromPending_Fails()
    {
        // Arrange
        var order = new OrderEntity { OrderStatus = OrderStatus.Pending };

        // Act
        var result = order.ProcessPayment("TXN-12345");

        // Assert
        result.Should().BeFalse();
        order.OrderStatus.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void Order_RecordDelivery_FromShipped_Succeeds()
    {
        // Arrange
        var order = new OrderEntity { OrderStatus = OrderStatus.Shipped };

        // Act
        var result = order.RecordDelivery();

        // Assert
        result.Should().BeTrue();
        order.OrderStatus.Should().Be(OrderStatus.Delivered);
        order.DeliveryDate.Should().NotBeNull();
    }

    [Fact]
    public void Order_RecordDelivery_FromPending_Fails()
    {
        // Arrange
        var order = new OrderEntity { OrderStatus = OrderStatus.Pending };

        // Act
        var result = order.RecordDelivery();

        // Assert
        result.Should().BeFalse();
        order.OrderStatus.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void Order_CancelOrder_FromPending_Succeeds()
    {
        // Arrange
        var order = new OrderEntity { OrderStatus = OrderStatus.Pending };

        // Act
        var result = order.CancelOrder();

        // Assert
        result.Should().BeTrue();
        order.OrderStatus.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Order_CancelOrder_FromProcessing_Succeeds()
    {
        // Arrange
        var order = new OrderEntity { OrderStatus = OrderStatus.Processing };

        // Act
        var result = order.CancelOrder();

        // Assert
        result.Should().BeTrue();
        order.OrderStatus.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Order_CancelOrder_FromShipped_Fails()
    {
        // Arrange
        var order = new OrderEntity { OrderStatus = OrderStatus.Shipped };

        // Act
        var result = order.CancelOrder();

        // Assert
        result.Should().BeFalse();
        order.OrderStatus.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public void Order_CancelOrder_FromDelivered_Fails()
    {
        // Arrange
        var order = new OrderEntity { OrderStatus = OrderStatus.Delivered };

        // Act
        var result = order.CancelOrder();

        // Assert
        result.Should().BeFalse();
        order.OrderStatus.Should().Be(OrderStatus.Delivered);
    }

    // ===== Validation Tests =====

    [Fact]
    public void Order_CanBeCancelled_InPending_ReturnsTrue()
    {
        // Arrange
        var order = new OrderEntity { OrderStatus = OrderStatus.Pending };

        // Act
        var result = order.CanBeCancelled();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Order_CanBeCancelled_InProcessing_ReturnsTrue()
    {
        // Arrange
        var order = new OrderEntity { OrderStatus = OrderStatus.Processing };

        // Act
        var result = order.CanBeCancelled();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Order_CanBeCancelled_InShipped_ReturnsFalse()
    {
        // Arrange
        var order = new OrderEntity { OrderStatus = OrderStatus.Shipped };

        // Act
        var result = order.CanBeCancelled();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Order_IsCompleted_InDelivered_ReturnsTrue()
    {
        // Arrange
        var order = new OrderEntity { OrderStatus = OrderStatus.Delivered };

        // Act
        var result = order.IsCompleted();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Order_IsCompleted_InCancelled_ReturnsTrue()
    {
        // Arrange
        var order = new OrderEntity { OrderStatus = OrderStatus.Cancelled };

        // Act
        var result = order.IsCompleted();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Order_IsCompleted_InProcessing_ReturnsFalse()
    {
        // Arrange
        var order = new OrderEntity { OrderStatus = OrderStatus.Processing };

        // Act
        var result = order.IsCompleted();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Order_GetValidationErrors_WithAllRequiredFields_ReturnsEmpty()
    {
        // Arrange
        var order = new OrderEntity
        {
            UserId = "user123",
            OrderNumber = "ORD-001",
            ShippingAddress = "123 Main St",
            PaymentMethod = "Card",
            TotalAmount = 100m
        };
        order.AddItem(1, "Item", 100m, 1);

        // Act
        var errors = order.GetValidationErrors();

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Order_GetValidationErrors_WithMissingUserId_ReturnsError()
    {
        // Arrange
        var order = new OrderEntity
        {
            OrderNumber = "ORD-001",
            ShippingAddress = "123 Main St",
            PaymentMethod = "Card"
        };

        // Act
        var errors = order.GetValidationErrors();

        // Assert
        errors.Should().Contain("UserId is required");
    }

    [Fact]
    public void Order_CanConfirmOrder_WithValidData_ReturnsTrue()
    {
        // Arrange
        var order = new OrderEntity
        {
            UserId = "user123",
            ShippingAddress = "123 Main St",
            PaymentMethod = "Card"
        };
        order.AddItem(1, "Item", 100m, 1);

        // Act
        var result = order.CanConfirmOrder();

        // Assert
        result.Should().BeTrue();
    }
}
