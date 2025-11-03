using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoDBP.Migrations
{
    /// <inheritdoc />
    public partial class AddComentariosCitas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comentarios",
                table: "Citas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comentarios",
                table: "Citas");
        }
    }
}
