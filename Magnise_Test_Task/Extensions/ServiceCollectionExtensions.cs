using Magnise_Test_Task.Data;
using Magnise_Test_Task.Interfaces;
using Magnise_Test_Task.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Magnise_Test_Task.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services)
        {
            services.AddSingleton<ITokenService, TokenService>();
            services.AddAutoMapper(typeof(AutoMapperProfile));
            services.AddSingleton<FintaTechService>();
            services.AddScoped<IAssetService, AssetService>();
            return services;
        }

        public static IServiceCollection AddDbContexts(this IServiceCollection services, IConfiguration configuration)
        {
            var server = configuration["DB_SERVER"];
            var database = configuration["DB_DATABASE"];
            var user = configuration["DB_USER"];
            var password = configuration["DB_PASSWORD"];

            var connectionString = $"Server={server},1433;Initial Catalog={database};User ID={user};Password={password};TrustServerCertificate=True";

            services.AddDbContext<MarketDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
            return services;
        }

        public static IServiceCollection AddHostedServices(this IServiceCollection services)
        {
            services.AddHostedService<AssetFetchingService>();
            services.AddHostedService<RealTimePricesService>();
            return services;
        }

        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            return services;
        }

        public static IServiceCollection AddControllersWithCustomOptions(this IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });
            return services;
        }
    }
}
