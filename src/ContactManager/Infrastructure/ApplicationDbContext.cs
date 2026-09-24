using ContactManager.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ContactManager.Infrastructure;

public class ApplicationDbContext : DbContext
{
    private readonly string connectionString;

    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Trainee> Trainees => Set<Trainee>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerContactNote> CustomerContactNotes => Set<CustomerContactNote>();

    public ApplicationDbContext()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        var configuredConnection = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=Data\\ContactManager.db";

        var builder = new SqliteConnectionStringBuilder(configuredConnection);
        if (!Path.IsPathRooted(builder.DataSource))
        {
            builder.DataSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, builder.DataSource);
        }

        var directory = Path.GetDirectoryName(builder.DataSource);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        connectionString = builder.ToString();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>()
            .HasDiscriminator<string>("PersonType")
            .HasValue<Customer>("Customer")
            .HasValue<Employee>("Employee")
            .HasValue<Trainee>("Trainee");

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.EmployeeNumber)
            .IsUnique();

        modelBuilder.Entity<CustomerContactNote>()
            .HasOne(n => n.Customer)
            .WithMany(c => c.ContactNotes)
            .HasForeignKey(n => n.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
