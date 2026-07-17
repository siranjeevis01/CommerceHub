using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using CommerceHub.Modules.Analytics.Application.Interfaces;
using CommerceHub.Modules.Analytics.Application.Services;

namespace CommerceHub.Modules.Analytics.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddValidatorsFromAssembly(assembly: Assembly.GetExecutingAssembly());

        services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));

        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
