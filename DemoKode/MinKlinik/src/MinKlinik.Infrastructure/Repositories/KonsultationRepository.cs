using Microsoft.EntityFrameworkCore;
using MinKlinik.Domain.Entities;
using MinKlinik.Infrastructure.Persistence;
using MinKlinik.UseCases;

namespace MinKlinik.Infrastructure.Repositories;

public class KonsultationRepository : IKonsultationRepository
{
    private readonly AppDbContext _db;

    public KonsultationRepository(AppDbContext db) => _db = db;

    public async Task<Konsultation?> HentAsync(Guid id)
        => await _db.Konsultationer.FirstOrDefaultAsync(k => k.Id == id);

    public async Task<IReadOnlyList<Konsultation>> HentForPatientAsync(Guid patientId)
        => await _db.Konsultationer.Where(k => k.PatientId == patientId).ToListAsync();

    public async Task<IReadOnlyList<Konsultation>> HentForBehandlerAsync(Guid behandlerId)
        => await _db.Konsultationer.Where(k => k.BehandlerId == behandlerId).ToListAsync();

    public async Task TilføjAsync(Konsultation konsultation)
        => await _db.Konsultationer.AddAsync(konsultation);

    // BEMÆRK: Ingen Update()-kald!
    // EF tracker automatisk ændringer på entities der er loaded via HentAsync.
    public async Task GemAsync()
        => await _db.SaveChangesAsync();
}
