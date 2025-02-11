using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sanki.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateOneToManyUserFlashcardRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "flashcards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_flashcards_user_id",
                table: "flashcards",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_flashcards_users_user_id",
                table: "flashcards",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_flashcards_users_user_id",
                table: "flashcards");

            migrationBuilder.DropIndex(
                name: "IX_flashcards_user_id",
                table: "flashcards");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "flashcards");
        }
    }
}
