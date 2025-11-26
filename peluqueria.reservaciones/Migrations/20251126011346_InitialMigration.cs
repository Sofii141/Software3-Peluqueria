using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace peluqueria.reservaciones.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BloqueoRangoDias",
                columns: table => new
                {
                    EstilistaId = table.Column<int>(type: "int", nullable: false),
                    FechaInicioBloqueo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFinBloqueo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloqueoRangoDias", x => new { x.EstilistaId, x.FechaInicioBloqueo });
                });

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstaActiva = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Identificacion = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NombreUsuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Identificacion);
                });

            migrationBuilder.CreateTable(
                name: "DescansoFijo",
                columns: table => new
                {
                    EstilistaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DescansoFijo", x => x.EstilistaId);
                });

            migrationBuilder.CreateTable(
                name: "Estilistas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Identificacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estilistas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Horarios",
                columns: table => new
                {
                    EstilistaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Horarios", x => x.EstilistaId);
                });

            migrationBuilder.CreateTable(
                name: "Servicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DuracionMinutos = table.Column<int>(type: "int", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    Disponible = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DescansoFijo_Dias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "time", nullable: false),
                    EsLaborable = table.Column<bool>(type: "bit", nullable: false),
                    DescansoFijoEstilistaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DescansoFijo_Dias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DescansoFijo_Dias_DescansoFijo_DescansoFijoEstilistaId",
                        column: x => x.DescansoFijoEstilistaId,
                        principalTable: "DescansoFijo",
                        principalColumn: "EstilistaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HorarioBase_Dias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "time", nullable: false),
                    EsLaborable = table.Column<bool>(type: "bit", nullable: false),
                    HorarioBaseEstilistaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorarioBase_Dias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorarioBase_Dias_Horarios_HorarioBaseEstilistaId",
                        column: x => x.HorarioBaseEstilistaId,
                        principalTable: "Horarios",
                        principalColumn: "EstilistaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeOnly>(type: "time", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClienteIdentificacion = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ServicioId = table.Column<int>(type: "int", nullable: false),
                    EstilistaId = table.Column<int>(type: "int", nullable: false),
                    TiempoAtencion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservaciones_Clientes_ClienteIdentificacion",
                        column: x => x.ClienteIdentificacion,
                        principalTable: "Clientes",
                        principalColumn: "Identificacion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservaciones_Estilistas_EstilistaId",
                        column: x => x.EstilistaId,
                        principalTable: "Estilistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservaciones_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DescansoFijo_Dias_DescansoFijoEstilistaId",
                table: "DescansoFijo_Dias",
                column: "DescansoFijoEstilistaId");

            migrationBuilder.CreateIndex(
                name: "IX_HorarioBase_Dias_HorarioBaseEstilistaId",
                table: "HorarioBase_Dias",
                column: "HorarioBaseEstilistaId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservaciones_ClienteIdentificacion",
                table: "Reservaciones",
                column: "ClienteIdentificacion");

            migrationBuilder.CreateIndex(
                name: "IX_Reservaciones_EstilistaId",
                table: "Reservaciones",
                column: "EstilistaId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservaciones_ServicioId",
                table: "Reservaciones",
                column: "ServicioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BloqueoRangoDias");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "DescansoFijo_Dias");

            migrationBuilder.DropTable(
                name: "HorarioBase_Dias");

            migrationBuilder.DropTable(
                name: "Reservaciones");

            migrationBuilder.DropTable(
                name: "DescansoFijo");

            migrationBuilder.DropTable(
                name: "Horarios");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Estilistas");

            migrationBuilder.DropTable(
                name: "Servicios");
        }
    }
}
