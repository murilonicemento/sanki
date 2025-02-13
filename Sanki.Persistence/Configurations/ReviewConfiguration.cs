using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanki.Entities;

namespace Sanki.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(e => e.Id).HasName("reviews_pkey");

        builder.ToTable("reviews");

        builder.HasIndex(e => e.FlashcardId, "idx_reviews_flashcard_id");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("uuid_generate_v4()")
            .HasColumnName("id");
        builder.Property(e => e.FlashcardId).HasColumnName("flashcard_id");
        builder.Property(e => e.ReviewDate)
            .HasColumnType("timestamp without time zone")
            .HasColumnName("review_date");

        builder.HasOne(d => d.Flashcard).WithOne(d => d.Review)
            .HasForeignKey<Review>(d => d.FlashcardId)
            .HasConstraintName("reviews_fk2");
    }
}