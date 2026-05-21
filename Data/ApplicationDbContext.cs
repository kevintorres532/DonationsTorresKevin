using DonationsTorresKevin.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace DonationsTorresKevin.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Donation> Donations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configuración de la entidad Donation usando Fluent API
        builder.Entity<Donation>(entity =>
        {
            // Configuración de la clave primaria
            entity.HasKey(d => d.Id);

            // Relación con el usuario (1 a muchos)
            entity.HasOne(d => d.User)
                .WithMany() // ApplicationUser no tiene una colección explícita de Donations
                .HasForeignKey(d => d.UserId)
                .IsRequired();

            // Configuración de la columna Amount
            entity.Property(d => d.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // Configuración de la columna Currency
            entity.Property(d => d.Currency)
                .HasMaxLength(3)
                .IsRequired();

            // Configuración de Status
            entity.Property(d => d.Status)
                .IsRequired();

            // Configuración de CreatedAt
            entity.Property(d => d.CreatedAt)
                .IsRequired();

            // StripeSessionId es opcional
            entity.Property(d => d.StripeSessionId)
                .HasMaxLength(255);
        });
    }
}