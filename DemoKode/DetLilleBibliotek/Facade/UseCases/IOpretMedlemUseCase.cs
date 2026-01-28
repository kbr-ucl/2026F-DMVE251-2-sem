namespace Facade.UseCases;

public interface IOpretMedlemUseCase
{
    void OpretMedlem(OpretMedlemCommandDto commandDto);
}

public record OpretMedlemCommandDto(int Medlemsnummer, string Navn);