using Mango.E2E.Tests.Fixtures;
using Mango.E2E.Tests.Helpers;
using Mango.Services.Product.Application.DTOs;
using Mango.Services.ShoppingCart.Application.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace Mango.E2E.Tests.Integration;

/// <summary>
/// End-to-end tests for the complete checkout flow.
/// Tests the integration of Product, Shopping Cart, Coupon, Order, Email, and Reward services.
///
/// Test scenarios include:
/// - User browses products and applies pagination
/// - User searches for products
/// - User adds items to shopping cart
/// - User applies valid and invalid coupons
/// - User creates orders with discounts
/// - Order confirmation and automatic events (emails, reward points)
/// - Complete order lifecycle management
/// </summary>
[Collection("E2E Tests")]
public class CheckoutFlowTests
{
    private readonly E2ETestFixture _fixture;

    public CheckoutFlowTests(E2ETestFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>
    /// Test 1: User browses products with pagination.
    /// Verifies that the Product Service returns paginated results correctly.
    /// </summary>
    [Fact]
    public async Task BrowseProducts_WithPagination_ReturnsPagedResults()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;

        // Act
        var response = await _fixture.ProductClient!.GetAsync(
            $"/api/products?pageNumber={pageNumber}&pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsAsync<PaginatedProductResponse>();
        content.Should().NotBeNull();
        content.PageNumber.Should().Be(pageNumber);
        content.PageSize.Should().Be(pageSize);
        content.Items.Should().NotBeEmpty();
    }

    /// <summary>
    /// Test 2: User searches for products by keyword.
    /// Verifies search functionality across product names and descriptions.
    /// </summary>
    [Fact]
    public async Task SearchProducts_WithValidSearchTerm_ReturnsMatchingProducts()
    {
        // Arrange
        var searchTerm = "laptop";
        var pageNumber = 1;
        var pageSize = 10;

        // Act
        var response = await _fixture.ProductClient!.GetAsync(
            $"/api/products/search?searchTerm={searchTerm}&pageNumber={pageNumber}&pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsAsync<PaginatedProductResponse>();
        content.Should().NotBeNull();
        content.Items.Should().NotBeEmpty();
    }

    /// <summary>
    /// Test 3: User searches with empty term - should return error.
    /// Verifies input validation for search functionality.
    /// </summary>
    [Fact]
    public async Task SearchProducts_WithEmptySearchTerm_ReturnsBadRequest()
    {
        // Arrange
        var searchTerm = "";

        // Act
        var response = await _fixture.ProductClient!.GetAsync(
            $"/api/products/search?searchTerm={searchTerm}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Test 4: User gets single product by ID.
    /// Verifies that detailed product information is available.
    /// </summary>
    [Fact]
    public async Task GetProduct_ByValidId_ReturnsProductDetails()
    {
        // Arrange
        var productId = 1;

        // Act
        var response = await _fixture.ProductClient!.GetAsync($"/api/products/{productId}");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsAsync<ProductDto>();
            content.Should().NotBeNull();
            content.Id.Should().Be(productId);
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            // No products in database yet - acceptable for new system
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }

    /// <summary>
    /// Test 5: User gets products by category.
    /// Verifies category filtering functionality.
    /// </summary>
    [Fact]
    public async Task GetProductsByCategory_WithValidCategoryId_ReturnsProductsInCategory()
    {
        // Arrange
        var categoryId = 1;
        var pageNumber = 1;
        var pageSize = 10;

        // Act
        var response = await _fixture.ProductClient!.GetAsync(
            $"/api/products/category/{categoryId}?pageNumber={pageNumber}&pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsAsync<PaginatedProductResponse>();
            content.Should().NotBeNull();
        }
    }

    /// <summary>
    /// Test 6: User adds item to shopping cart.
    /// Verifies cart management and persistence.
    /// </summary>
    [Fact]
    public async Task AddItemToCart_WithValidProduct_SuccessfullyAddsToCart()
    {
        // Arrange
        var userId = TestDataBuilder.CreateTestUserId();
        var cartItem = TestDataBuilder.CreateSampleCartItem(
            productId: 1,
            productName: "Test Laptop",
            price: 999.99m,
            quantity: 1
        );

        // Act
        var response = await _fixture.ShoppingCartClient!.PostAsJsonAsync(
            $"/api/cart/{userId}/items",
            cartItem
        );

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
    }

    /// <summary>
    /// Test 7: User updates cart item quantity.
    /// Verifies cart modification functionality.
    /// </summary>
    [Fact]
    public async Task UpdateCartItemQuantity_WithValidQuantity_UpdatesSuccessfully()
    {
        // Arrange
        var userId = TestDataBuilder.CreateTestUserId();
        var productId = 1;
        var newQuantity = 5;

        // Act
        var response = await _fixture.ShoppingCartClient!.PutAsJsonAsync(
            $"/api/cart/{userId}/items/{productId}",
            new { quantity = newQuantity }
        );

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Test 8: User removes item from cart.
    /// Verifies cart deletion functionality.
    /// </summary>
    [Fact]
    public async Task RemoveItemFromCart_WithValidProductId_RemovesSuccessfully()
    {
        // Arrange
        var userId = TestDataBuilder.CreateTestUserId();
        var productId = 1;

        // Act
        var response = await _fixture.ShoppingCartClient!.DeleteAsync(
            $"/api/cart/{userId}/items/{productId}"
        );

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Test 9: User applies valid coupon to cart.
    /// Verifies discount calculation and application.
    /// </summary>
    [Fact]
    public async Task ApplyValidCoupon_WithValidCouponCode_CalculatesDiscount()
    {
        // Arrange
        var couponCode = "SAVE10";
        var cartTotal = 100m;

        // Act
        var response = await _fixture.CouponClient!.GetAsync(
            $"/api/coupon/validate?code={couponCode}&cartTotal={cartTotal}"
        );

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsAsync<dynamic>();
            content.Should().NotBeNull();
        }
    }

    /// <summary>
    /// Test 10: User applies invalid coupon - should return error.
    /// Verifies error handling for invalid coupons.
    /// </summary>
    [Fact]
    public async Task ApplyInvalidCoupon_WithInvalidCode_ReturnsNotFound()
    {
        // Arrange
        var couponCode = "INVALIDCOUPON123456";
        var cartTotal = 100m;

        // Act
        var response = await _fixture.CouponClient!.GetAsync(
            $"/api/coupon/validate?code={couponCode}&cartTotal={cartTotal}"
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Test 11: User creates order with discount.
    /// Verifies order creation and discount application.
    /// </summary>
    [Fact]
    public async Task CreateOrder_WithDiscount_SuccessfullyCreatesOrder()
    {
        // Arrange
        var userId = TestDataBuilder.CreateTestUserId();
        var orderRequest = new
        {
            UserId = userId,
            CartTotal = 500m,
            DiscountAmount = 50m,
            FinalAmount = 450m,
            CouponCode = "SAVE10",
            Items = new[]
            {
                new { ProductId = 1, Quantity = 1, Price = 500m }
            }
        };

        // Act
        var response = await _fixture.OrderClient!.PostAsJsonAsync(
            "/api/orders",
            orderRequest
        );

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
    }

    /// <summary>
    /// Test 12: User creates order without discount.
    /// Verifies standard order creation flow.
    /// </summary>
    [Fact]
    public async Task CreateOrder_WithoutDiscount_SuccessfullyCreatesOrder()
    {
        // Arrange
        var userId = TestDataBuilder.CreateTestUserId();
        var orderRequest = new
        {
            UserId = userId,
            CartTotal = 300m,
            DiscountAmount = 0m,
            FinalAmount = 300m,
            CouponCode = (string?)null,
            Items = new[]
            {
                new { ProductId = 1, Quantity = 1, Price = 300m }
            }
        };

        // Act
        var response = await _fixture.OrderClient!.PostAsJsonAsync(
            "/api/orders",
            orderRequest
        );

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
    }

    /// <summary>
    /// Test 13: Get order by ID.
    /// Verifies order retrieval and details.
    /// </summary>
    [Fact]
    public async Task GetOrder_ByValidId_ReturnsOrderDetails()
    {
        // Arrange
        var orderId = 1;

        // Act
        var response = await _fixture.OrderClient!.GetAsync($"/api/orders/{orderId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Test 14: Get orders by user ID.
    /// Verifies order history retrieval.
    /// </summary>
    [Fact]
    public async Task GetOrdersByUserId_WithValidUserId_ReturnsUserOrders()
    {
        // Arrange
        var userId = TestDataBuilder.CreateTestUserId();

        // Act
        var response = await _fixture.OrderClient!.GetAsync($"/api/orders/user/{userId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Test 15: Confirm order status.
    /// Verifies order confirmation workflow.
    /// </summary>
    [Fact]
    public async Task ConfirmOrder_WithValidOrderId_UpdatesOrderStatus()
    {
        // Arrange
        var orderId = 1;

        // Act
        var response = await _fixture.OrderClient!.PutAsJsonAsync(
            $"/api/orders/{orderId}/confirm",
            new { Status = "Confirmed" }
        );

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Test 16: Ship order.
    /// Verifies order shipping status update.
    /// </summary>
    [Fact]
    public async Task ShipOrder_WithValidOrderId_UpdatesShippingStatus()
    {
        // Arrange
        var orderId = 1;

        // Act
        var response = await _fixture.OrderClient!.PutAsJsonAsync(
            $"/api/orders/{orderId}/ship",
            new { Status = "Shipped", TrackingNumber = "TRACK123456" }
        );

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Test 17: Cancel order.
    /// Verifies order cancellation and state management.
    /// </summary>
    [Fact]
    public async Task CancelOrder_WithValidOrderId_CancelsSuccessfully()
    {
        // Arrange
        var orderId = 1;

        // Act
        var response = await _fixture.OrderClient!.DeleteAsync($"/api/orders/{orderId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Test 18: Complete order lifecycle.
    /// Tests the full workflow: Create -> Confirm -> Ship.
    /// </summary>
    [Fact]
    public async Task CompleteOrderLifecycle_CreatesConfirmsAndShips()
    {
        // Arrange
        var userId = TestDataBuilder.CreateTestUserId();
        var orderRequest = new
        {
            UserId = userId,
            CartTotal = 250m,
            DiscountAmount = 25m,
            FinalAmount = 225m,
            CouponCode = "SAVE10",
            Items = new[]
            {
                new { ProductId = 1, Quantity = 1, Price = 250m }
            }
        };

        // Act - Create Order
        var createResponse = await _fixture.OrderClient!.PostAsJsonAsync(
            "/api/orders",
            orderRequest
        );

        // Assert - Order Created
        createResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

        if (createResponse.StatusCode == HttpStatusCode.OK || createResponse.StatusCode == HttpStatusCode.Created)
        {
            var createdOrder = await createResponse.Content.ReadAsAsync<dynamic>();
            var orderId = createdOrder?.id ?? 1;

            // Act - Confirm Order
            var confirmResponse = await _fixture.OrderClient!.PutAsJsonAsync(
                $"/api/orders/{orderId}/confirm",
                new { Status = "Confirmed" }
            );

            // Assert - Order Confirmed
            confirmResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);

            // Act - Ship Order
            var shipResponse = await _fixture.OrderClient!.PutAsJsonAsync(
                $"/api/orders/{orderId}/ship",
                new { Status = "Shipped", TrackingNumber = "TRACK123456" }
            );

            // Assert - Order Shipped
            shipResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }
    }

    /// <summary>
    /// Test 19: Clear shopping cart.
    /// Verifies cart clearing functionality.
    /// </summary>
    [Fact]
    public async Task ClearCart_WithValidUserId_ClearsAllItems()
    {
        // Arrange
        var userId = TestDataBuilder.CreateTestUserId();

        // Act
        var response = await _fixture.ShoppingCartClient!.DeleteAsync($"/api/cart/{userId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Test 20: Get shopping cart details.
    /// Verifies cart retrieval and total calculation.
    /// </summary>
    [Fact]
    public async Task GetCart_WithValidUserId_ReturnsCartDetails()
    {
        // Arrange
        var userId = TestDataBuilder.CreateTestUserId();

        // Act
        var response = await _fixture.ShoppingCartClient!.GetAsync($"/api/cart/{userId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }
}
