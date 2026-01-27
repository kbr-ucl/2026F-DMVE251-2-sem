namespace Facade.UseCases;

public interface IUdlånBogUseCase
{
    void LånAfBogTilMedlem(UdlånBogCommmandDto commmandDto);
}

public record UdlånBogCommmandDto(Guid MedlemsId, Guid BogId);
