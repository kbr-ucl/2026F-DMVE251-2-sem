using MinKlinik.Domain.Entities;
using MinKlinik.Domain.Exceptions;
using MinKlinik.Domain.ValueObjects;
using MinKlinik.Facade.DTOs;
using MinKlinik.Facade.UseCases;

namespace MinKlinik.UseCases.Konsultationer;

public class OpretKonsultationUseCase : IOpretKonsultationUseCase
{
    private readonly IKonsultationRepository _konsultationRepo;
    private readonly IBehandlingstypeRepository _behandlingstypeRepo;
    private readonly IPatientRepository _patientRepo;
    private readonly IBehandlerRepository _behandlerRepo;

    public OpretKonsultationUseCase(
        IKonsultationRepository konsultationRepo,
        IBehandlingstypeRepository behandlingstypeRepo,
        IPatientRepository patientRepo,
        IBehandlerRepository behandlerRepo)
    {
        _konsultationRepo = konsultationRepo;
        _behandlingstypeRepo = behandlingstypeRepo;
        _patientRepo = patientRepo;
        _behandlerRepo = behandlerRepo;
    }

    public async Task Udfør(OpretKonsultationRequest request)
    {
        // 1. Materialiser: verificér at de refererede aggregater eksisterer
        _ = await _behandlingstypeRepo.HentAsync(request.BehandlingstypeId)
            ?? throw new NotFoundException("Behandlingstype ikke fundet.");
        _ = await _patientRepo.HentAsync(request.PatientId)
            ?? throw new NotFoundException("Patient ikke fundet.");
        _ = await _behandlerRepo.HentAsync(request.BehandlerId)
            ?? throw new NotFoundException("Behandler ikke fundet.");

        var patientBookinger = await _konsultationRepo.HentForPatientAsync(request.PatientId);
        var behandlerBookinger = await _konsultationRepo.HentForBehandlerAsync(request.BehandlerId);

        // 2. Forretningslogik via factory-metode på Aggregate Root
        //    Konsultation modtager Guid'er — IKKE objektreferencer.
        var tidspunkt = new Tidsinterval(request.Fra, request.Til);
        var konsultation = Konsultation.Opret(
            tidspunkt, request.BehandlingstypeId,
            request.PatientId, request.BehandlerId,
            patientBookinger, behandlerBookinger);

        // 3. Persistér
        await _konsultationRepo.TilføjAsync(konsultation);
        await _konsultationRepo.GemAsync();
    }
}
