# Order Service - Quick Start Guide

## File Locations

All Order Service files are located in:
- **Domain**: `/src/Order/Domain/Mango.Services.Order.Domain/`
- **Application**: `/src/Order/Application/Mango.Services.Order.Application/`
- **Infrastructure**: `/src/Order/Infrastructure/Mango.Services.Order.Infrastructure/`
- **API**: `/src/Order/API/Mango.Services.Order.API/`
- **Tests**: `/tests/Mango.Services.Order.UnitTests/`

## Building the Service

```bash
# Build all Order Service projects
dotnet build "src/Order/Domain/Mango.Services.Order.Domain/Mango.Services.Order.Domain.csproj"
dotnet build "src/Order/Application/Mango.Services.Order.Application/Mango.Services.Order.Application.csproj"
dotnet build "src/Order/Infrastructure/Mango.Services.Order.Infrastructure/Mango.Services.Order.Infrastructure.csproj"
dotnet build "src/Order/API/Mango.Services.Order.API/Mango.Services.Order.API.csproj"
```

## Running Tests

```bash
# Run all 41 unit tests
dotnet test "tests/Mango.Services.Order.UnitTests/Mango.Services.Order.UnitTests.csproj"

# Expected output: Passed! - Failed: 0, Passed: 41, Skipped: 0
```

## Running the API

```bash
# Start the Order API
dotnet run --project "src/Order/API/Mango.Services.Order.API"

# API will be available at:
# https://localhost:7001/swagger (Swagger UI)
# http://localhost:5001 (HTTP)
```

## API Endpoints

### Create Order
```bash
POST /api/orders
Content-Type: application/json

{
  "userId": "user123",
  "shippingAddress": "123 Main St, City, State 12345",
  "billingAddress": "456 Billing St, City, State 12345",
  "paymentMethod": "CreditCard",
  "discountAmount": 25.00,
  "shippingCost": 10.00,
  "tax": 20.00,
  "items": [
    {
      "productId": 1,
      "productName": "Laptop",
      "unitPrice": 999.99,
      "quantity": 1
    }
  ]
}
```

### Get Order
```bash
GET /api/orders/1
```

### Get User Orders
```bash
GET /api/orders/user/user123?pageNumber=1&pageSize=10
```

### Update Order Status
```bash
PUT /api/orders/1/status
Content-Type: application/json

{
  "status": 2,
  "paymentTransactionId": "TXN-123456"
}
```

### Cancel Order
```bash
DELETE /api/orders/1
```

## Database Setup

### Create Database
```bash
# From the project root directory
dotnet ef database update \
  --project "src/Order/Infrastructure/Mango.Services.Order.Infrastructure" \
  --context OrderDbContext
```

### Connection String
Update in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "OrderDb": "Server=localhost,1433;Database=Mango_Order;User Id=sa;Password=YourPassword123!;Encrypt=false;TrustServerCertificate=true;"
  }
}
```

## Order State Machine

States and Valid Transitions:

```
Pending
  ├→ Processing (via ConfirmOrder())
  └→ Cancelled (via CancelOrder())

Processing
  ├→ Shipped (via ProcessPayment())
  └→ Cancelled (via CancelOrder())

Shipped
  └→ Delivered (via RecordDelivery())

Delivered (Final)

