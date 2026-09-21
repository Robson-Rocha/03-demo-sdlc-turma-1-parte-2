using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainingCatalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingLessons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LessonCount",
                table: "Trainings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "LessonDurationHours",
                table: "Trainings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql("""
                UPDATE "Trainings"
                SET "LessonCount" = "DurationHours",
                    "LessonDurationHours" = 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LessonCount",
                table: "Trainings");

            migrationBuilder.DropColumn(
                name: "LessonDurationHours",
                table: "Trainings");
        }
    }
}
