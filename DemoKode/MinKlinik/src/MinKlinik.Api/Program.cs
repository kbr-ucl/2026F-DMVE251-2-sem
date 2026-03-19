using Microsoft.EntityFrameworkCore;
using MinKlinik.Facade.Queries;
using MinKlinik.Infrastructure;
using MinKlinik.Facade.UseCases;
using MinKlinik.Infrastructure.Persistence;
using MinKlinik.Infrastructure.QueryHandlers;
using MinKlinik.Infrastructure.Repositories;
using MinKlinik.UseCases;
using MinKlinik.UseCases.Konsultationer;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI
builder.Services.AddOpenApi();

// Controllers
builder.Services.AddControllers();

// Database (InMemory til udvikling — skift til UseSqlServer for produktion)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("MinKlinikDb"));

// Repositories (Use Case-interfaces → Infrastructure-implementeringer)
builder.Services.AddScoped<IKonsultationRepository, KonsultationRepository>();
builder.Services.AddScoped<IBehandlingstypeRepository, BehandlingstypeRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IBehandlerRepository, BehandlerRepository>();

// Use Cases (Facade-interfaces → Use Case-implementeringer)
builder.Services.AddScoped<IOpretKonsultationUseCase, OpretKonsultationUseCase>();
builder.Services.AddScoped<IAfslutKonsultationUseCase, AfslutKonsultationUseCase>();
builder.Services.AddScoped<IAflysKonsultationUseCase, AflysKonsultationUseCase>();

// Queries (Facade-interfaces → Infrastructure-implementeringer)
builder.Services.AddScoped<IKonsultationQueries, KonsultationQueriesImpl>();
builder.Services.AddScoped<IBehandlingstypeQueries, BehandlingstypeQueriesImpl>();
builder.Services.AddScoped<IPatientQueries, PatientQueriesImpl>();
builder.Services.AddScoped<IBehandlerQueries, BehandlerQueriesImpl>();

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

