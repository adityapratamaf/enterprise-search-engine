using System.Reflection;
using SearchEngine.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Mapster;

namespace SearchEngine.Application;

public static class DependencyInjection
{
    public static IServiceCollection
        AddApplication(
            this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(
                Assembly.GetExecutingAssembly());
        });

        services.AddValidatorsFromAssembly(
            Assembly.GetExecutingAssembly());

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        TypeAdapterConfig.GlobalSettings.Scan(
            typeof(DependencyInjection).Assembly);

        return services;
    }
}
