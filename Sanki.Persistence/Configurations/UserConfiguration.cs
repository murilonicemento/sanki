using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanki.Entities;

namespace Sanki.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id).HasName("users_pkey");

        builder.ToTable("users");

        builder.HasIndex(e => e.Email, "users_email_key").IsUnique();

        builder.Property(e => e.Id)
            .HasDefaultValueSql("uuid_generate_v4()")
            .HasColumnName("id");
        builder.Property(e => e.Email)
            .HasMaxLength(255)
            .HasColumnName("email");
        builder.Property(e => e.FirstName)
            .HasMaxLength(50)
            .HasColumnName("first_name");
        builder.Property(e => e.LastName)
            .HasMaxLength(50)
            .HasColumnName("last_name");
        builder.Property(e => e.Password)
            .HasMaxLength(255)
            .HasColumnName("password");
        builder.Property(e => e.RefreshToken)
            .HasMaxLength(255)
            .HasColumnName("refresh_token");
        builder.Property(e => e.RefreshTokenExpiration)
            .HasMaxLength(255)
            .HasColumnName("refresh_token_expiration");
        builder.Property(e => e.Salt)
            .HasColumnName("salt");
    }
}