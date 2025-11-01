using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoDBP.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexCitas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Citas_IdStaffMedico",
                table: "Citas");

            migrationBuilder.CreateIndex(
                name: "UX_Citas_Medico_Fecha",
                table: "Citas",
                columns: new[] { "IdStaffMedico", "Fecha" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Citas_Medico_Fecha",
                table: "Citas");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_IdStaffMedico",
                table: "Citas",
                column: "IdStaffMedico");
        }
    }
}
