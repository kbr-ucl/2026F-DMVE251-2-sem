using Moq;
using Xunit;
using MinKlinik.Domain.Entities;
using MinKlinik.Domain.Exceptions;
using MinKlinik.Facade.DTOs;
using MinKlinik.UseCases;
using MinKlinik.UseCases.Konsultationer;

namespace MinKlinik.UseCases.Tests;

public class OpretKonsultationUseCaseTests
{
    private readonly Mock<IKonsultationRepository> _mockKonsRepo = new();
    private readonly Mock<IBehandlingstypeRepository> _mockBehandTypeRepo = new();
    private readonly Mock<IPatientRepository> _mockPatientRepo = new();
    private readonly Mock<IBehandlerRepository> _mockBehandlerRepo = new();

    private OpretKonsultationUseCase CreateSut() => new(
        _mockKonsRepo.Object,
        _mockBehandTypeRepo.Object,
        _mockPatientRepo.Object,
        _mockBehandlerRepo.Object);

    [Fact]
    public async Task Udfør_MedGyldigRequest_KalderTilføjOgGem()
    {
        var typeId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var behandlerId = Guid.NewGuid();

        _mockBehandTypeRepo.Setup(r => r.HentAsync(typeId))
            .ReturnsAsync(new Behandlingstype("Undersøgelse"));
        _mockPatientRepo.Setup(r => r.HentAsync(patientId))
            .ReturnsAsync(new Patient("Jens", "010190-1234"));
        _mockBehandlerRepo.Setup(r => r.HentAsync(behandlerId))
            .ReturnsAsync(new Behandler("Dr. Pia", "Almen medicin"));
        _mockKonsRepo.Setup(r => r.HentForPatientAsync(patientId))
            .ReturnsAsync(new List<Konsultation>());
        _mockKonsRepo.Setup(r => r.HentForBehandlerAsync(behandlerId))
            .ReturnsAsync(new List<Konsultation>());

        var request = new OpretKonsultationRequest(
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            typeId, patientId, behandlerId);

        await CreateSut().Udfør(request);

        _mockKonsRepo.Verify(r => r.TilføjAsync(It.IsAny<Konsultation>()), Times.Once);
        _mockKonsRepo.Verify(r => r.GemAsync(), Times.Once);
    }

    [Fact]
    public async Task Udfør_MedUkendtBehandlingstype_KasterNotFoundException()
    {
        _mockBehandTypeRepo.Setup(r => r.HentAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Behandlingstype?)null);

        var request = new OpretKonsultationRequest(
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => CreateSut().Udfør(request));
    }

    [Fact]
    public async Task Udfør_MedOverlapForPatient_KasterDomainException()
    {
        var typeId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var behandlerId1 = Guid.NewGuid();
        var behandlerId2 = Guid.NewGuid();

        var fra = DateTime.UtcNow.AddDays(1).Date.AddHours(9);
        var til = DateTime.UtcNow.AddDays(1).Date.AddHours(10);

        var eksisterende = Konsultation.Opret(
            new Domain.ValueObjects.Tidsinterval(fra, til),
            typeId, patientId, behandlerId1,
            Array.Empty<Konsultation>(), Array.Empty<Konsultation>());

        _mockBehandTypeRepo.Setup(r => r.HentAsync(typeId))
            .ReturnsAsync(new Behandlingstype("Undersøgelse"));
        _mockPatientRepo.Setup(r => r.HentAsync(patientId))
            .ReturnsAsync(new Patient("Jens", "010190-1234"));
        _mockBehandlerRepo.Setup(r => r.HentAsync(behandlerId2))
            .ReturnsAsync(new Behandler("Dr. Lars", "Ortopædi"));
        _mockKonsRepo.Setup(r => r.HentForPatientAsync(patientId))
            .ReturnsAsync(new List<Konsultation> { eksisterende });
        _mockKonsRepo.Setup(r => r.HentForBehandlerAsync(behandlerId2))
            .ReturnsAsync(new List<Konsultation>());

        var request = new OpretKonsultationRequest(
            fra.AddMinutes(30), til.AddMinutes(30),
            typeId, patientId, behandlerId2);

        await Assert.ThrowsAsync<DomainException>(() => CreateSut().Udfør(request));
        _mockKonsRepo.Verify(r => r.TilføjAsync(It.IsAny<Konsultation>()), Times.Never);
    }
}

public class AfslutKonsultationUseCaseTests
{
    [Fact]
    public async Task Udfør_MedGyldigRequest_AfslutterKonsultation()
    {
        var konsultation = Konsultation.Opret(
            new Domain.ValueObjects.Tidsinterval(
                DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1)),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Array.Empty<Konsultation>(), Array.Empty<Konsultation>());

        var mockRepo = new Mock<IKonsultationRepository>();
        mockRepo.Setup(r => r.HentAsync(konsultation.Id)).ReturnsAsync(konsultation);

        var useCase = new AfslutKonsultationUseCase(mockRepo.Object);
        await useCase.Udfør(new AfslutKonsultationRequest(konsultation.Id, "Alt OK"));

        Assert.Equal(Domain.Enums.KonsultationStatus.Afsluttet, konsultation.Status);
        mockRepo.Verify(r => r.GemAsync(), Times.Once);
    }
}
