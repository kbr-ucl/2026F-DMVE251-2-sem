namespace CodeToTest.Entity;

public class Author
{
    public Author(string name, DateTime birthDate)
    {
        Name = name;
        BirthDate = birthDate;
    }

    public string Name { get; private set; }
    public DateTime BirthDate { get; private set; }
}