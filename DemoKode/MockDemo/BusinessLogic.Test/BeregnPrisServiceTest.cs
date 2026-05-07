using Moq;

namespace BusinessLogic.Test;

public class BeregnPrisServiceTest
{
    [Fact]
    public void TestMockedRabatService_ReturnsPrisMedRabat()
    {
        // Arrange
        var pris = 200;
        var rabatProcent = 25;
        var expected = 150;

        // Mocken erstatter den rigtige BeregnRabatService, så testen ikke afhænger af tilfældige rabatter.
        var beregnRabatServiceMock = new Mock<IBeregnRabatService>();
        beregnRabatServiceMock
            .Setup(service => service.BeregnRabatProcent(pris))
            .Returns(rabatProcent);

        // System under test får mockens objekt gennem constructoren.
        var service = new BeregnPrisService(beregnRabatServiceMock.Object);

        // Act
        var actual = service.BeregnPrisMedRabat(pris, 0);

        // Assert
        Assert.Equal(expected, actual);

        // Verify kontrollerer, at BeregnPrisService faktisk bruger rabatservicen.
        beregnRabatServiceMock.Verify(
            service => service.BeregnRabatProcent(pris),
            Times.Once);
    }
}