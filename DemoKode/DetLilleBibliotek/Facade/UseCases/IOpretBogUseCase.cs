namespace Facade.UseCases;

public interface IOpretBogUseCase
{
    void OpretBog(OpretBogCommandDto commandDto);
}

public record OpretBogCommandDto(string Isbn, string Titel, string Forfatter);