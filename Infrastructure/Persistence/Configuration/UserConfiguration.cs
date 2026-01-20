using Core.Entities;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration;
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasColumnType("INT")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.Email)
            .HasConversion(email => email.Address, value => new Email(value))
            .HasColumnType("VARCHAR(255)")
            .IsRequired();

        builder.Property(u => u.Password)
            .HasConversion(password => password.Hashed, value => Password.FromHashed(value))
            .HasColumnType("VARCHAR(255)")
            .IsRequired();

        builder.Property(g => g.CreatedAt)
            .IsRequired()
            .HasColumnType("TIMESTAMPTZ");

        builder.HasIndex(u => u.Email).IsUnique();
    }
}
