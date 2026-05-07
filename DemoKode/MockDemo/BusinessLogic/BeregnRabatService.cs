namespace BusinessLogic;

public class BeregnRabatService : IBeregnRabatService
{
    public double BeregnRabatProcent(double pris)
    {
        // Next bruger en eksklusiv øvre grænse, så 81 giver værdier fra 0 til og med 80.
        return Random.Shared.Next(0, 81);
    }
}