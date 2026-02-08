using JobOutreach.Application.Interfaces;
using JobOutreach.Infrastructure.Persistence;
using JobOutreach.Infrastructure.Repositories;
using JobOutreach.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobOutreach.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IEmailRepository, EmailRepository>();
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<IEmailSentRepository, EmailSentRepository>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        return services;
    }
}
