using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sanki.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeFlashcardReviewRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_reviews_flashcard_id",
                table: "reviews",
                column: "flashcard_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_reviews_flashcard_id",
                table: "reviews");
        }
    }
}
