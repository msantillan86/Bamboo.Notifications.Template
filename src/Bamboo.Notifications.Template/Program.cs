using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Serilog;
using ProtocolManager.Contracts;
using ProtocolManager.REST;
using Bamboo.Serilog.Context;
using Bamboo.Notifications.Template.Application;
using Bamboo.Notifications.Template.HostedServices;
using Bamboo.Notifications.Template.Application.Configurations;
using Bamboo.Notifications.Template.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseBambooSerilog();
builder.Services.AddHealthChecks();
builder.Services.AddOptions();
builder.Services.Configure<BrokerConfig>(builder.Configuration.GetSection("BrokerConfig"));
builder.Services.Configure<ServicesConfig>(builder.Configuration.GetSection("ServicesConfig"));
builder.Services.RegisterSQSForRetries(builder.Configuration);
builder.Services.AddSingleton<IProtocolManager, RestProtocol>();
builder.Services.AddScoped<IBambooSerilogContext, BambooSerilogContext>();
builder.Services.AddHostedService<HostedService>();

// Add services to the container.
builder.Services
    .AddApplicationServices()
    .AddInfrastuctureServices();

var app = builder.Build();

app.UseLogHashKeyMiddleware();
app.UseBambooSerilogRequestLogging();
app.Run();