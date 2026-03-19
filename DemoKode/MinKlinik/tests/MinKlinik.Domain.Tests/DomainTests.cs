using MinKlinik.Domain.Entities;
using MinKlinik.Domain.Enums;
using MinKlinik.Domain.Exceptions;
using MinKlinik.Domain.ValueObjects;
using Xunit;

namespace MinKlinik.Domain.Tests;

public class TidsintervalTests
{
    [Fact]
    public void Constructor_MedTilFørFra_KasterDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new Tidsinterval(DateTime.UtcNow.AddHours(2), DateTime.UtcNow.AddHours(1)));
    }

    [Fact]
    public void OverlapperMed_OverlappendeIntervaller_ReturnererTrue()
    {
        var a = new Tidsinterval(Dag(9), Dag(10));
        var b = new Tidsinterval(Dag(9, 30), Dag(10, 30));
        Assert.True(a.OverlapperMed(b));
        Assert.True(b.OverlapperMed(a));
    }

    [Fact]
    public void OverlapperMed_TilstødendeIntervaller_ReturnererFalse()
    {
        var a = new Tidsinterval(Dag(9), Dag(10));
        var b = new Tidsinterval(Dag(10), Dag(11));
        Assert.False(a.OverlapperMed(b));
    }

    [Fact]
    public void Varighed_BeregnesKorrekt()
    {
        var t = new Tidsinterval(Dag(9), Dag(10, 30));
        Assert.Equal(TimeSpan.FromMinutes(90), t.Varighed);
    }

    private DateTime Dag(int time, int min = 0)
        => DateTime.UtcNow.AddDays(1).Date.AddHours(time).AddMinutes(min);
}

public class KonsultationTests
{
    private readonly Guid TypeId = Guid.NewGuid();
    private readonly Guid PatientId = Guid.NewGuid();
    private readonly Guid BehandlerId = Guid.NewGuid();

    private Tidsinterval Tid(int fraTime, int tilTime)
        => new(DateTime.UtcNow.AddDays(1).Date.AddHours(fraTime),
               DateTime.UtcNow.AddDays(1).Date.AddHours(tilTime));

    private Konsultation OpretUdenOverlap(
        Tidsinterval? tidspunkt = null,
        Guid? patientId = null,
        Guid? behandlerId = null)
    {
        return Konsultation.Opret(
            tidspunkt ?? Tid(9, 10),
            TypeId,
            patientId ?? PatientId,
            behandlerId ?? BehandlerId,
            eksisterendeForPatient: Array.Empty<Konsultation>(),
            eksisterendeForBehandler: Array.Empty<Konsultation>());
    }

    [Fact]
    public void Opret_MedGyldigeData_SætterStatusTilPlanlagt()
    {
        var k = OpretUdenOverlap();
        Assert.Equal(KonsultationStatus.Planlagt, k.Status);
    }

    [Fact]
    public void Opret_MedTidspunktIFortiden_KasterDomainException()
    {
        var fortid = new Tidsinterval(DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));
        Assert.Throws<DomainException>(() =>
            Konsultation.Opret(fortid, TypeId, PatientId, BehandlerId,
                Array.Empty<Konsultation>(), Array.Empty<Konsultation>()));
    }

    [Fact]
    public void Opret_MedPatientOverlap_KasterDomainException()
    {
        var patientId = Guid.NewGuid();
        var eksisterende = Konsultation.Opret(
            Tid(9, 10), TypeId, patientId, BehandlerId,
            Array.Empty<Konsultation>(), Array.Empty<Konsultation>());

        Assert.Throws<DomainException>(() =>
            Konsultation.Opret(
                Tid(9, 11), TypeId, patientId, Guid.NewGuid(),
                eksisterendeForPatient: new[] { eksisterende },
                eksisterendeForBehandler: Array.Empty<Konsultation>()));
    }

    [Fact]
    public void Opret_MedBehandlerOverlap_KasterDomainException()
    {
        var behandlerId = Guid.NewGuid();
        var eksisterende = Konsultation.Opret(
            Tid(9, 10), TypeId, PatientId, behandlerId,
            Array.Empty<Konsultation>(), Array.Empty<Konsultation>());

        Assert.Throws<DomainException>(() =>
            Konsultation.Opret(
                Tid(9, 11), TypeId, Guid.NewGuid(), behandlerId,
                eksisterendeForPatient: Array.Empty<Konsultation>(),
                eksisterendeForBehandler: new[] { eksisterende }));
    }

    [Fact]
    public void Opret_UdenOverlap_Lykkes()
    {
        var patientId = Guid.NewGuid();
        var behandlerId = Guid.NewGuid();
        var eksisterende = Konsultation.Opret(
            Tid(9, 10), TypeId, patientId, behandlerId,
            Array.Empty<Konsultation>(), Array.Empty<Konsultation>());

        var ny = Konsultation.Opret(
            Tid(10, 11), TypeId, patientId, behandlerId,
            eksisterendeForPatient: new[] { eksisterende },
            eksisterendeForBehandler: new[] { eksisterende });
        Assert.NotNull(ny);
    }

    [Fact]
    public void Opret_AflystBookingBlokererIkke()
    {
        var patientId = Guid.NewGuid();
        var behandlerId = Guid.NewGuid();
        var aflyst = Konsultation.Opret(
            Tid(9, 10), TypeId, patientId, behandlerId,
            Array.Empty<Konsultation>(), Array.Empty<Konsultation>());
        aflyst.Aflys();

        var ny = Konsultation.Opret(
            Tid(9, 10), TypeId, patientId, behandlerId,
            eksisterendeForPatient: new[] { aflyst },
            eksisterendeForBehandler: new[] { aflyst });
        Assert.NotNull(ny);
    }

    [Fact]
    public void OpdaterBehandlingstype_PaaAfsluttet_KasterDomainException()
    {
        var k = OpretUdenOverlap();
        k.Afslut("Test-notat");
        Assert.Throws<DomainException>(() => k.OpdaterBehandlingstype(Guid.NewGuid()));
    }

    [Fact]
    public void Afslut_MedTomNotat_KasterDomainException()
    {
        var k = OpretUdenOverlap();
        Assert.Throws<DomainException>(() => k.Afslut(""));
    }

    [Fact]
    public void Afslut_SætterStatusTilAfsluttet()
    {
        var k = OpretUdenOverlap();
        k.Afslut("Alt gik godt");
        Assert.Equal(KonsultationStatus.Afsluttet, k.Status);
        Assert.Equal("Alt gik godt", k.Notat);
    }

    [Fact]
    public void Aflys_SætterStatusTilAflyst()
    {
        var k = OpretUdenOverlap();
        k.Aflys();
        Assert.Equal(KonsultationStatus.Aflyst, k.Status);
    }

    [Fact]
    public void ErAktiv_AflystKonsultation_ReturnererFalse()
    {
        var k = OpretUdenOverlap();
        k.Aflys();
        Assert.False(k.ErAktiv);
    }
}
