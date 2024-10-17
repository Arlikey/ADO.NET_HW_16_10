using Microsoft.EntityFrameworkCore;

class Program
{
    static void Main(string[] args)
    {
        using(ApplicationContext db =  new ApplicationContext())
        {

            if (!db.Genres.Any())
            {
                db.Genres.AddRange(
                [
                    new Genre { Name = "Science Fiction", Description = "Fiction dealing with futuristic concepts and technologies." },
                    new Genre { Name = "Fantasy", Description = "Fiction set in magical or imaginary worlds." },
                    new Genre { Name = "Historical", Description = "Fiction set in a historical period, featuring real or imaginary events." },
                    new Genre { Name = "Mystery", Description = "Fiction dealing with solving a crime or uncovering secrets." },
                    new Genre { Name = "Horror", Description = "Fiction intended to scare, unsettle, or horrify readers." }
                ]);

                db.SaveChanges();
            }

            if (!db.Authors.Any())
            {
                db.Authors.AddRange(
                [
                    new Author { FirstName = "Isaac", LastName = "Asimov", BirthDate = new DateTime(1920, 1, 2), DeathDate = new DateTime(1992, 4, 6) },
                    new Author { FirstName = "J.R.R.", LastName = "Tolkien", BirthDate = new DateTime(1892, 1, 3), DeathDate = new DateTime(1973, 9, 2) },
                    new Author { FirstName = "Ken", LastName = "Follett", BirthDate = new DateTime(1949, 6, 5), DeathDate = null },
                    new Author { FirstName = "Agatha", LastName = "Christie", BirthDate = new DateTime(1890, 9, 15), DeathDate = new DateTime(1976, 1, 12) },
                    new Author { FirstName = "Stephen", LastName = "King", BirthDate = new DateTime(1947, 9, 21), DeathDate = null },
                    new Author { FirstName = "George", LastName = "Orwell", BirthDate = new DateTime(1903, 6, 25), DeathDate = new DateTime(1950, 1, 21) },
                    new Author { FirstName = "Frank", LastName = "Herbert", BirthDate = new DateTime(1920, 10, 8), DeathDate = new DateTime(1986, 2, 11) }
                ]);

                db.SaveChanges();
            }

            if (!db.Books.Any())
            {
                db.Books.AddRange(
                [
                    new Book { Title = "Foundation", Price = 9.99m, AuthorId = 1, GenreId = 1 },
                    new Book { Title = "The Hobbit", Price = 12.99m, AuthorId = 2, GenreId = 2 },
                    new Book { Title = "The Pillars of the Earth", Price = 15.99m, AuthorId = 3, GenreId = 3 },
                    new Book { Title = "Murder on the Orient Express", Price = 8.99m, AuthorId = 4, GenreId = 4 },
                    new Book { Title = "The Shining", Price = 10.99m, AuthorId = 5, GenreId = 5 },
                    new Book { Title = "1984", Price = 7.99m, AuthorId = 6, GenreId = 1 },
                    new Book { Title = "Dune", Price = 14.99m, AuthorId = 7, GenreId = 1 },
                    new Book { Title = "The Lord of the Rings", Price = 19.99m, AuthorId = 2, GenreId = 2 },
                    new Book { Title = "And Then There Were None", Price = 9.49m, AuthorId = 4, GenreId = 4 },
                    new Book { Title = "Carrie", Price = 7.99m, AuthorId = 5, GenreId = 5 },
                    new Book { Title = "The Children of Dune", Price = 11.99m, AuthorId = 7, GenreId = 1 },
                    new Book { Title = "The Silmarillion", Price = 13.99m, AuthorId = 2, GenreId = 2 },
                    new Book { Title = "The Winds of Winter", Price = 16.99m, AuthorId = 2, GenreId = 2 },
                    new Book { Title = "Pillars of Creation", Price = 14.49m, AuthorId = 3, GenreId = 3 },
                    new Book { Title = "Salem's Lot", Price = 9.89m, AuthorId = 5, GenreId = 5 },
                    new Book { Title = "The Murder of Roger Ackroyd", Price = 7.59m, AuthorId = 4, GenreId = 4 },
                    new Book { Title = "Brave New World", Price = 11.49m, AuthorId = 6, GenreId = 1 },
                    new Book { Title = "Cujo", Price = 10.79m, AuthorId = 5, GenreId = 5 },
                    new Book { Title = "The Gunslinger", Price = 12.99m, AuthorId = 5, GenreId = 5 }
                ]);

                db.SaveChanges();
            }
        }

        using (ApplicationContext db = new ApplicationContext())
        {
            int countBooksOfGenre = db.Books.Where(b => b.Genre.Name.Equals("Fantasy")).Count();
            decimal minPriceForAuthorBook = db.Books.Where(b => b.AuthorId == 3).Min(b => b.Price);
            decimal avgPriceBooksOfGenre = db.Books.Where(b => b.Genre.Name.Equals("Horror")).Average(b => b.Price);
            decimal sumPriceBooksOfAuthor = db.Books.Where(b => b.AuthorId == 4).Sum(b => b.Price);
            var orderBooksByGenre = db.Books.OrderBy(b => b.Genre);
            var bookTitlesOfGenre = db.Books.Where(b => b.GenreId == 1).Select(b => b.Title);
            var booksExceptGenre = db.Books.Except(db.Books.Where(b => b.GenreId == 2)).ToList();
            var combinedBooks = db.Books.Where(b => b.AuthorId == 1).Union(db.Books.Where(b => b.AuthorId == 2)).ToList();
            var take5MostExpensiveBooks = db.Books.OrderByDescending(b => b.Price).Take(5).ToList();
            var skip10take5Books = db.Books.Skip(10).Take(5).ToList();
        }
    }
}
public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public decimal Price { get; set; }
    public int AuthorId { get; set; }
    public Author Author { get; set; }
    public int GenreId { get; set; }
    public Genre Genre { get; set; }
}

public class Author
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public DateTime? DeathDate { get; set; }
    public List<Book> Books { get; set; }
}
public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<Book> Books { get; set; }
}
public class ApplicationContext : DbContext
{
    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<Author> Authors { get; set; } = null!;
    public DbSet<Genre> Genres { get; set; } = null!;
    public ApplicationContext() => Database.EnsureCreated();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Author)
            .WithMany(b => b.Books)
            .HasForeignKey(b => b.AuthorId);

        modelBuilder.Entity<Book>()
            .HasOne(b => b.Genre)
            .WithMany(g => g.Books)
            .HasForeignKey(b => b.GenreId);

        modelBuilder.Entity<Author>().Property(a => a.DeathDate).IsRequired(false);

        base.OnModelCreating(modelBuilder);
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=EFCoreDB;Trusted_Connection=True;");
    }
}