Cancelled (Final)
```

## Domain Model

### Order Entity
- **UserId**: Customer ID
- **OrderNumber**: Unique identifier (auto-generated)
- **OrderStatus**: Current state (Pending/Processing/Shipped/Delivered/Cancelled)
- **OrderDate**: When order was created
- **DeliveryDate**: When order was delivered
- **ShippingAddress**: Delivery address
- **BillingAddress**: Invoice address
- **PaymentMethod**: Payment type
- **PaymentTransactionId**: Payment gateway reference
- **DiscountAmount**: Total discount
- **ShippingCost**: Shipping fee
- **Tax**: Tax amount
- **TotalAmount**: Final total
- **Items**: Collection of OrderItem
- **IsDeleted**: Soft delete flag
- **CreatedAt/UpdatedAt**: Audit fields

### OrderItem Entity
- **ProductId**: Product reference
- **ProductName**: Product name (denormalized)
- **UnitPrice**: Price at time of order
- **Quantity**: Number of units
- **TotalPrice**: UnitPrice × Quantity
- **DiscountAmount**: Item discount

## Key Methods

### Order.AddItem()
Adds item to order (Pending state only)
```csharp
order.AddItem(productId: 1, productName: "Laptop", unitPrice: 999.99m, quantity: 1)
```

### Order.ConfirmOrder()
Transitions Pending → Processing
```csharp
order.ConfirmOrder()  // Returns bool
```

### Order.ProcessPayment()
Transitions Processing → Shipped
```csharp
order.ProcessPayment(transactionId: "TXN-12345")  // Returns bool
```

### Order.RecordDelivery()
Transitions Shipped → Delivered
```csharp
order.RecordDelivery()  // Returns bool
```

### Order.CancelOrder()
Transitions to Cancelled (from Pending or Processing)
```csharp
order.CancelOrder()  // Returns bool
```

## Configuration Files

### Mango.Services.Order.API.csproj
Located at: `/src/Order/API/Mango.Services.Order.API/Mango.Services.Order.API.csproj`

Project references:
- Mango.Services.Order.Domain
- Mango.Services.Order.Application
- Mango.Services.Order.Infrastructure

### Program.cs
Located at: `/src/Order/API/Mango.Services.Order.API/Program.cs`

Configures:
- DbContext with SQL Server
- MediatR for CQRS
- Repository dependency injection
- Serilog logging
- OpenAPI (Swagger)

### appsettings.json
Located at: `/src/Order/API/Mango.Services.Order.API/appsettings.json`

Contains:
- Logging configuration
- SQL Server connection string
- Application settings

## Environment Variables

Set in `appsettings.json` or environment:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "ConnectionStrings": {
    "OrderDb": "Server=YOUR_SERVER;Database=Mango_Order;..."
  }
}
```

## Common Tasks

### View All Tests
```bash
dotnet test "tests/Mango.Services.Order.UnitTests/Mango.Services.Order.UnitTests.csproj" --list-tests
```

### Run Specific Test
```bash
dotnet test "tests/Mango.Services.Order.UnitTests/Mango.Services.Order.UnitTests.csproj" \
  --filter "Order_ConfirmOrder_FromPending_Succeeds"
```

### Build in Release Mode
```bash
dotnet build "src/Order/API/Mango.Services.Order.API/Mango.Services.Order.API.csproj" -c Release
```

### Publish API
```bash
dotnet publish "src/Order/API/Mango.Services.Order.API/Mango.Services.Order.API.csproj" \
  -c Release -o ./publish
```

## Troubleshooting

### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet build
```

### Test Failures
```bash
# Run with verbose output
dotnet test --logger "console;verbosity=detailed"
```

### Database Connection Issues
- Verify SQL Server is running
- Check connection string in appsettings.json
- Ensure database exists or migrations have run

### Port Already in Use
```bash
# Use different port
dotnet run --project "src/Order/API/Mango.Services.Order.API" \
  -- --urls="http://localhost:6000;https://localhost:6001"
```

## Important Files Reference

| File | Purpose |
|------|---------|
| Order.cs | Main aggregate root with state machine |
| OrderItem.cs | Order line items |
| OrderStatus.cs | State enum |
| OrderDbContext.cs | Entity Framework configuration |
| OrderRepository.cs | Data access layer |
| CreateOrderCommand.cs | Create order use case |
| UpdateOrderStatusCommand.cs | Update status use case |
| OrdersController.cs | REST API endpoints |
| OrderEntityTests.cs | 41 unit tests |

## Testing Tips

- All tests use FluentAssertions for readable assertions
- Tests follow AAA pattern (Arrange, Act, Assert)
- Domain tests verify business rules and state transitions
- Run tests before committing changes

## Performance Notes

- Order queries include indexes on: UserId, OrderNumber, OrderStatus, OrderDate
- Pagination is implemented for large result sets
- Soft delete filter applied automatically to queries
- EF Core eager loads Items collection

## Security Considerations

- Implement JWT authentication in Program.cs
- Add authorization attributes to controller actions
- Validate user ownership of orders before operations
- Use HTTPS in production
- Sanitize user inputs in API

## Integration Notes

The Order Service can integrate with:
- **Payment Service**: Process payments via ProcessPayment()
- **Notification Service**: Send order confirmations, shipment, delivery notifications
- **Product Service**: Validate product availability and pricing
- **Inventory Service**: Decrement stock when order is confirmed
- **Email Service**: Send order confirmations and updates

## Documentation

Full documentation: `/ORDER_SERVICE_IMPLEMENTATION.md`

For detailed architecture, design patterns, and complete API examples, refer to the comprehensive guide.
