using MassTransit;
using Mango.Services.Reward.Application.Interfaces;
using Mango.Services.Reward.Application.Services;
using Mango.Services.Reward.Infrastructure.Consumers;
using Mango.Services.Reward.Infrastructure.Data;
using Mango.Services.Reward.Infrastructure.Repositories;
using Mango.MessageBus.Events;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<RewardDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IRewardRepository, RewardRepository>();
builder.Services.AddScoped<IRewardService, RewardService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCompletedEventConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        var mqSettings = builder.Configuration.GetSection("MassTransit:RabbitMQ");
        cfg.Host(
            mqSettings.GetValue<string>("Host") ?? "localhost",
            mqSettings.GetValue<int>("Port"),
            mqSettings.GetValue<string>("VirtualHost") ?? "/",
            h =>
            {
                h.Username(mqSettings.GetValue<string>("Username") ?? "guest");
                h.Password(mqSettings.GetValue<string>("Password") ?? "guest");
            });
        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddLogging();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Reward Service API V1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RewardDbContext>();
    await db.Database.MigrateAsync();

    // Seed reward tiers
    if (!await db.RewardTiers.AnyAsync())
    {
        db.RewardTiers.AddRange(Mango.Services.Reward.Domain.Entities.RewardTier.CreateDefaultTiers());
        await db.SaveChangesAsync();
    }
}

app.Run();
