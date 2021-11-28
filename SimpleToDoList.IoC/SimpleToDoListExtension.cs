using Microsoft.Extensions.DependencyInjection;
using SimpleToDoList.Application.Interfaces;
using SimpleToDoList.Application.Services;
using SimpleToDoList.Domain.Interfaces;
using SimpleToDoList.Infrastructure.Notifications;
using SimpleToDoList.Infrastructure.Providers;
using SimpleToDoList.Infrastructure.Repositories;

namespace SimpleToDoList.IoC
{
    public static class SimpleToDoListExtension
    {
        public static void AddSimpleToDoList(this IServiceCollection services, string dataBasePath)
        {
            services.AddScoped<IDatabaseFileProvider>(_ => new JsonFileProvider(dataBasePath));
            services.AddScoped<ISimpleTaskRepository, SimpleTaskRepository>();
            services.AddScoped<ISimpleTaskService, SimpleTaskService>();
            services.AddScoped<INotificationContext, NotificationContext>();
        }
    }
}
