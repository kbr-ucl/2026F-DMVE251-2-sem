namespace Domain.Entity;

public class Konsultationstype
{
    public TimeSpan Varighed { get; protected set; }
}

public class AlmindeligKonsultation : Konsultationstype
{
    public AlmindeligKonsultation()
    {
        Varighed = TimeSpan.FromMinutes(20);
    }
}

public class Vaccination : Konsultationstype
{
    public Vaccination()
    {
        Varighed = TimeSpan.FromMinutes(10);
    }
}

public class Receptfornyelse : Konsultationstype
{
    public Receptfornyelse()
    {
        Varighed = TimeSpan.FromMinutes(10);
    }
}

public class Rådgivning : Konsultationstype
{
    public Rådgivning()
    {
        Varighed = TimeSpan.FromMinutes(15);
        }
}