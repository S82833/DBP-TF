using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoDBP.Migrations
{
    /// <inheritdoc />
    public partial class RemoveHistorialClinico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistorialesClinicos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistorialesClinicos",
                columns: table => new
                {
                    IdHistorialClinico = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCita = table.Column<int>(type: "int", nullable: false),
                    Control = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiagnosticoDefinitivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiagnosticoPresuntivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnfermedadActual = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FuncionesBio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MotivoConsulta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pronostico = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Recomendaciones = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RelatoCron = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SignosSintomas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TiempoEnfermedad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tratamiento = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialesClinicos", x => x.IdHistorialClinico);
                    table.ForeignKey(
                        name: "FK_HistorialesClinicos_Citas_IdCita",
                        column: x => x.IdCita,
                        principalTable: "Citas",
                        principalColumn: "IdCita",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialesClinicos_IdCita",
                table: "HistorialesClinicos",
                column: "IdCita",
                unique: true);
        }
    }
}
