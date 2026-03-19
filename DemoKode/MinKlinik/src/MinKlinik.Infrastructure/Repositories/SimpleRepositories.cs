using Microsoft.EntityFrameworkCore;
using MinKlinik.Domain.Entities;
using MinKlinik.Infrastructure.Persistence;
using MinKlinik.UseCases;

namespace MinKlinik.Infrastructure.Repositories;

public class BehandlingstypeRepository : IBehandlingstypeRepository
{
    private readonly AppDbContext _db;
    public BehandlingstypeRepository(AppDbContext db) => _db = db;

    public async Task<Behandlingstype?> HentAsync(Guid id)
        => await _db.Behandlingstyper.FindAsync(id);
}

public class PatientRepository : IPatientRepository
{
    private readonly AppDbContext _db;
    public PatientRepository(AppDbContext db) => _db = db;

    public async Task<Patient?> HentAsync(Guid id)
        => await _db.Patienter.FindAsync(id);
}

public class BehandlerRepository : IBehandlerRepository
{
    private readonly AppDbContext _db;
    public BehandlerRepository(AppDbContext db) => _db = db;

    public async Task<Behandler?> HentAsync(Guid id)
        => await _db.Behandlere.FindAsync(id);
}
