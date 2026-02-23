using Microsoft.EntityFrameworkCore;
using Mango.Services.Product.Infrastructure.Data;
using Mango.Services.Product.Infrastructure.Repositories;
using Mango.Services.Product.Application.Interfaces;
using MediatR;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure database
var connectionString = builder.Configuration.GetConnectionString("ProductDb")
    ?? "Server=localhost,1433;Database=Mango_Product;User Id=sa;Password=YourPassword123!;Encrypt=false;TrustServerCertificate=true;";

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Mango.Services.Product.Application.MediatR.Queries.GetProductQuery).Assembly));

// Register repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();

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
