using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

namespace Mango.E2E.Tests.Fixtures;

/// <summary>
/// Base fixture for E2E tests providing HttpClient and shared services configuration.
/// Manages multiple API service clients for testing the complete microservices workflow.
/// </summary>
public class E2ETestFixture : IAsyncLifetime
{
    private WebApplicationFactory<Program>? _productFactory;
    private WebApplicationFactory<Program>? _shoppingCartFactory;
    private WebApplicationFactory<Program>? _orderFactory;
    private WebApplicationFactory<Program>? _couponFactory;
    private WebApplicationFactory<Program>? _emailFactory;
    private WebApplicationFactory<Program>? _rewardFactory;

    public HttpClient? ProductClient { get; private set; }
    public HttpClient? ShoppingCartClient { get; private set; }
    public HttpClient? OrderClient { get; private set; }
    public HttpClient? CouponClient { get; private set; }
    public HttpClient? EmailClient { get; private set; }
    public HttpClient? RewardClient { get; private set; }

    /// <summary>
    /// Initialize all service clients for E2E testing.
    /// This creates separate HTTP clients for each microservice to simulate real requests.
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            // Initialize Product Service
            _productFactory = new WebApplicationFactory<Mango.Services.Product.API.Program>();
            ProductClient = _productFactory.CreateClient();
            ProductClient.BaseAddress = new Uri("http://localhost:5001");

            // Initialize Shopping Cart Service
            _shoppingCartFactory = new WebApplicationFactory<Mango.Services.ShoppingCart.API.Program>();
            ShoppingCartClient = _shoppingCartFactory.CreateClient();
            ShoppingCartClient.BaseAddress = new Uri("http://localhost:5002");

            // Initialize Order Service
            _orderFactory = new WebApplicationFactory<Mango.Services.Order.API.Program>();
            OrderClient = _orderFactory.CreateClient();
            OrderClient.BaseAddress = new Uri("http://localhost:5003");

            // Initialize Coupon Service
            _couponFactory = new WebApplicationFactory<Mango.Services.Coupon.API.Program>();
            CouponClient = _couponFactory.CreateClient();
            CouponClient.BaseAddress = new Uri("http://localhost:5004");

            // Initialize Email Service
            _emailFactory = new WebApplicationFactory<Mango.Services.Email.API.Program>();
            EmailClient = _emailFactory.CreateClient();
            EmailClient.BaseAddress = new Uri("http://localhost:5005");

            // Initialize Reward Service
            _rewardFactory = new WebApplicationFactory<Mango.Services.Reward.API.Program>();
            RewardClient = _rewardFactory.CreateClient();
            RewardClient.BaseAddress = new Uri("http://localhost:5006");

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to initialize E2E test fixture", ex);
        }
    }

    /// <summary>
    /// Cleanup all service clients and factories after tests complete.
    /// </summary>
    public async Task DisposeAsync()
    {
        ProductClient?.Dispose();
        ShoppingCartClient?.Dispose();
        OrderClient?.Dispose();
        CouponClient?.Dispose();
        EmailClient?.Dispose();
        RewardClient?.Dispose();

        _productFactory?.Dispose();
        _shoppingCartFactory?.Dispose();
        _orderFactory?.Dispose();
        _couponFactory?.Dispose();
        _emailFactory?.Dispose();
        _rewardFactory?.Dispose();

        await Task.CompletedTask;
    }
}

/// <summary>
/// Test collection fixture that shares E2E test fixture across all tests in the collection.
/// </summary>
[CollectionDefinition("E2E Tests")]
public class E2ETestCollection : ICollectionFixture<E2ETestFixture>
{
}
