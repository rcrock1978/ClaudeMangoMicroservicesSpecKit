using Microsoft.EntityFrameworkCore;
using Mango.Services.Payment.Infrastructure.Data;
using Mango.Services.Payment.Infrastructure.Repositories;
using Mango.Services.Payment.Infrastructure.PaymentGateways;
using Mango.Services.Payment.Application.Interfaces;
using Mango.Services.Payment.Application.Services;
using MediatR;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure database
var connectionString = builder.Configuration.GetConnectionString("PaymentDb")
    ?? "Server=localhost,1433;Database=Mango_Payment;User Id=sa;Password=YourPassword123!;Encrypt=false;TrustServerCertificate=true;";

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Mango.Services.Payment.Application.MediatR.BaseCommand).Assembly));

// Register repositories
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Register payment service
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Register payment gateway factory and gateways
builder.Services.AddHttpClient<IPaymentGatewayFactory, PaymentGatewayFactory>();

// Add Serilog logging
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// Add CORS if needed
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();

// Enable CORS
app.UseCors("AllowAll");

// Map health check endpoint
app.MapGet("/health", () => Results.Ok("OK")).WithName("HealthCheck");

// Map controllers
app.MapControllers();

app.Run();
