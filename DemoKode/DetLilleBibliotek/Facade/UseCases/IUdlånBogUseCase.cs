namespace Facade.UseCases;

public interface IUdlånBogUseCase
{
    void LånAfBogTilMedlem(UdlånBogCommmandDto commmandDto);
}

public record UdlånBogCommmandDto(int Medlemsnummer, string Isbn);
