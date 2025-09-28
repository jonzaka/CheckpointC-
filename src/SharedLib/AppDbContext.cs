using Microsoft.EntityFrameworkCore;
using SharedLib.Models;

namespace SharedLib.Data;

public class AppDbContext : DbContext
{
    public DbSet<Assessor> Assessores => Set<Assessor>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Ativo> Ativos => Set<Ativo>();
    public DbSet<Carteira> Carteiras => Set<Carteira>();
    public DbSet<Posicao> Posicoes => Set<Posicao>();
    public DbSet<Ordem> Ordens => Set<Ordem>();

    public AppDbContext() { }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var oracleConn = Environment.GetEnvironmentVariable("ORACLE_CONN");
            if (!string.IsNullOrWhiteSpace(oracleConn))
            {
                // Oracle (FIAP) se a variável estiver definida
                optionsBuilder.UseOracle(oracleConn);
            }
            else
            {
                // Fallback local: SQLite
                var dbPath = Path.Combine(AppContext.BaseDirectory, "db.sqlite3");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Assessor>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).IsRequired().HasMaxLength(120);
            e.Property(x => x.CPF).IsRequired().HasMaxLength(14);
        });

        modelBuilder.Entity<Cliente>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).IsRequired().HasMaxLength(120);
            e.Property(x => x.CPF).IsRequired().HasMaxLength(14);
            e.HasOne(x => x.Assessor)
             .WithMany(a => a.Clientes)
             .HasForeignKey(x => x.AssessorId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Ativo>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Ticker).IsRequired().HasMaxLength(10);
            e.Property(x => x.Nome).IsRequired().HasMaxLength(120);
        });

        modelBuilder.Entity<Carteira>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).IsRequired().HasMaxLength(60);
            e.HasOne(x => x.Cliente)
             .WithMany(c => c.Carteiras)
             .HasForeignKey(x => x.ClienteId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Posicao>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Carteira).WithMany(c => c.Posicoes).HasForeignKey(x => x.CarteiraId);
            e.HasOne(x => x.Ativo).WithMany().HasForeignKey(x => x.AtivoId);
        });

        modelBuilder.Entity<Ordem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Tipo).IsRequired().HasMaxLength(10);
            e.HasOne(x => x.Cliente).WithMany(c => c.Ordens).HasForeignKey(x => x.ClienteId);
            e.HasOne(x => x.Ativo).WithMany(a => a.Ordens).HasForeignKey(x => x.AtivoId);
        });
    }
}
