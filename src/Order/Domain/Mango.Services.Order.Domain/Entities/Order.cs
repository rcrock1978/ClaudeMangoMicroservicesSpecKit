namespace Mango.Services.Order.Domain.Entities;

/// <summary>
/// Order aggregate root entity.
/// Represents a customer order with state machine pattern for lifecycle management.
/// Handles order creation, item management, payments, and status transitions.
/// </summary>
public class Order : AuditableEntity
{
    /// <summary>
    /// Customer/User ID who placed the order.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Unique order number for customer reference.
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Current state of the order.
    /// </summary>
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

    /// <summary>
    /// Date when the order was placed.
    /// </summary>
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Expected delivery date (set after shipping).
    /// </summary>
    public DateTime? DeliveryDate { get; set; }

    /// <summary>
    /// Shipping address (can be JSON or full address string).
    /// </summary>
    public string ShippingAddress { get; set; } = string.Empty;

    /// <summary>
    /// Billing address.
    /// </summary>
    public string BillingAddress { get; set; } = string.Empty;

    /// <summary>
    /// Payment method used (e.g., "CreditCard", "PayPal", "Bank Transfer").
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Transaction ID from payment gateway for tracking payments.
    /// </summary>
    public string? PaymentTransactionId { get; set; }

    /// <summary>
    /// Total discount amount for the entire order.
    /// </summary>
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>
    /// Shipping cost for the order.
    /// </summary>
    public decimal ShippingCost { get; set; } = 0;

    /// <summary>
    /// Tax amount calculated on subtotal.
    /// </summary>
    public decimal Tax { get; set; } = 0;

    /// <summary>
    /// Final total amount for the order (including all charges and discounts).
    /// </summary>
    public decimal TotalAmount { get; set; } = 0;

    /// <summary>
    /// Collection of items in this order.
    /// </summary>
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    /// <summary>
    /// Calculate the subtotal from all items before discounts and shipping.
    /// </summary>
    public decimal CalculateSubtotal()
    {
        return Items.Sum(item => item.CalculateTotal());
    }

    /// <summary>
    /// Calculate the complete order total with all charges.
    /// Formula: Subtotal - Discount + Tax + Shipping
    /// </summary>
    public decimal CalculateTotal()
    {
        var subtotal = CalculateSubtotal();
        TotalAmount = subtotal - DiscountAmount + Tax + ShippingCost;
        return TotalAmount;
    }

    /// <summary>
    /// Add an item to the order (only in Pending state).
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="productName">Product name</param>
    /// <param name="unitPrice">Unit price</param>
    /// <param name="quantity">Quantity</param>
    /// <returns>True if item was added, false if validation failed or invalid state</returns>
    public bool AddItem(int productId, string productName, decimal unitPrice, int quantity)
    {
        // Can only add items in Pending state
        if (OrderStatus != OrderStatus.Pending)
        {
            return false;
        }

        // Validate inputs
        if (productId <= 0 || quantity <= 0 || unitPrice <= 0)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(productName))
        {
            return false;
        }

        var item = new OrderItem
        {
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity,
            TotalPrice = unitPrice * quantity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Items.Add(item);
        return true;
    }

    /// <summary>
    /// Remove an item from the order (only in Pending state).
    /// </summary>
    /// <param name="itemId">Item ID to remove</param>
    /// <returns>True if removed, false otherwise</returns>
    public bool RemoveItem(int itemId)
    {
        // Can only remove items in Pending state
        if (OrderStatus != OrderStatus.Pending)
        {
            return false;
        }

        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
        {
            return false;
        }

        Items.Remove(item);
        return true;
    }

    /// <summary>
    /// Check if order can be confirmed (valid state and items exist).
    /// </summary>
    /// <returns>True if order can be confirmed</returns>
    public bool CanConfirmOrder()
    {
        return OrderStatus == OrderStatus.Pending &&
               Items.Any() &&
               !string.IsNullOrWhiteSpace(UserId) &&
               !string.IsNullOrWhiteSpace(ShippingAddress) &&
               !string.IsNullOrWhiteSpace(PaymentMethod) &&
               Items.All(i => i.IsValid());
    }

    /// <summary>
    /// Confirm order - transition from Pending to Processing (payment processing stage).
    /// </summary>
    /// <returns>True if confirmation succeeded</returns>
    public bool ConfirmOrder()
    {
        if (!CanConfirmOrder())
        {
            return false;
        }

        OrderStatus = OrderStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Record payment and transition to Shipped state (ready for delivery).
    /// </summary>
    /// <param name="transactionId">Payment transaction ID from gateway</param>
    /// <returns>True if payment was processed</returns>
    public bool ProcessPayment(string transactionId)
    {
        // Can only process payment in Processing state
        if (OrderStatus != OrderStatus.Processing)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(transactionId))
        {
            return false;
        }

        PaymentTransactionId = transactionId;
        OrderStatus = OrderStatus.Shipped;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Transition order to Delivered state.
    /// </summary>
    /// <returns>True if delivery was recorded</returns>
    public bool RecordDelivery()
    {
        // Can only mark delivered if already shipped
        if (OrderStatus != OrderStatus.Shipped)
        {
            return false;
        }

        OrderStatus = OrderStatus.Delivered;
        DeliveryDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Cancel the order (only allowed from Pending or Processing states).
    /// </summary>
    /// <returns>True if cancellation succeeded</returns>
    public bool CancelOrder()
    {
        // Can only cancel from Pending or Processing state
        if (OrderStatus != OrderStatus.Pending && OrderStatus != OrderStatus.Processing)
        {
            return false;
        }

        OrderStatus = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Check if order can be cancelled.
    /// </summary>
    public bool CanBeCancelled()
    {
        return OrderStatus == OrderStatus.Pending || OrderStatus == OrderStatus.Processing;
    }

    /// <summary>
    /// Check if order is in a completed state (Delivered or Cancelled).
    /// </summary>
    public bool IsCompleted()
    {
        return OrderStatus == OrderStatus.Delivered || OrderStatus == OrderStatus.Cancelled;
    }

    /// <summary>
    /// Get all validation errors for the current order state.
    /// </summary>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(UserId))
            errors.Add("UserId is required");

        if (string.IsNullOrWhiteSpace(OrderNumber))
            errors.Add("OrderNumber is required");

        if (string.IsNullOrWhiteSpace(ShippingAddress))
            errors.Add("ShippingAddress is required");

        if (string.IsNullOrWhiteSpace(PaymentMethod))
            errors.Add("PaymentMethod is required");

        if (!Items.Any())
            errors.Add("Order must contain at least one item");

        if (Items.Any(i => !i.IsValid()))
            errors.Add("One or more items are invalid");

        if (TotalAmount < 0)
            errors.Add("Total amount cannot be negative");

        return errors;
    }
}
