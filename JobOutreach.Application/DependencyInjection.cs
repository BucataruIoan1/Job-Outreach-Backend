using JobOutreach.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JobOutreach.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICountryService, CountryService>();
        services.AddScoped<IPositionService, PositionService>();
        services.AddScoped<IEmailSentService, EmailSentService>();
        return services;
    }
}
