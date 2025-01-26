using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanki.Entities;

namespace Sanki.Persistence.Configurations;

public class ResumeConfiguration : IEntityTypeConfiguration<Resume>
{
    public void Configure(EntityTypeBuilder<Resume> builder)
    {
        builder.HasKey(e => e.Id).HasName("resumes_pkey");

        builder.ToTable("resumes");

        builder.HasIndex(e => e.UserId, "idx_resumes_user_id");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("uuid_generate_v4()")
            .HasColumnName("id");
        builder.Property(e => e.Content).HasColumnName("content");
        builder.Property(e => e.Title)
            .HasMaxLength(100)
            .HasColumnName("title");
        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.HasOne(d => d.User).WithMany(p => p.Resumes)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("resumes_fk3");
    }
}