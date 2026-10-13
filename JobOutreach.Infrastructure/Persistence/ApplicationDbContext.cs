using JobOutreach.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobOutreach.Infrastructure.Persistence;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options
) : DbContext(options)
{

    public DbSet<Company> Company { get; set; } = null!;
    public DbSet<Email> Email { get; set; } = null!;
    public DbSet<Country> Country { get; set; } = null!;
    public DbSet<Position> Position { get; set; } = null!;
    public DbSet<CompanyPosition> CompanyPosition { get; set; } = null!;
    public DbSet<EmailSent> EmailSent { get; set; } = null!;
    public DbSet<CompanyPageSnapshot> CompanyPageSnapshot { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CompanyPhone).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CompanyEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CompanyUrl).IsRequired().HasMaxLength(255);
            entity.Property(e => e.IsContacted).IsRequired().HasDefaultValue(false);
            entity.HasOne(e => e.Country)
                  .WithMany()
                  .HasForeignKey(e => e.CountryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Company>()
            .HasMany(c => c.Positions)
            .WithMany()
            .UsingEntity<CompanyPosition>(
                j => j
                    .HasOne(cp => cp.Position)
                    .WithMany()
                    .HasForeignKey(cp => cp.PositionId)
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne(cp => cp.Company)
                    .WithMany()
                    .HasForeignKey(cp => cp.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey(cp => new { cp.CompanyId, cp.PositionId });
                    j.ToTable("CompanyPosition");
                });

        modelBuilder.Entity<Email>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Content).IsRequired(); // MAX by default in EF Core for string without length
            entity.Property(e => e.Attachment).IsRequired().HasMaxLength(255);
            entity.Property(e => e.SentBy).IsRequired().HasMaxLength(255);
            entity.HasOne(e => e.Country)
                  .WithMany()
                  .HasForeignKey(e => e.CountryId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Position)
                  .WithMany()
                  .HasForeignKey(e => e.PositionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CountryName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CountryCode).IsRequired().HasMaxLength(10);
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PositionName).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<EmailSent>(entity =>
        {
            entity.ToTable("EmailSent");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SentToEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.SentAt).HasDefaultValueSql("SYSDATETIME()");

            entity.HasOne(e => e.Email)
                .WithMany()
                .HasForeignKey(e => e.EmailId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Position)
                .WithMany()
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CompanyPageSnapshot>(entity =>
        {
            entity.ToTable("CompanyPageSnapshot");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Url).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ContentHash).IsRequired().HasMaxLength(64).IsFixedLength().IsUnicode(false); // SHA-256 hex
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.LastCheckedAt).HasDefaultValueSql("SYSDATETIME()");
            entity.Property(e => e.LastError).HasMaxLength(1000);
            entity.HasIndex(e => e.CompanyId).IsUnique();

            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}
