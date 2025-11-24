using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Peluqueria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
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
                name: "Estilistas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentityId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    Imagen = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estilistas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParametrosSistema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BufferMinutos = table.Column<int>(type: "int", nullable: false),
                    ToleranciaLlegadaMinutos = table.Column<int>(type: "int", nullable: false),
                    DuracionMinimaServicioMinutos = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParametrosSistema", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Servicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DuracionMinutos = table.Column<int>(type: "int", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Imagen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Disponible = table.Column<bool>(type: "bit", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Servicios_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BloqueosDescansoFijoDiario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstilistaId = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    HoraInicioDescanso = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFinDescanso = table.Column<TimeSpan>(type: "time", nullable: false),
                    Razon = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloqueosDescansoFijoDiario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BloqueosDescansoFijoDiario_Estilistas_EstilistaId",
                        column: x => x.EstilistaId,
                        principalTable: "Estilistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BloqueosRangoDiasLibres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstilistaId = table.Column<int>(type: "int", nullable: false),
                    FechaInicioBloqueo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFinBloqueo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Razon = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloqueosRangoDiasLibres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BloqueosRangoDiasLibres_Estilistas_EstilistaId",
                        column: x => x.EstilistaId,
                        principalTable: "Estilistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HorariosSemanalBase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstilistaId = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    HoraInicioJornada = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFinJornada = table.Column<TimeSpan>(type: "time", nullable: false),
                    EsLaborable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosSemanalBase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorariosSemanalBase_Estilistas_EstilistaId",
                        column: x => x.EstilistaId,
                        principalTable: "Estilistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EstilistaServicios",
                columns: table => new
                {
                    EstilistaId = table.Column<int>(type: "int", nullable: false),
                    ServicioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstilistaServicios", x => new { x.EstilistaId, x.ServicioId });
                    table.ForeignKey(
                        name: "FK_EstilistaServicios_Estilistas_EstilistaId",
                        column: x => x.EstilistaId,
                        principalTable: "Estilistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EstilistaServicios_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "a20b1a03-9f2d-45f8-8f8e-20bc8b67b7e3", null, "Estilista", "ESTILISTA" },
                    { "d17abceb-8c0b-454e-b296-883bc029d82b", null, "Admin", "ADMIN" },
                    { "e10b1a03-9f2d-45f8-8f8e-20bc8b67b7e3", null, "Cliente", "CLIENTE" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NombreCompleto", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "Telefono", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "a18be9c0-aa65-4af8-bd17-00bd9344e575", 0, "00000000-0000-0000-0000-000000000000", "admin@test.com", true, false, null, "Administrador Principal", "ADMIN@TEST.COM", "ADMIN", "AQAAAAIAAYagAAAAEBijLS+itQ0VaPgIjuFO3RbRX0RrP9A4P8PsTkyb+7yq/3CtTrpKaNS2X5Vd5PwLrA==", null, false, "00000000-0000-0000-0000-000000000000", "3001234567", false, "admin" },
                    { "b7e289d1-d21a-4c9f-8d7e-00bd9344e575", 0, "00000000-0000-0000-0000-000000000000", "laura.e@pelu.com", true, false, null, "Laura Valencia", "LAURA.E@PELU.COM", "LAURA.E", "AQAAAAIAAYagAAAAEN1ZM6AS7ujCjxfYMwuIP1aNfIq0QgD5t0szfgsBJNjWZ+djEq6R035umMj9GhU8WA==", null, false, "00000000-0000-0000-0000-000000000000", "3001234568", false, "laura.e" },
                    { "c7e289d1-d21a-4c9f-8d7e-00bd9344e575", 0, "00000000-0000-0000-0000-000000000000", "juan.c@mail.com", true, false, null, "Juan Cliente", "JUAN.C@MAIL.COM", "JUAN.C", "AQAAAAIAAYagAAAAEEQH4FDWPbE6VKuvgUdxdLwYkdBaqKucgj2WhCouQ7uGTijcA0KU4EekJQpvtv1s8A==", null, false, "00000000-0000-0000-0000-000000000000", "3109876543", false, "juan.c" }
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "EstaActiva", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "Cortes de Cabello" },
                    { 2, true, "Tratamientos Capilares" },
                    { 3, true, "Manicura y Pedicura" },
                    { 4, true, "Otra" }
                });

            migrationBuilder.InsertData(
                table: "Estilistas",
                columns: new[] { "Id", "EstaActivo", "IdentityId", "Imagen", "NombreCompleto", "Telefono" },
                values: new object[,]
                {
                    { 1, true, "a18be9c0-aa65-4af8-bd17-00bd9344e575", "", "Administrador Principal", "3001234567" },
                    { 2, true, "b7e289d1-d21a-4c9f-8d7e-00bd9344e575", "", "Laura Valencia", "3001234568" }
                });

            migrationBuilder.InsertData(
                table: "ParametrosSistema",
                columns: new[] { "Id", "BufferMinutos", "DuracionMinimaServicioMinutos", "ToleranciaLlegadaMinutos" },
                values: new object[] { 1, 5, 45, 10 });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "d17abceb-8c0b-454e-b296-883bc029d82b", "a18be9c0-aa65-4af8-bd17-00bd9344e575" },
                    { "a20b1a03-9f2d-45f8-8f8e-20bc8b67b7e3", "b7e289d1-d21a-4c9f-8d7e-00bd9344e575" },
                    { "e10b1a03-9f2d-45f8-8f8e-20bc8b67b7e3", "c7e289d1-d21a-4c9f-8d7e-00bd9344e575" }
                });

            migrationBuilder.InsertData(
                table: "BloqueosDescansoFijoDiario",
                columns: new[] { "Id", "DiaSemana", "EstilistaId", "HoraFinDescanso", "HoraInicioDescanso", "Razon" },
                values: new object[,]
                {
                    { 1, 1, 2, new TimeSpan(0, 14, 0, 0, 0), new TimeSpan(0, 13, 0, 0, 0), "Almuerzo" },
                    { 2, 3, 2, new TimeSpan(0, 14, 0, 0, 0), new TimeSpan(0, 13, 0, 0, 0), "Almuerzo" },
                    { 3, 5, 2, new TimeSpan(0, 14, 0, 0, 0), new TimeSpan(0, 13, 0, 0, 0), "Almuerzo" }
                });

            migrationBuilder.InsertData(
                table: "BloqueosRangoDiasLibres",
                columns: new[] { "Id", "EstilistaId", "FechaFinBloqueo", "FechaInicioBloqueo", "Razon" },
                values: new object[] { 1, 2, new DateTime(2026, 1, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Vacaciones" });

            migrationBuilder.InsertData(
                table: "HorariosSemanalBase",
                columns: new[] { "Id", "DiaSemana", "EsLaborable", "EstilistaId", "HoraFinJornada", "HoraInicioJornada" },
                values: new object[,]
                {
                    { 1, 1, true, 2, new TimeSpan(0, 18, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0) },
                    { 2, 3, true, 2, new TimeSpan(0, 18, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0) },
                    { 3, 5, true, 2, new TimeSpan(0, 18, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0) }
                });

            migrationBuilder.InsertData(
                table: "Servicios",
                columns: new[] { "Id", "CategoriaId", "Descripcion", "Disponible", "DuracionMinutos", "FechaCreacion", "Imagen", "Nombre", "Precio" },
                values: new object[,]
                {
                    { 1, 1, "Un corte clásico y elegante por encima de los hombros.", true, 60, new DateTime(2025, 10, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "https://localhost:7274/images/bobCut.png", "Corte Estilo Bob", 50000m },
                    { 2, 2, "Tratamiento a base de keratina para revitalizar tu cabello.", true, 90, new DateTime(2025, 10, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "https://localhost:7274/images/hidratacion.jpg", "Hidratación Profunda", 80000m },
                    { 3, 3, "Una experiencia relajante que dejará tus manos suaves y perfectas.", true, 75, new DateTime(2025, 10, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "https://localhost:7274/images/manicura.jpg", "Manicura SPA Completa", 80000m },
                    { 4, 1, "Corte clásico para caballero.", true, 45, new DateTime(2025, 10, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "https://localhost:7274/images/manCut.png", "Corte para hombre", 30000m }
                });

            migrationBuilder.InsertData(
                table: "EstilistaServicios",
                columns: new[] { "EstilistaId", "ServicioId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 1, 3 },
                    { 2, 1 },
                    { 2, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BloqueosDescansoFijoDiario_EstilistaId",
                table: "BloqueosDescansoFijoDiario",
                column: "EstilistaId");

            migrationBuilder.CreateIndex(
                name: "IX_BloqueosRangoDiasLibres_EstilistaId",
                table: "BloqueosRangoDiasLibres",
                column: "EstilistaId");

            migrationBuilder.CreateIndex(
                name: "IX_EstilistaServicios_ServicioId",
                table: "EstilistaServicios",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_HorariosSemanalBase_EstilistaId",
                table: "HorariosSemanalBase",
                column: "EstilistaId");

            migrationBuilder.CreateIndex(
                name: "IX_Servicios_CategoriaId",
                table: "Servicios",
                column: "CategoriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BloqueosDescansoFijoDiario");

            migrationBuilder.DropTable(
                name: "BloqueosRangoDiasLibres");

            migrationBuilder.DropTable(
                name: "EstilistaServicios");

            migrationBuilder.DropTable(
                name: "HorariosSemanalBase");

            migrationBuilder.DropTable(
                name: "ParametrosSistema");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Servicios");

            migrationBuilder.DropTable(
                name: "Estilistas");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}
