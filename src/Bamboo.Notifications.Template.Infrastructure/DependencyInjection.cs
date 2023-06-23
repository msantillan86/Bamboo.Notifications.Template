using Microsoft.Extensions.DependencyInjection;
using Bamboo.Notifications.Template.Application.Contracts;
using Bamboo.Notifications.Template.Infrastructure.Services;

namespace Bamboo.Notifications.Template.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastuctureServices(this IServiceCollection services)
    {
        services.AddTransient<IBrokerConsumer, BrokerConsumer>();
        services.AddTransient<IAntifraudService, AntifraudService>();
        return services;
    }
}