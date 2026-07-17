using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using CommerceHub.Modules.Identity.Application.Common.Interfaces;
using CommerceHub.Modules.Identity.Application.Services;

namespace CommerceHub.Modules.Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
        services.AddScoped<IGdprService, GdprService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        return services;
    }
}
