using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanki.Entities;

namespace Sanki.Persistence.Configurations;

public class FlashcardConfiguration : IEntityTypeConfiguration<Flashcard>
{
    public void Configure(EntityTypeBuilder<Flashcard> builder)
    {
        builder.HasKey(e => e.Id).HasName("flashcards_pkey");

        builder.ToTable("flashcards");

        builder.HasIndex(e => e.ResumeId, "idx_flashcards_resume_id");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("uuid_generate_v4()")
            .HasColumnName("id");
        builder.Property(e => e.Question)
            .HasMaxLength(255)
            .HasColumnName("question");
        builder.Property(e => e.Response)
            .HasMaxLength(255)
            .HasColumnName("response");
        builder.Property(e => e.ResumeId).HasColumnName("resume_id");
        builder.Property(e => e.UserId).HasColumnName("user_id");
        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .HasColumnName("status");

        builder.HasOne(d => d.User).WithMany(p => p.Flashcards);
        builder.HasOne(d => d.Resume).WithMany(p => p.Flashcards)
            .HasForeignKey(d => d.ResumeId)
            .HasConstraintName("flashcards_fk4");
    }
}