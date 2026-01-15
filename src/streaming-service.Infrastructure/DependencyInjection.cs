using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using streaming_service.Domain.Ports;
using streaming_service.Infrastructure.Authentication;
using streaming_service.Infrastructure.Messaging;
using streaming_service.Infrastructure.Persistence;
using streaming_service.Infrastructure.Services; // Kept for StubStreamingRecordingService
using streaming_service.Infrastructure.Messaging.Consumers;

namespace streaming_service.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Persistence (MongoDB)
            services.AddSingleton<MongoDbContext>();
            services.AddScoped<IStreamingSessionRepository, MongoStreamingSessionRepository>();
            services.AddScoped<IStreamingAccessRepository, MongoStreamingAccessRepository>();
            services.AddScoped<IAuditLogger, MongoAuditLogger>();

            // Messaging (RabbitMQ)
            services.AddSingleton<RabbitMQMessagePublisher>();
            services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMQMessagePublisher>());
            
            // Consumers
            services.AddScoped<ReservationConfirmedConsumer>();

            // Authentication (JWT)
            services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();

            // External Services
            // services.AddScoped<ISignalRService, SignalRService>(); // Moved to API
            services.AddScoped<IStreamingRecordingService, StubStreamingRecordingService>();
            services.AddScoped<IImageProcessor, StubImageProcessor>();

            return services;
        }
    }
}
