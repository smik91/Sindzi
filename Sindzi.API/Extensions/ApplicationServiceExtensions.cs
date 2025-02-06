using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sindzi.Bot;
using Sindzi.Common.Options;

namespace Sindzi.API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            services.Configure<TelegramBotOptions>(
                configuration.GetSection(
                    key: nameof(TelegramBotOptions)));

            services.Configure<MistralOptions>(
                configuration.GetSection(
                    key: nameof(MistralOptions)));

            services.AddHostedService<BotStarterService>();
            services.AddSingleton<MistralRequestService>();

            return services;
        }
    }
}
