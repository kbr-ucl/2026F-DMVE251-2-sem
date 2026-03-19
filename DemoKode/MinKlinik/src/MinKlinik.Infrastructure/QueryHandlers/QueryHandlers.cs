using Microsoft.EntityFrameworkCore;
using MinKlinik.Facade.DTOs;
using MinKlinik.Facade.Queries;
using MinKlinik.Infrastructure.Persistence;

namespace MinKlinik.Infrastructure.QueryHandlers;

public class KonsultationQueriesImpl : IKonsultationQueries
{
    private readonly AppDbContext _db;

    public KonsultationQueriesImpl(AppDbContext db) => _db = db;

    public async Task<KonsultationDto?> HentAsync(Guid id)
    {
        // Ingen Include — vi har kun Guid-referencer, ingen navigation properties.
        // Navn-felter hentes via separate lookups eller joins.
        return await _db.Konsultationer
            .AsNoTracking()
            .Where(k => k.Id == id)
            .Select(k => new KonsultationDto(
                k.Id,
                k.Tidspunkt.Fra,
                k.Tidspunkt.Til,
                k.BehandlingstypeId,
                _db.Behandlingstyper.Where(bt => bt.Id == k.BehandlingstypeId).Select(bt => bt.Navn).FirstOrDefault() ?? "",
                k.PatientId,
                _db.Patienter.Where(p => p.Id == k.PatientId).Select(p => p.Navn).FirstOrDefault() ?? "",
                k.BehandlerId,
                _db.Behandlere.Where(b => b.Id == k.BehandlerId).Select(b => b.Navn).FirstOrDefault() ?? "",
                k.Status.ToString(),
                k.Notat))
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<KonsultationDto>> HentAlleAsync()
    {
        return await _db.Konsultationer
            .AsNoTracking()
            .Select(k => new KonsultationDto(
                k.Id,
                k.Tidspunkt.Fra,
                k.Tidspunkt.Til,
                k.BehandlingstypeId,
                _db.Behandlingstyper.Where(bt => bt.Id == k.BehandlingstypeId).Select(bt => bt.Navn).FirstOrDefault() ?? "",
                k.PatientId,
                _db.Patienter.Where(p => p.Id == k.PatientId).Select(p => p.Navn).FirstOrDefault() ?? "",
                k.BehandlerId,
                _db.Behandlere.Where(b => b.Id == k.BehandlerId).Select(b => b.Navn).FirstOrDefault() ?? "",
                k.Status.ToString(),
                k.Notat))
            .ToListAsync();
    }
}

public class BehandlingstypeQueriesImpl : IBehandlingstypeQueries
{
    private readonly AppDbContext _db;
    public BehandlingstypeQueriesImpl(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<BehandlingstypeDto>> HentAlleAsync()
        => await _db.Behandlingstyper.AsNoTracking()
            .Select(b => new BehandlingstypeDto(b.Id, b.Navn))
            .ToListAsync();
}

public class PatientQueriesImpl : IPatientQueries
{
    private readonly AppDbContext _db;
    public PatientQueriesImpl(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<PatientDto>> HentAlleAsync()
        => await _db.Patienter.AsNoTracking()
            .Select(p => new PatientDto(p.Id, p.Navn))
            .ToListAsync();
}

public class BehandlerQueriesImpl : IBehandlerQueries
{
    private readonly AppDbContext _db;
    public BehandlerQueriesImpl(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<BehandlerDto>> HentAlleAsync()
        => await _db.Behandlere.AsNoTracking()
            .Select(b => new BehandlerDto(b.Id, b.Navn, b.Speciale))
            .ToListAsync();
}
