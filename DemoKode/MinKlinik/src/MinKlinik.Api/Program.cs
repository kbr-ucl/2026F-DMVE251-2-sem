using MinKlinik.Infrastructure;
using MinKlinik.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Web/API-specifikt
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Applikationslag — hvert lag ejer sin egen DI-opsætning
builder.Services
    .AddUseCases()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Seed testdata
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    new SeedData().Initialize(db);
}

// OpenAPI endpoint (genererer /openapi/v1.json)
app.MapOpenApi();

// Scalar UI (tilgængelig på /scalar/v1)
app.MapScalarApiReference();

app.MapControllers();

app.Run();
