using Bamboo.Notifications.Template.Application.Contracts;
using Bamboo.Notifications.Template.Application.Handlers;
using Bamboo.Notifications.Template.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bamboo.Notifications.Template.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddTransient<IListenerService, ListenerService>();
            services.AddScoped<TransactionCompletedHandler>();
            return services;
        }
    }
}
