namespace BusinessLogic;

public class BeregnPrisService
{
    private readonly IBeregnRabatService _beregnRabatService;

    public BeregnPrisService(IBeregnRabatService beregnRabatService)
    {
        // Afhængigheden injiceres, så servicen kan testes med en mock i stedet for den rigtige rabatservice.
        _beregnRabatService = beregnRabatService;
    }
    public double BeregnPrisMedRabat(double pris, double beregnRabatProcent)
    {
        // Rabatten hentes fra afhængigheden. I testen bestemmer mocken denne værdi.
        var rabatProcent = _beregnRabatService.BeregnRabatProcent(pris);
        var prisMedRabat = pris * (1 - rabatProcent / 100);
        return prisMedRabat;
    }
}