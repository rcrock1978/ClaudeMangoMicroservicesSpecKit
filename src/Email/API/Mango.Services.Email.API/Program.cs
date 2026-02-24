using MassTransit;
using Mango.Services.Email.Application.Interfaces;
using Mango.Services.Email.Application.Services;
using Mango.Services.Email.Infrastructure.Consumers;
using Mango.Services.Email.Infrastructure.Data;
using Mango.Services.Email.Infrastructure.Repositories;
using Mango.MessageBus.Events;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<EmailDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.MigrationsAssembly("Mango.Services.Email.Infrastructure");
        sqlOptions.CommandTimeout(30);
    }));

// Email settings configuration
var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>()
    ?? throw new InvalidOperationException("EmailSettings configuration is missing");
builder.Services.AddSingleton(emailSettings);

// Application services
builder.Services.AddScoped<IEmailRepository, EmailRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();

// MassTransit configuration
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var mqSettings = builder.Configuration.GetSection("MassTransit:RabbitMQ");
        var host = mqSettings.GetValue<string>("Host") ?? "localhost";
        var port = mqSettings.GetValue<int>("Port");
        var username = mqSettings.GetValue<string>("Username") ?? "guest";
        var password = mqSettings.GetValue<string>("Password") ?? "guest";
        var vhost = mqSettings.GetValue<string>("VirtualHost") ?? "/";

        cfg.Host(host, port, vhost, h =>
        {
            h.Username(username);
            h.Password(password);
        });

        // Configure retry policy
        cfg.UseMessageRetry(r =>
        {
            r.Interval(3, TimeSpan.FromSeconds(5));
        });

        cfg.ConfigureEndpoints(context);
    });
});

// MediatR configuration
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Email Service API V1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EmailDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
