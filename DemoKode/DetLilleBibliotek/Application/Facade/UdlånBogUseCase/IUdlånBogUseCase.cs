namespace Application.Facade.UdlånBogUseCase;

interface IUdlånBogUseCase
{
    void LånAfBogTilMedlem(UdlånBogCommmandDto commmandDto);
}

public record UdlånBogCommmandDto(Guid MedlemsId, Guid BogId);
