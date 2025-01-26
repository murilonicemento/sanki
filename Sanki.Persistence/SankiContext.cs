using Microsoft.EntityFrameworkCore;
using Sanki.Entities;

namespace Sanki.Persistence;

public class SankiContext : DbContext
{
    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Resume> Resumes { get; set; }

    public virtual DbSet<Flashcard> Flashcards { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public SankiContext()
    {
    }

    public SankiContext(DbContextOptions<SankiContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SankiContext).Assembly);
    }
}