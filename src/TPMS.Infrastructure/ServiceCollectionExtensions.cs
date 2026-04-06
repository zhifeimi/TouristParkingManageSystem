using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TPMS.Application.Abstractions;
using TPMS.Infrastructure.Auth;
using TPMS.Infrastructure.Availability;
using TPMS.Infrastructure.BackgroundJobs;
using TPMS.Infrastructure.Payments;
using TPMS.Infrastructure.Persistence;
using TPMS.Infrastructure.Persistence.Interceptors;
using TPMS.Infrastructure.Realtime;

namespace TPMS.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.SectionName));

        services.AddScoped<RowVersionSaveChangesInterceptor>();
        services.AddScoped<OutboxSaveChangesInterceptor>();

        services.AddDbContext<TpmsDbContext>((serviceProvider, options) =>
        {
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

            if (string.Equals(databaseOptions.Provider, "SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                var connectionString = BuildSqlServerConnectionString(configuration);
                options.UseSqlServer(connectionString);
            }
            else
            {
                var connectionString = configuration.GetConnectionString("TpmsDatabase") ?? "Data Source=tpms.db";
                options.UseSqlite(connectionString);
            }

            options.AddInterceptors(
                serviceProvider.GetRequiredService<RowVersionSaveChangesInterceptor>(),
                serviceProvider.GetRequiredService<OutboxSaveChangesInterceptor>());
        });

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<TpmsDbContext>();

        services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IParkingLotRepository, ParkingLotRepository>();
        services.AddScoped<IParkingBayRepository, ParkingBayRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPermitRepository, PermitRepository>();
        services.AddScoped<IViolationCaseRepository, ViolationCaseRepository>();
        services.AddScoped<IEdgeNodeRepository, EdgeNodeRepository>();
        services.AddScoped<IAvailabilityReadService, AvailabilityReadService>();
        services.AddScoped<IPermitReadService, PermitReadService>();
        services.AddScoped<IEdgeSyncService, EdgeSyncService>();
        services.AddScoped<DatabaseSeeder>();

        services.AddHttpClient<IPaymentGateway, StripePaymentGateway>();
        services.AddSingleton<TPMS.Infrastructure.Lpr.MockLprRecognitionService>();

        services.AddHostedService<ReservationExpiryBackgroundService>();
        services.AddHostedService<OutboxProcessorBackgroundService>();

        return services;
    }

    private static string BuildSqlServerConnectionString(IConfiguration configuration)
    {
        var configuredConnectionString =
            configuration.GetConnectionString("TpmsDatabase") ?? "Server=localhost,1433;Database=NationalPark;TrustServerCertificate=True;MultipleActiveResultSets=true";

        var server = Environment.GetEnvironmentVariable("dev_mssql_server");
        var port = Environment.GetEnvironmentVariable("dev_mssql_port");
        var userId = Environment.GetEnvironmentVariable("dev_mssql_id");
        var password = Environment.GetEnvironmentVariable("dev_mssql_password");

        if (string.IsNullOrWhiteSpace(server) ||
            string.IsNullOrWhiteSpace(port) ||
            string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(password))
        {
            return configuredConnectionString;
        }

        var builder = new SqlConnectionStringBuilder(configuredConnectionString)
        {
            DataSource = $"{server},{port}",
            UserID = userId,
            Password = password,
            TrustServerCertificate = true,
            MultipleActiveResultSets = true
        };

        return builder.ConnectionString;
    }
}
