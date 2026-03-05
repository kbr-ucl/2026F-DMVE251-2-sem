namespace CodeToTest.Entity;

public class Book
{
    public Book(string isbn, string title)
    {
        Isbn = isbn;
        Title = title;
    }

    public string Isbn { get; private set; }
    public string Title { get; private set; }
}