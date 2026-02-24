using Microsoft.EntityFrameworkCore;
using Mango.Services.Order.Infrastructure.Data;
using Mango.Services.Order.Infrastructure.Repositories;
using Mango.Services.Order.Application.Interfaces;
using MediatR;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure database
var connectionString = builder.Configuration.GetConnectionString("OrderDb")
    ?? "Server=localhost,1433;Database=Mango_Order;User Id=sa;Password=YourPassword123!;Encrypt=false;TrustServerCertificate=true;";

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Mango.Services.Order.Application.MediatR.BaseCommand).Assembly));

// Register repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Add Serilog logging
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();

// Map health check endpoint
app.MapGet("/health", () => Results.Ok("OK")).WithName("HealthCheck");

// Map controllers
app.MapControllers();

app.Run();
