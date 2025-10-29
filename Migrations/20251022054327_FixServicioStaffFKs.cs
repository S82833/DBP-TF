using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoDBP.Migrations
{
    /// <inheritdoc />
    public partial class FixServicioStaffFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiciosStaff_Servicios_ServicioIdServicio",
                table: "ServiciosStaff");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiciosStaff_StaffMedico_StaffMedicoIdStaffMedico",
                table: "ServiciosStaff");

            migrationBuilder.DropIndex(
                name: "IX_ServiciosStaff_ServicioIdServicio",
                table: "ServiciosStaff");

            migrationBuilder.DropIndex(
                name: "IX_ServiciosStaff_StaffMedicoIdStaffMedico",
                table: "ServiciosStaff");

            migrationBuilder.DropColumn(
                name: "ServicioIdServicio",
                table: "ServiciosStaff");

            migrationBuilder.DropColumn(
                name: "StaffMedicoIdStaffMedico",
                table: "ServiciosStaff");

            migrationBuilder.CreateIndex(
                name: "IX_ServiciosStaff_IdServicio_IdStaffMedico",
                table: "ServiciosStaff",
                columns: new[] { "IdServicio", "IdStaffMedico" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiciosStaff_IdStaffMedico",
                table: "ServiciosStaff",
                column: "IdStaffMedico");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiciosStaff_Servicios_IdServicio",
                table: "ServiciosStaff",
                column: "IdServicio",
                principalTable: "Servicios",
                principalColumn: "IdServicio",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiciosStaff_StaffMedico_IdStaffMedico",
                table: "ServiciosStaff",
                column: "IdStaffMedico",
                principalTable: "StaffMedico",
                principalColumn: "IdStaffMedico",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiciosStaff_Servicios_IdServicio",
                table: "ServiciosStaff");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiciosStaff_StaffMedico_IdStaffMedico",
                table: "ServiciosStaff");

            migrationBuilder.DropIndex(
                name: "IX_ServiciosStaff_IdServicio_IdStaffMedico",
                table: "ServiciosStaff");

            migrationBuilder.DropIndex(
                name: "IX_ServiciosStaff_IdStaffMedico",
                table: "ServiciosStaff");

            migrationBuilder.AddColumn<int>(
                name: "ServicioIdServicio",
                table: "ServiciosStaff",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StaffMedicoIdStaffMedico",
                table: "ServiciosStaff",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ServiciosStaff_ServicioIdServicio",
                table: "ServiciosStaff",
                column: "ServicioIdServicio");

            migrationBuilder.CreateIndex(
                name: "IX_ServiciosStaff_StaffMedicoIdStaffMedico",
                table: "ServiciosStaff",
                column: "StaffMedicoIdStaffMedico");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiciosStaff_Servicios_ServicioIdServicio",
                table: "ServiciosStaff",
                column: "ServicioIdServicio",
                principalTable: "Servicios",
                principalColumn: "IdServicio",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiciosStaff_StaffMedico_StaffMedicoIdStaffMedico",
                table: "ServiciosStaff",
                column: "StaffMedicoIdStaffMedico",
                principalTable: "StaffMedico",
                principalColumn: "IdStaffMedico",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
