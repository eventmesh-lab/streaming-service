using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using streaming_service.Application.UseCases;
using streaming_service.Domain.Ports;
using streaming_service.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// DI: registramos implementaciones concretas
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrations (composition root)
// Reemplazar por registros reales (EF, HttpClient, Messaging, etc.)
builder.Services.AddScoped<IExampleRepository, InMemoryExampleRepository>();
builder.Services.AddScoped<CreateExampleUseCase>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
