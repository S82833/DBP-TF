using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoDBP.Migrations
{
    /// <inheritdoc />
    public partial class Migration01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Servicios",
                columns: table => new
                {
                    IdServicio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicios", x => x.IdServicio);
                });

            migrationBuilder.CreateTable(
                name: "StaffMedico",
                columns: table => new
                {
                    IdStaffMedico = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Especialidad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Biografia = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffMedico", x => x.IdStaffMedico);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contraseña = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dni = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Edad = table.Column<int>(type: "int", nullable: false),
                    Sexo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ocupacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TelefonoEmergencia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Alergias = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Antecedentes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AntecedentesFam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rol = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.IdUsuario);
                });

            migrationBuilder.CreateTable(
                name: "DoctorDisponibilidades",
                columns: table => new
                {
                    IdDoctorDisponibilidad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdStaffMedico = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HoraInicio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HoraFin = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorDisponibilidades", x => x.IdDoctorDisponibilidad);
                    table.ForeignKey(
                        name: "FK_DoctorDisponibilidades_StaffMedico_IdStaffMedico",
                        column: x => x.IdStaffMedico,
                        principalTable: "StaffMedico",
                        principalColumn: "IdStaffMedico",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiciosStaff",
                columns: table => new
                {
                    IdServiciosStaff = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdServicio = table.Column<int>(type: "int", nullable: false),
                    IdStaffMedico = table.Column<int>(type: "int", nullable: false),
                    ServicioIdServicio = table.Column<int>(type: "int", nullable: false),
                    StaffMedicoIdStaffMedico = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiciosStaff", x => x.IdServiciosStaff);
                    table.ForeignKey(
                        name: "FK_ServiciosStaff_Servicios_ServicioIdServicio",
                        column: x => x.ServicioIdServicio,
                        principalTable: "Servicios",
                        principalColumn: "IdServicio",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiciosStaff_StaffMedico_StaffMedicoIdStaffMedico",
                        column: x => x.StaffMedicoIdStaffMedico,
                        principalTable: "StaffMedico",
                        principalColumn: "IdStaffMedico",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Citas",
                columns: table => new
                {
                    IdCita = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    IdServicio = table.Column<int>(type: "int", nullable: false),
                    IdStaffMedico = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citas", x => x.IdCita);
                    table.ForeignKey(
                        name: "FK_Citas_Servicios_IdServicio",
                        column: x => x.IdServicio,
                        principalTable: "Servicios",
                        principalColumn: "IdServicio",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Citas_StaffMedico_IdStaffMedico",
                        column: x => x.IdStaffMedico,
                        principalTable: "StaffMedico",
                        principalColumn: "IdStaffMedico",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Citas_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistorialesClinicos",
                columns: table => new
                {
                    IdHistorialClinico = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCita = table.Column<int>(type: "int", nullable: false),
                    MotivoConsulta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnfermedadActual = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TiempoEnfermedad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SignosSintomas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RelatoCron = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FuncionesBio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiagnosticoPresuntivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiagnosticoDefinitivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tratamiento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pronostico = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Recomendaciones = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Control = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "IX_Citas_IdServicio",
                table: "Citas",
                column: "IdServicio");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_IdStaffMedico",
                table: "Citas",
                column: "IdStaffMedico");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_IdUsuario",
                table: "Citas",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorDisponibilidades_IdStaffMedico",
                table: "DoctorDisponibilidades",
                column: "IdStaffMedico");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialesClinicos_IdCita",
                table: "HistorialesClinicos",
                column: "IdCita",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiciosStaff_ServicioIdServicio",
                table: "ServiciosStaff",
                column: "ServicioIdServicio");

            migrationBuilder.CreateIndex(
                name: "IX_ServiciosStaff_StaffMedicoIdStaffMedico",
                table: "ServiciosStaff",
                column: "StaffMedicoIdStaffMedico");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorDisponibilidades");

            migrationBuilder.DropTable(
                name: "HistorialesClinicos");

            migrationBuilder.DropTable(
                name: "ServiciosStaff");

            migrationBuilder.DropTable(
                name: "Citas");

            migrationBuilder.DropTable(
                name: "Servicios");

            migrationBuilder.DropTable(
                name: "StaffMedico");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
